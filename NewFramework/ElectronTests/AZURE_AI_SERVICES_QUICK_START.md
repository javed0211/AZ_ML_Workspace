# Azure AI Services - Quick Start Guide

Quick reference for running Azure AI Search and Document Intelligence tests.

## 🚀 Quick Start

### 1. Install Dependencies

```bash
cd /Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/ElectronTests
npm install
npm run build
```

### 2. Configure Services

Edit `../Config/appsettings.json` and add your Azure credentials:

```json
{
  "Environments": {
    "dev": {
      "AzureAISearch": {
        "Endpoint": "https://YOUR-SERVICE.search.windows.net",
        "ApiKey": "YOUR-API-KEY",
        "IndexName": "documents-index"
      },
      "DocumentIntelligence": {
        "Endpoint": "https://YOUR-SERVICE.cognitiveservices.azure.com/",
        "ApiKey": "YOUR-API-KEY"
      }
    }
  }
}
```

### 3. Run Tests

```bash
# Run all Azure AI services tests
npx playwright test tests/azure-ai-services.test.ts

# Run with UI visible
npx playwright test tests/azure-ai-services.test.ts --headed

# Run specific test suite
npx playwright test tests/azure-ai-services.test.ts -g "Azure AI Search"
```

## 📋 Test Suites

### Azure AI Search Tests
```bash
npx playwright test tests/azure-ai-services.test.ts -g "Azure AI Search Service"
```

**What it tests:**
- ✅ Search index configuration
- ✅ Document indexing script
- ✅ Query examples (simple, filtered, semantic)

**Generated files:**
- `azure-search-config.json`
- `azure_search_indexer.py`
- `azure_search_queries.py`

### Document Intelligence Tests
```bash
npx playwright test tests/azure-ai-services.test.ts -g "Azure Document Intelligence Service"
```

**What it tests:**
- ✅ Document Intelligence configuration
- ✅ Invoice/receipt/ID analysis
- ✅ Custom model training

**Generated files:**
- `document-intelligence-config.json`
- `document_intelligence_analyzer.py`
- `custom_model_trainer.py`

### Integration Tests
```bash
npx playwright test tests/azure-ai-services.test.ts -g "Azure AI Services Integration"
```

**What it tests:**
- ✅ End-to-end document processing pipeline
- ✅ Document Intelligence → AI Search integration
- ✅ Requirements file generation

**Generated files:**
- `integrated_document_pipeline.py`
- `azure-ai-requirements.txt`

## 🔧 Using Generated Scripts

### 1. Install Python Dependencies

```bash
pip install -r azure-ai-requirements.txt
```

### 2. Azure AI Search Example

```python
from azure_search_indexer import AzureSearchIndexer

indexer = AzureSearchIndexer(
    service_endpoint="https://your-service.search.windows.net",
    api_key="your-api-key",
    index_name="documents-index"
)

# Create index
indexer.create_index()

# Index documents
documents = [{
    "id": "1",
    "title": "Test Document",
    "content": "This is a test document",
    "category": "Test",
    "created_date": "2024-01-01T00:00:00Z",
    "tags": ["test", "demo"]
}]
indexer.index_documents(documents)

# Search
results = indexer.search_documents("test", use_semantic_search=True)
print(results)
```

### 3. Document Intelligence Example

```python
from document_intelligence_analyzer import DocumentIntelligenceAnalyzer

analyzer = DocumentIntelligenceAnalyzer(
    endpoint="https://your-service.cognitiveservices.azure.com/",
    api_key="your-api-key"
)

# Analyze invoice
invoice_data = analyzer.analyze_invoice("path/to/invoice.pdf")
print(f"Vendor: {invoice_data['vendor_name']}")
print(f"Total: {invoice_data['total']}")

# Analyze receipt
receipt_data = analyzer.analyze_receipt("path/to/receipt.jpg")
print(f"Merchant: {receipt_data['merchant_name']}")
```

### 4. Integrated Pipeline Example

```python
from integrated_document_pipeline import DocumentProcessingPipeline

pipeline = DocumentProcessingPipeline(
    doc_intel_endpoint="https://your-doc-intel.cognitiveservices.azure.com/",
    doc_intel_key="your-doc-intel-key",
    search_endpoint="https://your-search.search.windows.net",
    search_key="your-search-key",
    search_index="documents-index"
)

# Process and index documents
summary = pipeline.process_and_index_batch(
    document_paths=["invoice1.pdf", "invoice2.pdf"],
    document_type="invoice"
)
print(f"Processed: {summary['processed']}, Indexed: {summary['indexed']}")

# Search processed documents
results = pipeline.search_documents(
    query="vendor invoices",
    document_type="invoice"
)
```

## 📊 Test Results

After running tests, view results:

```bash
# Open HTML report
open test-results/html-report/index.html

# View JSON results
cat test-results/results.json

# View JUnit results
cat test-results/junit-results.xml
```

## 🐛 Troubleshooting

### Test Fails to Start VS Code
```bash
# Check VS Code path in config
cat ../Config/appsettings.json | grep ExecutablePath

# Update if needed
```

### Authentication Errors
```bash
# Verify credentials
echo $AZURE_SEARCH_API_KEY
echo $AZURE_DOC_INTEL_API_KEY

# Or check appsettings.json
```

### Module Not Found
```bash
# Rebuild TypeScript
npm run build

# Reinstall dependencies
npm install
```

## 📚 Additional Resources

- **Full Documentation:** `tests/AZURE_AI_SERVICES_README.md`
- **Azure AI Search Docs:** https://learn.microsoft.com/azure/search/
- **Document Intelligence Docs:** https://learn.microsoft.com/azure/ai-services/document-intelligence/

## 🎯 Common Commands

```bash
# Run all tests
npm test

# Run specific test file
npx playwright test tests/azure-ai-services.test.ts

# Run in debug mode
npx playwright test tests/azure-ai-services.test.ts --debug

# Run with UI
npx playwright test tests/azure-ai-services.test.ts --ui

# Generate report
npx playwright show-report

# Clean and rebuild
npm run clean && npm install && npm run build
```

## ✅ Checklist

Before running tests:
- [ ] Azure AI Search service created
- [ ] Document Intelligence service created
- [ ] API keys obtained
- [ ] Configuration updated in `appsettings.json`
- [ ] Dependencies installed (`npm install`)
- [ ] TypeScript compiled (`npm run build`)
- [ ] VS Code path configured correctly

## 💡 Tips

1. **Run tests in headed mode first** to see what's happening
2. **Check screenshots** in `test-results/` for debugging
3. **Use semantic search** for better search results
4. **Batch process documents** for better performance
5. **Monitor Azure costs** when testing with real services

---

**Need Help?** Check the full documentation in `tests/AZURE_AI_SERVICES_README.md`