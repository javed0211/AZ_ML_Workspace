# Azure AI Services Test Suite

This test suite provides comprehensive testing for **Azure AI Search** and **Azure Document Intelligence** services integrated with VS Code automation.

## Overview

The test suite includes automated tests for:

1. **Azure AI Search Service**
   - Search index configuration and management
   - Document indexing and retrieval
   - Semantic search capabilities
   - Query patterns and filtering
   - Faceted search and aggregations

2. **Azure Document Intelligence Service** (formerly Form Recognizer)
   - Invoice processing and data extraction
   - Receipt analysis
   - ID document recognition
   - Layout analysis
   - Custom model training
   - General document processing

3. **Integrated Pipeline**
   - End-to-end document processing workflow
   - Document Intelligence → AI Search integration
   - Batch processing capabilities

## Test Files

### Main Test File
- **`azure-ai-services.test.ts`** - Main Playwright test suite

### Generated Python Scripts

#### Azure AI Search
1. **`azure_search_indexer.py`**
   - Create and manage search indexes
   - Index documents with semantic search
   - Configure search fields and analyzers
   - Implement semantic search configurations

2. **`azure_search_queries.py`**
   - Simple full-text search
   - Filtered search with OData expressions
   - Faceted search for aggregations
   - Semantic search with AI ranking
   - Autocomplete and suggestions

#### Document Intelligence
3. **`document_intelligence_analyzer.py`**
   - Analyze invoices and extract structured data
   - Process receipts and extract transaction details
   - Recognize ID documents (passports, licenses)
   - Extract document layout (text, tables, structure)
   - Process general documents with key-value pairs

4. **`custom_model_trainer.py`**
   - Train custom document models
   - Compose multiple models
   - Manage model lifecycle
   - Get resource information

#### Integration
5. **`integrated_document_pipeline.py`**
   - Complete document processing pipeline
   - Extract data with Document Intelligence
   - Index extracted data into AI Search
   - Enable semantic search on processed documents
   - Batch processing capabilities

### Configuration Files
- **`azure-search-config.json`** - Azure AI Search configuration
- **`document-intelligence-config.json`** - Document Intelligence configuration
- **`azure-ai-requirements.txt`** - Python dependencies

## Prerequisites

### Azure Resources Required

1. **Azure AI Search Service**
   - Service endpoint
   - Admin API key
   - Search index created

2. **Azure Document Intelligence Service**
   - Service endpoint
   - API key
   - Appropriate pricing tier for desired features

3. **Azure Storage Account** (for custom model training)
   - Blob container with training data
   - SAS token for access

### Python Dependencies

Install required packages:

```bash
pip install -r azure-ai-requirements.txt
```

Key packages:
- `azure-search-documents` - Azure AI Search SDK
- `azure-ai-formrecognizer` - Document Intelligence SDK
- `azure-identity` - Azure authentication
- `azure-core` - Core Azure functionality

## Configuration

### 1. Update appsettings.json

Edit `/NewFramework/Config/appsettings.json`:

```json
{
  "Environments": {
    "dev": {
      "AzureAISearch": {
        "ServiceName": "your-search-service",
        "Endpoint": "https://your-search-service.search.windows.net",
        "ApiKey": "your-search-api-key",
        "ApiVersion": "2023-11-01",
        "IndexName": "documents-index",
        "SemanticConfiguration": "default-semantic-config"
      },
      "DocumentIntelligence": {
        "ServiceName": "your-doc-intelligence",
        "Endpoint": "https://your-doc-intelligence.cognitiveservices.azure.com/",
        "ApiKey": "your-doc-intelligence-api-key",
        "ApiVersion": "2023-07-31",
        "Models": {
          "Invoice": "prebuilt-invoice",
          "Receipt": "prebuilt-receipt",
          "IdDocument": "prebuilt-idDocument",
          "Layout": "prebuilt-layout",
          "GeneralDocument": "prebuilt-document"
        }
      }
    }
  }
}
```

### 2. Environment Variables (Optional)

Create a `.env` file:

