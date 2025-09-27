# 📊 Jupyter Notebook Testing Guide

This guide explains how to test Jupyter notebook (.ipynb) functionality in VS Code using our Playwright-based testing framework.

## 🎯 **Quick Start**

### **1. Verify Setup**
```bash
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/ElectronTests
node verify-jupyter-setup.js
```

### **2. Run Jupyter Tests**
```bash
# Run all Jupyter notebook tests
node run-jupyter-tests.js

# Or run with Playwright directly
npx playwright test tests/jupyter-notebook.test.ts --project=electron-vscode
```

### **3. View Results**
```bash
# Open HTML report
open test-results/html-report/index.html

# Check screenshots
ls test-results/
```

## 📋 **Test Cases Included**

### **1. Basic Functionality**
- ✅ **Notebook Opening**: Tests VS Code's ability to open .ipynb files
- ✅ **Display Validation**: Verifies notebook interface renders correctly
- ✅ **File Recognition**: Ensures VS Code recognizes notebook format

### **2. Cell Interactions**
- ✅ **Cell Navigation**: Tests keyboard shortcuts (j/k for navigation)
- ✅ **Edit Mode**: Tests entering/exiting cell edit mode
- ✅ **Content Modification**: Tests typing in notebook cells
- ✅ **Cell Selection**: Tests cell focus and selection

### **3. Execution Capabilities**
- ✅ **Run All Cells**: Tests Jupyter execution commands
- ✅ **Python Integration**: Tests Python extension compatibility
- ✅ **Kernel Management**: Tests kernel selection and management
- ✅ **Output Display**: Tests cell output rendering

### **4. File Operations**
- ✅ **Save Functionality**: Tests notebook saving
- ✅ **New Notebook Creation**: Tests creating new .ipynb files
- ✅ **File Management**: Tests notebook file operations
- ✅ **Auto-save**: Tests automatic saving features

### **5. Content Validation**
- ✅ **JSON Structure**: Validates notebook JSON format
- ✅ **Cell Types**: Tests markdown and code cells
- ✅ **Metadata**: Validates notebook metadata
- ✅ **Format Compliance**: Ensures nbformat compliance

### **6. Advanced Features**
- ✅ **Complex Content**: Tests rich markdown, math equations, lists
- ✅ **Data Science Libraries**: Tests pandas, numpy, matplotlib integration
- ✅ **Visualizations**: Tests plot rendering and display
- ✅ **Multiple Cell Types**: Tests mixed content notebooks

### **7. Error Handling**
- ✅ **Syntax Errors**: Tests handling of invalid Python code
- ✅ **Runtime Errors**: Tests execution error handling
- ✅ **Malformed Notebooks**: Tests corrupted file handling
- ✅ **Missing Dependencies**: Tests graceful degradation

## 🏗️ **Test Architecture**

### **File Structure**
```
ElectronTests/
├── tests/
│   └── jupyter-notebook.test.ts     # Main test suite
├── utils/
│   ├── test-helpers.ts              # Helper utilities
│   └── vscode-electron.ts           # VS Code automation
├── run-jupyter-tests.js             # Test runner script
├── verify-jupyter-setup.js          # Setup verification
└── JUPYTER_TESTING.md               # This guide
```

### **Test Flow**
1. **Setup**: Creates temporary workspace with test notebooks
2. **Launch**: Starts VS Code Electron instance
3. **Execute**: Runs comprehensive test scenarios
4. **Capture**: Takes screenshots and records interactions
5. **Validate**: Checks expected behaviors and outputs
6. **Cleanup**: Removes temporary files and closes VS Code

## 🔧 **Configuration**

### **Playwright Config**
The tests use the existing `playwright.config.ts` with these key settings:
- **Project**: `electron-vscode`
- **Headless**: `false` (visible testing)
- **Screenshots**: On failure
- **Videos**: On failure
- **Timeouts**: Extended for Electron apps

### **Test Environment**
- **Python Version**: 3.9.13 (via pyenv)
- **VS Code**: Latest stable via Electron
- **Extensions**: Python, Jupyter (if available)
- **Workspace**: Temporary isolated directories

## 📊 **Sample Notebooks**

### **Simple Test Notebook**
```json
{
  "cells": [
    {
      "cell_type": "markdown",
      "source": ["# Test Notebook\nSimple test content"]
    },
    {
      "cell_type": "code",
      "source": ["print('Hello, Jupyter!')"]
    }
  ],
  "metadata": {...},
  "nbformat": 4,
  "nbformat_minor": 4
}
```

