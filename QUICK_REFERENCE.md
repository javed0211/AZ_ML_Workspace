# Quick Reference - Azure ML BDD Framework

## 🚀 Getting Started (30 seconds)

### Open in Visual Studio
```bash
# Double-click this file:
AzureML-BDD-Framework.sln
```

### Or use Command Line
```bash
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests
dotnet test --filter "Category=smoke"
```

---

## 📁 Key Files

| File | Purpose | Location |
|------|---------|----------|
| **Solution File** | Open in Visual Studio | `AzureML-BDD-Framework.sln` |
| **C# Project** | Main test project | `NewFramework/CSharpTests/PlaywrightFramework.csproj` |
| **Configuration** | Azure credentials | `NewFramework/Config/appsettings.json` |
| **BDD Features** | Test scenarios | `NewFramework/CSharpTests/Features/*.feature` |
| **Step Definitions** | C# implementations | `NewFramework/CSharpTests/StepDefinitions/*.cs` |
| **Helpers** | Azure SDK wrappers | `NewFramework/CSharpTests/Utils/*Helper.cs` |

---

## ⚡ Common Commands

### Build & Test
```bash
# Navigate to project
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests

# Build
dotnet build

# Run all tests
dotnet test

# Run smoke tests only
dotnet test --filter "Category=smoke"

# Run integration tests
dotnet test --filter "Category=integration"

# Run Azure AI Services tests
dotnet test --filter "Category=AzureAI"

# Run specific feature
dotnet test --filter "FullyQualifiedName~AzureAISearchIntegration"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Package Management
```bash
# Restore packages
dotnet restore

# List packages
dotnet list package

# Check for updates
dotnet list package --outdated

# Add new package
dotnet add package PackageName --version X.Y.Z
```

### Clean & Rebuild
```bash
# Clean
dotnet clean

# Rebuild
dotnet clean && dotnet build
```

---

## 🧪 Test Categories

| Category | Description | Command |
|----------|-------------|---------|
| `@smoke` | Quick validation tests | `dotnet test --filter "Category=smoke"` |
| `@integration` | End-to-end integration tests | `dotnet test --filter "Category=integration"` |
| `@performance` | Performance benchmarks | `dotnet test --filter "Category=performance"` |
| `@AzureAI` | Azure AI Services tests | `dotnet test --filter "Category=AzureAI"` |

---

## 📊 Test Features Available

### Azure AI Services (35 scenarios) ✅
- **AzureAISearchIntegration.feature** (11 scenarios)
  - Index management, search, semantic search, autocomplete
- **AzureDocumentIntelligence.feature** (13 scenarios)
  - Document analysis, custom models, batch processing
- **AzureAIServicesIntegration.feature** (11 scenarios)
  - End-to-end pipelines combining both services

### Azure ML (Legacy)
- **AzureMLWorkspace.feature**
- **AzureMLComputeAutomation.feature**
- **AzureAISearch.feature**

---

## 🔧 Configuration

### Required Settings in `appsettings.json`

```json
{
  "Azure": {
    "Search": {
      "Endpoint": "https://YOUR-SERVICE.search.windows.net",
      "ApiKey": "YOUR-API-KEY"
    },
    "DocumentIntelligence": {
      "Endpoint": "https://YOUR-SERVICE.cognitiveservices.azure.com/",
      "ApiKey": "YOUR-API-KEY"
    }
  }
}
```

**File Location:** `NewFramework/Config/appsettings.json`

---

## 🐛 Debugging

### In Visual Studio
1. Set breakpoint in step definition (F9)
2. Right-click test in Test Explorer
3. Select "Debug Selected Tests"

### In VS Code
1. Install C# extension
2. Set breakpoint
3. F5 to debug

### Command Line
```bash
# Run with verbose logging
dotnet test --logger "console;verbosity=detailed"

