# ── Managed certificates (free, auto-renewed by Azure) ──
# AzureRM ~4.0 doesn't support managed certificates natively,
# so we use azapi to create them via the ARM API.

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
}

# ── Custom domain bindings ───────────────────────────────

resource "azurerm_container_app_custom_domain" "api" {
  name                                     = var.api_domain_prod
  container_app_id                         = azurerm_container_app.api_prod.id
  container_app_environment_certificate_id = azapi_resource.cert_api.id
  certificate_binding_type                 = "SniEnabled"
}

resource "azurerm_container_app_custom_domain" "frontend" {
  name                                     = var.frontend_domain_prod
  container_app_id                         = azurerm_container_app.frontend_prod.id
  container_app_environment_certificate_id = azapi_resource.cert_frontend.id
  certificate_binding_type                 = "SniEnabled"
}
