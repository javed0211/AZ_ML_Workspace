# Cross-Platform Jupyter Notebook Test Runner

This test suite provides a comprehensive solution for running existing Jupyter notebook files in VS Code across Windows, Linux, and macOS platforms.

## 🎯 Features

- ✅ **Cross-Platform Support**: Works on Windows, Linux, and macOS
- ✅ **Existing Notebook Support**: Opens and runs notebooks from any folder
- ✅ **Multiple File Handling**: Can open all notebooks in a folder
- ✅ **Real VS Code Integration**: Uses actual VS Code application, not mocks
- ✅ **Automatic Platform Detection**: Detects OS and configures accordingly
- ✅ **Fallback Mechanisms**: Creates test notebooks if none exist
- ✅ **Process Management**: Proper cleanup and verification

## 🚀 Quick Start

### 1. Basic Usage
```bash
# Run all cross-platform tests
npx playwright test tests/cross-platform-jupyter.test.ts --headed

# Run specific test for existing notebooks
npx playwright test tests/cross-platform-jupyter.test.ts -g "existing notebook" --headed
```

### 2. Custom Folder Usage
```bash
# Set environment variables to use your own notebooks
export NOTEBOOK_FOLDER="/path/to/your/notebooks"
export NOTEBOOK_FILE="your_notebook.ipynb"
npx playwright test tests/cross-platform-jupyter.test.ts --headed
```

### 3. Windows Usage
```cmd
# Windows Command Prompt
set NOTEBOOK_FOLDER=C:\Users\YourName\Documents\Notebooks
set NOTEBOOK_FILE=my_notebook.ipynb
npx playwright test tests/cross-platform-jupyter.test.ts --headed
```

```powershell
# Windows PowerShell
$env:NOTEBOOK_FOLDER="C:\Users\YourName\Documents\Notebooks"
$env:NOTEBOOK_FILE="my_notebook.ipynb"
npx playwright test tests/cross-platform-jupyter.test.ts --headed
```

### 4. Linux Usage
```bash
# Linux/Ubuntu
export NOTEBOOK_FOLDER="/home/username/notebooks"
export NOTEBOOK_FILE="analysis.ipynb"
npx playwright test tests/cross-platform-jupyter.test.ts --headed
```

## 📋 Test Cases

### Test 1: Cross-Platform Notebook Creation and Execution
- Creates a comprehensive test notebook with system information
- Tests basic Python libraries (numpy, pandas)
- Performs file operations
- Validates cross-platform compatibility

### Test 2: Different Notebook Formats and Encodings
- Tests simple and complex notebook structures
- Handles special characters and Unicode
- Validates different cell types (markdown, code, raw)

### Test 3: Existing Notebook Execution
- Opens existing notebooks from specified folders
- Handles multiple notebooks in a folder
- Provides fallback mechanisms
- Validates notebook structure before opening

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `NOTEBOOK_FOLDER` | Path to existing notebook folder | `/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook` |
| `NOTEBOOK_FILE` | Specific notebook to open | `azure_ml_project.ipynb` |
| `VSCODE_PATH` | Custom VS Code path | Auto-detected |

### Platform-Specific Settings

The test automatically detects your platform and configures:

#### Windows
- VS Code Command: `code.cmd`
- Default Path: `%LOCALAPPDATA%\Programs\Microsoft VS Code\bin\code.cmd`
- Process Check: Uses `tasklist`
- Process Kill: Uses `taskkill`

#### macOS
- VS Code Command: `code`
- Default Path: `/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code`
- Process Check: Uses `ps aux`
- Process Kill: Uses `kill -TERM`

#### Linux
- VS Code Command: `code`
- Default Path: `/usr/bin/code`
- Process Check: Uses `ps aux`
- Process Kill: Uses `kill -TERM`
- Additional Args: `--no-sandbox` (for automation)

## 📁 File Structure

