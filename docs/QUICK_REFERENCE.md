# Azure ML Automation Framework - Quick Reference

## Quick Start

```bash
# Setup the framework
python -m azure_ml_automation.cli setup

# Run smoke tests
python -m azure_ml_automation.cli test -p "smoke"

# Run all tests
python -m azure_ml_automation.cli test
```

## Common Commands

| Command | Description |
|---------|-------------|
| `python -m azure_ml_automation.cli test -p "smoke"` | Run smoke tests |
| `python -m azure_ml_automation.cli test -b firefox --headed` | Run tests with Firefox in headed mode |
| `python -m azure_ml_automation.cli validate` | Validate Azure configuration |
| `python -m azure_ml_automation.cli start-compute` | Start compute instance |
| `python -m azure_ml_automation.cli stop-compute` | Stop compute instance |
| `python -m azure_ml_automation.cli report` | Generate test reports |

## Test Structure

```
tests/
├── smoke/                          # Smoke tests
│   └── test_workspace_smoke.py     # Basic workspace tests
├── auth/                           # Authentication tests
├── workspace/                      # Workspace functionality tests
├── compute/                        # Compute instance tests
│   └── test_compute_lifecycle.py   # Compute lifecycle tests
├── notebooks/                      # Notebook tests
├── data/                          # Data and dataset tests
├── models/                        # Model tests
└── pipelines/                     # Pipeline tests
```

## Configuration

Create `.env` file:
```env
AZURE_TENANT_ID=your-tenant-id
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_WORKSPACE_NAME=your-workspace-name
```

## Adding New Tests

1. **Create test file**: `tests/your-category/test_your_feature.py`
2. **Inherit from BaseTest**:

```python
from azure_ml_automation.tests.base_test import BaseTest

class TestYourFeature(BaseTest):
    async def test_your_functionality(self):
        # Your test code here
        pass
```

## Page Objects

Create page objects in `src/azure_ml_automation/pages/`:

```python
from azure_ml_automation.pages.base_page import BasePage

class YourPage(BasePage):
    def __init__(self, page):
        super().__init__(page)
        self.your_element = page.locator("[data-testid='your-element']")
    
    async def perform_action(self):
        await self.your_element.click()
```

## Troubleshooting

### Common Issues

1. **Authentication failures**: Check your `.env` configuration
2. **Timeout errors**: Increase timeout values in configuration
3. **Browser issues**: Try different browsers with `-b` flag

### Reset Environment

```bash
python -m azure_ml_automation.cli clean
python -m azure_ml_automation.cli setup
```

### Debug Mode

```bash
python -m azure_ml_automation.cli test --headed -v
```

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `AZURE_TENANT_ID` | Azure tenant ID | Required |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID | Required |
| `AZURE_RESOURCE_GROUP` | Resource group name | Required |
| `AZURE_WORKSPACE_NAME` | Workspace name | Required |
| `LOG_LEVEL` | Logging level | `info` |
| `MAX_WORKERS` | Parallel workers | `4` |
| `DEFAULT_TIMEOUT` | Default timeout (ms) | `30000` |