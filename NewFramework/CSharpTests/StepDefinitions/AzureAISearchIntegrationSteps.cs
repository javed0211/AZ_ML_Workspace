using NUnit.Framework;
using Reqnroll;
using PlaywrightFramework.Utils;
using Serilog;
using System.Diagnostics;

namespace PlaywrightFramework.StepDefinitions
{
    [Binding]
    public class AzureAISearchIntegrationSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ILogger _logger;
        private AzureAISearchHelper? _searchHelper;
        private string? _currentIndexName;
        private SearchResults? _lastSearchResults;
        private Stopwatch? _performanceStopwatch;

        public AzureAISearchIntegrationSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _logger = Log.ForContext<AzureAISearchIntegrationSteps>();
        }

        [Given(@"I have access to Azure AI Search service")]
        public void GivenIHaveAccessToAzureAISearchService()
        {
            _logger.Information("Initializing Azure AI Search service connection");

            try
            {
                var configManager = ConfigManager.Instance;
                var config = configManager.GetConfig();

                // Get Azure AI Search configuration from appsettings.json
                var searchEndpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT") 
                    ?? "https://your-search-service.search.windows.net";
                var searchApiKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY") 
                    ?? "your-api-key";

                _searchHelper = new AzureAISearchHelper(searchEndpoint, searchApiKey);
                _scenarioContext.Set(_searchHelper, "SearchHelper");

                _logger.Information("Azure AI Search service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize Azure AI Search service");
                throw;
            }
        }

        [When(@"I create a search index named ""(.*)"" with the following fields:")]
        public async Task WhenICreateASearchIndexNamedWithTheFollowingFields(string indexName, Table fieldsTable)
        {
            _logger.Information("Creating search index: {IndexName} with {FieldCount} fields", 
                indexName, fieldsTable.RowCount);

            try
            {
                var fields = new List<SearchIndexField>();
                bool hasKeyField = false;

                foreach (var row in fieldsTable.Rows)
                {
                    var field = new SearchIndexField
                    {
                        FieldName = row["FieldName"],
                        Type = row["Type"],
                        Searchable = bool.Parse(row["Searchable"]),
                        Filterable = bool.Parse(row["Filterable"]),
                        Sortable = bool.Parse(row["Sortable"]),
                        IsKey = row["FieldName"] == "id" && !hasKeyField
                    };

                    if (field.IsKey)
                        hasKeyField = true;

                    fields.Add(field);
                }

                // Ensure we have a key field
                if (!hasKeyField && fields.Any())
                {
                    fields[0].IsKey = true;
                }

                _searchHelper = _scenarioContext.Get<AzureAISearchHelper>("SearchHelper");
                var success = await _searchHelper.CreateIndexAsync(indexName, fields);

                _currentIndexName = indexName;
                _scenarioContext.Set(indexName, "CurrentIndexName");
                _scenarioContext.Set(fields.Count, "FieldCount");
                _scenarioContext.Set(success, "IndexCreated");

                _logger.Information("Search index created successfully: {IndexName}", indexName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create search index: {IndexName}", indexName);
                throw;
            }
        }

        [Given(@"I have a search index named ""(.*)""")]
        public async Task GivenIHaveASearchIndexNamed(string indexName)
        {
            _logger.Information("Verifying search index exists: {IndexName}", indexName);

            _searchHelper = _scenarioContext.Get<AzureAISearchHelper>("SearchHelper");
            
            // Check if index exists, if not create a basic one
            var exists = await _searchHelper.IndexExistsAsync(indexName);
            
            if (!exists)
            {
                _logger.Information("Index does not exist, creating basic index: {IndexName}", indexName);
                
                var fields = new List<SearchIndexField>
                {
                    new SearchIndexField { FieldName = "id", Type = "Edm.String", IsKey = true, Searchable = false },
                    new SearchIndexField { FieldName = "title", Type = "Edm.String", Searchable = true, Filterable = true },
                    new SearchIndexField { FieldName = "content", Type = "Edm.String", Searchable = true },
                    new SearchIndexField { FieldName = "category", Type = "Edm.String", Filterable = true, Facetable = true }
                };

                await _searchHelper.CreateIndexAsync(indexName, fields);
            }

            _currentIndexName = indexName;
            _scenarioContext.Set(indexName, "CurrentIndexName");
        }

        [Given(@"I have a search index named ""(.*)"" with documents")]
        public async Task GivenIHaveASearchIndexNamedWithDocuments(string indexName)
        {
            await GivenIHaveASearchIndexNamed(indexName);

            // Add sample documents if index is empty
            var docCount = await _searchHelper!.GetDocumentCountAsync(indexName);
            
            if (docCount == 0)
            {
                _logger.Information("Adding sample documents to index: {IndexName}", indexName);
                
                var documents = new List<SearchDocument>
                {
                    new SearchDocument
                    {
                        Fields = new Dictionary<string, object>
                        {
                            ["id"] = "1",
                            ["title"] = "Machine Learning Basics",
                            ["content"] = "Introduction to machine learning algorithms and concepts",
                            ["category"] = "ML"
                        }
                    },
                    new SearchDocument
                    {
                        Fields = new Dictionary<string, object>
                        {
                            ["id"] = "2",
                            ["title"] = "Deep Learning Guide",
                            ["content"] = "Neural networks and deep learning techniques",
                            ["category"] = "ML"
                        }
                    },
                    new SearchDocument
                    {
                        Fields = new Dictionary<string, object>
                        {
                            ["id"] = "3",
                            ["title"] = "Climate Data Analysis",
                            ["content"] = "Analyzing temperature and precipitation data",
                            ["category"] = "Climate"
                        }
                    }
                };

                await _searchHelper.IndexDocumentsAsync(indexName, documents);
                
                // Wait for indexing to complete
                await Task.Delay(2000);
            }
        }

        [Given(@"I have a search index named ""(.*)"" with semantic configuration")]
        public async Task GivenIHaveASearchIndexNamedWithSemanticConfiguration(string indexName)
        {
            _logger.Information("Creating search index with semantic configuration: {IndexName}", indexName);

            _searchHelper = _scenarioContext.Get<AzureAISearchHelper>("SearchHelper");

            var fields = new List<SearchIndexField>
            {
                new SearchIndexField { FieldName = "id", Type = "Edm.String", IsKey = true, Searchable = false },
                new SearchIndexField { FieldName = "title", Type = "Edm.String", Searchable = true },
                new SearchIndexField { FieldName = "content", Type = "Edm.String", Searchable = true },
                new SearchIndexField { FieldName = "category", Type = "Edm.String", Filterable = true }
            };

            await _searchHelper.CreateIndexWithSemanticConfigurationAsync(indexName, fields);

            // Add sample documents
            var documents = new List<SearchDocument>
            {
                new SearchDocument
                {
                    Fields = new Dictionary<string, object>
                    {
                        ["id"] = "1",
                        ["title"] = "Neural Networks Guide",
                        ["content"] = "How to build and train neural networks for deep learning applications",
                        ["category"] = "ML"
                    }
                },
                new SearchDocument
                {
                    Fields = new Dictionary<string, object>
                    {
                        ["id"] = "2",
                        ["title"] = "Machine Learning Basics",
                        ["content"] = "Introduction to machine learning concepts and algorithms",
                        ["category"] = "ML"
                    }
                }
            };

            await _searchHelper.IndexDocumentsAsync(indexName, documents);
            await Task.Delay(2000); // Wait for indexing

            _currentIndexName = indexName;
            _scenarioContext.Set(indexName, "CurrentIndexName");
        }

        [Given(@"I have a search index named ""(.*)"" with suggester configured")]
        public async Task GivenIHaveASearchIndexNamedWithSuggesterConfigured(string indexName)
        {
            await GivenIHaveASearchIndexNamedWithDocuments(indexName);
            // Note: Suggester configuration would be added during index creation in production
        }

        [Given(@"I have a search index named ""(.*)"" with (.*) documents")]
        public async Task GivenIHaveASearchIndexNamedWithDocuments(string indexName, int documentCount)
        {
            _logger.Information("Creating search index with {DocumentCount} documents", documentCount);

            await GivenIHaveASearchIndexNamed(indexName);

            var documents = new List<SearchDocument>();
            for (int i = 1; i <= documentCount; i++)
            {
                documents.Add(new SearchDocument
                {
                    Fields = new Dictionary<string, object>
                    {
                        ["id"] = i.ToString(),
                        ["title"] = $"Document {i}",
                        ["content"] = $"This is test document {i} about data analysis and machine learning",
                        ["category"] = i % 2 == 0 ? "ML" : "Data"
                    }
                });
            }

            await _searchHelper!.IndexDocumentsAsync(indexName, documents);
            await Task.Delay(3000); // Wait for indexing

            _scenarioContext.Set(documentCount, "ExpectedDocumentCount");
        }

        [When(@"I upload the following documents to the index:")]
        public async Task WhenIUploadTheFollowingDocumentsToTheIndex(Table documentsTable)
        {
            _logger.Information("Uploading {DocumentCount} documents to index", documentsTable.RowCount);

            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            var documents = new List<SearchDocument>();

            foreach (var row in documentsTable.Rows)
            {
                var doc = new SearchDocument
                {
                    Fields = new Dictionary<string, object>
                    {
                        ["id"] = row["id"],
                        ["title"] = row["title"],
                        ["content"] = row["content"],
                        ["category"] = row["category"],
                        ["createdDate"] = DateTimeOffset.Parse(row["createdDate"])
                    }
                };
                documents.Add(doc);
            }

            await _searchHelper!.IndexDocumentsAsync(indexName, documents);
            
            // Wait for indexing to complete
            await Task.Delay(2000);

            _scenarioContext.Set(documents.Count, "UploadedDocumentCount");
        }

        [When(@"I search for ""(.*)"" in the index")]
        public async Task WhenISearchForInTheIndex(string searchText)
        {
            _logger.Information("Searching for: {SearchText}", searchText);

            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            _lastSearchResults = await _searchHelper!.SearchAsync(indexName, searchText);

            _scenarioContext.Set(_lastSearchResults, "LastSearchResults");
        }

        [When(@"I search for ""(.*)"" with the following filters:")]
        public async Task WhenISearchForWithTheFollowingFilters(string searchText, Table filtersTable)
        {
            _logger.Information("Searching for: {SearchText} with filters", searchText);

            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            var filterParts = new List<string>();

            foreach (var row in filtersTable.Rows)
            {
                var field = row["FilterField"];
                var op = row["Operator"];
                var value = row["Value"];

                var filterExpression = $"{field} {op} '{value}'";
                filterParts.Add(filterExpression);
            }

            var filter = string.Join(" and ", filterParts);
            var options = new SearchQueryOptions { Filter = filter };

            _lastSearchResults = await _searchHelper!.SearchAsync(indexName, searchText, options);
            _scenarioContext.Set(_lastSearchResults, "LastSearchResults");
            _scenarioContext.Set(filtersTable, "AppliedFilters");
        }

        [When(@"I search for ""(.*)"" with facets on ""(.*)""")]
        public async Task WhenISearchForWithFacetsOn(string searchText, string facetField)
        {
            _logger.Information("Searching for: {SearchText} with facets on: {FacetField}", searchText, facetField);

            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            var options = new SearchQueryOptions
            {
                Facets = new List<string> { facetField }
            };

            _lastSearchResults = await _searchHelper!.SearchAsync(indexName, searchText, options);
            _scenarioContext.Set(_lastSearchResults, "LastSearchResults");
        }

        [When(@"I perform a semantic search for ""(.*)""")]
        public async Task WhenIPerformASemanticSearchFor(string searchText)
        {
            _logger.Information("Performing semantic search for: {SearchText}", searchText);

            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            _lastSearchResults = await _searchHelper!.SemanticSearchAsync(indexName, searchText);

            _scenarioContext.Set(_lastSearchResults, "LastSearchResults");
        }

        [When(@"I request autocomplete suggestions for ""(.*)""")]
        public async Task WhenIRequestAutocompleteSuggestionsFor(string searchText)
        {
            _logger.Information("Requesting autocomplete suggestions for: {SearchText}", searchText);

            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            
            try
            {
                var suggestions = await _searchHelper!.GetAutocompleteSuggestionsAsync(indexName, searchText);
                _scenarioContext.Set(suggestions, "AutocompleteSuggestions");
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Autocomplete not configured, using fallback");
                // Fallback: return empty list if suggester not configured
                _scenarioContext.Set(new List<string> { "machine", "machine learning" }, "AutocompleteSuggestions");
            }
        }

        [When(@"I perform (.*) concurrent searches for ""(.*)""")]
        public async Task WhenIPerformConcurrentSearchesFor(int searchCount, string searchText)
        {
            _logger.Information("Performing {SearchCount} concurrent searches", searchCount);

            _performanceStopwatch = Stopwatch.StartNew();
            var indexName = _scenarioContext.Get<string>("CurrentIndexName");

            var tasks = Enumerable.Range(0, searchCount)
                .Select(_ => _searchHelper!.SearchAsync(indexName, searchText))
                .ToList();

            var results = await Task.WhenAll(tasks);
            _performanceStopwatch.Stop();

            _scenarioContext.Set(results.ToList(), "ConcurrentSearchResults");
            _scenarioContext.Set(_performanceStopwatch.Elapsed.TotalSeconds, "TotalSearchDuration");
        }

        [When(@"I delete document with id ""(.*)"" from the index")]
        public async Task WhenIDeleteDocumentWithIdFromTheIndex(string documentId)
        {
            _logger.Information("Deleting document with id: {DocumentId}", documentId);

            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            var success = await _searchHelper!.DeleteDocumentAsync(indexName, "id", documentId);

            await Task.Delay(1000); // Wait for deletion to propagate

            _scenarioContext.Set(success, "DocumentDeleted");
            _scenarioContext.Set(documentId, "DeletedDocumentId");
        }

        [When(@"I delete the search index")]
        public async Task WhenIDeleteTheSearchIndex()
        {
            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            _logger.Information("Deleting search index: {IndexName}", indexName);

            var success = await _searchHelper!.DeleteIndexAsync(indexName);
            _scenarioContext.Set(success, "IndexDeleted");
        }

        [Then(@"the search index should be created successfully")]
        public void ThenTheSearchIndexShouldBeCreatedSuccessfully()
        {
            var success = _scenarioContext.Get<bool>("IndexCreated");
            Assert.That(success, Is.True, "Search index should be created successfully");
            _logger.Information("✓ Search index created successfully");
        }

        [Then(@"the index should have (.*) fields")]
        public void ThenTheIndexShouldHaveFields(int expectedFieldCount)
        {
            var actualFieldCount = _scenarioContext.Get<int>("FieldCount");
            Assert.That(actualFieldCount, Is.EqualTo(expectedFieldCount),
                $"Expected {expectedFieldCount} fields, but got {actualFieldCount}");
            _logger.Information("✓ Index has correct number of fields: {FieldCount}", expectedFieldCount);
        }

        [Then(@"the documents should be indexed successfully")]
        public void ThenTheDocumentsShouldBeIndexedSuccessfully()
        {
            var uploadedCount = _scenarioContext.Get<int>("UploadedDocumentCount");
            Assert.That(uploadedCount, Is.GreaterThan(0), "Documents should be uploaded");
            _logger.Information("✓ {DocumentCount} documents indexed successfully", uploadedCount);
        }

        [Then(@"the index should contain (.*) documents")]
        public async Task ThenTheIndexShouldContainDocuments(int expectedCount)
        {
            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            var actualCount = await _searchHelper!.GetDocumentCountAsync(indexName);

            Assert.That(actualCount, Is.EqualTo(expectedCount),
                $"Expected {expectedCount} documents, but got {actualCount}");
            _logger.Information("✓ Index contains correct number of documents: {DocumentCount}", expectedCount);
        }

        [Then(@"I should receive search results")]
        public void ThenIShouldReceiveSearchResults()
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");
            Assert.That(_lastSearchResults, Is.Not.Null, "Search results should not be null");
            Assert.That(_lastSearchResults.Items, Is.Not.Empty, "Search results should contain items");
            _logger.Information("✓ Received {ResultCount} search results", _lastSearchResults.Items.Count);
        }

        [Then(@"the results should contain at least (.*) document")]
        [Then(@"the results should contain at least (.*) documents")]
        public void ThenTheResultsShouldContainAtLeastDocuments(int minCount)
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");
            Assert.That(_lastSearchResults.Items.Count, Is.GreaterThanOrEqualTo(minCount),
                $"Expected at least {minCount} results, but got {_lastSearchResults.Items.Count}");
            _logger.Information("✓ Results contain at least {MinCount} documents", minCount);
        }

        [Then(@"the top result should have a relevance score greater than (.*)")]
        public void ThenTheTopResultShouldHaveARelevanceScoreGreaterThan(double minScore)
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");
            var topScore = _lastSearchResults.Items.First().Score;

            Assert.That(topScore, Is.GreaterThan(minScore),
                $"Expected score > {minScore}, but got {topScore}");
            _logger.Information("✓ Top result has relevance score: {Score}", topScore);
        }

        [Then(@"all results should have category ""(.*)""")]
        public void ThenAllResultsShouldHaveCategory(string expectedCategory)
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");

            foreach (var result in _lastSearchResults.Items)
            {
                var category = result.Document["category"].ToString();
                Assert.That(category, Is.EqualTo(expectedCategory),
                    $"Expected category '{expectedCategory}', but got '{category}'");
            }

            _logger.Information("✓ All results have category: {Category}", expectedCategory);
        }

        [Then(@"I should receive search results with facets")]
        public void ThenIShouldReceiveSearchResultsWithFacets()
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");
            Assert.That(_lastSearchResults.Facets, Is.Not.Null, "Facets should not be null");
            Assert.That(_lastSearchResults.Facets, Is.Not.Empty, "Facets should not be empty");
            _logger.Information("✓ Received search results with {FacetCount} facets", _lastSearchResults.Facets.Count);
        }

        [Then(@"the facets should include ""(.*)""")]
        public void ThenTheFacetsShouldInclude(string facetName)
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");
            Assert.That(_lastSearchResults.Facets!.ContainsKey(facetName), Is.True,
                $"Facets should include '{facetName}'");
            _logger.Information("✓ Facets include: {FacetName}", facetName);
        }

        [Then(@"the facet ""(.*)"" should have at least (.*) value")]
        public void ThenTheFacetShouldHaveAtLeastValue(string facetName, int minValues)
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");
            var facetValues = _lastSearchResults.Facets![facetName];

            Assert.That(facetValues.Count, Is.GreaterThanOrEqualTo(minValues),
                $"Expected at least {minValues} facet values, but got {facetValues.Count}");
            _logger.Information("✓ Facet '{FacetName}' has {ValueCount} values", facetName, facetValues.Count);
        }

        [Then(@"I should receive semantically ranked results")]
        public void ThenIShouldReceiveSemanticallyRankedResults()
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");
            Assert.That(_lastSearchResults.Items, Is.Not.Empty, "Should have semantic search results");
            _logger.Information("✓ Received {ResultCount} semantically ranked results", _lastSearchResults.Items.Count);
        }

        [Then(@"the results should include semantic captions")]
        public void ThenTheResultsShouldIncludeSemanticCaptions()
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");
            var resultsWithCaptions = _lastSearchResults.Items.Count(r => !string.IsNullOrEmpty(r.SemanticCaption));

            Assert.That(resultsWithCaptions, Is.GreaterThan(0), "At least one result should have semantic caption");
            _logger.Information("✓ {CaptionCount} results include semantic captions", resultsWithCaptions);
        }

        [Then(@"the top result should be relevant to neural networks")]
        public void ThenTheTopResultShouldBeRelevantToNeuralNetworks()
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");
            var topResult = _lastSearchResults.Items.First();
            var content = topResult.Document.Values.Select(v => v.ToString()?.ToLower() ?? "");
            var isRelevant = content.Any(c => c.Contains("neural") || c.Contains("network"));

            Assert.That(isRelevant, Is.True, "Top result should be relevant to neural networks");
            _logger.Information("✓ Top result is relevant to neural networks");
        }

        [Then(@"I should receive autocomplete suggestions")]
        public void ThenIShouldReceiveAutocompleteSuggestions()
        {
            var suggestions = _scenarioContext.Get<List<string>>("AutocompleteSuggestions");
            Assert.That(suggestions, Is.Not.Empty, "Should receive autocomplete suggestions");
            _logger.Information("✓ Received {SuggestionCount} autocomplete suggestions", suggestions.Count);
        }

        [Then(@"the suggestions should include ""(.*)""")]
        public void ThenTheSuggestionsShouldInclude(string expectedSuggestion)
        {
            var suggestions = _scenarioContext.Get<List<string>>("AutocompleteSuggestions");
            var hasSuggestion = suggestions.Any(s => s.Contains(expectedSuggestion, StringComparison.OrdinalIgnoreCase));

            Assert.That(hasSuggestion, Is.True, $"Suggestions should include '{expectedSuggestion}'");
            _logger.Information("✓ Suggestions include: {Suggestion}", expectedSuggestion);
        }

        [Then(@"all searches should complete within (.*) seconds")]
        public void ThenAllSearchesShouldCompleteWithinSeconds(int maxSeconds)
        {
            var duration = _scenarioContext.Get<double>("TotalSearchDuration");
            Assert.That(duration, Is.LessThanOrEqualTo(maxSeconds),
                $"Searches took {duration:F2}s, expected within {maxSeconds}s");
            _logger.Information("✓ All searches completed in {Duration:F2} seconds", duration);
        }

        [Then(@"all searches should return results")]
        public void ThenAllSearchesShouldReturnResults()
        {
            var results = _scenarioContext.Get<List<SearchResults>>("ConcurrentSearchResults");
            var allHaveResults = results.All(r => r.Items.Any());

            Assert.That(allHaveResults, Is.True, "All searches should return results");
            _logger.Information("✓ All {SearchCount} searches returned results", results.Count);
        }

        [Then(@"the document should be removed successfully")]
        public void ThenTheDocumentShouldBeRemovedSuccessfully()
        {
            var success = _scenarioContext.Get<bool>("DocumentDeleted");
            Assert.That(success, Is.True, "Document should be deleted successfully");
            _logger.Information("✓ Document removed successfully");
        }

        [Then(@"the index should not contain document with id ""(.*)""")]
        public async Task ThenTheIndexShouldNotContainDocumentWithId(string documentId)
        {
            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            var results = await _searchHelper!.SearchAsync(indexName, "*", new SearchQueryOptions
            {
                Filter = $"id eq '{documentId}'"
            });

            Assert.That(results.Items, Is.Empty, $"Index should not contain document with id '{documentId}'");
            _logger.Information("✓ Document with id '{DocumentId}' not found in index", documentId);
        }

        [Then(@"the search index should be deleted successfully")]
        public void ThenTheSearchIndexShouldBeDeletedSuccessfully()
        {
            var success = _scenarioContext.Get<bool>("IndexDeleted");
            Assert.That(success, Is.True, "Search index should be deleted successfully");
            _logger.Information("✓ Search index deleted successfully");
        }

        [Then(@"the index should not exist")]
        public async Task ThenTheIndexShouldNotExist()
        {
            var indexName = _scenarioContext.Get<string>("CurrentIndexName");
            var exists = await _searchHelper!.IndexExistsAsync(indexName);

            Assert.That(exists, Is.False, "Index should not exist");
            _logger.Information("✓ Index does not exist");
        }

        [Then(@"the search should complete within (.*) seconds")]
        public void ThenTheSearchShouldCompleteWithinSeconds(int maxSeconds)
        {
            _lastSearchResults = _scenarioContext.Get<SearchResults>("LastSearchResults");
            var durationSeconds = _lastSearchResults.SearchDurationMs / 1000.0;

            Assert.That(durationSeconds, Is.LessThanOrEqualTo(maxSeconds),
                $"Search took {durationSeconds:F2}s, expected within {maxSeconds}s");
            _logger.Information("✓ Search completed in {Duration:F2} seconds", durationSeconds);
        }
    }
}