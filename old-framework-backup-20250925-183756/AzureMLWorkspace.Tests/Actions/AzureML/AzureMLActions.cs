using Microsoft.Playwright;
using AzureMLWorkspace.Tests.Actions.Core;
using AzureMLWorkspace.Tests.Helpers;
using AzureMLWorkspace.Tests.Configuration;

namespace AzureMLWorkspace.Tests.Actions.AzureML;

/// <summary>
/// Azure ML specific actions for workspace management and operations
/// </summary>
public static class AzureMLActions
{
    /// <summary>
    /// Login to Azure ML workspace
    /// </summary>
    public static LoginAction Login(IPage page, TestLogger logger, TestConfiguration config, string email, string password)
        => new(page, logger, config, email, password);

    /// <summary>
    /// Navigate to workspace home
    /// </summary>
    public static NavigateToWorkspaceAction NavigateToWorkspace(IPage page, TestLogger logger, TestConfiguration config, string workspaceName)
        => new(page, logger, config, workspaceName);

    /// <summary>
    /// Create a new notebook
    /// </summary>
    public static CreateNotebookAction CreateNotebook(IPage page, TestLogger logger, TestConfiguration config, string notebookName)
        => new(page, logger, config, notebookName);

    /// <summary>
    /// Open existing notebook
    /// </summary>
    public static OpenNotebookAction OpenNotebook(IPage page, TestLogger logger, TestConfiguration config, string notebookName)
        => new(page, logger, config, notebookName);

    /// <summary>
    /// Run notebook cell
    /// </summary>
    public static RunNotebookCellAction RunNotebookCell(IPage page, TestLogger logger, TestConfiguration config, int cellIndex = 0)
        => new(page, logger, config, cellIndex);

    /// <summary>
    /// Create compute instance
    /// </summary>
    public static CreateComputeInstanceAction CreateComputeInstance(IPage page, TestLogger logger, TestConfiguration config, string instanceName, string vmSize)
        => new(page, logger, config, instanceName, vmSize);

    /// <summary>
    /// Start compute instance
    /// </summary>
    public static StartComputeInstanceAction StartComputeInstance(IPage page, TestLogger logger, TestConfiguration config, string instanceName)
        => new(page, logger, config, instanceName);

    /// <summary>
    /// Stop compute instance
    /// </summary>
    public static StopComputeInstanceAction StopComputeInstance(IPage page, TestLogger logger, TestConfiguration config, string instanceName)
        => new(page, logger, config, instanceName);

    /// <summary>
    /// Upload dataset
    /// </summary>
    public static UploadDatasetAction UploadDataset(IPage page, TestLogger logger, TestConfiguration config, string datasetName, string filePath)
        => new(page, logger, config, datasetName, filePath);

    /// <summary>
    /// Create experiment
    /// </summary>
    public static CreateExperimentAction CreateExperiment(IPage page, TestLogger logger, TestConfiguration config, string experimentName)
        => new(page, logger, config, experimentName);

    /// <summary>
    /// Deploy model
    /// </summary>
    public static DeployModelAction DeployModel(IPage page, TestLogger logger, TestConfiguration config, string modelName, string deploymentName)
        => new(page, logger, config, modelName, deploymentName);
}

/// <summary>
/// Login to Azure ML workspace
/// </summary>
public class LoginAction : BaseAction
{
    private readonly string _email;
    private readonly string _password;

