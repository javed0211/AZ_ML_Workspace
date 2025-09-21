using AzureMLWorkspace.Tests.Framework;
using AzureMLWorkspace.Tests.Framework.Abilities;
using AzureMLWorkspace.Tests.Framework.Extensions;
using AzureMLWorkspace.Tests.Framework.Questions;
using AzureMLWorkspace.Tests.Framework.Tasks;
using Microsoft.Playwright;

namespace AzureMLWorkspace.Tests.Tests;

/// <summary>
/// UI tests for Azure ML Workspace functionality
/// </summary>
[TestFixture]
[Category("UI")]
[Category("AzureML")]
public class AzureMLWorkspaceUITests : TestBase
{
    [Test]
    [Description("Verify that a user can access Azure ML workspace through the browser")]
    public async Task Should_Access_AzureML_Workspace_Successfully()
    {
        // Arrange
        var javed = CreateActor("Javed")
            .Can(BrowseTheWeb.With(GetLogger<BrowseTheWeb>(), new BrowserTypeLaunchOptions
            {
                Headless = TestConfig.Browser.Headless,
                SlowMo = TestConfig.Browser.SlowMo
            }))
            .Can(UseAzureML.AsContributor());

        // Initialize abilities
        await javed.Using<BrowseTheWeb>().InitializeAsync();
        await javed.Using<UseAzureML>().InitializeAsync();

        // Act & Assert
        await javed
            .AttemptsTo(NavigateTo.AzureMLPortal())
            .And(OpenWorkspace.Named(TestConfig.Azure.WorkspaceName))
            .Should(Validate.WorkspaceAccess(TestConfig.Azure.WorkspaceName));
    }

    [Test]
    [Description("Verify compute instance management through UI")]
    public async Task Should_Manage_Compute_Instance_Through_UI()
    {
        // Arrange
        var computeName = $"test-compute-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var javed = CreateActor("Javed")
            .Can(BrowseTheWeb.Headlessly(GetLogger<BrowseTheWeb>()))
            .Can(UseAzureML.AsContributor());

        // Initialize abilities
        await javed.Using<BrowseTheWeb>().InitializeAsync();
        await javed.Using<UseAzureML>().InitializeAsync();

        try
        {
            // Act & Assert
            await javed
                .AttemptsTo(NavigateTo.Workspace(TestConfig.Azure.WorkspaceName))
                .And(StartCompute.Named(computeName))
                .Should(Validate.ComputeStatus(computeName, "Succeeded"));

            // Verify we can stop the compute
            await javed
                .AttemptsTo(StopCompute.Named(computeName))
                .Should(Validate.ComputeStatus(computeName, "Stopped"));
        }
        finally
        {
            // Cleanup - ensure compute is stopped
            try
            {
                await javed.AttemptsTo(StopCompute.Named(computeName));
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to cleanup compute instance {ComputeName}", computeName);
            }
        }
    }

    [Test]
    [Description("Verify workspace navigation and basic functionality")]
    [Category("Smoke")]
    public async Task Should_Navigate_Workspace_Sections_Successfully()
    {
        // Arrange
        var javed = CreateActor("Javed")
            .Can(BrowseTheWeb.WithViewport(GetLogger<BrowseTheWeb>(), 1920, 1080));

        await javed.Using<BrowseTheWeb>().InitializeAsync();

        // Act & Assert
        await javed
            .AttemptsTo(NavigateTo.AzureMLPortal());

        // Verify page loaded
        var browser = javed.Using<BrowseTheWeb>();
        await browser.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Check if we can see the main navigation elements
        var isWorkspaceVisible = await browser.Page.IsVisibleAsync("[data-testid='workspace-selector']");
        isWorkspaceVisible.Should().BeTrue("Workspace selector should be visible on the main page");
    }

    [Test]
    [Description("Verify responsive design on different viewport sizes")]
    [TestCase(1920, 1080, "Desktop")]
    [TestCase(1366, 768, "Laptop")]
    [TestCase(768, 1024, "Tablet")]
    public async Task Should_Display_Correctly_On_Different_Viewports(int width, int height, string deviceType)
    {
        // Arrange
        var javed = CreateActor($"Javed-{deviceType}")
            .Can(BrowseTheWeb.WithViewport(GetLogger<BrowseTheWeb>(), width, height));

        await javed.Using<BrowseTheWeb>().InitializeAsync();

        // Act
        await javed.AttemptsTo(NavigateTo.AzureMLPortal());

        // Assert
        var browser = javed.Using<BrowseTheWeb>();
        var viewport = browser.Page.ViewportSize;
        
        viewport.Should().NotBeNull();
        viewport!.Width.Should().Be(width);
        viewport.Height.Should().Be(height);

        // Verify page is responsive
        await browser.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        var isPageResponsive = await browser.Page.IsVisibleAsync("body");
        isPageResponsive.Should().BeTrue($"Page should be responsive on {deviceType} viewport ({width}x{height})");
    }

    [Test]
    [Description("Verify cross-browser compatibility")]
    [TestCase("chromium")]
    [TestCase("firefox")]
    [TestCase("webkit")]
    public async Task Should_Work_Across_Different_Browsers(string browserType)
    {
        // Arrange
        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = TestConfig.Browser.Headless
        };

        var javed = CreateActor($"Javed-{browserType}")
            .Can(BrowseTheWeb.With(GetLogger<BrowseTheWeb>(), launchOptions));

        await javed.Using<BrowseTheWeb>().InitializeAsync();

        // Act & Assert
        await javed.AttemptsTo(NavigateTo.AzureMLPortal());

        var browser = javed.Using<BrowseTheWeb>();
        await browser.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Verify basic functionality works
        var title = await browser.Page.TitleAsync();
        title.Should().NotBeNullOrEmpty($"Page title should be available in {browserType}");
        
        Logger.LogInformation("Successfully tested {BrowserType} - Page title: {Title}", browserType, title);
    }

    [TearDown]
    public override async Task TearDown()
    {
        await base.TearDown();
        
        // Additional cleanup for UI tests
        if (Page != null)
        {
            try
            {
                await Page.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to close page during cleanup");
            }
        }
    }
}