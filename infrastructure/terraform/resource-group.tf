data "azurerm_resource_group" "main" {
  name = "rg-${var.project}"
}

resource "azurerm_management_lock" "rg_no_delete" {
  name       = "no-delete"
  scope      = data.azurerm_resource_group.main.id
  lock_level = "CanNotDelete"
  notes      = "Prevent accidental deletion of the resource group"

  # Lock must be created after all resources that could be deleted.
  # CanNotDelete blocks ANY resource deletion in the RG, so Terraform
  # must remove the lock first when destroying resources.
  depends_on = [
    azurerm_postgresql_flexible_server.main,
    azurerm_container_app_environment.main,
    module.pr_env,
    azurerm_postgresql_flexible_server_database.pr,
  ]
}
