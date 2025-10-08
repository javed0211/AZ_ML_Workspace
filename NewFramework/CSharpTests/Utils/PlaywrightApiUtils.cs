using Microsoft.Playwright;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace PlaywrightFramework.Utils
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new();
        public T? Data { get; set; }
        public string RawResponse { get; set; } = string.Empty;
        public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
        public TimeSpan ResponseTime { get; set; }
    }

    public class ApiRequestOptions
    {
        public Dictionary<string, string> Headers { get; set; } = new();
        public Dictionary<string, string> QueryParams { get; set; } = new();
        public object? Data { get; set; }
        public int? Timeout { get; set; }
        public bool IgnoreHTTPSErrors { get; set; } = false;
        public string? ContentType { get; set; }
    }

    public class PlaywrightApiUtils
    {
        private readonly IAPIRequestContext _apiContext;
        private readonly Logger _logger;
        private readonly ConfigManager _config;
        private readonly Dictionary<string, string> _defaultHeaders;

        public PlaywrightApiUtils(IAPIRequestContext apiContext)
        {
            _apiContext = apiContext;
            _logger = Logger.Instance;
            _config = ConfigManager.Instance;
            _defaultHeaders = new Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "Accept", "application/json" },
                { "User-Agent", "PlaywrightFramework-API-Client/1.0" }
            };
        }

        // Static factory method to create API context
        public static async Task<PlaywrightApiUtils> CreateAsync(IPlaywright playwright, string? baseUrl = null, Dictionary<string, string>? defaultHeaders = null)
        {
            var config = ConfigManager.Instance;
            var currentEnv = config.GetCurrentEnvironment();
            
            var contextOptions = new APIRequestNewContextOptions
            {
                BaseURL = baseUrl ?? currentEnv.BaseUrl,
                IgnoreHTTPSErrors = true,
                ExtraHTTPHeaders = defaultHeaders ?? new Dictionary<string, string>()
            };

            var apiContext = await playwright.APIRequest.NewContextAsync(contextOptions);
            return new PlaywrightApiUtils(apiContext);
        }

        #region HTTP Methods

        /// <summary>
        /// Performs a GET request
        /// </summary>
        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, ApiRequestOptions? options = null)
        {
            return await SendRequestAsync<T>("GET", endpoint, options);
        }

        /// <summary>
        /// Performs a POST request
        /// </summary>
        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null, ApiRequestOptions? options = null)
        {
            options ??= new ApiRequestOptions();
            options.Data = data;
            return await SendRequestAsync<T>("POST", endpoint, options);
        }

        /// <summary>
        /// Performs a PUT request
        /// </summary>
        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null, ApiRequestOptions? options = null)
        {
            options ??= new ApiRequestOptions();
            options.Data = data;
            return await SendRequestAsync<T>("PUT", endpoint, options);
        }

        /// <summary>
        /// Performs a PATCH request
        /// </summary>
        public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null, ApiRequestOptions? options = null)
        {
            options ??= new ApiRequestOptions();
            options.Data = data;
            return await SendRequestAsync<T>("PATCH", endpoint, options);
        }

        /// <summary>
        /// Performs a DELETE request
        /// </summary>
        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, ApiRequestOptions? options = null)
        {
            return await SendRequestAsync<T>("DELETE", endpoint, options);
        }

        /// <summary>
        /// Performs a HEAD request
        /// </summary>
        public async Task<ApiResponse<object>> HeadAsync(string endpoint, ApiRequestOptions? options = null)
        {
            return await SendRequestAsync<object>("HEAD", endpoint, options);
        }

        /// <summary>
        /// Performs an OPTIONS request
        /// </summary>
        public async Task<ApiResponse<object>> OptionsAsync(string endpoint, ApiRequestOptions? options = null)
        {
            return await SendRequestAsync<object>("OPTIONS", endpoint, options);
        }

        #endregion

        #region Core Request Method

        private async Task<ApiResponse<T>> SendRequestAsync<T>(string method, string endpoint, ApiRequestOptions? options = null)
        {
            var startTime = DateTime.UtcNow;
            options ??= new ApiRequestOptions();

            try
            {
                // Build URL with query parameters
                var url = BuildUrlWithQueryParams(endpoint, options.QueryParams);
                
                // Prepare headers
                var headers = MergeHeaders(_defaultHeaders, options.Headers);
                
                // Prepare request options
                var requestOptions = new APIRequestContextOptions
                {
                    Headers = headers,
                    Timeout = options.Timeout ?? _config.GetTestSettings().DefaultTimeout,
                    IgnoreHTTPSErrors = options.IgnoreHTTPSErrors
                };

                // Add data for methods that support body
                if (options.Data != null && (method == "POST" || method == "PUT" || method == "PATCH"))
                {
                    if (options.Data is string stringData)
                    {
                        requestOptions.Data = stringData;
                    }
                    else if (options.Data is byte[] byteData)
                    {
                        requestOptions.DataByte = byteData;
                    }
                    else
                    {
                        // Serialize object to JSON
                        var jsonData = JsonConvert.SerializeObject(options.Data, Formatting.None);
                        requestOptions.Data = jsonData;
                    }
                }

                _logger.LogAction($"API {method} Request", url);
                _logger.LogInfo($"Headers: {JsonConvert.SerializeObject(headers, Formatting.Indented)}");
                
                if (options.Data != null)
                {
                    var dataLog = options.Data is string str ? str : JsonConvert.SerializeObject(options.Data, Formatting.Indented);
                    _logger.LogInfo($"Request Body: {dataLog}");
                }

                // Send request
                IAPIResponse response = method.ToUpper() switch
                {
                    "GET" => await _apiContext.GetAsync(url, requestOptions),
                    "POST" => await _apiContext.PostAsync(url, requestOptions),
                    "PUT" => await _apiContext.PutAsync(url, requestOptions),
                    "PATCH" => await _apiContext.PatchAsync(url, requestOptions),
                    "DELETE" => await _apiContext.DeleteAsync(url, requestOptions),
                    "HEAD" => await _apiContext.HeadAsync(url, requestOptions),
                    "OPTIONS" => await _apiContext.FetchAsync(url, new APIRequestContextOptions { Method = "OPTIONS", Headers = headers }),
                    _ => throw new ArgumentException($"Unsupported HTTP method: {method}")
                };

                var endTime = DateTime.UtcNow;
                var responseTime = endTime - startTime;

                // Process response
                var apiResponse = await ProcessResponseAsync<T>(response, responseTime);
                
                _logger.LogInfo($"API Response - Status: {apiResponse.StatusCode} ({apiResponse.StatusText}), Time: {responseTime.TotalMilliseconds}ms");
                _logger.LogInfo($"Response Headers: {JsonConvert.SerializeObject(apiResponse.Headers, Formatting.Indented)}");
                _logger.LogInfo($"Response Body: {apiResponse.RawResponse}");

                return apiResponse;
            }
            catch (Exception ex)
            {
                var endTime = DateTime.UtcNow;
                var responseTime = endTime - startTime;
                
                _logger.LogError($"API {method} Request Failed: {endpoint}", ex);
                
                return new ApiResponse<T>
                {
                    StatusCode = 0,
                    StatusText = "Request Failed",
                    RawResponse = ex.Message,
                    ResponseTime = responseTime
                };
            }
        }

        #endregion

        #region Response Processing

        private async Task<ApiResponse<T>> ProcessResponseAsync<T>(IAPIResponse response, TimeSpan responseTime)
        {
            var rawResponse = await response.TextAsync();
            var headers = new Dictionary<string, string>();
            
            // Extract headers
            foreach (var header in response.Headers)
            {
                headers[header.Key] = header.Value;
            }

            var apiResponse = new ApiResponse<T>
            {
                StatusCode = response.Status,
                StatusText = response.StatusText,
                Headers = headers,
                RawResponse = rawResponse,
                ResponseTime = responseTime
            };

            // Try to deserialize response data
            if (!string.IsNullOrEmpty(rawResponse) && typeof(T) != typeof(object))
            {
                try
                {
                    if (typeof(T) == typeof(string))
                    {
                        apiResponse.Data = (T)(object)rawResponse;
                    }
                    else
                    {
                        apiResponse.Data = JsonConvert.DeserializeObject<T>(rawResponse);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning($"Failed to deserialize response to {typeof(T).Name}: {ex.Message}");
                    // Keep Data as null, but preserve RawResponse
                }
            }

            return apiResponse;
        }

        #endregion

        #region Authentication Methods

        /// <summary>
        /// Sets Bearer token for authentication
        /// </summary>
        public void SetBearerToken(string token)
        {
            _defaultHeaders["Authorization"] = $"Bearer {token}";
            _logger.LogAction("Set Bearer Token", "***");
        }

        /// <summary>
        /// Sets Basic authentication
        /// </summary>
        public void SetBasicAuth(string username, string password)
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            _defaultHeaders["Authorization"] = $"Basic {credentials}";
            _logger.LogAction("Set Basic Auth", username);
        }

        /// <summary>
        /// Sets API Key authentication
        /// </summary>
        public void SetApiKey(string key, string value, string location = "header")
        {
            if (location.ToLower() == "header")
            {
                _defaultHeaders[key] = value;
                _logger.LogAction($"Set API Key Header: {key}", "***");
            }
            else
            {
                _logger.LogWarning("Query parameter API keys should be added per request");
            }
        }

        /// <summary>
        /// Clears authentication headers
        /// </summary>
        public void ClearAuth()
        {
            _defaultHeaders.Remove("Authorization");
            _logger.LogAction("Clear Authentication");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Sets default header for all requests
        /// </summary>
        public void SetDefaultHeader(string key, string value)
        {
            _defaultHeaders[key] = value;
            _logger.LogAction($"Set Default Header: {key}", value);
        }

        /// <summary>
        /// Removes default header
        /// </summary>
        public void RemoveDefaultHeader(string key)
        {
            _defaultHeaders.Remove(key);
            _logger.LogAction($"Remove Default Header: {key}");
        }

        /// <summary>
        /// Gets current default headers
        /// </summary>
        public Dictionary<string, string> GetDefaultHeaders()
        {
            return new Dictionary<string, string>(_defaultHeaders);
        }

        private string BuildUrlWithQueryParams(string endpoint, Dictionary<string, string> queryParams)
        {
            if (queryParams == null || !queryParams.Any())
                return endpoint;

            var queryString = string.Join("&", queryParams.Select(kvp => 
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            var separator = endpoint.Contains("?") ? "&" : "?";
            return $"{endpoint}{separator}{queryString}";
        }

        private Dictionary<string, string> MergeHeaders(Dictionary<string, string> defaultHeaders, Dictionary<string, string> requestHeaders)
        {
            var merged = new Dictionary<string, string>(defaultHeaders);
            
            foreach (var header in requestHeaders)
            {
                merged[header.Key] = header.Value;
            }
            
            return merged;
        }

        #endregion

        #region Assertion Methods

        /// <summary>
        /// Asserts that the response status code matches expected value
        /// </summary>
        public void AssertStatusCode<T>(ApiResponse<T> response, int expectedStatusCode)
        {
            if (response.StatusCode != expectedStatusCode)
            {
                var message = $"Status Code Assertion Failed: Expected status code {expectedStatusCode}, but got {response.StatusCode} ({response.StatusText})";
                _logger.LogError(message);
                throw new AssertionException(message);
            }
            _logger.LogInfo($"✓ Status code assertion passed: {expectedStatusCode}");
        }

        /// <summary>
        /// Asserts that the response is successful (2xx status code)
        /// </summary>
        public void AssertSuccess<T>(ApiResponse<T> response)
        {
            if (!response.IsSuccess)
            {
                var message = $"Success Assertion Failed: Expected successful response, but got {response.StatusCode} ({response.StatusText})";
                _logger.LogError(message);
                throw new AssertionException(message);
            }
            _logger.LogInfo($"✓ Success assertion passed: {response.StatusCode}");
        }

        /// <summary>
        /// Asserts that response contains specific header
        /// </summary>
        public void AssertHeader<T>(ApiResponse<T> response, string headerName, string? expectedValue = null)
        {
            if (!response.Headers.ContainsKey(headerName))
            {
                var message = $"Header Assertion Failed: Expected header '{headerName}' not found in response";
                _logger.LogError(message);
                throw new AssertionException(message);
            }

            if (expectedValue != null && response.Headers[headerName] != expectedValue)
            {
                var message = $"Header Value Assertion Failed: Header '{headerName}' expected value '{expectedValue}', but got '{response.Headers[headerName]}'";
                _logger.LogError(message);
                throw new AssertionException(message);
            }

            _logger.LogInfo($"✓ Header assertion passed: {headerName}");
        }

        /// <summary>
        /// Asserts that response body contains specific text
        /// </summary>
        public void AssertResponseContains<T>(ApiResponse<T> response, string expectedText)
        {
            if (!response.RawResponse.Contains(expectedText))
            {
                var message = $"Response Content Assertion Failed: Response body does not contain expected text: '{expectedText}'";
                _logger.LogError(message);
                throw new AssertionException(message);
            }
            _logger.LogInfo($"✓ Response contains assertion passed: '{expectedText}'");
        }

        /// <summary>
        /// Asserts that response body matches regex pattern
        /// </summary>
        public void AssertResponseMatches<T>(ApiResponse<T> response, string pattern)
        {
            if (!Regex.IsMatch(response.RawResponse, pattern))
            {
                var message = $"Response Pattern Assertion Failed: Response body does not match pattern: '{pattern}'";
                _logger.LogError(message);
                throw new AssertionException(message);
            }
            _logger.LogInfo($"✓ Response pattern assertion passed: '{pattern}'");
        }

        /// <summary>
        /// Asserts that response time is within expected range
        /// </summary>
        public void AssertResponseTime<T>(ApiResponse<T> response, int maxMilliseconds)
        {
            if (response.ResponseTime.TotalMilliseconds > maxMilliseconds)
            {
                var message = $"Response Time Assertion Failed: Response time {response.ResponseTime.TotalMilliseconds}ms exceeded maximum {maxMilliseconds}ms";
                _logger.LogError(message);
                throw new AssertionException(message);
            }
            _logger.LogInfo($"✓ Response time assertion passed: {response.ResponseTime.TotalMilliseconds}ms <= {maxMilliseconds}ms");
        }

        #endregion

        #region File Upload/Download Methods

        /// <summary>
        /// Uploads a file using multipart/form-data
        /// </summary>
        public async Task<ApiResponse<T>> UploadFileAsync<T>(string endpoint, string filePath, string fieldName = "file", Dictionary<string, string>? additionalFields = null, ApiRequestOptions? options = null)
        {
            options ??= new ApiRequestOptions();
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            
            // Create multipart form data
            var boundary = "----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            var contentType = $"multipart/form-data; boundary={boundary}";
            
            var formData = new StringBuilder();
            
            // Add additional fields
            if (additionalFields != null)
            {
                foreach (var field in additionalFields)
                {
                    formData.AppendLine($"--{boundary}");
                    formData.AppendLine($"Content-Disposition: form-data; name=\"{field.Key}\"");
                    formData.AppendLine();
                    formData.AppendLine(field.Value);
                }
            }
            
            // Add file
            formData.AppendLine($"--{boundary}");
            formData.AppendLine($"Content-Disposition: form-data; name=\"{fieldName}\"; filename=\"{fileName}\"");
            formData.AppendLine($"Content-Type: {GetContentType(filePath)}");
            formData.AppendLine();
            
            var formDataBytes = Encoding.UTF8.GetBytes(formData.ToString());
            var endBoundaryBytes = Encoding.UTF8.GetBytes($"\r\n--{boundary}--\r\n");
            
            var requestData = new byte[formDataBytes.Length + fileBytes.Length + endBoundaryBytes.Length];
            Array.Copy(formDataBytes, 0, requestData, 0, formDataBytes.Length);
            Array.Copy(fileBytes, 0, requestData, formDataBytes.Length, fileBytes.Length);
            Array.Copy(endBoundaryBytes, 0, requestData, formDataBytes.Length + fileBytes.Length, endBoundaryBytes.Length);
            
            options.Data = requestData;
            options.ContentType = contentType;
            options.Headers["Content-Type"] = contentType;
            
            _logger.LogAction($"Upload File: {fileName}", endpoint);
            return await SendRequestAsync<T>("POST", endpoint, options);
        }

        /// <summary>
        /// Downloads a file from API response
        /// </summary>
        public async Task<string> DownloadFileAsync(string endpoint, string downloadPath, ApiRequestOptions? options = null)
        {
            var response = await GetAsync<byte[]>(endpoint, options);
            
            if (!response.IsSuccess)
            {
                throw new Exception($"Failed to download file: {response.StatusCode} - {response.StatusText}");
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(downloadPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Get response as bytes
            var apiResponse = await _apiContext.GetAsync(endpoint, new APIRequestContextOptions
            {
                Headers = MergeHeaders(_defaultHeaders, options?.Headers ?? new Dictionary<string, string>())
            });

            var fileBytes = await apiResponse.BodyAsync();
            await File.WriteAllBytesAsync(downloadPath, fileBytes);
            
            _logger.LogAction($"Downloaded File: {downloadPath}", endpoint);
            return downloadPath;
        }

        private string GetContentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Disposes the API context
        /// </summary>
        public async Task DisposeAsync()
        {
            await _apiContext.DisposeAsync();
            _logger.LogAction("API Context Disposed");
        }

        #endregion
    }

    // Custom exception for API assertions
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
        public AssertionException(string message, Exception innerException) : base(message, innerException) { }
    }
}