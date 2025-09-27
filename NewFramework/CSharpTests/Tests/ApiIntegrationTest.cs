using Microsoft.Playwright;
using PlaywrightFramework.Utils;
using NUnit.Framework;

namespace PlaywrightFramework.Tests
{
    /// <summary>
    /// Integration test to verify API utilities work correctly with the framework
    /// </summary>
    [TestFixture]
    public class ApiIntegrationTest
    {
        private IPlaywright? _playwright;
        private PlaywrightApiUtils? _apiClient;
        private Logger? _logger;
        private ConfigManager? _config;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _playwright = await Playwright.CreateAsync();
            _logger = Logger.Instance;
            _config = ConfigManager.Instance;
        }

        [SetUp]
        public async Task SetUp()
        {
            // Use JSONPlaceholder as a test API
            _apiClient = await PlaywrightApiUtils.CreateAsync(_playwright!, "https://jsonplaceholder.typicode.com");
            
            // Set some default headers
            _apiClient.SetDefaultHeader("User-Agent", "PlaywrightFramework-Test/1.0");
        }

        [TearDown]
        public async Task TearDown()
        {
            if (_apiClient != null)
            {
                await _apiClient.DisposeAsync();
            }
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            if (_playwright != null)
            {
                _playwright.Dispose();
            }
        }

        [Test]
        [Category("Integration")]
        [Category("API")]
        public async Task ApiClient_BasicFunctionality_WorksCorrectly()
        {
            // Test GET request
            var getResponse = await _apiClient!.GetAsync<Post>("/posts/1");
            
            // Verify response
            _apiClient.AssertSuccess(getResponse);
            _apiClient.AssertStatusCode(getResponse, 200);
            _apiClient.AssertHeader(getResponse, "Content-Type");
            _apiClient.AssertResponseContains(getResponse, "userId");
            _apiClient.AssertResponseTime(getResponse, 10000); // 10 seconds max
            
            // Verify data
            Assert.That(getResponse.Data, Is.Not.Null);
            Assert.That(getResponse.Data!.Id, Is.EqualTo(1));
            Assert.That(getResponse.Data.Title, Is.Not.Empty);
            Assert.That(getResponse.Data.Body, Is.Not.Empty);
            Assert.That(getResponse.Data.UserId, Is.GreaterThan(0));
            
            _logger!.LogInfo("✓ GET request test passed");
        }

        [Test]
        [Category("Integration")]
        [Category("API")]
        public async Task ApiClient_PostRequest_WorksCorrectly()
        {
            // Test POST request
            var newPost = new Post
            {
                Title = "Integration Test Post",
                Body = "This is a test post created during integration testing",
                UserId = 1
            };

            var postResponse = await _apiClient!.PostAsync<Post>("/posts", newPost);
            
            // Verify response
            _apiClient.AssertSuccess(postResponse);
            _apiClient.AssertStatusCode(postResponse, 201);
            
            // Verify data
            Assert.That(postResponse.Data, Is.Not.Null);
            Assert.That(postResponse.Data!.Title, Is.EqualTo(newPost.Title));
            Assert.That(postResponse.Data.Body, Is.EqualTo(newPost.Body));
            Assert.That(postResponse.Data.UserId, Is.EqualTo(newPost.UserId));
            
            _logger!.LogInfo("✓ POST request test passed");
        }

        [Test]
        [Category("Integration")]
        [Category("API")]
        public async Task ApiClient_QueryParameters_WorkCorrectly()
        {
            // Test GET request with query parameters
            var options = new ApiRequestOptions
            {
                QueryParams = new Dictionary<string, string>
                {
                    ["userId"] = "1",
                    ["_limit"] = "3"
                }
            };

            var response = await _apiClient!.GetAsync<List<Post>>("/posts", options);
            
            // Verify response
            _apiClient.AssertSuccess(response);
            _apiClient.AssertStatusCode(response, 200);
            
            // Verify data
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Count, Is.LessThanOrEqualTo(3));
            
            // Verify all posts belong to userId 1
            foreach (var post in response.Data)
            {
                Assert.That(post.UserId, Is.EqualTo(1));
            }
            
