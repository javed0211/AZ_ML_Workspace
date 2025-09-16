# Azure ML Workspace Automation Framework (Python)

A comprehensive Python-based automation framework for testing and managing Azure Machine Learning workspaces using Playwright for web automation.

## 🚀 Features

- **Web Automation**: Playwright-based browser automation for Azure ML Studio
- **Azure Integration**: Native Azure SDK integration for workspace management
- **Authentication**: Multiple authentication methods (DefaultAzureCredential, Interactive, Managed Identity)
- **Compute Management**: Start/stop compute instances programmatically
- **Test Framework**: Comprehensive test suite with parallel execution
- **CLI Interface**: Command-line interface for all operations
- **Reporting**: HTML and Allure test reports
- **Configuration**: Flexible configuration via environment variables
- **Logging**: Structured logging with multiple output formats

## 📋 Prerequisites

- Python 3.9+
- Azure subscription with ML workspace
- Appropriate Azure permissions

## 🛠️ Installation

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd AZ_ML_Workspace
   ```

2. **Install the package:**
   ```bash
   pip install -e .
   ```

3. **Install Playwright browsers:**
   ```bash
   playwright install
   ```

4. **Set up configuration:**
   ```bash
   cp .env.example .env
   # Edit .env with your Azure configuration
   ```

## ⚙️ Configuration

Create a `.env` file with your Azure configuration:

```env
# Required
AZURE_TENANT_ID=your-tenant-id
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_WORKSPACE_NAME=your-workspace-name

# Optional
USE_INTERACTIVE_AUTH=false
LOG_LEVEL=info
MAX_WORKERS=4
```

## 🎯 Usage

### CLI Commands

```bash
# Set up the framework
python -m azure_ml_automation.cli setup

# Validate configuration
python -m azure_ml_automation.cli validate

# Run tests
python -m azure_ml_automation.cli test

# Run specific test pattern
python -m azure_ml_automation.cli test -p "smoke"

# Run tests with specific browser
python -m azure_ml_automation.cli test -b firefox --headed

# Manage compute instances
python -m azure_ml_automation.cli start-compute
python -m azure_ml_automation.cli stop-compute
python -m azure_ml_automation.cli compute-status

# Generate reports
python -m azure_ml_automation.cli report

# Clean up artifacts
python -m azure_ml_automation.cli clean
```

### Python API

```python
import asyncio
from azure_ml_automation import AzureMLHelper, create_azure_ml_helper

async def main():
    # Create helper instance
    helper = await create_azure_ml_helper()
    
    # Get workspace info
    workspace = await helper.get_workspace()
    print(f"Workspace: {workspace.name}")
    
    # Start compute instance
    await helper.start_compute_instance("my-compute")
    
    # Run browser automation
    from azure_ml_automation.helpers.browser_manager import BrowserManager
    
    async with BrowserManager() as browser:
        page = await browser.new_page()
        await page.goto("https://ml.azure.com")
        # Your automation code here

if __name__ == "__main__":
    asyncio.run(main())
```

## 🧪 Testing

The framework includes comprehensive test suites:

### Test Categories

- **Smoke Tests**: Basic functionality validation
- **Authentication Tests**: Login and credential validation
- **Workspace Tests**: Workspace navigation and operations
- **Compute Tests**: Compute instance management
- **Notebook Tests**: Jupyter notebook operations
- **Data Tests**: Dataset and datastore operations
- **Model Tests**: Model registration and deployment
- **Pipeline Tests**: ML pipeline operations

### Running Tests

```bash
# Run all tests
python -m azure_ml_automation.cli test

# Run smoke tests only
python -m azure_ml_automation.cli test -p "smoke"

# Run with specific browser
python -m azure_ml_automation.cli test -b firefox

# Run in headed mode (visible browser)
python -m azure_ml_automation.cli test --headed

# Run with parallel workers
python -m azure_ml_automation.cli test -w 2
```

## 📊 Reporting

The framework generates multiple report formats:

- **HTML Reports**: `reports/html/index.html`
- **Allure Reports**: `reports/allure/`
- **JUnit XML**: `reports/junit.xml`

Generate reports:
```bash
python -m azure_ml_automation.cli report
```

## 🏗️ Architecture

```
src/azure_ml_automation/
├── __init__.py              # Main package exports
├── cli.py                   # Command-line interface
├── helpers/
│   ├── auth.py             # Authentication management
│   ├── azure_helpers.py    # Azure SDK operations
│   ├── browser_manager.py  # Playwright browser management
│   ├── config.py           # Configuration management
│   ├── logger.py           # Structured logging
│   └── utils.py            # Utility functions
├── pages/                  # Page Object Model
│   ├── base_page.py        # Base page class
│   ├── login_page.py       # Login page
│   ├── workspace_page.py   # Workspace page
│   ├── compute_page.py     # Compute page
│   └── notebook_page.py    # Notebook page
└── tests/                  # Test suites
    ├── smoke/              # Smoke tests
    ├── auth/               # Authentication tests
    ├── workspace/          # Workspace tests
    ├── compute/            # Compute tests
    ├── notebooks/          # Notebook tests
    ├── data/               # Data tests
    ├── models/             # Model tests
    └── pipelines/          # Pipeline tests
```

## 🔧 Development

### Adding New Tests

1. Create test file in appropriate directory
2. Inherit from `BaseTest` class
3. Use page objects for UI interactions
4. Follow naming convention: `test_*.py`

Example:
```python
import pytest
from azure_ml_automation.tests.base_test import BaseTest

class TestMyFeature(BaseTest):
    async def test_my_feature(self):
        # Your test code here
        pass
```

### Adding New Pages

1. Create page class in `pages/` directory
2. Inherit from `BasePage`
3. Define locators and methods

Example:
```python
from azure_ml_automation.pages.base_page import BasePage

class MyPage(BasePage):
    def __init__(self, page):
        super().__init__(page)
        self.my_button = page.locator("[data-testid='my-button']")
    
    async def click_my_button(self):
        await self.my_button.click()
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Run the test suite
6. Submit a pull request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For issues and questions:
1. Check the documentation
2. Search existing issues
3. Create a new issue with detailed information

## 🔄 Migration from TypeScript

This framework was migrated from TypeScript to Python while maintaining all functionality:

- ✅ All TypeScript files removed
- ✅ Python equivalents implemented
- ✅ Same CLI interface
- ✅ Same test structure
- ✅ Same configuration options
- ✅ Enhanced Azure SDK integration
- ✅ Improved error handling
- ✅ Better logging and reporting