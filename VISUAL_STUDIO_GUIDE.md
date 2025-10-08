# Visual Studio Guide - Azure ML BDD Framework

## 📋 Overview

This guide explains how to use the Azure ML BDD Framework in **Visual Studio** (Windows/Mac) or **Visual Studio Code** (cross-platform).

---

## 🎯 Quick Start

### Option 1: Visual Studio (Recommended for C# Development)

#### Opening the Solution

1. **Double-click** the solution file:
   ```
   AzureML-BDD-Framework.sln
   ```

2. **Or** open Visual Studio and:
   - File → Open → Project/Solution
   - Navigate to: `/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/`
   - Select: `AzureML-BDD-Framework.sln`

#### What You'll See

The solution contains:

```
AzureML-BDD-Framework (Solution)
├── PlaywrightFramework (C# Test Project)
│   ├── Features/                    # BDD Feature files (Gherkin)
│   │   ├── AzureAISearchIntegration.feature
│   │   ├── AzureDocumentIntelligence.feature
│   │   ├── AzureAIServicesIntegration.feature
│   │   ├── AzureMLWorkspace.feature
│   │   └── AzureMLComputeAutomation.feature
│   ├── StepDefinitions/             # C# Step implementations
│   │   ├── AzureAISearchIntegrationSteps.cs
│   │   ├── AzureDocumentIntelligenceSteps.cs
│   │   ├── AzureAIServicesIntegrationSteps.cs
│   │   └── ...
│   ├── Utils/                       # Helper classes
│   │   ├── AzureAISearchHelper.cs
│   │   ├── AzureDocumentIntelligenceHelper.cs
│   │   └── ...
│   └── Tests/                       # Traditional unit tests
├── Solution Items/                  # Documentation
├── Configuration/                   # Config files
├── Documentation/                   # Guides
├── Scripts/                         # PowerShell scripts
└── TestData/                        # Test data files
```

---

## 🔧 Setting Up in Visual Studio

### 1. Install Required Extensions

#### For Visual Studio 2022 (Windows/Mac):

**Reqnroll Extension** (BDD Support):
- Go to: **Extensions → Manage Extensions**
- Search for: **"Reqnroll for Visual Studio 2022"**
- Install and restart Visual Studio

**Or download from:**
- https://marketplace.visualstudio.com/items?itemName=Reqnroll.ReqnrollForVisualStudio2022

#### Features You Get:
- ✅ Syntax highlighting for `.feature` files
- ✅ IntelliSense for Gherkin keywords
- ✅ Navigation from steps to step definitions
- ✅ Code generation for missing steps
- ✅ Test Explorer integration

### 2. Restore NuGet Packages

Visual Studio should automatically restore packages. If not:

**Method 1: Visual Studio UI**
- Right-click on Solution → **Restore NuGet Packages**

**Method 2: Package Manager Console**
```powershell
# Tools → NuGet Package Manager → Package Manager Console
Update-Package -reinstall
```

**Method 3: Command Line**
```bash
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests
dotnet restore
```

### 3. Configure Azure Credentials

Edit: `NewFramework/Config/appsettings.json`

```json
{
  "Azure": {
    "SubscriptionId": "your-subscription-id",
    "TenantId": "your-tenant-id",
    "ResourceGroup": "your-resource-group",
    "WorkspaceName": "your-workspace-name",
    "Search": {
      "Endpoint": "https://your-search-service.search.windows.net",
      "ApiKey": "your-search-api-key"
    },
    "DocumentIntelligence": {
      "Endpoint": "https://your-doc-intel.cognitiveservices.azure.com/",
      "ApiKey": "your-doc-intel-api-key"
    }
  }
}
```

### 4. Build the Solution

**Method 1: Visual Studio UI**
- Build → Build Solution (Ctrl+Shift+B / Cmd+Shift+B)

**Method 2: Command Line**
```bash
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests
dotnet build
```

---

## 🧪 Running Tests in Visual Studio

### Using Test Explorer

1. **Open Test Explorer**:
   - View → Test Explorer (Ctrl+E, T / Cmd+E, T)

2. **Discover Tests**:
   - Visual Studio will automatically discover all Reqnroll scenarios
   - Each scenario appears as a separate test

3. **Run Tests**:
   - **Run All**: Click "Run All" button
   - **Run Specific Test**: Right-click test → Run
   - **Run by Category**: Group by Traits/Categories

### Using Test Explorer Filters

```
# Filter by category
Category=smoke
Category=integration
Category=AzureAI

# Filter by feature
FullyQualifiedName~AzureAISearchIntegration
FullyQualifiedName~AzureDocumentIntelligence

# Filter by scenario name
FullyQualifiedName~"Create search index"
```

