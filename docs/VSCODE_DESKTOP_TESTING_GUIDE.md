# VS Code Desktop Testing Guide

## Overview

This guide covers testing VS Code Desktop integration with Azure ML workspaces using the Python automation framework.

## Prerequisites

- Python 3.9+
- VS Code Desktop installed
- Azure ML workspace access
- Framework setup completed

## Configuration

Add VS Code path to your `.env` file:
```env
VSCODE_PATH=/Applications/Visual Studio Code.app/Contents/MacOS/Electron
```

## Running VS Code Tests

```bash
# Run all VS Code tests
python -m azure_ml_automation.cli test -p "vscode"

# Run specific VS Code test
python -m azure_ml_automation.cli test -p "test_vscode_desktop"

# Run with debugging
python -m azure_ml_automation.cli test -p "vscode" --headed -v
```

## Test Structure

```
tests/electron/
├── test_vscode_desktop.py          # Main VS Code tests
├── test_vscode_integration.py      # Integration tests
└── test_vscode_notebooks.py        # Notebook-specific tests
```

## Writing VS Code Tests

```python
from azure_ml_automation.tests.base_test import BaseTest
from azure_ml_automation.helpers.electron_manager import ElectronManager

class TestVSCodeDesktop(BaseTest):
    async def test_vscode_launch(self):
        async with ElectronManager() as electron:
            # Test VS Code functionality
            await electron.launch_vscode()
            # Add your test logic here
```

## Common Test Scenarios

### 1. Workspace Connection
```python
async def test_workspace_connection(self):
    async with ElectronManager() as electron:
        await electron.connect_to_workspace()
        # Verify connection
```

### 2. Notebook Operations
```python
async def test_notebook_execution(self):
    async with ElectronManager() as electron:
        await electron.open_notebook("test.ipynb")
        await electron.run_notebook()
        # Verify results
```

### 3. Compute Instance Integration
```python
async def test_compute_integration(self):
    async with ElectronManager() as electron:
        await electron.connect_to_compute()
        # Test compute functionality
```

## Troubleshooting

### VS Code Not Found
- Verify `VSCODE_PATH` in `.env`
- Check VS Code installation

### Connection Issues
- Verify Azure credentials
- Check workspace permissions

### Performance Issues
- Increase timeouts in configuration
- Use fewer parallel workers

## Best Practices

1. **Clean State**: Start each test with a clean VS Code instance
2. **Error Handling**: Implement proper error handling for VS Code operations
3. **Resource Cleanup**: Ensure proper cleanup of VS Code processes
4. **Timeouts**: Use appropriate timeouts for VS Code operations

## Example Test

```python
import pytest
from azure_ml_automation.tests.base_test import BaseTest
from azure_ml_automation.helpers.electron_manager import ElectronManager

class TestVSCodeWorkspace(BaseTest):
    
    async def test_workspace_navigation(self):
        """Test basic workspace navigation in VS Code."""
        async with ElectronManager() as electron:
            # Launch VS Code
            await electron.launch_vscode()
            
            # Connect to workspace
            await electron.connect_to_workspace()
            
            # Navigate to files
            await electron.open_file_explorer()
            
            # Verify workspace is loaded
            assert await electron.is_workspace_loaded()
    
    async def test_notebook_creation(self):
        """Test creating and running a notebook."""
        async with ElectronManager() as electron:
            await electron.launch_vscode()
            await electron.connect_to_workspace()
            
            # Create new notebook
            notebook_path = await electron.create_notebook("test_notebook.ipynb")
            
            # Add and run cell
            await electron.add_notebook_cell("print('Hello, Azure ML!')")
            await electron.run_notebook_cell()
            
            # Verify output
            output = await electron.get_cell_output()
            assert "Hello, Azure ML!" in output
```