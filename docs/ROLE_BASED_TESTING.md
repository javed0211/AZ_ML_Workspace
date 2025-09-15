# Role-Based Access Control Testing

This document describes the comprehensive role-based access control (RBAC) testing framework for Azure ML workspaces.

## Overview

The framework provides automated testing for different Azure roles and their permissions within Azure ML workspaces. It validates that users with different roles can only access the resources and perform the actions they're authorized for.

## Supported Roles

### 1. Owner
- **Full workspace access** including role management
- **Permissions**: All operations on all resources
- **Test Focus**: Verify complete access and role management capabilities

### 2. Contributor  
- **Full resource access** but no role management
- **Permissions**: Create, read, update, delete resources (except role assignments)
- **Test Focus**: Verify resource management without access control capabilities

### 3. Reader
- **Read-only access** to all resources
- **Permissions**: View resources, download data/models
- **Test Focus**: Verify read access and restriction of write operations

### 4. Data Scientist (AzureML Data Scientist)
- **ML experiment and model management** access
- **Permissions**: Create experiments, register models, use compute
- **Test Focus**: Verify ML workflow capabilities with limited infrastructure access

### 5. Compute Operator (AzureML Compute Operator)
- **Compute resource management** access
- **Permissions**: Manage compute instances and clusters
- **Test Focus**: Verify compute management with limited data/model access

### 6. PIM Integration
- **Privileged Identity Management** testing
- **Permissions**: Temporary elevated access through PIM activation
- **Test Focus**: Verify role elevation and time-limited access

## Test Cases

### VS Code Desktop Tests

#### Test Case 1: VS Code Desktop Launch and Azure ML Connection
**Objective**: Verify VS Code desktop can connect to Azure ML workspace

**Steps**:
1. Launch VS Code desktop application
2. Install Azure ML extension
3. Connect to Azure account
4. Select Azure ML workspace
5. Verify workspace connection status
6. Validate Azure ML commands are available
7. Check compute instances visibility

**Expected Results**:
- VS Code launches successfully
- Azure ML extension installs and activates
- Workspace connection established
- Azure ML features accessible based on user role

#### Test Case 2: Notebook Creation and Execution in VS Code
**Objective**: Verify notebook creation and execution capabilities

**Steps**:
1. Launch VS Code with required extensions
2. Create new Jupyter notebook
3. Add code cells with data analysis
4. Select Python kernel
5. Execute all notebook cells
6. Verify cell outputs
7. Save notebook

**Expected Results**:
- Notebook created successfully
- All cells execute without errors
- Expected outputs generated
- Notebook saved properly

#### Test Case 3: Remote Compute Connection and Code Execution
**Objective**: Verify remote development on Azure ML compute instances

**Steps**:
1. Launch VS Code with remote extensions
2. Connect to Azure account
3. Establish SSH connection to compute instance
4. Create Python script on remote compute
5. Execute script via integrated terminal
6. Test file synchronization
7. Test remote debugging

**Expected Results**:
- Remote connection established
- Code executes on remote compute
- File sync works properly
- Debugging functions correctly

### Website Tests

#### Test Case 1: Complete ML Experiment Lifecycle
**Objective**: Verify end-to-end ML experiment management

**Steps**:
1. Create and register dataset
2. Create AutoML experiment
3. Monitor experiment progress
4. Test experiment management features
5. Verify model management
6. Test endpoint management

**Expected Results**:
- Dataset registered successfully
- Experiment created and runs
- Progress monitoring works
- Model and endpoint management accessible based on role

#### Test Case 2: Collaborative Features and Workspace Sharing
**Objective**: Verify collaboration and sharing capabilities

**Steps**:
1. Test workspace information access
2. Verify access control permissions
3. Test shared resources visibility
4. Create and share notebook
5. Test workspace activity logs
6. Verify quota and usage information

**Expected Results**:
- Workspace information accessible
- Permissions match assigned role
- Shared resources visible appropriately
- Activity logging works
- Usage information available

#### Test Case 3: Security and Compliance Features
**Objective**: Verify security and compliance capabilities

**Steps**:
1. Test network security settings
2. Verify data encryption configuration
3. Check compliance status
4. Test audit logging
5. Verify data lineage and governance
6. Test security alerts and recommendations

**Expected Results**:
- Security settings accessible based on role
- Compliance status visible
- Audit logs available (if permitted)
- Security features function properly

### Role-Based Access Tests

#### Test Case 1: Owner Role Permissions
**Objective**: Verify Owner role has complete access

**Steps**:
1. Verify access control management
2. Test workspace settings modification
3. Test resource management capabilities
4. Verify data and model management
5. Test endpoint management
6. Validate complete permissions

**Expected Results**:
- Full access to all features
- Can manage role assignments
- Can modify workspace settings
- All resource operations permitted

#### Test Case 2: Contributor Role Permissions
**Objective**: Verify Contributor role restrictions

