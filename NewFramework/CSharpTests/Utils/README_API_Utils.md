# API Testing Utilities Documentation

This document provides comprehensive guidance on using the API testing utilities in the Playwright BDD Framework.

## 🚀 Overview

The API testing utilities provide a complete solution for REST API automation testing using Playwright's built-in API testing capabilities. The framework includes:

- **PlaywrightApiUtils**: Core API client with HTTP methods and assertions
- **ApiTestHelpers**: Helper methods for common testing patterns
- **ApiStepDefinitions**: BDD step definitions for Gherkin scenarios
- **Comprehensive Examples**: Sample tests and feature files

## 📁 File Structure

```
Utils/
├── PlaywrightApiUtils.cs      # Core API client
├── ApiTestHelpers.cs          # Helper methods and patterns
└── README_API_Utils.md        # This documentation

StepDefinitions/
└── ApiStepDefinitions.cs      # BDD step definitions

Tests/
└── ApiTestExample.cs          # Example test class

Features/
└── ApiTesting.feature         # Example BDD scenarios
```

## 🔧 Core Components

### 1. PlaywrightApiUtils

The main API client class that provides:

#### HTTP Methods
- `GetAsync<T>(endpoint, options?)`
- `PostAsync<T>(endpoint, data?, options?)`
- `PutAsync<T>(endpoint, data?, options?)`
- `PatchAsync<T>(endpoint, data?, options?)`
- `DeleteAsync<T>(endpoint, options?)`
- `HeadAsync(endpoint, options?)`
- `OptionsAsync(endpoint, options?)`

#### Authentication Methods
- `SetBearerToken(token)`
- `SetBasicAuth(username, password)`
- `SetApiKey(key, value, location)`
- `ClearAuth()`

#### Assertion Methods
- `AssertStatusCode<T>(response, expectedCode)`
- `AssertSuccess<T>(response)`
- `AssertHeader<T>(response, headerName, expectedValue?)`
- `AssertResponseContains<T>(response, expectedText)`
- `AssertResponseMatches<T>(response, pattern)`
- `AssertResponseTime<T>(response, maxMilliseconds)`

#### File Operations
- `UploadFileAsync<T>(endpoint, filePath, fieldName?, additionalFields?)`
- `DownloadFileAsync(endpoint, downloadPath, options?)`

### 2. ApiTestHelpers

Utility class with common testing patterns:

#### Factory Methods
- `CreateApiClientAsync(playwright, baseUrl?, authToken?)`

#### Testing Patterns
- `PerformCrudTestAsync<TCreate, TRead, TUpdate>(apiClient, baseEndpoint, createData, updateData, getIdFunc)`
- `TestPaginationAsync<T>(apiClient, endpoint, pageSize?, maxPages?)`
- `TestRateLimitAsync(apiClient, endpoint, requestCount?, delay?)`

#### JSON Utilities
- `ValidateJsonSchema<T>(response, schema)`
- `ExtractJsonValue<T>(jsonResponse, jsonPath)`
- `CompareJsonIgnoringFields(json1, json2, fieldsToIgnore)`

#### Test Data Generation
- `TestDataGenerator.RandomString(length?)`
- `TestDataGenerator.RandomEmail()`
- `TestDataGenerator.RandomInt(min?, max?)`
- `TestDataGenerator.RandomDate(start?, end?)`
- `TestDataGenerator.RandomPhoneNumber()`
- `TestDataGenerator.RandomDecimal(min?, max?)`

## 🎯 Usage Examples

### Basic API Client Usage

```csharp
// Create API client
var playwright = await Playwright.CreateAsync();
var apiClient = await PlaywrightApiUtils.CreateAsync(playwright, "https://api.example.com");

// Set authentication
apiClient.SetBearerToken("your-token-here");

// Make requests
var response = await apiClient.GetAsync<User>("/users/1");
apiClient.AssertSuccess(response);
apiClient.AssertStatusCode(response, 200);

// Work with response data
var user = response.Data;
Console.WriteLine($"User: {user.Name}");
```

### CRUD Operations Example