            _logger!.LogInfo("✓ Query parameters test passed");
        }

        [Test]
        [Category("Integration")]
        [Category("API")]
        public async Task ApiClient_Authentication_WorksCorrectly()
        {
            // Test Bearer token authentication
            var testToken = "test-bearer-token-12345";
            _apiClient!.SetBearerToken(testToken);
            
            var response = await _apiClient.GetAsync<Post>("/posts/1");
            
            // Verify response (JSONPlaceholder doesn't validate auth, but request should succeed)
            _apiClient.AssertSuccess(response);
            
            // Verify headers were set
            var headers = _apiClient.GetDefaultHeaders();
            Assert.That(headers.ContainsKey("Authorization"), Is.True);
            Assert.That(headers["Authorization"], Is.EqualTo($"Bearer {testToken}"));
            
            _logger!.LogInfo("✓ Authentication test passed");
        }

        [Test]
        [Category("Integration")]
        [Category("API")]
        public async Task ApiClient_ErrorHandling_WorksCorrectly()
        {
            // Test 404 error handling
            var response = await _apiClient!.GetAsync<Post>("/posts/999999");
            
            // Verify error response
            _apiClient.AssertStatusCode(response, 404);
            Assert.That(response.IsSuccess, Is.False);
            Assert.That(response.Data, Is.Null);
            
            _logger!.LogInfo("✓ Error handling test passed");
        }

        [Test]
        [Category("Integration")]
        [Category("API")]
        public async Task ApiTestHelpers_CrudTest_WorksCorrectly()
        {
            // Test CRUD helper method
            var createData = new Post
            {
                Title = "CRUD Test Post",
                Body = "This post is for CRUD testing",
                UserId = 1
            };

            var updateData = new Post
            {
                Title = "Updated CRUD Test Post",
                Body = "This post has been updated",
                UserId = 1
            };

            var crudResult = await ApiTestHelpers.PerformCrudTestAsync<Post, Post, Post>(
                _apiClient!,
                "/posts",
                createData,
                updateData,
                post => post.Id
            );

            // Verify CRUD operations
            Assert.That(crudResult.Success, Is.True, crudResult.Error);
            Assert.That(crudResult.CreatedId, Is.Not.Null);
            Assert.That(crudResult.CreateResponse, Is.Not.Null);
            Assert.That(crudResult.ReadResponse, Is.Not.Null);
            Assert.That(crudResult.UpdateResponse, Is.Not.Null);
            Assert.That(crudResult.DeleteResponse, Is.Not.Null);
            
            _logger!.LogInfo("✓ CRUD helper test passed");
        }

        [Test]
        [Category("Integration")]
        [Category("API")]
        public async Task ApiTestHelpers_TestDataGenerator_WorksCorrectly()
        {
            // Test data generators
            var randomString = ApiTestHelpers.TestDataGenerator.RandomString(10);
            var randomEmail = ApiTestHelpers.TestDataGenerator.RandomEmail();
            var randomInt = ApiTestHelpers.TestDataGenerator.RandomInt(1, 100);
            var randomDate = ApiTestHelpers.TestDataGenerator.RandomDate();
            var randomPhone = ApiTestHelpers.TestDataGenerator.RandomPhoneNumber();
            var randomDecimal = ApiTestHelpers.TestDataGenerator.RandomDecimal(0, 1000);

            // Verify generated data
            Assert.That(randomString, Is.Not.Empty);
            Assert.That(randomString.Length, Is.EqualTo(10));
            
            Assert.That(randomEmail, Is.Not.Empty);
            Assert.That(randomEmail, Does.Contain("@"));
            Assert.That(randomEmail, Does.Contain("."));
            
            Assert.That(randomInt, Is.InRange(1, 100));
            
            Assert.That(randomDate, Is.InRange(DateTime.Now.AddYears(-1), DateTime.Now.AddYears(1)));
            
            Assert.That(randomPhone, Is.Not.Empty);
            Assert.That(randomPhone, Does.StartWith("+1"));
            
            Assert.That(randomDecimal, Is.InRange(0, 1000));
            
            _logger!.LogInfo("✓ Test data generator test passed");
        }

        [Test]
        [Category("Integration")]
        [Category("API")]
        public async Task ApiClient_JsonExtraction_WorksCorrectly()
        {
            // Test JSON value extraction
            var response = await _apiClient!.GetAsync<Post>("/posts/1");
            _apiClient.AssertSuccess(response);

            // Extract values using JSONPath
            var id = ApiTestHelpers.ExtractJsonValue<int>(response.RawResponse, "$.id");
            var title = ApiTestHelpers.ExtractJsonValue<string>(response.RawResponse, "$.title");
            var userId = ApiTestHelpers.ExtractJsonValue<int>(response.RawResponse, "$.userId");

            // Verify extracted values
            Assert.That(id, Is.EqualTo(1));
            Assert.That(title, Is.Not.Empty);
            Assert.That(userId, Is.GreaterThan(0));

            _logger!.LogInfo("✓ JSON extraction test passed");
        }

        [Test]
        [Category("Integration")]
        [Category("API")]
        [Category("Performance")]
        public async Task ApiClient_PerformanceTest_MeetsRequirements()
        {
            // Test multiple requests for performance
            var tasks = new List<Task<ApiResponse<Post>>>();
            
            for (int i = 1; i <= 5; i++)
            {
                tasks.Add(_apiClient!.GetAsync<Post>($"/posts/{i}"));
            }

            var responses = await Task.WhenAll(tasks);

            // Verify all requests succeeded
            foreach (var response in responses)
            {
                _apiClient!.AssertSuccess(response);
                _apiClient.AssertResponseTime(response, 10000); // 10 seconds max per request
            }

            // Calculate average response time
            var averageResponseTime = responses.Average(r => r.ResponseTime.TotalMilliseconds);
            Assert.That(averageResponseTime, Is.LessThan(5000), "Average response time should be less than 5 seconds");

            _logger!.LogInfo($"✓ Performance test passed - Average response time: {averageResponseTime:F2}ms");
        }

        #region Data Models

        public class Post
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
            public int UserId { get; set; }
        }

        #endregion
    }
}