    public LoginAction(IPage page, TestLogger logger, TestConfiguration config, string email, string password)
        : base(page, logger, config)
    {
        _email = email;
        _password = password;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep("Logging into Azure ML workspace");
        
        // Navigate to Azure ML portal
        await Page.GotoAsync("https://ml.azure.com", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        
        // Check if already logged in
        if (await IsElementVisibleAsync("[data-testid='workspace-selector']"))
        {
            Logger.LogStep("Already logged in");
            return;
        }
        
        // Handle Microsoft login flow
        await WaitForElementAsync("input[type='email']", 10000);
        await Page.FillAsync("input[type='email']", _email);
        await Page.ClickAsync("input[type='submit']");
        
        await WaitForElementAsync("input[type='password']", 10000);
        await Page.FillAsync("input[type='password']", _password);
        await Page.ClickAsync("input[type='submit']");
        
        // Handle "Stay signed in?" prompt
        try
        {
            await Page.WaitForSelectorAsync("input[type='submit'][value='Yes']", new PageWaitForSelectorOptions { Timeout = 5000 });
            await Page.ClickAsync("input[type='submit'][value='Yes']");
        }
        catch
        {
            // Prompt may not appear, continue
        }
        
        // Wait for workspace to load
        await WaitForElementAsync("[data-testid='workspace-selector']", 30000);
        Logger.LogStep("Login completed successfully");
    }
}

/// <summary>
/// Navigate to specific workspace
/// </summary>
public class NavigateToWorkspaceAction : BaseAction
{
    private readonly string _workspaceName;

    public NavigateToWorkspaceAction(IPage page, TestLogger logger, TestConfiguration config, string workspaceName)
        : base(page, logger, config)
    {
        _workspaceName = workspaceName;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Navigating to workspace: {_workspaceName}");
        
        // Click workspace selector
        await WaitForElementAsync("[data-testid='workspace-selector']");
        await Page.ClickAsync("[data-testid='workspace-selector']");
        
        // Select workspace from dropdown
        await WaitForElementAsync($"text={_workspaceName}");
        await Page.ClickAsync($"text={_workspaceName}");
        
        // Wait for workspace to load
        await WaitForPageLoadAsync();
        Logger.LogStep($"Successfully navigated to workspace: {_workspaceName}");
    }
}

/// <summary>
/// Create a new notebook
/// </summary>
public class CreateNotebookAction : BaseAction
{
    private readonly string _notebookName;

    public CreateNotebookAction(IPage page, TestLogger logger, TestConfiguration config, string notebookName)
        : base(page, logger, config)
    {
        _notebookName = notebookName;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Creating notebook: {_notebookName}");
        
        // Navigate to Notebooks section
        await WaitForElementAsync("text=Notebooks");
        await Page.ClickAsync("text=Notebooks");
        
        // Click Create button
        await WaitForElementAsync("[data-testid='create-notebook-button']");
        await Page.ClickAsync("[data-testid='create-notebook-button']");
        
        // Enter notebook name
        await WaitForElementAsync("input[placeholder*='notebook name']");
        await Page.FillAsync("input[placeholder*='notebook name']", _notebookName);
        
        // Click Create
        await Page.ClickAsync("button:has-text('Create')");
        
        // Wait for notebook to be created and opened
        await WaitForElementAsync(".notebook-editor", 30000);
        Logger.LogStep($"Notebook created successfully: {_notebookName}");
    }
}

/// <summary>
/// Open existing notebook
/// </summary>
public class OpenNotebookAction : BaseAction
{
    private readonly string _notebookName;

    public OpenNotebookAction(IPage page, TestLogger logger, TestConfiguration config, string notebookName)
        : base(page, logger, config)
    {
        _notebookName = notebookName;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Opening notebook: {_notebookName}");
        
        // Navigate to Notebooks section
        await WaitForElementAsync("text=Notebooks");
        await Page.ClickAsync("text=Notebooks");
        
        // Find and click the notebook
        await WaitForElementAsync($"text={_notebookName}");
        await Page.ClickAsync($"text={_notebookName}");
        
        // Wait for notebook to load
        await WaitForElementAsync(".notebook-editor", 30000);
        Logger.LogStep($"Notebook opened successfully: {_notebookName}");
    }
}

/// <summary>
/// Run notebook cell
/// </summary>
public class RunNotebookCellAction : BaseAction
{
    private readonly int _cellIndex;

