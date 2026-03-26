# Guide de setup Azure pour HouseFlow

Checklist manuelle à réaliser avant de déployer l'infrastructure Terraform.

## Prérequis

- [ ] Souscription Azure active (le tier gratuit suffit pour Container Apps)
- [ ] Azure CLI installé (`brew install azure-cli` / `curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash`)
- [ ] Être connecté : `az login`

## 1. Azure AD — App Registration (Workload Identity Federation)

```bash
# Créer l'App Registration
az ad app create --display-name "houseflow-github-actions"

# ⚠️ Noter les valeurs suivantes :
# - Application (client) ID  → AZURE_CLIENT_ID
# - Directory (tenant) ID    → az account show --query tenantId -o tsv

# Créer le Service Principal associé
az ad sp create --id <AZURE_CLIENT_ID>
```

## 2. Federated Credentials (OIDC pour GitHub Actions)

```bash
# Credential pour les déploiements depuis main (prod/preprod)
az ad app federated-credential create --id <AZURE_CLIENT_ID> --parameters '{
  "name": "github-actions-main",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:BarbeRouss/HouseFlow:ref:refs/heads/main",
  "audiences": ["api://AzureADTokenExchange"]
}'

# Credential pour les environnements éphémères (PRs)
az ad app federated-credential create --id <AZURE_CLIENT_ID> --parameters '{
  "name": "github-actions-pr",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:BarbeRouss/HouseFlow:environment:azure-deploy",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

## 3. Resource Group + rôle RBAC

```bash
# Créer le Resource Group
az group create --name rg-houseflow --location westeurope

# Assigner Contributor au Service Principal
SUBSCRIPTION_ID=$(az account show --query id -o tsv)
az role assignment create \
  --assignee <AZURE_CLIENT_ID> \
  --role Contributor \
  --scope /subscriptions/$SUBSCRIPTION_ID/resourceGroups/rg-houseflow
```

## 4. Storage Account pour le Terraform State

```bash
az storage account create \
  --name sthouseflowtfstate \
  --resource-group rg-houseflow \
  --sku Standard_LRS \
  --location westeurope

az storage container create \
  --name tfstate \
  --account-name sthouseflowtfstate
```

## 5. PAT GitHub (pull GHCR depuis Azure)

1. Aller sur https://github.com/settings/tokens?type=beta (Fine-grained tokens)
2. Créer un token avec :
   - **Name** : `houseflow-azure-ghcr-pull`
   - **Expiration** : 1 an
   - **Repository access** : `BarbeRouss/HouseFlow` uniquement
   - **Permissions** : `Read access to packages` uniquement
3. Copier le token

## 6. Secrets GitHub Actions

Ajouter dans **Settings > Secrets and variables > Actions** du repo :

| Secret                   | Valeur                                       |
|--------------------------|----------------------------------------------|
| `AZURE_CLIENT_ID`        | Application (client) ID de l'App Registration |
| `AZURE_TENANT_ID`        | Directory (tenant) ID                         |
| `AZURE_SUBSCRIPTION_ID`  | `az account show --query id -o tsv`           |
| `GHCR_PAT`              | Le PAT fine-grained créé à l'étape 5          |

## 7. Vérification

```bash
# Vérifier que tout est en place
az ad app list --display-name "houseflow-github-actions" --query "[].{id:appId, name:displayName}" -o table
az group show --name rg-houseflow --query "{name:name, location:location}" -o table
az storage account show --name sthouseflowtfstate --query "{name:name, sku:sku.name}" -o table
```

Une fois toutes les cases cochées, le Terraform et les workflows GitHub Actions peuvent être déployés.
