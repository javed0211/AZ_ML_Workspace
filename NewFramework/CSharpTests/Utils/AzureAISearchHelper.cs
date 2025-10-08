using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Serilog;
using System.Diagnostics;

namespace PlaywrightFramework.Utils
{
    public class SearchIndexField
    {
        public string FieldName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Searchable { get; set; }
        public bool Filterable { get; set; }
        public bool Sortable { get; set; }
        public bool Facetable { get; set; }
        public bool IsKey { get; set; }
    }

    public class SearchDocument
    {
        public Dictionary<string, object> Fields { get; set; } = new();
    }

    public class SearchQueryOptions
    {
        public string? Filter { get; set; }
        public List<string>? Facets { get; set; }
        public List<string>? OrderBy { get; set; }
        public int? Top { get; set; }
        public int? Skip { get; set; }
        public bool IncludeTotalCount { get; set; }
        public string? SearchMode { get; set; }
        public string? QueryType { get; set; }
        public List<string>? Select { get; set; }
        public string? SemanticConfigurationName { get; set; }
    }

    public class SearchResultItem
    {
        public Dictionary<string, object> Document { get; set; } = new();
        public double Score { get; set; }
        public string? SemanticCaption { get; set; }
        public Dictionary<string, object>? Highlights { get; set; }
    }

    public class SearchResults
    {
        public List<SearchResultItem> Items { get; set; } = new();
        public long? TotalCount { get; set; }
        public Dictionary<string, List<FacetValue>>? Facets { get; set; }
        public double SearchDurationMs { get; set; }
    }

    public class FacetValue
    {
        public object Value { get; set; } = new();
        public long Count { get; set; }
    }

    public class AzureAISearchHelper
    {
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly ILogger _logger;
        private SearchIndexClient? _indexClient;
        private readonly Dictionary<string, SearchClient> _searchClients = new();

        public AzureAISearchHelper(string endpoint, string apiKey)
        {
            _endpoint = endpoint;
            _apiKey = apiKey;
            _logger = Log.ForContext<AzureAISearchHelper>();
        }

        private SearchIndexClient GetIndexClient()
        {
            if (_indexClient == null)
            {
                var credential = new AzureKeyCredential(_apiKey);
                _indexClient = new SearchIndexClient(new Uri(_endpoint), credential);
                _logger.Information("Created Search Index Client for endpoint: {Endpoint}", _endpoint);
            }
            return _indexClient;
        }

        private SearchClient GetSearchClient(string indexName)
        {
            if (!_searchClients.ContainsKey(indexName))
            {
                var credential = new AzureKeyCredential(_apiKey);
                _searchClients[indexName] = new SearchClient(new Uri(_endpoint), indexName, credential);
                _logger.Information("Created Search Client for index: {IndexName}", indexName);
            }
            return _searchClients[indexName];
        }

