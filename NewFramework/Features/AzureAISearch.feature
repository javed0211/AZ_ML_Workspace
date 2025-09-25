Feature: Azure AI Search Integration
    As a data scientist
    I want to search through AI-indexed content
    So that I can find relevant information for my ML projects

Background:
    Given I am a data scientist named "Javed"
    And I have access to Azure AI Search

Scenario: Search for Climate Data
    When I search for "climate-data" in the AI search index
    Then I should see more than 10 results
    And the results should be relevant to climate data

Scenario: Search Performance Test
    When I search for "machine learning" in the AI search index
    Then the search should complete within 2 seconds
    And I should receive search results

Scenario: Search with Filters
    When I search for "temperature data" with filters:
        | Filter     | Value      |
        | Category   | Climate    |
        | DateRange  | 2023-2024  |
        | DataType   | Numerical  |
    Then I should see filtered results
    And all results should match the applied filters

Scenario: Empty Search Results
    When I search for "nonexistent-data-xyz123" in the AI search index
    Then I should see 0 results
    And I should receive a proper empty result response

Scenario Outline: Search Different Data Types
    When I search for "<SearchTerm>" in the AI search index
    Then I should see at least <MinResults> results
    And the response time should be acceptable

    Examples:
        | SearchTerm        | MinResults |
        | weather           | 5          |
        | temperature       | 10         |
        | precipitation     | 3          |
        | climate change    | 15         |