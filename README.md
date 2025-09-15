# Azure ML Workspace Automation Framework

A comprehensive end-to-end testing framework for Azure Machine Learning workspaces using Playwright and TypeScript. This framework provides automated testing capabilities for Azure ML Studio, VS Code Web, JupyterLab, and VS Code Desktop (Electron) environments.

## Features

- **Multi-Platform Support**: Test Azure ML Studio, VS Code Web, JupyterLab, and VS Code Desktop
- **Comprehensive Test Coverage**: Workspace management, compute lifecycle, notebook execution, job management
- **Azure Integration**: Native Azure SDK integration with authentication and resource management
- **CLI Integration**: Azure CLI automation for compute and job management
- **Notebook Automation**: Execute and validate Jupyter notebooks programmatically
- **Electron Support**: Automate VS Code desktop application
- **PIM Support**: Privileged Identity Management integration for elevated permissions
- **Robust Reporting**: HTML reports, Allure integration, structured logging
- **CI/CD Ready**: GitHub Actions workflows and containerized execution
- **Artifact Management**: Screenshot, video, and trace collection with Azure Storage upload

## Quick Start

### Prerequisites

- Node.js 18+ 
- Azure CLI
- Azure subscription with ML workspace
- VS Code (for Electron tests)

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd azure-ml-workspace-automation
```

2. Install dependencies:
```bash
npm install
```

3. Install Playwright browsers:
```bash
npm run install:browsers
```

4. Configure environment:
```bash
cp .env.example .env
# Edit .env with your Azure configuration
```

5. Run setup:
```bash
npm run setup
```

### Basic Usage

Run all tests:
```bash
npm test
```

Run specific test suites:
```bash
npm run test:smoke      # Smoke tests
npm run test:compute    # Compute lifecycle tests
npm run test:notebook   # Notebook execution tests
npm run test:electron   # VS Code desktop tests
```

Run tests with UI mode:
```bash
npm run test:ui
```

## Configuration

### Environment Variables

Key configuration options in `.env`:

```bash
# Azure Configuration
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_ML_WORKSPACE_NAME=your-workspace-name

# Authentication
USE_INTERACTIVE_AUTH=false
USE_MANAGED_IDENTITY=false

# Test Configuration
BASE_URL=https://ml.azure.com
DEFAULT_TIMEOUT=30000
MAX_RETRIES=3
```

### Authentication Options

1. **Service Principal** (Recommended for CI/CD):
```bash
AZURE_CLIENT_SECRET=your-client-secret
```

2. **Interactive Browser** (Development):
```bash
USE_INTERACTIVE_AUTH=true
```

3. **Managed Identity** (Azure-hosted runners):
```bash
USE_MANAGED_IDENTITY=true
```

4. **Key Vault Integration**:
```bash
AZURE_KEY_VAULT_URL=https://your-keyvault.vault.azure.net/
```

## Test Structure

```
src/
├── tests/
│   ├── smoke/           # Basic functionality tests
│   ├── compute/         # Compute instance lifecycle
│   ├── notebooks/       # Notebook execution tests
│   ├── jobs/           # Job management tests
│   ├── electron/       # VS Code desktop tests
│   └── pim/            # Privileged access tests
├── pages/              # Page object models
│   ├── azure-ml-studio.ts
│   ├── jupyter-lab.ts
│   ├── vscode-web.ts
│   └── base-page.ts
├── helpers/            # Utility functions
│   ├── auth.ts
│   ├── azure-helpers.ts
│   ├── cli-runner.ts
│   ├── logger.ts
│   └── config.ts
├── notebooks/          # Notebook execution utilities
└── electron/           # VS Code Electron helpers
```

## Writing Tests

### Basic Test Example

```typescript
import { test } from '@playwright/test';
import { AzureMLStudioPage } from '../pages/azure-ml-studio';
import { logTestStart, logTestEnd } from '../helpers/logger';

test('should create compute instance', async ({ page }) => {
  const testLogger = logTestStart('Create Compute Instance Test');
  
  try {
    const azureMLPage = new AzureMLStudioPage(page, testLogger);
    await azureMLPage.waitForPageLoad();
    
    await azureMLPage.navigateToCompute();
    await azureMLPage.createComputeInstance('test-instance', 'Standard_DS3_v2');
    await azureMLPage.assertComputeInstanceExists('test-instance');
    
    logTestEnd(testLogger, true);
  } catch (error) {
    logTestEnd(testLogger, false);
    throw error;
  }
});
```

### Notebook Execution Example

```typescript
import { createNotebookRunner } from '../notebooks/notebook-runner';