        public async Task<bool> CreateIndexAsync(string indexName, List<SearchIndexField> fields)
        {
            try
            {
                _logger.Information("Creating search index: {IndexName} with {FieldCount} fields", indexName, fields.Count);

                var searchFields = new List<SearchField>();
                foreach (var field in fields)
                {
                    var searchField = new SearchField(field.FieldName, GetSearchFieldDataType(field.Type))
                    {
                        IsSearchable = field.Searchable,
                        IsFilterable = field.Filterable,
                        IsSortable = field.Sortable,
                        IsFacetable = field.Facetable,
                        IsKey = field.IsKey
                    };
                    searchFields.Add(searchField);
                }

                var index = new SearchIndex(indexName)
                {
                    Fields = searchFields
                };

                var indexClient = GetIndexClient();
                await indexClient.CreateOrUpdateIndexAsync(index);

                _logger.Information("Successfully created search index: {IndexName}", indexName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create search index: {IndexName}", indexName);
                throw;
            }
        }

        public async Task<bool> CreateIndexWithSemanticConfigurationAsync(string indexName, List<SearchIndexField> fields, string semanticConfigName = "default")
        {
            try
            {
                _logger.Information("Creating search index with semantic configuration: {IndexName}", indexName);

                var searchFields = new List<SearchField>();
                foreach (var field in fields)
                {
                    var searchField = new SearchField(field.FieldName, GetSearchFieldDataType(field.Type))
                    {
                        IsSearchable = field.Searchable,
                        IsFilterable = field.Filterable,
                        IsSortable = field.Sortable,
                        IsFacetable = field.Facetable,
                        IsKey = field.IsKey
                    };
                    searchFields.Add(searchField);
                }

                // Create semantic configuration
                var semanticConfig = new SemanticConfiguration(semanticConfigName, new SemanticPrioritizedFields
                {
                    TitleField = new SemanticField("title"),
                    ContentFields = { new SemanticField("content") }
                });

                var index = new SearchIndex(indexName)
                {
                    Fields = searchFields,
                    SemanticSearch = new SemanticSearch
                    {
                        Configurations = { semanticConfig }
                    }
                };

                var indexClient = GetIndexClient();
                await indexClient.CreateOrUpdateIndexAsync(index);

                _logger.Information("Successfully created search index with semantic configuration: {IndexName}", indexName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create search index with semantic configuration: {IndexName}", indexName);
                throw;
            }
        }

        public async Task<bool> IndexDocumentsAsync(string indexName, List<SearchDocument> documents)
        {
            try
            {
                _logger.Information("Indexing {DocumentCount} documents to index: {IndexName}", documents.Count, indexName);

                var searchClient = GetSearchClient(indexName);
                var batch = new List<Azure.Search.Documents.Models.SearchDocument>();

                foreach (var doc in documents)
                {
                    var searchDoc = new Azure.Search.Documents.Models.SearchDocument(doc.Fields);
                    batch.Add(searchDoc);
                }

                var result = await searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(batch));

                _logger.Information("Successfully indexed {DocumentCount} documents", documents.Count);
                return result.Value.Results.All(r => r.Succeeded);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to index documents to: {IndexName}", indexName);
                throw;
            }
        }

        public async Task<SearchResults> SearchAsync(string indexName, string searchText, SearchQueryOptions? options = null)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.Information("Searching index: {IndexName} for: {SearchText}", indexName, searchText);

                var searchClient = GetSearchClient(indexName);
                var searchOptions = new Azure.Search.Documents.SearchOptions
                {
                    IncludeTotalCount = options?.IncludeTotalCount ?? true,
                    Size = options?.Top ?? 50,
                    Skip = options?.Skip ?? 0
                };

                if (options?.Filter != null)
                    searchOptions.Filter = options.Filter;

                if (options?.Facets != null)
                {
                    foreach (var facet in options.Facets)
                        searchOptions.Facets.Add(facet);
                }

                if (options?.OrderBy != null)
                {
                    foreach (var orderBy in options.OrderBy)
                        searchOptions.OrderBy.Add(orderBy);
                }

                if (options?.Select != null)
                {
                    foreach (var select in options.Select)
                        searchOptions.Select.Add(select);
                }

                var response = await searchClient.SearchAsync<Azure.Search.Documents.Models.SearchDocument>(searchText, searchOptions);
                var results = new SearchResults();

                await foreach (var result in response.Value.GetResultsAsync())
                {
                    var item = new SearchResultItem
                    {
                        Document = result.Document.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        Score = result.Score ?? 0
                    };

                    if (result.Highlights != null)
                    {
                        item.Highlights = result.Highlights.ToDictionary(
                            kvp => kvp.Key,
                            kvp => (object)kvp.Value
                        );
                    }

                    results.Items.Add(item);
                }

                results.TotalCount = response.Value.TotalCount;

                if (response.Value.Facets != null)
                {
                    results.Facets = new Dictionary<string, List<FacetValue>>();
                    foreach (var facet in response.Value.Facets)
                    {
                        var facetValues = facet.Value.Select(f => new FacetValue
                        {
                            Value = f.Value,
                            Count = f.Count ?? 0
                        }).ToList();
                        results.Facets[facet.Key] = facetValues;
                    }
                }

                stopwatch.Stop();
                results.SearchDurationMs = stopwatch.Elapsed.TotalMilliseconds;

                _logger.Information("Search completed in {Duration}ms, found {ResultCount} results",
                    results.SearchDurationMs, results.Items.Count);

                return results;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to search index: {IndexName}", indexName);
                throw;
            }
        }

        public async Task<SearchResults> SemanticSearchAsync(string indexName, string searchText, string semanticConfigName = "default")
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.Information("Performing semantic search on index: {IndexName} for: {SearchText}", indexName, searchText);

                var searchClient = GetSearchClient(indexName);
                var searchOptions = new Azure.Search.Documents.SearchOptions
                {
                    QueryType = Azure.Search.Documents.Models.SearchQueryType.Semantic,
                    SemanticSearch = new SemanticSearchOptions
                    {
                        SemanticConfigurationName = semanticConfigName
                    },
                    IncludeTotalCount = true
                };

