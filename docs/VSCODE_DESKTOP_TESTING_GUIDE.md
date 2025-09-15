# VS Code Desktop Testing Guide for Azure ML Workspace Automation

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Quick Start](#quick-start)
4. [Test Configuration](#test-configuration)
5. [Running Tests](#running-tests)
6. [Understanding Test Results](#understanding-test-results)
7. [Mock vs Real Testing](#mock-vs-real-testing)
8. [Customizing Tests](#customizing-tests)
9. [Troubleshooting](#troubleshooting)
10. [Advanced Usage](#advanced-usage)

## Overview

This testing framework provides comprehensive automation for testing Azure ML integrations with VS Code Desktop. It supports both mock testing (for development) and real Azure integration testing (for production validation).

### What This Framework Tests
- VS Code Desktop application launch and configuration
- Azure ML extension installation and activation
- Azure account authentication and workspace connection
- Jupyter notebook creation and execution
- Remote compute instance management
- Python script execution with Azure ML SDK
- File operations and workspace management

## Prerequisites

### System Requirements
- **Operating System**: macOS, Linux, or Windows
- **Node.js**: Version 16 or higher
- **Python**: Version 3.8 or higher (for real Azure ML testing)
- **VS Code**: Installed on the system (for real testing)

### Required Dependencies
```bash
# Install Node.js dependencies
npm install

# Install Python dependencies (for real testing)
pip install -r test-data/requirements.txt
```

### Azure Requirements (for real testing)
- Azure subscription with Azure ML workspace
- Service Principal with appropriate permissions
- Azure CLI installed and configured

## Quick Start

### 1. Clone and Setup
```bash
git clone <repository-url>
cd AZ_ML_Workspace
npm install
```

### 2. Run Mock Tests (Recommended for Development)
```bash
# Run all VS Code Desktop tests in mock mode
npm run test:vscode:mock

# Or use the direct command
NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true npx playwright test --grep "@electron.*@integration"
```

### 3. View Test Results
```bash
# Open test results in browser
npx playwright show-report

# View test summary
cat test-results/reports/test-summary.json
```

## Test Configuration

### Environment Variables

#### Required for All Tests
```bash
NODE_ENV=test                    # Enables test mode
MOCK_VSCODE=true                # Uses mock VS Code helper
MOCK_AZURE_SERVICES=true        # Uses mock Azure services
```

#### Required for Real Azure Testing
```bash
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_ML_WORKSPACE_NAME=your-workspace-name
```

#### Optional Configuration
```bash
SKIP_AZURE_AUTH=true            # Skip Azure authentication
TEST_TIMEOUT=60000              # Test timeout in milliseconds
TEST_WORKERS=1                  # Number of parallel workers
```

### Configuration Files

#### VS Code Settings (`test-data/vscode-settings.json`)
```json
{
  "python.defaultInterpreterPath": "/opt/miniconda/envs/azureml_py38/bin/python",
  "python.terminal.activateEnvironment": true,
  "azureML.workspaceConfigPath": "./test-data/azure-ml-config.json",
  "azureML.enableRemoteCompute": true,
  "jupyter.askForKernelRestart": false
}
```

#### Azure ML Configuration (`test-data/azure-ml-config.json`)
```json
{
  "subscription_id": "your-subscription-id",
  "resource_group": "your-resource-group",
  "workspace_name": "your-workspace-name",
  "compute_targets": {
    "cpu-cluster": {
      "type": "AmlCompute",
      "size": "Standard_DS3_v2"
    }
  }
}
```

## Running Tests

### Basic Test Execution

#### 1. Mock Testing (Development)
```bash
# Run all VS Code Desktop tests with mocks
npm run test:vscode:mock

# Run specific test with verbose output
NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \
npx playwright test --grep "should launch VS Code and connect to Azure ML workspace" \
--reporter=line --workers=1

# Run tests with debug mode
NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \
npx playwright test --grep "@electron" --debug
```

#### 2. Real Azure Testing (Production)
```bash
# Set up environment variables first
export AZURE_TENANT_ID=your-tenant-id
export AZURE_CLIENT_ID=your-client-id
export AZURE_CLIENT_SECRET=your-client-secret
export AZURE_SUBSCRIPTION_ID=your-subscription-id
export AZURE_RESOURCE_GROUP=your-resource-group
export AZURE_ML_WORKSPACE_NAME=your-workspace-name

# Run tests against real Azure services
NODE_ENV=production npx playwright test --grep "@electron.*@integration"
```

### Test Categories

#### Integration Tests
```bash
# All integration tests
npx playwright test --grep "@integration"

# VS Code Desktop integration tests only
npx playwright test --grep "@electron.*@integration"

# Notebook-specific tests
npx playwright test --grep "@electron.*@notebook"

# Remote compute tests
npx playwright test --grep "@electron.*@remote"
```

#### Specific Test Scenarios
```bash
# Test VS Code launch and Azure ML connection
npx playwright test --grep "should launch VS Code and connect to Azure ML workspace"

# Test notebook creation and execution
npx playwright test --grep "should create and execute Jupyter notebook"

# Test remote compute connection
npx playwright test --grep "should connect to remote compute instance"
```

### Advanced Test Options

#### Browser Selection
```bash
# Run on specific browser
npx playwright test --project=chromium --grep "@electron"

# Run on all browsers
npx playwright test --grep "@electron"

# Run on mobile browsers
npx playwright test --project="Mobile Chrome" --grep "@electron"
```

#### Parallel Execution
```bash
# Run with multiple workers (be careful with real Azure resources)
npx playwright test --workers=2 --grep "@electron"

# Force sequential execution
npx playwright test --workers=1 --grep "@electron"
```

#### Test Filtering
```bash
# Run tests with specific timeout
npx playwright test --timeout=120000 --grep "@electron"

# Run only failed tests
npx playwright test --last-failed --grep "@electron"

# Run tests matching pattern
npx playwright test --grep "Azure ML.*workspace" 
```

## Understanding Test Results

### Test Output Structure
```
Running 7 tests using 1 worker

✓ [chromium] VS Code Desktop Launch and Azure ML Connection
✓ [firefox] VS Code Desktop Launch and Azure ML Connection  
✓ [webkit] VS Code Desktop Launch and Azure ML Connection
...

Test Results:
- 7 passed
- 0 failed
- 0 skipped
```

### Log Analysis

#### Successful Test Logs
```json
{
  "timestamp": "2025-09-14T13:12:21.137Z",
  "level": "info",
  "message": "Test started",
  "correlationId": "test-1757855541137-ctfji9w4j",
  "testName": "VS Code Desktop Launch and Azure ML Connection"
}
```

#### Error Identification
```json
{
  "timestamp": "2025-09-14T13:12:27.960Z",
  "level": "error", 
  "message": "VS Code Azure ML connection test failed",
  "error": "vscodeHelper.waitForExtensionActivation is not a function"
}
```

### Test Reports

#### HTML Report
```bash
# Generate and open HTML report
npx playwright show-report
```

#### JSON Reports
```bash
# View test summary
cat test-results/reports/test-summary.json

# View performance metrics
cat test-results/reports/performance-report.json

# View resource usage
cat test-results/reports/resource-usage-report.json
```

### Screenshots and Videos
- **Screenshots**: Automatically captured on test failures
- **Videos**: Available for debugging (enable with `--video=on`)
- **Traces**: Detailed execution traces (enable with `--trace=on`)

## Mock vs Real Testing

### Mock Testing (Recommended for Development)

#### Advantages
- ✅ Fast execution (no real Azure API calls)
- ✅ No Azure costs or resource consumption
- ✅ Consistent, predictable results
- ✅ Safe for CI/CD pipelines
- ✅ No authentication requirements

#### Use Cases
- Development and debugging
- Unit testing
- CI/CD pipeline validation
- Feature development
- Regression testing

#### Mock Capabilities
```typescript
// Mock Azure ML operations
await azureMLHelper.createComputeInstance('test-instance');
await azureMLHelper.submitJob(jobConfig);
await azureMLHelper.getJobStatus('job-123');

// Mock VS Code operations  
await vscodeHelper.launch();
await vscodeHelper.installExtension('ms-toolsai.vscode-ai');
await vscodeHelper.connectToAzure(azureConfig);
```

### Real Testing (Production Validation)

#### Advantages
- ✅ Tests actual Azure ML functionality
- ✅ Validates real VS Code behavior
- ✅ End-to-end integration verification
- ✅ Real performance metrics

#### Requirements
- Valid Azure subscription and credentials
- VS Code installed on test machine
- Network connectivity to Azure
- Appropriate Azure permissions

#### Use Cases
- Production deployment validation
- End-to-end testing
- Performance benchmarking
- Integration verification

## Customizing Tests

### Adding New Test Cases

#### 1. Create Test File
```typescript
// src/tests/electron/custom-test.spec.ts
import { test, expect } from '@playwright/test';
import { createVSCodeHelper } from '../../electron/vscode-electron-helper';
import { createAzureMLHelper } from '../../helpers/azure-helpers';

test.describe('Custom VS Code Tests', () => {
  test('should perform custom Azure ML operation @electron @custom', async () => {
    const vscodeHelper = createVSCodeHelper();
    const azureMLHelper = createAzureMLHelper();
    
    // Your test logic here
    await vscodeHelper.launch();
    // ... test steps
  });
});
```

#### 2. Add Test Data
```bash
# Create custom test data
echo '{"custom": "data"}' > test-data/custom-config.json
```

#### 3. Run Custom Tests
```bash
npx playwright test --grep "@custom"
```

### Modifying Mock Behavior

#### Customize Mock Responses
```typescript
// src/electron/vscode-electron-mock.ts
async getCustomWorkspaceInfo(): Promise<any> {
  return {
    name: 'custom-workspace',
    status: 'active',
    customProperty: 'custom-value'
  };
}
```

#### Add New Mock Methods
```typescript
// Add to MockVSCodeElectronHelper class
async performCustomOperation(params: any): Promise<void> {
  logger.info('Mock: Performing custom operation', { params });
  await this.delay(1000);
  logger.info('Mock: Custom operation completed');
}
```

### Configuration Customization

#### Custom VS Code Settings
```json
{
  "python.defaultInterpreterPath": "/your/custom/python/path",
  "azureML.customSetting": "your-value",
  "jupyter.customNotebookSetting": true
}
```

#### Custom Azure ML Configuration
```json
{
  "subscription_id": "your-subscription",
  "custom_compute_targets": {
    "gpu-cluster": {
      "type": "AmlCompute", 
      "size": "Standard_NC6s_v3"
    }
  }
}
```

## Troubleshooting

### Common Issues

#### 1. Test Timeouts
```bash
# Increase timeout
npx playwright test --timeout=120000 --grep "@electron"

# Or set in environment
export TEST_TIMEOUT=120000
```

#### 2. Missing Mock Methods
```
Error: vscodeHelper.someMethod is not a function
```

**Solution**: Add the missing method to the mock helper:
```typescript
async someMethod(): Promise<void> {
  logger.info('Mock: Some method called');
  await this.delay(500);
}
```

#### 3. Authentication Failures (Real Testing)
```
Error: Azure authentication failed
```

**Solution**: Verify environment variables:
```bash
echo $AZURE_TENANT_ID
echo $AZURE_CLIENT_ID
# Ensure all required variables are set
```

#### 4. VS Code Launch Issues
```
Error: VS Code is not launched
```

**Solution**: 
- For mock testing: Ensure `MOCK_VSCODE=true`
- For real testing: Ensure VS Code is installed and accessible

#### 5. Test Data Issues
```bash
# Validate test data
node scripts/validate-test-data.js

# Recreate test data
node scripts/create-test-data.js
```

### Debug Mode

#### Enable Debug Logging
```bash
DEBUG=* npx playwright test --grep "@electron"
```

#### Run Single Test with Debug
```bash
npx playwright test --grep "specific test name" --debug
```

#### Capture Additional Information
```bash
npx playwright test --grep "@electron" --trace=on --video=on
```

### Performance Issues

#### Optimize Test Execution
```bash
# Reduce browser instances
npx playwright test --workers=1 --grep "@electron"

# Skip unnecessary browsers
npx playwright test --project=chromium --grep "@electron"
```

#### Monitor Resource Usage
```bash
# Check resource usage report
cat test-results/reports/resource-usage-report.json
```

## Advanced Usage

### CI/CD Integration

#### GitHub Actions Example
```yaml
name: VS Code Desktop Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: npm install
      
      - name: Run VS Code Desktop tests
        run: |
          NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \
          npx playwright test --grep "@electron.*@integration"
        env:
          CI: true
      
      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results
          path: test-results/
```

#### Jenkins Pipeline Example
```groovy
pipeline {
    agent any
    
    stages {
        stage('Test') {
            steps {
                sh '''
                    NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \
                    npx playwright test --grep "@electron.*@integration"
                '''
            }
        }
    }
    
    post {
        always {
            publishHTML([
                allowMissing: false,
                alwaysLinkToLastBuild: true,
                keepAll: true,
                reportDir: 'test-results',
                reportFiles: 'index.html',
                reportName: 'Test Report'
            ])
        }
    }
}
```

### Custom Reporters

#### Create Custom Reporter
```typescript
// reporters/custom-reporter.ts
import { Reporter, TestCase, TestResult } from '@playwright/test/reporter';

class CustomReporter implements Reporter {
  onTestEnd(test: TestCase, result: TestResult) {
    console.log(`Test ${test.title}: ${result.status}`);
    // Custom reporting logic
  }
}

export default CustomReporter;
```

#### Use Custom Reporter
```bash
npx playwright test --reporter=./reporters/custom-reporter.ts
```

### Performance Monitoring

#### Add Performance Metrics
```typescript
// In test files
test('performance test', async () => {
  const startTime = Date.now();
  
  // Test operations
  await vscodeHelper.launch();
  
  const endTime = Date.now();
  const duration = endTime - startTime;
  
  expect(duration).toBeLessThan(10000); // 10 seconds max
});
```

#### Monitor Resource Usage
```typescript
// Add to test setup
test.beforeEach(async () => {
  // Monitor memory usage
  const memUsage = process.memoryUsage();
  console.log('Memory usage:', memUsage);
});
```

### Test Data Management

#### Dynamic Test Data Generation
```typescript
// scripts/generate-test-data.ts
import { faker } from '@faker-js/faker';

function generateTestDataset(rows: number) {
  const data = [];
  for (let i = 0; i < rows; i++) {
    data.push({
      id: faker.datatype.uuid(),
      name: faker.name.fullName(),
      value: faker.datatype.number()
    });
  }
  return data;
}
```

#### Test Data Cleanup
```typescript
// In test teardown
test.afterEach(async () => {
  // Clean up test files
  await fs.remove('test-data/temp/');
});
```

## Best Practices

### 1. Test Organization
- Use descriptive test names
- Group related tests in describe blocks
- Use appropriate tags (@electron, @integration, etc.)
- Keep tests focused and atomic

### 2. Mock Usage
- Use mocks for development and CI/CD
- Implement realistic delays in mocks
- Maintain mock behavior consistency
- Document mock limitations

### 3. Real Testing
- Use real testing for production validation
- Implement proper cleanup procedures
- Monitor Azure resource usage
- Use dedicated test environments

### 4. Error Handling
- Implement comprehensive error handling
- Capture screenshots on failures
- Log detailed error information
- Provide clear error messages

### 5. Maintenance
- Regularly update test data
- Keep mock implementations current
- Monitor test execution times
- Review and update documentation

## Support and Resources

### Documentation
- [Playwright Documentation](https://playwright.dev/)
- [Azure ML SDK Documentation](https://docs.microsoft.com/en-us/azure/machine-learning/)
- [VS Code Extension API](https://code.visualstudio.com/api)

### Getting Help
- Check the troubleshooting section
- Review test logs and error messages
- Consult the test execution summary
- Create issues in the project repository

### Contributing
- Follow the existing code patterns
- Add tests for new features
- Update documentation
- Submit pull requests with clear descriptions

---

*Last Updated: 2025-09-14*
*Version: 1.0.0*
*Framework: Playwright + Azure ML + VS Code Desktop*