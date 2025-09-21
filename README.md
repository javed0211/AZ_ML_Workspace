# Azure ML Workspace Test Framework

A comprehensive C# Playwright-based test framework for Azure Machine Learning workspace automation and testing.

## Features

- **Playwright Integration**: Modern browser automation with C# and .NET
- **Page Object Model**: Maintainable and scalable test architecture
- **Comprehensive Logging**: Detailed test execution logging with Serilog
- **Screenshot & Video Capture**: Automatic capture on test failures
- **Trace Recording**: Detailed execution traces for debugging
- **Configuration Management**: Flexible configuration via JSON and environment variables
- **Azure ML Integration**: Specialized page objects for Azure ML workspace components

## Test Categories

### AI Document Search
- PDF text extraction testing
- Image OCR text extraction
- Document classification
- Key phrase extraction
- Document summarization

### ML Workspace Management
- Workspace access and navigation
- Notebook creation and management
- Compute resource management
- Dataset management
- Model management
- Experiment management

### Integration Tests
- End-to-end ML workflows
- Document processing workflows
- Model deployment workflows

### Security Tests
- Authentication and authorization
- Resource access control
- Session management

## Project Structure

```
AzureMLWorkspace.Tests/
├── Configuration/          # Test configuration classes
├── Helpers/               # Base test classes and utilities
├── PageObjects/           # Page Object Model classes
├── Tests/                 # Test classes organized by category
│   ├── AIDocumentSearch/
│   ├── MLWorkspace/
│   ├── Integration/
│   └── Security/
├── TestData/              # Test data files
└── appsettings.json       # Configuration files
```

## Getting Started

### Prerequisites

- .NET 9.0 or later
- Visual Studio 2022 or VS Code
- Azure subscription with ML workspace (for integration tests)

### Installation

1. Clone the repository
2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Install Playwright browsers:
   ```bash
   pwsh bin/Debug/net9.0/playwright.ps1 install
   ```

### Configuration

1. Update `appsettings.json` with your Azure ML workspace details:
   ```json
   {
     "BaseUrl": "https://ml.azure.com",
     "Azure": {
       "SubscriptionId": "your-subscription-id",
       "ResourceGroup": "your-resource-group",
       "WorkspaceName": "your-workspace-name",
       "TenantId": "your-tenant-id"
     }
   }
   ```

2. For local development, create `appsettings.test.json` to override settings:
   ```json
   {
     "HeadlessMode": false,
     "SlowMo": true,
     "CaptureVideos": true
   }
   ```

### Running Tests

Run all tests:
```bash
dotnet test
```

Run specific test categories:
```bash
dotnet test --filter Category=DocumentProcessing
dotnet test --filter Category=WorkspaceManagement
dotnet test --filter Category=Integration
dotnet test --filter Category=Security
```

Run tests with specific browser:
```bash
dotnet test -- Playwright.BrowserName=chromium
dotnet test -- Playwright.BrowserName=firefox
dotnet test -- Playwright.BrowserName=webkit
```

### Test Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `HeadlessMode` | Run browser in headless mode | `true` |
| `BrowserType` | Browser to use (chromium, firefox, webkit) | `chromium` |
| `SlowMo` | Add delay between actions | `false` |
| `SlowMoDelay` | Delay in milliseconds | `100` |
| `CaptureScreenshots` | Capture screenshots on failure | `true` |
| `CaptureVideos` | Record test execution videos | `false` |
| `CaptureTraces` | Record execution traces | `true` |
| `DefaultTimeout` | Default timeout for operations | `30000` |

### Test Data

The framework includes sample test data in the `TestData/` directory:
- `sample-notebook.ipynb`: Sample Jupyter notebook for testing
- `azure-ml-config.json`: Sample Azure ML configuration
- Additional test files can be added as needed

### Logging

Test execution logs are automatically generated in the `Logs/` directory with:
- Console output with timestamps
- File-based logging with daily rotation
- Test step logging
- Screenshot capture logging
- Error and exception logging

### Continuous Integration

The framework is designed to work with CI/CD pipelines. Key considerations:
- Use headless mode in CI environments
- Configure appropriate timeouts
- Store Azure credentials securely
- Archive test artifacts (screenshots, videos, traces)

## Contributing

1. Follow the existing code structure and naming conventions
2. Add appropriate test categories and documentation
3. Include logging for test steps
4. Update configuration as needed for new features

## License

This project is licensed under the MIT License - see the LICENSE file for details.