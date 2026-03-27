# One-time import: CAE was recreated by a previous timed-out run but
# never recorded in the state. Remove this file after successful apply.
import {
  to = azurerm_container_app_environment.main
  id = "/subscriptions/${var.subscription_id}/resourceGroups/rg-${var.project}/providers/Microsoft.App/managedEnvironments/cae-${var.project}"
}
