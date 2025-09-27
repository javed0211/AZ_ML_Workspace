using Microsoft.Playwright;
using PlaywrightFramework.Utils;
using TechTalk.SpecFlow;
using NUnit.Framework;
using Newtonsoft.Json;

namespace PlaywrightFramework.StepDefinitions
{
    /// <summary>
    /// Step definitions for API testing scenarios in BDD tests
    /// </summary>
    [Binding]
    public class ApiStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly Logger _logger;
        private PlaywrightApiUtils? _apiClient;
        private ApiResponse<object>? _lastResponse;
        private readonly Dictionary<string, object> _testData;

        public ApiStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _logger = Logger.Instance;
            _testData = new Dictionary<string, object>();
        }

        #region Setup and Teardown

        [BeforeScenario("@api")]
        public async Task BeforeApiScenario()
        {
            var playwright = await Playwright.CreateAsync();
            _scenarioContext["playwright"] = playwright;

            // Get base URL from configuration or scenario tags
            var baseUrl = GetBaseUrlFromScenario();
            _apiClient = await PlaywrightApiUtils.CreateAsync(playwright, baseUrl);
            _scenarioContext["apiClient"] = _apiClient;

            _logger.LogInfo($"API client initialized with base URL: {baseUrl}");
        }

        [AfterScenario("@api")]
        public async Task AfterApiScenario()
        {
            if (_apiClient != null)
            {
                await _apiClient.DisposeAsync();
            }

            if (_scenarioContext.ContainsKey("playwright"))
            {
                var playwright = _scenarioContext["playwright"] as IPlaywright;
                playwright?.Dispose();
            }
        }

        #endregion

        #region Authentication Steps

        [Given(@"I set the bearer token to ""(.*)""")]
        public void GivenISetTheBearerTokenTo(string token)
        {
            _apiClient!.SetBearerToken(token);
            _logger.LogAction("Set Bearer Token", "***");
        }

        [Given(@"I set basic authentication with username ""(.*)"" and password ""(.*)""")]
        public void GivenISetBasicAuthenticationWithUsernameAndPassword(string username, string password)
        {
            _apiClient!.SetBasicAuth(username, password);
            _logger.LogAction("Set Basic Auth", username);
        }

        [Given(@"I set API key ""(.*)"" to ""(.*)""")]
        public void GivenISetAPIKeyTo(string keyName, string keyValue)
        {
            _apiClient!.SetApiKey(keyName, keyValue);
            _logger.LogAction($"Set API Key: {keyName}", "***");
        }

        [Given(@"I set header ""(.*)"" to ""(.*)""")]
        public void GivenISetHeaderTo(string headerName, string headerValue)
        {
            _apiClient!.SetDefaultHeader(headerName, headerValue);
            _logger.LogAction($"Set Header: {headerName}", headerValue);
        }

        #endregion

        #region HTTP Request Steps

        [When(@"I send a GET request to ""(.*)""")]
        public async Task WhenISendAGETRequestTo(string endpoint)
        {
            _lastResponse = await _apiClient!.GetAsync<object>(endpoint);
            _scenarioContext["lastResponse"] = _lastResponse;
            _logger.LogAction("GET Request", endpoint);
        }

        [When(@"I send a POST request to ""(.*)"" with body:")]
        public async Task WhenISendAPOSTRequestToWithBody(string endpoint, string body)
        {
            var data = ParseRequestBody(body);
            _lastResponse = await _apiClient!.PostAsync<object>(endpoint, data);
            _scenarioContext["lastResponse"] = _lastResponse;
            _logger.LogAction("POST Request", endpoint);
        }

        [When(@"I send a PUT request to ""(.*)"" with body:")]
        public async Task WhenISendAPUTRequestToWithBody(string endpoint, string body)
        {
            var data = ParseRequestBody(body);
            _lastResponse = await _apiClient!.PutAsync<object>(endpoint, data);
            _scenarioContext["lastResponse"] = _lastResponse;
            _logger.LogAction("PUT Request", endpoint);
        }

        [When(@"I send a PATCH request to ""(.*)"" with body:")]
        public async Task WhenISendAPATCHRequestToWithBody(string endpoint, string body)
        {
            var data = ParseRequestBody(body);
            _lastResponse = await _apiClient!.PatchAsync<object>(endpoint, data);
            _scenarioContext["lastResponse"] = _lastResponse;
            _logger.LogAction("PATCH Request", endpoint);
        }

        [When(@"I send a DELETE request to ""(.*)""")]
        public async Task WhenISendADELETERequestTo(string endpoint)
        {
            _lastResponse = await _apiClient!.DeleteAsync<object>(endpoint);
            _scenarioContext["lastResponse"] = _lastResponse;
            _logger.LogAction("DELETE Request", endpoint);
        }

        [When(@"I send a GET request to ""(.*)"" with query parameters:")]
        public async Task WhenISendAGETRequestToWithQueryParameters(string endpoint, Table table)
        {
            var queryParams = new Dictionary<string, string>();
            foreach (var row in table.Rows)
            {
                queryParams[row["Parameter"]] = row["Value"];
            }

            var options = new ApiRequestOptions { QueryParams = queryParams };
            _lastResponse = await _apiClient!.GetAsync<object>(endpoint, options);
            _scenarioContext["lastResponse"] = _lastResponse;
            _logger.LogAction("GET Request with Query Params", endpoint);
        }

        #endregion

        #region Response Validation Steps

        [Then(@"the response status code should be (\d+)")]
        public void ThenTheResponseStatusCodeShouldBe(int expectedStatusCode)
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            _apiClient!.AssertStatusCode(_lastResponse!, expectedStatusCode);
        }

        [Then(@"the response should be successful")]
        public void ThenTheResponseShouldBeSuccessful()
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            _apiClient!.AssertSuccess(_lastResponse!);
        }

        [Then(@"the response should contain header ""(.*)""")]
        public void ThenTheResponseShouldContainHeader(string headerName)
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            _apiClient!.AssertHeader(_lastResponse!, headerName);
        }

        [Then(@"the response header ""(.*)"" should be ""(.*)""")]
        public void ThenTheResponseHeaderShouldBe(string headerName, string expectedValue)
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            _apiClient!.AssertHeader(_lastResponse!, headerName, expectedValue);
        }

        [Then(@"the response should contain ""(.*)""")]
        public void ThenTheResponseShouldContain(string expectedText)
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            _apiClient!.AssertResponseContains(_lastResponse!, expectedText);
        }

        [Then(@"the response should match pattern ""(.*)""")]
        public void ThenTheResponseShouldMatchPattern(string pattern)
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            _apiClient!.AssertResponseMatches(_lastResponse!, pattern);
        }

        [Then(@"the response time should be less than (\d+) milliseconds")]
        public void ThenTheResponseTimeShouldBeLessThanMilliseconds(int maxMilliseconds)
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            _apiClient!.AssertResponseTime(_lastResponse!, maxMilliseconds);
        }

        [Then(@"the response should have the following JSON structure:")]
        public void ThenTheResponseShouldHaveTheFollowingJSONStructure(string expectedJson)
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            
            var actualJson = _lastResponse!.RawResponse;
            var expectedObject = JsonConvert.DeserializeObject(expectedJson);
            var actualObject = JsonConvert.DeserializeObject(actualJson);
            
            Assert.That(actualObject, Is.Not.Null);
            // Add more sophisticated JSON comparison logic here if needed
        }

        [Then(@"the JSON response should have field ""(.*)"" with value ""(.*)""")]
        public void ThenTheJSONResponseShouldHaveFieldWithValue(string fieldPath, string expectedValue)
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            
            var actualValue = ApiTestHelpers.ExtractJsonValue<string>(_lastResponse!.RawResponse, fieldPath);
            Assert.That(actualValue, Is.EqualTo(expectedValue), $"Field '{fieldPath}' does not match expected value");
        }

        [Then(@"the JSON response should have field ""(.*)"" of type ""(.*)""")]
        public void ThenTheJSONResponseShouldHaveFieldOfType(string fieldPath, string expectedType)
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            
            var value = ApiTestHelpers.ExtractJsonValue<object>(_lastResponse!.RawResponse, fieldPath);
            Assert.That(value, Is.Not.Null, $"Field '{fieldPath}' not found");
            
            // Validate type based on expectedType
            switch (expectedType.ToLower())
            {
                case "string":
                    Assert.That(value, Is.TypeOf<string>());
                    break;
                case "number":
                case "integer":
                    Assert.That(value, Is.TypeOf<long>().Or.TypeOf<int>().Or.TypeOf<decimal>().Or.TypeOf<double>());
                    break;
                case "boolean":
                    Assert.That(value, Is.TypeOf<bool>());
                    break;
                case "array":
                    Assert.That(value, Is.AssignableFrom<System.Collections.IEnumerable>());
                    break;
                default:
                    Assert.Fail($"Unknown type: {expectedType}");
                    break;
            }
        }

        #endregion

        #region Data Management Steps

        [Given(@"I store the value ""(.*)"" as ""(.*)""")]
        public void GivenIStoreTheValueAs(string value, string variableName)
        {
            _testData[variableName] = value;
            _logger.LogAction($"Store Variable: {variableName}", value);
        }

        [Given(@"I store the response field ""(.*)"" as ""(.*)""")]
        public void GivenIStoreTheResponseFieldAs(string fieldPath, string variableName)
        {
            Assert.That(_lastResponse, Is.Not.Null, "No response received");
            
            var value = ApiTestHelpers.ExtractJsonValue<object>(_lastResponse!.RawResponse, fieldPath);
            _testData[variableName] = value!;
            _logger.LogAction($"Store Response Field: {variableName}", value?.ToString() ?? "null");
        }

        [When(@"I send a GET request to ""(.*)"" using stored variable ""(.*)""")]
        public async Task WhenISendAGETRequestToUsingStoredVariable(string endpointTemplate, string variableName)
        {
            Assert.That(_testData.ContainsKey(variableName), Is.True, $"Variable '{variableName}' not found");
            
            var endpoint = endpointTemplate.Replace($"{{{variableName}}}", _testData[variableName].ToString());
            await WhenISendAGETRequestTo(endpoint);
        }

        #endregion

        #region File Upload/Download Steps

        [When(@"I upload file ""(.*)"" to ""(.*)"" with field name ""(.*)""")]
        public async Task WhenIUploadFileToWithFieldName(string filePath, string endpoint, string fieldName)
        {
            _lastResponse = await _apiClient!.UploadFileAsync<object>(endpoint, filePath, fieldName);
            _scenarioContext["lastResponse"] = _lastResponse;
            _logger.LogAction("File Upload", $"{filePath} -> {endpoint}");
        }

        [When(@"I download file from ""(.*)"" to ""(.*)""")]
        public async Task WhenIDownloadFileFromTo(string endpoint, string downloadPath)
        {
            var filePath = await _apiClient!.DownloadFileAsync(endpoint, downloadPath);
            _testData["downloadedFilePath"] = filePath;
            _logger.LogAction("File Download", $"{endpoint} -> {filePath}");
        }

        [Then(@"the downloaded file should exist")]
        public void ThenTheDownloadedFileShouldExist()
        {
            Assert.That(_testData.ContainsKey("downloadedFilePath"), Is.True, "No file was downloaded");
            var filePath = _testData["downloadedFilePath"].ToString();
            Assert.That(File.Exists(filePath), Is.True, $"Downloaded file does not exist: {filePath}");
        }

        #endregion

        #region Helper Methods

        private string GetBaseUrlFromScenario()
        {
            // Check scenario tags for base URL
            var tags = _scenarioContext.ScenarioInfo.Tags;
            var baseUrlTag = tags.FirstOrDefault(t => t.StartsWith("baseUrl:"));
            
            if (baseUrlTag != null)
            {
                return baseUrlTag.Substring("baseUrl:".Length);
            }

            // Fallback to configuration
            var config = ConfigManager.Instance;
            return config.GetCurrentEnvironment().BaseUrl;
        }

        private object ParseRequestBody(string body)
        {
            // Replace variables in the body
            foreach (var kvp in _testData)
            {
                body = body.Replace($"{{{kvp.Key}}}", kvp.Value.ToString());
            }

            try
            {
                // Try to parse as JSON
                return JsonConvert.DeserializeObject(body)!;
            }
            catch
            {
                // Return as string if not valid JSON
                return body;
            }
        }

        #endregion
    }
}