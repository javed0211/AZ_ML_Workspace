Feature: Azure Document Intelligence Service Integration
    As a data scientist
    I want to use Azure Document Intelligence to extract information from documents
    So that I can automate document processing and data extraction

Background:
    Given I am a data scientist named "Javed"
    And I have access to Azure Document Intelligence service

@smoke @document
Scenario: Analyze a prebuilt invoice document
    Given I have an invoice document at "test-data/sample-invoice.pdf"
    When I analyze the document using the prebuilt invoice model
    Then the analysis should complete successfully
    And the result should contain invoice fields
    And the invoice should have a vendor name
    And the invoice should have a total amount
    And the confidence score should be greater than 0.7

@smoke @document
Scenario: Analyze a receipt document
    Given I have a receipt document at "test-data/sample-receipt.jpg"
    When I analyze the document using the prebuilt receipt model
    Then the analysis should complete successfully
    And the result should contain receipt fields
    And the receipt should have merchant information
    And the receipt should have transaction date
    And the receipt should have total amount

@document @id
Scenario: Extract information from ID document
    Given I have an ID document at "test-data/sample-id.png"
    When I analyze the document using the prebuilt ID model
    Then the analysis should complete successfully
    And the result should contain ID fields
    And the ID should have first name
    And the ID should have last name
    And the ID should have date of birth
    And the ID should have document number

@document @layout
Scenario: Extract layout and structure from document
    Given I have a document at "test-data/sample-document.pdf"
    When I analyze the document layout
    Then the analysis should complete successfully
    And the result should contain pages
    And the result should contain text lines
    And the result should contain tables if present
    And the result should contain selection marks if present

@document @custom
Scenario: Train a custom document model
    Given I have training documents in storage container "training-data"
    When I train a custom model named "custom-form-model" with the training data
    Then the model training should complete successfully
    And the model should have a model ID
    And the model should be ready for use
    And the model accuracy should be greater than 0.8

@document @custom
Scenario: Analyze document with custom model
    Given I have a custom model named "custom-form-model"
    And I have a document at "test-data/custom-form.pdf"
    When I analyze the document using the custom model
    Then the analysis should complete successfully
    And the result should contain custom fields
    And all custom fields should have confidence scores

@document @batch
Scenario: Batch process multiple documents
    Given I have multiple documents in folder "test-data/batch":
        | FileName           | Type    |
        | invoice-001.pdf    | invoice |
        | invoice-002.pdf    | invoice |
        | receipt-001.jpg    | receipt |
    When I batch process all documents with appropriate models
    Then all documents should be processed successfully
    And each result should contain extracted fields
    And the batch processing should complete within 30 seconds

@document @table
Scenario: Extract tables from document
    Given I have a document with tables at "test-data/document-with-tables.pdf"
    When I analyze the document layout
    Then the analysis should complete successfully
    And the result should contain at least 1 table
    And each table should have rows and columns
    And table cells should contain text content

@document @compose
Scenario: Create composed model from multiple models
    Given I have trained custom models:
        | ModelName        | Description          |
        | invoice-model    | Invoice extraction   |
        | receipt-model    | Receipt extraction   |
    When I compose a new model named "financial-docs-model" from these models
    Then the composed model should be created successfully
    And the composed model should have a model ID
    And the composed model should support both invoice and receipt documents

@document @performance
Scenario: Document analysis performance test
    Given I have 10 documents in folder "test-data/performance"
    When I analyze all documents concurrently using prebuilt models
    Then all analyses should complete within 60 seconds
    And all analyses should return results
    And the average processing time per document should be less than 10 seconds

@document @validation
Scenario: Validate extracted data quality
    Given I have an invoice document at "test-data/sample-invoice.pdf"
    When I analyze the document using the prebuilt invoice model
    Then the analysis should complete successfully
    And all required fields should be present:
        | FieldName       |
        | VendorName      |
        | InvoiceDate     |
        | InvoiceTotal    |
        | CustomerName    |
    And all field confidence scores should be greater than 0.6

@document @error
Scenario: Handle unsupported document format
    Given I have a document at "test-data/unsupported.txt"
    When I attempt to analyze the document using the prebuilt invoice model
    Then the analysis should fail with appropriate error
    And the error message should indicate unsupported format

Scenario Outline: Analyze different document types
    Given I have a document at "<DocumentPath>"
    When I analyze the document using the prebuilt <ModelType> model
    Then the analysis should complete successfully
    And the result should contain <ModelType> fields
    And the confidence score should be greater than <MinConfidence>

    Examples:
        | DocumentPath                  | ModelType | MinConfidence |
        | test-data/invoice-sample.pdf  | invoice   | 0.7           |
        | test-data/receipt-sample.jpg  | receipt   | 0.7           |
        | test-data/id-sample.png       | ID        | 0.8           |
        | test-data/business-card.jpg   | businessCard | 0.7        |