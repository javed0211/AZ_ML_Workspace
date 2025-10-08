# Azure ML BDD Test Framework

> **Comprehensive C# BDD test framework for Azure Machine Learning and Azure AI Services**

[![.NET](https://img.shields.io/badge/.NET-9.0-blue)](https://dotnet.microsoft.com/)
[![Reqnroll](https://img.shields.io/badge/Reqnroll-2.0.3-green)](https://reqnroll.net/)
[![Azure](https://img.shields.io/badge/Azure-AI%20Services-0078D4)](https://azure.microsoft.com/)

---

## 🚀 Quick Start

### Open in Visual Studio
```bash
# Double-click to open:
AzureML-BDD-Framework.sln
```

### Run Tests from Command Line
```bash
cd NewFramework/CSharpTests
dotnet test --filter "Category=smoke"
```

**📖 Full Guide:** See [VISUAL_STUDIO_GUIDE.md](VISUAL_STUDIO_GUIDE.md)

---

## 📋 What's Inside

### ✅ 35 BDD Test Scenarios (C# + Reqnroll)

| Feature | Scenarios | Status |
|---------|-----------|--------|
| **Azure AI Search Integration** | 11 | ✅ Ready |
| **Azure Document Intelligence** | 13 | ✅ Ready |
| **Azure AI Services Integration** | 11 | ✅ Ready |

### 🎯 Test Coverage

**Azure AI Search:**
- Index management (create, update, delete)
- Document indexing and search
- Filtered search with OData
- Faceted navigation
- Semantic search with AI ranking
- Autocomplete suggestions
- Performance testing

**Document Intelligence:**
- Prebuilt models (invoice, receipt, ID, business card)
- Layout and table extraction
- Custom model training
- Model composition
- Batch processing
- Data validation
- Error handling

**Integration Scenarios:**
- End-to-end document-to-search pipelines
- Batch processing workflows
- Semantic search on extracted data
- High-volume processing
- Pipeline health monitoring

---

## 📁 Repository Structure

```
AZ_ML_Workspace/
├── AzureML-BDD-Framework.sln          # ← Open this in Visual Studio
├── VISUAL_STUDIO_GUIDE.md             # Complete setup guide
├── QUICK_REFERENCE.md                 # Quick command reference
├── README.md                          # This file
│
└── NewFramework/
    ├── Config/
    │   └── appsettings.json           # ← Configure Azure credentials here
    │
    └── CSharpTests/                   # Main C# test project
        ├── PlaywrightFramework.csproj # Project file
        ├── reqnroll.json              # BDD configuration
        │
        ├── Features/                  # BDD scenarios (Gherkin)
        │   ├── AzureAISearchIntegration.feature
        │   ├── AzureDocumentIntelligence.feature
        │   ├── AzureAIServicesIntegration.feature
        │   └── ...
        │
        ├── StepDefinitions/           # C# step implementations
        │   ├── AzureAISearchIntegrationSteps.cs
        │   ├── AzureDocumentIntelligenceSteps.cs
        │   ├── AzureAIServicesIntegrationSteps.cs
        │   └── ...
        │
        ├── Utils/                     # Helper classes
        │   ├── AzureAISearchHelper.cs
        │   ├── AzureDocumentIntelligenceHelper.cs
        │   └── ...
        │
        └── Documentation/
            ├── README_AZURE_AI_SERVICES_TESTS.md
            ├── QUICKSTART_AZURE_AI_TESTS.md
            ├── AZURE_AI_SERVICES_TEST_SUMMARY.md
            └── AZURE_AI_SERVICES_COMPLETION_STATUS.md
```

---

## 🔧 Prerequisites

### Required Software
- **.NET 9.0 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Visual Studio 2022** (Windows/Mac) - [Download](https://visualstudio.microsoft.com/)
  - Or **Visual Studio Code** with C# extension

### Required Visual Studio Extension
- **Reqnroll for Visual Studio 2022** - [Install](https://marketplace.visualstudio.com/items?itemName=Reqnroll.ReqnrollForVisualStudio2022)

### Azure Resources
- Azure AI Search service
- Azure Document Intelligence service
- Valid API keys and endpoints

---

## ⚙️ Setup

### 1. Clone Repository
```bash
git clone <your-repo-url>
cd AZ_ML_Workspace
```

### 2. Configure Azure Credentials

Edit `NewFramework/Config/appsettings.json`:

```json
{
  "Azure": {
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

### 3. Restore Packages
```bash
cd NewFramework/CSharpTests
dotnet restore
```

### 4. Build
```bash
dotnet build
```

### 5. Run Tests
```bash
# Run smoke tests
dotnet test --filter "Category=smoke"

# Run all tests
dotnet test
```

---

## 🧪 Running Tests

### In Visual Studio

1. **Open Solution**: `AzureML-BDD-Framework.sln`
2. **Open Test Explorer**: View → Test Explorer
3. **Run Tests**: Click "Run All" or right-click specific tests

### From Command Line

```bash
# Navigate to test project
cd NewFramework/CSharpTests

# Run all tests
dotnet test

# Run by category
dotnet test --filter "Category=smoke"
dotnet test --filter "Category=integration"
dotnet test --filter "Category=AzureAI"

# Run specific feature
dotnet test --filter "FullyQualifiedName~AzureAISearchIntegration"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Test Categories

| Tag | Description | Command |
|-----|-------------|---------|
| `@smoke` | Quick validation tests | `--filter "Category=smoke"` |
| `@integration` | End-to-end tests | `--filter "Category=integration"` |
| `@performance` | Performance benchmarks | `--filter "Category=performance"` |
| `@AzureAI` | Azure AI Services tests | `--filter "Category=AzureAI"` |

---

## 📊 Beautiful HTML Reports

Generate stunning Allure reports with charts, graphs, and screenshots!

### Quick Start
```bash
# Install Allure (one-time)
brew install allure  # macOS

# Run tests and generate report
cd NewFramework/CSharpTests
./generate-allure-report.sh

# Report opens automatically in browser! 🎉
```

### Features
- 🎨 Modern, interactive UI with dark/light themes
- 📊 Charts and graphs (pass rate, duration, trends)
- 📸 Screenshots automatically attached on failures
- 🏷️ Test categorization and filtering
- 📈 Historical trends and statistics
- 🔍 Detailed step-by-step execution timeline

**📖 Full Guide:** See [ALLURE_QUICK_START.md](ALLURE_QUICK_START.md) and [ALLURE_REPORTS_GUIDE.md](ALLURE_REPORTS_GUIDE.md)

---

## 📖 Documentation

| Document | Description |
|----------|-------------|
| **[VISUAL_STUDIO_GUIDE.md](VISUAL_STUDIO_GUIDE.md)** | Complete Visual Studio setup and usage guide |
| **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** | Quick command reference card |
| **[ALLURE_QUICK_START.md](ALLURE_QUICK_START.md)** | 📊 Beautiful HTML reports - Quick start |
| **[ALLURE_REPORTS_GUIDE.md](ALLURE_REPORTS_GUIDE.md)** | 📊 Complete Allure reporting guide |
| **[README_AZURE_AI_SERVICES_TESTS.md](NewFramework/CSharpTests/README_AZURE_AI_SERVICES_TESTS.md)** | Azure AI Services test documentation |
| **[QUICKSTART_AZURE_AI_TESTS.md](NewFramework/CSharpTests/QUICKSTART_AZURE_AI_TESTS.md)** | 5-minute quick start guide |
| **[AZURE_AI_SERVICES_COMPLETION_STATUS.md](NewFramework/CSharpTests/AZURE_AI_SERVICES_COMPLETION_STATUS.md)** | Project completion status |

---

## 🏗️ Architecture

### BDD Framework Stack

```
┌─────────────────────────────────────┐
│   Gherkin Feature Files (.feature)  │  ← Business-readable scenarios
├─────────────────────────────────────┤
│   Reqnroll (BDD Framework)          │  ← Converts Gherkin to C#
├─────────────────────────────────────┤
│   Step Definitions (.cs)            │  ← C# test implementations
├─────────────────────────────────────┤
│   Helper Classes                    │  ← Azure SDK wrappers
├─────────────────────────────────────┤
│   Azure SDKs                        │  ← Azure.Search, Azure.AI
├─────────────────────────────────────┤
│   NUnit Test Runner                 │  ← Test execution
└─────────────────────────────────────┘
```

### Design Patterns

- **Helper Pattern** - Azure SDK operations encapsulated in reusable classes
- **Dependency Injection** - ScenarioContext for state management
- **Singleton Pattern** - ConfigManager for configuration
- **Factory Pattern** - Dynamic model selection
- **Async/Await** - Proper asynchronous programming

---

## 🛠️ Technology Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 9.0 | Runtime framework |
| **Reqnroll** | 2.0.3 | BDD framework (SpecFlow successor) |
| **NUnit** | 4.2.2 | Test runner |
| **Azure.Search.Documents** | 11.5.1 | Azure AI Search SDK |
| **Azure.AI.FormRecognizer** | 4.1.0 | Document Intelligence SDK |
| **Microsoft.Playwright** | 1.40.0 | Browser automation |
| **Allure** | 2.12.1 | Beautiful HTML test reports |
| **Serilog** | 3.1.1 | Structured logging |
| **FluentAssertions** | 6.12.0 | Fluent test assertions |

---

## 📊 Test Statistics

| Metric | Count |
|--------|-------|
| **Feature Files** | 3 (Azure AI Services) |
| **Test Scenarios** | 35 |
| **Step Definitions** | ~2,100 lines |
| **Helper Classes** | ~950 lines |
| **Total Code** | ~5,350 lines |
| **Documentation** | ~1,500 lines |

---

## 🎯 Example Test Scenario

```gherkin
Feature: Azure AI Search Integration
  
  @smoke @AzureAI
  Scenario: Create and query search index
    Given I have configured Azure AI Search
    When I create a search index "products-index"
    And I index 10 sample documents
    And I search for "laptop" in the index
    Then I should get at least 1 search result
    And the results should have relevance scores
```

**C# Step Definition:**
```csharp
[When(@"I create a search index ""(.*)""")]
public async Task WhenICreateSearchIndex(string indexName)
{
    var helper = new AzureAISearchHelper(endpoint, apiKey);
    await helper.CreateIndexAsync(indexName, fields);
    _scenarioContext["IndexName"] = indexName;
}
```

---

## 🐛 Debugging

### Set Breakpoints
1. Open step definition file
2. Click in margin to set breakpoint (F9)
3. Right-click test → Debug

### View Logs
```bash
# Logs are written to:
NewFramework/CSharpTests/bin/Debug/net9.0/logs/

# View latest log:
tail -f NewFramework/CSharpTests/bin/Debug/net9.0/logs/test-*.log
```

### Verbose Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

---

## 🔍 Troubleshooting

### Tests Not Appearing?
```bash
dotnet clean
dotnet build
# Restart Visual Studio
```

### Build Errors?
```bash
dotnet restore
dotnet clean && dotnet build
```

### Azure Connection Issues?
1. Verify credentials in `appsettings.json`
2. Test endpoints in Azure Portal
3. Check firewall/network settings

**See [VISUAL_STUDIO_GUIDE.md](VISUAL_STUDIO_GUIDE.md#troubleshooting) for more solutions**

---

## 📈 CI/CD Integration

### GitHub Actions Example

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
      
      - name: Run tests
        run: |
          cd NewFramework/CSharpTests
          dotnet test --filter "Category=smoke"
```

---

## 🤝 Contributing

### Adding New Tests

1. **Create Feature File**: `Features/MyFeature.feature`
2. **Write Scenarios**: Use Gherkin syntax
3. **Generate Steps**: Right-click → Generate Step Definition
4. **Implement**: Add C# code in step definitions
5. **Test**: Run with `dotnet test`

### Code Style

- Follow C# naming conventions
- Use async/await for Azure SDK calls
- Add XML documentation comments
- Include error handling with try-catch
- Use structured logging with Serilog

---

## 📝 License

[Your License Here]

---

## 📞 Support

### Documentation
- [Visual Studio Guide](VISUAL_STUDIO_GUIDE.md)
- [Quick Reference](QUICK_REFERENCE.md)
- [Azure AI Services Tests](NewFramework/CSharpTests/README_AZURE_AI_SERVICES_TESTS.md)

### External Resources
- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [Azure SDK Documentation](https://learn.microsoft.com/en-us/dotnet/azure/)
- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)

---

## ✅ Status

**Current Status:** ✅ **Production Ready**

- ✅ 35 comprehensive BDD test scenarios
- ✅ Full C# implementation with Reqnroll
- ✅ Azure AI Search integration
- ✅ Azure Document Intelligence integration
- ✅ End-to-end integration tests
- ✅ Complete documentation
- ✅ Visual Studio solution configured
- ✅ Ready for CI/CD integration

---

## 🎉 Getting Started Checklist

- [ ] Clone repository
- [ ] Open `AzureML-BDD-Framework.sln` in Visual Studio
- [ ] Install Reqnroll extension
- [ ] Configure Azure credentials in `appsettings.json`
- [ ] Run `dotnet restore`
- [ ] Run `dotnet build`
- [ ] Run `dotnet test --filter "Category=smoke"`
- [ ] Review test results in Test Explorer

---

**Ready to test! 🚀**

For detailed instructions, see [VISUAL_STUDIO_GUIDE.md](VISUAL_STUDIO_GUIDE.md)