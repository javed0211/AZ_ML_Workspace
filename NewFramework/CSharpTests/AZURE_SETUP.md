# Azure Services Setup Guide

Complete guide to setting up Azure services for the C# Playwright test framework.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Azure ML Workspace](#azure-ml-workspace)
- [Azure AI Search](#azure-ai-search)
- [Azure Document Intelligence](#azure-document-intelligence)
- [Azure Speech Services](#azure-speech-services)
- [Authentication](#authentication)
- [Environment Configuration](#environment-configuration)
- [Troubleshooting](#troubleshooting)

## Prerequisites

### Required Tools

1. **Azure CLI**
   ```bash
   # Install Azure CLI
   # Windows: https://aka.ms/installazurecliwindows
   # macOS: brew install azure-cli
   # Linux: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
   
   # Verify installation
   az --version
   ```

2. **Azure Subscription**
   - Active Azure subscription
   - Appropriate permissions to create resources

3. **.NET 9.0 SDK**
   ```bash
   dotnet --version
   ```

### Azure Login

```bash
# Login to Azure
az login

# List subscriptions
az account list --output table

# Set active subscription
az account set --subscription "your-subscription-id"

# Verify current subscription
az account show
```

## Azure ML Workspace

### Create Resource Group

```bash
# Set variables
RESOURCE_GROUP="ml-test-rg"
LOCATION="eastus"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION
```

### Create Azure ML Workspace

#### Using Azure CLI

```bash
# Set variables
WORKSPACE_NAME="ml-test-workspace"
RESOURCE_GROUP="ml-test-rg"
LOCATION="eastus"

# Create workspace
az ml workspace create \
  --name $WORKSPACE_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION
```

#### Using Azure Portal

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource"
3. Search for "Machine Learning"
4. Click "Create"
5. Fill in details:
   - Subscription: Your subscription
   - Resource group: Create new or select existing
   - Workspace name: Unique name
   - Region: Select region
   - Storage account: Auto-created
   - Key vault: Auto-created
   - Application insights: Auto-created
   - Container registry: None (or create if needed)
6. Click "Review + Create"
7. Click "Create"

### Get Workspace Details

```bash
# Get workspace details
az ml workspace show \
  --name $WORKSPACE_NAME \
  --resource-group $RESOURCE_GROUP

# Get workspace ID
az ml workspace show \
  --name $WORKSPACE_NAME \
  --resource-group $RESOURCE_GROUP \
  --query id -o tsv
```

### Create Compute Instance

```bash
# Create compute instance
az ml compute create \
  --name "test-compute" \
  --type ComputeInstance \
  --size Standard_DS3_v2 \
  --workspace-name $WORKSPACE_NAME \
  --resource-group $RESOURCE_GROUP
```

### Environment Variables

```bash
# Add to .env file
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=ml-test-rg
AZURE_WORKSPACE_NAME=ml-test-workspace
AZURE_REGION=eastus
```

## Azure AI Search

### Create Search Service

#### Using Azure CLI

```bash
# Set variables
SEARCH_SERVICE_NAME="test-search-service"
RESOURCE_GROUP="ml-test-rg"
LOCATION="eastus"
SKU="basic"  # Options: free, basic, standard, standard2, standard3

# Create search service
az search service create \
  --name $SEARCH_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --sku $SKU
```

#### Using Azure Portal

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource"
3. Search for "Azure AI Search"
4. Click "Create"
5. Fill in details:
   - Subscription: Your subscription
   - Resource group: Select existing
   - Service name: Unique name
   - Location: Select region
   - Pricing tier: Select tier
6. Click "Review + Create"
7. Click "Create"

### Get Search Service Details

```bash
# Get search service details
az search service show \
  --name $SEARCH_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP

# Get admin key
az search admin-key show \
  --service-name $SEARCH_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP
```

### Create Search Index

```bash
# Create index (example)
az search index create \
  --service-name $SEARCH_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --name "test-index" \
  --fields '[
    {"name": "id", "type": "Edm.String", "key": true},
    {"name": "content", "type": "Edm.String", "searchable": true},
    {"name": "category", "type": "Edm.String", "filterable": true}
  ]'
```

### Environment Variables

```bash
# Add to .env file
AZURE_SEARCH_ENDPOINT=https://test-search-service.search.windows.net
AZURE_SEARCH_API_KEY=your-admin-key
AZURE_SEARCH_INDEX_NAME=test-index
```

## Azure Document Intelligence

### Create Document Intelligence Resource

#### Using Azure CLI

```bash
# Set variables
DOC_INTEL_NAME="test-doc-intelligence"
RESOURCE_GROUP="ml-test-rg"
LOCATION="eastus"
SKU="S0"  # Options: F0 (free), S0 (standard)

# Create resource
az cognitiveservices account create \
  --name $DOC_INTEL_NAME \
  --resource-group $RESOURCE_GROUP \
  --kind FormRecognizer \
  --sku $SKU \
  --location $LOCATION \
  --yes
```

#### Using Azure Portal

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource"
3. Search for "Document Intelligence"
4. Click "Create"
5. Fill in details:
   - Subscription: Your subscription
   - Resource group: Select existing
   - Region: Select region
   - Name: Unique name
   - Pricing tier: Select tier
6. Click "Review + Create"
7. Click "Create"

### Get Document Intelligence Details

```bash
# Get endpoint
az cognitiveservices account show \
  --name $DOC_INTEL_NAME \
  --resource-group $RESOURCE_GROUP \
  --query properties.endpoint -o tsv

# Get key
az cognitiveservices account keys list \
  --name $DOC_INTEL_NAME \
  --resource-group $RESOURCE_GROUP \
  --query key1 -o tsv
```

### Environment Variables

```bash
# Add to .env file
AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT=https://test-doc-intelligence.cognitiveservices.azure.com/
AZURE_DOCUMENT_INTELLIGENCE_KEY=your-key
```

## Azure Speech Services

### Create Speech Service

#### Using Azure CLI

```bash
# Set variables
SPEECH_SERVICE_NAME="test-speech-service"
RESOURCE_GROUP="ml-test-rg"
LOCATION="eastus"
SKU="S0"  # Options: F0 (free), S0 (standard)

# Create resource
az cognitiveservices account create \
  --name $SPEECH_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --kind SpeechServices \
  --sku $SKU \
  --location $LOCATION \
  --yes
```

#### Using Azure Portal

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Click "Create a resource"
3. Search for "Speech"
4. Click "Create"
5. Fill in details:
   - Subscription: Your subscription
   - Resource group: Select existing
   - Region: Select region
   - Name: Unique name
   - Pricing tier: Select tier
6. Click "Review + Create"
7. Click "Create"

### Get Speech Service Details

```bash
# Get key
az cognitiveservices account keys list \
  --name $SPEECH_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --query key1 -o tsv

# Get region
az cognitiveservices account show \
  --name $SPEECH_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --query location -o tsv
```

### Environment Variables

```bash
# Add to .env file
AZURE_SPEECH_KEY=your-speech-key
AZURE_SPEECH_REGION=eastus
```

## Authentication

### Azure CLI Authentication

The framework uses Azure CLI authentication by default.

```bash
# Login
az login

# Set subscription
az account set --subscription "your-subscription-id"

# Verify
az account show
```

### Service Principal Authentication

For CI/CD pipelines, use service principal authentication.

#### Create Service Principal

```bash
# Create service principal
az ad sp create-for-rbac \
  --name "test-automation-sp" \
  --role Contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group}

# Output:
# {
#   "appId": "your-app-id",
#   "displayName": "test-automation-sp",
#   "password": "your-password",
#   "tenant": "your-tenant-id"
# }
```

#### Set Environment Variables

```bash
# Add to .env file or CI/CD secrets
AZURE_CLIENT_ID=your-app-id
AZURE_CLIENT_SECRET=your-password
AZURE_TENANT_ID=your-tenant-id
AZURE_SUBSCRIPTION_ID=your-subscription-id
```

#### Login with Service Principal

```bash
az login --service-principal \
  --username $AZURE_CLIENT_ID \
  --password $AZURE_CLIENT_SECRET \
  --tenant $AZURE_TENANT_ID
```

### Managed Identity Authentication

For Azure-hosted environments (VMs, App Service, etc.).

```csharp
// Code automatically uses managed identity when available
var credential = new DefaultAzureCredential();
```

## Environment Configuration

### Complete .env File

```bash
# =============================================================================
# AZURE CONFIGURATION
# =============================================================================

# Azure ML Workspace
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=ml-test-rg
AZURE_WORKSPACE_NAME=ml-test-workspace
AZURE_REGION=eastus

# Azure AI Search
AZURE_SEARCH_ENDPOINT=https://test-search-service.search.windows.net
AZURE_SEARCH_API_KEY=your-search-admin-key
AZURE_SEARCH_INDEX_NAME=test-index

# Azure Document Intelligence
AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT=https://test-doc-intelligence.cognitiveservices.azure.com/
AZURE_DOCUMENT_INTELLIGENCE_KEY=your-doc-intel-key

# Azure Speech Services
AZURE_SPEECH_KEY=your-speech-key
AZURE_SPEECH_REGION=eastus

# Service Principal (for CI/CD)
# AZURE_CLIENT_ID=your-app-id
# AZURE_CLIENT_SECRET=your-password
# AZURE_TENANT_ID=your-tenant-id
```

### appsettings.json

```json
{
  "Azure": {
    "SubscriptionId": "",
    "ResourceGroup": "",
    "WorkspaceName": "",
    "Region": "eastus"
  },
  "AzureSearch": {
    "Endpoint": "",
    "ApiKey": "",
    "IndexName": ""
  },
  "DocumentIntelligence": {
    "Endpoint": "",
    "Key": ""
  },
  "SpeechServices": {
    "Key": "",
    "Region": "eastus"
  }
}
```

## Troubleshooting

### Authentication Issues

#### Issue: "No subscriptions found"

```bash
# Solution: Login again
az login

# Or use specific tenant
az login --tenant your-tenant-id
```

#### Issue: "Insufficient permissions"

```bash
# Solution: Check role assignments
az role assignment list --assignee your-user-or-sp-id

# Add required role
az role assignment create \
  --assignee your-user-or-sp-id \
  --role Contributor \
  --scope /subscriptions/{subscription-id}/resourceGroups/{resource-group}
```

### Resource Creation Issues

#### Issue: "Resource name already exists"

```bash
# Solution: Use unique names
WORKSPACE_NAME="ml-test-workspace-$(date +%s)"
```

#### Issue: "Quota exceeded"

```bash
# Solution: Check quotas
az vm list-usage --location eastus -o table

# Request quota increase in Azure Portal
```

### Network Issues

#### Issue: "Connection timeout"

```bash
# Solution: Check firewall rules
az ml workspace show \
  --name $WORKSPACE_NAME \
  --resource-group $RESOURCE_GROUP \
  --query publicNetworkAccess

# Enable public access if needed
az ml workspace update \
  --name $WORKSPACE_NAME \
  --resource-group $RESOURCE_GROUP \
  --public-network-access Enabled
```

### Service-Specific Issues

#### Azure AI Search

```bash
# Check service status
az search service show \
  --name $SEARCH_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP \
  --query status

# Check admin keys
az search admin-key show \
  --service-name $SEARCH_SERVICE_NAME \
  --resource-group $RESOURCE_GROUP
```

#### Document Intelligence

```bash
# Test endpoint
curl -X GET "https://your-doc-intel.cognitiveservices.azure.com/formrecognizer/info" \
  -H "Ocp-Apim-Subscription-Key: your-key"
```

#### Speech Services

```bash
# Test endpoint
curl -X GET "https://eastus.api.cognitive.microsoft.com/sts/v1.0/issuetoken" \
  -H "Ocp-Apim-Subscription-Key: your-key"
```

## Cleanup

### Delete Resources

```bash
# Delete specific resource
az ml workspace delete \
  --name $WORKSPACE_NAME \
  --resource-group $RESOURCE_GROUP \
  --yes

# Delete entire resource group (careful!)
az group delete \
  --name $RESOURCE_GROUP \
  --yes
```

### Cost Management

```bash
# Check costs
az consumption usage list \
  --start-date 2024-01-01 \
  --end-date 2024-01-31

# Set budget alerts in Azure Portal
```

## Best Practices

1. **Use Resource Groups**: Organize related resources
2. **Tag Resources**: Add tags for cost tracking
3. **Use Free Tiers**: For development/testing
4. **Clean Up**: Delete unused resources
5. **Monitor Costs**: Set up budget alerts
6. **Use Service Principal**: For CI/CD automation
7. **Secure Secrets**: Use Azure Key Vault
8. **Enable Logging**: For troubleshooting

## Additional Resources

- [Azure ML Documentation](https://docs.microsoft.com/en-us/azure/machine-learning/)
- [Azure AI Search Documentation](https://docs.microsoft.com/en-us/azure/search/)
- [Azure Document Intelligence Documentation](https://docs.microsoft.com/en-us/azure/ai-services/document-intelligence/)
- [Azure Speech Services Documentation](https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/)
- [Azure CLI Documentation](https://docs.microsoft.com/en-us/cli/azure/)