# VS Code Notebook Automation Guide

This guide explains how to use the TypeScript and Playwright-based automation for VS Code Jupyter notebook operations.

## 🎯 Overview

The VS Code Notebook Automation framework provides comprehensive automation capabilities for:

- **Launching VS Code** as an Electron application
- **Creating new Jupyter notebooks** (.ipynb files)
- **Opening existing notebooks**
- **Adding and executing code/markdown cells**
- **Verifying execution results**
- **Error handling and validation**
- **Screenshot capture for debugging**

## 🏗️ Architecture

### Core Components

1. **VSCodeNotebookUtils**: Main utility class for VS Code automation
2. **ElectronUtils**: Base Electron application automation utilities
3. **Feature Files**: BDD scenarios in Gherkin syntax
4. **Step Definitions**: TypeScript implementations of test steps

### File Structure

```
NewFramework/
├── TypeScriptTests/
│   ├── Features/
│   │   └── VSCodeNotebookAutomation.feature
│   └── StepDefinitions/
│       └── VSCodeNotebookSteps.ts
├── Utils/
│   └── ElectronUtils.ts
├── Scripts/
│   └── run-vscode-notebook-tests.sh
└── Config/
    └── appsettings.json
```

## 🚀 Quick Start

### Prerequisites

1. **VS Code installed** on your system
2. **Python extension** for VS Code (for Jupyter support)
3. **Node.js and npm** installed
4. **Required npm packages**:
   ```bash
   npm install @playwright/test @cucumber/cucumber ts-node typescript
   ```

### Configuration

Update `Config/appsettings.json` with your VS Code installation path:

```json
{
  "ElectronApp": {
    "ExecutablePath": "/Applications/Visual Studio Code.app/Contents/MacOS/Electron",
    "WindowsExecutablePath": "C:\\Users\\%USERNAME%\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe",
    "LinuxExecutablePath": "/usr/share/code/code",
    "Args": ["--no-sandbox", "--disable-dev-shm-usage"]
  }
}
```

### Running Tests

#### Option 1: Use the Test Runner Script
```bash
./Scripts/run-vscode-notebook-tests.sh
```

#### Option 2: Run Individual Tests
```bash
# Run all VS Code notebook tests
npx cucumber-js TypeScriptTests/Features/VSCodeNotebookAutomation.feature

# Run specific scenario
npx cucumber-js TypeScriptTests/Features/VSCodeNotebookAutomation.feature:8

# Run with tags
npx cucumber-js --tags '@vscode and @notebook'
```

## 📋 Test Scenarios

### 1. Create and Execute New Notebook
**Scenario**: Create a new Jupyter notebook and execute Python code cells

**Steps**:
- Launch VS Code
- Create new notebook
- Add Python code cells
- Execute all cells
- Verify results
- Save notebook

### 2. Open and Execute Existing Notebook
**Scenario**: Open an existing notebook and execute its cells

**Steps**:
- Launch VS Code
- Open existing `sample-notebook.ipynb`
- Execute all cells
- Verify execution results

### 3. Comprehensive Notebook Workflow
**Scenario**: Complete workflow with multiple cell types

**Steps**:
- Create new notebook with custom name
- Add markdown cell with title
- Add data analysis code cell
- Add visualization code cell
- Add machine learning code cell
- Execute all cells sequentially
- Verify all output types
- Save notebook
- Open existing notebook
- Execute partial cells

### 4. Error Handling
**Scenario**: Test error handling with broken and valid code

**Steps**:
- Create new notebook
- Add intentionally broken Python code
- Add valid Python code
- Execute all cells
- Verify error outputs
- Verify successful outputs

## 🔧 Key Features

### VS Code Automation Capabilities

#### Application Management
```typescript
// Launch VS Code
await vsCodeUtils.launchVSCode();

// Wait for full loading
await vsCodeUtils.waitForVSCodeToLoad();

// Close VS Code
await vsCodeUtils.closeVSCode();
```

#### Notebook Operations
```typescript
// Create new notebook
await vsCodeUtils.createNewNotebook('my-notebook.ipynb');

// Open existing notebook
await vsCodeUtils.openExistingNotebook('sample-notebook.ipynb');

// Save notebook
await vsCodeUtils.saveNotebook();
```

#### Cell Management
```typescript
// Add markdown cell
await vsCodeUtils.addMarkdownCell('# My Title');

// Add code cell
await vsCodeUtils.addCodeCell('print("Hello World")');

// Execute all cells
await vsCodeUtils.executeAllCells();

// Execute first N cells
await vsCodeUtils.executeFirstNCells(3);
```

