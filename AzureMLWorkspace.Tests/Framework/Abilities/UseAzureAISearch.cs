using Azure.Core;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Abilities;

/// <summary>
/// Ability to interact with Azure AI Search services
/// </summary>
public class UseAzureAISearch : IAbility
{
    private readonly ILogger<UseAzureAISearch> _logger;
    private readonly IConfiguration _configuration;
    
    private SearchClient? _searchClient;
    private SearchIndexClient? _indexClient;
    private string? _serviceName;
    private string? _indexName;

    public string Name => "Use Azure AI Search";
    public SearchClient SearchClient => _searchClient ?? throw new InvalidOperationException("Search client not initialized");
    public SearchIndexClient IndexClient => _indexClient ?? throw new InvalidOperationException("Index client not initialized");
    public string ServiceName => _serviceName ?? throw new InvalidOperationException("Service name not set");
    public string IndexName => _indexName ?? throw new InvalidOperationException("Index name not set");

    private UseAzureAISearch(ILogger<UseAzureAISearch> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Azure AI Search client");

        try
        {
            _serviceName = _configuration["AzureAISearch:ServiceName"] ?? 
                throw new InvalidOperationException("AzureAISearch:ServiceName not configured");
            _indexName = _configuration["AzureAISearch:IndexName"] ?? 
                throw new InvalidOperationException("AzureAISearch:IndexName not configured");

            var endpoint = new Uri($"https://{_serviceName}.search.windows.net");
            var credential = new DefaultAzureCredential();

            _indexClient = new SearchIndexClient(endpoint, credential);
            _searchClient = new SearchClient(endpoint, _indexName, credential);

            // Test the connection
            await _indexClient.GetIndexAsync(_indexName);

            _logger.LogInformation("Azure AI Search client initialized successfully for service {ServiceName}, index {IndexName}", 
                _serviceName, _indexName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Azure AI Search client");
            throw;
        }
    }

    public async Task CleanupAsync()
    {
        _logger.LogInformation("Cleaning up Azure AI Search resources");
        
        _searchClient = null;
        _indexClient = null;
        _serviceName = null;
        _indexName = null;
        
        _logger.LogInformation("Azure AI Search cleanup completed");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates a new UseAzureAISearch ability
    /// </summary>
    public static UseAzureAISearch WithDefaultConfiguration()
    {
        return new UseAzureAISearch(
            AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<UseAzureAISearch>>(),
            AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<IConfiguration>());
    }

    /// <summary>
    /// Searches for documents in the index
    /// </summary>
    public async Task<SearchResults<SearchDocument>> Search(string query, SearchOptions? options = null)
    {
        _logger.LogInformation("Searching for: {Query}", query);
        
        try
        {
            var searchOptions = options ?? new SearchOptions
            {
                IncludeTotalCount = true,
                Size = 50
            };

            var results = await _searchClient.SearchAsync<SearchDocument>(query, searchOptions);
            
            _logger.LogInformation("Search completed. Found {TotalCount} results", results.Value.TotalCount);
            return results.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed for query: {Query}", query);
            throw;
        }
    }

    /// <summary>
    /// Gets the count of search results for a query
    /// </summary>
    public async Task<long?> GetSearchResultCount(string query)
    {
        var searchOptions = new SearchOptions
        {
            IncludeTotalCount = true,
            Size = 0 // We only want the count, not the actual results
        };

        var results = await Search(query, searchOptions);
        return results.TotalCount;
    }

    /// <summary>
    /// Tests AI Search functionality with a specific query and index
    /// </summary>
    public async Task<SearchTestResult> TestAISearch(string query, string? customIndexName = null)
    {
        _logger.LogInformation("Testing AI Search with query: {Query}", query);
        
        try
        {
            var indexToUse = customIndexName ?? _indexName;
            var searchClient = customIndexName != null 
                ? new SearchClient(_indexClient.Endpoint, customIndexName, new DefaultAzureCredential())
                : _searchClient;

            var startTime = DateTime.UtcNow;
            var results = await searchClient.SearchAsync<SearchDocument>(query, new SearchOptions
            {
                IncludeTotalCount = true,
                Size = 10,
                HighlightFields = { "*" }
            });
            var endTime = DateTime.UtcNow;

            var testResult = new SearchTestResult
            {
                Query = query,
                IndexName = indexToUse,
                TotalResults = results.Value.TotalCount ?? 0,
                ResponseTime = endTime - startTime,
                Success = true,
                Results = new List<SearchResult<SearchDocument>>()
            };

            _logger.LogInformation("AI Search test completed successfully. Query: {Query}, Results: {ResultCount}, Time: {ResponseTime}ms", 
                query, testResult.TotalResults, testResult.ResponseTime.TotalMilliseconds);

            return testResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI Search test failed for query: {Query}", query);
            
            return new SearchTestResult
            {
                Query = query,
                IndexName = customIndexName ?? _indexName,
                Success = false,
                Error = ex.Message,
                Results = new List<SearchResult<SearchDocument>>()
            };
        }
    }

    /// <summary>
    /// Gets index statistics
    /// </summary>
    public async Task<SearchIndexStatistics> GetIndexStatistics()
    {
        try
        {
            var stats = await _indexClient.GetIndexStatisticsAsync(_indexName);
            return stats.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get index statistics for {IndexName}", _indexName);
            throw;
        }
    }
}

/// <summary>
/// Result of an AI Search test
/// </summary>
public class SearchTestResult
{
    public string Query { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public long TotalResults { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public IList<SearchResult<SearchDocument>> Results { get; set; } = new List<SearchResult<SearchDocument>>();
}