```csharp
// Complete CRUD test
var newUser = new User { Name = "John Doe", Email = "john@example.com" };

// CREATE
var createResponse = await apiClient.PostAsync<User>("/users", newUser);
apiClient.AssertSuccess(createResponse);
var userId = createResponse.Data.Id;

// READ
var readResponse = await apiClient.GetAsync<User>($"/users/{userId}");
apiClient.AssertSuccess(readResponse);

// UPDATE
var updatedUser = new User { Id = userId, Name = "Jane Doe", Email = "jane@example.com" };
var updateResponse = await apiClient.PutAsync<User>($"/users/{userId}", updatedUser);
apiClient.AssertSuccess(updateResponse);

// DELETE
var deleteResponse = await apiClient.DeleteAsync<object>($"/users/{userId}");
apiClient.AssertSuccess(deleteResponse);
```

### Using Helper Methods

```csharp
// Automated CRUD testing
var crudResult = await ApiTestHelpers.PerformCrudTestAsync<CreateUserRequest, User, UpdateUserRequest>(
    apiClient,
    "/users",
    new CreateUserRequest { Name = "Test User", Email = "test@example.com" },
    new UpdateUserRequest { Name = "Updated User", Email = "updated@example.com" },
    user => user.Id
);

Assert.That(crudResult.Success, Is.True);

// Pagination testing
var paginationResult = await ApiTestHelpers.TestPaginationAsync<User>(
    apiClient,
    "/users",
    pageSize: 10,
    maxPages: 5
);

Assert.That(paginationResult.Success, Is.True);
Assert.That(paginationResult.AllItems.Count, Is.GreaterThan(0));
```

## 🥒 BDD Integration

### Feature File Example

```gherkin
@api @baseUrl:https://api.example.com
Feature: User Management API
    As a developer
    I want to test the User Management API
    So that I can ensure it works correctly

Background:
    Given I set the bearer token to "test-token"
    And I set header "Content-Type" to "application/json"

Scenario: Get user by ID
    When I send a GET request to "/users/1"
    Then the response should be successful
    And the response status code should be 200
    And the JSON response should have field "$.name" of type "string"
    And the JSON response should have field "$.email" of type "string"

Scenario: Create new user
    When I send a POST request to "/users" with body:
        """
        {
            "name": "John Doe",
            "email": "john.doe@example.com"
        }
        """
    Then the response should be successful
    And the response status code should be 201
    And the JSON response should have field "$.name" with value "John Doe"
```

### Available BDD Steps

#### Authentication Steps
- `Given I set the bearer token to "token"`
- `Given I set basic authentication with username "user" and password "pass"`
- `Given I set API key "key-name" to "key-value"`
- `Given I set header "header-name" to "header-value"`

#### Request Steps
- `When I send a GET request to "endpoint"`
- `When I send a POST request to "endpoint" with body:`
- `When I send a PUT request to "endpoint" with body:`
- `When I send a PATCH request to "endpoint" with body:`
- `When I send a DELETE request to "endpoint"`
- `When I send a GET request to "endpoint" with query parameters:`

#### Response Validation Steps
- `Then the response status code should be 200`
- `Then the response should be successful`
- `Then the response should contain header "header-name"`
- `Then the response header "header-name" should be "expected-value"`
- `Then the response should contain "expected-text"`
- `Then the response should match pattern "regex-pattern"`
- `Then the response time should be less than 5000 milliseconds`
- `Then the JSON response should have field "$.path" with value "expected"`
- `Then the JSON response should have field "$.path" of type "string"`

#### Data Management Steps
- `Given I store the value "value" as "variable-name"`
- `Given I store the response field "$.path" as "variable-name"`
- `When I send a GET request to "/endpoint/{variable}" using stored variable "variable-name"`

#### File Operations Steps
- `When I upload file "path/to/file" to "endpoint" with field name "file"`
- `When I download file from "endpoint" to "path/to/save"`
- `Then the downloaded file should exist`

## 🔐 Authentication Examples

### Bearer Token
```csharp
apiClient.SetBearerToken("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...");
```

### Basic Authentication
```csharp
apiClient.SetBasicAuth("username", "password");
```

### API Key (Header)
```csharp
apiClient.SetApiKey("X-API-Key", "your-api-key-here");
```

### Custom Headers
```csharp
apiClient.SetDefaultHeader("Authorization", "Custom auth-scheme");
apiClient.SetDefaultHeader("X-Client-Version", "1.0.0");
```

## 📊 Response Handling

