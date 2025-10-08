using Microsoft.Playwright;
using NUnit.Framework;
using Reqnroll;
using Serilog;
using PlaywrightFramework.Utils;
using Allure.Net.Commons;

namespace PlaywrightFramework.Hooks
{
    [Binding]
    public class TestHooks
    {
        private static ILogger? _logger;
        private static bool _isInitialized = false;

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            if (!_isInitialized)
            {
                // Initialize logging
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("../Reports/logs/bdd-tests.log", rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                _logger = Log.ForContext<TestHooks>();
                _logger.Information("Starting BDD test run");

                // Install Playwright browsers if needed
                Microsoft.Playwright.Program.Main(new[] { "install" });

                _isInitialized = true;
            }
        }

        [AfterTestRun]
        public static void AfterTestRun()
        {
            _logger?.Information("Completing BDD test run");
            Log.CloseAndFlush();
        }

        [BeforeFeature]
        public static void BeforeFeature(FeatureContext featureContext)
        {
            _logger?.Information("Starting feature: {FeatureTitle}", featureContext.FeatureInfo.Title);
        }

        [AfterFeature]
        public static void AfterFeature(FeatureContext featureContext)
        {
            _logger?.Information("Completing feature: {FeatureTitle}", featureContext.FeatureInfo.Title);
        }

        [BeforeScenario]
        public void BeforeScenario(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            _logger?.Information("Starting scenario: {ScenarioTitle} in feature: {FeatureTitle}", 
                scenarioContext.ScenarioInfo.Title, 
                featureContext.FeatureInfo.Title);

            // Set scenario start time for performance tracking
            scenarioContext.Set(DateTime.UtcNow, "ScenarioStartTime");

            // Add Allure labels and metadata
            AllureApi.AddLabel("feature", featureContext.FeatureInfo.Title);
            AllureApi.AddLabel("story", scenarioContext.ScenarioInfo.Title);
            
            // Add tags as labels
            foreach (var tag in scenarioContext.ScenarioInfo.Tags)
            {
                AllureApi.AddLabel("tag", tag);
            }
        }

        [AfterScenario]
        public async Task AfterScenario(ScenarioContext scenarioContext)
        {
            var startTime = scenarioContext.Get<DateTime>("ScenarioStartTime");
            var duration = DateTime.UtcNow - startTime;

            _logger?.Information("Completing scenario: {ScenarioTitle} (Duration: {Duration:F2}s, Status: {Status})", 
                scenarioContext.ScenarioInfo.Title,
                duration.TotalSeconds,
                scenarioContext.ScenarioExecutionStatus);

            // Take screenshot on failure
            if (scenarioContext.TestError != null)
            {
                await TakeScreenshotOnFailure(scenarioContext);
            }
        }

        [AfterStep]
        public async Task AfterStep(ScenarioContext scenarioContext)
        {
            if (scenarioContext.TestError != null)
            {
                _logger?.Error(scenarioContext.TestError, "Step failed: {StepText}", 
                    scenarioContext.StepContext.StepInfo.Text);

                await TakeScreenshotOnFailure(scenarioContext);
            }
        }

        private static async Task TakeScreenshotOnFailure(ScenarioContext scenarioContext)
        {
            try
            {
                if (scenarioContext.TryGetValue("Page", out IPage page))
                {
                    var screenshotPath = Path.Combine(
                        "../Reports/screenshots",
                        $"{scenarioContext.ScenarioInfo.Title.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png");

                    Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);

                    await page.ScreenshotAsync(new PageScreenshotOptions
                    {
                        Path = screenshotPath,
                        FullPage = true
                    });

                    _logger?.Information("Screenshot saved: {ScreenshotPath}", screenshotPath);

                    // Attach screenshot to Allure report
                    if (File.Exists(screenshotPath))
                    {
                        AllureApi.AddAttachment("Screenshot on Failure", "image/png", screenshotPath);
                    }

                    // Attach screenshot to test context for reporting
                    TestContext.AddTestAttachment(screenshotPath, "Screenshot on failure");
                }
            }
            catch (Exception ex)
            {
                _logger?.Warning(ex, "Failed to take screenshot on failure");
            }
        }
    }
}