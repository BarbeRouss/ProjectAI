# ── Custom domains with managed SSL certificates ─────────
#
# Azure Container Apps requires a 3-step process:
#   1. Register hostname on the container app (no cert)
#   2. Create managed certificate (Azure validates CNAME)
#   3. Bind certificate to the hostname (via ARM API patch)
#
# AzureRM ~4.0 doesn't support managed certificates natively,
# so we use azapi for steps 2 and 3.

# ── Step 1: Register custom hostnames (no certificate yet) ──

resource "azurerm_container_app_custom_domain" "api" {
  name                     = var.api_domain_prod
  container_app_id         = azurerm_container_app.api_prod.id
  certificate_binding_type = "Disabled"

  lifecycle {
    ignore_changes = [certificate_binding_type, container_app_environment_certificate_id]
  }
}

resource "azurerm_container_app_custom_domain" "frontend" {
  name                     = var.frontend_domain_prod
  container_app_id         = azurerm_container_app.frontend_prod.id
  certificate_binding_type = "Disabled"

  lifecycle {
    ignore_changes = [certificate_binding_type, container_app_environment_certificate_id]
  }
}

# ── Step 2: Create managed certificates (free, auto-renewed) ──

resource "azapi_resource" "cert_api" {
  type      = "Microsoft.App/managedEnvironments/managedCertificates@2024-03-01"
  name      = "cert-api-prod"
  parent_id = local.main.container_app_environment_id
  location  = "westeurope"

  body = {
    properties = {
      subjectName             = var.api_domain_prod
      domainControlValidation = "CNAME"
    }
  }

  depends_on = [azurerm_container_app_custom_domain.api]
}

resource "azapi_resource" "cert_frontend" {
  type      = "Microsoft.App/managedEnvironments/managedCertificates@2024-03-01"
  name      = "cert-frontend-prod"
  parent_id = local.main.container_app_environment_id
  location  = "westeurope"

  body = {
    properties = {
      subjectName             = var.frontend_domain_prod
      domainControlValidation = "CNAME"
    }
  }

  depends_on = [azurerm_container_app_custom_domain.frontend]
}

# ── Step 3: Bind certificates to hostnames ───────────────

resource "azapi_update_resource" "api_cert_binding" {
  type        = "Microsoft.App/containerApps@2024-03-01"
  resource_id = azurerm_container_app.api_prod.id

  body = {
    properties = {
      configuration = {
        ingress = {
          customDomains = [{
            name          = var.api_domain_prod
            certificateId = azapi_resource.cert_api.id
            bindingType   = "SniEnabled"
          }]
        }
      }
    }
  }
}

resource "azapi_update_resource" "frontend_cert_binding" {
  type        = "Microsoft.App/containerApps@2024-03-01"
  resource_id = azurerm_container_app.frontend_prod.id

  body = {
    properties = {
      configuration = {
        ingress = {
          customDomains = [{
            name          = var.frontend_domain_prod
            certificateId = azapi_resource.cert_frontend.id
            bindingType   = "SniEnabled"
          }]
        }
      }
    }
  }
}
