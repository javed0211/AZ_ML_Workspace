# Azure AI Services Test Architecture

## 🏗️ Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Playwright Test Suite                         │
│                  (azure-ai-services.test.ts)                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ├─────────────────────────────────┐
                              │                                 │
                              ▼                                 ▼
        ┌──────────────────────────────┐      ┌──────────────────────────────┐
        │   VS Code Automation         │      │   Test Helpers & Utils       │
        │   (vscode-electron.ts)       │      │   (test-helpers.ts)          │
        └──────────────────────────────┘      └──────────────────────────────┘
                              │
                              ▼
        ┌─────────────────────────────────────────────────────────┐
        │              Generated Python Scripts                    │
        │                                                           │
        │  ┌─────────────────────┐  ┌─────────────────────┐      │
        │  │  Azure AI Search    │  │ Document Intelligence│      │
        │  │                     │  │                      │      │
        │  │ • Indexer           │  │ • Analyzer           │      │
        │  │ • Query Examples    │  │ • Custom Trainer     │      │
        │  └─────────────────────┘  └─────────────────────┘      │
        │                                                           │
        │  ┌──────────────────────────────────────────────┐       │
        │  │      Integrated Pipeline                      │       │
        │  │  (Document Intelligence → AI Search)          │       │
        │  └──────────────────────────────────────────────┘       │
        └─────────────────────────────────────────────────────────┘
                              │
                              ▼
        ┌─────────────────────────────────────────────────────────┐
        │                  Azure Services                          │
        │                                                           │
        │  ┌──────────────────┐         ┌──────────────────┐      │
        │  │ Azure AI Search  │         │ Document Intel   │      │
        │  │                  │         │                  │      │
        │  │ • Indexes        │         │ • Prebuilt Models│      │
        │  │ • Semantic Search│         │ • Custom Models  │      │
        │  └──────────────────┘         └──────────────────┘      │
        └─────────────────────────────────────────────────────────┘
```

## 📊 Test Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         Test Execution Flow                      │
└─────────────────────────────────────────────────────────────────┘

1. Test Setup
   │
   ├─► Launch VS Code
   ├─► Create Test Workspace
   └─► Initialize Test Environment
   
2. Azure AI Search Tests
   │
   ├─► Create Configuration File
   │   └─► azure-search-config.json
   │
   ├─► Generate Indexer Script
   │   └─► azure_search_indexer.py
   │       ├─► create_index()
   │       ├─► index_documents()
   │       └─► search_documents()
   │
   └─► Generate Query Examples
       └─► azure_search_queries.py
           ├─► simple_search()
           ├─► filtered_search()
           ├─► faceted_search()
           └─► semantic_search()

3. Document Intelligence Tests
   │
   ├─► Create Configuration File
   │   └─► document-intelligence-config.json
   │
   ├─► Generate Analyzer Script
   │   └─► document_intelligence_analyzer.py
   │       ├─► analyze_invoice()
   │       ├─► analyze_receipt()
   │       ├─► analyze_id_document()
   │       └─► analyze_layout()
   │
   └─► Generate Training Script
       └─► custom_model_trainer.py
           ├─► train_custom_model()
           ├─► compose_models()
           └─► list_models()

4. Integration Tests
   │
   ├─► Generate Pipeline Script
   │   └─► integrated_document_pipeline.py
   │       ├─► process_invoice()
   │       ├─► index_document()
   │       └─► search_documents()
   │
   ├─► Generate Requirements
   │   └─► azure-ai-requirements.txt
   │
   └─► Verify All Files Created

5. Test Cleanup
   │
   ├─► Close VS Code
   ├─► Cleanup Temp Files
   └─► Generate Test Report
```

## 🔄 Data Flow - Integrated Pipeline

