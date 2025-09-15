# VS Code Desktop Testing Framework for Azure ML

## 🎯 Overview

This comprehensive testing framework enables automated testing of VS Code Desktop integration with Azure ML workspaces. It supports both mock testing (for development) and real Azure integration testing (for production validation).

## ✨ Features

- **🎭 Mock Testing**: Fast, safe testing with simulated Azure ML and VS Code operations
- **🔗 Real Integration**: End-to-end testing with actual Azure ML services
- **📊 Comprehensive Reporting**: Detailed test results, performance metrics, and visual reports
- **🐛 Debug Support**: Step-by-step debugging with traces and videos
- **🚀 CI/CD Ready**: Automated testing for continuous integration pipelines
- **📱 Multi-Browser**: Testing across Chromium, Firefox, WebKit, and mobile browsers

## 🚀 Quick Start

### 1. Install Dependencies
```bash
npm install
npx playwright install
```

### 2. Run Mock Tests (Recommended for Development)
```bash
# Simple command
npm run test:vscode:mock

# Or view the interactive example
npm run example
```

### 3. View Results
```bash
npm run test:report
```

## 📋 Available Commands

### Basic Testing
```bash
npm run test:vscode:mock          # Mock testing (development)
npm run test:vscode:integration   # Integration tests
npm run test:vscode:notebook      # Notebook-specific tests
npm run test:vscode:remote        # Remote compute tests
npm run test:vscode:debug         # Debug mode
```

### Examples and Documentation
```bash
npm run example                   # Interactive usage guide
npm run example:demo              # Full demonstration
npm run example:test              # Run mock tests
```

### Utilities
```bash
npm run validate:testdata         # Validate test data files
npm run create:testdata           # Create/recreate test data
npm run test:report               # Open HTML test report
```

## 📁 Project Structure

```
├── src/
│   ├── tests/electron/           # VS Code Desktop test files
│   ├── electron/                 # VS Code automation helpers
│   └── helpers/                  # Azure ML helpers and mocks
├── test-data/                    # Test configuration and sample files
├── examples/                     # Usage examples and demonstrations
├── docs/                         # Comprehensive documentation
├── scripts/                      # Utility scripts
└── test-results/                 # Generated test reports and artifacts
```

## 🎭 Mock vs Real Testing

### Mock Testing (Development)
- ✅ **Fast execution** - No real Azure API calls
- ✅ **No costs** - No Azure resource consumption
- ✅ **Consistent results** - Predictable test outcomes
- ✅ **Safe for CI/CD** - No authentication required
- ✅ **Offline capable** - Works without internet

**Use for**: Development, debugging, CI/CD pipelines, feature testing

### Real Testing (Production)
- ✅ **Actual functionality** - Tests real Azure ML operations
- ✅ **End-to-end validation** - Complete integration verification
- ✅ **Performance metrics** - Real-world performance data
- ✅ **Production readiness** - Validates deployment scenarios

**Use for**: Production validation, performance testing, integration verification

## 🔧 Configuration

### Environment Variables

#### Mock Testing (Default)
```bash
NODE_ENV=test
MOCK_VSCODE=true
MOCK_AZURE_SERVICES=true
```

#### Real Azure Testing
```bash
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_ML_WORKSPACE_NAME=your-workspace-name
```

### Configuration Files

#### VS Code Settings (`test-data/vscode-settings.json`)
```json
{
  "python.defaultInterpreterPath": "/opt/miniconda/envs/azureml_py38/bin/python",
  "azureML.workspaceConfigPath": "./test-data/azure-ml-config.json",
  "azureML.enableRemoteCompute": true,
  "jupyter.askForKernelRestart": false
}
```

#### Azure ML Config (`test-data/azure-ml-config.json`)
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

## 🧪 Test Scenarios

### 1. VS Code Launch and Azure ML Connection
- Launches VS Code Desktop application
- Installs and activates Azure ML extension
- Connects to Azure account and workspace
- Verifies workspace status and available commands

### 2. Jupyter Notebook Operations
- Creates new Jupyter notebooks
- Executes notebook cells with Azure ML code
- Manages kernel connections
- Tests remote compute integration

### 3. Remote Compute Management
- Connects to Azure ML compute instances
- Executes remote Python scripts
- Manages compute lifecycle
- Tests data transfer operations

## 📊 Test Results and Reporting

### HTML Reports
```bash
npm run test:report
```
Interactive HTML report with:
- Test execution timeline
- Pass/fail status for each test
- Screenshots and videos of failures
- Performance metrics

### JSON Reports
```bash
cat test-results/reports/test-summary.json
cat test-results/reports/performance-report.json
cat test-results/reports/resource-usage-report.json
```

### Logs and Artifacts
- **Screenshots**: Captured on test failures
- **Videos**: Full test execution recordings
- **Traces**: Detailed step-by-step execution
- **Logs**: Structured JSON logging with correlation IDs