    public RunNotebookCellAction(IPage page, TestLogger logger, TestConfiguration config, int cellIndex)
        : base(page, logger, config)
    {
        _cellIndex = cellIndex;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Running notebook cell at index: {_cellIndex}");
        
        // Select the cell
        var cellSelector = $".notebook-cell:nth-child({_cellIndex + 1})";
        await WaitForElementAsync(cellSelector);
        await Page.ClickAsync(cellSelector);
        
        // Run the cell (Shift+Enter)
        await Page.Keyboard.PressAsync("Shift+Enter");
        
        // Wait for cell execution to complete
        await Page.WaitForSelectorAsync($"{cellSelector} .cell-output", new PageWaitForSelectorOptions { Timeout = 60000 });
        Logger.LogStep($"Cell execution completed for index: {_cellIndex}");
    }
}

/// <summary>
/// Create compute instance
/// </summary>
public class CreateComputeInstanceAction : BaseAction
{
    private readonly string _instanceName;
    private readonly string _vmSize;

    public CreateComputeInstanceAction(IPage page, TestLogger logger, TestConfiguration config, string instanceName, string vmSize)
        : base(page, logger, config)
    {
        _instanceName = instanceName;
        _vmSize = vmSize;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Creating compute instance: {_instanceName} with VM size: {_vmSize}");
        
        // Navigate to Compute section
        await WaitForElementAsync("text=Compute");
        await Page.ClickAsync("text=Compute");
        
        // Click Create button
        await WaitForElementAsync("[data-testid='create-compute-button']");
        await Page.ClickAsync("[data-testid='create-compute-button']");
        
        // Enter instance name
        await WaitForElementAsync("input[placeholder*='compute name']");
        await Page.FillAsync("input[placeholder*='compute name']", _instanceName);
        
        // Select VM size
        await WaitForElementAsync("select[data-testid='vm-size-selector']");
        await Page.SelectOptionAsync("select[data-testid='vm-size-selector']", _vmSize);
        
        // Click Create
        await Page.ClickAsync("button:has-text('Create')");
        
        // Wait for creation to complete
        await WaitForElementAsync($"text={_instanceName}", 120000);
        Logger.LogStep($"Compute instance created successfully: {_instanceName}");
    }
}

/// <summary>
/// Start compute instance
/// </summary>
public class StartComputeInstanceAction : BaseAction
{
    private readonly string _instanceName;

    public StartComputeInstanceAction(IPage page, TestLogger logger, TestConfiguration config, string instanceName)
        : base(page, logger, config)
    {
        _instanceName = instanceName;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Starting compute instance: {_instanceName}");
        
        // Navigate to Compute section
        await WaitForElementAsync("text=Compute");
        await Page.ClickAsync("text=Compute");
        
        // Find the instance and click Start
        var instanceRow = Page.Locator($"tr:has-text('{_instanceName}')");
        await instanceRow.Locator("button:has-text('Start')").ClickAsync();
        
        // Wait for instance to start
        await Page.WaitForSelectorAsync($"tr:has-text('{_instanceName}') td:has-text('Running')", 
            new PageWaitForSelectorOptions { Timeout = 300000 });
        
        Logger.LogStep($"Compute instance started successfully: {_instanceName}");
    }
}

/// <summary>
/// Stop compute instance
/// </summary>
public class StopComputeInstanceAction : BaseAction
{
    private readonly string _instanceName;

    public StopComputeInstanceAction(IPage page, TestLogger logger, TestConfiguration config, string instanceName)
        : base(page, logger, config)
    {
        _instanceName = instanceName;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Stopping compute instance: {_instanceName}");
        
        // Navigate to Compute section
        await WaitForElementAsync("text=Compute");
        await Page.ClickAsync("text=Compute");
        
        // Find the instance and click Stop
        var instanceRow = Page.Locator($"tr:has-text('{_instanceName}')");
        await instanceRow.Locator("button:has-text('Stop')").ClickAsync();
        
        // Wait for instance to stop
        await Page.WaitForSelectorAsync($"tr:has-text('{_instanceName}') td:has-text('Stopped')", 
            new PageWaitForSelectorOptions { Timeout = 300000 });
        
        Logger.LogStep($"Compute instance stopped successfully: {_instanceName}");
    }
}

/// <summary>
/// Upload dataset
/// </summary>
public class UploadDatasetAction : BaseAction
{
    private readonly string _datasetName;
    private readonly string _filePath;

