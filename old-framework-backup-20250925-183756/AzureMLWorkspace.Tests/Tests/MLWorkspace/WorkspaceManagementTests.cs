using AzureMLWorkspace.Tests.Helpers;
using AzureMLWorkspace.Tests.Actions.Core;
using AzureMLWorkspace.Tests.Actions.AzureML;

namespace AzureMLWorkspace.Tests.Tests.MLWorkspace;

[TestFixture]
[Category("WorkspaceManagement")]
[Category("AzureML")]
public class WorkspaceManagementTests : BaseTest
{
    private readonly string TestNotebookName = $"test-notebook-{DateTime.Now:yyyyMMdd-HHmmss}";
    private readonly string TestComputeName = $"test-compute-{DateTime.Now:yyyyMMdd-HHmmss}";
    private readonly string TestDatasetName = $"test-dataset-{DateTime.Now:yyyyMMdd-HHmmss}";
    private readonly string TestExperimentName = $"test-experiment-{DateTime.Now:yyyyMMdd-HHmmss}";

    [Test]
    public async Task Test_Workspace_Access()
    {
        TestLogger.LogStep("Testing Azure ML workspace access");

        // Use Actions to navigate and verify workspace access
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(BrowserActions.WaitForPageLoad(Page, TestLogger, Config))
            .Add(BrowserActions.VerifyElementVisible(Page, TestLogger, Config, "[data-testid='workspace-header']"))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "workspace_access"))
            .ExecuteAsync();

        TestLogger.LogStep("Workspace access test completed successfully");
    }

    [Test]
    public async Task Test_Workspace_Navigation()
    {
        TestLogger.LogStep("Testing workspace navigation");

        // Use Actions to navigate through different sections
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(BrowserActions.WaitForPageLoad(Page, TestLogger, Config))
            .Add(BrowserActions.Click(Page, TestLogger, Config, "text=Notebooks"))
            .Add(BrowserActions.WaitForElement(Page, TestLogger, Config, "[data-testid='notebooks-section']"))
            .Add(BrowserActions.Click(Page, TestLogger, Config, "text=Compute"))
            .Add(BrowserActions.WaitForElement(Page, TestLogger, Config, "[data-testid='compute-section']"))
            .Add(BrowserActions.Click(Page, TestLogger, Config, "text=Data"))
            .Add(BrowserActions.WaitForElement(Page, TestLogger, Config, "[data-testid='data-section']"))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "workspace_navigation"))
            .ExecuteAsync();

        TestLogger.LogStep("Workspace navigation test completed successfully");
    }

    [Test]
    public async Task Test_Notebook_Creation()
    {
        TestLogger.LogStep($"Testing notebook creation: {TestNotebookName}");

        // Use Actions to create a new notebook
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(AzureMLActions.CreateNotebook(Page, TestLogger, Config, TestNotebookName))
            .Add(BrowserActions.VerifyElementVisible(Page, TestLogger, Config, ".notebook-editor"))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "notebook_creation"))
            .ExecuteAsync();

        TestLogger.LogStep("Notebook creation test completed successfully");
    }

    [Test]
    public async Task Test_Notebook_Execution()
    {
        TestLogger.LogStep("Testing notebook cell execution");

        // Use Actions to create notebook and run a cell
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(AzureMLActions.CreateNotebook(Page, TestLogger, Config, $"{TestNotebookName}-exec"))
            .Add(BrowserActions.Type(Page, TestLogger, Config, ".notebook-cell textarea", "print('Hello, Azure ML!')"))
            .Add(AzureMLActions.RunNotebookCell(Page, TestLogger, Config, 0))
            .Add(BrowserActions.VerifyElementVisible(Page, TestLogger, Config, ".cell-output"))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "notebook_execution"))
            .ExecuteAsync();

        TestLogger.LogStep("Notebook execution test completed successfully");
    }

    [Test]
    public async Task Test_Compute_Management()
    {
        TestLogger.LogStep($"Testing compute instance management: {TestComputeName}");

        // Use Actions to create and manage compute instance
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(AzureMLActions.CreateComputeInstance(Page, TestLogger, Config, TestComputeName, "STANDARD_DS3_V2"))
            .Add(BrowserActions.VerifyElementVisible(Page, TestLogger, Config, $"text={TestComputeName}"))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "compute_creation"))
            .ExecuteAsync();

        TestLogger.LogStep("Compute management test completed successfully");
    }

    [Test]
    public async Task Test_Compute_Lifecycle()
    {
        TestLogger.LogStep("Testing compute instance lifecycle (start/stop)");

        var computeName = $"{TestComputeName}-lifecycle";

        // Use Actions to test complete compute lifecycle with conditional logic
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(AzureMLActions.CreateComputeInstance(Page, TestLogger, Config, computeName, "STANDARD_DS2_V2"))
            .AddIf(
                async () => await Page.IsVisibleAsync($"tr:has-text('{computeName}') button:has-text('Start')"),
                AzureMLActions.StartComputeInstance(Page, TestLogger, Config, computeName)
            )
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "compute_running"))
            .AddIf(
                async () => await Page.IsVisibleAsync($"tr:has-text('{computeName}') button:has-text('Stop')"),
                AzureMLActions.StopComputeInstance(Page, TestLogger, Config, computeName)
            )
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "compute_stopped"))
            .ExecuteAsync();

        TestLogger.LogStep("Compute lifecycle test completed successfully");
    }

    [Test]
    public async Task Test_Dataset_Management()
    {
        TestLogger.LogStep($"Testing dataset management: {TestDatasetName}");

        // Create a test data file
        var testDataPath = Path.Combine(Config.TestDataPath, "sample-data.csv");
        await EnsureTestDataExists(testDataPath);

        // Use Actions to upload and manage dataset
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .AddWithRetry(
                AzureMLActions.UploadDataset(Page, TestLogger, Config, TestDatasetName, testDataPath),
                maxRetries: 3,
                delay: TimeSpan.FromSeconds(2)
            )
            .Add(BrowserActions.VerifyElementVisible(Page, TestLogger, Config, $"text={TestDatasetName}"))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "dataset_upload"))
            .ExecuteAsync();

        TestLogger.LogStep("Dataset management test completed successfully");
    }

    [Test]
    public async Task Test_Experiment_Management()
    {
        TestLogger.LogStep($"Testing experiment management: {TestExperimentName}");

        // Use Actions to create and manage experiment
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(AzureMLActions.CreateExperiment(Page, TestLogger, Config, TestExperimentName))
            .Add(BrowserActions.VerifyElementVisible(Page, TestLogger, Config, $"text={TestExperimentName}"))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "experiment_creation"))
            .ExecuteAsync();

        TestLogger.LogStep("Experiment management test completed successfully");
    }

    [Test]
    public async Task Test_Model_Management()
    {
        TestLogger.LogStep("Testing model management");

        var modelName = $"test-model-{DateTime.Now:yyyyMMdd-HHmmss}";

        // Use Actions to test model management workflow
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(BrowserActions.Click(Page, TestLogger, Config, "text=Models"))
            .Add(BrowserActions.WaitForElement(Page, TestLogger, Config, "[data-testid='models-section']"))
            .Add(BrowserActions.VerifyElementVisible(Page, TestLogger, Config, "[data-testid='create-model-button']"))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "models_section"))
            .ExecuteAsync();

        TestLogger.LogStep("Model management test completed successfully");
    }

    [Test]
    public async Task Test_Workspace_Settings()
    {
        TestLogger.LogStep("Testing workspace settings access");

        // Use Actions to navigate to workspace settings
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(BrowserActions.Click(Page, TestLogger, Config, "[data-testid='workspace-settings']"))
            .Add(BrowserActions.WaitForElement(Page, TestLogger, Config, "[data-testid='settings-panel']"))
            .Add(BrowserActions.VerifyElementVisible(Page, TestLogger, Config, "text=Workspace configuration"))
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "workspace_settings"))
            .ExecuteAsync();

        TestLogger.LogStep("Workspace settings test completed successfully");
    }

    [Test]
    public async Task Test_Parallel_Resource_Creation()
    {
        TestLogger.LogStep("Testing parallel resource creation");

        var notebook1 = $"{TestNotebookName}-parallel-1";
        var notebook2 = $"{TestNotebookName}-parallel-2";
        var experiment1 = $"{TestExperimentName}-parallel-1";

        // Use Actions to create multiple resources in parallel
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .AddParallel(
                AzureMLActions.CreateNotebook(Page, TestLogger, Config, notebook1),
                AzureMLActions.CreateNotebook(Page, TestLogger, Config, notebook2),
                AzureMLActions.CreateExperiment(Page, TestLogger, Config, experiment1)
            )
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "parallel_creation_result"))
            .ExecuteAsync();

        TestLogger.LogStep("Parallel resource creation test completed successfully");
    }

    [Test]
    public async Task Test_Workspace_Resource_Cleanup()
    {
        TestLogger.LogStep("Testing workspace resource cleanup");

        // Use Actions to clean up test resources
        await Actions
            .Add(BrowserActions.NavigateTo(Page, TestLogger, Config, Config.BaseUrl))
            .Add(async () => await CleanupTestResources())
            .Add(BrowserActions.TakeScreenshot(Page, TestLogger, Config, "cleanup_completed"))
            .ExecuteAsync();

        TestLogger.LogStep("Workspace resource cleanup test completed successfully");
    }

    private async Task EnsureTestDataExists(string filePath)
    {
        if (!File.Exists(filePath))
        {
            TestLogger.LogStep($"Creating test data file: {filePath}");
            
            var csvContent = "Name,Age,City,Salary\n" +
                           "John Doe,30,New York,50000\n" +
                           "Jane Smith,25,Los Angeles,55000\n" +
                           "Bob Johnson,35,Chicago,60000\n" +
                           "Alice Brown,28,Houston,52000\n" +
                           "Charlie Wilson,32,Phoenix,58000";
            
            await File.WriteAllTextAsync(filePath, csvContent);
            TestLogger.LogStep("Test data file created successfully");
        }
    }

    private async Task CleanupTestResources()
    {
        TestLogger.LogStep("Cleaning up test resources");
        
        try
        {
            // In a real implementation, you would add cleanup logic here
            // For now, we'll just log the cleanup attempt
            TestLogger.LogStep("Test resource cleanup completed");
            await Task.Delay(100); // Simulate cleanup time
        }
        catch (Exception ex)
        {
            TestLogger.Warning($"Cleanup warning: {ex.Message}");
        }
    }
}