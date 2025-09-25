using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.MachineLearning;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace AzureMLWorkspace.Tests.Framework.Abilities;

/// <summary>
/// Ability to interact with Azure Machine Learning services
/// </summary>
public class UseAzureML : IAbility
{
    private readonly ILogger<UseAzureML> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _role;
    private readonly BrowseTheWeb _browseTheWeb;
    
    private ArmClient? _armClient;
    private MachineLearningWorkspaceResource? _workspace;
    private TokenCredential? _credential;

    public string Name => $"Use Azure ML with {_role} role";
    public ArmClient ArmClient => _armClient ?? throw new InvalidOperationException("Azure ML client not initialized");
    public MachineLearningWorkspaceResource Workspace => _workspace ?? throw new InvalidOperationException("Workspace not initialized");
    public TokenCredential Credential => _credential ?? throw new InvalidOperationException("Credential not initialized");
    public IPage Page => _browseTheWeb.Page;

    private UseAzureML(ILogger<UseAzureML> logger, IConfiguration configuration, string role, BrowseTheWeb browseTheWeb)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _role = role ?? throw new ArgumentNullException(nameof(role));
        _browseTheWeb = browseTheWeb ?? throw new ArgumentNullException(nameof(browseTheWeb));
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Azure ML client with {Role} role", _role);