### Working with Typed Responses
```csharp
// Strongly typed response
var response = await apiClient.GetAsync<User>("/users/1");
if (response.IsSuccess)
{
    var user = response.Data; // User object
    Console.WriteLine($"User: {user.Name}");
}

// Raw string response
var rawResponse = await apiClient.GetAsync<string>("/users/1");
var jsonString = rawResponse.Data;
```

### Response Properties
```csharp
var response = await apiClient.GetAsync<User>("/users/1");

// Status information
Console.WriteLine($"Status: {response.StatusCode} - {response.StatusText}");
Console.WriteLine($"Success: {response.IsSuccess}");

// Headers
foreach (var header in response.Headers)
{
    Console.WriteLine($"{header.Key}: {header.Value}");
}

// Timing
Console.WriteLine($"Response time: {response.ResponseTime.TotalMilliseconds}ms");

// Raw response
Console.WriteLine($"Raw JSON: {response.RawResponse}");
```

## 📁 File Operations

### File Upload
```csharp
// Simple file upload
var response = await apiClient.UploadFileAsync<UploadResponse>(
    "/upload",
    "/path/to/file.pdf",
    "document"
);

// Upload with additional form fields
var additionalFields = new Dictionary<string, string>
{
    ["description"] = "Test document",
    ["category"] = "legal"
};

var response = await apiClient.UploadFileAsync<UploadResponse>(
    "/upload",
    "/path/to/file.pdf",
    "document",
    additionalFields
);
```

### File Download
```csharp
var downloadPath = await apiClient.DownloadFileAsync(
    "/files/123/download",
    "/path/to/save/file.pdf"
);

Console.WriteLine($"File downloaded to: {downloadPath}");
```

## 🧪 Testing Patterns

### Error Handling Testing
```csharp
// Test 404 error
var response = await apiClient.GetAsync<User>("/users/999999");
apiClient.AssertStatusCode(response, 404);

// Test validation errors
var invalidUser = new User { Email = "invalid-email" };
var createResponse = await apiClient.PostAsync<User>("/users", invalidUser);
apiClient.AssertStatusCode(createResponse, 400);
```

### Performance Testing
```csharp
var response = await apiClient.GetAsync<List<User>>("/users");
apiClient.AssertSuccess(response);
apiClient.AssertResponseTime(response, 2000); // Max 2 seconds

Console.WriteLine($"Response time: {response.ResponseTime.TotalMilliseconds}ms");
```

### Data Validation
```csharp
var response = await apiClient.GetAsync<User>("/users/1");
apiClient.AssertSuccess(response);

// Validate response structure
apiClient.AssertResponseContains(response, "name");
apiClient.AssertResponseContains(response, "email");
apiClient.AssertResponseMatches(response, @"""id"":\s*\d+");

// Extract and validate specific values
var userId = ApiTestHelpers.ExtractJsonValue<int>(response.RawResponse, "$.id");
var userName = ApiTestHelpers.ExtractJsonValue<string>(response.RawResponse, "$.name");

Assert.That(userId, Is.GreaterThan(0));
Assert.That(userName, Is.Not.Empty);
```

## 🔧 Configuration

### API Client Configuration
```csharp
// Create with custom configuration
var defaultHeaders = new Dictionary<string, string>
{
    ["User-Agent"] = "MyApp/1.0",
    ["Accept"] = "application/json"
};

var apiClient = await PlaywrightApiUtils.CreateAsync(
    playwright,
    "https://api.example.com",
    defaultHeaders
);

// Configure request options
var options = new ApiRequestOptions
{
    Headers = new Dictionary<string, string>
    {
        ["X-Request-ID"] = Guid.NewGuid().ToString()
    },
    QueryParams = new Dictionary<string, string>
    {
        ["include"] = "profile,settings"
    },
    Timeout = 10000, // 10 seconds
    IgnoreHTTPSErrors = true
};

var response = await apiClient.GetAsync<User>("/users/1", options);
```

### Environment Configuration
The API utilities integrate with the existing ConfigManager for environment-specific settings:

```json
{
  "Environment": "test",
  "Environments": {
    "test": {
      "BaseUrl": "https://api-test.example.com",
      "Username": "test-user",
      "Password": "test-password"
    },
    "prod": {
      "BaseUrl": "https://api.example.com",
      "Username": "prod-user",
      "Password": "prod-password"
    }
  }
}
```

