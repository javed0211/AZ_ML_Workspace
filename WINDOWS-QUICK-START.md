# 🪟 Windows 11 Quick Start Guide

## ⚡ **TL;DR - 5 Minute Setup**

Your macOS framework **works perfectly on Windows 11**. Here's the fastest way to get started:

---

## 📦 **1. Install Prerequisites (5 minutes)**

```powershell
# Open PowerShell as Administrator and run:

# .NET 9.0 SDK
winget install Microsoft.DotNet.SDK.9

# Visual Studio 2022 Community (with Node.js workload)
winget install Microsoft.VisualStudio.2022.Community

# Node.js LTS
winget install OpenJS.NodeJS.LTS

# Git for Windows
winget install Git.Git

# VS Code
winget install Microsoft.VisualStudioCode

# Docker Desktop (optional - for container testing)
winget install Docker.DockerDesktop
```

**Manual Downloads (if winget fails):**
- .NET 9.0: https://dotnet.microsoft.com/download/dotnet/9.0
- Visual Studio 2022: https://visualstudio.microsoft.com/downloads/
- Node.js: https://nodejs.org/
- Git: https://git-scm.com/download/win
- VS Code: https://code.visualstudio.com/download
- Docker Desktop: https://www.docker.com/products/docker-desktop/

---

## 📥 **2. Clone Repository (1 minute)**

```powershell
# Open PowerShell (normal user, not admin)
cd C:\Users\YourName\Documents\GitHub

# Clone your repository
git clone <your-repo-url> AZ_ML_Workspace
cd AZ_ML_Workspace

# Configure Git line endings
git config core.autocrlf true
```

---

## 🏗️ **3. Open in Visual Studio 2022 (30 seconds)**

```powershell
# Option 1: Double-click
start AzureML-BDD-Framework.sln

# Option 2: Command line
devenv AzureML-BDD-Framework.sln
```

**Or in Visual Studio:**
- File → Open → Project/Solution
- Navigate to: `C:\Users\YourName\Documents\GitHub\AZ_ML_Workspace\AzureML-BDD-Framework.sln`

---

## 📦 **4. Restore Dependencies (2 minutes)**

### **In Visual Studio:**
```
Right-click Solution → Restore NuGet Packages
```

### **Or via PowerShell:**
```powershell
# Restore .NET packages
dotnet restore AzureML-BDD-Framework.sln

# Restore Node.js packages
cd NewFramework\ElectronTests
npm install

# Install Playwright browsers
npx playwright install
```

---

## 🔨 **5. Build Solution (1 minute)**

### **In Visual Studio:**
```
Build → Build Solution (Ctrl+Shift+B)
```

### **Or via PowerShell:**
```powershell
dotnet build AzureML-BDD-Framework.sln --configuration Release
```

---

## ✅ **6. Run Tests (30 seconds)**

### **In Visual Studio:**
```
1. Test → Test Explorer (Ctrl+E, T)
2. Click "Run All Tests"
```

### **Or via PowerShell:**

**C# BDD Tests:**
```powershell
# Run all tests
dotnet test

# Run specific category
dotnet test --filter "Category=Prerequisites"
dotnet test --filter "Category=VSCode"
dotnet test --filter "Category=Authentication"
```

**TypeScript/Playwright Tests:**
```powershell
cd NewFramework\ElectronTests

# Run all Playwright tests
npx playwright test

# Run Jupyter notebook tests
npx playwright test tests\cross-platform-jupyter.test.ts --headed
```

---

## 🎯 **Common Commands**

### **Build & Test**
```powershell
# Build
dotnet build

# Test all
dotnet test

# Test with filter
dotnet test --filter "Category=VSCode"

# Test with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### **Playwright Tests**
```powershell
cd NewFramework\ElectronTests

# List all tests
npx playwright test --list

# Run specific test
npx playwright test tests\cross-platform-jupyter.test.ts

# Run with UI
npx playwright test --headed

# Debug mode
npx playwright test --debug
```

### **Docker (Optional)**
```powershell
# Build image
docker build -t azure-ml-tests:latest .

# Run all tests
docker run --rm -v ${PWD}\TestResults:/workspace/TestResults azure-ml-tests:latest

# Run specific category
docker run --rm azure-ml-tests:latest dotnet test --filter "Category=Prerequisites"

