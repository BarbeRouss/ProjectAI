data "azurerm_resource_group" "main" {
  name = "rg-${var.project}"
}

# Locks on critical resources only (prod + DB).
# Not on the RG — that would block PR preview cleanup.

resource "azurerm_management_lock" "db" {
  name       = "no-delete-db"
  scope      = azurerm_postgresql_flexible_server.main.id
  lock_level = "CanNotDelete"
  notes      = "Protect production database from accidental deletion"
}

resource "azurerm_management_lock" "api_prod" {
  name       = "no-delete-api-prod"
  scope      = azurerm_container_app.api_prod.id
  lock_level = "CanNotDelete"
  notes      = "Protect production API from accidental deletion"
}

resource "azurerm_management_lock" "frontend_prod" {
  name       = "no-delete-frontend-prod"
  scope      = azurerm_container_app.frontend_prod.id
  lock_level = "CanNotDelete"
  notes      = "Protect production frontend from accidental deletion"
}