## 🚀 Best Practices

### 1. Use Strongly Typed Models
```csharp
// Good
var response = await apiClient.GetAsync<User>("/users/1");
var user = response.Data;

// Avoid
var response = await apiClient.GetAsync<object>("/users/1");
var userJson = response.RawResponse;
```

### 2. Implement Proper Error Handling
```csharp
var response = await apiClient.GetAsync<User>("/users/1");

if (response.IsSuccess)
{
    // Handle success
    var user = response.Data;
    ProcessUser(user);
}
else
{
    // Handle error
    _logger.LogError($"API call failed: {response.StatusCode} - {response.StatusText}");
    
    if (response.StatusCode == 404)
    {
        // Handle not found
    }
    else if (response.StatusCode >= 500)
    {
        // Handle server error
    }
}
```

### 3. Use Assertions for Validation
```csharp
var response = await apiClient.GetAsync<User>("/users/1");

// Use built-in assertions
apiClient.AssertSuccess(response);
apiClient.AssertStatusCode(response, 200);
apiClient.AssertResponseTime(response, 5000);

// Additional NUnit assertions
Assert.That(response.Data, Is.Not.Null);
Assert.That(response.Data.Name, Is.Not.Empty);
```

### 4. Organize Tests by Functionality
```csharp
[TestFixture]
public class UserApiTests
{
    [Test]
    public async Task GetUser_ValidId_ReturnsUser() { }
    
    [Test]
    public async Task GetUser_InvalidId_Returns404() { }
    
    [Test]
    public async Task CreateUser_ValidData_ReturnsCreatedUser() { }
    
    [Test]
    public async Task CreateUser_InvalidData_Returns400() { }
}
```

### 5. Use Test Data Generators
```csharp
// Generate test data
var testUser = new User
{
    Name = ApiTestHelpers.TestDataGenerator.RandomString(10),
    Email = ApiTestHelpers.TestDataGenerator.RandomEmail(),
    Age = ApiTestHelpers.TestDataGenerator.RandomInt(18, 65),
    CreatedDate = ApiTestHelpers.TestDataGenerator.RandomDate()
};
```

## 🐛 Troubleshooting

### Common Issues

1. **SSL Certificate Errors**
   ```csharp
   var options = new ApiRequestOptions
   {
       IgnoreHTTPSErrors = true
   };
   ```

2. **Timeout Issues**
   ```csharp
   var options = new ApiRequestOptions
   {
       Timeout = 30000 // 30 seconds
   };
   ```

3. **JSON Deserialization Errors**
   - Check that your model properties match the JSON response
   - Use `response.RawResponse` to inspect the actual JSON
   - Consider using `JsonPropertyName` attributes for property mapping

4. **Authentication Issues**
   - Verify token format and expiration
   - Check that headers are being sent correctly
   - Use network debugging tools to inspect requests

### Debugging Tips

1. **Enable Detailed Logging**
   ```json
   {
     "Logging": {
       "LogLevel": "Debug",
       "LogToConsole": true
     }
   }
   ```

2. **Inspect Raw Responses**
   ```csharp
   var response = await apiClient.GetAsync<User>("/users/1");
   Console.WriteLine($"Raw Response: {response.RawResponse}");
   Console.WriteLine($"Headers: {JsonConvert.SerializeObject(response.Headers, Formatting.Indented)}");
   ```

3. **Use Network Monitoring**
   - Enable Playwright's network logging
   - Use browser dev tools for debugging
   - Consider using tools like Fiddler or Postman for API exploration

## 📚 Additional Resources

- [Playwright API Testing Documentation](https://playwright.dev/dotnet/docs/api-testing)
- [JSON.NET Documentation](https://www.newtonsoft.com/json/help/html/Introduction.htm)
- [NUnit Documentation](https://docs.nunit.org/)
- [SpecFlow/Reqnroll Documentation](https://docs.reqnroll.net/)

## 🤝 Contributing

When adding new API utilities:

1. Follow the existing naming conventions
2. Add comprehensive XML documentation
3. Include unit tests for new functionality
4. Update this README with new features
5. Add example usage in the test files

## 📝 License

This API testing framework is part of the AzureML BDD Framework project.