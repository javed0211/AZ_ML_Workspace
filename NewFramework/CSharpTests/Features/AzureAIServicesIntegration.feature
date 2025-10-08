Feature: Azure AI Services End-to-End Integration
    As a data scientist
    I want to extract data from documents and make it searchable
    So that I can build an intelligent document search system

Background:
    Given I am a data scientist named "Javed"
    And I have access to Azure Document Intelligence service
    And I have access to Azure AI Search service

@integration @e2e
Scenario: Extract invoice data and index for search
    Given I have an invoice document at "test-data/sample-invoice.pdf"
    When I analyze the invoice using Document Intelligence
    And I extract the following fields:
        | FieldName       |
        | VendorName      |
        | InvoiceDate     |
        | InvoiceTotal    |
        | CustomerName    |
        | InvoiceId       |
    And I create a search document from the extracted data
    And I upload the document to search index "invoices-index"
    Then the document should be searchable
    And I should be able to find it by vendor name
    And I should be able to filter by invoice date

@integration @e2e
Scenario: Process multiple documents and create searchable index
    Given I have multiple documents in folder "test-data/documents":
        | FileName           | Type    |
        | invoice-001.pdf    | invoice |
        | invoice-002.pdf    | invoice |
        | receipt-001.jpg    | receipt |
        | receipt-002.jpg    | receipt |
    When I process all documents with Document Intelligence
    And I extract relevant fields from each document
    And I create a unified search index "financial-docs-index"
    And I upload all extracted data to the search index
    Then the search index should contain 4 documents
    And I should be able to search across all document types
    And I should be able to filter by document type

@integration @semantic
Scenario: Build semantic search for document content
    Given I have analyzed documents with extracted text content
    When I create a search index with semantic configuration
    And I index the document content with metadata
    And I perform a semantic search for "vendor invoices from January 2024"
    Then I should receive semantically ranked results
    And the results should prioritize relevant invoices
    And the results should include semantic captions

@integration @pipeline
Scenario: Automated document processing pipeline
    Given I have a document processing pipeline configured
    When I upload a new invoice to the input folder
    Then the pipeline should automatically:
        | Step                                    |
        | Detect the document type                |
        | Analyze with appropriate model          |
        | Extract structured data                 |
        | Validate extracted fields               |
        | Create search document                  |
        | Index in Azure AI Search                |
        | Make document searchable                |
    And the entire pipeline should complete within 15 seconds
    And the document should be searchable immediately

@integration @enrichment
Scenario: Enrich search index with AI-extracted data
    Given I have a basic search index with document metadata
    When I analyze documents with Document Intelligence
    And I extract entities and key information
    And I enrich the search index with:
        | EnrichmentType      |
        | ExtractedEntities   |
        | KeyPhrases          |
        | DocumentStructure   |
        | ConfidenceScores    |
    Then the search index should have enriched fields
    And I should be able to search by extracted entities
    And I should be able to filter by confidence scores

@integration @realtime
Scenario: Real-time document search after extraction
    Given I have a real-time document processing system
    When I submit a document for analysis
    Then the document should be analyzed within 5 seconds
    And the extracted data should be indexed within 2 seconds
    And the document should be searchable within 10 seconds total
    And search results should reflect the new document

@integration @validation
Scenario: Validate end-to-end data accuracy
    Given I have a document with known data at "test-data/validated-invoice.pdf"
    And the document contains the following expected data:
        | Field           | ExpectedValue        |
        | VendorName      | Contoso Corporation  |
        | InvoiceTotal    | 1250.00              |
        | InvoiceDate     | 2024-01-15           |
    When I process the document through the pipeline
    And I search for "Contoso Corporation" in the index
    Then the search should return the document
    And the extracted vendor name should match "Contoso Corporation"
    And the extracted total should match 1250.00
    And the extracted date should match "2024-01-15"

@integration @performance
Scenario: High-volume document processing and indexing
    Given I have 50 documents ready for processing
    When I process all documents concurrently
    And I index all extracted data in batches of 10
    Then all documents should be processed within 120 seconds
    And all documents should be indexed successfully
    And the search index should contain 50 documents
    And all documents should be searchable

@integration @monitoring
Scenario: Monitor document processing pipeline health
    Given I have a document processing pipeline running
    When I process 10 test documents
    Then I should track the following metrics:
        | Metric                          |
        | DocumentsProcessed              |
        | AverageProcessingTime           |
        | ExtractionSuccessRate           |
        | IndexingSuccessRate             |
        | AverageConfidenceScore          |
    And the extraction success rate should be greater than 95%
    And the indexing success rate should be 100%
    And the average confidence score should be greater than 0.75

@integration @error-handling
Scenario: Handle failures in document processing pipeline
    Given I have a document processing pipeline
    When I submit a corrupted document for processing
    Then the pipeline should detect the error
    And the pipeline should log the failure
    And the pipeline should continue processing other documents
    And the failed document should be moved to error queue
    And I should receive an error notification

Scenario Outline: Process and search different document categories
    Given I have a <DocumentType> document at "<DocumentPath>"
    When I analyze it with Document Intelligence
    And I extract <DocumentType> specific fields
    And I index it in category "<Category>"
    Then I should be able to search for it by <DocumentType> fields
    And I should be able to filter by category "<Category>"
    And the processing should complete within <MaxSeconds> seconds

    Examples:
        | DocumentType | DocumentPath                  | Category  | MaxSeconds |
        | invoice      | test-data/invoice-sample.pdf  | Financial | 10         |
        | receipt      | test-data/receipt-sample.jpg  | Financial | 10         |
        | contract     | test-data/contract-sample.pdf | Legal     | 15         |
        | form         | test-data/form-sample.pdf     | HR        | 10         |