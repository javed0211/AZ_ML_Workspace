using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Serilog;
using System.Diagnostics;

namespace PlaywrightFramework.Utils
{
    public class DocumentField
    {
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }
        public float Confidence { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class DocumentTable
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<DocumentTableCell> Cells { get; set; } = new();
    }

    public class DocumentTableCell
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public string Content { get; set; } = string.Empty;
        public int RowSpan { get; set; } = 1;
        public int ColumnSpan { get; set; } = 1;
    }

    public class DocumentAnalysisResult
    {
        public string ModelId { get; set; } = string.Empty;
        public Dictionary<string, DocumentField> Fields { get; set; } = new();
        public List<string> Pages { get; set; } = new();
        public List<DocumentTable> Tables { get; set; } = new();
        public string Content { get; set; } = string.Empty;
        public double AnalysisDurationMs { get; set; }
        public float AverageConfidence { get; set; }
    }

    public class CustomModelTrainingResult
    {
        public string ModelId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public float Accuracy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Dictionary<string, float> FieldAccuracies { get; set; } = new();
    }

    public class BatchProcessingResult
    {
        public List<DocumentAnalysisResult> Results { get; set; } = new();
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public double TotalDurationMs { get; set; }
        public double AverageDurationMs { get; set; }
    }

    public class AzureDocumentIntelligenceHelper
    {
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly ILogger _logger;
        private DocumentAnalysisClient? _client;
        private DocumentModelAdministrationClient? _adminClient;

        public AzureDocumentIntelligenceHelper(string endpoint, string apiKey)
        {
            _endpoint = endpoint;
            _apiKey = apiKey;
            _logger = Log.ForContext<AzureDocumentIntelligenceHelper>();
        }

        private DocumentAnalysisClient GetClient()
        {
            if (_client == null)
            {
                var credential = new AzureKeyCredential(_apiKey);
                _client = new DocumentAnalysisClient(new Uri(_endpoint), credential);
                _logger.Information("Created Document Analysis Client for endpoint: {Endpoint}", _endpoint);
            }
            return _client;
        }

        private DocumentModelAdministrationClient GetAdminClient()
        {
            if (_adminClient == null)
            {
                var credential = new AzureKeyCredential(_apiKey);
                _adminClient = new DocumentModelAdministrationClient(new Uri(_endpoint), credential);
                _logger.Information("Created Document Model Administration Client for endpoint: {Endpoint}", _endpoint);
            }
            return _adminClient;
        }

        public async Task<DocumentAnalysisResult> AnalyzeDocumentAsync(string documentPath, string modelId = "prebuilt-document")
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.Information("Analyzing document: {DocumentPath} with model: {ModelId}", documentPath, modelId);

                var client = GetClient();
                
                using var stream = File.OpenRead(documentPath);
                var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, modelId, stream);
                var result = operation.Value;

                var analysisResult = new DocumentAnalysisResult
                {
                    ModelId = modelId,
                    Content = result.Content
                };

                // Extract fields
                if (result.Documents.Count > 0)
                {
                    var document = result.Documents[0];
                    foreach (var field in document.Fields)
                    {
                        analysisResult.Fields[field.Key] = new DocumentField
                        {
                            Name = field.Key,
                            Value = GetFieldValue(field.Value),
                            Confidence = field.Value.Confidence ?? 0f,
                            Type = field.Value.FieldType.ToString()
                        };
                    }

                    // Calculate average confidence
                    if (analysisResult.Fields.Any())
                    {
                        analysisResult.AverageConfidence = analysisResult.Fields.Values.Average(f => f.Confidence);
                    }
                }

                // Extract pages
                foreach (var page in result.Pages)
                {
                    analysisResult.Pages.Add($"Page {page.PageNumber}: {page.Lines.Count} lines");
                }

                // Extract tables
                foreach (var table in result.Tables)
                {
                    var docTable = new DocumentTable
                    {
                        RowCount = table.RowCount,
                        ColumnCount = table.ColumnCount
                    };

                    foreach (var cell in table.Cells)
                    {
                        docTable.Cells.Add(new DocumentTableCell
                        {
                            RowIndex = cell.RowIndex,
                            ColumnIndex = cell.ColumnIndex,
                            Content = cell.Content,
                            RowSpan = cell.RowSpan > 0 ? cell.RowSpan : 1,
                            ColumnSpan = cell.ColumnSpan > 0 ? cell.ColumnSpan : 1
                        });
                    }

                    analysisResult.Tables.Add(docTable);
                }

                stopwatch.Stop();
                analysisResult.AnalysisDurationMs = stopwatch.Elapsed.TotalMilliseconds;

                _logger.Information("Document analysis completed in {Duration}ms with {FieldCount} fields extracted",
                    analysisResult.AnalysisDurationMs, analysisResult.Fields.Count);

                return analysisResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to analyze document: {DocumentPath}", documentPath);
                throw;
            }
        }

        public async Task<DocumentAnalysisResult> AnalyzeInvoiceAsync(string documentPath)
        {
            _logger.Information("Analyzing invoice document: {DocumentPath}", documentPath);
            return await AnalyzeDocumentAsync(documentPath, "prebuilt-invoice");
        }

        public async Task<DocumentAnalysisResult> AnalyzeReceiptAsync(string documentPath)
        {
            _logger.Information("Analyzing receipt document: {DocumentPath}", documentPath);
            return await AnalyzeDocumentAsync(documentPath, "prebuilt-receipt");
        }

        public async Task<DocumentAnalysisResult> AnalyzeIdDocumentAsync(string documentPath)
        {
            _logger.Information("Analyzing ID document: {DocumentPath}", documentPath);
            return await AnalyzeDocumentAsync(documentPath, "prebuilt-idDocument");
        }

        public async Task<DocumentAnalysisResult> AnalyzeBusinessCardAsync(string documentPath)
        {
            _logger.Information("Analyzing business card: {DocumentPath}", documentPath);
            return await AnalyzeDocumentAsync(documentPath, "prebuilt-businessCard");
        }

        public async Task<DocumentAnalysisResult> AnalyzeLayoutAsync(string documentPath)
        {
            _logger.Information("Analyzing document layout: {DocumentPath}", documentPath);
            return await AnalyzeDocumentAsync(documentPath, "prebuilt-layout");
        }

        public async Task<DocumentAnalysisResult> AnalyzeWithCustomModelAsync(string documentPath, string customModelId)
        {
            _logger.Information("Analyzing document with custom model: {ModelId}", customModelId);
            return await AnalyzeDocumentAsync(documentPath, customModelId);
        }

        public async Task<BatchProcessingResult> BatchProcessDocumentsAsync(List<string> documentPaths, string modelId = "prebuilt-document")
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.Information("Batch processing {DocumentCount} documents with model: {ModelId}",
                    documentPaths.Count, modelId);

                var result = new BatchProcessingResult();
                var tasks = documentPaths.Select(path => AnalyzeDocumentAsync(path, modelId));
                
                var results = await Task.WhenAll(tasks);
                result.Results = results.ToList();
                result.SuccessCount = results.Length;
                result.FailureCount = 0;

                stopwatch.Stop();
                result.TotalDurationMs = stopwatch.Elapsed.TotalMilliseconds;
                result.AverageDurationMs = result.TotalDurationMs / documentPaths.Count;

                _logger.Information("Batch processing completed in {Duration}ms, average {AvgDuration}ms per document",
                    result.TotalDurationMs, result.AverageDurationMs);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to batch process documents");
                throw;
            }
        }

        public async Task<BatchProcessingResult> BatchProcessDocumentsConcurrentlyAsync(
            Dictionary<string, string> documentPathsWithModels, int maxConcurrency = 5)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                _logger.Information("Batch processing {DocumentCount} documents concurrently (max {MaxConcurrency})",
                    documentPathsWithModels.Count, maxConcurrency);

                var result = new BatchProcessingResult();
                var semaphore = new SemaphoreSlim(maxConcurrency);
                var tasks = new List<Task<DocumentAnalysisResult>>();

                foreach (var kvp in documentPathsWithModels)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            return await AnalyzeDocumentAsync(kvp.Key, kvp.Value);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }));
                }

                var results = await Task.WhenAll(tasks);
                result.Results = results.ToList();
                result.SuccessCount = results.Length;
                result.FailureCount = 0;

                stopwatch.Stop();
                result.TotalDurationMs = stopwatch.Elapsed.TotalMilliseconds;
                result.AverageDurationMs = result.TotalDurationMs / documentPathsWithModels.Count;

                _logger.Information("Concurrent batch processing completed in {Duration}ms, average {AvgDuration}ms per document",
                    result.TotalDurationMs, result.AverageDurationMs);

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to batch process documents concurrently");
                throw;
            }
        }

        public async Task<CustomModelTrainingResult> TrainCustomModelAsync(
            string trainingDataUrl, string modelName, string description = "")
        {
            try
            {
                _logger.Information("Training custom model: {ModelName} with data from: {TrainingDataUrl}",
                    modelName, trainingDataUrl);

                var trainingClient = GetAdminClient();
                
                var trainingOperation = await trainingClient.BuildDocumentModelAsync(
                    WaitUntil.Completed,
                    new Uri(trainingDataUrl),
                    DocumentBuildMode.Template,
                    modelName);

                var model = trainingOperation.Value;

                var result = new CustomModelTrainingResult
                {
                    ModelId = model.ModelId,
                    Status = "Ready",
                    CreatedOn = model.CreatedOn.DateTime,
                    Accuracy = 0.85f // Placeholder - actual accuracy would come from training metrics
                };

                _logger.Information("Custom model trained successfully: {ModelId}", result.ModelId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to train custom model: {ModelName}", modelName);
                throw;
            }
        }

        public async Task<string> ComposeModelsAsync(List<string> modelIds, string composedModelName, string description = "")
        {
            try
            {
                _logger.Information("Composing {ModelCount} models into: {ComposedModelName}",
                    modelIds.Count, composedModelName);

                var adminClient = GetAdminClient();
                
                var operation = await adminClient.ComposeDocumentModelAsync(
                    WaitUntil.Completed,
                    modelIds,
                    composedModelName);

                var composedModel = operation.Value;

                _logger.Information("Models composed successfully: {ModelId}", composedModel.ModelId);
                return composedModel.ModelId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to compose models");
                throw;
            }
        }

        public async Task<bool> DeleteModelAsync(string modelId)
        {
            try
            {
                _logger.Information("Deleting model: {ModelId}", modelId);

                var adminClient = GetAdminClient();

                await adminClient.DeleteDocumentModelAsync(modelId);

                _logger.Information("Model deleted successfully: {ModelId}", modelId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to delete model: {ModelId}", modelId);
                throw;
            }
        }

        public async Task<List<string>> ListModelsAsync()
        {
            try
            {
                _logger.Information("Listing all custom models");

                var adminClient = GetAdminClient();

                var models = new List<string>();
                await foreach (var model in adminClient.GetDocumentModelsAsync())
                {
                    models.Add(model.ModelId);
                }

                _logger.Information("Found {ModelCount} custom models", models.Count);
                return models;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to list models");
                throw;
            }
        }

        public bool ValidateExtractedData(DocumentAnalysisResult result, Dictionary<string, object> expectedValues)
        {
            _logger.Information("Validating extracted data against expected values");

            foreach (var expected in expectedValues)
            {
                if (!result.Fields.ContainsKey(expected.Key))
                {
                    _logger.Warning("Expected field not found: {FieldName}", expected.Key);
                    return false;
                }

                var actualValue = result.Fields[expected.Key].Value?.ToString();
                var expectedValue = expected.Value?.ToString();

                if (actualValue != expectedValue)
                {
                    _logger.Warning("Field value mismatch for {FieldName}: expected {Expected}, got {Actual}",
                        expected.Key, expectedValue, actualValue);
                    return false;
                }
            }

            _logger.Information("All extracted data validated successfully");
            return true;
        }

        public Dictionary<string, object> ConvertToSearchDocument(DocumentAnalysisResult result, string documentId)
        {
            _logger.Information("Converting document analysis result to search document");

            var searchDoc = new Dictionary<string, object>
            {
                ["id"] = documentId,
                ["content"] = result.Content,
                ["modelId"] = result.ModelId,
                ["averageConfidence"] = result.AverageConfidence,
                ["pageCount"] = result.Pages.Count,
                ["tableCount"] = result.Tables.Count
            };

            // Add extracted fields
            foreach (var field in result.Fields)
            {
                if (field.Value.Value != null)
                {
                    searchDoc[field.Key] = field.Value.Value;
                }
            }

            _logger.Information("Converted to search document with {FieldCount} fields", searchDoc.Count);
            return searchDoc;
        }

        private object? GetFieldValue(Azure.AI.FormRecognizer.DocumentAnalysis.DocumentField field)
        {
            return field.FieldType switch
            {
                Azure.AI.FormRecognizer.DocumentAnalysis.DocumentFieldType.String => field.Value.AsString(),
                Azure.AI.FormRecognizer.DocumentAnalysis.DocumentFieldType.Date => field.Value.AsDate(),
                Azure.AI.FormRecognizer.DocumentAnalysis.DocumentFieldType.Time => field.Value.AsTime(),
                Azure.AI.FormRecognizer.DocumentAnalysis.DocumentFieldType.PhoneNumber => field.Value.AsPhoneNumber(),
                Azure.AI.FormRecognizer.DocumentAnalysis.DocumentFieldType.Double => field.Value.AsDouble(),
                Azure.AI.FormRecognizer.DocumentAnalysis.DocumentFieldType.Int64 => field.Value.AsInt64(),
                Azure.AI.FormRecognizer.DocumentAnalysis.DocumentFieldType.Address => field.Value.AsAddress()?.ToString(),
                Azure.AI.FormRecognizer.DocumentAnalysis.DocumentFieldType.Currency => field.Value.AsCurrency().Amount,
                _ => field.Content
            };
        }
    }
}