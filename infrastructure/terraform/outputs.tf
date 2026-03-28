# ── Container App URLs ────────────────────────────────

output "api_prod_url" {
  description = "Production API URL"
  value       = "https://${azurerm_container_app.api_prod.ingress[0].fqdn}"
}

output "api_preprod_url" {
  description = "Preprod API URL"
  value       = "https://${azurerm_container_app.api_preprod.ingress[0].fqdn}"
}

output "frontend_prod_url" {
  description = "Production frontend URL"
  value       = "https://${azurerm_container_app.frontend_prod.ingress[0].fqdn}"
}

output "frontend_preprod_url" {
  description = "Preprod frontend URL"
  value       = "https://${azurerm_container_app.frontend_preprod.ingress[0].fqdn}"
}

# ── PostgreSQL ────────────────────────────────────────

output "postgresql_fqdn" {
  description = "PostgreSQL Flexible Server FQDN"
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

# ── Container Apps Environment ────────────────────────

output "container_app_environment_id" {
  description = "Container Apps Environment ID (used by ephemeral envs)"
  value       = azurerm_container_app_environment.main.id
}

# ── Bastion ──────────────────────────────────────────

output "bastion_fqdn" {
  description = "Bastion SSH host for DB tunnel (port 2222)"
  value       = azurerm_container_app.bastion.ingress[0].fqdn
}
