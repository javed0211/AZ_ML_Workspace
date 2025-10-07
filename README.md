# Azure ML BDD Test Automation Framework

## 🎯 Overview
This is a clean, professional BDD (Behavior-Driven Development) test automation framework for Azure ML testing with both C# and TypeScript implementations.

## 🖥️ **Cross-Platform Support**
✅ **Works on Windows, macOS, and Linux!**
- **Windows 11**: Full Visual Studio 2022 support
- **macOS**: Native development environment
- **Linux**: Full CLI and container support

**New to Windows?** See [WINDOWS-QUICK-START.md](WINDOWS-QUICK-START.md) for 5-minute setup guide!

## 🏗️ Solution Structure
```
AZ_ML_Workspace/
├── AzureML-BDD-Framework.sln          # Main Visual Studio Solution
└── NewFramework/                      # Framework Implementation
    ├── src/
    │   ├── AzureML.BDD.CSharp/        # C# BDD Project
    │   └── AzureML.BDD.TypeScript/    # TypeScript BDD Project
    ├── Config/                        # Shared Configuration
    ├── Documentation/                 # Project Documentation
    └── Scripts/                       # PowerShell Scripts
```

## 🚀 Getting Started

### Prerequisites
- .NET 9.0 SDK
- Node.js 18+ and npm
- **For Visual Studio**: Visual Studio 2022 with Node.js development workload
- **For VS Code**: Any version (uses workspace file)
- Playwright browsers

> **Note**: The `.esproj` TypeScript project requires Visual Studio 2022 with the JavaScript/Node.js workload. VS Code users can use the workspace file for equivalent functionality.

### Open in Visual Studio 2022
1. **Requirements**: Visual Studio 2022 with Node.js development workload
2. Open `AzureML-BDD-Framework.sln` in Visual Studio 2022
3. You'll see two clean BDD projects with organized structure:
   - **AzureML.BDD.CSharp** - C# BDD tests (.csproj)
   - **AzureML.BDD.TypeScript** - TypeScript BDD tests (.esproj)
   - **Solution Items** - Shared configuration files
4. Both projects are fully buildable and debuggable in Visual Studio

### Open in VS Code
1. Open `AzureML-BDD-Framework.code-workspace` in VS Code
2. The workspace provides organized folder structure:
   - 🏠 **Root** - Main project files
   - 🔷 **C# BDD Tests** - C# project with IntelliSense
   - 📘 **TypeScript BDD Tests** - TypeScript project with IntelliSense
   - ⚙️ **Configuration** - Shared config files
   - 📚 **Documentation** - Project documentation
3. Use built-in tasks: `Ctrl+Shift+P` → "Tasks: Run Task"

### Build & Test

#### C# BDD Tests
```bash
# Build solution
dotnet build AzureML-BDD-Framework.sln

# Run C# BDD tests
dotnet test NewFramework/src/AzureML.BDD.CSharp/AzureML.BDD.CSharp.csproj
```

#### TypeScript BDD Tests
```bash
# Install dependencies
cd NewFramework/src/AzureML.BDD.TypeScript
npm install

# Run TypeScript BDD tests
npm run bdd
```

## 📋 Key Features
- ✅ **Pure BDD Architecture** - No mixed test types
- ✅ **Visual Studio Integration** - Clean solution structure
- ✅ **Dual Technology Support** - C# and TypeScript
- ✅ **Cross-Platform** - Windows, macOS, Linux
- ✅ **Container Support** - Docker-based testing
- ✅ **VS Code Desktop Testing** - Full GUI automation
- ✅ **Jupyter Notebook Testing** - Cross-platform notebook automation
- ✅ **Shared Resources** - Configuration and documentation
- ✅ **Independent Development** - Each project can be developed separately

## 📁 Project Details
- **C# Project**: `NewFramework/src/AzureML.BDD.CSharp/`
- **TypeScript Project**: `NewFramework/src/AzureML.BDD.TypeScript/`
- **Documentation**: `NewFramework/Documentation/`
- **Configuration**: `NewFramework/Config/`

## 📚 Documentation

### **Quick Start Guides**
- [WINDOWS-QUICK-START.md](WINDOWS-QUICK-START.md) - 5-minute Windows setup
- [QUICKSTART-Container.md](QUICKSTART-Container.md) - Container testing quick start

### **Platform-Specific Guides**
- [WINDOWS-MIGRATION-GUIDE.md](WINDOWS-MIGRATION-GUIDE.md) - Complete Windows migration guide
- [README-Container-Testing.md](README-Container-Testing.md) - Container testing guide
- [VSCODE-CONTAINER-TESTING.md](VSCODE-CONTAINER-TESTING.md) - VS Code container testing

### **Feature Guides**
- [NewFramework/ElectronTests/README-CrossPlatform.md](NewFramework/ElectronTests/README-CrossPlatform.md) - Cross-platform Jupyter testing
- [NewFramework/Documentation/](NewFramework/Documentation/) - Detailed feature documentation

### **Reference**
- [CHEATSHEET-Container.md](CHEATSHEET-Container.md) - Container commands cheat sheet

---
*This framework provides a clean, maintainable structure for BDD test automation across Windows, macOS, and Linux.*