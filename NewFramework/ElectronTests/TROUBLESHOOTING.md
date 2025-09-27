# VS Code Electron Testing - Troubleshooting Guide

## 🚨 Common Issues and Solutions

### Issue: `Error: spawn /Applications/Visual Studio Code.app/Contents/MacOS/Electron ENOENT`

This error occurs when VS Code is not installed or not found at the expected location.

#### **Root Cause**
- VS Code is not installed on the system
- VS Code is installed but at a different location
- The executable path in the configuration is incorrect

#### **Solutions**

### 🛠️ **Solution 1: Install VS Code (Recommended)**

#### **Option A: Using Homebrew (Fastest)**
```bash
# Install VS Code via Homebrew
brew install --cask visual-studio-code

# Verify installation
ls "/Applications/Visual Studio Code.app/Contents/MacOS/"
```

#### **Option B: Direct Download**
```bash
# Run our installation script
cd NewFramework/ElectronTests
./install-vscode.sh
```

#### **Option C: Manual Installation**
1. Download VS Code from: https://code.visualstudio.com/
2. Install the `.dmg` file
3. Move VS Code to `/Applications/`

### 🔍 **Solution 2: Validate Installation**

Use our validation script to check if VS Code is properly installed:

```bash
cd NewFramework/ElectronTests
node validate-vscode.js
```

This will:
- ✅ Check if VS Code is installed
- 📍 Show the detected executable path
- 📋 Display version information
- 🚀 Confirm readiness for testing

### 🔧 **Solution 3: Manual Path Configuration**

If VS Code is installed in a non-standard location, you can override the path:

```typescript
// In your test file
import { VSCodeElectron } from '../utils/vscode-electron';

const vscode = new VSCodeElectron({
  executablePath: '/path/to/your/vscode/executable'
});
```

### 📋 **Expected VS Code Locations**

#### **macOS**
- `/Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code` (newer versions)
- `/Applications/Visual Studio Code.app/Contents/MacOS/Electron` (older versions)
- `/Applications/Visual Studio Code - Insiders.app/Contents/MacOS/Visual Studio Code - Insiders`

#### **Windows**
- `C:\Program Files\Microsoft VS Code\Code.exe`
- `C:\Program Files (x86)\Microsoft VS Code\Code.exe`
- `%USERPROFILE%\AppData\Local\Programs\Microsoft VS Code\Code.exe`

#### **Linux**
- `/usr/share/code/code`
- `/usr/bin/code`
- `/snap/code/current/usr/share/code/code`

## 🔍 **Diagnostic Commands**

### Check if VS Code is installed:
```bash
# macOS
ls "/Applications/Visual Studio Code.app/Contents/MacOS/"

# Windows
dir "C:\Program Files\Microsoft VS Code\"

# Linux
which code
```

### Get VS Code version:
```bash
# macOS
"/Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code" --version

# Windows
"C:\Program Files\Microsoft VS Code\Code.exe" --version

# Linux
code --version
```

### Check running processes:
```bash
# Check if Homebrew installation is in progress
ps aux | grep "visual-studio-code"

# Check if VS Code is running
ps aux | grep "Visual Studio Code"
```

## 🚀 **Quick Fix Commands**

### **For macOS:**
```bash
# 1. Install VS Code
brew install --cask visual-studio-code

# 2. Validate installation
cd NewFramework/ElectronTests
node validate-vscode.js

# 3. Run tests
npm test
```

### **For Windows:**
```powershell
# 1. Install VS Code
winget install Microsoft.VisualStudioCode

# 2. Validate installation
cd NewFramework\ElectronTests
node validate-vscode.js

# 3. Run tests
npm test
```

### **For Linux:**
```bash
# 1. Install VS Code (Ubuntu/Debian)
sudo apt update
sudo apt install code

# 2. Validate installation
cd NewFramework/ElectronTests
node validate-vscode.js

# 3. Run tests
npm test
```

## 🔧 **Advanced Configuration**

### Custom VS Code Configuration:
```typescript
import { VSCodeElectron } from '../utils/vscode-electron';

const vscode = new VSCodeElectron({
  executablePath: '/custom/path/to/vscode',
  userDataDir: '/custom/user/data/dir',
  extensionsDir: '/custom/extensions/dir',
  workspaceDir: '/custom/workspace/dir'
});
```

### Launch with Custom Arguments:
```typescript
await vscode.launch('/path/to/workspace', [
  '--disable-extensions',
  '--new-window',
  '--wait'
]);
```

## 📞 **Getting Help**

If you're still experiencing issues:

1. **Run the validation script**: `node validate-vscode.js`
2. **Check the console output** for detailed error messages
3. **Verify VS Code works manually** by launching it from the command line
4. **Check file permissions** on the VS Code executable
5. **Try VS Code Insiders** if the stable version doesn't work

## 🎯 **Success Indicators**

When everything is working correctly, you should see:

```
✅ VS Code validation passed: VS Code found at: /Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code
🚀 Launching VS Code with executable: /Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code
📋 Launch arguments: --no-sandbox --disable-dev-shm-usage --disable-extensions...
✅ VS Code launched successfully
```

## 🔄 **Automated Setup**

For a completely automated setup, run:

```bash
cd NewFramework/ElectronTests
./setup.sh
```

This will:
1. Install dependencies
2. Install VS Code (if needed)
3. Validate the installation
4. Run a test to verify everything works