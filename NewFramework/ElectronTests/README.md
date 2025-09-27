# VS Code Electron Testing Framework

This directory contains a comprehensive Electron testing framework for automating VS Code using Playwright. The framework is specifically designed to test VS Code functionality, Azure ML integrations, and desktop application workflows.

## Overview

The framework treats VS Code as an Electron application and provides:
- **ElectronApplication** → Backend (Node/Electron process) access
- **Page** → VS Code UI window (Chromium) interaction
- Cross-platform support (Windows, macOS, Linux)
- Comprehensive VS Code automation utilities

## Directory Structure

```
ElectronTests/
├── config/
│   └── vscode-config.ts          # VS Code configuration and paths
├── utils/
│   ├── vscode-electron.ts        # Main VS Code Electron wrapper
│   └── test-helpers.ts           # Test utility functions
├── tests/
│   ├── vscode-basic.test.ts      # Basic VS Code functionality tests
│   ├── azure-ml-integration.test.ts # Azure ML specific tests
│   └── vscode-settings.test.ts   # Settings and configuration tests
├── temp/                         # Temporary files (auto-created)
├── package.json                  # Node.js dependencies
├── tsconfig.json                 # TypeScript configuration
├── playwright.config.ts          # Playwright test configuration
└── README.md                     # This file
```

## Prerequisites

1. **VS Code Installation**: Ensure VS Code is installed on your system
   - **Windows**: `C:\\Program Files\\Microsoft VS Code\\Code.exe`
   - **macOS**: `/Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code` (newer) or `Electron` (older)
   - **Linux**: `/usr/share/code/code` or `/usr/bin/code`

2. **Node.js**: Version 16 or higher

### 🚨 **VS Code Not Installed?**

If you get an error like `spawn /Applications/Visual Studio Code.app/Contents/MacOS/Electron ENOENT`, VS Code is not installed. 

**Quick Fix:**
```bash
# macOS (using Homebrew)
brew install --cask visual-studio-code

# Windows (using winget)
winget install Microsoft.VisualStudioCode

# Linux (Ubuntu/Debian)
sudo apt install code

# Or run our installation script
./install-vscode.sh
```

**Validate Installation:**
```bash
node validate-vscode.js
```

See [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) for detailed solutions.
3. **TypeScript**: For development (installed as dev dependency)

## Setup

1. **Install Dependencies**:
   ```bash
   cd NewFramework/ElectronTests
   npm install
   ```

2. **Install Playwright Browsers**:
   ```bash
   npm run install-browsers
   ```

3. **Build TypeScript**:
   ```bash
   npm run build
   ```

## Configuration

### VS Code Executable Path

The framework automatically detects VS Code installation paths for different platforms. If you have a custom installation, modify the path in `config/vscode-config.ts`:

```typescript
// Custom VS Code path
const customConfig = {
  executablePath: '/path/to/your/vscode/executable'
};
```

### Launch Arguments

Default launch arguments are configured for testing:
- `--no-sandbox`: Disable sandbox for testing
- `--disable-extensions`: Start with clean slate
- `--disable-gpu`: Disable GPU acceleration
- Custom user data directory for isolation

## Running Tests

### Basic Commands

```bash
# Run all tests
npm test

# Run tests in headed mode (visible browser)
npm run test:headed

# Run tests with debugging
npm run test:debug

# Run tests with UI mode
npm run test:ui

# Build only
npm run build

# Clean build artifacts
npm run clean
```

### Specific Test Files

```bash
# Run basic functionality tests
npx playwright test vscode-basic.test.ts

# Run Azure ML integration tests
npx playwright test azure-ml-integration.test.ts

# Run settings tests
npx playwright test vscode-settings.test.ts
```

## Test Categories

### 1. Basic Functionality Tests (`vscode-basic.test.ts`)
- VS Code launch and initialization
- File creation and editing
- Command palette operations
- Keyboard shortcuts
- Multiple window handling

### 2. Azure ML Integration Tests (`azure-ml-integration.test.ts`)
- Azure ML workspace configuration
- Python ML script creation
- Jupyter notebook handling
- Azure ML extension commands
- Project structure navigation
- Terminal operations

### 3. Settings and Configuration Tests (`vscode-settings.test.ts`)
- Settings UI navigation
- JSON settings modification
- Python interpreter configuration
- Extension management
- Workspace settings
- Keybindings configuration
- Color theme selection
- User snippets management

## Key Features

### VSCodeElectron Class

Main wrapper class providing:

```typescript
// Launch VS Code
await vscode.launch(workspaceOrFile, additionalArgs);

// File operations
await vscode.createNewFile();
await vscode.openFile(filePath);
await vscode.saveFile();

// Editor operations
await vscode.typeInEditor(text);
await vscode.getEditorContent();

// Command operations
await vscode.openCommandPalette();
await vscode.executeCommand(command);

// Utility operations
await vscode.takeScreenshot(path);
await vscode.waitForElement(selector);
await vscode.close();
```

### TestHelpers Class

Utility functions for:
- Workspace management
- File operations
- Retry logic
- State verification
- Logging

## Platform-Specific Notes

### Windows
- Uses `Code.exe` executable
- Supports both Program Files locations
- User data in `%APPDATA%\Code`

### macOS
- Uses Electron executable inside app bundle
- User data in `~/Library/Application Support/Code`
- Supports both Intel and Apple Silicon

### Linux
- Multiple installation paths supported
- User data in `~/.config/Code`
- Snap package support

## Reporting

Test results are generated in multiple formats:
- **HTML Report**: `../Reports/electron-test-results/index.html`
- **JSON Report**: `../Reports/electron-test-results.json`
- **JUnit XML**: `../Reports/electron-junit-results.xml`
- **Screenshots**: Captured on failures and key steps

## Troubleshooting

### Common Issues

1. **VS Code Not Found**:
   - Verify installation path in `vscode-config.ts`
   - Check executable permissions

2. **Launch Timeout**:
   - Increase timeout in launch options
   - Check system resources

3. **Element Not Found**:
   - VS Code UI may take time to load
   - Increase wait times or use retry logic

4. **Permission Issues**:
   - Ensure VS Code can write to user data directory
   - Check file system permissions

### Debug Mode

Run tests in debug mode to step through execution:
```bash
npm run test:debug
```

### Verbose Logging

Enable detailed logging by setting environment variable:
```bash
DEBUG=pw:api npm test
```

## Best Practices

1. **Wait for Elements**: Always wait for UI elements to be ready
2. **Clean State**: Use fresh user data directory for each test
3. **Error Handling**: Wrap operations in try-catch blocks
4. **Screenshots**: Take screenshots for verification and debugging
5. **Cleanup**: Always close VS Code and clean temporary files

## Integration with CI/CD

The framework is designed to work in CI/CD environments:
- Headless mode by default
- Configurable timeouts
- Multiple report formats
- Screenshot capture on failures

Example GitHub Actions integration:
```yaml
- name: Run Electron Tests
  run: |
    cd NewFramework/ElectronTests
    npm install
    npm run install-browsers
    npm test
```

## Contributing

When adding new tests:
1. Follow existing naming conventions
2. Use TestHelpers for common operations
3. Add proper error handling
4. Include screenshots for verification
5. Update this README if adding new features

## Support

For issues or questions:
1. Check the troubleshooting section
2. Review Playwright documentation
3. Check VS Code extension APIs
4. Create an issue with detailed logs and screenshots