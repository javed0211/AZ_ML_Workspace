# Azure ML Automation Framework Examples

This directory contains example scripts and usage patterns for the Azure ML Automation Framework.

## Available Examples

### 1. Basic Usage (`basic_usage.py`)
Demonstrates fundamental framework usage including:
- Creating Azure ML helper instances
- Getting workspace information
- Listing compute instances
- Basic browser automation
- Taking screenshots

### 2. Advanced Testing Patterns
Coming soon:
- Custom page objects
- Complex test scenarios
- Integration patterns
- Error handling examples

## Running Examples

### Prerequisites
- Python 3.9+
- Framework installed (`pip install -e .`)
- Azure credentials configured
- Playwright browsers installed (`playwright install`)

### Basic Usage Example

```bash
# Run the basic usage example
cd examples
python basic_usage.py
```

### Configuration

Create a `.env` file in the project root:
```env
AZURE_TENANT_ID=your-tenant-id
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_WORKSPACE_NAME=your-workspace-name
```

## Example Structure

```
examples/
├── README.md           # This file
├── basic_usage.py      # Basic framework usage
├── advanced/           # Advanced examples (coming soon)
│   ├── custom_pages.py
│   ├── complex_tests.py
│   └── integrations.py
└── data/              # Example data files
    ├── sample.csv
    └── test_notebook.ipynb
```

## Writing Your Own Examples

### 1. Basic Script Structure

```python
#!/usr/bin/env python3
import asyncio
import sys
from pathlib import Path

# Add src to path for development
sys.path.insert(0, str(Path(__file__).parent.parent / "src"))

from azure_ml_automation import create_azure_ml_helper
from azure_ml_automation.helpers.logger import get_logger

logger = get_logger(__name__)

async def main():
    """Your example function."""
    logger.info("Starting example...")
    
    try:
        # Your code here
        helper = await create_azure_ml_helper()
        # ... example logic ...
        
        logger.info("Example completed successfully!")
        return 0
    except Exception as e:
        logger.error(f"Example failed: {e}")
        return 1

if __name__ == "__main__":
    exit_code = asyncio.run(main())
    sys.exit(exit_code)
```

### 2. Browser Automation Example

```python
from azure_ml_automation.helpers.browser_manager import BrowserManager

async def browser_example():
    async with BrowserManager(headless=False) as browser_manager:
        browser = await browser_manager.get_browser()
        page = await browser.new_page()
        
        # Navigate and interact
        await page.goto("https://ml.azure.com")
        await page.screenshot(path="example.png")
```

### 3. Azure SDK Example

```python
async def azure_example():
    helper = await create_azure_ml_helper()
    
    # Get workspace info
    workspace = await helper.get_workspace()
    print(f"Workspace: {workspace.name}")
    
    # List compute instances
    compute_instances = await helper.list_compute_instances()
    for compute in compute_instances:
        print(f"Compute: {compute.name} - {compute.provisioning_state}")
```

## Best Practices

1. **Error Handling**: Always include proper error handling
2. **Logging**: Use the framework's logging system
3. **Resource Cleanup**: Ensure proper cleanup of resources
4. **Configuration**: Use environment variables for configuration
5. **Documentation**: Document your examples clearly

## Contributing Examples

1. Create your example script
2. Add documentation
3. Test thoroughly
4. Submit a pull request

## Support

For questions about examples:
1. Check the main README.md
2. Review existing examples
3. Create an issue for help