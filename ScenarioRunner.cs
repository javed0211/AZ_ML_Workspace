using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Questions;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Tasks;
using AzureMLWorkspace.Tests.Framework.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

namespace AzureMLWorkspace;

/// <summary>
/// Console application to manually execute the Azure ML Workspace with VS Code Desktop Integration scenario
/// </summary>
public class ScenarioRunner
{
    private static IServiceProvider? _serviceProvider;
    private static ILogger<ScenarioRunner>? _logger;

    public static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== Azure ML Workspace with VS Code Desktop Integration Scenario ===");
            Console.WriteLine();

            // Initialize services
            InitializeServices();
            _logger = _serviceProvider!.GetRequiredService<ILogger<ScenarioRunner>>();

            // Set up the test context
            TestContext.ServiceProvider = _serviceProvider!;

            _logger.LogInformation("Starting Azure ML Workspace with VS Code Desktop Integration scenario...");

            // Execute the scenario
            await ExecuteScenario();

            _logger.LogInformation("Scenario completed successfully!");
            Console.WriteLine();
            Console.WriteLine("✅ Scenario completed successfully!");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Scenario execution failed");
            Console.WriteLine();
            Console.WriteLine($"❌ Scenario failed: {ex.Message}");
            Environment.Exit(1);
        }
        finally
        {
            // Cleanup
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            Log.CloseAndFlush();
        }
    }

    private static void InitializeServices()
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/scenario-execution-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Build service collection
        var services = new ServiceCollection();
        
        // Register configuration
        services.AddSingleton<IConfiguration>(configuration);

        // Register logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(Log.Logger);
        });

        // Register framework services
        services.AddTransient<Actor>();
        services.AddSingleton<ILoggerFactory, SerilogLoggerFactory>();

        // Register framework utilities
        services.AddTransient<VSCodeDesktopHelper>();

        _serviceProvider = services.BuildServiceProvider();
    }

    private static async Task ExecuteScenario()
    {
        _logger!.LogInformation("Creating actor 'Javed'...");
        
        // Given I am a data scientist named "Javed"
        var actorLogger = _serviceProvider!.GetRequiredService<ILogger<Actor>>();
        var actor = Actor.Named("Javed", actorLogger);
        
        _logger.LogInformation("Setting up browser and Azure ML abilities...");
        
        // Create browser ability first
        var browserLogger = _serviceProvider!.GetRequiredService<ILogger<BrowseTheWeb>>();
        var browserAbility = BrowseTheWeb.Maximized(browserLogger);
        
        // And I have Contributor access to Azure ML
        var azureMLAbility = UseAzureML.WithRole("Contributor", browserAbility);
        
        // Add abilities to actor
        actor.Can(browserAbility);
        actor.Can(azureMLAbility);
        
        // Initialize abilities
        await browserAbility.InitializeAsync();
        await azureMLAbility.InitializeAsync();

        _logger.LogInformation("Executing scenario steps...");

        try
        {
            // When I go to workspace "ml-workspace"
            _logger.LogInformation("Step: Going to workspace 'ml-workspace'...");
            await actor.AttemptsTo(NavigateToWorkspace.Named("ml-workspace"));

            // And If login required I login as user "Javed Khan"
            _logger.LogInformation("Step: Logging in as user 'Javed Khan' if required...");
            await actor.AttemptsTo(LoginAsUser.Named("Javed Khan"));

            // And I select Workspace "CTO-workspace"
            _logger.LogInformation("Step: Selecting workspace 'CTO-workspace'...");
            await actor.AttemptsTo(SelectWorkspace.Named("CTO-workspace"));

            // And I choose compute option
            _logger.LogInformation("Step: Choosing compute option...");
            await actor.AttemptsTo(ChooseComputeOption.Now());

            // And I open compute "com-jk"
            _logger.LogInformation("Step: Opening compute 'com-jk'...");
            await actor.AttemptsTo(OpenCompute.Named("com-jk"));

            // And If compute is not running, I start compute
            _logger.LogInformation("Step: Starting compute if not running...");
            await actor.AttemptsTo(StartComputeIfNotRunning.Named("com-jk"));

            // Then I check if application link are enabled
            _logger.LogInformation("Step: Checking if application links are enabled...");
            var linksEnabled = await actor.AsksFor(ApplicationLinksEnabled.InCurrentWorkspace());
            _logger.LogInformation("Application links enabled: {LinksEnabled}", linksEnabled);

            // When I start VS code Desktop
            _logger.LogInformation("Step: Starting VS Code Desktop...");
            await actor.AttemptsTo(StartVSCodeDesktop.Now());

            // Then I check if I am able to interact with VS code
            _logger.LogInformation("Step: Checking VS Code interactivity...");
            var isInteractive = await actor.AsksFor(VSCodeInteractivity.IsWorking());
            _logger.LogInformation("VS Code is interactive: {IsInteractive}", isInteractive);

            // Verify results
            if (linksEnabled && isInteractive)
            {
                _logger.LogInformation("✅ All scenario steps completed successfully!");
                Console.WriteLine("✅ Application links are enabled");
                Console.WriteLine("✅ VS Code Desktop is interactive");
            }
            else
            {
                _logger.LogWarning("⚠️ Some scenario steps had issues:");
                if (!linksEnabled) Console.WriteLine("⚠️ Application links are not enabled");
                if (!isInteractive) Console.WriteLine("⚠️ VS Code Desktop is not interactive");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scenario execution");
            throw;
        }
        finally
        {
            // Cleanup abilities
            _logger.LogInformation("Cleaning up abilities...");
            await actor.DisposeAsync();
        }
    }
}