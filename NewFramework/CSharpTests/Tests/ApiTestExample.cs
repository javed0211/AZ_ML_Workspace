using Microsoft.Playwright;
using PlaywrightFramework.Utils;
using NUnit.Framework;

namespace PlaywrightFramework.Tests
{
    /// <summary>
    /// Example test class demonstrating API automation using PlaywrightApiUtils
    /// </summary>
    [TestFixture]
    public class ApiTestExample
    {
        private IPlaywright? _playwright;
        private PlaywrightApiUtils? _apiClient;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _playwright = await Playwright.CreateAsync();
        }

        [SetUp]
        public async Task SetUp()
        {
            // Create API client with base URL
            _apiClient = await PlaywrightApiUtils.CreateAsync(_playwright!, "https://jsonplaceholder.typicode.com");
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

        #region Basic HTTP Methods Tests

        [Test]
        public async Task Test_GET_Request()
        {
            // Arrange
            var endpoint = "/posts/1";

            // Act
            var response = await _apiClient!.GetAsync<Post>(endpoint);

            // Assert
            _apiClient.AssertSuccess(response);
            _apiClient.AssertStatusCode(response, 200);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Id, Is.EqualTo(1));
            Assert.That(response.Data.Title, Is.Not.Empty);
        }

        [Test]
        public async Task Test_POST_Request()
        {
            // Arrange
            var endpoint = "/posts";
            var newPost = new Post
            {
                Title = "Test Post",
                Body = "This is a test post body",
                UserId = 1
            };

            // Act
            var response = await _apiClient!.PostAsync<Post>(endpoint, newPost);

            // Assert
            _apiClient.AssertSuccess(response);
            _apiClient.AssertStatusCode(response, 201);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Title, Is.EqualTo(newPost.Title));
            Assert.That(response.Data.Body, Is.EqualTo(newPost.Body));
        }

        [Test]
        public async Task Test_PUT_Request()
        {
            // Arrange
            var endpoint = "/posts/1";
            var updatedPost = new Post
            {
                Id = 1,
                Title = "Updated Test Post",
                Body = "This is an updated test post body",
                UserId = 1
            };

            // Act
            var response = await _apiClient!.PutAsync<Post>(endpoint, updatedPost);

            // Assert
            _apiClient.AssertSuccess(response);
            _apiClient.AssertStatusCode(response, 200);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Title, Is.EqualTo(updatedPost.Title));
        }

        [Test]
        public async Task Test_DELETE_Request()
        {
            // Arrange
            var endpoint = "/posts/1";

            // Act
            var response = await _apiClient!.DeleteAsync<object>(endpoint);

            // Assert
            _apiClient.AssertSuccess(response);
            _apiClient.AssertStatusCode(response, 200);
        }

        #endregion

        #region Authentication Tests

        [Test]
        public async Task Test_Bearer_Token_Authentication()
        {
            // Arrange
            var token = "test-bearer-token";
            _apiClient!.SetBearerToken(token);

            // Act
            var response = await _apiClient.GetAsync<Post>("/posts/1");

            // Assert
            var headers = _apiClient.GetDefaultHeaders();
            Assert.That(headers["Authorization"], Is.EqualTo($"Bearer {token}"));
        }

        [Test]
        public async Task Test_Basic_Authentication()
        {
            // Arrange
            var username = "testuser";
            var password = "testpass";
            _apiClient!.SetBasicAuth(username, password);

            // Act
            var response = await _apiClient.GetAsync<Post>("/posts/1");

            // Assert
            var headers = _apiClient.GetDefaultHeaders();
            Assert.That(headers.ContainsKey("Authorization"), Is.True);
            Assert.That(headers["Authorization"], Does.StartWith("Basic "));
        }

        [Test]
        public async Task Test_API_Key_Authentication()
        {
            // Arrange
            var apiKey = "test-api-key";
            _apiClient!.SetApiKey("X-API-Key", apiKey);

            // Act
            var response = await _apiClient.GetAsync<Post>("/posts/1");

            // Assert
            var headers = _apiClient.GetDefaultHeaders();
            Assert.That(headers["X-API-Key"], Is.EqualTo(apiKey));
        }

        #endregion

        #region Query Parameters and Headers Tests

        [Test]
        public async Task Test_Query_Parameters()
        {
            // Arrange
            var endpoint = "/posts";
            var options = new ApiRequestOptions
            {
                QueryParams = new Dictionary<string, string>
                {
                    ["userId"] = "1",
                    ["_limit"] = "5"
                }
            };

            // Act
            var response = await _apiClient!.GetAsync<List<Post>>(endpoint, options);

            // Assert
            _apiClient.AssertSuccess(response);
            Assert.That(response.Data, Is.Not.Null);
            Assert.That(response.Data!.Count, Is.LessThanOrEqualTo(5));
        }

        [Test]
        public async Task Test_Custom_Headers()
        {
            // Arrange
            var endpoint = "/posts/1";
            var options = new ApiRequestOptions
            {
                Headers = new Dictionary<string, string>
                {
                    ["X-Custom-Header"] = "custom-value",
                    ["Accept-Language"] = "en-US"
                }
            };

            // Act
            var response = await _apiClient!.GetAsync<Post>(endpoint, options);

            // Assert
            _apiClient.AssertSuccess(response);
            Assert.That(response.Data, Is.Not.Null);
        }

        #endregion

        #region Response Validation Tests

        [Test]
        public async Task Test_Response_Headers_Validation()
        {
            // Arrange
            var endpoint = "/posts/1";

            // Act
            var response = await _apiClient!.GetAsync<Post>(endpoint);

            // Assert
            _apiClient.AssertSuccess(response);
            _apiClient.AssertHeader(response, "Content-Type");
            Assert.That(response.Headers["Content-Type"], Does.Contain("application/json"));
        }

        [Test]
        public async Task Test_Response_Content_Validation()
        {
            // Arrange
            var endpoint = "/posts/1";

            // Act
            var response = await _apiClient!.GetAsync<Post>(endpoint);

            // Assert
            _apiClient.AssertSuccess(response);
            _apiClient.AssertResponseContains(response, "userId");
            _apiClient.AssertResponseContains(response, "title");
            _apiClient.AssertResponseMatches(response, @"""id"":\s*1");
        }

        [Test]
        public async Task Test_Response_Time_Validation()
        {
            // Arrange
            var endpoint = "/posts/1";

            // Act
            var response = await _apiClient!.GetAsync<Post>(endpoint);

            // Assert
            _apiClient.AssertSuccess(response);
            _apiClient.AssertResponseTime(response, 5000); // 5 seconds max
            Assert.That(response.ResponseTime.TotalMilliseconds, Is.GreaterThan(0));
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public async Task Test_404_Error_Handling()
        {
            // Arrange
            var endpoint = "/posts/999999"; // Non-existent post

            // Act
            var response = await _apiClient!.GetAsync<Post>(endpoint);

            // Assert
            _apiClient.AssertStatusCode(response, 404);
            Assert.That(response.IsSuccess, Is.False);
        }

        [Test]
        public async Task Test_Invalid_JSON_Handling()
        {
            // Arrange
            var endpoint = "/posts";
            var invalidData = "{ invalid json }";

            // Act
            var response = await _apiClient!.PostAsync<Post>(endpoint, invalidData);

            // Assert - Should handle gracefully without throwing
            Assert.That(response, Is.Not.Null);
        }

        #endregion

        #region Advanced Testing Scenarios

        [Test]
        public async Task Test_CRUD_Operations_Complete_Flow()
        {
            // This test demonstrates a complete CRUD flow
            var baseEndpoint = "/posts";
            
            // CREATE
            var newPost = new Post
            {
                Title = $"Test Post {DateTime.Now.Ticks}",
                Body = "Test post body for CRUD operations",
                UserId = 1
            };

            var createResponse = await _apiClient!.PostAsync<Post>(baseEndpoint, newPost);
            _apiClient.AssertSuccess(createResponse);
            var createdId = createResponse.Data!.Id;

            // READ
            var readResponse = await _apiClient.GetAsync<Post>($"{baseEndpoint}/{createdId}");
            _apiClient.AssertSuccess(readResponse);
            Assert.That(readResponse.Data!.Title, Is.EqualTo(newPost.Title));

            // UPDATE
            var updatedPost = new Post
            {
                Id = createdId,
                Title = "Updated " + newPost.Title,
                Body = "Updated " + newPost.Body,
                UserId = newPost.UserId
            };

            var updateResponse = await _apiClient.PutAsync<Post>($"{baseEndpoint}/{createdId}", updatedPost);
            _apiClient.AssertSuccess(updateResponse);

            // DELETE
            var deleteResponse = await _apiClient.DeleteAsync<object>($"{baseEndpoint}/{createdId}");
            _apiClient.AssertSuccess(deleteResponse);
        }

        [Test]
        public async Task Test_Pagination_Handling()
        {
            // Arrange
            var endpoint = "/posts";
            var allPosts = new List<Post>();
            var page = 1;
            var pageSize = 10;

            // Act - Fetch multiple pages
            while (page <= 3) // Limit to 3 pages for test
            {
                var options = new ApiRequestOptions
                {
                    QueryParams = new Dictionary<string, string>
                    {
                        ["_page"] = page.ToString(),
                        ["_limit"] = pageSize.ToString()
                    }
                };

                var response = await _apiClient!.GetAsync<List<Post>>(endpoint, options);
                _apiClient.AssertSuccess(response);

                if (response.Data != null && response.Data.Any())
                {
                    allPosts.AddRange(response.Data);
                    page++;
                }
                else
                {
                    break;
                }
            }

            // Assert
            Assert.That(allPosts.Count, Is.GreaterThan(0));
            Assert.That(allPosts.Count, Is.LessThanOrEqualTo(30)); // 3 pages * 10 items
        }

        #endregion

        #region Helper Methods and Data Models

        /// <summary>
        /// Data model for Post entity
        /// </summary>
        public class Post
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Body { get; set; } = string.Empty;
            public int UserId { get; set; }
        }

        /// <summary>
        /// Data model for User entity
        /// </summary>
        public class User
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public Address? Address { get; set; }
            public string Phone { get; set; } = string.Empty;
            public string Website { get; set; } = string.Empty;
            public Company? Company { get; set; }
        }

        public class Address
        {
            public string Street { get; set; } = string.Empty;
            public string Suite { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Zipcode { get; set; } = string.Empty;
            public Geo? Geo { get; set; }
        }

        public class Geo
        {
            public string Lat { get; set; } = string.Empty;
            public string Lng { get; set; } = string.Empty;
        }

        public class Company
        {
            public string Name { get; set; } = string.Empty;
            public string CatchPhrase { get; set; } = string.Empty;
            public string Bs { get; set; } = string.Empty;
        }

        #endregion
    }
}