# Run single test
dotnet test --filter "FullyQualifiedName~YourTestName"
```

---

## 📦 NuGet Packages Used

| Package | Version | Purpose |
|---------|---------|---------|
| **Reqnroll** | 2.0.3 | BDD framework |
| **Reqnroll.NUnit** | 2.0.3 | NUnit integration |
| **Azure.Search.Documents** | 11.5.1 | Azure AI Search |
| **Azure.AI.FormRecognizer** | 4.1.0 | Document Intelligence |
| **Microsoft.Playwright** | 1.40.0 | Browser automation |
| **Serilog** | 3.1.1 | Logging |
| **FluentAssertions** | 6.12.0 | Test assertions |

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| **VISUAL_STUDIO_GUIDE.md** | Complete Visual Studio setup guide |
| **README_AZURE_AI_SERVICES_TESTS.md** | Azure AI Services test documentation |
| **QUICKSTART_AZURE_AI_TESTS.md** | Quick start for Azure AI tests |
| **AZURE_AI_SERVICES_COMPLETION_STATUS.md** | Project completion status |
| **AZURE_AI_SERVICES_TEST_SUMMARY.md** | Detailed test summary |

---

## 🎯 Quick Workflows

### Run Your First Test
```bash
# 1. Configure credentials
nano NewFramework/Config/appsettings.json

# 2. Build
cd NewFramework/CSharpTests
dotnet build

# 3. Run smoke tests
dotnet test --filter "Category=smoke"
```

### Add New Test
```bash
# 1. Create feature file
touch Features/MyNewFeature.feature

# 2. Write scenario in Gherkin
# 3. Generate step definitions
# 4. Implement steps in C#
# 5. Run test
dotnet test --filter "FullyQualifiedName~MyNewFeature"
```

### Debug Failing Test
```bash
# 1. Run with verbose output
dotnet test --filter "FullyQualifiedName~FailingTest" --logger "console;verbosity=detailed"

# 2. Check logs
cat bin/Debug/net9.0/logs/*.log

# 3. Set breakpoint and debug in Visual Studio
```

---

## 🔍 Troubleshooting

### Tests Not Showing in Test Explorer?
```bash
dotnet clean
dotnet build
# Restart Visual Studio
```

### Build Errors?
```bash
# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean && dotnet build
```

### Azure Credentials Not Working?
1. Check `appsettings.json` syntax
2. Verify file is copied to output: `bin/Debug/net9.0/appsettings.json`
3. Test credentials with Azure Portal

### Reqnroll Extension Not Working?
1. Install from: Extensions → Manage Extensions
2. Search: "Reqnroll for Visual Studio 2022"
3. Restart Visual Studio

---

## 💡 Pro Tips

### Tip 1: Run Tests in Parallel
```bash
dotnet test --parallel
```

### Tip 2: Generate Beautiful HTML Report with Allure
```bash
# Install Allure (one-time)
brew install allure  # macOS
# or scoop install allure  # Windows

# Generate and view report
cd NewFramework/CSharpTests
./generate-allure-report.sh
```

### Tip 3: Watch Mode (Auto-run on changes)
```bash
dotnet watch test
```

### Tip 4: Filter by Multiple Categories
```bash
dotnet test --filter "Category=smoke|Category=integration"
```

### Tip 5: Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🆘 Need Help?

### Check Documentation
1. `VISUAL_STUDIO_GUIDE.md` - Complete setup guide
2. `README_AZURE_AI_SERVICES_TESTS.md` - Test documentation
3. Feature files - See example scenarios

### Common Issues
- **Build errors in old code**: Run only Azure AI tests with `--filter "Category=AzureAI"`
- **Missing packages**: Run `dotnet restore`
- **Tests not found**: Run `dotnet clean && dotnet build`

### External Resources
- Reqnroll Docs: https://docs.reqnroll.net/
- Azure SDK Docs: https://learn.microsoft.com/en-us/dotnet/azure/

---

## ✅ Checklist

Before running tests:
- [ ] Opened `AzureML-BDD-Framework.sln` in Visual Studio
- [ ] Installed Reqnroll extension
- [ ] Restored NuGet packages (`dotnet restore`)
- [ ] Configured Azure credentials in `appsettings.json`
- [ ] Built solution successfully (`dotnet build`)
- [ ] Tests appear in Test Explorer

---

**Ready to test! 🚀**

Quick start: `dotnet test --filter "Category=smoke"`