```bash
# Azure AI Search
AZURE_SEARCH_ENDPOINT=https://your-search-service.search.windows.net
AZURE_SEARCH_API_KEY=your-search-api-key
AZURE_SEARCH_INDEX_NAME=documents-index

# Azure Document Intelligence
AZURE_DOC_INTEL_ENDPOINT=https://your-doc-intelligence.cognitiveservices.azure.com/
AZURE_DOC_INTEL_API_KEY=your-doc-intelligence-api-key

# Azure Storage (for custom model training)
AZURE_STORAGE_CONNECTION_STRING=your-storage-connection-string
AZURE_STORAGE_CONTAINER=training-data
```

## Running the Tests

### Run All Azure AI Services Tests

```bash
# From ElectronTests directory
npm run build
npx playwright test tests/azure-ai-services.test.ts
```

### Run Specific Test Suites

```bash
# Azure AI Search tests only
npx playwright test tests/azure-ai-services.test.ts -g "Azure AI Search Service"

# Document Intelligence tests only
npx playwright test tests/azure-ai-services.test.ts -g "Azure Document Intelligence Service"

# Integration tests only
npx playwright test tests/azure-ai-services.test.ts -g "Azure AI Services Integration"
```

### Run in Headed Mode (See VS Code UI)

```bash
npx playwright test tests/azure-ai-services.test.ts --headed
```

### Debug Mode

```bash
npx playwright test tests/azure-ai-services.test.ts --debug
```

## Test Scenarios

### Azure AI Search Tests

1. **Create Search Configuration**
   - Creates `azure-search-config.json` with index schema
   - Defines searchable fields and analyzers
   - Configures semantic search settings

2. **Create Indexing Script**
   - Generates `azure_search_indexer.py`
   - Implements document indexing logic
   - Includes semantic search configuration

3. **Create Query Examples**
   - Generates `azure_search_queries.py`
   - Demonstrates various query patterns
   - Shows filtering, faceting, and semantic search

### Document Intelligence Tests

1. **Create Configuration**
   - Creates `document-intelligence-config.json`
   - Lists available prebuilt models
   - Configures processing options

2. **Create Analysis Script**
   - Generates `document_intelligence_analyzer.py`
   - Implements invoice, receipt, and ID analysis
   - Includes layout and general document processing

3. **Create Custom Model Training Script**
   - Generates `custom_model_trainer.py`
   - Shows how to train custom models
   - Demonstrates model composition and management

### Integration Tests

1. **Create Integrated Pipeline**
   - Generates `integrated_document_pipeline.py`
   - Combines Document Intelligence and AI Search
   - Implements end-to-end document processing

2. **Create Requirements File**
   - Generates `azure-ai-requirements.txt`
   - Lists all required Python packages

3. **Verify File Creation**
   - Confirms all files are created
   - Takes screenshots for verification

## Usage Examples

### Azure AI Search

```python
from azure_search_indexer import AzureSearchIndexer

# Initialize
indexer = AzureSearchIndexer(
    service_endpoint="https://your-service.search.windows.net",
    api_key="your-api-key",
    index_name="documents-index"
)

# Create index
indexer.create_index()

# Index documents
documents = [
    {
        "id": "1",
        "title": "Document Title",
        "content": "Document content...",
        "category": "Documentation",
        "tags": ["azure", "search"]
    }
]
indexer.index_documents(documents)

# Search
results = indexer.search_documents(
    query="azure search",
    use_semantic_search=True
)
```

### Document Intelligence

```python
from document_intelligence_analyzer import DocumentIntelligenceAnalyzer

# Initialize
analyzer = DocumentIntelligenceAnalyzer(
    endpoint="https://your-service.cognitiveservices.azure.com/",
    api_key="your-api-key"
)

# Analyze invoice
invoice_data = analyzer.analyze_invoice("invoice.pdf")
print(f"Vendor: {invoice_data['vendor_name']}")
print(f"Total: {invoice_data['total']}")

# Analyze receipt
receipt_data = analyzer.analyze_receipt("receipt.jpg")
print(f"Merchant: {receipt_data['merchant_name']}")
print(f"Total: {receipt_data['total']}")
```

