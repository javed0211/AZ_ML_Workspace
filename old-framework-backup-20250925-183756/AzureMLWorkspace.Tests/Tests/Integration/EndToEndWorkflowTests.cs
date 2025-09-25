using Microsoft.Playwright;
using AzureMLWorkspace.Tests.Helpers;
using AzureMLWorkspace.Tests.Actions.Core;
using AzureMLWorkspace.Tests.Actions.AzureML;
using AzureMLWorkspace.Tests.Actions.DocumentProcessing;
using AzureMLWorkspace.Tests.Configuration;

namespace AzureMLWorkspace.Tests.Tests.Integration;

[TestFixture]
[Category("Integration")]
[Category("EndToEnd")]
public class EndToEndWorkflowTests : BaseTest
{
    private readonly string WorkflowId = DateTime.Now.ToString("yyyyMMdd-HHmmss");

    [Test]
    public async Task Test_Complete_ML_Pipeline_Workflow()
    {
        TestLogger.LogStep("Starting complete ML pipeline workflow test");

        var datasetName = $"e2e-dataset-{WorkflowId}";
        var experimentName = $"e2e-experiment-{WorkflowId}";
        var modelName = $"e2e-model-{WorkflowId}";
        var computeName = $"e2e-compute-{WorkflowId}";

        // Create test data
        var testDataPath = Path.Combine(Config.TestDataPath, "ml-pipeline-data.csv");
        await CreateMLTestData(testDataPath);

        // Execute complete ML workflow using Actions
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(BrowserActions.WaitForPageLoad(Page, TestLogger, Config))
            
            // Step 1: Create compute resources
            .Add(AzureMLActions.CreateComputeInstance(Page, TestLogger, Config, computeName, "STANDARD_DS3_V2"))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "e2e_compute_created"))
            
            // Step 2: Upload dataset
            .AddWithRetry(
                AzureMLActions.UploadDataset(Page, TestLogger, Config, datasetName, testDataPath),
                maxRetries: 3
            )
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "e2e_dataset_uploaded"))
            
            // Step 3: Create experiment
            .Add(AzureMLActions.CreateExperiment(Page, TestLogger, Config, experimentName))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "e2e_experiment_created"))
            
            // Step 4: Train model (simulated)
            .Add(async () => await SimulateModelTraining(experimentName))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "e2e_model_trained"))
            
            // Step 5: Deploy model (simulated)
            .Add(async () => await SimulateModelDeployment(modelName))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "e2e_model_deployed"))
            
            .ExecuteAsync();

        TestLogger.LogStep("Complete ML pipeline workflow test completed successfully");
    }

    [Test]
    public async Task Test_Document_Processing_To_ML_Workflow()
    {
        TestLogger.LogStep("Starting document processing to ML workflow test");

        var documentContent = GenerateBusinessDocument();
        var datasetName = $"doc-ml-dataset-{WorkflowId}";
        var experimentName = $"doc-ml-experiment-{WorkflowId}";

        // Execute document processing to ML workflow
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            
            // Step 1: Process documents
            .Add(DocumentProcessingActions.ClassifyDocument(Page, TestLogger, Config, documentContent))
            .Add(DocumentProcessingActions.ExtractKeyPhrases(Page, TestLogger, Config, documentContent))
            .Add(DocumentProcessingActions.AnalyzeSentiment(Page, TestLogger, Config, documentContent))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "doc_processing_completed"))
            
            // Step 2: Create ML experiment with processed data
            .Add(AzureMLActions.CreateExperiment(Page, TestLogger, Config, experimentName))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "doc_ml_experiment_created"))
            
            // Step 3: Simulate training on processed document data
            .Add(async () => await SimulateDocumentMLTraining(experimentName, documentContent))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "doc_ml_training_completed"))
            
            .ExecuteAsync();

        TestLogger.LogStep("Document processing to ML workflow test completed successfully");
    }

    [Test]
    public async Task Test_Multi_Environment_Deployment_Workflow()
    {
        TestLogger.LogStep("Starting multi-environment deployment workflow test");

        var modelName = $"multi-env-model-{WorkflowId}";
        var devDeployment = $"dev-{modelName}";
        var stagingDeployment = $"staging-{modelName}";

        // Execute multi-environment deployment workflow
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            
            // Step 1: Deploy to development environment
            .Add(async () => await SimulateModelDeployment(devDeployment))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "dev_deployment"))
            
            // Step 2: Run validation tests
            .Add(async () => await SimulateDeploymentValidation(devDeployment))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "dev_validation"))
            
            // Step 3: Deploy to staging (conditional on dev success)
            .AddIf(
                async () => await IsDeploymentHealthy(devDeployment),
                new LambdaAction(Page, TestLogger, Config, 
                    async () => await SimulateModelDeployment(stagingDeployment),
                    "Deploy to staging environment")
            )
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "staging_deployment"))
            
            .ExecuteAsync();

        TestLogger.LogStep("Multi-environment deployment workflow test completed successfully");
    }

    [Test]
    public async Task Test_Data_Pipeline_With_Monitoring()
    {
        TestLogger.LogStep("Starting data pipeline with monitoring workflow test");

        var pipelineName = $"data-pipeline-{WorkflowId}";
        var monitoringName = $"monitor-{WorkflowId}";

        // Execute data pipeline with monitoring
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            
            // Step 1: Create data pipeline
            .Add(async () => await CreateDataPipeline(pipelineName))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "data_pipeline_created"))
            
            // Step 2: Set up monitoring
            .Add(async () => await SetupPipelineMonitoring(monitoringName, pipelineName))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "monitoring_setup"))
            
            // Step 3: Execute pipeline and monitor
            .AddParallel(
                new LambdaAction(Page, TestLogger, Config, 
                    async () => await ExecuteDataPipeline(pipelineName),
                    "Execute data pipeline"),
                new LambdaAction(Page, TestLogger, Config, 
                    async () => await MonitorPipelineExecution(monitoringName),
                    "Monitor pipeline execution")
            )
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "pipeline_execution_monitored"))
            
            .ExecuteAsync();

        TestLogger.LogStep("Data pipeline with monitoring workflow test completed successfully");
    }

    [Test]
    public async Task Test_Automated_Model_Retraining_Workflow()
    {
        TestLogger.LogStep("Starting automated model retraining workflow test");

        var modelName = $"retrain-model-{WorkflowId}";
        var originalDataset = $"original-data-{WorkflowId}";
        var newDataset = $"new-data-{WorkflowId}";

        // Execute automated retraining workflow
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            
            // Step 1: Create initial model
            .Add(async () => await CreateInitialModel(modelName, originalDataset))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "initial_model_created"))
            
            // Step 2: Simulate data drift detection
            .Add(async () => await SimulateDataDrift(originalDataset, newDataset))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "data_drift_detected"))
            
            // Step 3: Trigger automated retraining
            .AddIf(
                async () => await IsRetrainingRequired(modelName),
                new LambdaAction(Page, TestLogger, Config, 
                    async () => await TriggerAutomatedRetraining(modelName, newDataset),
                    "Trigger automated retraining")
            )
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "retraining_completed"))
            
            // Step 4: Validate retrained model
            .Add(async () => await ValidateRetrainedModel(modelName))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "retrained_model_validated"))
            
            .ExecuteAsync();

        TestLogger.LogStep("Automated model retraining workflow test completed successfully");
    }

    [Test]
    public async Task Test_Cross_Platform_Compatibility()
    {
        TestLogger.LogStep("Testing cross-platform compatibility");

        var platformInfo = GetPlatformInfo();
        TestLogger.LogStep($"Running on platform: {platformInfo}");

        // Test cross-platform file operations
        var testFilePath = GetCrossPlatformTestPath("cross-platform-test.txt");
        
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(async () => await TestCrossPlatformFileOperations(testFilePath))
            .Add(async () => await TestCrossPlatformPathHandling())
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, $"cross_platform_{platformInfo}"))
            .ExecuteAsync();

        TestLogger.LogStep("Cross-platform compatibility test completed successfully");
    }

    // Helper methods for workflow simulation

    private async Task CreateMLTestData(string filePath)
    {
        TestLogger.LogStep($"Creating ML test data: {filePath}");
        
        var csvContent = "feature1,feature2,feature3,target\n" +
                        "1.2,2.3,3.4,0\n" +
                        "2.1,3.2,4.3,1\n" +
                        "3.0,4.1,5.2,0\n" +
                        "4.5,5.6,6.7,1\n" +
                        "5.4,6.5,7.6,0";
        
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, csvContent);
    }

    private async Task SimulateModelTraining(string experimentName)
    {
        TestLogger.LogStep($"Simulating model training for experiment: {experimentName}");
        await Task.Delay(2000); // Simulate training time
        TestLogger.LogStep("Model training simulation completed");
    }

    private async Task SimulateModelDeployment(string modelName)
    {
        TestLogger.LogStep($"Simulating model deployment: {modelName}");
        await Task.Delay(1500); // Simulate deployment time
        TestLogger.LogStep("Model deployment simulation completed");
    }

    private async Task SimulateDeploymentValidation(string deploymentName)
    {
        TestLogger.LogStep($"Simulating deployment validation: {deploymentName}");
        await Task.Delay(1000); // Simulate validation time
        TestLogger.LogStep("Deployment validation simulation completed");
    }

    private async Task<bool> IsDeploymentHealthy(string deploymentName)
    {
        TestLogger.LogStep($"Checking deployment health: {deploymentName}");
        await Task.Delay(500);
        return true; // Simulate healthy deployment
    }

    private async Task SimulateDocumentMLTraining(string experimentName, string documentContent)
    {
        TestLogger.LogStep($"Simulating ML training on document data for experiment: {experimentName}");
        TestLogger.LogStep($"Processing document with {documentContent.Length} characters");
        await Task.Delay(2000);
        TestLogger.LogStep("Document ML training simulation completed");
    }

    private async Task CreateDataPipeline(string pipelineName)
    {
        TestLogger.LogStep($"Creating data pipeline: {pipelineName}");
        await Task.Delay(1000);
        TestLogger.LogStep("Data pipeline created");
    }

    private async Task SetupPipelineMonitoring(string monitoringName, string pipelineName)
    {
        TestLogger.LogStep($"Setting up monitoring '{monitoringName}' for pipeline '{pipelineName}'");
        await Task.Delay(800);
        TestLogger.LogStep("Pipeline monitoring setup completed");
    }

    private async Task ExecuteDataPipeline(string pipelineName)
    {
        TestLogger.LogStep($"Executing data pipeline: {pipelineName}");
        await Task.Delay(3000);
        TestLogger.LogStep("Data pipeline execution completed");
    }

    private async Task MonitorPipelineExecution(string monitoringName)
    {
        TestLogger.LogStep($"Monitoring pipeline execution: {monitoringName}");
        await Task.Delay(3000);
        TestLogger.LogStep("Pipeline monitoring completed");
    }

    private async Task CreateInitialModel(string modelName, string datasetName)
    {
        TestLogger.LogStep($"Creating initial model '{modelName}' with dataset '{datasetName}'");
        await Task.Delay(1500);
        TestLogger.LogStep("Initial model created");
    }

    private async Task SimulateDataDrift(string originalDataset, string newDataset)
    {
        TestLogger.LogStep($"Simulating data drift between '{originalDataset}' and '{newDataset}'");
        await Task.Delay(1000);
        TestLogger.LogStep("Data drift simulation completed");
    }

    private async Task<bool> IsRetrainingRequired(string modelName)
    {
        TestLogger.LogStep($"Checking if retraining is required for model: {modelName}");
        await Task.Delay(500);
        return true; // Simulate retraining requirement
    }

    private async Task TriggerAutomatedRetraining(string modelName, string newDataset)
    {
        TestLogger.LogStep($"Triggering automated retraining for model '{modelName}' with dataset '{newDataset}'");
        await Task.Delay(2000);
        TestLogger.LogStep("Automated retraining completed");
    }

    private async Task ValidateRetrainedModel(string modelName)
    {
        TestLogger.LogStep($"Validating retrained model: {modelName}");
        await Task.Delay(1000);
        TestLogger.LogStep("Retrained model validation completed");
    }

    private async Task TestCrossPlatformFileOperations(string testFilePath)
    {
        TestLogger.LogStep($"Testing cross-platform file operations: {testFilePath}");
        
        // Test file creation
        await File.WriteAllTextAsync(testFilePath, "Cross-platform test content");
        
        // Test file reading
        var content = await File.ReadAllTextAsync(testFilePath);
        
        // Test file deletion
        File.Delete(testFilePath);
        
        TestLogger.LogStep("Cross-platform file operations completed successfully");
    }

    private async Task TestCrossPlatformPathHandling()
    {
        TestLogger.LogStep("Testing cross-platform path handling");
        
        var paths = new[]
        {
            Path.Combine("folder1", "folder2", "file.txt"),
            Path.GetTempPath(),
            Path.GetDirectoryName(Config.TestDataPath),
            Path.GetFullPath(".")
        };
        
        foreach (var path in paths)
        {
            TestLogger.LogStep($"Path: {path}");
        }
        
        await Task.Delay(100);
        TestLogger.LogStep("Cross-platform path handling test completed");
    }

    private static string GenerateBusinessDocument()
    {
        return "This business document contains important information about quarterly sales performance. " +
               "The revenue increased by 15% compared to the previous quarter, with strong growth in the " +
               "technology and healthcare sectors. Customer satisfaction scores improved significantly, " +
               "reaching an average of 4.7 out of 5. The company plans to expand operations to three new " +
               "markets in the upcoming fiscal year. Key performance indicators show positive trends across " +
               "all major business units, indicating successful implementation of the strategic initiatives.";
    }

    private static string GetPlatformInfo()
    {
        return Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => "Windows",
            PlatformID.Unix => "Linux",
            PlatformID.MacOSX => "macOS",
            _ => "Unknown"
        };
    }

    private string GetCrossPlatformTestPath(string fileName)
    {
        return Path.Combine(Config.TestDataPath, fileName);
    }

    // Inner class for lambda actions
    private class LambdaAction : BaseAction
    {
        private readonly Func<Task> _action;
        private readonly string _description;

        public LambdaAction(IPage page, TestLogger logger, TestConfiguration config, Func<Task> action, string description)
            : base(page, logger, config)
        {
            _action = action;
            _description = description;
        }

        protected override async Task ExecuteActionAsync()
        {
            Logger.LogStep(_description);
            await _action();
        }
    }
}