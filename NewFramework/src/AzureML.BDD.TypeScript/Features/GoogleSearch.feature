Feature: Google Search Functionality (TypeScript)
  As a user
  I want to search for products on Google
  So that I can find relevant information

  @typescript @google @search
  Scenario: Search for Marshall headphones on Google
    Given I navigate to Google homepage
    When I accept cookies if present
    And I search for "marshall headphones"
    Then I should see search results
    And the results should contain "marshall"
    And at least one search result should be visible

  @typescript @google @search @navigation
  Scenario: Search and click on first Marshall result
    Given I navigate to Google homepage
    When I accept cookies if present
    And I search for "marshall headphones"
    And I click on the first Marshall-related result
    Then I should be navigated to a Marshall-related page
    And the page should contain Marshall information