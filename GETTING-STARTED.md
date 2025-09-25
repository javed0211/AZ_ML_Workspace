# 🚀 Getting Started with Azure ML Test Framework

## 📁 Solution Structure

Your Azure ML Test Framework is now ready to use! Here's how to get started:

## 🎯 Opening in Visual Studio

### Option 1: Visual Studio (Recommended for C# Development)
1. **Open Visual Studio**
2. **File → Open → Project/Solution**
3. **Navigate to**: `/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/`
4. **Select**: `AzureMLTestFramework.sln`
5. **Click Open**

### Option 2: Visual Studio Code (Recommended for Full-Stack Development)
1. **Open Visual Studio Code**
2. **File → Open Workspace from File**
3. **Navigate to**: `/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/`
4. **Select**: `AzureMLTestFramework.code-workspace`
5. **Click Open**

### Option 3: Command Line (Quick Start)
```bash
# Navigate to the project
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace

# Open in Visual Studio Code
code AzureMLTestFramework.code-workspace

# Or open the solution in Visual Studio
open AzureMLTestFramework.sln
```

## 🏗️ Solution Organization

Your solution is organized into the following folders:

### 📂 **Tests**
- **PlaywrightFramework** - Main C# test project with:
  - ✅ **PIM Role Activation** - Automated Azure PIM role management
  - ✅ **Azure ML Workspace Tests** - Complete workspace automation
  - ✅ **BDD Step Definitions** - Reqnroll/Gherkin support
  - ✅ **Compute Instance Management** - Start/stop/verify compute instances
  - ✅ **VS Code Desktop Integration** - Desktop application testing

### 📂 **Features**
- **AzureMLWorkspace.feature** - BDD scenarios for Azure ML testing
- **AzureAISearch.feature** - Azure AI Search test scenarios

### 📂 **Configuration**
- **appsettings.json** - Test configuration and settings

### 📂 **Documentation**
- **AzureML-Automation-Guide.md** - Complete automation guide
- **PIM-Setup-Guide.md** - PIM configuration and troubleshooting

### 📂 **Scripts**
- **run-tests.sh** - Execute all tests
- **run-azure-ml-tests.sh** - Run Azure ML specific tests
- **Setup-PIMPermissions.ps1** - PowerShell script for PIM setup

## 🔧 Building and Running

### Build the Solution
```bash
# From the root directory
dotnet build NewFramework/CSharpTests/PlaywrightFramework.csproj
```

### Run Tests
```bash
# Run all C# tests
dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj

# Run specific test category
dotnet test --filter Category=AzureML

# Run with detailed logging
dotnet test --logger "console;verbosity=detailed"
```

### Run BDD Tests
```bash
# Navigate to NewFramework
cd NewFramework

# Run BDD scenarios
dotnet test --filter "TestCategory=BDD"
```

## 🎮 Visual Studio Features

### IntelliSense and Debugging
- ✅ Full IntelliSense support for all C# code
- ✅ Breakpoint debugging in tests and step definitions
- ✅ Real-time error detection and suggestions
- ✅ Integrated test runner with results

### Test Explorer
- ✅ View all tests in Test Explorer
- ✅ Run individual tests or test categories
- ✅ Debug specific tests with breakpoints
- ✅ View test results and output

### Solution Explorer Organization
```
📁 Tests
  └── 📁 PlaywrightFramework
      ├── 📁 Tests (NUnit test classes)
      ├── 📁 StepDefinitions (BDD step definitions)
      ├── 📁 Utils (Utility classes)
      └── 📁 Hooks (Test setup/teardown)

📁 Features
  ├── AzureMLWorkspace.feature
  └── AzureAISearch.feature

📁 Configuration
  └── appsettings.json

📁 Documentation
  ├── AzureML-Automation-Guide.md
  └── PIM-Setup-Guide.md

📁 Scripts
  ├── run-tests.sh
  ├── run-azure-ml-tests.sh
  └── Setup-PIMPermissions.ps1
```

## 🚀 Quick Test Run

### 1. Open the Solution
```bash
# Open in Visual Studio
open AzureMLTestFramework.sln

# Or open in VS Code
code AzureMLTestFramework.code-workspace
```

### 2. Build the Project
- **Visual Studio**: Build → Build Solution (Ctrl+Shift+B)
- **VS Code**: Terminal → Run Task → "Build C# Tests"

### 3. Run Your First Test
- **Visual Studio**: Test → Run All Tests
- **VS Code**: Terminal → Run Task → "Run C# Tests"

### 4. View Results
- Check the Test Explorer for results
- View detailed logs in the Output window
- Screenshots and reports are saved in `NewFramework/Reports/`

## 🔐 PIM Setup (Required for Azure ML Tests)

Before running Azure ML tests, ensure PIM is configured:

1. **Run PIM Setup Script**:
   ```powershell
   ./NewFramework/Scripts/Setup-PIMPermissions.ps1
   ```

2. **Configure Authentication**:
   - Update `NewFramework/Config/appsettings.json` with your credentials
   - See `NewFramework/Documentation/PIM-Setup-Guide.md` for details

## 📊 Test Categories

Your framework includes these test categories:

- **`AzureML`** - Azure ML workspace tests
- **`PIM`** - PIM role activation tests
- **`Compute`** - Compute instance management tests
- **`VSCode`** - VS Code Desktop integration tests
- **`BDD`** - Behavior-driven development scenarios

## 🎯 Next Steps

1. **Configure Authentication** - Update appsettings.json with your Azure credentials
2. **Run PIM Setup** - Execute the PowerShell script for PIM permissions
3. **Run Your First Test** - Try the "ShouldAccessAzureMLWorkspace" test
4. **Explore BDD Scenarios** - Check out the .feature files
5. **Customize Tests** - Modify tests for your specific Azure ML workspace

## 🆘 Need Help?

- **Documentation**: Check `NewFramework/Documentation/` folder
- **Configuration Issues**: See `PIM-Setup-Guide.md`
- **Test Failures**: Check logs in `NewFramework/Reports/logs/`
- **Build Issues**: Ensure .NET 8.0 SDK is installed

**Your framework is ready to go!** 🎉