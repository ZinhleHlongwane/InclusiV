# Azure Foundation

The Bicep template creates a Log Analytics workspace, Azure Container Apps managed environment and Azure Container Registry. It does not create PostgreSQL, Key Vault, model deployments or application container apps yet.

Before deployment:
1. Sign in and select the intended Azure subscription.
2. Create a dedicated resource group.
3. Configure budget alerts.
4. Check regional availability and pricing.
5. Review the template and parameters.

Example:
```bash
az login
az account set --subscription "<subscription-id>"
az group create --name "<resource-group>" --location "<region>"
az deployment group create --resource-group "<resource-group>" --template-file foundation.bicep --parameters location="<region>" namePrefix="<unique-prefix>"
```
This creates billable resources. Do not deploy until expected costs are reviewed.
