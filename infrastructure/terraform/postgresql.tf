resource "azurerm_postgresql_flexible_server" "main" {
  name                          = "psql-${var.project}"
  location                      = var.location
  resource_group_name           = data.azurerm_resource_group.main.name
  version                       = "16"
  administrator_login           = "houseflow"
  administrator_password        = var.pg_admin_password
  sku_name                      = var.pg_sku
  storage_mb                    = var.pg_storage_mb
  backup_retention_days         = 7
  geo_redundant_backup_enabled  = false
  public_network_access_enabled = true
  zone                          = "3"

  lifecycle {
    prevent_destroy = true
  }
}

# ── Databases ────────────────────────────────────────

resource "azurerm_postgresql_flexible_server_database" "prod" {
  name      = "${var.project}_prod"
  server_id = azurerm_postgresql_flexible_server.main.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

resource "azurerm_postgresql_flexible_server_database" "preprod" {
  name      = "${var.project}_preprod"
  server_id = azurerm_postgresql_flexible_server.main.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

# ── Firewall: allow Azure services ───────────────────
# This allows Container Apps (and other Azure services) to reach PostgreSQL.
# Azure services use IPs in the 0.0.0.0 range internally.
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure" {
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# ── Connection strings (local values) ────────────────

locals {
  pg_host = azurerm_postgresql_flexible_server.main.fqdn

  pg_connection_prod = join(";", [
    "Host=${local.pg_host}",
    "Port=5432",
    "Database=${azurerm_postgresql_flexible_server_database.prod.name}",
    "Username=houseflow",
    "Password=${var.pg_admin_password}",
    "SSL Mode=Require",
    "Trust Server Certificate=true",
  ])

  pg_connection_preprod = join(";", [
    "Host=${local.pg_host}",
    "Port=5432",
    "Database=${azurerm_postgresql_flexible_server_database.preprod.name}",
    "Username=houseflow",
    "Password=${var.pg_admin_password}",
    "SSL Mode=Require",
    "Trust Server Certificate=true",
  ])
}