### **Complex Test Notebook**
- **Data Science Libraries**: pandas, numpy, matplotlib, seaborn
- **Rich Markdown**: Headers, lists, math equations, code blocks
- **Visualizations**: Scatter plots, histograms, heatmaps
- **Statistical Analysis**: Descriptive statistics, correlations
- **Error Scenarios**: Syntax errors, runtime errors

## 🎯 **Expected Outcomes**

### **✅ Success Scenarios**
- Notebook opens in VS Code without errors
- Cells are properly displayed and navigable
- JSON structure is valid and compliant
- File operations work correctly
- Screenshots show proper rendering

### **⚠️ Partial Success**
- Notebook opens but Jupyter extension unavailable
- Basic editing works but execution limited
- File operations work but advanced features missing
- VS Code handles gracefully with fallbacks

### **❌ Failure Scenarios**
- VS Code fails to launch
- Notebook file cannot be opened
- Critical errors in test execution
- Invalid JSON structure
- File system permissions issues

## 🔍 **Troubleshooting**

### **Common Issues**

#### **VS Code Won't Launch**
```bash
# Check VS Code installation
code --version

# Verify Electron path
echo $PLAYWRIGHT_BROWSERS_PATH

# Try manual launch
node launch-vscode-visible.js
```

#### **Jupyter Extension Missing**
```bash
# Install Jupyter extension (optional)
code --install-extension ms-toolsai-jupyter

# Check installed extensions
code --list-extensions | grep jupyter
```

#### **Python Environment Issues**
```bash
# Verify Python version
python --version

# Check required packages
pip list | grep -E "(pandas|numpy|matplotlib)"

# Install missing packages
pip install pandas numpy matplotlib seaborn
```

#### **Test Failures**
```bash
# Run with debug output
DEBUG=pw:api npx playwright test tests/jupyter-notebook.test.ts

# Check test results
cat test-results/results.json

# View detailed HTML report
open test-results/html-report/index.html
```

## 📈 **Performance Expectations**

### **Timing**
- **Setup**: ~5-10 seconds
- **VS Code Launch**: ~10-15 seconds
- **Notebook Opening**: ~3-5 seconds
- **Each Test Case**: ~10-30 seconds
- **Total Suite**: ~5-10 minutes

### **Resources**
- **Memory**: ~500MB-1GB for VS Code
- **Disk**: ~50MB for temporary files
- **CPU**: Moderate during test execution

## 🚀 **Advanced Usage**

### **Custom Test Notebooks**
```javascript
// Create custom notebook for testing
const customNotebook = {
  cells: [
    {
      cell_type: "code",
      source: ["# Your custom test code here"]
    }
  ],
  metadata: { /* ... */ },
  nbformat: 4,
  nbformat_minor: 4
};

fs.writeFileSync('custom_test.ipynb', JSON.stringify(customNotebook, null, 2));
```

### **Selective Test Execution**
```bash
# Run specific test
npx playwright test -g "should open and display Jupyter notebook correctly"

# Run with specific timeout
npx playwright test tests/jupyter-notebook.test.ts --timeout=120000

# Run in headed mode with slow motion
npx playwright test tests/jupyter-notebook.test.ts --headed --slowMo=1000
```

### **CI/CD Integration**
```bash
# Headless mode for CI
npx playwright test tests/jupyter-notebook.test.ts --project=electron-vscode --reporter=junit

# Generate reports
npx playwright show-report
```

## 📝 **Contributing**

### **Adding New Test Cases**
1. Add test function to `jupyter-notebook.test.ts`
2. Use existing helper methods from `TestHelpers`
3. Include proper error handling and screenshots
4. Update this documentation

### **Extending Functionality**
1. Add new methods to `test-helpers.ts`
2. Extend `vscode-electron.ts` for new VS Code interactions
3. Update configuration if needed
4. Test thoroughly before committing

## 📞 **Support**

### **Getting Help**
- Check existing test results in `test-results/`
- Review screenshots for visual debugging
- Consult VS Code and Playwright documentation
- Check GitHub issues for similar problems

### **Reporting Issues**
Include in your report:
- Test output and error messages
- Screenshots from `test-results/`
- System information (OS, VS Code version, Python version)
- Steps to reproduce the issue

---

## 🎉 **Ready to Test!**

Your Jupyter notebook testing environment is now fully configured and ready to use. Run the verification script and then execute the test suite to validate VS Code's Jupyter notebook functionality.

```bash
# Final verification
node verify-jupyter-setup.js

# Run the tests
node run-jupyter-tests.js
```

Happy testing! 🚀📊