                var response = await searchClient.SearchAsync<Azure.Search.Documents.Models.SearchDocument>(searchText, searchOptions);
                var results = new SearchResults();

                await foreach (var result in response.Value.GetResultsAsync())
                {
                    var item = new SearchResultItem
                    {
                        Document = result.Document.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        Score = result.Score ?? 0
                    };

                    // Extract semantic captions if available
                    if (result.SemanticSearch?.Captions != null && result.SemanticSearch.Captions.Any())
                    {
                        item.SemanticCaption = result.SemanticSearch.Captions.First().Text;
                    }

                    results.Items.Add(item);
                }

                results.TotalCount = response.Value.TotalCount;
                stopwatch.Stop();
                results.SearchDurationMs = stopwatch.Elapsed.TotalMilliseconds;

                _logger.Information("Semantic search completed in {Duration}ms, found {ResultCount} results",
                    results.SearchDurationMs, results.Items.Count);

                return results;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to perform semantic search on index: {IndexName}", indexName);
                throw;
            }
        }

        public async Task<List<string>> GetAutocompleteSuggestionsAsync(string indexName, string searchText, string suggesterName = "sg")
        {
            try
            {
                _logger.Information("Getting autocomplete suggestions for: {SearchText}", searchText);

                var searchClient = GetSearchClient(indexName);
                var options = new AutocompleteOptions
                {
                    Mode = AutocompleteMode.OneTerm,
                    Size = 10
                };

                var response = await searchClient.AutocompleteAsync(searchText, suggesterName, options);
                var suggestions = response.Value.Results.Select(r => r.Text).ToList();

                _logger.Information("Found {SuggestionCount} autocomplete suggestions", suggestions.Count);
                return suggestions;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get autocomplete suggestions");
                throw;
            }
        }

        public async Task<bool> DeleteDocumentAsync(string indexName, string keyFieldName, string keyValue)
        {
            try
            {
                _logger.Information("Deleting document with {KeyField}={KeyValue} from index: {IndexName}",
                    keyFieldName, keyValue, indexName);

                var searchClient = GetSearchClient(indexName);
                var doc = new Azure.Search.Documents.Models.SearchDocument
                {
                    [keyFieldName] = keyValue
                };

                var batch = IndexDocumentsBatch.Delete(new[] { doc });
                var result = await searchClient.IndexDocumentsAsync(batch);

                _logger.Information("Successfully deleted document");
                return result.Value.Results.All(r => r.Succeeded);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete document from index: {IndexName}", indexName);
                throw;
            }
        }

        public async Task<bool> DeleteIndexAsync(string indexName)
        {
            try
            {
                _logger.Information("Deleting search index: {IndexName}", indexName);

                var indexClient = GetIndexClient();
                await indexClient.DeleteIndexAsync(indexName);

                _logger.Information("Successfully deleted search index: {IndexName}", indexName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete search index: {IndexName}", indexName);
                throw;
            }
        }

        public async Task<bool> IndexExistsAsync(string indexName)
        {
            try
            {
                var indexClient = GetIndexClient();
                var indexNames = indexClient.GetIndexNamesAsync();
                await foreach (var name in indexNames)
                {
                    if (name == indexName)
                        return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to check if index exists: {IndexName}", indexName);
                throw;
            }
        }

        public async Task<long> GetDocumentCountAsync(string indexName)
        {
            try
            {
                var searchClient = GetSearchClient(indexName);
                var response = await searchClient.SearchAsync<Azure.Search.Documents.Models.SearchDocument>("*", new Azure.Search.Documents.SearchOptions
                {
                    IncludeTotalCount = true,
                    Size = 0
                });

                return response.Value.TotalCount ?? 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get document count for index: {IndexName}", indexName);
                throw;
            }
        }

        private SearchFieldDataType GetSearchFieldDataType(string type)
        {
            return type switch
            {
                "Edm.String" => SearchFieldDataType.String,
                "Edm.Int32" => SearchFieldDataType.Int32,
                "Edm.Int64" => SearchFieldDataType.Int64,
                "Edm.Double" => SearchFieldDataType.Double,
                "Edm.Boolean" => SearchFieldDataType.Boolean,
                "Edm.DateTimeOffset" => SearchFieldDataType.DateTimeOffset,
                "Edm.GeographyPoint" => SearchFieldDataType.GeographyPoint,
                _ => SearchFieldDataType.String
            };
        }
    }
}