test('should execute notebook', async ({ page }) => {
  const notebookRunner = createNotebookRunner(page, testLogger);
  
  const result = await notebookRunner.executeNotebookUI('sample-notebook.ipynb');
  
  expect(result.success).toBe(true);
  expect(result.failedCells).toBe(0);
});
```

### Azure CLI Integration Example

```typescript
import { createAzureMLHelper } from '../helpers/azure-helpers';

test('should manage compute via CLI', async () => {
  const azureHelper = createAzureMLHelper(testLogger);
  
  await azureHelper.startComputeInstance('test-instance');
  await azureHelper.waitForComputeInstanceState('test-instance', 'Running');
  
  const instance = await azureHelper.getComputeInstanceStatus('test-instance');
  expect(instance.state).toBe('Running');
});
```

## Advanced Features

### PIM Integration

For tests requiring elevated permissions:

```typescript
test('should activate PIM role', async () => {
  const azureHelper = createAzureMLHelper(testLogger);
  
  await azureHelper.requestPIMActivation(
    'Contributor',
    '/subscriptions/sub-id/resourceGroups/rg-name'
  );
  
  // Wait for activation and run privileged operations
});
```

### Electron Testing

For VS Code desktop automation:

```typescript
import { createVSCodeElectronHelper } from '../electron/vscode-electron';

test('should automate VS Code desktop', async () => {
  const vscode = createVSCodeElectronHelper({
    workspaceFolder: '/path/to/workspace'
  }, testLogger);
  
  await vscode.launch();
  await vscode.openFile('test.py');
  await vscode.runTerminalCommand('python test.py');
  await vscode.close();
});
```

### Custom Page Objects

Extend the base page for new platforms:

```typescript
import { BasePage } from './base-page';

export class CustomPlatformPage extends BasePage {
  async isPageLoaded(): Promise<boolean> {
    return await this.isElementVisible('.custom-platform-indicator');
  }
  
  getPageIdentifier(): string {
    return 'Custom Platform';
  }
  
  async customAction(): Promise<void> {
    await this.clickElement('.custom-button');
  }
}
```

## CI/CD Integration

### GitHub Actions

The framework includes GitHub Actions workflows:

```yaml
# .github/workflows/test.yml
name: Azure ML Tests
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
      - run: npm ci
      - run: npm run test:smoke
        env:
          AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
          AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
          AZURE_CLIENT_SECRET: ${{ secrets.AZURE_CLIENT_SECRET }}
```

### Docker Support

Run tests in containers:

```bash
docker build -t azure-ml-tests .
docker run -e AZURE_TENANT_ID=... azure-ml-tests npm test
```

## Reporting and Monitoring

### HTML Reports

View detailed test results:
```bash
npm run test:report
```

### Allure Reports

Generate comprehensive reports:
```bash
npm run test:allure
```

### Structured Logging

All test actions are logged with correlation IDs:
```json
{
  "timestamp": "2024-01-01T12:00:00.000Z",
  "level": "info",
  "message": "Test started",
  "testName": "Create Compute Instance Test",
  "correlationId": "test-1704110400000-abc123"
}
```

### Artifact Collection

Automatic collection of:
- Screenshots on failure
- Video recordings
- Network traces
- Console logs
- Performance metrics

## Troubleshooting

### Common Issues

1. **Authentication Failures**:
   - Verify Azure credentials
   - Check service principal permissions
   - Ensure subscription access

2. **Timeout Issues**:
   - Increase timeout values in config
   - Check network connectivity
   - Verify Azure service availability

3. **Compute Instance Issues**:
   - Verify quota availability
   - Check resource group permissions
   - Ensure VM size is available in region

4. **VS Code Electron Issues**:
   - Verify VS Code installation path
   - Check user data directory permissions
   - Disable auto-updates

### Debug Mode

Run tests with debug information:
```bash
npm run test:debug
```

Enable verbose logging:
```bash
LOG_LEVEL=debug npm test
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

### Development Setup

```bash
npm install
npm run dev  # Watch mode for TypeScript compilation
npm run lint # Code linting
npm run format # Code formatting
```

## License

MIT License - see LICENSE file for details.

## Support

For issues and questions:
- Create GitHub issues for bugs
- Check documentation for common solutions
- Review logs for detailed error information

## Roadmap

- [ ] Enhanced notebook execution validation
- [ ] Performance benchmarking
- [ ] Multi-region testing support
- [ ] Advanced PIM automation
- [ ] Custom extension testing
- [ ] API testing integration
- [ ] Mobile browser support