### Integrated Pipeline

```python
from integrated_document_pipeline import DocumentProcessingPipeline

# Initialize pipeline
pipeline = DocumentProcessingPipeline(
    doc_intel_endpoint="https://your-doc-intel.cognitiveservices.azure.com/",
    doc_intel_key="your-doc-intel-key",
    search_endpoint="https://your-search.search.windows.net",
    search_key="your-search-key",
    search_index="documents-index"
)

# Process and index invoices
invoice_paths = ["invoice1.pdf", "invoice2.pdf"]
summary = pipeline.process_and_index_batch(invoice_paths, "invoice")

# Search processed documents
results = pipeline.search_documents(
    query="vendor invoices from last month",
    document_type="invoice"
)
```

## Features Demonstrated

### Azure AI Search Features
- ✅ Index creation and management
- ✅ Document indexing with semantic search
- ✅ Full-text search
- ✅ Filtered search with OData
- ✅ Faceted search and aggregations
- ✅ Semantic search with AI ranking
- ✅ Autocomplete and suggestions
- ✅ Custom analyzers and tokenizers

### Document Intelligence Features
- ✅ Prebuilt invoice model
- ✅ Prebuilt receipt model
- ✅ Prebuilt ID document model
- ✅ Layout analysis (text, tables, structure)
- ✅ General document processing
- ✅ Custom model training
- ✅ Model composition
- ✅ Batch processing

### Integration Features
- ✅ End-to-end document processing
- ✅ Automatic data extraction and indexing
- ✅ Semantic search on processed documents
- ✅ Batch processing capabilities
- ✅ Error handling and logging

## Troubleshooting

### Common Issues

1. **Authentication Errors**
   - Verify API keys are correct
   - Check endpoint URLs
   - Ensure proper permissions

2. **Index Not Found**
   - Create index before indexing documents
   - Verify index name matches configuration

3. **Model Not Available**
   - Check Document Intelligence pricing tier
   - Verify model name is correct
   - Some models require specific regions

4. **Rate Limiting**
   - Implement retry logic
   - Add delays between requests
   - Consider upgrading pricing tier

### Debug Tips

1. Enable detailed logging:
```python
import logging
logging.basicConfig(level=logging.DEBUG)
```

2. Check service health:
```python
# For AI Search
info = index_client.get_service_statistics()

# For Document Intelligence
info = admin_client.get_resource_details()
```

3. Validate documents before indexing:
```python
# Check document structure matches index schema
```

## Best Practices

1. **Security**
   - Never commit API keys to source control
   - Use Azure Key Vault for production
   - Implement proper authentication (Azure AD)

2. **Performance**
   - Batch document uploads when possible
   - Use appropriate index size
   - Implement caching for frequent queries

3. **Cost Optimization**
   - Choose appropriate pricing tiers
   - Monitor usage and costs
   - Clean up unused indexes and models

4. **Error Handling**
   - Implement retry logic
   - Log errors for debugging
   - Handle rate limiting gracefully

## Resources

### Documentation
- [Azure AI Search Documentation](https://learn.microsoft.com/azure/search/)
- [Azure Document Intelligence Documentation](https://learn.microsoft.com/azure/ai-services/document-intelligence/)
- [Azure SDK for Python](https://learn.microsoft.com/python/api/overview/azure/)

### Pricing
- [Azure AI Search Pricing](https://azure.microsoft.com/pricing/details/search/)
- [Azure Document Intelligence Pricing](https://azure.microsoft.com/pricing/details/form-recognizer/)

### Support
- [Azure Support](https://azure.microsoft.com/support/)
- [Stack Overflow - Azure](https://stackoverflow.com/questions/tagged/azure)

## Contributing

To add new test scenarios:

1. Add test cases to `azure-ai-services.test.ts`
2. Follow existing patterns for consistency
3. Update this README with new features
4. Test thoroughly before committing

## License

This test suite is part of the AZ ML Workspace project and follows the same license.

---

**Last Updated:** 2024
**Version:** 1.0.0
**Maintainer:** AZ ML Workspace Team