### Using Command Line (Terminal in VS)

```bash
# Navigate to test project
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests

# Run all tests
dotnet test

# Run specific category
dotnet test --filter "Category=smoke"

# Run specific feature
dotnet test --filter "FullyQualifiedName~AzureAISearchIntegration"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run and generate report
dotnet test --logger "trx;LogFileName=test-results.trx"
```

---

## 🎨 Working with BDD Features

### Creating New Feature Files

1. **Right-click** on `Features` folder
2. **Add → New Item**
3. Search for **"Reqnroll Feature File"** (if extension installed)
4. **Or** manually create `.feature` file

Example:
```gherkin
Feature: My New Feature
  As a user
  I want to test something
  So that I can verify it works

  @smoke
  Scenario: My first test
    Given I have a precondition
    When I perform an action
    Then I should see a result
```

### Generating Step Definitions

**With Reqnroll Extension:**
1. Right-click on undefined step (purple text)
2. Select **"Generate Step Definition"**
3. Choose target class or create new one

**Manual Creation:**
```csharp
[Given(@"I have a precondition")]
public void GivenIHaveAPrecondition()
{
    // Implementation
}

[When(@"I perform an action")]
public void WhenIPerformAnAction()
{
    // Implementation
}

[Then(@"I should see a result")]
public void ThenIShouldSeeAResult()
{
    // Implementation
}
```

### Navigating Between Features and Steps

**With Reqnroll Extension:**
- **Ctrl+Click** (Cmd+Click on Mac) on a step in `.feature` file
- Jumps directly to step definition in `.cs` file

**Find All References:**
- Right-click on step definition → **Find All References**
- Shows all feature files using that step

---

## 🐛 Debugging Tests

### Debug Single Test

1. **Set Breakpoint** in step definition (F9)
2. **Right-click** test in Test Explorer
3. **Debug Selected Tests**

### Debug from Feature File

**With Reqnroll Extension:**
1. Open `.feature` file
2. Click in margin next to scenario (sets breakpoint)
3. Right-click scenario → **Debug**

### Debug Configuration

Add to `launch.json` (if using VS Code):
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Test",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "dotnet",
      "args": [
        "test",
        "${workspaceFolder}/NewFramework/CSharpTests/PlaywrightFramework.csproj",
        "--filter",
        "FullyQualifiedName~YourTestName"
      ],
      "cwd": "${workspaceFolder}/NewFramework/CSharpTests",
      "console": "internalConsole",
      "stopAtEntry": false
    }
  ]
}
```

---

## 📊 Viewing Test Results

### In Visual Studio

**Test Explorer Results:**
- ✅ Green checkmark = Passed
- ❌ Red X = Failed
- ⚠️ Yellow triangle = Skipped
- Click test to see output/error details

**Output Window:**
- View → Output
- Select "Tests" from dropdown
- Shows detailed test execution logs

### Test Reports

**Generate HTML Report:**
```bash
# Install LivingDoc tool
dotnet tool install --global Reqnroll.Tools.LivingDoc.CLI

# Generate report
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests
livingdoc test-assembly bin/Debug/net9.0/PlaywrightFramework.dll -t bin/Debug/net9.0/TestExecution.json
```

**TRX Report (Visual Studio format):**
```bash
dotnet test --logger "trx;LogFileName=test-results.trx"
# Opens in Visual Studio: Test → Analyze Test Run → Import Test Results
```

---

## 🔍 Solution Structure Explained

### PlaywrightFramework.csproj

This is your main C# test project containing:

**Key Folders:**
- **Features/** - BDD scenarios in Gherkin syntax
- **StepDefinitions/** - C# implementations of Gherkin steps
- **Utils/** - Helper classes and utilities
- **Tests/** - Traditional NUnit tests (non-BDD)

**Key Files:**
- **PlaywrightFramework.csproj** - Project configuration
- **reqnroll.json** - Reqnroll BDD configuration
- **appsettings.json** - Test configuration (copied from Config/)

### Configuration Files

**appsettings.json** (in Config folder):
- Shared configuration for all tests
- Azure credentials and endpoints
- Environment-specific settings

**reqnroll.json**:
- Reqnroll framework configuration
- Step definition scanning
- Report generation settings

---

## 🚀 Common Tasks

### Task 1: Add New Azure AI Service Test

1. **Create Feature File**:
   ```
   Features/MyNewService.feature
   ```

2. **Write Scenarios**:
   ```gherkin
   Feature: My New Azure Service
     @smoke
     Scenario: Test basic functionality
       Given I have configured the service
       When I call the API
       Then I should get a response
   ```

3. **Generate Steps**:
   - Right-click undefined steps → Generate Step Definition
   - Or create manually in `StepDefinitions/MyNewServiceSteps.cs`

4. **Create Helper** (optional):
   ```
   Utils/MyNewServiceHelper.cs
   ```

5. **Register in .csproj**:
   ```xml
   <Reqnroll Include="Features/MyNewService.feature">
     <Generator>ReqnrollSingleFileGenerator</Generator>
     <LastGenOutput>MyNewService.feature.cs</LastGenOutput>
   </Reqnroll>
   ```

6. **Build and Run**:
   ```bash
   dotnet build
   dotnet test --filter "FullyQualifiedName~MyNewService"
   ```

### Task 2: Run Tests in CI/CD

**GitHub Actions Example:**
```yaml
name: Run BDD Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'
      
      - name: Restore dependencies
        run: dotnet restore NewFramework/CSharpTests/PlaywrightFramework.csproj
      
      - name: Build
        run: dotnet build NewFramework/CSharpTests/PlaywrightFramework.csproj --no-restore
      
      - name: Run smoke tests
        run: dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj --filter "Category=smoke" --logger "trx"
      
      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results
          path: '**/TestResults/*.trx'
