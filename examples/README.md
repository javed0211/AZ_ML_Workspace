# VS Code Desktop Testing Examples

This directory contains practical examples and demonstrations of how to use the VS Code Desktop testing framework for Azure ML workspace automation.

## 🚀 Quick Start

### Run the Interactive Example
```bash
# Show usage examples and next steps
node examples/example-usage.js

# Run full demonstration with setup
node examples/example-usage.js --demo

# Run mock tests only
node examples/example-usage.js --test

# Show help
node examples/example-usage.js --help
```

## 📁 Files in this Directory

### `example-usage.js`
Interactive demonstration script that shows:
- Prerequisites checking
- Environment setup
- Mock testing execution
- Test categories and filtering
- Reporting and results viewing
- Customization options
- Debugging techniques
- Usage examples for different scenarios

## 🎯 What You'll Learn

### 1. Basic Usage
- How to run mock tests for development
- How to set up environment variables
- How to view test results and reports

### 2. Advanced Features
- Test categorization and filtering
- Custom test configurations
- Debugging and troubleshooting
- Performance monitoring

### 3. Real-World Scenarios
- Development workflow with mocks
- Production validation with real Azure
- CI/CD pipeline integration
- Custom test creation

## 🛠️ Prerequisites

Before running the examples, ensure you have:
- Node.js 16+ installed
- npm or yarn package manager
- Project dependencies installed (`npm install`)

## 📚 Related Documentation

- [Complete Testing Guide](../docs/VSCODE_DESKTOP_TESTING_GUIDE.md)
- [Quick Reference](../docs/QUICK_REFERENCE.md)
- [Test Execution Summary](../TEST_EXECUTION_SUMMARY.md)

## 🎨 Example Scenarios

### Development Testing
```bash
# Fast mock testing for development
NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \
npx playwright test --grep "@electron.*@integration"
```

### Production Validation
```bash
# Real Azure testing (requires credentials)
export AZURE_TENANT_ID=your-tenant-id
export AZURE_CLIENT_ID=your-client-id
export AZURE_CLIENT_SECRET=your-client-secret
NODE_ENV=production npx playwright test --grep "@electron.*@integration"
```

### Debug Mode
```bash
# Debug specific test
NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \
npx playwright test --grep "should launch VS Code" --debug
```

### CI/CD Pipeline
```bash
# Automated testing in CI/CD
NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \
npx playwright test --grep "@electron.*@integration" --reporter=json
```

## 🔧 Customization

The example script demonstrates how to:
- Modify test timeouts and workers
- Add custom environment variables
- Create new test categories
- Implement custom reporting
- Add debugging capabilities

## 🐛 Troubleshooting

Common issues and solutions are demonstrated in the example script:
- Test timeouts → Increase timeout values
- Missing mock methods → Add to mock helpers
- Authentication failures → Check environment variables
- Test data issues → Validate and recreate test data

## 🚀 Next Steps

After running the examples:
1. Explore the generated test results in `test-results/`
2. Customize test data in `test-data/` for your needs
3. Create new tests in `src/tests/electron/`
4. Set up real Azure credentials for production testing
5. Integrate tests into your CI/CD pipeline

---
*Examples for VS Code Desktop Testing Framework*
*Last Updated: 2025-09-14*