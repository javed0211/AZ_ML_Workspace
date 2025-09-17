# ML Workspace Management Tests

This directory contains comprehensive tests for Azure ML workspace management, including PIM role activation, compute instance management, VS Code Desktop integration, and data science library testing.

## Test Files

### `test_pim_compute_vscode_workflow.py`
**Complete end-to-end workflow test** that covers:

1. **PIM Role Activation**: Checks if required Azure roles are active and activates them if needed
2. **Workspace Navigation**: Navigates to the specific ML workspace (`CTAO AI Platform ML Workspace`)
3. **Compute Management**: Starts the specified compute instance (`ci-disc-dev3-jk-cpu-sm`)
4. **VS Code Desktop**: Launches VS Code Desktop and waits for remote connection
5. **Notebook Creation**: Creates a Jupyter notebook with data science library tests
6. **Library Testing**: Tests availability and functionality of 20+ data science libraries

### `test_data_science_libraries.py`
**Focused library testing** that can be run independently to verify:
- Essential libraries (pandas, numpy, scikit-learn, matplotlib, seaborn)
- Extended libraries (tensorflow, torch, azure-ai-ml, etc.)
- Library functionality tests
- Azure integration libraries
- Machine learning specific libraries
- Data processing libraries
- Visualization libraries
- Jupyter environment

### `test_compute_lifecycle.py`
**Compute instance lifecycle management** including:
- Creation, starting, stopping, and deletion of compute instances
- Status monitoring and error handling
- Parallel operations testing

## Configuration

### Authentication
**All tests use Managed Identity by default.** This is the recommended approach for running tests in Azure environments (Azure VMs, Azure Container Instances, Azure Functions, etc.) where Managed Identity is available.

**Important**: To ensure Managed Identity is used:
- **Do NOT set `AZURE_CLIENT_SECRET`** in your `.env` file (client secret takes priority over Managed Identity)
- Ensure `USE_MANAGED_IDENTITY=true` (this is the default)
- Ensure `USE_INTERACTIVE_AUTH=false` (this is the default)

If you need to use a different authentication method:
- Set `USE_MANAGED_IDENTITY=false` to disable Managed Identity
- Set `USE_INTERACTIVE_AUTH=true` for interactive browser authentication
- Provide `AZURE_CLIENT_SECRET` for service principal authentication (overrides Managed Identity)

### Environment Variables
Set these in your `.env` file:

```bash
# Azure Configuration
AZURE_TENANT_ID=afd0e3db-52d8-42c3-9648-3e3a1c3c1d5f
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_ML_WORKSPACE_NAME=CTAO AI Platform ML Workspace

# Authentication (defaults to Managed Identity)
USE_INTERACTIVE_AUTH=false
USE_MANAGED_IDENTITY=true

# PIM Configuration
PIM_ROLE_NAME=Contributor
PIM_SCOPE=/subscriptions/your-subscription-id
PIM_JUSTIFICATION=Automated testing for ML workspace operations

# Compute Configuration
COMPUTE_INSTANCE_NAME=ci-disc-dev3-jk-cpu-sm
COMPUTE_SIZE=Standard_DS3_v2
```

## Running the Tests

### Complete Workflow Test
```bash
# Run the complete PIM -> Compute -> VS Code -> Data Science workflow
pytest tests/ml_workspace/workspace_management/test_pim_compute_vscode_workflow.py::TestPIMComputeVSCodeWorkflow::test_complete_pim_compute_vscode_workflow -v -s

# Run with specific markers
pytest -m "pim and compute" tests/ml_workspace/workspace_management/test_pim_compute_vscode_workflow.py -v -s
```

### Library Testing Only
```bash
# Test essential libraries
pytest tests/ml_workspace/workspace_management/test_data_science_libraries.py::TestDataScienceLibraries::test_essential_libraries_availability -v

# Test all library categories
pytest tests/ml_workspace/workspace_management/test_data_science_libraries.py -v

# Test specific library category
pytest tests/ml_workspace/workspace_management/test_data_science_libraries.py::TestDataScienceLibraries::test_machine_learning_libraries -v
```

### Compute Lifecycle Tests
```bash
# Test compute instance management
pytest tests/ml_workspace/workspace_management/test_compute_lifecycle.py -v -s
```

## Test Markers

The tests use the following pytest markers:

- `@pytest.mark.pim` - Tests involving Privileged Identity Management
- `@pytest.mark.compute` - Tests involving compute instances
- `@pytest.mark.azure` - Tests requiring Azure resources
- `@pytest.mark.slow` - Long-running tests (may take 15+ minutes)
- `@pytest.mark.cleanup` - Cleanup tests that should run last

### Running by Markers
```bash
# Run only PIM-related tests
pytest -m pim -v -s

# Run only compute tests
pytest -m compute -v -s

# Run Azure tests but skip slow ones
pytest -m "azure and not slow" -v

# Run slow tests (for CI/CD or comprehensive testing)
pytest -m slow -v -s
```

## Expected Test Duration

- **Complete workflow test**: 20-30 minutes (includes PIM activation, compute startup, VS Code launch)
- **Library testing only**: 2-5 minutes
- **Compute lifecycle tests**: 10-20 minutes (depending on compute startup time)

## Test Data Science Libraries

The tests verify availability of these libraries:

### Essential Libraries (must be available)
- pandas
- numpy
- scikit-learn
- matplotlib
- seaborn

### Extended Libraries
- plotly
- tensorflow
- torch
- transformers
- azure-ai-ml
- azure-storage-blob
- azure-keyvault-secrets
- requests
- beautifulsoup4
- openpyxl
- sqlalchemy
- pymongo
- redis
- jupyter
- ipywidgets

## Troubleshooting

### Common Issues

1. **PIM Activation Fails**
   - Verify you have the correct role assignment
   - Check that PIM is enabled for your subscription
   - Ensure the role name and scope are correct

2. **Compute Instance Not Found**
   - Verify the compute instance name is correct
   - Check that you have access to the workspace
   - Ensure the compute instance exists and is not deleted

3. **VS Code Desktop Launch Fails**
   - Check that VS Code Desktop is available for the compute instance
   - Verify the compute instance is running
   - Try launching VS Code manually to test connectivity

4. **Library Tests Fail**
   - Some libraries may not be pre-installed
   - Check the compute instance environment
   - Consider using a different compute instance image

### Debug Mode
Run tests with additional logging:
```bash
pytest tests/ml_workspace/workspace_management/ -v -s --log-cli-level=DEBUG
```

## Contributing

When adding new tests:
1. Follow the existing test structure
2. Add appropriate pytest markers
3. Include proper error handling and logging
4. Update this README with new test descriptions
5. Consider test execution time and mark slow tests appropriately