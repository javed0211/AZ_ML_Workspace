using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Questions;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Tasks;
using AzureMLWorkspace.Tests.Framework.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace AzureMLWorkspace.Tests.Tests;

[TestFixture]
public class VSCodeDesktopIntegrationTest
{
    private IServiceProvider _serviceProvider = null!;
    private ILogger<VSCodeDesktopIntegrationTest> _logger = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Setup service provider for testing
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddScoped<VSCodeDesktopHelper>();
        
        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<VSCodeDesktopIntegrationTest>>();
        
        // Set the test context
        TestContext.ServiceProvider = _serviceProvider;
    }

    [Test]
    [Category("Integration")]
    [Category("VSCodeDesktop")]
    public async Task VSCodeDesktop_LaunchAndCheckInteractivity_ShouldSucceed()
    {
        // Arrange
        var actor = Actor.Named("Test User", _logger);
        var vsCodeHelper = _serviceProvider.GetRequiredService<VSCodeDesktopHelper>();
        var vsCodeAbility = UseVSCodeDesktop.With(vsCodeHelper);
        actor.Can(vsCodeAbility);

        try
        {
            // Act - Launch VS Code
            _logger.LogInformation("Starting VS Code Desktop test...");
            await actor.AttemptsTo(StartVSCodeDesktop.Now());

            // Wait a moment for VS Code to fully load
            await Task.Delay(5000);

            // Assert - Check if VS Code is interactive
            var isInteractive = await actor.AsksFor(VSCodeInteractivity.IsWorking());
            
            Assert.That(isInteractive, Is.True, "VS Code should be interactive");
            _logger.LogInformation("VS Code Desktop test completed successfully");
        }
        finally
        {
            // Cleanup
            try
            {
                await vsCodeAbility.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during VS Code cleanup");
            }
        }
    }

    [Test]
    [Category("Integration")]
    [Category("VSCodeDesktop")]
    public async Task VSCodeDesktop_CheckApplicationLinks_ShouldReturnResults()
    {
        // Arrange
        var actor = Actor.Named("Test User", _logger);
        var vsCodeHelper = _serviceProvider.GetRequiredService<VSCodeDesktopHelper>();
        var vsCodeAbility = UseVSCodeDesktop.With(vsCodeHelper);
        actor.Can(vsCodeAbility);

        try
        {
            // Act - Launch VS Code
            await actor.AttemptsTo(StartVSCodeDesktop.Now());
            
            // Wait for VS Code to load
            await Task.Delay(3000);

            // Check application links
            var result = await vsCodeAbility.CheckApplicationLinksAsync();
            
            // Assert
            Assert.That(result.Success, Is.True, "Application links check should succeed");
            Assert.That(result.Data, Is.Not.Null, "Application links data should not be null");
            
            _logger.LogInformation("Application links check result: {Result}", result.Message);
        }
        finally
        {
            // Cleanup
            try
            {
                await vsCodeAbility.CloseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during VS Code cleanup");
            }
        }
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _serviceProvider?.Dispose();
    }
}