using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.MachineLearning;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureMLWorkspace.Tests.Framework.Abilities;

/// <summary>
/// Ability to interact with Azure Machine Learning services
/// </summary>
public class UseAzureML : IAbility
{
    private readonly ILogger<UseAzureML> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _role;
    
    private ArmClient? _armClient;
    private MachineLearningWorkspaceResource? _workspace;
    private TokenCredential? _credential;

    public string Name => $"Use Azure ML with {_role} role";
    public ArmClient ArmClient => _armClient ?? throw new InvalidOperationException("Azure ML client not initialized");
    public MachineLearningWorkspaceResource Workspace => _workspace ?? throw new InvalidOperationException("Workspace not initialized");
    public TokenCredential Credential => _credential ?? throw new InvalidOperationException("Credential not initialized");

    private UseAzureML(ILogger<UseAzureML> logger, IConfiguration configuration, string role)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _role = role ?? throw new ArgumentNullException(nameof(role));
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing Azure ML client with {Role} role", _role);

        try
        {
            // Initialize credential based on environment
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

            // Initialize ARM client
            _armClient = new ArmClient(_credential);

            // Get workspace
            var subscriptionId = _configuration["Azure:SubscriptionId"] ?? 
                throw new InvalidOperationException("Azure:SubscriptionId not configured");
            var resourceGroupName = _configuration["Azure:ResourceGroup"] ?? 
                throw new InvalidOperationException("Azure:ResourceGroup not configured");
            var workspaceName = _configuration["Azure:WorkspaceName"] ?? 
                throw new InvalidOperationException("Azure:WorkspaceName not configured");

            var subscription = await _armClient.GetDefaultSubscriptionAsync();
            var resourceGroup = await subscription.GetResourceGroupAsync(resourceGroupName);
            _workspace = await resourceGroup.Value.GetMachineLearningWorkspaceAsync(workspaceName);

            _logger.LogInformation("Azure ML client initialized successfully for workspace {WorkspaceName}", workspaceName);
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
        
        _logger.LogInformation("Azure ML cleanup completed");
        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates a new UseAzureML ability with the specified role
    /// </summary>
    public static UseAzureML WithRole(string role)
    {
        return new UseAzureML(
            Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<ILogger<UseAzureML>>(AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider),
            Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService<IConfiguration>(AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider),
            role);
    }

    /// <summary>
    /// Creates a new UseAzureML ability with Contributor role
    /// </summary>
    public static UseAzureML AsContributor()
    {
        return WithRole("Contributor");
    }

    /// <summary>
    /// Creates a new UseAzureML ability with Reader role
    /// </summary>
    public static UseAzureML AsReader()
    {
        return WithRole("Reader");
    }

    /// <summary>
    /// Creates a new UseAzureML ability with Owner role
    /// </summary>
    public static UseAzureML AsOwner()
    {
        return WithRole("Owner");
    }

    /// <summary>
    /// Creates a new UseAzureML ability with custom role
    /// </summary>
    public static UseAzureML WithCustomRole(string customRole)
    {
        return WithRole(customRole);
    }

    /// <summary>
    /// Sets PIM (Privileged Identity Management) role
    /// </summary>
    public async Task SetPIMRole(string roleName)
    {
        _logger.LogInformation("Setting PIM role: {RoleName}", roleName);
        
        // This would integrate with Microsoft Graph API to activate PIM roles
        // Implementation depends on your PIM setup
        try
        {
            // TODO: Implement PIM role activation using Microsoft Graph
            // This is a placeholder for the actual PIM integration
            await Task.Delay(1000); // Simulate PIM activation time
            
            _logger.LogInformation("PIM role {RoleName} activated successfully", roleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate PIM role {RoleName}", roleName);
            throw;
        }
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
}

/// <summary>
/// Static class to provide access to the current test context
/// </summary>
public static class TestContext
{
    public static IServiceProvider ServiceProvider { get; set; } = null!;
}