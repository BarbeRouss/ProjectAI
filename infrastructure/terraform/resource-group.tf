data "azurerm_resource_group" "main" {
  name = "rg-${var.project}"
}

resource "azurerm_management_lock" "rg_no_delete" {
  name       = "no-delete"
  scope      = data.azurerm_resource_group.main.id
  lock_level = "CanNotDelete"
  notes      = "Prevent accidental deletion of the resource group"
}
