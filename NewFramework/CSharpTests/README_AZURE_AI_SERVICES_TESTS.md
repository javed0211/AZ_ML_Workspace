# Azure AI Services C# BDD Test Suite

## Overview

This comprehensive test suite provides **Behavior-Driven Development (BDD)** test cases for **Azure AI Search** and **Azure Document Intelligence** services using **Reqnroll** (SpecFlow successor) with **C#** and **NUnit**.

## 📁 Project Structure

```
CSharpTests/
├── Features/
│   ├── AzureAISearchIntegration.feature          # Azure AI Search test scenarios
│   ├── AzureDocumentIntelligence.feature         # Document Intelligence test scenarios
│   └── AzureAIServicesIntegration.feature        # End-to-end integration scenarios
├── StepDefinitions/
│   ├── AzureAISearchIntegrationSteps.cs          # Search step implementations
│   ├── AzureDocumentIntelligenceSteps.cs         # Document Intelligence step implementations
│   └── AzureAIServicesIntegrationSteps.cs        # Integration step implementations
├── Utils/
│   ├── AzureAISearchHelper.cs                    # Azure AI Search SDK wrapper
│   ├── AzureDocumentIntelligenceHelper.cs        # Document Intelligence SDK wrapper
│   └── ConfigManager.cs                          # Configuration management
└── PlaywrightFramework.csproj                    # Project file with dependencies
```

## 🎯 Test Coverage

### Azure AI Search Integration (11 scenarios)
- ✅ Create search index with custom fields
- ✅ Upload documents to index
- ✅ Simple text search
- ✅ Search with filters (OData)
- ✅ Search with facets
- ✅ Semantic search with AI ranking
- ✅ Autocomplete suggestions
- ✅ Performance testing (concurrent searches)
- ✅ Delete documents
- ✅ Delete index
- ✅ Scenario outline for different content types

### Azure Document Intelligence (13 scenarios)
- ✅ Analyze prebuilt invoice documents
- ✅ Analyze receipt documents
- ✅ Extract ID document information
- ✅ Extract document layout and structure
- ✅ Train custom document models
- ✅ Analyze with custom models
- ✅ Batch process multiple documents
- ✅ Extract tables from documents
- ✅ Compose models from multiple models
- ✅ Performance testing (concurrent analysis)
- ✅ Validate extracted data quality
- ✅ Handle unsupported document formats
- ✅ Scenario outline for different document types

### End-to-End Integration (11 scenarios)
- ✅ Extract invoice data and index for search
- ✅ Process multiple documents and create searchable index
- ✅ Build semantic search for document content
- ✅ Automated document processing pipeline
- ✅ Enrich search index with AI-extracted data
- ✅ Real-time document search after extraction
- ✅ Validate end-to-end data accuracy
- ✅ High-volume document processing and indexing
- ✅ Monitor pipeline health and metrics
- ✅ Handle failures in processing pipeline
- ✅ Scenario outline for different document categories

**Total: 35 comprehensive test scenarios**

## 🚀 Getting Started

### Prerequisites

1. **.NET 9.0 SDK** installed
2. **Azure Subscription** with:
   - Azure AI Search service
   - Azure Document Intelligence (Form Recognizer) service
3. **Visual Studio 2022** or **VS Code** with C# extension
4. **NuGet packages** (automatically restored):
   - Azure.Search.Documents (11.5.1)
   - Azure.AI.FormRecognizer (4.1.0)
   - Reqnroll (2.0.3)
   - NUnit (3.13.3)

### Configuration

1. **Update appsettings.json** with your Azure credentials:

```json
{
  "Environment": "dev",
  "Environments": {
    "dev": {
      "AzureAISearch": {
        "Endpoint": "https://YOUR-SERVICE.search.windows.net",
        "ApiKey": "YOUR-SEARCH-API-KEY",
        "IndexName": "documents-index"
      },
      "DocumentIntelligence": {
        "Endpoint": "https://YOUR-SERVICE.cognitiveservices.azure.com/",
        "ApiKey": "YOUR-DOC-INTELLIGENCE-API-KEY"
      }
    }
  }
}
```

2. **Or use environment variables**:

```bash
export AZURE_SEARCH_ENDPOINT="https://your-service.search.windows.net"
export AZURE_SEARCH_API_KEY="your-api-key"
export AZURE_DOCUMENT_INTELLIGENCE_ENDPOINT="https://your-service.cognitiveservices.azure.com/"
export AZURE_DOCUMENT_INTELLIGENCE_API_KEY="your-api-key"
```

### Installation

```bash
# Navigate to test project
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Generate Reqnroll feature code-behind files
dotnet build
```

## 🧪 Running Tests

### Run All Tests

```bash
# Run all Azure AI Services tests
dotnet test --filter "FullyQualifiedName~AzureAI"

# Run with detailed output
dotnet test --filter "FullyQualifiedName~AzureAI" --logger "console;verbosity=detailed"
```

### Run Specific Feature Files

```bash
# Run only Azure AI Search tests
dotnet test --filter "FullyQualifiedName~AzureAISearchIntegration"

# Run only Document Intelligence tests
dotnet test --filter "FullyQualifiedName~AzureDocumentIntelligence"

# Run only Integration tests
dotnet test --filter "FullyQualifiedName~AzureAIServicesIntegration"
```

### Run Tests by Tags

```bash
# Run smoke tests only
dotnet test --filter "Category=smoke"

# Run search tests
dotnet test --filter "Category=search"

# Run document tests
dotnet test --filter "Category=document"

# Run integration tests
dotnet test --filter "Category=integration"

# Run performance tests
dotnet test --filter "Category=performance"
```

### Run Specific Scenarios

```bash
# Run a specific scenario by name
dotnet test --filter "DisplayName~Create a new search index"

# Run semantic search tests
dotnet test --filter "DisplayName~semantic"

# Run batch processing tests
dotnet test --filter "DisplayName~batch"
```

### Using Visual Studio

1. Open `PlaywrightFramework.sln` in Visual Studio
2. Open **Test Explorer** (Test → Test Explorer)
3. Right-click on test scenarios and select **Run** or **Debug**
4. Filter tests by traits/categories

### Using VS Code

```bash
# Install .NET Test Explorer extension
# Then use the Test Explorer sidebar to run tests
```

## 📊 Test Reports

### Generate HTML Report

```bash
# Install ReportGenerator
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Generate HTML report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"Reports/coverage" -reporttypes:Html
```

### Generate Beautiful HTML Reports with Allure

```bash
# Install Allure (one-time)
brew install allure  # macOS
# or scoop install allure  # Windows

# Generate and view report
./generate-allure-report.sh  # macOS/Linux
# or .\generate-allure-report.ps1  # Windows
```

**Features:**
- 📊 Interactive dashboard with charts and statistics
- 📸 Automatic screenshot capture on failures
- 📈 Historical trends tracking
- 🏷️ Test categorization by features and tags
- ⏱️ Timeline view of test execution

See [ALLURE_QUICK_START.md](../../ALLURE_QUICK_START.md) for complete guide.

## 🔧 Helper Classes

### AzureAISearchHelper

Provides methods for:
- Creating and managing search indexes
- Indexing documents
- Performing searches (simple, filtered, faceted, semantic)
- Autocomplete suggestions
- Deleting documents and indexes

**Example Usage:**

```csharp
var searchHelper = new AzureAISearchHelper(endpoint, apiKey);

// Create index
var fields = new List<SearchIndexField>
{
    new SearchIndexField { FieldName = "id", Type = "Edm.String", IsKey = true },
    new SearchIndexField { FieldName = "content", Type = "Edm.String", Searchable = true }
};
await searchHelper.CreateIndexAsync("my-index", fields);

// Search
var results = await searchHelper.SearchAsync("my-index", "machine learning");
```

### AzureDocumentIntelligenceHelper

Provides methods for:
- Analyzing documents with prebuilt models (invoice, receipt, ID, business card)
- Analyzing document layout
- Training custom models
- Batch processing documents
- Composing models
- Validating extracted data

**Example Usage:**

```csharp
var docHelper = new AzureDocumentIntelligenceHelper(endpoint, apiKey);

// Analyze invoice
var result = await docHelper.AnalyzeInvoiceAsync("path/to/invoice.pdf");

// Access extracted fields
var vendorName = result.Fields["VendorName"].Value;
var total = result.Fields["InvoiceTotal"].Value;
```

## 📝 Writing New Tests

### 1. Add Feature File

Create a new `.feature` file in the `Features/` directory:

```gherkin
Feature: My New Feature
    As a user
    I want to test something
    So that I can verify it works

Scenario: My test scenario
    Given I have some precondition
    When I perform an action
    Then I should see expected result
```

### 2. Implement Step Definitions

Create corresponding step definitions in `StepDefinitions/`:

```csharp
[Binding]
public class MyNewFeatureSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly ILogger _logger;

    public MyNewFeatureSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _logger = Log.ForContext<MyNewFeatureSteps>();
    }

    [Given(@"I have some precondition")]
    public void GivenIHaveSomePrecondition()
    {
        _logger.Information("Setting up precondition");
        // Implementation
    }

    [When(@"I perform an action")]
    public async Task WhenIPerformAnAction()
    {
        _logger.Information("Performing action");
        // Implementation
    }

    [Then(@"I should see expected result")]
    public void ThenIShouldSeeExpectedResult()
    {
        _logger.Information("Verifying result");
        Assert.That(/* condition */, Is.True);
    }
}
```

### 3. Update Project File

Add the new feature file to `PlaywrightFramework.csproj`:

```xml
<Reqnroll Include="Features/MyNewFeature.feature">
  <Generator>ReqnrollSingleFileGenerator</Generator>
  <LastGenOutput>MyNewFeature.feature.cs</LastGenOutput>
</Reqnroll>
```

### 4. Build and Run

```bash
dotnet build
dotnet test --filter "FullyQualifiedName~MyNewFeature"
```

## 🐛 Debugging

### Enable Detailed Logging

Update `reqnroll.json`:

```json
{
  "trace": {
    "traceSuccessfulSteps": true,
    "traceTimings": true,
    "minTracedDuration": "0:0:0.1"
  }
}
```

### Debug in Visual Studio

1. Set breakpoints in step definitions
2. Right-click test in Test Explorer
3. Select **Debug**

### View Logs

Logs are written to:
- Console output
- `Reports/logs/test-execution.log`

## 📈 Performance Considerations

### Concurrent Test Execution

```bash
# Run tests in parallel (4 workers)
dotnet test --parallel 4
```

### Test Data Management

- Use `[BeforeScenario]` and `[AfterScenario]` hooks for setup/cleanup
- Create test documents dynamically
- Clean up Azure resources after tests

### Cost Optimization

- Use shared Azure resources for tests
- Delete test indexes after scenarios
- Use mock data where possible
- Implement retry logic for transient failures

## 🔒 Security Best Practices

1. **Never commit API keys** to source control
2. Use **Azure Key Vault** for production credentials
3. Use **Managed Identity** when running in Azure
4. Rotate API keys regularly
5. Use **environment-specific** configurations

## 🤝 Contributing

### Code Style

- Follow C# coding conventions
- Use meaningful variable names
- Add XML documentation comments
- Keep methods focused and small

### Testing Guidelines

- Write descriptive scenario names
- Use Given-When-Then structure
- One assertion per Then step
- Use scenario outlines for data-driven tests
- Add appropriate tags (@smoke, @integration, etc.)

## 📚 Additional Resources

- [Reqnroll Documentation](https://docs.reqnroll.net/)
- [Azure AI Search .NET SDK](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/search.documents-readme)
- [Azure Document Intelligence .NET SDK](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/ai.formrecognizer-readme)
- [NUnit Documentation](https://docs.nunit.org/)
- [Gherkin Syntax Reference](https://cucumber.io/docs/gherkin/reference/)

## 🆘 Troubleshooting

### Common Issues

**Issue: Tests fail with "Index not found"**
```
Solution: Ensure index is created in BeforeScenario hook or Given step
```

**Issue: "Unauthorized" errors**
```
Solution: Verify API keys in appsettings.json or environment variables
```

**Issue: "Document not found" errors**
```
Solution: Ensure test documents exist or are created dynamically
```

**Issue: Reqnroll code-behind not generated**
```
Solution: Run `dotnet build` to trigger code generation
```

**Issue: Tests timeout**
```
Solution: Increase timeout in NUnit settings or add delays for indexing
```

## 📞 Support

For issues or questions:
1. Check this README
2. Review test logs in `Reports/logs/`
3. Check Azure service health
4. Review Azure SDK documentation

## 📄 License

This test suite is part of the Azure ML Workspace automation project.

---

**Last Updated:** 2024
**Version:** 1.0.0
**Maintainer:** Azure ML Automation Team