**Steps**:
1. Test resource management capabilities
2. Verify access control restrictions
3. Test workspace settings limitations
4. Validate data and model management
5. Test experiment management

**Expected Results**:
- Full resource management access
- Cannot manage role assignments
- Limited workspace settings access
- Full data/model/experiment access

#### Test Case 3: Reader Role Permissions
**Objective**: Verify Reader role read-only access

**Steps**:
1. Test read access to resources
2. Verify write operation restrictions
3. Test management access restrictions
4. Validate view-only capabilities

**Expected Results**:
- Can view all resources
- Cannot create/modify/delete resources
- No management access
- Download permissions where appropriate

#### Test Case 4: Data Scientist Role Permissions
**Objective**: Verify Data Scientist role ML-focused access

**Steps**:
1. Test experiment and model management
2. Verify data access permissions
3. Test compute access limitations
4. Validate notebook capabilities
5. Test endpoint deployment permissions

**Expected Results**:
- Full ML workflow access
- Limited compute infrastructure access
- Cannot create compute resources
- Can use existing compute

#### Test Case 5: Compute Operator Role Permissions
**Objective**: Verify Compute Operator role infrastructure focus

**Steps**:
1. Test compute management permissions
2. Verify resource monitoring access
3. Test data access restrictions
4. Validate experiment access limitations
5. Test model management restrictions

**Expected Results**:
- Full compute management access
- Limited data/model access
- Cannot create experiments
- Can monitor resource utilization

#### Test Case 6: PIM Integration and Role Escalation
**Objective**: Verify PIM role activation and elevated access

**Steps**:
1. Check current role and permissions
2. Request PIM role activation
3. Verify elevated permissions
4. Test elevated operations
5. Validate role-based UI elements

**Expected Results**:
- PIM activation successful
- Elevated permissions granted
- Additional operations accessible
- UI reflects elevated role

## Running Role-Based Tests

### Prerequisites

1. **Multiple Service Principals**: Create service principals for each role
2. **Role Assignments**: Assign appropriate Azure roles to each service principal
3. **Environment Configuration**: Set up role-specific credentials

### Configuration

#### Environment Variables

```bash
# Default credentials
AZURE_TENANT_ID=your-tenant-id
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_ML_WORKSPACE_NAME=your-workspace-name

# Role-specific credentials
OWNER_CLIENT_ID=owner-service-principal-id
OWNER_CLIENT_SECRET=owner-service-principal-secret

CONTRIBUTOR_CLIENT_ID=contributor-service-principal-id
CONTRIBUTOR_CLIENT_SECRET=contributor-service-principal-secret

READER_CLIENT_ID=reader-service-principal-id
READER_CLIENT_SECRET=reader-service-principal-secret

DATASCIENTIST_CLIENT_ID=data-scientist-service-principal-id
DATASCIENTIST_CLIENT_SECRET=data-scientist-service-principal-secret

COMPUTEOPERATOR_CLIENT_ID=compute-operator-service-principal-id
COMPUTEOPERATOR_CLIENT_SECRET=compute-operator-service-principal-secret

# PIM Configuration (optional)
PIM_ENABLED=true
PIM_ROLE_NAME=Contributor
PIM_SCOPE=/subscriptions/{subscription-id}/resourceGroups/{resource-group}
PIM_JUSTIFICATION=Automated testing
```

#### Credentials File (Alternative)

Create `credentials.json`:

```json
{
  "owner": {
    "AZURE_CLIENT_ID": "owner-client-id",
    "AZURE_CLIENT_SECRET": "owner-client-secret"
  },
  "contributor": {
    "AZURE_CLIENT_ID": "contributor-client-id", 
    "AZURE_CLIENT_SECRET": "contributor-client-secret"
  },
  "reader": {
    "AZURE_CLIENT_ID": "reader-client-id",
    "AZURE_CLIENT_SECRET": "reader-client-secret"
  }
}
```

### Running Tests

#### Individual Role Tests

```bash
# Test specific role
npm run test:roles:owner
npm run test:roles:contributor
npm run test:roles:reader
npm run test:roles:data-scientist
npm run test:roles:compute-operator
npm run test:roles:pim

# With custom options
node scripts/run-role-tests.js --role owner --headed --debug
```

#### All Roles

```bash
# Sequential execution
npm run test:roles:all

# Parallel execution (faster)
npm run test:roles:parallel

# With custom credentials file
node scripts/run-role-tests.js --all --credentials credentials.json
```

#### Interactive Mode

```bash
# Interactive role selection
npm run test:roles
```

### Test Reports

#### Individual Role Reports
- Located in `test-results/role-tests/{role}/`
- HTML report: `test-results/role-tests/{role}/index.html`

#### Combined Report
- Located in `test-results/role-tests/index.html`
- Shows summary of all role test results
- Links to individual role reports

### Advanced Options

#### Command Line Options