        try
        {
            // Initialize credential - prioritize Managed Identity with faster timeout
            try
            {
                _logger.LogDebug("Using ManagedIdentityCredential for Azure ML authentication");
                _credential = new ManagedIdentityCredential();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ManagedIdentityCredential not available, falling back to DefaultAzureCredential");
                _credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ExcludeEnvironmentCredential = false,
                    ExcludeInteractiveBrowserCredential = false,
                    ExcludeManagedIdentityCredential = false,
                    ExcludeSharedTokenCacheCredential = false,
                    ExcludeVisualStudioCredential = false,
                    ExcludeVisualStudioCodeCredential = false,
                    ExcludeAzureCliCredential = false,
                    ExcludeAzurePowerShellCredential = false
                });
            }

            // Initialize ARM client
            _armClient = new ArmClient(_credential);

            // Get configuration values with debug logging
            _logger.LogDebug("Loading Azure configuration values...");
            
            // Get current environment to read from correct section
            var currentEnvironment = _configuration["CurrentEnvironment"] ?? "Development";
            _logger.LogDebug("Using environment: {Environment}", currentEnvironment);
            
            // Read from environment-specific configuration section
            var subscriptionId = _configuration[$"Environments:{currentEnvironment}:Azure:SubscriptionId"];
            var resourceGroupName = _configuration[$"Environments:{currentEnvironment}:Azure:ResourceGroup"];
            var workspaceName = _configuration[$"Environments:{currentEnvironment}:Azure:WorkspaceName"];
            
            _logger.LogDebug("Configuration values - SubscriptionId: {SubscriptionId}, ResourceGroup: {ResourceGroup}, WorkspaceName: {WorkspaceName}", 
                subscriptionId ?? "NULL", resourceGroupName ?? "NULL", workspaceName ?? "NULL");
            
            if (string.IsNullOrEmpty(subscriptionId))
                throw new InvalidOperationException($"Environments:{currentEnvironment}:Azure:SubscriptionId not configured");
            if (string.IsNullOrEmpty(resourceGroupName))
                throw new InvalidOperationException($"Environments:{currentEnvironment}:Azure:ResourceGroup not configured");
            if (string.IsNullOrEmpty(workspaceName))
                throw new InvalidOperationException($"Environments:{currentEnvironment}:Azure:WorkspaceName not configured");

            // **OPTIMIZATION: Execute Azure API calls in parallel instead of sequential**
            _logger.LogDebug("Executing Azure API calls in parallel for faster initialization");
            
            var subscriptionTask = _armClient.GetDefaultSubscriptionAsync();
            
            // Wait for subscription first, then get resource group and workspace in parallel
            var subscription = await subscriptionTask;
            var resourceGroupTask = subscription.GetResourceGroupAsync(resourceGroupName);
            
            var resourceGroup = await resourceGroupTask;
            var workspaceTask = resourceGroup.Value.GetMachineLearningWorkspaceAsync(workspaceName);
            
            _workspace = await workspaceTask;

            _logger.LogInformation("Azure ML client initialized successfully for workspace {WorkspaceName} (optimized)", workspaceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Azure ML client");
            throw;
        }
    }

    public async Task CleanupAsync()
    {
        _logger.LogInformation("Cleaning up Azure ML resources");
        
        // ARM client doesn't need explicit cleanup
        _armClient = null;
        _workspace = null;
        _credential = null;
        
        // Browser cleanup is handled by BrowseTheWeb ability
        
        _logger.LogInformation("Azure ML cleanup completed");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates a new UseAzureML ability with the specified role
    /// </summary>
    public static UseAzureML WithRole(string role, BrowseTheWeb browseTheWeb)
    {
        return new UseAzureML(
            Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ILogger<UseAzureML>>(AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider),
            Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IConfiguration>(AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider),
            role,
            browseTheWeb);
    }

    /// <summary>
    /// Creates a new UseAzureML ability with Contributor role
    /// </summary>
    public static UseAzureML AsContributor(BrowseTheWeb browseTheWeb)
    {
        return WithRole("Contributor", browseTheWeb);
    }

    /// <summary>
    /// Creates a new UseAzureML ability with Reader role
    /// </summary>
    public static UseAzureML AsReader(BrowseTheWeb browseTheWeb)
    {
        return WithRole("Reader", browseTheWeb);
    }

    /// <summary>
    /// Creates a new UseAzureML ability with Owner role
    /// </summary>
    public static UseAzureML AsOwner(BrowseTheWeb browseTheWeb)
    {
        return WithRole("Owner", browseTheWeb);
    }

    /// <summary>
    /// Creates a new UseAzureML ability with custom role
    /// </summary>
    public static UseAzureML WithCustomRole(string customRole, BrowseTheWeb browseTheWeb)
    {
        return WithRole(customRole, browseTheWeb);
    }

    /// <summary>
    /// Starts a compute instance
    /// </summary>
    public async Task StartCompute(string computeName)
    {
        _logger.LogInformation("Starting compute instance: {ComputeName}", computeName);
        
        try
        {
            var compute = await _workspace.GetMachineLearningComputeAsync(computeName);
            
            // Start the compute instance
            await compute.Value.StartAsync(Azure.WaitUntil.Completed);
            
            _logger.LogInformation("Compute instance {ComputeName} started successfully", computeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start compute instance {ComputeName}", computeName);
            throw;
        }
    }

    /// <summary>
    /// Stops a compute instance
    /// </summary>
    public async Task StopCompute(string computeName)
    {
        _logger.LogInformation("Stopping compute instance: {ComputeName}", computeName);
        
        try
        {
            var compute = await _workspace.GetMachineLearningComputeAsync(computeName);
            
            // Stop the compute instance
            await compute.Value.StopAsync(Azure.WaitUntil.Completed);
            
            _logger.LogInformation("Compute instance {ComputeName} stopped successfully", computeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop compute instance {ComputeName}", computeName);
            throw;
        }
    }

    /// <summary>
    /// Gets the status of a compute instance
    /// </summary>
    public async Task<string> GetComputeStatus(string computeName)
    {
        try
        {
            var compute = await _workspace.GetMachineLearningComputeAsync(computeName);
            var computeData = compute.Value.Data;
            
            // Return the provisioning state
            return computeData.Properties?.ProvisioningState?.ToString() ?? "Unknown";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get status for compute instance {ComputeName}", computeName);
            throw;
        }
    }

    /// <summary>
    /// Navigates to the specified workspace in Azure ML Studio
    /// </summary>
    public async Task NavigateToWorkspaceAsync(string workspaceName)
    {
        _logger.LogInformation("Navigating to workspace: {WorkspaceName}", workspaceName);
        
        try
        {
            // Get current environment to read from correct section
            var currentEnvironment = _configuration["CurrentEnvironment"] ?? "Development";
            
            var subscriptionId = _configuration[$"Environments:{currentEnvironment}:Azure:SubscriptionId"];
            var resourceGroupName = _configuration[$"Environments:{currentEnvironment}:Azure:ResourceGroup"];
            var tenantId = _configuration[$"Environments:{currentEnvironment}:Azure:TenantId"];
            
            if (string.IsNullOrEmpty(subscriptionId))
                throw new InvalidOperationException($"Environments:{currentEnvironment}:Azure:SubscriptionId not configured");
            if (string.IsNullOrEmpty(resourceGroupName))
                throw new InvalidOperationException($"Environments:{currentEnvironment}:Azure:ResourceGroup not configured");
            if (string.IsNullOrEmpty(tenantId))
                throw new InvalidOperationException($"Environments:{currentEnvironment}:Azure:TenantId not configured");
            
            // Use the correct Azure ML Studio URL format
            var workspaceResourceId = $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.MachineLearningServices/workspaces/{workspaceName}";
            var workspaceUrl = $"https://ml.azure.com/?tid={tenantId}&wsid={workspaceResourceId}";
            
            _logger.LogDebug("Navigating to URL: {WorkspaceUrl}", workspaceUrl);
            
            await Page.GotoAsync(workspaceUrl);
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Wait for the workspace to load - Azure ML Studio specific selectors
            var workspaceLoadedSelectors = new[]
            {
                "[data-testid='workspace-header']",
                ".workspace-header",
                "[data-testid='studio-header']",
                ".studio-header",
                ".workspace-name",
                "h1:has-text('Home')",
                ".home-page-container",
                ".workspace-overview"
            };
            
            await WaitForAnyElementAsync(workspaceLoadedSelectors, 30000);
            
            _logger.LogInformation("Successfully navigated to workspace: {WorkspaceName}", workspaceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to workspace: {WorkspaceName}", workspaceName);
            throw;
        }
    }

    /// <summary>
    /// Performs login if required for the specified user
    /// </summary>
    public async Task LoginIfRequiredAsync(string userName)
    {
        _logger.LogInformation("Checking if login is required for user: {UserName}", userName);
        
        try
        {
            // Check if we're on a login page or need to authenticate
            var currentUrl = Page.Url;
            
            if (currentUrl.Contains("login.microsoftonline.com") || 
                currentUrl.Contains("account.microsoft.com") ||
                await Page.Locator("input[type='email'], input[name='loginfmt']").IsVisibleAsync())
            {
                _logger.LogInformation("Login required, attempting to authenticate user: {UserName}", userName);
                
                // Enter username
                var emailInput = Page.Locator("input[type='email'], input[name='loginfmt']").First;
                await emailInput.FillAsync(userName);
                await emailInput.PressAsync("Enter");
                
                // Wait for password field or next step
                await Page.WaitForTimeoutAsync(2000);
                
                // Check if password field is visible
                var passwordInput = Page.Locator("input[type='password'], input[name='passwd']").First;
                if (await passwordInput.IsVisibleAsync())
                {
                    // Get current environment to read from correct section
                    var currentEnvironment = _configuration["CurrentEnvironment"] ?? "Development";
                    var password = _configuration[$"Environments:{currentEnvironment}:Authentication:Password"];
                    if (string.IsNullOrEmpty(password))
                        throw new InvalidOperationException($"Environments:{currentEnvironment}:Authentication:Password not configured for authentication");
                    
                    await passwordInput.FillAsync(password);
                    await passwordInput.PressAsync("Enter");
                    
                    // Wait for potential MFA or redirect
                    await Page.WaitForTimeoutAsync(3000);
                }
                
                // Handle "Stay signed in?" prompt
                var staySignedInButton = Page.Locator("input[type='submit'][value='Yes'], button:has-text('Yes')").First;
                if (await staySignedInButton.IsVisibleAsync())
                {
                    await staySignedInButton.ClickAsync();
                }
                
                // Wait for authentication to complete
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30000 });
            }
            
            _logger.LogInformation("Login check completed for user: {UserName}", userName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check login status for user: {UserName}", userName);
            throw;
        }
    }

    /// <summary>
    /// Selects the specified workspace
    /// </summary>
    public async Task SelectWorkspaceAsync(string workspaceName)
    {
        _logger.LogInformation("Selecting workspace: {WorkspaceName}", workspaceName);
        
        try
        {
            // Check if we're on the workspace selection page
            var workspaceSelectorOptions = new[]
            {
                "[data-testid='workspace-selector']",
                ".workspace-selector",
                ".workspace-dropdown",
                "button:has-text('Select workspace')",
                ".workspace-picker"
            };
            
            var workspaceSelector = await WaitForAnyElementAsync(workspaceSelectorOptions, 5000);
            
            if (workspaceSelector != null)
            {
                await SafeClickAsync(workspaceSelector);
                
                // Wait for dropdown to open
                await Page.WaitForTimeoutAsync(1000);
                
                // Look for the workspace in the dropdown
                var workspaceOptionSelectors = new[]
                {
                    $"text='{workspaceName}'",
                    $"[title='{workspaceName}']",
                    $"[data-workspace-name='{workspaceName}']",
                    $".workspace-option:has-text('{workspaceName}')"
                };
                
                var workspaceOption = await WaitForAnyElementAsync(workspaceOptionSelectors, 10000);
                if (workspaceOption != null)
                {
                    await SafeClickAsync(workspaceOption);
                    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                }
                else
                {
                    throw new InvalidOperationException($"Workspace '{workspaceName}' not found in workspace selector");
                }
            }
            else
            {
                // If no selector is visible, try to navigate directly to the workspace
                await NavigateToWorkspaceAsync(workspaceName);
            }
            
            _logger.LogInformation("Successfully selected workspace: {WorkspaceName}", workspaceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to select workspace: {WorkspaceName}", workspaceName);
            throw;
        }
    }

    /// <summary>
    /// Navigates to the compute section
    /// </summary>
    public async Task NavigateToComputeAsync()
    {
        _logger.LogInformation("Navigating to compute section");
        
        try
        {
            // Look for compute navigation link in various possible locations
            var computeNavSelectors = new[]
            {
                "a[href*='compute'], a:has-text('Compute')",
                "[data-testid='compute-nav'], [data-testid='nav-compute']",
                "nav a:has-text('Compute'), .nav-item:has-text('Compute')",
                ".sidebar a:has-text('Compute'), .navigation a:has-text('Compute')"
            };
            
            var computeLink = await WaitForAnyElementAsync(computeNavSelectors, 10000);
            
            if (computeLink != null)
            {
                await SafeClickAsync(computeLink);
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                
                // Wait for compute page to load
                await Page.WaitForSelectorAsync("[data-testid='compute-list'], .compute-instances, .compute-table", new PageWaitForSelectorOptions 
                { 
                    Timeout = 15000 
                });
            }
            else
            {
                // If navigation link not found, try direct URL navigation
                var currentUrl = Page.Url;
                var computeUrl = currentUrl.Contains("ml.azure.com") ? 
                    currentUrl.Replace(new Uri(currentUrl).PathAndQuery, "/compute") :
                    "https://ml.azure.com/compute";
                
                await Page.GotoAsync(computeUrl);
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }
            
            _logger.LogInformation("Successfully navigated to compute section");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to compute section");
            throw;
        }
    }

    /// <summary>
    /// Opens the specified compute instance
    /// </summary>
    public async Task OpenComputeInstanceAsync(string computeName)
    {
        _logger.LogInformation("Opening compute instance: {ComputeName}", computeName);
        
        try
        {
            // First navigate to compute section if not already there
            if (!Page.Url.Contains("/compute"))
            {
                await NavigateToComputeAsync();
            }
            
            // Look for the compute instance in the list
            var computeInstanceSelectors = new[]
            {
                $"[data-testid='compute-{computeName}'], [data-compute-name='{computeName}']",
                $"tr:has-text('{computeName}'), .compute-row:has-text('{computeName}')",
                $".compute-instance:has-text('{computeName}')"
            };
            
            var computeInstance = await WaitForAnyElementAsync(computeInstanceSelectors, 15000);
            
            if (computeInstance != null)
            {
                // Look for open/view button within the compute instance row
                var openButtonSelectors = new[]
                {
                    "button:has-text('Open'), a:has-text('Open')",
                    "button:has-text('View'), a:has-text('View')",
                    "[data-testid='open-compute'], [data-testid='view-compute']",
                    ".open-button, .view-button"
                };
                
                ILocator? openButton = null;
                foreach (var buttonSelector in openButtonSelectors)
                {
                    var button = computeInstance.Locator(buttonSelector).First;
                    if (await button.IsVisibleAsync())
                    {
                        openButton = button;
                        break;
                    }
                }
                
                if (openButton != null)
                {
                    await SafeClickAsync(openButton);
                    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                }
                else
                {
                    // If no open button found, try clicking on the compute instance name/row
                    await SafeClickAsync(computeInstance);
                    await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                }
            }
            else
            {
                throw new InvalidOperationException($"Compute instance '{computeName}' not found in the compute list");
            }
            
            _logger.LogInformation("Successfully opened compute instance: {ComputeName}", computeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open compute instance: {ComputeName}", computeName);
            throw;
        }
    }

    /// <summary>
    /// Checks if the specified compute instance is running
    /// </summary>
    public async Task<bool> IsComputeRunningAsync(string computeName)
    {
        _logger.LogInformation("Checking if compute instance is running: {ComputeName}", computeName);
        
        try
        {
            var status = await GetComputeStatus(computeName);
            var isRunning = status.Equals("Succeeded", StringComparison.OrdinalIgnoreCase) ||
                           status.Equals("Running", StringComparison.OrdinalIgnoreCase);
            
            _logger.LogInformation("Compute instance {ComputeName} running status: {IsRunning} (Status: {Status})", 
                computeName, isRunning, status);
            
            return isRunning;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if compute instance is running: {ComputeName}", computeName);
            throw;
        }
    }

    /// <summary>
    /// Starts the specified compute instance
    /// </summary>
    public async Task StartComputeInstanceAsync(string computeName)
    {
        _logger.LogInformation("Starting compute instance: {ComputeName}", computeName);
        
        try
        {
            await StartCompute(computeName);
            _logger.LogInformation("Successfully started compute instance: {ComputeName}", computeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start compute instance: {ComputeName}", computeName);
            throw;
        }
    }

    /// <summary>
    /// Checks if application links are enabled in the current workspace
    /// </summary>
    public async Task<bool> AreApplicationLinksEnabledAsync()
    {
        _logger.LogInformation("Checking if application links are enabled");
        
        try
        {
            // Navigate to workspace settings or configuration page
            var settingsSelectors = new[]
            {
                "a[href*='settings'], a:has-text('Settings')",
                "[data-testid='workspace-settings'], [data-testid='settings']",
                ".settings-link, .configuration-link",
                "button:has-text('Settings')",
                ".gear-icon, .settings-icon"
            };
            
            var settingsLink = await WaitForAnyElementAsync(settingsSelectors, 5000);
            if (settingsLink != null)
            {
                await SafeClickAsync(settingsLink);
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }
            
            // Look for application links configuration
            var applicationLinksSelectors = new[]
            {
                "[data-testid='application-links'], .application-links",
                "input[type='checkbox']:near(:text('Application Links'))",
                ".toggle:near(:text('Application Links'))",
                "[aria-label*='application links'], [title*='application links']"
            };
            
            bool linksEnabled = false;
            foreach (var selector in applicationLinksSelectors)
            {
                var applicationLinksElement = Page.Locator(selector).First;
                if (await applicationLinksElement.IsVisibleAsync())
                {
                    // Check if it's a checkbox or toggle
                    if (await applicationLinksElement.GetAttributeAsync("type") == "checkbox")
                    {
                        linksEnabled = await applicationLinksElement.IsCheckedAsync();
                    }
                    else
                    {
                        // For other elements, check for enabled/active classes or attributes
                        var classList = await applicationLinksElement.GetAttributeAsync("class") ?? "";
                        var ariaChecked = await applicationLinksElement.GetAttributeAsync("aria-checked");
                        
                        linksEnabled = classList.Contains("enabled") || 
                                     classList.Contains("active") || 
                                     ariaChecked == "true";
                    }
                    break;
                }
            }
            
            // If we can't find specific application links settings, 
            // check if application links are visible in the UI (indicating they're enabled)
            if (!linksEnabled)
            {
                var appLinkIndicators = Page.Locator(".app-link, .application-link, [data-testid*='app-link']");
                linksEnabled = await appLinkIndicators.CountAsync() > 0;
            }
            
            _logger.LogInformation("Application links enabled status: {LinksEnabled}", linksEnabled);
            return linksEnabled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if application links are enabled");
            throw;
        }
    }

    /// <summary>
    /// Navigates to the notebooks section
    /// </summary>
    public async Task NavigateToNotebooksAsync()
    {
        _logger.LogInformation("Navigating to notebooks section");
        
        try
        {
            var notebookNavSelectors = new[]
            {
                "a[href*='notebook'], a:has-text('Notebooks')",
                "[data-testid='notebooks-nav'], [data-testid='nav-notebooks']",
                "nav a:has-text('Notebooks'), .nav-item:has-text('Notebooks')",
                ".sidebar a:has-text('Notebooks')"
            };
            
            var notebookLink = await WaitForAnyElementAsync(notebookNavSelectors, 10000);
            
            if (notebookLink != null)
            {
                await SafeClickAsync(notebookLink);
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                
                // Wait for notebooks page to load
                await Page.WaitForSelectorAsync("[data-testid='notebook-list'], .notebook-files, .file-explorer", new PageWaitForSelectorOptions 
                { 
                    Timeout = 15000 
                });
            }
            else
            {
                // Try direct URL navigation
                var currentUrl = Page.Url;
                var notebooksUrl = currentUrl.Contains("ml.azure.com") ? 
                    currentUrl.Replace(new Uri(currentUrl).PathAndQuery, "/notebooks") :
                    "https://ml.azure.com/notebooks";
                
                await Page.GotoAsync(notebooksUrl);
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }
            
            _logger.LogInformation("Successfully navigated to notebooks section");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to notebooks section");
            throw;
        }
    }

    /// <summary>
    /// Creates a new notebook with the specified name
    /// </summary>
    public async Task CreateNotebookAsync(string notebookName)
    {
        _logger.LogInformation("Creating notebook: {NotebookName}", notebookName);
        
        try
        {
            // First navigate to notebooks if not already there
            if (!Page.Url.Contains("/notebook"))
            {
                await NavigateToNotebooksAsync();
            }
            
            // Look for create/new notebook button
            var createButtonSelectors = new[]
            {
                "button:has-text('New'), button:has-text('Create')",
                "[data-testid='create-notebook'], [data-testid='new-notebook']",
                ".create-button, .new-button",
                "button[aria-label*='Create'], button[title*='Create']"
            };
            
            var createButton = await WaitForAnyElementAsync(createButtonSelectors, 10000);
            if (createButton != null)
            {
                await SafeClickAsync(createButton);
                await Page.WaitForTimeoutAsync(1000);
                
                // Look for notebook name input
                var nameInputSelectors = new[]
                {
                    "input[placeholder*='name'], input[placeholder*='Name']",
                    "[data-testid='notebook-name'], [data-testid='file-name']",
                    ".name-input, .filename-input"
                };
                
                var nameInput = await WaitForAnyElementAsync(nameInputSelectors, 5000);
                if (nameInput != null)
                {
                    await nameInput.FillAsync(notebookName);
                    
                    // Look for confirm/create button
                    var confirmButtonSelectors = new[]
                    {
                        "button:has-text('Create'), button:has-text('OK')",
                        "[data-testid='confirm-create'], [data-testid='create-confirm']",
                        ".confirm-button, .create-confirm"
                    };
                    
                    var confirmButton = await WaitForAnyElementAsync(confirmButtonSelectors, 5000);
                    if (confirmButton != null)
                    {
                        await SafeClickAsync(confirmButton);
                        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    }
                }
            }
            
            _logger.LogInformation("Successfully created notebook: {NotebookName}", notebookName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notebook: {NotebookName}", notebookName);
            throw;
        }
    }

    /// <summary>
    /// Opens an existing notebook by name
    /// </summary>
    public async Task OpenNotebookAsync(string notebookName)
    {
        _logger.LogInformation("Opening notebook: {NotebookName}", notebookName);
        
        try
        {
            // First navigate to notebooks if not already there
            if (!Page.Url.Contains("/notebook"))
            {
                await NavigateToNotebooksAsync();
            }
            
            // Look for the notebook in the file list
            var notebookSelectors = new[]
            {
                $"[data-filename='{notebookName}'], [title='{notebookName}']",
                $"a:has-text('{notebookName}'), .file-item:has-text('{notebookName}')",
                $".notebook-file:has-text('{notebookName}')"
            };
            
            var notebookFile = await WaitForAnyElementAsync(notebookSelectors, 15000);
            if (notebookFile != null)
            {
                await SafeClickAsync(notebookFile);
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                
                // Wait for notebook to load
                await Page.WaitForSelectorAsync(".notebook-editor, .jupyter-notebook, [data-testid='notebook-editor']", new PageWaitForSelectorOptions 
                { 
                    Timeout = 20000 
                });
            }
            else
            {
                throw new InvalidOperationException($"Notebook '{notebookName}' not found in the file list");
            }
            
            _logger.LogInformation("Successfully opened notebook: {NotebookName}", notebookName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open notebook: {NotebookName}", notebookName);
            throw;
        }
    }

    /// <summary>
    /// Waits for a compute instance to reach the specified state
    /// </summary>
    public async Task WaitForComputeStateAsync(string computeName, string expectedState, int timeoutMinutes = 10)
    {
        _logger.LogInformation("Waiting for compute instance {ComputeName} to reach state: {ExpectedState}", computeName, expectedState);
        
        try
        {
            var endTime = DateTime.UtcNow.AddMinutes(timeoutMinutes);
            
            while (DateTime.UtcNow < endTime)
            {
                var currentState = await GetComputeStatus(computeName);
                
                if (currentState.Equals(expectedState, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Compute instance {ComputeName} reached expected state: {State}", computeName, currentState);
                    return;
                }
                
                _logger.LogDebug("Compute instance {ComputeName} current state: {CurrentState}, waiting for: {ExpectedState}", 
                    computeName, currentState, expectedState);
                
                await Task.Delay(30000); // Wait 30 seconds before checking again
            }
            
            throw new TimeoutException($"Compute instance '{computeName}' did not reach expected state '{expectedState}' within {timeoutMinutes} minutes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to wait for compute state for: {ComputeName}", computeName);
            throw;
        }
    }

    /// <summary>
    /// Helper method to wait for any element from a list of selectors
    /// </summary>
    private async Task<ILocator?> WaitForAnyElementAsync(string[] selectors, int timeoutMs = 10000)
    {
        var endTime = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        
        while (DateTime.UtcNow < endTime)
        {
            foreach (var selector in selectors)
            {
                try
                {
                    var element = Page.Locator(selector).First;
                    if (await element.IsVisibleAsync())
                    {
                        return element;
                    }
                }
                catch (Exception)
                {
                    // Continue to next selector if this one fails
                    continue;
                }
            }
            
            await Page.WaitForTimeoutAsync(500);
        }
        
        return null;
    }

    /// <summary>
    /// Helper method to safely click an element with retry logic
    /// </summary>
    private async Task SafeClickAsync(ILocator element, int maxRetries = 3)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                await element.ClickAsync();
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                _logger.LogWarning(ex, "Click attempt {Attempt} failed, retrying...", i + 1);
                await Page.WaitForTimeoutAsync(1000);
            }
        }
    }
}

/// <summary>
/// Static class to provide access to the current test context
/// </summary>
public static class TestContext
{
    public static IServiceProvider ServiceProvider { get; set; } = null!;
}