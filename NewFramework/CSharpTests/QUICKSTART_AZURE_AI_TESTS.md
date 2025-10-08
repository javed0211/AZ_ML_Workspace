# Quick Start Guide - Azure AI Services C# BDD Tests

## ⚡ 5-Minute Setup

### Step 1: Configure Azure Credentials

Edit `/NewFramework/Config/appsettings.json`:

```json
{
  "Environment": "dev",
  "Environments": {
    "dev": {
      "AzureAISearch": {
        "Endpoint": "https://YOUR-SERVICE.search.windows.net",
        "ApiKey": "YOUR-API-KEY"
      },
      "DocumentIntelligence": {
        "Endpoint": "https://YOUR-SERVICE.cognitiveservices.azure.com/",
        "ApiKey": "YOUR-API-KEY"
      }
    }
  }
}
```

### Step 2: Build Project

```bash
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests
dotnet restore
dotnet build
```

### Step 3: Run Tests

```bash
# Run all Azure AI tests
dotnet test --filter "FullyQualifiedName~AzureAI"

# Run smoke tests only
dotnet test --filter "Category=smoke"

# Run specific feature
dotnet test --filter "FullyQualifiedName~AzureAISearchIntegration"
```

## 🎯 Common Commands

### Run Tests by Category

```bash
# Smoke tests (quick validation)
dotnet test --filter "Category=smoke"

# Search tests
dotnet test --filter "Category=search"

# Document Intelligence tests
dotnet test --filter "Category=document"

# Integration tests
dotnet test --filter "Category=integration"

# Performance tests
dotnet test --filter "Category=performance"
```

### Run Specific Scenarios

```bash
# Search scenarios
dotnet test --filter "DisplayName~search"

# Semantic search
dotnet test --filter "DisplayName~semantic"

# Invoice processing
dotnet test --filter "DisplayName~invoice"

# Batch processing
dotnet test --filter "DisplayName~batch"
```

### Debug Mode

```bash
# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run single test
dotnet test --filter "DisplayName~Create a new search index"
```

## 📊 Test Structure

### Feature Files (Gherkin)
```
Features/
├── AzureAISearchIntegration.feature       # 11 scenarios
├── AzureDocumentIntelligence.feature      # 13 scenarios
└── AzureAIServicesIntegration.feature     # 11 scenarios
```

### Step Definitions (C#)
```
StepDefinitions/
├── AzureAISearchIntegrationSteps.cs
├── AzureDocumentIntelligenceSteps.cs
└── AzureAIServicesIntegrationSteps.cs
```

### Helper Classes
```
Utils/
├── AzureAISearchHelper.cs                 # Search SDK wrapper
└── AzureDocumentIntelligenceHelper.cs     # Doc Intelligence SDK wrapper
```

## 🔍 Example Test Scenarios

### Azure AI Search

```gherkin
Scenario: Create a new search index
    When I create a search index named "documents-index" with fields
    Then the search index should be created successfully
    And the index should have 5 fields

Scenario: Perform semantic search with ranking
    Given I have a search index with semantic configuration
    When I perform a semantic search for "how to build neural networks"
    Then I should receive semantically ranked results
    And the results should include semantic captions
```

### Document Intelligence

```gherkin
Scenario: Analyze a prebuilt invoice document
    Given I have an invoice document at "test-data/sample-invoice.pdf"
    When I analyze the document using the prebuilt invoice model
    Then the analysis should complete successfully
    And the invoice should have a vendor name
    And the confidence score should be greater than 0.7

Scenario: Batch process multiple documents
    Given I have multiple documents in folder "test-data/batch"
    When I batch process all documents with appropriate models
    Then all documents should be processed successfully
```

### End-to-End Integration

```gherkin
Scenario: Extract invoice data and index for search
    Given I have an invoice document
    When I analyze the invoice using Document Intelligence
    And I extract relevant fields
    And I create a search document from the extracted data
    And I upload the document to search index
    Then the document should be searchable
    And I should be able to find it by vendor name
```

## 🛠️ Helper Class Usage

### Azure AI Search Helper

```csharp
// Initialize
var searchHelper = new AzureAISearchHelper(endpoint, apiKey);

// Create index
var fields = new List<SearchIndexField>
{
    new SearchIndexField { FieldName = "id", Type = "Edm.String", IsKey = true },
    new SearchIndexField { FieldName = "content", Type = "Edm.String", Searchable = true }
};
await searchHelper.CreateIndexAsync("my-index", fields);

// Index documents
var documents = new List<SearchDocument>
{
    new SearchDocument 
    { 
        Fields = new Dictionary<string, object>
        {
            ["id"] = "1",
            ["content"] = "Machine learning basics"
        }
    }
};
await searchHelper.IndexDocumentsAsync("my-index", documents);

// Search
var results = await searchHelper.SearchAsync("my-index", "machine learning");

// Semantic search
var semanticResults = await searchHelper.SemanticSearchAsync("my-index", "AI concepts");

// Delete index
await searchHelper.DeleteIndexAsync("my-index");
```