    public UploadDatasetAction(IPage page, TestLogger logger, TestConfiguration config, string datasetName, string filePath)
        : base(page, logger, config)
    {
        _datasetName = datasetName;
        _filePath = filePath;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Uploading dataset: {_datasetName} from file: {_filePath}");
        
        // Navigate to Data section
        await WaitForElementAsync("text=Data");
        await Page.ClickAsync("text=Data");
        
        // Click Create button
        await WaitForElementAsync("[data-testid='create-dataset-button']");
        await Page.ClickAsync("[data-testid='create-dataset-button']");
        
        // Enter dataset name
        await WaitForElementAsync("input[placeholder*='dataset name']");
        await Page.FillAsync("input[placeholder*='dataset name']", _datasetName);
        
        // Upload file
        var normalizedPath = Path.GetFullPath(_filePath);
        await WaitForElementAsync("input[type='file']");
        await Page.SetInputFilesAsync("input[type='file']", normalizedPath);
        
        // Click Create
        await Page.ClickAsync("button:has-text('Create')");
        
        // Wait for upload to complete
        await WaitForElementAsync($"text={_datasetName}", 60000);
        Logger.LogStep($"Dataset uploaded successfully: {_datasetName}");
    }
}

/// <summary>
/// Create experiment
/// </summary>
public class CreateExperimentAction : BaseAction
{
    private readonly string _experimentName;

    public CreateExperimentAction(IPage page, TestLogger logger, TestConfiguration config, string experimentName)
        : base(page, logger, config)
    {
        _experimentName = experimentName;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Creating experiment: {_experimentName}");
        
        // Navigate to Experiments section
        await WaitForElementAsync("text=Experiments");
        await Page.ClickAsync("text=Experiments");
        
        // Click Create button
        await WaitForElementAsync("[data-testid='create-experiment-button']");
        await Page.ClickAsync("[data-testid='create-experiment-button']");
        
        // Enter experiment name
        await WaitForElementAsync("input[placeholder*='experiment name']");
        await Page.FillAsync("input[placeholder*='experiment name']", _experimentName);
        
        // Click Create
        await Page.ClickAsync("button:has-text('Create')");
        
        // Wait for experiment to be created
        await WaitForElementAsync($"text={_experimentName}", 30000);
        Logger.LogStep($"Experiment created successfully: {_experimentName}");
    }
}

/// <summary>
/// Deploy model
/// </summary>
public class DeployModelAction : BaseAction
{
    private readonly string _modelName;
    private readonly string _deploymentName;

    public DeployModelAction(IPage page, TestLogger logger, TestConfiguration config, string modelName, string deploymentName)
        : base(page, logger, config)
    {
        _modelName = modelName;
        _deploymentName = deploymentName;
    }

    protected override async Task ExecuteActionAsync()
    {
        Logger.LogStep($"Deploying model: {_modelName} as deployment: {_deploymentName}");
        
        // Navigate to Models section
        await WaitForElementAsync("text=Models");
        await Page.ClickAsync("text=Models");
        
        // Find the model and click Deploy
        var modelRow = Page.Locator($"tr:has-text('{_modelName}')");
        await modelRow.Locator("button:has-text('Deploy')").ClickAsync();
        
        // Enter deployment name
        await WaitForElementAsync("input[placeholder*='deployment name']");
        await Page.FillAsync("input[placeholder*='deployment name']", _deploymentName);
        
        // Click Deploy
        await Page.ClickAsync("button:has-text('Deploy')");
        
        // Wait for deployment to complete
        await WaitForElementAsync($"text={_deploymentName}", 300000);
        Logger.LogStep($"Model deployed successfully: {_modelName} as {_deploymentName}");
    }
}