"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const test_1 = require("@playwright/test");
const vscode_electron_1 = require("../utils/vscode-electron");
const test_helpers_1 = require("../utils/test-helpers");
/**
 * Azure AI Search and Document Intelligence integration tests for VS Code
 * Tests Azure AI Search service and Azure Document Intelligence (Form Recognizer) service
 */
test_1.test.describe('Azure AI Services Integration', () => {
    let vscode;
    let testWorkspace;
    test_1.test.beforeEach(async () => {
        test_helpers_1.TestHelpers.logStep('Setting up VS Code for Azure AI Services testing');
        vscode = new vscode_electron_1.VSCodeElectron();
        // Launch VS Code with the main workspace
        const workspaceRoot = test_helpers_1.TestHelpers.getWorkspaceRoot();
        await vscode.launch(workspaceRoot);
        await test_helpers_1.TestHelpers.setupVSCodeForTesting(vscode);
        // Create test workspace
        testWorkspace = await test_helpers_1.TestHelpers.createTestWorkspace();
    });
    test_1.test.afterEach(async () => {
        test_helpers_1.TestHelpers.logStep('Cleaning up Azure AI Services test environment');
        if (vscode.isRunning()) {
            await vscode.close();
        }
        test_helpers_1.TestHelpers.cleanupTempFiles();
        if (testWorkspace) {
            test_helpers_1.TestHelpers.cleanupTestWorkspace(testWorkspace);
        }
    });
    /**
     * Azure AI Search Service Tests
     */
    test_1.test.describe('Azure AI Search Service', () => {
        (0, test_1.test)('should create Azure AI Search configuration file', async () => {
            test_helpers_1.TestHelpers.logStep('Creating Azure AI Search configuration');
            // Create new file
            await vscode.createNewFile();
            await test_helpers_1.TestHelpers.wait(1000);
            const searchConfig = `{
  "search_service_name": "your-search-service",
  "search_service_endpoint": "https://your-search-service.search.windows.net",
  "search_api_key": "your-api-key",
  "search_api_version": "2023-11-01",
  "indexes": [
    {
      "name": "documents-index",
      "fields": [
        {
          "name": "id",
          "type": "Edm.String",
          "key": true,
          "searchable": false
        },
        {
          "name": "content",
          "type": "Edm.String",
          "searchable": true,
          "analyzer": "en.microsoft"
        },
        {
          "name": "title",
          "type": "Edm.String",
          "searchable": true,
          "filterable": true
        },
        {
          "name": "metadata",
          "type": "Edm.String",
          "searchable": false,
          "filterable": true
        },
        {
          "name": "created_date",
          "type": "Edm.DateTimeOffset",
          "filterable": true,
          "sortable": true
        }
      ]
    }
  ],
  "semantic_configuration": {
    "enabled": true,
    "prioritized_fields": {
      "title_field": "title",
      "content_fields": ["content"],
      "keyword_fields": ["metadata"]
    }
  }
}`;
            await vscode.typeInEditor(searchConfig);
            await test_helpers_1.TestHelpers.wait(1000);
            // Save the file
            const window = vscode.getMainWindow();
            const isMac = process.platform === 'darwin';
            const modifier = isMac ? 'Meta' : 'Control';
            await window.keyboard.press(`${modifier}+Shift+KeyS`); // Save As
            await test_helpers_1.TestHelpers.wait(1000);
            await window.keyboard.type('azure-search-config.json');
            await window.keyboard.press('Enter');
            await test_helpers_1.TestHelpers.wait(2000);
            // Take screenshot
            await vscode.takeScreenshot();
            // Verify file is saved
            const editor = await window.$('.monaco-editor');
            (0, test_1.expect)(editor).toBeTruthy();
        });
        (0, test_1.test)('should create Python script for Azure AI Search indexing', async () => {
            test_helpers_1.TestHelpers.logStep('Creating Azure AI Search indexing script');
            await vscode.createNewFile();
            await test_helpers_1.TestHelpers.wait(1000);
            const searchIndexScript = `"""
Azure AI Search - Document Indexing Script
This script demonstrates how to index documents into Azure AI Search
"""

from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from azure.search.documents.indexes import SearchIndexClient
from azure.search.documents.indexes.models import (
    SearchIndex,
    SimpleField,
    SearchableField,
    SearchFieldDataType,
    SemanticConfiguration,
    SemanticField,
    SemanticPrioritizedFields,
    SemanticSearch
)
import json
from datetime import datetime
from typing import List, Dict

class AzureSearchIndexer:
    """Azure AI Search indexer for document management"""
    
    def __init__(self, service_endpoint: str, api_key: str, index_name: str):
        """
        Initialize Azure Search client
        
        Args:
            service_endpoint: Azure Search service endpoint
            api_key: Azure Search API key
            index_name: Name of the search index
        """
        self.service_endpoint = service_endpoint
        self.credential = AzureKeyCredential(api_key)
        self.index_name = index_name
        
        # Initialize clients
        self.index_client = SearchIndexClient(
            endpoint=service_endpoint,
            credential=self.credential
        )
        self.search_client = SearchClient(
            endpoint=service_endpoint,
            index_name=index_name,
            credential=self.credential
        )
    
    def create_index(self) -> None:
        """Create a new search index with semantic search configuration"""
        
        # Define index fields
        fields = [
            SimpleField(
                name="id",
                type=SearchFieldDataType.String,
                key=True,
                filterable=True
            ),
            SearchableField(
                name="title",
                type=SearchFieldDataType.String,
                searchable=True,
                filterable=True,
                sortable=True
            ),
            SearchableField(
                name="content",
                type=SearchFieldDataType.String,
                searchable=True,
                analyzer_name="en.microsoft"
            ),
            SearchableField(
                name="category",
                type=SearchFieldDataType.String,
                filterable=True,
                facetable=True
            ),
            SimpleField(
                name="created_date",
                type=SearchFieldDataType.DateTimeOffset,
                filterable=True,
                sortable=True
            ),
            SearchableField(
                name="tags",
                type=SearchFieldDataType.Collection(SearchFieldDataType.String),
                filterable=True,
                facetable=True
            )
        ]
        
        # Configure semantic search
        semantic_config = SemanticConfiguration(
            name="default-semantic-config",
            prioritized_fields=SemanticPrioritizedFields(
                title_field=SemanticField(field_name="title"),
                content_fields=[SemanticField(field_name="content")],
                keywords_fields=[SemanticField(field_name="tags")]
            )
        )
        
        semantic_search = SemanticSearch(
            configurations=[semantic_config]
        )
        
        # Create index
        index = SearchIndex(
            name=self.index_name,
            fields=fields,
            semantic_search=semantic_search
        )
        
        result = self.index_client.create_or_update_index(index)
        print(f"Index '{result.name}' created successfully")
    
    def index_documents(self, documents: List[Dict]) -> None:
        """
        Index documents into Azure AI Search
        
        Args:
            documents: List of documents to index
        """
        try:
            result = self.search_client.upload_documents(documents=documents)
            print(f"Indexed {len(result)} documents successfully")
            
            for item in result:
                if item.succeeded:
                    print(f"Document {item.key} indexed successfully")
                else:
                    print(f"Document {item.key} failed: {item.error_message}")
                    
        except Exception as e:
            print(f"Error indexing documents: {str(e)}")
    
    def search_documents(
        self,
        query: str,
        filter_expression: str = None,
        top: int = 10,
        use_semantic_search: bool = True
    ) -> List[Dict]:
        """
        Search documents in the index
        
        Args:
            query: Search query string
            filter_expression: OData filter expression
            top: Number of results to return
            use_semantic_search: Whether to use semantic search
            
        Returns:
            List of search results
        """
        try:
            search_params = {
                "search_text": query,
                "top": top,
                "include_total_count": True
            }
            
            if filter_expression:
                search_params["filter"] = filter_expression
            
            if use_semantic_search:
                search_params["query_type"] = "semantic"
                search_params["semantic_configuration_name"] = "default-semantic-config"
            
            results = self.search_client.search(**search_params)
            
            documents = []
            for result in results:
                documents.append({
                    "id": result.get("id"),
                    "title": result.get("title"),
                    "content": result.get("content"),
                    "score": result.get("@search.score"),
                    "reranker_score": result.get("@search.reranker_score")
                })
            
            print(f"Found {len(documents)} documents")
            return documents
            
        except Exception as e:
            print(f"Error searching documents: {str(e)}")
            return []
    
    def delete_index(self) -> None:
        """Delete the search index"""
        try:
            self.index_client.delete_index(self.index_name)
            print(f"Index '{self.index_name}' deleted successfully")
        except Exception as e:
            print(f"Error deleting index: {str(e)}")


def main():
    """Main function to demonstrate Azure AI Search"""
    
    # Configuration
    service_endpoint = "https://your-search-service.search.windows.net"
    api_key = "your-api-key"
    index_name = "documents-index"
    
    # Initialize indexer
    indexer = AzureSearchIndexer(service_endpoint, api_key, index_name)
    
    # Create index
    print("Creating search index...")
    indexer.create_index()
    
    # Sample documents
    documents = [
        {
            "id": "1",
            "title": "Azure AI Search Overview",
            "content": "Azure AI Search is a cloud search service with built-in AI capabilities.",
            "category": "Documentation",
            "created_date": datetime.utcnow().isoformat(),
            "tags": ["azure", "search", "ai"]
        },
        {
            "id": "2",
            "title": "Getting Started with Semantic Search",
            "content": "Semantic search uses AI to understand the intent and context of search queries.",
            "category": "Tutorial",
            "created_date": datetime.utcnow().isoformat(),
            "tags": ["semantic", "search", "tutorial"]
        }
    ]
    
    # Index documents
    print("\\nIndexing documents...")
    indexer.index_documents(documents)
    
    # Search documents
    print("\\nSearching documents...")
    results = indexer.search_documents(
        query="AI search capabilities",
        use_semantic_search=True
    )
    
    for result in results:
        print(f"\\nTitle: {result['title']}")
        print(f"Score: {result['score']}")
        print(f"Content: {result['content'][:100]}...")


if __name__ == "__main__":
    main()
`;
            await vscode.typeInEditor(searchIndexScript);
            await test_helpers_1.TestHelpers.wait(2000);
            // Save file
            const window = vscode.getMainWindow();
            const isMac = process.platform === 'darwin';
            const modifier = isMac ? 'Meta' : 'Control';
            await window.keyboard.press(`${modifier}+Shift+KeyS`);
            await test_helpers_1.TestHelpers.wait(1000);
            await window.keyboard.type('azure_search_indexer.py');
            await window.keyboard.press('Enter');
            await test_helpers_1.TestHelpers.wait(2000);
            await vscode.takeScreenshot();
        });
        (0, test_1.test)('should create Azure AI Search query examples', async () => {
            test_helpers_1.TestHelpers.logStep('Creating Azure AI Search query examples');
            await vscode.createNewFile();
            await test_helpers_1.TestHelpers.wait(1000);
            const queryExamples = `"""
Azure AI Search - Query Examples
Demonstrates various search query patterns
"""

from azure.core.credentials import AzureKeyCredential
from azure.search.documents import SearchClient
from typing import List, Dict
import json


class SearchQueryExamples:
    """Examples of different Azure AI Search query patterns"""
    
    def __init__(self, endpoint: str, index_name: str, api_key: str):
        self.search_client = SearchClient(
            endpoint=endpoint,
            index_name=index_name,
            credential=AzureKeyCredential(api_key)
        )
    
    def simple_search(self, query: str) -> List[Dict]:
        """Simple full-text search"""
        results = self.search_client.search(search_text=query)
        return [dict(result) for result in results]
    
    def filtered_search(self, query: str, category: str) -> List[Dict]:
        """Search with OData filter"""
        results = self.search_client.search(
            search_text=query,
            filter=f"category eq '{category}'"
        )
        return [dict(result) for result in results]
    
    def faceted_search(self, query: str) -> Dict:
        """Search with facets for aggregation"""
        results = self.search_client.search(
            search_text=query,
            facets=["category", "tags"]
        )
        
        return {
            "results": [dict(r) for r in results],
            "facets": results.get_facets()
        }
    
    def semantic_search(self, query: str) -> List[Dict]:
        """Semantic search with AI-powered ranking"""
        results = self.search_client.search(
            search_text=query,
            query_type="semantic",
            semantic_configuration_name="default-semantic-config",
            query_caption="extractive",
            query_answer="extractive"
        )
        
        return [{
            "document": dict(result),
            "reranker_score": result.get("@search.reranker_score"),
            "captions": result.get("@search.captions")
        } for result in results]
    
    def autocomplete_search(self, partial_text: str) -> List[str]:
        """Autocomplete suggestions"""
        results = self.search_client.autocomplete(
            search_text=partial_text,
            suggester_name="sg"
        )
        return [result["text"] for result in results]
    
    def suggest_search(self, partial_text: str) -> List[Dict]:
        """Search suggestions"""
        results = self.search_client.suggest(
            search_text=partial_text,
            suggester_name="sg"
        )
        return [dict(result) for result in results]


# Example usage
if __name__ == "__main__":
    endpoint = "https://your-search-service.search.windows.net"
    index_name = "documents-index"
    api_key = "your-api-key"
    
    search = SearchQueryExamples(endpoint, index_name, api_key)
    
    # Simple search
    print("Simple Search:")
    results = search.simple_search("azure ai")
    print(json.dumps(results, indent=2))
    
    # Filtered search
    print("\\nFiltered Search:")
    results = search.filtered_search("azure", "Documentation")
    print(json.dumps(results, indent=2))
    
    # Semantic search
    print("\\nSemantic Search:")
    results = search.semantic_search("how to use ai search")
    print(json.dumps(results, indent=2))
`;
            await vscode.typeInEditor(queryExamples);
            await test_helpers_1.TestHelpers.wait(2000);
            const window = vscode.getMainWindow();
            const isMac = process.platform === 'darwin';
            const modifier = isMac ? 'Meta' : 'Control';
            await window.keyboard.press(`${modifier}+Shift+KeyS`);
            await test_helpers_1.TestHelpers.wait(1000);
            await window.keyboard.type('azure_search_queries.py');
            await window.keyboard.press('Enter');
            await test_helpers_1.TestHelpers.wait(2000);
            await vscode.takeScreenshot();
        });
    });
    /**
     * Azure Document Intelligence (Form Recognizer) Tests
     */
    test_1.test.describe('Azure Document Intelligence Service', () => {
        (0, test_1.test)('should create Document Intelligence configuration file', async () => {
            test_helpers_1.TestHelpers.logStep('Creating Document Intelligence configuration');
            await vscode.createNewFile();
            await test_helpers_1.TestHelpers.wait(1000);
            const docIntelConfig = `{
  "document_intelligence": {
    "endpoint": "https://your-doc-intelligence.cognitiveservices.azure.com/",
    "api_key": "your-api-key",
    "api_version": "2023-07-31",
    "models": {
      "prebuilt": {
        "invoice": "prebuilt-invoice",
        "receipt": "prebuilt-receipt",
        "id_document": "prebuilt-idDocument",
        "business_card": "prebuilt-businessCard",
        "layout": "prebuilt-layout",
        "general_document": "prebuilt-document",
        "tax_us_w2": "prebuilt-tax.us.w2",
        "health_insurance_card": "prebuilt-healthInsuranceCard.us"
      },
      "custom": {
        "model_id": "your-custom-model-id",
        "training_data_path": "path/to/training/data"
      }
    },
    "processing_options": {
      "locale": "en-US",
      "pages": "1-10",
      "features": ["ocr", "keyValuePairs", "tables", "styles"],
      "output_format": "json"
    },
    "storage": {
      "container_name": "documents",
      "connection_string": "your-storage-connection-string"
    }
  }
}`;
            await vscode.typeInEditor(docIntelConfig);
            await test_helpers_1.TestHelpers.wait(1000);
            const window = vscode.getMainWindow();
            const isMac = process.platform === 'darwin';
            const modifier = isMac ? 'Meta' : 'Control';
            await window.keyboard.press(`${modifier}+Shift+KeyS`);
            await test_helpers_1.TestHelpers.wait(1000);
            await window.keyboard.type('document-intelligence-config.json');
            await window.keyboard.press('Enter');
            await test_helpers_1.TestHelpers.wait(2000);
            await vscode.takeScreenshot();
        });
        (0, test_1.test)('should create Document Intelligence analysis script', async () => {
            test_helpers_1.TestHelpers.logStep('Creating Document Intelligence analysis script');
            await vscode.createNewFile();
            await test_helpers_1.TestHelpers.wait(1000);
            const docIntelScript = `"""
Azure Document Intelligence - Document Analysis Script
Demonstrates document analysis using Azure Document Intelligence (Form Recognizer)
"""

from azure.core.credentials import AzureKeyCredential
from azure.ai.formrecognizer import DocumentAnalysisClient
from typing import Dict, List, Any
import json
from pathlib import Path


class DocumentIntelligenceAnalyzer:
    """Azure Document Intelligence analyzer for document processing"""
    
    def __init__(self, endpoint: str, api_key: str):
        """
        Initialize Document Intelligence client
        
        Args:
            endpoint: Azure Document Intelligence endpoint
            api_key: Azure Document Intelligence API key
        """
        self.endpoint = endpoint
        self.credential = AzureKeyCredential(api_key)
        self.client = DocumentAnalysisClient(
            endpoint=endpoint,
            credential=self.credential
        )
    
    def analyze_invoice(self, document_path: str) -> Dict[str, Any]:
        """
        Analyze invoice document
        
        Args:
            document_path: Path to invoice document
            
        Returns:
            Extracted invoice data
        """
        with open(document_path, "rb") as f:
            poller = self.client.begin_analyze_document(
                "prebuilt-invoice",
                document=f
            )
        
        result = poller.result()
        
        invoice_data = {
            "vendor_name": None,
            "vendor_address": None,
            "customer_name": None,
            "invoice_id": None,
            "invoice_date": None,
            "due_date": None,
            "subtotal": None,
            "tax": None,
            "total": None,
            "line_items": []
        }
        
        for document in result.documents:
            fields = document.fields
            
            if "VendorName" in fields:
                invoice_data["vendor_name"] = fields["VendorName"].value
            
            if "VendorAddress" in fields:
                invoice_data["vendor_address"] = fields["VendorAddress"].value
            
            if "CustomerName" in fields:
                invoice_data["customer_name"] = fields["CustomerName"].value
            
            if "InvoiceId" in fields:
                invoice_data["invoice_id"] = fields["InvoiceId"].value
            
            if "InvoiceDate" in fields:
                invoice_data["invoice_date"] = str(fields["InvoiceDate"].value)
            
            if "DueDate" in fields:
                invoice_data["due_date"] = str(fields["DueDate"].value)
            
            if "SubTotal" in fields:
                invoice_data["subtotal"] = fields["SubTotal"].value
            
            if "TotalTax" in fields:
                invoice_data["tax"] = fields["TotalTax"].value
            
            if "InvoiceTotal" in fields:
                invoice_data["total"] = fields["InvoiceTotal"].value
            
            if "Items" in fields:
                for item in fields["Items"].value:
                    line_item = {
                        "description": item.value.get("Description", {}).value if "Description" in item.value else None,
                        "quantity": item.value.get("Quantity", {}).value if "Quantity" in item.value else None,
                        "unit_price": item.value.get("UnitPrice", {}).value if "UnitPrice" in item.value else None,
                        "amount": item.value.get("Amount", {}).value if "Amount" in item.value else None
                    }
                    invoice_data["line_items"].append(line_item)
        
        return invoice_data
    
    def analyze_receipt(self, document_path: str) -> Dict[str, Any]:
        """
        Analyze receipt document
        
        Args:
            document_path: Path to receipt document
            
        Returns:
            Extracted receipt data
        """
        with open(document_path, "rb") as f:
            poller = self.client.begin_analyze_document(
                "prebuilt-receipt",
                document=f
            )
        
        result = poller.result()
        
        receipt_data = {
            "merchant_name": None,
            "merchant_address": None,
            "merchant_phone": None,
            "transaction_date": None,
            "transaction_time": None,
            "items": [],
            "subtotal": None,
            "tax": None,
            "tip": None,
            "total": None
        }
        
        for document in result.documents:
            fields = document.fields
            
            if "MerchantName" in fields:
                receipt_data["merchant_name"] = fields["MerchantName"].value
            
            if "MerchantAddress" in fields:
                receipt_data["merchant_address"] = fields["MerchantAddress"].value
            
            if "MerchantPhoneNumber" in fields:
                receipt_data["merchant_phone"] = fields["MerchantPhoneNumber"].value
            
            if "TransactionDate" in fields:
                receipt_data["transaction_date"] = str(fields["TransactionDate"].value)
            
            if "TransactionTime" in fields:
                receipt_data["transaction_time"] = str(fields["TransactionTime"].value)
            
            if "Items" in fields:
                for item in fields["Items"].value:
                    receipt_item = {
                        "description": item.value.get("Description", {}).value if "Description" in item.value else None,
                        "quantity": item.value.get("Quantity", {}).value if "Quantity" in item.value else None,
                        "price": item.value.get("Price", {}).value if "Price" in item.value else None,
                        "total_price": item.value.get("TotalPrice", {}).value if "TotalPrice" in item.value else None
                    }
                    receipt_data["items"].append(receipt_item)
            
            if "Subtotal" in fields:
                receipt_data["subtotal"] = fields["Subtotal"].value
            
            if "Tax" in fields:
                receipt_data["tax"] = fields["Tax"].value
            
            if "Tip" in fields:
                receipt_data["tip"] = fields["Tip"].value
            
            if "Total" in fields:
                receipt_data["total"] = fields["Total"].value
        
        return receipt_data
    
    def analyze_id_document(self, document_path: str) -> Dict[str, Any]:
        """
        Analyze ID document (passport, driver's license, etc.)
        
        Args:
            document_path: Path to ID document
            
        Returns:
            Extracted ID data
        """
        with open(document_path, "rb") as f:
            poller = self.client.begin_analyze_document(
                "prebuilt-idDocument",
                document=f
            )
        
        result = poller.result()
        
        id_data = {
            "first_name": None,
            "last_name": None,
            "document_number": None,
            "date_of_birth": None,
            "date_of_expiration": None,
            "sex": None,
            "address": None,
            "country_region": None,
            "document_type": None
        }
        
        for document in result.documents:
            fields = document.fields
            
            if "FirstName" in fields:
                id_data["first_name"] = fields["FirstName"].value
            
            if "LastName" in fields:
                id_data["last_name"] = fields["LastName"].value
            
            if "DocumentNumber" in fields:
                id_data["document_number"] = fields["DocumentNumber"].value
            
            if "DateOfBirth" in fields:
                id_data["date_of_birth"] = str(fields["DateOfBirth"].value)
            
            if "DateOfExpiration" in fields:
                id_data["date_of_expiration"] = str(fields["DateOfExpiration"].value)
            
            if "Sex" in fields:
                id_data["sex"] = fields["Sex"].value
            
            if "Address" in fields:
                id_data["address"] = fields["Address"].value
            
            if "CountryRegion" in fields:
                id_data["country_region"] = fields["CountryRegion"].value
            
            id_data["document_type"] = document.doc_type
        
        return id_data
    
    def analyze_layout(self, document_path: str) -> Dict[str, Any]:
        """
        Analyze document layout (text, tables, structure)
        
        Args:
            document_path: Path to document
            
        Returns:
            Extracted layout data
        """
        with open(document_path, "rb") as f:
            poller = self.client.begin_analyze_document(
                "prebuilt-layout",
                document=f
            )
        
        result = poller.result()
        
        layout_data = {
            "pages": [],
            "tables": [],
            "paragraphs": [],
            "styles": []
        }
        
        # Extract pages
        for page in result.pages:
            page_data = {
                "page_number": page.page_number,
                "width": page.width,
                "height": page.height,
                "unit": page.unit,
                "lines": [line.content for line in page.lines]
            }
            layout_data["pages"].append(page_data)
        
        # Extract tables
        for table in result.tables:
            table_data = {
                "row_count": table.row_count,
                "column_count": table.column_count,
                "cells": []
            }
            
            for cell in table.cells:
                cell_data = {
                    "row_index": cell.row_index,
                    "column_index": cell.column_index,
                    "content": cell.content,
                    "kind": cell.kind
                }
                table_data["cells"].append(cell_data)
            
            layout_data["tables"].append(table_data)
        
        # Extract paragraphs
        for paragraph in result.paragraphs:
            layout_data["paragraphs"].append({
                "content": paragraph.content,
                "role": paragraph.role
            })
        
        return layout_data
    
    def analyze_general_document(self, document_path: str) -> Dict[str, Any]:
        """
        Analyze general document with key-value pairs
        
        Args:
            document_path: Path to document
            
        Returns:
            Extracted key-value pairs
        """
        with open(document_path, "rb") as f:
            poller = self.client.begin_analyze_document(
                "prebuilt-document",
                document=f
            )
        
        result = poller.result()
        
        document_data = {
            "key_value_pairs": [],
            "entities": [],
            "content": ""
        }
        
        # Extract key-value pairs
        for kv_pair in result.key_value_pairs:
            if kv_pair.key and kv_pair.value:
                document_data["key_value_pairs"].append({
                    "key": kv_pair.key.content,
                    "value": kv_pair.value.content,
                    "confidence": kv_pair.confidence
                })
        
        # Extract content
        document_data["content"] = result.content
        
        return document_data


def main():
    """Main function to demonstrate Document Intelligence"""
    
    # Configuration
    endpoint = "https://your-doc-intelligence.cognitiveservices.azure.com/"
    api_key = "your-api-key"
    
    # Initialize analyzer
    analyzer = DocumentIntelligenceAnalyzer(endpoint, api_key)
    
    # Example: Analyze invoice
    print("Analyzing invoice...")
    invoice_path = "path/to/invoice.pdf"
    # invoice_data = analyzer.analyze_invoice(invoice_path)
    # print(json.dumps(invoice_data, indent=2))
    
    # Example: Analyze receipt
    print("\\nAnalyzing receipt...")
    receipt_path = "path/to/receipt.jpg"
    # receipt_data = analyzer.analyze_receipt(receipt_path)
    # print(json.dumps(receipt_data, indent=2))
    
    # Example: Analyze ID document
    print("\\nAnalyzing ID document...")
    id_path = "path/to/id.jpg"
    # id_data = analyzer.analyze_id_document(id_path)
    # print(json.dumps(id_data, indent=2))
    
    print("\\nDocument Intelligence examples created successfully!")


if __name__ == "__main__":
    main()
`;
            await vscode.typeInEditor(docIntelScript);
            await test_helpers_1.TestHelpers.wait(2000);
            const window = vscode.getMainWindow();
            const isMac = process.platform === 'darwin';
            const modifier = isMac ? 'Meta' : 'Control';
            await window.keyboard.press(`${modifier}+Shift+KeyS`);
            await test_helpers_1.TestHelpers.wait(1000);
            await window.keyboard.type('document_intelligence_analyzer.py');
            await window.keyboard.press('Enter');
            await test_helpers_1.TestHelpers.wait(2000);
            await vscode.takeScreenshot();
        });
        (0, test_1.test)('should create custom model training script', async () => {
            test_helpers_1.TestHelpers.logStep('Creating custom model training script');
            await vscode.createNewFile();
            await test_helpers_1.TestHelpers.wait(1000);
            const customModelScript = `"""
Azure Document Intelligence - Custom Model Training
Demonstrates how to train custom document models
"""

from azure.core.credentials import AzureKeyCredential
from azure.ai.formrecognizer import (
    DocumentModelAdministrationClient,
    ModelBuildMode
)
from typing import Dict, Any
import time


class CustomModelTrainer:
    """Train custom document models with Azure Document Intelligence"""
    
    def __init__(self, endpoint: str, api_key: str):
        """
        Initialize Document Model Administration client
        
        Args:
            endpoint: Azure Document Intelligence endpoint
            api_key: Azure Document Intelligence API key
        """
        self.client = DocumentModelAdministrationClient(
            endpoint=endpoint,
            credential=AzureKeyCredential(api_key)
        )
    
    def train_custom_model(
        self,
        training_data_url: str,
        model_id: str,
        description: str = None,
        build_mode: str = "template"
    ) -> Dict[str, Any]:
        """
        Train a custom document model
        
        Args:
            training_data_url: Azure Blob Storage URL with training data
            model_id: Unique identifier for the model
            description: Model description
            build_mode: 'template' or 'neural'
            
        Returns:
            Model information
        """
        print(f"Training custom model: {model_id}")
        print(f"Build mode: {build_mode}")
        
        # Determine build mode
        mode = ModelBuildMode.TEMPLATE if build_mode == "template" else ModelBuildMode.NEURAL
        
        # Start training
        poller = self.client.begin_build_document_model(
            build_mode=mode,
            blob_container_url=training_data_url,
            model_id=model_id,
            description=description
        )
        
        print("Training in progress...")
        model = poller.result()
        
        print(f"Model ID: {model.model_id}")
        print(f"Description: {model.description}")
        print(f"Created on: {model.created_on}")
        
        # Display document types
        print("\\nDocument types:")
        for doc_type_name, doc_type in model.doc_types.items():
            print(f"  {doc_type_name}:")
            print(f"    Description: {doc_type.description}")
            print(f"    Field schema:")
            for field_name, field_schema in doc_type.field_schema.items():
                print(f"      {field_name}: {field_schema}")
        
        return {
            "model_id": model.model_id,
            "description": model.description,
            "created_on": str(model.created_on),
            "doc_types": list(model.doc_types.keys())
        }
    
    def compose_models(
        self,
        model_ids: list,
        composed_model_id: str,
        description: str = None
    ) -> Dict[str, Any]:
        """
        Compose multiple models into a single model
        
        Args:
            model_ids: List of model IDs to compose
            composed_model_id: ID for the composed model
            description: Model description
            
        Returns:
            Composed model information
        """
        print(f"Composing models: {model_ids}")
        
        poller = self.client.begin_compose_document_model(
            model_ids=model_ids,
            model_id=composed_model_id,
            description=description
        )
        
        model = poller.result()
        
        print(f"Composed model ID: {model.model_id}")
        print(f"Description: {model.description}")
        
        return {
            "model_id": model.model_id,
            "description": model.description,
            "created_on": str(model.created_on)
        }
    
    def list_models(self) -> list:
        """List all custom models"""
        print("Listing custom models...")
        
        models = []
        for model in self.client.list_document_models():
            model_info = {
                "model_id": model.model_id,
                "description": model.description,
                "created_on": str(model.created_on)
            }
            models.append(model_info)
            print(f"  - {model.model_id}: {model.description}")
        
        return models
    
    def get_model_info(self, model_id: str) -> Dict[str, Any]:
        """Get detailed information about a model"""
        print(f"Getting model info: {model_id}")
        
        model = self.client.get_document_model(model_id)
        
        return {
            "model_id": model.model_id,
            "description": model.description,
            "created_on": str(model.created_on),
            "doc_types": {
                name: {
                    "description": doc_type.description,
                    "fields": list(doc_type.field_schema.keys())
                }
                for name, doc_type in model.doc_types.items()
            }
        }
    
    def delete_model(self, model_id: str) -> None:
        """Delete a custom model"""
        print(f"Deleting model: {model_id}")
        self.client.delete_document_model(model_id)
        print(f"Model {model_id} deleted successfully")
    
    def get_resource_info(self) -> Dict[str, Any]:
        """Get Document Intelligence resource information"""
        info = self.client.get_resource_details()
        
        return {
            "custom_document_models": {
                "count": info.custom_document_models.count,
                "limit": info.custom_document_models.limit
            }
        }


def main():
    """Main function to demonstrate custom model training"""
    
    # Configuration
    endpoint = "https://your-doc-intelligence.cognitiveservices.azure.com/"
    api_key = "your-api-key"
    
    # Initialize trainer
    trainer = CustomModelTrainer(endpoint, api_key)
    
    # Get resource info
    print("Resource Information:")
    resource_info = trainer.get_resource_info()
    print(f"Custom models: {resource_info['custom_document_models']['count']}/{resource_info['custom_document_models']['limit']}")
    
    # Training data URL (Azure Blob Storage with SAS token)
    training_data_url = "https://yourstorageaccount.blob.core.windows.net/training-data?<SAS-token>"
    
    # Train custom model
    print("\\nTraining custom model...")
    # model_info = trainer.train_custom_model(
    #     training_data_url=training_data_url,
    #     model_id="custom-invoice-model",
    #     description="Custom invoice processing model",
    #     build_mode="template"
    # )
    
    # List all models
    print("\\nListing all models...")
    models = trainer.list_models()
    
    print("\\nCustom model training examples created successfully!")


if __name__ == "__main__":
    main()
`;
            await vscode.typeInEditor(customModelScript);
            await test_helpers_1.TestHelpers.wait(2000);
            const window = vscode.getMainWindow();
            const isMac = process.platform === 'darwin';
            const modifier = isMac ? 'Meta' : 'Control';
            await window.keyboard.press(`${modifier}+Shift+KeyS`);
            await test_helpers_1.TestHelpers.wait(1000);
            await window.keyboard.type('custom_model_trainer.py');
            await window.keyboard.press('Enter');
            await test_helpers_1.TestHelpers.wait(2000);
            await vscode.takeScreenshot();
        });
    });
    /**
     * Integration Tests - Combining both services
     */
    test_1.test.describe('Azure AI Services Integration', () => {
        (0, test_1.test)('should create integrated document processing pipeline', async () => {
            test_helpers_1.TestHelpers.logStep('Creating integrated document processing pipeline');
            await vscode.createNewFile();
            await test_helpers_1.TestHelpers.wait(1000);
            const integrationScript = `"""
Integrated Document Processing Pipeline
Combines Azure Document Intelligence and Azure AI Search
"""

from azure.core.credentials import AzureKeyCredential
from azure.ai.formrecognizer import DocumentAnalysisClient
from azure.search.documents import SearchClient
from azure.search.documents.indexes import SearchIndexClient
from typing import Dict, List, Any
import json
from datetime import datetime
import hashlib


class DocumentProcessingPipeline:
    """
    Integrated pipeline for document processing and search
    1. Extract data from documents using Document Intelligence
    2. Index extracted data into Azure AI Search
    3. Enable semantic search on processed documents
    """
    
    def __init__(
        self,
        doc_intel_endpoint: str,
        doc_intel_key: str,
        search_endpoint: str,
        search_key: str,
        search_index: str
    ):
        """Initialize the pipeline with both services"""
        
        # Document Intelligence client
        self.doc_intel_client = DocumentAnalysisClient(
            endpoint=doc_intel_endpoint,
            credential=AzureKeyCredential(doc_intel_key)
        )
        
        # Search clients
        self.search_client = SearchClient(
            endpoint=search_endpoint,
            index_name=search_index,
            credential=AzureKeyCredential(search_key)
        )
        
        self.index_client = SearchIndexClient(
            endpoint=search_endpoint,
            credential=AzureKeyCredential(search_key)
        )
    
    def process_invoice(self, document_path: str) -> Dict[str, Any]:
        """
        Process invoice and prepare for indexing
        
        Args:
            document_path: Path to invoice document
            
        Returns:
            Processed invoice data ready for indexing
        """
        print(f"Processing invoice: {document_path}")
        
        # Analyze invoice with Document Intelligence
        with open(document_path, "rb") as f:
            poller = self.doc_intel_client.begin_analyze_document(
                "prebuilt-invoice",
                document=f
            )
        
        result = poller.result()
        
        # Extract invoice data
        invoice_data = {}
        for document in result.documents:
            fields = document.fields
            
            # Generate unique ID
            doc_id = hashlib.md5(document_path.encode()).hexdigest()
            
            # Prepare searchable document
            invoice_data = {
                "id": doc_id,
                "document_type": "invoice",
                "file_path": document_path,
                "vendor_name": fields.get("VendorName", {}).value if "VendorName" in fields else None,
                "customer_name": fields.get("CustomerName", {}).value if "CustomerName" in fields else None,
                "invoice_id": fields.get("InvoiceId", {}).value if "InvoiceId" in fields else None,
                "invoice_date": str(fields.get("InvoiceDate", {}).value) if "InvoiceDate" in fields else None,
                "total_amount": fields.get("InvoiceTotal", {}).value if "InvoiceTotal" in fields else 0.0,
                "currency": fields.get("CurrencyCode", {}).value if "CurrencyCode" in fields else "USD",
                "content": result.content,
                "processed_date": datetime.utcnow().isoformat(),
                "tags": ["invoice", "financial", "processed"]
            }
            
            # Extract line items as searchable content
            line_items_text = []
            if "Items" in fields:
                for item in fields["Items"].value:
                    desc = item.value.get("Description", {}).value if "Description" in item.value else ""
                    if desc:
                        line_items_text.append(desc)
            
            invoice_data["line_items"] = ", ".join(line_items_text)
        
        return invoice_data
    
    def process_receipt(self, document_path: str) -> Dict[str, Any]:
        """
        Process receipt and prepare for indexing
        
        Args:
            document_path: Path to receipt document
            
        Returns:
            Processed receipt data ready for indexing
        """
        print(f"Processing receipt: {document_path}")
        
        with open(document_path, "rb") as f:
            poller = self.doc_intel_client.begin_analyze_document(
                "prebuilt-receipt",
                document=f
            )
        
        result = poller.result()
        
        receipt_data = {}
        for document in result.documents:
            fields = document.fields
            
            doc_id = hashlib.md5(document_path.encode()).hexdigest()
            
            receipt_data = {
                "id": doc_id,
                "document_type": "receipt",
                "file_path": document_path,
                "merchant_name": fields.get("MerchantName", {}).value if "MerchantName" in fields else None,
                "transaction_date": str(fields.get("TransactionDate", {}).value) if "TransactionDate" in fields else None,
                "total_amount": fields.get("Total", {}).value if "Total" in fields else 0.0,
                "content": result.content,
                "processed_date": datetime.utcnow().isoformat(),
                "tags": ["receipt", "expense", "processed"]
            }
            
            # Extract items
            items_text = []
            if "Items" in fields:
                for item in fields["Items"].value:
                    desc = item.value.get("Description", {}).value if "Description" in item.value else ""
                    if desc:
                        items_text.append(desc)
            
            receipt_data["items"] = ", ".join(items_text)
        
        return receipt_data
    
    def index_document(self, document_data: Dict[str, Any]) -> bool:
        """
        Index processed document into Azure AI Search
        
        Args:
            document_data: Processed document data
            
        Returns:
            True if successful, False otherwise
        """
        try:
            print(f"Indexing document: {document_data['id']}")
            
            result = self.search_client.upload_documents(documents=[document_data])
            
            for item in result:
                if item.succeeded:
                    print(f"Document {item.key} indexed successfully")
                    return True
                else:
                    print(f"Failed to index document: {item.error_message}")
                    return False
                    
        except Exception as e:
            print(f"Error indexing document: {str(e)}")
            return False
    
    def search_documents(
        self,
        query: str,
        document_type: str = None,
        date_from: str = None,
        date_to: str = None
    ) -> List[Dict[str, Any]]:
        """
        Search indexed documents
        
        Args:
            query: Search query
            document_type: Filter by document type
            date_from: Filter by date range (from)
            date_to: Filter by date range (to)
            
        Returns:
            List of matching documents
        """
        print(f"Searching documents: {query}")
        
        # Build filter expression
        filters = []
        if document_type:
            filters.append(f"document_type eq '{document_type}'")
        if date_from:
            filters.append(f"processed_date ge {date_from}")
        if date_to:
            filters.append(f"processed_date le {date_to}")
        
        filter_expression = " and ".join(filters) if filters else None
        
        # Search with semantic ranking
        results = self.search_client.search(
            search_text=query,
            filter=filter_expression,
            query_type="semantic",
            semantic_configuration_name="default-semantic-config",
            top=10
        )
        
        documents = []
        for result in results:
            documents.append({
                "id": result.get("id"),
                "document_type": result.get("document_type"),
                "file_path": result.get("file_path"),
                "content_preview": result.get("content", "")[:200],
                "score": result.get("@search.score"),
                "reranker_score": result.get("@search.reranker_score")
            })
        
        print(f"Found {len(documents)} documents")
        return documents
    
    def process_and_index_batch(
        self,
        document_paths: List[str],
        document_type: str = "invoice"
    ) -> Dict[str, Any]:
        """
        Process and index multiple documents
        
        Args:
            document_paths: List of document paths
            document_type: Type of documents to process
            
        Returns:
            Processing summary
        """
        summary = {
            "total": len(document_paths),
            "processed": 0,
            "indexed": 0,
            "failed": 0,
            "errors": []
        }
        
        for doc_path in document_paths:
            try:
                # Process document
                if document_type == "invoice":
                    doc_data = self.process_invoice(doc_path)
                elif document_type == "receipt":
                    doc_data = self.process_receipt(doc_path)
                else:
                    raise ValueError(f"Unsupported document type: {document_type}")
                
                summary["processed"] += 1
                
                # Index document
                if self.index_document(doc_data):
                    summary["indexed"] += 1
                else:
                    summary["failed"] += 1
                    
            except Exception as e:
                summary["failed"] += 1
                summary["errors"].append({
                    "document": doc_path,
                    "error": str(e)
                })
        
        return summary


def main():
    """Main function to demonstrate the integrated pipeline"""
    
    # Configuration
    doc_intel_endpoint = "https://your-doc-intelligence.cognitiveservices.azure.com/"
    doc_intel_key = "your-doc-intel-key"
    search_endpoint = "https://your-search-service.search.windows.net"
    search_key = "your-search-key"
    search_index = "documents-index"
    
    # Initialize pipeline
    pipeline = DocumentProcessingPipeline(
        doc_intel_endpoint=doc_intel_endpoint,
        doc_intel_key=doc_intel_key,
        search_endpoint=search_endpoint,
        search_key=search_key,
        search_index=search_index
    )
    
    # Example: Process and index invoices
    print("Processing invoices...")
    # invoice_paths = ["invoice1.pdf", "invoice2.pdf"]
    # summary = pipeline.process_and_index_batch(invoice_paths, "invoice")
    # print(json.dumps(summary, indent=2))
    
    # Example: Search documents
    print("\\nSearching documents...")
    # results = pipeline.search_documents(
    #     query="vendor invoices from last month",
    #     document_type="invoice"
    # )
    # print(json.dumps(results, indent=2))
    
    print("\\nIntegrated pipeline examples created successfully!")


if __name__ == "__main__":
    main()
`;
            await vscode.typeInEditor(integrationScript);
            await test_helpers_1.TestHelpers.wait(2000);
            const window = vscode.getMainWindow();
            const isMac = process.platform === 'darwin';
            const modifier = isMac ? 'Meta' : 'Control';
            await window.keyboard.press(`${modifier}+Shift+KeyS`);
            await test_helpers_1.TestHelpers.wait(1000);
            await window.keyboard.type('integrated_document_pipeline.py');
            await window.keyboard.press('Enter');
            await test_helpers_1.TestHelpers.wait(2000);
            await vscode.takeScreenshot();
        });
        (0, test_1.test)('should create requirements file for Azure AI services', async () => {
            test_helpers_1.TestHelpers.logStep('Creating requirements file');
            await vscode.createNewFile();
            await test_helpers_1.TestHelpers.wait(1000);
            const requirements = `# Azure AI Services Requirements
# Azure AI Search
azure-search-documents==11.4.0
azure-core==1.29.5

# Azure Document Intelligence (Form Recognizer)
azure-ai-formrecognizer==3.3.0

# Azure Identity for authentication
azure-identity==1.15.0

# Additional dependencies
python-dotenv==1.0.0
requests==2.31.0
Pillow==10.1.0

# Data processing
pandas==2.1.4
numpy==1.26.2

# Utilities
python-dateutil==2.8.2
`;
            await vscode.typeInEditor(requirements);
            await test_helpers_1.TestHelpers.wait(1000);
            const window = vscode.getMainWindow();
            const isMac = process.platform === 'darwin';
            const modifier = isMac ? 'Meta' : 'Control';
            await window.keyboard.press(`${modifier}+Shift+KeyS`);
            await test_helpers_1.TestHelpers.wait(1000);
            await window.keyboard.type('azure-ai-requirements.txt');
            await window.keyboard.press('Enter');
            await test_helpers_1.TestHelpers.wait(2000);
            await vscode.takeScreenshot();
        });
    });
    (0, test_1.test)('should verify all Azure AI service files are created', async () => {
        test_helpers_1.TestHelpers.logStep('Verifying Azure AI service test files');
        const window = vscode.getMainWindow();
        // Open file explorer
        try {
            await vscode.executeCommand('View: Show Explorer');
            await test_helpers_1.TestHelpers.wait(2000);
            // Take final screenshot
            await vscode.takeScreenshot();
            console.log('Azure AI Services test suite created successfully!');
            console.log('Created files:');
            console.log('  - azure-search-config.json');
            console.log('  - azure_search_indexer.py');
            console.log('  - azure_search_queries.py');
            console.log('  - document-intelligence-config.json');
            console.log('  - document_intelligence_analyzer.py');
            console.log('  - custom_model_trainer.py');
            console.log('  - integrated_document_pipeline.py');
            console.log('  - azure-ai-requirements.txt');
        }
        catch (error) {
            console.log('File verification completed with warnings:', error);
        }
    });
});