### Document Intelligence Helper

```csharp
// Initialize
var docHelper = new AzureDocumentIntelligenceHelper(endpoint, apiKey);

// Analyze invoice
var result = await docHelper.AnalyzeInvoiceAsync("invoice.pdf");
var vendorName = result.Fields["VendorName"].Value;
var total = result.Fields["InvoiceTotal"].Value;

// Analyze receipt
var receiptResult = await docHelper.AnalyzeReceiptAsync("receipt.jpg");

// Analyze ID document
var idResult = await docHelper.AnalyzeIdDocumentAsync("id-card.png");

// Analyze layout
var layoutResult = await docHelper.AnalyzeLayoutAsync("document.pdf");

// Batch process
var documents = new Dictionary<string, string>
{
    ["invoice1.pdf"] = "prebuilt-invoice",
    ["receipt1.jpg"] = "prebuilt-receipt"
};
var batchResult = await docHelper.BatchProcessDocumentsConcurrentlyAsync(documents);

// Train custom model
var trainingResult = await docHelper.TrainCustomModelAsync(
    trainingDataUrl, "my-custom-model", "Custom form model");
```

## 📝 Writing Your First Test

### 1. Create Feature File

`Features/MyTest.feature`:

```gherkin
Feature: My First Azure AI Test
    As a developer
    I want to test Azure AI Search
    So that I can verify it works

@smoke
Scenario: Simple search test
    Given I have access to Azure AI Search service
    And I have a search index named "test-index" with documents
    When I search for "test" in the index
    Then I should receive search results
    And the results should contain at least 1 document
```

### 2. Run Your Test

```bash
dotnet build
dotnet test --filter "DisplayName~Simple search test"
```

## 🐛 Troubleshooting

### Issue: "Unauthorized" Error
```bash
# Check your API keys in appsettings.json
# Or set environment variables:
export AZURE_SEARCH_API_KEY="your-key"
export AZURE_DOCUMENT_INTELLIGENCE_API_KEY="your-key"
```

### Issue: Tests Not Found
```bash
# Rebuild to generate Reqnroll code-behind files
dotnet clean
dotnet build
```

### Issue: Index Already Exists
```bash
# Tests clean up automatically, but you can manually delete:
# Use Azure Portal or Azure CLI to delete test indexes
```

### Issue: Document Not Found
```bash
# Tests create test documents automatically
# Ensure test-data directory exists or check file paths
```

## 📊 View Test Results

### Console Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Beautiful HTML Report with Allure
```bash
# Install Allure (one-time)
brew install allure  # macOS
# or scoop install allure  # Windows

# Generate and view report
./generate-allure-report.sh  # macOS/Linux
# or .\generate-allure-report.ps1  # Windows
```

See [ALLURE_QUICK_START.md](../../ALLURE_QUICK_START.md) for complete guide.

## 🎓 Next Steps

1. **Explore Feature Files**: Review existing scenarios in `Features/` directory
2. **Read Full Documentation**: See `README_AZURE_AI_SERVICES_TESTS.md`
3. **Customize Tests**: Modify scenarios for your use cases
4. **Add New Tests**: Create new feature files and step definitions
5. **Integrate CI/CD**: Add tests to your build pipeline

## 📚 Key Files

| File | Purpose |
|------|---------|
| `Features/*.feature` | BDD test scenarios (Gherkin) |
| `StepDefinitions/*Steps.cs` | Step implementations (C#) |
| `Utils/*Helper.cs` | Azure SDK wrappers |
| `PlaywrightFramework.csproj` | Project configuration |
| `appsettings.json` | Azure credentials |
| `reqnroll.json` | Reqnroll configuration |

## 🚀 Pro Tips

1. **Use Tags**: Filter tests with `@smoke`, `@integration`, `@performance`
2. **Parallel Execution**: Run `dotnet test --parallel 4` for faster execution
3. **Watch Mode**: Use `dotnet watch test` for continuous testing
4. **Selective Tests**: Use `--filter` to run specific scenarios
5. **Debug Mode**: Set breakpoints in step definitions and debug in VS

## 📞 Need Help?

- Check logs in `Reports/logs/test-execution.log`
- Review Azure service status
- See full README for detailed documentation
- Check Azure SDK documentation

---

**Ready to test? Run your first test now!**

```bash
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/CSharpTests
dotnet test --filter "Category=smoke"
```