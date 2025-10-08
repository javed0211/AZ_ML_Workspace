using Microsoft.Playwright;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace PlaywrightFramework.Utils
{
    /// <summary>
    /// Helper class for common API testing patterns and scenarios
    /// </summary>
    public static class ApiTestHelpers
    {
        /// <summary>
        /// Creates a new API context with common configuration
        /// </summary>
        public static async Task<PlaywrightApiUtils> CreateApiClientAsync(IPlaywright playwright, string? baseUrl = null, string? authToken = null)
        {
            var defaultHeaders = new Dictionary<string, string>();
            
            if (!string.IsNullOrEmpty(authToken))
            {
                defaultHeaders["Authorization"] = $"Bearer {authToken}";
            }

            var apiClient = await PlaywrightApiUtils.CreateAsync(playwright, baseUrl, defaultHeaders);
            return apiClient;
        }

        /// <summary>
        /// Performs a complete CRUD test cycle
        /// </summary>
        public static async Task<CrudTestResult> PerformCrudTestAsync<TCreate, TRead, TUpdate>(
            PlaywrightApiUtils apiClient,
            string baseEndpoint,
            TCreate createData,
            TUpdate updateData,
            Func<TRead, object> getIdFunc)
        {
            var result = new CrudTestResult();

            try
            {
                // CREATE
                var createResponse = await apiClient.PostAsync<TRead>(baseEndpoint, createData);
                apiClient.AssertSuccess(createResponse);
                result.CreateResponse = createResponse;
                result.CreatedId = getIdFunc(createResponse.Data!);

                // READ
                var readEndpoint = $"{baseEndpoint}/{result.CreatedId}";
                var readResponse = await apiClient.GetAsync<TRead>(readEndpoint);
                apiClient.AssertSuccess(readResponse);
                result.ReadResponse = readResponse;

                // UPDATE
                var updateResponse = await apiClient.PutAsync<TRead>(readEndpoint, updateData);
                apiClient.AssertSuccess(updateResponse);
                result.UpdateResponse = updateResponse;

                // DELETE
                var deleteResponse = await apiClient.DeleteAsync<object>(readEndpoint);
                apiClient.AssertSuccess(deleteResponse);
                result.DeleteResponse = deleteResponse;

                // VERIFY DELETE
                var verifyDeleteResponse = await apiClient.GetAsync<TRead>(readEndpoint);
                if (verifyDeleteResponse.StatusCode != 404)
                {
                    throw new Exception($"Expected 404 after delete, but got {verifyDeleteResponse.StatusCode}");
                }
                result.VerifyDeleteResponse = verifyDeleteResponse;

                result.Success = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Tests API pagination
        /// </summary>
        public static async Task<PaginationTestResult<T>> TestPaginationAsync<T>(
            PlaywrightApiUtils apiClient,
            string endpoint,
            int pageSize = 10,
            int maxPages = 5)
        {
            var result = new PaginationTestResult<T>();
            var allItems = new List<T>();

            try
            {
                for (int page = 1; page <= maxPages; page++)
                {
                    var options = new ApiRequestOptions
                    {
                        QueryParams = new Dictionary<string, string>
                        {
                            ["page"] = page.ToString(),
                            ["limit"] = pageSize.ToString()
                        }
                    };

                    var response = await apiClient.GetAsync<PaginatedResponse<T>>(endpoint, options);
                    apiClient.AssertSuccess(response);

                    if (response.Data?.Items != null)
                    {
                        allItems.AddRange(response.Data.Items);
                        result.Pages.Add(response);

                        if (response.Data.Items.Count < pageSize)
                        {
                            // Last page reached
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                result.AllItems = allItems;
                result.TotalItemsRetrieved = allItems.Count;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Tests API rate limiting
        /// </summary>
        public static async Task<RateLimitTestResult> TestRateLimitAsync(
            PlaywrightApiUtils apiClient,
            string endpoint,
            int requestCount = 100,
            TimeSpan? delay = null)
        {
            var result = new RateLimitTestResult();
            var responses = new List<ApiResponse<object>>();

            try
            {
                for (int i = 0; i < requestCount; i++)
                {
                    var response = await apiClient.GetAsync<object>(endpoint);
                    responses.Add(response);

                    if (response.StatusCode == 429) // Too Many Requests
                    {
                        result.RateLimitHit = true;
                        result.RequestsBeforeLimit = i + 1;
                        
                        // Check for Retry-After header
                        if (response.Headers.ContainsKey("Retry-After"))
                        {
                            result.RetryAfterSeconds = int.Parse(response.Headers["Retry-After"]);
                        }
                        break;
                    }

                    if (delay.HasValue)
                    {
                        await Task.Delay(delay.Value);
                    }
                }

                result.TotalRequests = responses.Count;
                result.SuccessfulRequests = responses.Count(r => r.IsSuccess);
                result.Responses = responses;
                result.Success = true;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Validates JSON schema of API response
        /// </summary>
        public static bool ValidateJsonSchema<T>(ApiResponse<T> response, JSchema schema)
        {
            try
            {
                var json = JToken.Parse(response.RawResponse);
                return json.IsValid(schema);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts value from JSON response using JSONPath
        /// </summary>
        public static T? ExtractJsonValue<T>(string jsonResponse, string jsonPath)
        {
            try
            {
                var json = JToken.Parse(jsonResponse);
                var token = json.SelectToken(jsonPath);
                return token != null ? token.ToObject<T>() : default;
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Compares two JSON objects ignoring specified fields
        /// </summary>
        public static bool CompareJsonIgnoringFields(string json1, string json2, params string[] fieldsToIgnore)
        {
            try
            {
                var obj1 = JObject.Parse(json1);
                var obj2 = JObject.Parse(json2);

                foreach (var field in fieldsToIgnore)
                {
                    obj1.Remove(field);
                    obj2.Remove(field);
                }

                return JToken.DeepEquals(obj1, obj2);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generates test data for API testing
        /// </summary>
        public static class TestDataGenerator
        {
            private static readonly Random _random = new();

            public static string RandomString(int length = 10)
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[_random.Next(s.Length)]).ToArray());
            }

            public static string RandomEmail()
            {
                return $"{RandomString(8)}@{RandomString(6)}.com";
            }

            public static int RandomInt(int min = 1, int max = 1000)
            {
                return _random.Next(min, max);
            }

            public static DateTime RandomDate(DateTime? start = null, DateTime? end = null)
            {
                start ??= DateTime.Now.AddYears(-1);
                end ??= DateTime.Now.AddYears(1);
                
                var range = end.Value - start.Value;
                var randomTimeSpan = new TimeSpan((long)(_random.NextDouble() * range.Ticks));
                return start.Value + randomTimeSpan;
            }

            public static string RandomPhoneNumber()
            {
                return $"+1{_random.Next(100, 999)}{_random.Next(100, 999)}{_random.Next(1000, 9999)}";
            }

            public static decimal RandomDecimal(decimal min = 0, decimal max = 1000)
            {
                return (decimal)(_random.NextDouble() * (double)(max - min)) + min;
            }
        }
    }

    #region Result Classes

    public class CrudTestResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public object? CreatedId { get; set; }
        public object? CreateResponse { get; set; }
        public object? ReadResponse { get; set; }
        public object? UpdateResponse { get; set; }
        public object? DeleteResponse { get; set; }
        public object? VerifyDeleteResponse { get; set; }
    }

    public class PaginationTestResult<T>
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public List<ApiResponse<PaginatedResponse<T>>> Pages { get; set; } = new();
        public List<T> AllItems { get; set; } = new();
        public int TotalItemsRetrieved { get; set; }
    }

    public class RateLimitTestResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public bool RateLimitHit { get; set; }
        public int RequestsBeforeLimit { get; set; }
        public int? RetryAfterSeconds { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public List<ApiResponse<object>> Responses { get; set; } = new();
    }

    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }

    #endregion
}