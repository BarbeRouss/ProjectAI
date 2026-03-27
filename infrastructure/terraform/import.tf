# One-time import: the Container Apps Environment existed before the VNet migration.
# This block can be removed after the first successful apply.
import {
  to = azurerm_container_app_environment.main
  id = "/subscriptions/${var.subscription_id}/resourceGroups/rg-${var.project}/providers/Microsoft.App/managedEnvironments/cae-${var.project}"
}
