# Azure ML BDD Test Automation Framework

## 🎯 Overview
This is a clean, professional BDD (Behavior-Driven Development) test automation framework for Azure ML testing with both C# and TypeScript implementations.

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

### Open in Visual Studio
1. Open `AzureML-BDD-Framework.sln` in Visual Studio
2. You'll see two clean BDD projects with organized structure
3. All shared resources are properly linked

### Build & Test
```bash
# Build entire solution
dotnet build

# Run TypeScript BDD tests (dry-run)
cd NewFramework && npm run bdd:dry-run

# Run C# BDD tests
cd NewFramework && npm run test:csharp
```

## 📋 Key Features
- ✅ **Pure BDD Architecture** - No mixed test types
- ✅ **Visual Studio Integration** - Clean solution structure
- ✅ **Dual Technology Support** - C# and TypeScript
- ✅ **Shared Resources** - Configuration and documentation
- ✅ **Independent Development** - Each project can be developed separately

## 📁 Project Details
- **C# Project**: `NewFramework/src/AzureML.BDD.CSharp/`
- **TypeScript Project**: `NewFramework/src/AzureML.BDD.TypeScript/`
- **Documentation**: `NewFramework/Documentation/`
- **Configuration**: `NewFramework/Config/`

---
*This framework provides a clean, maintainable structure for BDD test automation in Visual Studio.*