```bash
node scripts/run-role-tests.js [options]

Options:
  -r, --role <role>        Run tests for specific role
  -a, --all               Run tests for all roles
  -p, --parallel          Run tests in parallel
  --headed                Run tests in headed mode
  -d, --debug             Enable debug mode
  --no-report             Skip generating HTML report
  -c, --credentials <file> Use custom credentials file
  -o, --output <dir>      Output directory for test results
  -l, --list-roles        List available roles
  -h, --help              Show help message
```

#### Examples

```bash
# List available roles
npm run test:roles -- --list-roles

# Test specific role with debugging
npm run test:roles -- --role dataScientist --headed --debug

# Test all roles in parallel with custom output
npm run test:roles -- --all --parallel --output ./custom-results

# Use custom credentials file
npm run test:roles -- --all --credentials ./my-credentials.json
```

## Best Practices

### 1. Service Principal Setup

```bash
# Create service principal for each role
az ad sp create-for-rbac --name "azure-ml-automation-owner" --role Owner --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group}

az ad sp create-for-rbac --name "azure-ml-automation-contributor" --role Contributor --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group}

az ad sp create-for-rbac --name "azure-ml-automation-reader" --role Reader --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group}
```

### 2. Azure ML Specific Roles

```bash
# Assign Azure ML specific roles
az role assignment create --assignee {service-principal-id} --role "AzureML Data Scientist" --scope /subscriptions/{subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.MachineLearningServices/workspaces/{workspace-name}

az role assignment create --assignee {service-principal-id} --role "AzureML Compute Operator" --scope /subscriptions/{subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.MachineLearningServices/workspaces/{workspace-name}
```

### 3. PIM Configuration

```bash
# Enable PIM for eligible roles
az rest --method POST --url "https://graph.microsoft.com/v1.0/privilegedAccess/azureResources/roleAssignments" --body '{
  "roleDefinitionId": "{role-definition-id}",
  "resourceId": "{resource-id}",
  "subjectId": "{user-or-service-principal-id}",
  "assignmentState": "Eligible",
  "schedule": {
    "type": "Once",
    "startDateTime": "2024-01-01T00:00:00Z",
    "endDateTime": "2024-12-31T23:59:59Z"
  }
}'
```

### 4. Security Considerations

- **Rotate Credentials**: Regularly rotate service principal secrets
- **Least Privilege**: Assign minimum required permissions
- **Audit Logs**: Monitor role assignment changes
- **Secure Storage**: Store credentials securely (Key Vault, GitHub Secrets)

### 5. CI/CD Integration

```yaml
# GitHub Actions example
- name: Run Role-Based Tests
  run: npm run test:roles:all
  env:
    AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
    AZURE_SUBSCRIPTION_ID: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
    OWNER_CLIENT_ID: ${{ secrets.OWNER_CLIENT_ID }}
    OWNER_CLIENT_SECRET: ${{ secrets.OWNER_CLIENT_SECRET }}
    CONTRIBUTOR_CLIENT_ID: ${{ secrets.CONTRIBUTOR_CLIENT_ID }}
    CONTRIBUTOR_CLIENT_SECRET: ${{ secrets.CONTRIBUTOR_CLIENT_SECRET }}
    # ... other role credentials
```

## Troubleshooting

### Common Issues

1. **Authentication Failures**
   - Verify service principal credentials
   - Check role assignments
   - Ensure subscription access

2. **Permission Denied Errors**
   - Verify role has required permissions
   - Check resource scope
   - Validate workspace access

3. **PIM Activation Failures**
   - Verify PIM eligibility
   - Check activation requirements
   - Validate justification

4. **Test Timeouts**
   - Increase timeout values
   - Check network connectivity
   - Verify Azure service availability

### Debug Mode

```bash
# Enable debug logging
LOG_LEVEL=debug npm run test:roles:owner

# Run with Playwright debug
npm run test:roles -- --role owner --debug
```

### Validation Scripts

```bash
# Validate role configuration
node -e "
const { validateRoleConfiguration } = require('./src/helpers/role-config');
console.log(validateRoleConfiguration('owner'));
"

# Test Azure connectivity
az account show
az ml workspace show --name {workspace-name} --resource-group {resource-group}
```

## Contributing

When adding new role-based tests:

1. **Update Role Configuration**: Add new permissions to `src/helpers/role-config.ts`
2. **Create Test Cases**: Add test cases to appropriate spec files
3. **Update Documentation**: Document new roles and permissions
4. **Test Thoroughly**: Verify tests work with actual role assignments

## References

- [Azure RBAC Documentation](https://docs.microsoft.com/en-us/azure/role-based-access-control/)
- [Azure ML RBAC](https://docs.microsoft.com/en-us/azure/machine-learning/how-to-assign-roles)
- [Azure PIM Documentation](https://docs.microsoft.com/en-us/azure/active-directory/privileged-identity-management/)
- [Playwright Testing](https://playwright.dev/docs/intro)