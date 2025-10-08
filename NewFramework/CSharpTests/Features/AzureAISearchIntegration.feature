Feature: Azure AI Search Service Integration
    As a data scientist
    I want to use Azure AI Search to index and search documents
    So that I can find relevant information quickly using semantic search

Background:
    Given I am a data scientist named "Javed"
    And I have access to Azure AI Search service

@smoke @search
Scenario: Create a new search index
    When I create a search index named "documents-index" with the following fields:
        | FieldName   | Type           | Searchable | Filterable | Sortable |
        | id          | Edm.String     | false      | false      | false    |
        | title       | Edm.String     | true       | true       | true     |
        | content     | Edm.String     | true       | false      | false    |
        | category    | Edm.String     | false      | true       | true     |
        | createdDate | Edm.DateTimeOffset | false  | true       | true     |
    Then the search index should be created successfully
    And the index should have 5 fields

@smoke @search
Scenario: Upload documents to search index
    Given I have a search index named "documents-index"
    When I upload the following documents to the index:
        | id  | title                    | content                                      | category | createdDate          |
        | 1   | Machine Learning Basics  | Introduction to ML algorithms and concepts   | ML       | 2024-01-15T10:00:00Z |
        | 2   | Deep Learning Guide      | Neural networks and deep learning techniques | ML       | 2024-01-16T10:00:00Z |
        | 3   | Climate Data Analysis    | Analyzing temperature and precipitation data | Climate  | 2024-01-17T10:00:00Z |
    Then the documents should be indexed successfully
    And the index should contain 3 documents

@search @query
Scenario: Perform simple text search
    Given I have a search index named "documents-index" with documents
    When I search for "machine learning" in the index
    Then I should receive search results
    And the results should contain at least 1 document
    And the top result should have a relevance score greater than 0.5

@search @query
Scenario: Search with filters
    Given I have a search index named "documents-index" with documents
    When I search for "*" with the following filters:
        | FilterField | Operator | Value   |
        | category    | eq       | ML      |
    Then I should receive search results
    And all results should have category "ML"
    And the results should contain at least 1 document

@search @query
Scenario: Search with facets
    Given I have a search index named "documents-index" with documents
    When I search for "*" with facets on "category"
    Then I should receive search results with facets
    And the facets should include "category"
    And the facet "category" should have at least 1 value

@search @semantic
Scenario: Perform semantic search with ranking
    Given I have a search index named "documents-index" with semantic configuration
    When I perform a semantic search for "how to build neural networks"
    Then I should receive semantically ranked results
    And the results should include semantic captions
    And the top result should be relevant to neural networks

@search @autocomplete
Scenario: Get autocomplete suggestions
    Given I have a search index named "documents-index" with suggester configured
    When I request autocomplete suggestions for "mach"
    Then I should receive autocomplete suggestions
    And the suggestions should include "machine"

@search @performance
Scenario: Search performance test
    Given I have a search index named "documents-index" with 100 documents
    When I perform 10 concurrent searches for "data analysis"
    Then all searches should complete within 5 seconds
    And all searches should return results

@search @delete
Scenario: Delete documents from index
    Given I have a search index named "documents-index" with documents
    When I delete document with id "1" from the index
    Then the document should be removed successfully
    And the index should not contain document with id "1"

@search @cleanup
Scenario: Delete search index
    Given I have a search index named "documents-index"
    When I delete the search index
    Then the search index should be deleted successfully
    And the index should not exist

Scenario Outline: Search different content types
    Given I have a search index named "documents-index" with documents
    When I search for "<SearchTerm>" in the index
    Then I should receive search results
    And the results should contain at least <MinResults> documents
    And the search should complete within <MaxSeconds> seconds

    Examples:
        | SearchTerm        | MinResults | MaxSeconds |
        | machine learning  | 1          | 2          |
        | climate           | 1          | 2          |
        | data analysis     | 1          | 2          |
        | neural networks   | 1          | 2          |