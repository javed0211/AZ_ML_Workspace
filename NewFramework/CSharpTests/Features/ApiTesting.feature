@api @baseUrl:https://jsonplaceholder.typicode.com
Feature: API Testing Examples
    As a QA engineer
    I want to test REST APIs using BDD scenarios
    So that I can ensure API functionality works correctly

Background:
    Given I set header "Content-Type" to "application/json"
    And I set header "Accept" to "application/json"

@smoke @get
Scenario: Get a single post
    When I send a GET request to "/posts/1"
    Then the response should be successful
    And the response status code should be 200
    And the response should contain "userId"
    And the response should contain "title"
    And the JSON response should have field "$.id" with value "1"
    And the JSON response should have field "$.userId" of type "number"
    And the JSON response should have field "$.title" of type "string"
    And the response time should be less than 5000 milliseconds

@smoke @get @pagination
Scenario: Get posts with pagination
    When I send a GET request to "/posts" with query parameters:
        | Parameter | Value |
        | _page     | 1     |
        | _limit    | 5     |
    Then the response should be successful
    And the response status code should be 200
    And the response should contain "userId"

@crud @post
Scenario: Create a new post
    When I send a POST request to "/posts" with body:
        """
        {
            "title": "Test Post from BDD",
            "body": "This is a test post created via BDD scenario",
            "userId": 1
        }
        """
    Then the response should be successful
    And the response status code should be 201
    And the JSON response should have field "$.title" with value "Test Post from BDD"
    And the JSON response should have field "$.body" with value "This is a test post created via BDD scenario"
    And the JSON response should have field "$.userId" with value "1"

@crud @put
Scenario: Update an existing post
    When I send a PUT request to "/posts/1" with body:
        """
        {
            "id": 1,
            "title": "Updated Post Title",
            "body": "This post has been updated via BDD scenario",
            "userId": 1
        }
        """
    Then the response should be successful
    And the response status code should be 200
    And the JSON response should have field "$.title" with value "Updated Post Title"
    And the JSON response should have field "$.body" with value "This post has been updated via BDD scenario"

@crud @patch
Scenario: Partially update a post
    When I send a PATCH request to "/posts/1" with body:
        """
        {
            "title": "Partially Updated Title"
        }
        """
    Then the response should be successful
    And the response status code should be 200
    And the JSON response should have field "$.title" with value "Partially Updated Title"

@crud @delete
Scenario: Delete a post
    When I send a DELETE request to "/posts/1"
    Then the response should be successful
    And the response status code should be 200

@authentication @bearer
Scenario: API call with Bearer token authentication
    Given I set the bearer token to "test-bearer-token-12345"
    When I send a GET request to "/posts/1"
    Then the response should be successful
    And the response status code should be 200

@authentication @basic
Scenario: API call with Basic authentication
    Given I set basic authentication with username "testuser" and password "testpass"
    When I send a GET request to "/posts/1"
    Then the response should be successful
    And the response status code should be 200

@authentication @apikey
Scenario: API call with API key authentication
    Given I set API key "X-API-Key" to "my-secret-api-key"
    When I send a GET request to "/posts/1"
    Then the response should be successful
    And the response status code should be 200

@validation @headers
Scenario: Validate response headers
    When I send a GET request to "/posts/1"
    Then the response should be successful
    And the response should contain header "Content-Type"
    And the response header "Content-Type" should be "application/json; charset=utf-8"

@validation @content
Scenario: Validate response content patterns
    When I send a GET request to "/posts/1"
    Then the response should be successful
    And the response should contain "sunt aut facere"
    And the response should match pattern "\"id\":\s*1"

@datadriven @variables
Scenario: Use stored variables in API calls
    Given I store the value "1" as "postId"
    When I send a GET request to "/posts/{postId}" using stored variable "postId"
    Then the response should be successful
    And the response status code should be 200
    And I store the response field "$.userId" as "authorId"
    When I send a GET request to "/users/{authorId}" using stored variable "authorId"
    Then the response should be successful
    And the response status code should be 200

@crud @complete-flow
Scenario: Complete CRUD operations flow
    # Create a new post
    When I send a POST request to "/posts" with body:
        """
        {
            "title": "BDD Test Post",
            "body": "This post will be used for CRUD testing",
            "userId": 1
        }
        """
    Then the response should be successful
    And the response status code should be 201
    And I store the response field "$.id" as "newPostId"
    
    # Read the created post
    When I send a GET request to "/posts/{newPostId}" using stored variable "newPostId"
    Then the response should be successful
    And the response status code should be 200
    And the JSON response should have field "$.title" with value "BDD Test Post"
    
    # Update the post
    When I send a PUT request to "/posts/{newPostId}" using stored variable "newPostId" with body:
        """
        {
            "id": {newPostId},
            "title": "Updated BDD Test Post",
            "body": "This post has been updated during CRUD testing",
            "userId": 1
        }
        """
    Then the response should be successful
    And the response status code should be 200
    
    # Delete the post
    When I send a DELETE request to "/posts/{newPostId}" using stored variable "newPostId"
    Then the response should be successful
    And the response status code should be 200

@performance @load
Scenario: Performance testing - Response time validation
    When I send a GET request to "/posts"
    Then the response should be successful
    And the response time should be less than 2000 milliseconds

@error-handling @404
Scenario: Handle 404 errors gracefully
    When I send a GET request to "/posts/999999"
    Then the response status code should be 404

@complex @nested-data
Scenario: Work with nested JSON data
    When I send a GET request to "/users/1"
    Then the response should be successful
    And the response status code should be 200
    And the JSON response should have field "$.address.city" of type "string"
    And the JSON response should have field "$.company.name" of type "string"
    And the response should contain "address"
    And the response should contain "company"