#### Verification and Validation
```typescript
// Verify execution results
const hasResults = await vsCodeUtils.verifyExecutionResults();

// Verify markdown rendering
const hasMarkdown = await vsCodeUtils.verifyMarkdownRendering();

// Verify error handling
const hasErrors = await vsCodeUtils.verifyErrorHandling();
```

### Predefined Code Snippets

The framework includes predefined code snippets for testing:

1. **Data Analysis Code**: Pandas operations, data exploration
2. **Visualization Code**: Matplotlib/Seaborn plotting
3. **Machine Learning Code**: Scikit-learn model training
4. **Broken Code**: Intentional errors for testing
5. **Valid Code**: Simple successful operations

## 🎨 Customization

### Adding Custom Test Scenarios

1. **Add new scenario to feature file**:
```gherkin
@vscode @custom
Scenario: My custom notebook test
  When I launch VS Code using Electron automation
  And I create a new notebook with my custom content
  Then I should see my expected results
```

2. **Implement step definitions**:
```typescript
When('I create a new notebook with my custom content', async function () {
  await vsCodeUtils.createNewNotebook();
  await vsCodeUtils.addCodeCell('my custom code');
});
```

### Custom Code Snippets

Add your own code snippets to `VSCodeNotebookUtils`:

```typescript
getMyCustomCode(): string {
  return `# My custom code
import my_library
result = my_library.do_something()
print(result)`;
}
```

## 🐛 Debugging and Troubleshooting

### Common Issues

1. **VS Code not launching**:
   - Check executable path in `appsettings.json`
   - Verify VS Code is installed
   - Check permissions

2. **Notebook not creating**:
   - Ensure Python extension is installed in VS Code
   - Check VS Code is fully loaded before operations
   - Verify Jupyter support is available

3. **Cell execution failing**:
   - Check Python interpreter is configured
   - Verify required packages are installed
   - Check for kernel connection issues

### Debug Features

1. **Screenshots**: Automatic screenshots at key points
2. **Detailed logging**: Comprehensive logging of all operations
3. **Error capture**: Detailed error information and stack traces
4. **Timeout handling**: Configurable timeouts for operations

### Log Files

- **Test execution logs**: `Reports/logs/test-execution.log`
- **Screenshots**: `Reports/screenshots/vscode-*.png`
- **JSON results**: `Reports/vscode-*-results.json`

## 🔍 Advanced Usage

### Environment Variables

```bash
# Run in headless mode
export HEADLESS=true

# Set custom timeout
export DEFAULT_TIMEOUT=60000

# Enable debug logging
export LOG_LEVEL=debug
```

### Custom Configuration

```typescript
// Custom VS Code launch options
const customOptions = {
  executablePath: '/custom/path/to/vscode',
  args: ['--disable-extensions', '--new-window']
};
```

### Integration with CI/CD

```yaml
# GitHub Actions example
- name: Run VS Code Notebook Tests
  run: |
    export HEADLESS=true
    ./Scripts/run-vscode-notebook-tests.sh
```

## 📊 Test Results and Reporting

### Output Formats

1. **Console output**: Real-time test progress
2. **JSON reports**: Machine-readable results
3. **Screenshots**: Visual verification
4. **HTML reports**: Human-readable results (with additional setup)

### Metrics Tracked

- Test execution time
- Cell execution success/failure rates
- Error types and frequencies
- Screenshot capture points
- Performance metrics

## 🤝 Contributing

### Adding New Features

1. Extend `VSCodeNotebookUtils` class
2. Add corresponding step definitions
3. Create test scenarios in feature files
4. Update documentation

### Best Practices

1. **Use descriptive step names**
2. **Add proper error handling**
3. **Include verification steps**
4. **Take screenshots at key points**
5. **Use meaningful test data**

## 📚 References

- [Playwright Electron Documentation](https://playwright.dev/docs/api/class-electron)
- [Cucumber.js Documentation](https://cucumber.io/docs/cucumber/)
- [VS Code Extension API](https://code.visualstudio.com/api)
- [Jupyter Notebook Format](https://nbformat.readthedocs.io/)

## 🏷️ Tags Reference

- `@vscode`: VS Code related tests
- `@notebook`: Jupyter notebook tests
- `@electron`: Electron application tests
- `@existing`: Tests with existing files
- `@comprehensive`: Full workflow tests
- `@error-handling`: Error scenario tests

## 🎯 Next Steps

1. **Run your first test**: Use the quick start guide
2. **Explore scenarios**: Try different test scenarios
3. **Customize for your needs**: Add your own code snippets
4. **Integrate with CI/CD**: Automate your testing pipeline
5. **Extend functionality**: Add new VS Code automation features