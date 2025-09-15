# VS Code Desktop Testing - Quick Reference

## 🚀 Quick Start Commands

### Mock Testing (Development)
```bash
# Run all VS Code Desktop tests with mocks
NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \
npx playwright test --grep "@electron.*@integration" --reporter=line --workers=1

# Short version using npm script
npm run test:vscode:mock
```

### Real Azure Testing (Production)
```bash
# Set environment variables first
export AZURE_TENANT_ID=your-tenant-id
export AZURE_CLIENT_ID=your-client-id  
export AZURE_CLIENT_SECRET=your-client-secret
export AZURE_SUBSCRIPTION_ID=your-subscription-id
export AZURE_RESOURCE_GROUP=your-resource-group
export AZURE_ML_WORKSPACE_NAME=your-workspace-name

# Run tests
NODE_ENV=production npx playwright test --grep "@electron.*@integration"
```

## 📋 Common Test Commands

| Command | Description |
|---------|-------------|
| `npm run test:vscode:mock` | Run all VS Code tests with mocks |
| `npx playwright test --grep "@electron"` | Run all electron tests |
| `npx playwright test --grep "@integration"` | Run integration tests only |
| `npx playwright test --grep "@notebook"` | Run notebook-specific tests |
| `npx playwright test --debug` | Run tests in debug mode |
| `npx playwright show-report` | Open test results in browser |

## 🔧 Environment Variables

### Required for Mock Testing
```bash
NODE_ENV=test
MOCK_VSCODE=true
MOCK_AZURE_SERVICES=true
```

### Required for Real Testing
```bash
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_ML_WORKSPACE_NAME=your-workspace-name
```

### Optional Settings
```bash
SKIP_AZURE_AUTH=true        # Skip Azure authentication
TEST_TIMEOUT=60000          # Test timeout in milliseconds
TEST_WORKERS=1              # Number of parallel workers
```

## 📁 Key Files and Directories

```
├── src/tests/electron/vscode-desktop.spec.ts    # Main test file
├── src/electron/vscode-electron-mock.ts         # VS Code mock helper
├── src/helpers/azure-helpers-mock.ts            # Azure ML mock helper
├── test-data/                                   # Test data files
│   ├── vscode-settings.json                     # VS Code configuration
│   ├── azure-ml-config.json                     # Azure ML configuration
│   ├── sample-notebook.ipynb                    # Sample Jupyter notebook
│   └── sample-python-script.py                  # Sample Python script
├── scripts/                                     # Utility scripts
│   ├── run-vscode-test.js                       # Test runner
│   └── validate-test-data.js                    # Data validation
└── test-results/                                # Test output
    └── reports/                                 # Generated reports
```

## 🎯 Test Tags

| Tag | Description |
|-----|-------------|
| `@electron` | VS Code Desktop tests |
| `@integration` | Integration tests |
| `@notebook` | Jupyter notebook tests |
| `@remote` | Remote compute tests |
| `@mock` | Mock-only tests |

## 🐛 Troubleshooting Quick Fixes

### Test Timeouts
```bash
npx playwright test --timeout=120000 --grep "@electron"
```

### Missing Mock Methods
Add to `src/electron/vscode-electron-mock.ts`:
```typescript
async yourMethod(): Promise<void> {
  logger.info('Mock: Your method called');
  await this.delay(500);
}
```

### Authentication Issues
```bash
# Verify environment variables
echo $AZURE_TENANT_ID
echo $AZURE_CLIENT_ID
```

### Test Data Issues
```bash
# Validate test data
node scripts/validate-test-data.js

# Recreate test data  
node scripts/create-test-data.js
```

## 📊 Viewing Results

### HTML Report
```bash
npx playwright show-report
```

### JSON Reports
```bash
cat test-results/reports/test-summary.json
cat test-results/reports/performance-report.json
```

### Logs
```bash
# View latest test logs
tail -f test-results/logs/latest.log
```

## 🔄 CI/CD Integration

### GitHub Actions
```yaml
- name: Run VS Code Desktop tests
  run: |
    NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \
    npx playwright test --grep "@electron.*@integration"
```

### Jenkins
```groovy
sh '''
  NODE_ENV=test MOCK_VSCODE=true MOCK_AZURE_SERVICES=true \
  npx playwright test --grep "@electron.*@integration"
'''
```

## 📝 Adding New Tests

1. **Create test file**: `src/tests/electron/your-test.spec.ts`
2. **Add test data**: `test-data/your-data.json`
3. **Update mock helpers**: Add required methods
4. **Run tests**: `npx playwright test --grep "your-test"`

## 🎨 Mock Customization

### Add Mock Method
```typescript
// In MockVSCodeElectronHelper
async customMethod(param: string): Promise<any> {
  logger.info('Mock: Custom method', { param });
  await this.delay(1000);
  return { result: 'mock-result' };
}
```

### Modify Mock Behavior
```typescript
// Change delays, responses, or logic
async launch(): Promise<void> {
  logger.info('Mock: Custom launch behavior');
  await this.delay(2000); // Custom delay
  this.isLaunched = true;
}
```

## 🔍 Debug Mode

```bash
# Run single test with debug
npx playwright test --grep "specific test" --debug

# Enable verbose logging
DEBUG=* npx playwright test --grep "@electron"

# Capture traces and videos
npx playwright test --trace=on --video=on --grep "@electron"
```

## 📈 Performance Monitoring

```bash
# Monitor resource usage
cat test-results/reports/resource-usage-report.json

# Check test execution times
grep "duration" test-results/reports/performance-report.json
```

## 🛠️ Maintenance Commands

```bash
# Update dependencies
npm update

# Clean test results
rm -rf test-results/

# Validate configuration
node scripts/validate-config.js

# Reset test environment
npm run test:reset
```

---
*Quick Reference for VS Code Desktop Testing Framework*
*Last Updated: 2025-09-14*