```
┌──────────────────────────────────────────────────────────────────┐
│                    Document Processing Pipeline                   │
└──────────────────────────────────────────────────────────────────┘

Input Documents
    │
    ├─► invoice.pdf
    ├─► receipt.jpg
    └─► id_card.png
    
    ▼
┌─────────────────────────────────┐
│  Azure Document Intelligence    │
│                                  │
│  • Extract structured data       │
│  • OCR text recognition          │
│  • Table extraction              │
│  • Key-value pairs               │
└─────────────────────────────────┘
    │
    ▼
Extracted Data
    │
    ├─► Vendor Name
    ├─► Invoice Total
    ├─► Line Items
    ├─► Dates
    └─► Full Text Content
    
    ▼
┌─────────────────────────────────┐
│  Data Transformation             │
│                                  │
│  • Format for indexing           │
│  • Generate unique IDs           │
│  • Add metadata                  │
│  • Prepare searchable fields     │
└─────────────────────────────────┘
    │
    ▼
Indexed Document
    │
    ├─► id: "abc123"
    ├─► document_type: "invoice"
    ├─► vendor_name: "Acme Corp"
    ├─► total_amount: 1500.00
    ├─► content: "Full text..."
    └─► tags: ["invoice", "processed"]
    
    ▼
┌─────────────────────────────────┐
│  Azure AI Search                 │
│                                  │
│  • Index document                │
│  • Enable semantic search        │
│  • Configure facets              │
│  • Set up filters                │
└─────────────────────────────────┘
    │
    ▼
Searchable Index
    │
    └─► Ready for queries
    
    ▼
┌─────────────────────────────────┐
│  Search & Retrieval              │
│                                  │
│  • Full-text search              │
│  • Semantic search               │
│  • Filtered queries              │
│  • Faceted navigation            │
└─────────────────────────────────┘
    │
    ▼
Search Results
    │
    ├─► Ranked by relevance
    ├─► Semantic scoring
    ├─► Highlighted matches
    └─► Facet counts
```

## 🎯 Component Relationships

```
┌────────────────────────────────────────────────────────────────┐
│                      Test Components                            │
└────────────────────────────────────────────────────────────────┘

azure-ai-services.test.ts
    │
    ├─► Uses: VSCodeElectron
    │   └─► Methods:
    │       ├─► launch()
    │       ├─► createNewFile()
    │       ├─► typeInEditor()
    │       ├─► saveFile()
    │       └─► takeScreenshot()
    │
    ├─► Uses: TestHelpers
    │   └─► Methods:
    │       ├─► logStep()
    │       ├─► wait()
    │       ├─► createTestWorkspace()
    │       ├─► cleanupTempFiles()
    │       └─► getWorkspaceRoot()
    │
    └─► Generates: Python Scripts
        │
        ├─► azure_search_indexer.py
        │   └─► Uses: azure-search-documents SDK
        │
        ├─► document_intelligence_analyzer.py
        │   └─► Uses: azure-ai-formrecognizer SDK
        │
        └─► integrated_document_pipeline.py
            └─► Uses: Both SDKs
```

## 📦 File Dependencies

```
Project Structure
│
├─── NewFramework/
│    │
│    ├─── Config/
│    │    └─── appsettings.json ◄─── Configuration
│    │
│    └─── ElectronTests/
│         │
│         ├─── tests/
│         │    ├─── azure-ai-services.test.ts ◄─── Main Test
│         │    └─── AZURE_AI_SERVICES_README.md ◄─── Docs
│         │
│         ├─── utils/
│         │    ├─── vscode-electron.ts ◄─── VS Code Control
│         │    └─── test-helpers.ts ◄─── Helper Functions
│         │
│         ├─── playwright.config.ts ◄─── Test Config
│         │
│         └─── Generated Files (during test run):
│              ├─── azure-search-config.json
│              ├─── azure_search_indexer.py
│              ├─── azure_search_queries.py
│              ├─── document-intelligence-config.json
│              ├─── document_intelligence_analyzer.py
│              ├─── custom_model_trainer.py
│              ├─── integrated_document_pipeline.py
│              └─── azure-ai-requirements.txt
```

## 🔐 Authentication Flow

```
┌────────────────────────────────────────────────────────────────┐
│                    Authentication Methods                       │
└────────────────────────────────────────────────────────────────┘

Option 1: API Key Authentication (Used in tests)
    │
    ├─► appsettings.json
    │   └─► ApiKey: "your-api-key"
    │
    └─► Python Script
        └─► AzureKeyCredential(api_key)

Option 2: Azure AD Authentication (Production)
    │
    ├─► Azure Identity
    │   └─► DefaultAzureCredential()
    │
    └─► Supports:
        ├─► Managed Identity
        ├─► Service Principal
        ├─► Azure CLI
        └─► Interactive Browser

Option 3: Environment Variables
    │
    ├─► .env file
    │   ├─► AZURE_SEARCH_API_KEY
    │   └─► AZURE_DOC_INTEL_API_KEY
    │
    └─► Load with python-dotenv
```

## 🎬 Test Execution Sequence