```
tests/
├── cross-platform-jupyter.test.ts    # Main test file
├── cross-platform-config.json        # Configuration file
└── README-CrossPlatform.md           # This documentation

temp/
├── cross-platform-{timestamp}/       # Test workspaces
│   ├── cross_platform_test.ipynb    # Generated test notebook
│   ├── requirements.txt              # Python dependencies
│   ├── config.json                   # Project configuration
│   ├── data_sample.csv              # Sample data
│   └── README.md                     # Project documentation
```

## 🎯 Example Usage Scenarios

### Scenario 1: Data Science Project
```bash
# Point to your data science notebooks folder
export NOTEBOOK_FOLDER="/Users/yourname/DataScience/Projects"
export NOTEBOOK_FILE="analysis.ipynb"
npx playwright test tests/cross-platform-jupyter.test.ts --headed
```

### Scenario 2: Machine Learning Experiments
```bash
# Open all ML notebooks in a folder
export NOTEBOOK_FOLDER="/path/to/ml-experiments"
npx playwright test tests/cross-platform-jupyter.test.ts -g "existing notebook" --headed
```

### Scenario 3: Educational Content
```bash
# Test educational Jupyter notebooks
export NOTEBOOK_FOLDER="/Users/teacher/JupyterLessons"
npx playwright test tests/cross-platform-jupyter.test.ts --headed
```

## 🔍 What the Test Does

1. **Platform Detection**: Automatically detects Windows/macOS/Linux
2. **VS Code Verification**: Checks if VS Code is installed and accessible
3. **Folder Scanning**: Scans specified folder for `.ipynb` files
4. **Notebook Validation**: Validates notebook JSON structure
5. **VS Code Launch**: Opens VS Code with the notebook folder
6. **File Opening**: Opens individual notebooks in VS Code
7. **Process Verification**: Confirms VS Code is running properly
8. **Cleanup**: Properly closes VS Code processes after testing

## 🛠️ Troubleshooting

### VS Code Not Found
```bash
# Check if VS Code is in PATH
code --version

# If not, install VS Code and ensure it's in PATH
# Or set custom path:
export VSCODE_PATH="/custom/path/to/code"
```

### Permission Issues (Linux)
```bash
# If you get permission errors on Linux
sudo chmod +x /usr/bin/code
# Or run with --no-sandbox flag (automatically added)
```

### Windows Path Issues
```cmd
# Ensure VS Code is in PATH or use full path
set VSCODE_PATH="C:\Users\YourName\AppData\Local\Programs\Microsoft VS Code\bin\code.cmd"
```

## 📊 Test Output Example

```
🖥️  Detected platform: darwin
🔧 VS Code command: code
📍 VS Code executable: /Applications/Visual Studio Code.app/Contents/Resources/app/bin/code
📁 Looking for notebooks in: /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook
📓 Target notebook: azure_ml_project.ipynb
✅ Notebook folder exists: /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook
📚 Found 1 notebook files:
  - azure_ml_project.ipynb
🎯 Selected notebook: azure_ml_project.ipynb
✅ Notebook structure valid: 17 cells, nbformat 4
🚀 Opening existing folder in VS Code: /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook
✅ VS Code launched with PID: 4714
📖 Opening notebook: /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook/azure_ml_project.ipynb
📚 Opening all 1 notebooks:
  📓 Opening: azure_ml_project.ipynb
✅ VS Code processes verified: 44 processes running

============================================================
🎉 EXISTING NOTEBOOK EXECUTION TEST COMPLETED
============================================================
📁 Source folder: /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook
📓 Notebooks opened: 1
🖥️  Platform: darwin
✅ Status: SUCCESS
============================================================
```

## 🎉 Success!

Your cross-platform Jupyter notebook test runner is now ready! It will:
- ✅ Work on Windows, Linux, and macOS
- ✅ Open existing notebooks from any folder
- ✅ Handle multiple notebooks automatically
- ✅ Provide detailed logging and verification
- ✅ Clean up processes properly

Happy testing! 🚀