```

### Task 3: Update Azure SDK Packages

```bash
# Check for updates
dotnet list package --outdated

# Update specific package
dotnet add package Azure.Search.Documents --version 11.6.0

# Update all packages
dotnet restore
```

---

## 🛠️ Troubleshooting

### Issue: Tests Not Appearing in Test Explorer

**Solution:**
1. Clean and rebuild solution
2. Close and reopen Test Explorer
3. Ensure Reqnroll extension is installed
4. Check `.feature.cs` files are generated in `Features/` folder

```bash
# Force regeneration
dotnet clean
dotnet build
```

### Issue: Step Definitions Not Found

**Solution:**
1. Check namespace matches in step definition files
2. Ensure `[Binding]` attribute is present on step definition class
3. Verify step regex patterns match exactly

```csharp
[Binding]  // ← Make sure this is present
public class MySteps
{
    [Given(@"I have a precondition")]  // ← Check regex pattern
    public void GivenIHaveAPrecondition()
    {
    }
}
```

### Issue: Azure Credentials Not Loading

**Solution:**
1. Verify `appsettings.json` is copied to output directory
2. Check `.csproj` has:
   ```xml
   <None Update="../Config/appsettings.json">
     <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
   </None>
   ```
3. Ensure ConfigManager is reading correct path

### Issue: Build Errors in Existing Code

**Current Known Issues:**
- `AzureMLComputeAutomationTests.cs` - Logger conflicts (existing issue)
- `AzureMLComputeAutomationUtils.cs` - Azure SDK compatibility (existing issue)

**These do NOT affect new Azure AI Services tests.**

**Workaround:**
```bash
# Run only Azure AI Services tests (which compile successfully)
dotnet test --filter "Category=AzureAI"
```

---

## 📚 Additional Resources

### Reqnroll Documentation
- Official Docs: https://docs.reqnroll.net/
- GitHub: https://github.com/reqnroll/Reqnroll
- Migration from SpecFlow: https://docs.reqnroll.net/latest/guides/migrating-from-specflow.html

### Azure SDK Documentation
- Azure.Search.Documents: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/search.documents-readme
- Azure.AI.FormRecognizer: https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.formrecognizer-readme

### Project Documentation
- Azure AI Services Tests: `NewFramework/CSharpTests/README_AZURE_AI_SERVICES_TESTS.md`
- Quick Start Guide: `NewFramework/CSharpTests/QUICKSTART_AZURE_AI_TESTS.md`
- Completion Status: `NewFramework/CSharpTests/AZURE_AI_SERVICES_COMPLETION_STATUS.md`

---

## 🎯 Summary

### To Use This Repo in Visual Studio:

1. ✅ **Open** `AzureML-BDD-Framework.sln`
2. ✅ **Install** Reqnroll extension
3. ✅ **Restore** NuGet packages
4. ✅ **Configure** Azure credentials in `appsettings.json`
5. ✅ **Build** solution
6. ✅ **Run** tests from Test Explorer

### Key Files:
- **Solution**: `AzureML-BDD-Framework.sln` ← Open this in Visual Studio
- **Project**: `NewFramework/CSharpTests/PlaywrightFramework.csproj`
- **Config**: `NewFramework/Config/appsettings.json`
- **Features**: `NewFramework/CSharpTests/Features/*.feature`

### Quick Commands:
```bash
# Navigate to project
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests

# Build
dotnet build

# Run all tests
dotnet test

# Run smoke tests
dotnet test --filter "Category=smoke"

# Run Azure AI tests
dotnet test --filter "Category=AzureAI"
```

---

**You're all set! Open the solution in Visual Studio and start testing! 🚀**