```
┌────────────────────────────────────────────────────────────────┐
│                    Execution Timeline                           │
└────────────────────────────────────────────────────────────────┘

Time    Action                              Component
────────────────────────────────────────────────────────────────
0s      Start Test Suite                    Playwright
1s      Launch VS Code                      VSCodeElectron
3s      Setup Test Environment              TestHelpers
5s      Create Test Workspace               TestHelpers

        ┌─── Azure AI Search Tests ───┐
10s     Create Search Config            Test 1
15s     Generate Indexer Script         Test 2
25s     Generate Query Examples         Test 3

        ┌─── Document Intelligence Tests ───┐
35s     Create Doc Intel Config         Test 4
40s     Generate Analyzer Script        Test 5
55s     Generate Training Script        Test 6

        ┌─── Integration Tests ───┐
70s     Generate Pipeline Script        Test 7
85s     Generate Requirements           Test 8
90s     Verify Files Created            Test 9

95s     Cleanup Test Environment        TestHelpers
96s     Close VS Code                   VSCodeElectron
97s     Generate Test Report            Playwright
98s     Complete                        ✅
```

## 💾 Data Models

```
┌────────────────────────────────────────────────────────────────┐
│                    Search Index Schema                          │
└────────────────────────────────────────────────────────────────┘

Document {
    id: string (key)
    title: string (searchable, filterable)
    content: string (searchable)
    category: string (filterable, facetable)
    created_date: datetime (filterable, sortable)
    tags: string[] (filterable, facetable)
}

┌────────────────────────────────────────────────────────────────┐
│                    Invoice Data Model                           │
└────────────────────────────────────────────────────────────────┘

Invoice {
    vendor_name: string
    vendor_address: string
    customer_name: string
    invoice_id: string
    invoice_date: date
    due_date: date
    subtotal: float
    tax: float
    total: float
    line_items: LineItem[]
}

LineItem {
    description: string
    quantity: float
    unit_price: float
    amount: float
}

┌────────────────────────────────────────────────────────────────┐
│                    Receipt Data Model                           │
└────────────────────────────────────────────────────────────────┘

Receipt {
    merchant_name: string
    merchant_address: string
    transaction_date: date
    transaction_time: time
    items: ReceiptItem[]
    subtotal: float
    tax: float
    tip: float
    total: float
}
```

## 🔧 Configuration Hierarchy

```
┌────────────────────────────────────────────────────────────────┐
│                    Configuration Layers                         │
└────────────────────────────────────────────────────────────────┘

1. Global Config (appsettings.json)
   │
   ├─► Environment: dev/qa/prod
   ├─► Azure Credentials
   ├─► Service Endpoints
   └─► Test Settings

2. Test Config (playwright.config.ts)
   │
   ├─► Test Directory
   ├─► Timeouts
   ├─► Reporters
   └─► Browser Settings

3. Service Config (Generated JSON files)
   │
   ├─► azure-search-config.json
   │   ├─► Index Schema
   │   ├─► Field Definitions
   │   └─► Semantic Config
   │
   └─► document-intelligence-config.json
       ├─► Model Selection
       ├─► Processing Options
       └─► Output Format

4. Runtime Config (Environment Variables)
   │
   ├─► AZURE_SEARCH_ENDPOINT
   ├─► AZURE_SEARCH_API_KEY
   ├─► AZURE_DOC_INTEL_ENDPOINT
   └─► AZURE_DOC_INTEL_API_KEY
```

## 📈 Scalability Considerations

```
┌────────────────────────────────────────────────────────────────┐
│                    Scaling Strategy                             │
└────────────────────────────────────────────────────────────────┘

Horizontal Scaling
    │
    ├─► Multiple Search Replicas
    │   └─► Increase query throughput
    │
    ├─► Multiple Search Partitions
    │   └─► Increase index size capacity
    │
    └─► Parallel Document Processing
        └─► Process multiple documents simultaneously

Vertical Scaling
    │
    ├─► Upgrade Search Service Tier
    │   └─► More resources per instance
    │
    └─► Upgrade Document Intelligence Tier
        └─► Higher transaction limits

Optimization
    │
    ├─► Batch Operations
    │   └─► Index multiple documents at once
    │
    ├─► Caching
    │   └─► Cache frequent queries
    │
    └─► Async Processing
        └─► Non-blocking operations
```

## 🎓 Learning Path

```
┌────────────────────────────────────────────────────────────────┐
│                    Recommended Learning Path                    │
└────────────────────────────────────────────────────────────────┘

Level 1: Basics
    │
    ├─► Run tests and observe
    ├─► Review generated scripts
    └─► Understand basic concepts

Level 2: Intermediate
    │
    ├─► Modify test parameters
    ├─► Customize generated scripts
    └─► Add new test cases

Level 3: Advanced
    │
    ├─► Implement custom models
    ├─► Optimize search performance
    └─► Build production pipelines

Level 4: Expert
    │
    ├─► Design complex workflows
    ├─► Implement advanced features
    └─► Contribute improvements
```

---

**Architecture Version:** 1.0.0  
**Last Updated:** 2024  
**Complexity:** Intermediate to Advanced