# Using Docker Compose
docker-compose up bdd-tests
docker-compose up vscode-tests
```

---

## 🔧 **PowerShell Scripts (Already Included)**

Your framework already has Windows PowerShell scripts:

```powershell
# Run Azure ML automation tests
.\NewFramework\CSharpTests\run-automation-tests.ps1

# Run Electron/VS Code tests
.\NewFramework\Scripts\run-electron-tests.ps1

# Run Jupyter notebook tests
.\NewFramework\Scripts\run-vscode-notebook-tests.ps1

# Setup Electron tests
.\NewFramework\ElectronTests\setup.ps1

# Run Jupyter test (batch file)
.\NewFramework\ElectronTests\run-jupyter-test.bat
```

---

## 🐛 **Quick Troubleshooting**

### **PowerShell Execution Policy Error**
```powershell
# Fix: Set execution policy
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### **VS Code Not Found**
```powershell
# Fix: Add to PATH or set environment variable
$env:VSCODE_PATH = "C:\Users\YourName\AppData\Local\Programs\Microsoft VS Code\bin\code.cmd"

# Verify
code --version
```

### **Playwright Browsers Missing**
```powershell
cd NewFramework\ElectronTests
npx playwright install
```

### **Docker Volume Mount Error**
```powershell
# Fix: Use PowerShell syntax
docker run -v ${PWD}\TestResults:/workspace/TestResults azure-ml-tests:latest

# Or enable file sharing in Docker Desktop:
# Settings → Resources → File Sharing → Add your project folder
```

### **Line Ending Warnings**
```powershell
# Fix: Configure Git
git config core.autocrlf true
```

---

## 📊 **What Works on Windows**

| Component | Status | Notes |
|-----------|--------|-------|
| Visual Studio 2022 | ✅ **Perfect** | Opens `.sln` directly |
| C# Tests | ✅ **Perfect** | .NET 9.0 cross-platform |
| TypeScript Tests | ✅ **Perfect** | Node.js cross-platform |
| VS Code Testing | ✅ **Perfect** | Auto-detects Windows paths |
| Jupyter Testing | ✅ **Perfect** | Platform-aware |
| Docker Containers | ✅ **Works** | Via Docker Desktop + WSL2 |
| PowerShell Scripts | ✅ **Native** | Already included |
| Azure ML SDK | ✅ **Perfect** | Cloud-based |
| BDD/SpecFlow | ✅ **Perfect** | Cross-platform |

---

## 🎯 **Verification Checklist**

Run these commands to verify everything works:

```powershell
# 1. Check .NET
dotnet --version
# Expected: 9.0.x

# 2. Check Node.js
node --version
# Expected: v18.x or higher

# 3. Check VS Code
code --version
# Expected: 1.x.x

# 4. Build solution
dotnet build AzureML-BDD-Framework.sln
# Expected: Build succeeded

# 5. Run tests
dotnet test --filter "Category=Prerequisites"
# Expected: Tests run (pass or skip)

# 6. List Playwright tests
cd NewFramework\ElectronTests
npx playwright test --list
# Expected: Lists all tests

# 7. Check Docker (optional)
docker --version
# Expected: Docker version x.x.x
```

---

## 📚 **Documentation**

| Document | Purpose |
|----------|---------|
| **WINDOWS-MIGRATION-GUIDE.md** | Complete Windows migration guide |
| **WINDOWS-QUICK-START.md** | This quick start (you are here) |
| **README.md** | Framework overview |
| **README-Container-Testing.md** | Container testing guide |
| **VSCODE-CONTAINER-TESTING.md** | VS Code testing guide |
| **QUICKSTART-Container.md** | Container quick start |
| **CHEATSHEET-Container.md** | Container commands cheat sheet |

---

## 🎉 **You're Ready!**

Your framework is now running on Windows 11! 🚀

### **Next Steps:**
1. ✅ Open Visual Studio 2022
2. ✅ Build solution
3. ✅ Run tests
4. ✅ Start developing!

### **Need More Help?**
- Read: `WINDOWS-MIGRATION-GUIDE.md` (comprehensive guide)
- Check: `README.md` (framework overview)
- Review: Feature files in `NewFramework/CSharpTests/Features/`

---

**🎯 Everything works exactly the same as on macOS!**

No code changes needed - just different commands for the same functionality! 🎉