## 🐛 Debugging and Troubleshooting

### Debug Mode
```bash
npm run test:vscode:debug
```
Opens browser in debug mode with step-by-step execution.

### Common Issues

#### Test Timeouts
```bash
# Increase timeout
npx playwright test --timeout=120000 --grep "@electron"
```

#### Missing Mock Methods
Add to `src/electron/vscode-electron-mock.ts`:
```typescript
async yourMethod(): Promise<void> {
  logger.info('Mock: Your method called');
  await this.delay(500);
}
```

#### Authentication Failures
```bash
# Verify environment variables
echo $AZURE_TENANT_ID
echo $AZURE_CLIENT_ID
```

#### Test Data Issues
```bash
npm run validate:testdata
npm run create:testdata
```

## 🔄 CI/CD Integration

### GitHub Actions
```yaml
- name: Run VS Code Desktop Tests
  run: npm run test:vscode:mock
  env:
    CI: true
```

### Jenkins
```groovy
stage('VS Code Tests') {
    steps {
        sh 'npm run test:vscode:mock'
    }
}
```

## 🎨 Customization

### Adding New Tests
1. Create test file in `src/tests/electron/`
2. Add test data in `test-data/`
3. Update mock helpers if needed
4. Run tests with appropriate tags

### Custom Mock Behavior
```typescript
// In MockVSCodeElectronHelper
async customOperation(params: any): Promise<any> {
  logger.info('Mock: Custom operation', { params });
  await this.delay(1000);
  return { result: 'custom-result' };
}
```

### Custom Test Data
```bash
# Create custom configuration
echo '{"custom": "config"}' > test-data/custom-config.json
```

## 📚 Documentation

### Comprehensive Guides
- **[Complete Testing Guide](docs/VSCODE_DESKTOP_TESTING_GUIDE.md)** - Detailed documentation
- **[Quick Reference](docs/QUICK_REFERENCE.md)** - Command reference and troubleshooting
- **[Test Execution Summary](TEST_EXECUTION_SUMMARY.md)** - Latest test results

### Examples
- **[Interactive Examples](examples/)** - Hands-on demonstrations
- **[Usage Examples](examples/example-usage.js)** - Practical implementation examples

## 🏆 Best Practices

### Development Workflow
1. **Start with mock tests** for rapid development
2. **Use debug mode** for troubleshooting
3. **Validate test data** regularly
4. **Monitor performance** metrics

### Production Validation
1. **Test with real Azure** before deployment
2. **Use dedicated test environments**
3. **Monitor resource usage**
4. **Implement proper cleanup**

### CI/CD Integration
1. **Use mock tests** in pipelines
2. **Implement parallel execution** carefully
3. **Store test artifacts**
4. **Monitor test execution times**

## 🤝 Contributing

### Adding Features
1. Follow existing code patterns
2. Add comprehensive tests
3. Update documentation
4. Submit pull requests with clear descriptions

### Reporting Issues
1. Check troubleshooting section
2. Review test logs
3. Provide reproduction steps
4. Include environment details

## 📈 Performance and Monitoring

### Execution Times
- **Mock tests**: ~30-60 seconds per test case
- **Real tests**: ~2-5 minutes per test case
- **Setup time**: ~1 second

### Resource Usage
- **Memory**: Minimal for mock tests
- **CPU**: Low to moderate usage
- **Network**: None for mock tests

### Monitoring
```bash
# Check performance reports
cat test-results/reports/performance-report.json

# Monitor resource usage
cat test-results/reports/resource-usage-report.json
```

## 🔮 Future Enhancements

### Planned Features
- Visual regression testing
- Performance benchmarking
- Extended browser support
- Real-time test monitoring
- Advanced reporting dashboards

### Roadmap
1. **Q1 2025**: Enhanced mock capabilities
2. **Q2 2025**: Visual testing integration
3. **Q3 2025**: Performance optimization
4. **Q4 2025**: Advanced analytics

## 📞 Support

### Getting Help
- Review the [troubleshooting guide](docs/VSCODE_DESKTOP_TESTING_GUIDE.md#troubleshooting)
- Check [test execution logs](test-results/logs/)
- Run the [interactive examples](examples/)
- Create issues in the project repository

### Resources
- [Playwright Documentation](https://playwright.dev/)
- [Azure ML Documentation](https://docs.microsoft.com/en-us/azure/machine-learning/)
- [VS Code Extension API](https://code.visualstudio.com/api)

---

## 🎉 Getting Started

Ready to start testing? Run the interactive example:

```bash
npm run example
```

This will guide you through:
- ✅ Prerequisites checking
- ✅ Environment setup
- ✅ Mock test execution
- ✅ Results viewing
- ✅ Customization options

**Happy Testing!** 🚀

---
*VS Code Desktop Testing Framework for Azure ML*  
*Version 1.0.0 | Last Updated: 2025-09-14*  
*Built with Playwright + TypeScript + Azure ML SDK*