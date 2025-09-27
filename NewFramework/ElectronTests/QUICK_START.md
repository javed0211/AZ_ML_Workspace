# Quick Start Guide - VS Code Electron Testing

Get up and running with VS Code automation in 5 minutes!

## 🚀 Quick Setup

### 1. Prerequisites Check
- ✅ VS Code installed
- ✅ Node.js 16+ installed
- ✅ Terminal/Command Prompt access

### 2. One-Command Setup

**macOS/Linux:**
```bash
cd NewFramework/ElectronTests
./setup.sh
```

**Windows (PowerShell):**
```powershell
cd NewFramework\ElectronTests
.\setup.ps1
```

**Manual Setup:**
```bash
npm install
npm run install-browsers
npm run build
```

## 🧪 Run Your First Test

```bash
# Run the demo test
npm test -- --grep "Complete VS Code automation workflow demo"

# Run all tests
npm test

# Run with visible browser
npm run test:headed
```

## 📋 What You Get

The framework provides:

### Core Classes
- **`VSCodeElectron`** - Main VS Code automation wrapper
- **`TestHelpers`** - Utility functions for testing

### Key Methods
```typescript
// Launch VS Code
await vscode.launch();

// File operations
await vscode.createNewFile();
await vscode.openFile('path/to/file');
await vscode.typeInEditor('Hello World!');
await vscode.saveFile();

// Commands
await vscode.executeCommand('View: Toggle Terminal');
await vscode.openCommandPalette();

// Screenshots
await vscode.takeScreenshot();

// Cleanup
await vscode.close();
```

## 🎯 Example Test

```typescript
import { test, expect } from '@playwright/test';
import { VSCodeElectron } from '../utils/vscode-electron';
import { TestHelpers } from '../utils/test-helpers';

test('My first VS Code test', async () => {
  const vscode = new VSCodeElectron();
  
  try {
    // Launch VS Code
    await vscode.launch();
    
    // Create and edit file
    await vscode.createNewFile();
    await vscode.typeInEditor('print("Hello from automation!")');
    
    // Take screenshot
    await vscode.takeScreenshot();
    
    // Verify VS Code is working
    expect(vscode.isRunning()).toBe(true);
    
  } finally {
    await vscode.close();
  }
});
```

## 📊 Test Results

After running tests, check:
- **HTML Report**: `../Reports/electron-test-results/index.html`
- **Screenshots**: `../Reports/screenshots/`
- **JSON Results**: `../Reports/electron-test-results.json`

## 🔧 Common Commands

```bash
# Development
npm run build              # Build TypeScript
npm run test:debug         # Debug mode
npm run test:ui           # Interactive UI mode

# Specific tests
npx playwright test vscode-basic.test.ts
npx playwright test azure-ml-integration.test.ts

# Cleanup
npm run clean             # Clean build files
```

## 🆘 Troubleshooting

### VS Code Not Found
```bash
# Check your VS Code path in config/vscode-config.ts
# Default paths:
# macOS: /Applications/Visual Studio Code.app/Contents/MacOS/Electron
# Windows: C:\Program Files\Microsoft VS Code\Code.exe
# Linux: /usr/share/code/code
```

### Tests Failing
```bash
# Run in headed mode to see what's happening
npm run test:headed

# Check screenshots in Reports/screenshots/
# Enable debug logging
DEBUG=pw:api npm test
```

### Permission Issues
```bash
# macOS/Linux: Ensure VS Code executable permissions
chmod +x "/Applications/Visual Studio Code.app/Contents/MacOS/Electron"

# Windows: Run PowerShell as Administrator
```

## 🎓 Next Steps

1. **Explore Examples**: Check `tests/example-demo.test.ts`
2. **Read Documentation**: See `README.md` for detailed info
3. **Create Custom Tests**: Use the provided utilities
4. **Integrate with CI/CD**: Add to your pipeline

## 📚 Key Files

```
ElectronTests/
├── tests/
│   ├── example-demo.test.ts      # 👈 Start here!
│   ├── vscode-basic.test.ts      # Basic functionality
│   └── azure-ml-integration.test.ts # Azure ML specific
├── utils/
│   ├── vscode-electron.ts        # Main automation class
│   └── test-helpers.ts           # Utility functions
├── config/
│   └── vscode-config.ts          # VS Code configuration
└── README.md                     # Full documentation
```

## 🎉 Success!

You're now ready to automate VS Code with Playwright! 

The framework handles:
- ✅ Cross-platform VS Code launching
- ✅ File operations and editing
- ✅ Command palette automation
- ✅ Terminal integration
- ✅ Screenshot capture
- ✅ Error handling and cleanup

Happy testing! 🚀