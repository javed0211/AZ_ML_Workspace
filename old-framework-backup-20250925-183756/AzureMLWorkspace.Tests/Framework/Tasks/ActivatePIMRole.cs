using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using AzureMLWorkspace.Tests.Framework.Screenplay;
using AzureMLWorkspace.Tests.Framework.Abilities;
using Microsoft.Extensions.DependencyInjection;
using AzureMLWorkspace.Tests.Framework.Tasks.Generated;

namespace AzureMLWorkspace.Tests.Framework.Tasks;

/// <summary>
/// Task to activate a Privileged Identity Management (PIM) role through the Azure Portal UI
/// </summary>
public class ActivatePIMRole : ITask
{
    private readonly string _roleName;
    private readonly string _justification;
    private readonly TimeSpan _duration;
    private readonly ILogger<ActivatePIMRole> _logger;

    public string Name => $"Activate PIM Role: {_roleName}";

    public ActivatePIMRole(string roleName, string justification, TimeSpan duration, ILogger<ActivatePIMRole> logger)
    {
        _roleName = roleName ?? throw new ArgumentNullException(nameof(roleName));
        _justification = justification ?? throw new ArgumentNullException(nameof(justification));
        _duration = duration;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new ActivatePIMRole task for a specific role
    /// </summary>
    /// <param name="roleName">Name of the role to activate (e.g., "Data Scientist")</param>
    /// <returns>ActivatePIMRole task builder</returns>
    public static ActivatePIMRoleBuilder ForRole(string roleName)
    {
        return new ActivatePIMRoleBuilder(roleName);
    }

    /// <summary>
    /// Creates a new ActivatePIMRole task for the Data Scientist role
    /// </summary>
    /// <returns>ActivatePIMRole task builder</returns>
    public static ActivatePIMRoleBuilder ForDataScientistRole(string role)
    {
        return new ActivatePIMRoleBuilder(role);
    }

    /// <summary>
    /// Executes the PIM role activation through the Azure Portal UI
    /// </summary>
    /// <param name="actor">The actor performing the task</param>
    public async Task PerformAs(IActor actor)
    {
        _logger.LogInformation("Starting PIM role activation for role: {RoleName}", _roleName);

        try
        {
            // First, ensure we're logged into Azure Portal
            _logger.LogInformation("Ensuring authentication to Azure Portal");
            await actor.AttemptsTo(LoginToAzurePortal.FromConfiguration());

            // Get the browser ability
            var browserAbility = actor.Using<BrowseTheWeb>();
            var page = browserAbility.Page;

            // Navigate to the PIM activation page
            _logger.LogInformation("Navigating to Azure Portal PIM activation page");
            await page.GotoAsync("https://portal.azure.com/?feature.msaljs=true#view/Microsoft_Azure_PIMCommon/ActivationMenuBlade/~/aadgroup/provider/aadgroup");

            // Wait for the page to load
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // **OPTIMIZATION: Reduced timeout and improved selectors for faster PIM activation**
            _logger.LogInformation("Waiting for PIM roles list to load (optimized)");
            
            // Use multiple selectors to catch the page faster
            var gridSelectors = new[]
            {
                "//table[@class='azc-grid-full']/tbody[@class='azc-grid-groupdata']",
                ".azc-grid-full tbody",
                "[data-automation-id*='grid'] tbody",
                "table tbody"
            };
            
            var _ = await WaitForAnyElementAsync(page, gridSelectors, 15000); // Reduced from 30s to 15s

            // Check active assignments first (faster check)
            await page.ClickAsync("//span[contains(text(),'Active assignments')]/parent::span/parent::div[@role='tab']");
            await page.WaitForTimeoutAsync(1000); // Brief wait for tab content to load
            
            var activeRoles = await page.QuerySelectorAllAsync("//td[@aria-colindex='2']//div[text()='PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST']");
            if (activeRoles.Count > 0)
            {
                _logger.LogInformation("{RoleName} is already active - skipping activation", _roleName);
                return; // Early return to save time
            }

            // Switch to eligible assignments tab
            await page.ClickAsync("//span[contains(text(),'Eligible assignments')]/parent::span/parent::div[@role='tab']");
            await page.WaitForTimeoutAsync(1000);

            // Find and click on the specific role with improved selectors
            _logger.LogInformation("Looking for eligible role: {RoleName}", _roleName);
            var roleSelectors = new[]
            {
                $"//div[contains(text(),'{_roleName}')]",
                $"//td[contains(text(),'{_roleName}')]",
                $"//*[contains(text(),'{_roleName}')]"
            };

            var roleElement = await WaitForAnyElementAsync(page, roleSelectors, 10000); // Reduced from 15s to 10s
            if (roleElement == null)
            {
                throw new InvalidOperationException($"Could not find PIM role: {_roleName}");
            }

            // Click on the role to select it
            await roleElement.ClickAsync();
            _logger.LogInformation("Selected PIM role: {RoleName}", _roleName);

            // Click the Activate button with improved selector
            var activateSelectors = new[]
            {
                $"//div[contains(text(),'{_roleName}')]/../../../following-sibling::td//a",
                "//a[contains(text(),'Activate')]",
                "//button[contains(text(),'Activate')]",
                "[data-automation-id*='activate']"
            };
            
            var activateButton = await WaitForAnyElementAsync(page, activateSelectors, 8000);
            await activateButton.ClickAsync();
            _logger.LogInformation("Clicked Activate button");

            // Wait for the activation form with multiple selectors
            var formSelectors = new[]
            {
                "//div[@aria-labelledby='fxsContextPaneTitle']",
                ".fxs-contextpane-content",
                "[aria-labelledby*='contextpane']",
                ".activation-form"
            };
            
            await WaitForAnyElementAsync(page, formSelectors, 8000); // Reduced from 10s to 8s

            // Fill in the justification with improved selector
            _logger.LogInformation("Filling in justification: {Justification}", _justification);
            var justificationSelectors = new[]
            {
                "//label[contains(text(),'Reason')]//../..//textarea",
                "textarea[aria-label*='reason']",
                "textarea[placeholder*='reason']",
                "textarea"
            };
            
            var justificationField = await WaitForAnyElementAsync(page, justificationSelectors, 5000);
            await justificationField.FillAsync(_justification);

            // Set the duration if different from default
            if (_duration != TimeSpan.FromHours(8)) // Assuming 8 hours is default
            {
                _logger.LogInformation("Setting activation duration to: {Duration}", _duration);
                await SetActivationDuration(page, _duration);
            }

            // Submit the activation request with improved selector
            _logger.LogInformation("Submitting PIM role activation request");
            var submitSelectors = new[]
            {
                "//div[contains(.,'Activate') and @role='button']",
                "//button[contains(text(),'Activate')]",
                "[data-automation-id*='activate-button']",
                ".primary-button"
            };
            
            var submitButton = await WaitForAnyElementAsync(page, submitSelectors, 5000);
            await submitButton.ClickAsync();

            // Wait for confirmation with reduced timeout and multiple selectors
            _logger.LogInformation("Waiting for activation confirmation (optimized)");
            var confirmationSelectors = new[]
            {
                "//span[contains(text(),'Your active assignments have changed')]",
                "//span[contains(text(),'successfully activated')]",
                ".success-message",
                "[data-automation-id*='success']"
            };
            
            await WaitForAnyElementAsync(page, confirmationSelectors, 30000); // Reduced from 60s to 30s

            // Navigate back to active assignments
            await page.ClickAsync("//span[contains(text(),'Active assignments')]/parent::span/parent::div[@role='tab']");
            _logger.LogInformation("PIM role activation completed successfully for role: {RoleName} (optimized)", _roleName);    
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout occurred during PIM role activation for role: {RoleName}", _roleName);
            throw new InvalidOperationException($"Timeout during PIM role activation for {_roleName}. The page may have taken too long to load or the role may not be available.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate PIM role: {RoleName}", _roleName);
            throw new InvalidOperationException($"Failed to activate PIM role: {_roleName}", ex);
        }
    }



    /// <summary>
    /// Waits for any of the provided selectors to become available (optimization helper)
    /// </summary>
    private async Task<IElementHandle?> WaitForAnyElementAsync(IPage page, string[] selectors, int timeoutMs)
    {
        var tasks = selectors.Select(async selector =>
        {
            try
            {
                var element = await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
                {
                    Timeout = timeoutMs
                });
                return element;
            }
            catch (TimeoutException)
            {
                return null;
            }
        });

        var completedTask = await Task.WhenAny(tasks);
        var result = await completedTask;
        
        if (result == null)
        {
            throw new TimeoutException($"None of the selectors were found within {timeoutMs}ms: {string.Join(", ", selectors)}");
        }
        
        return result;
    }



    /// <summary>
    /// Sets the activation duration in the form
    /// </summary>
    private async Task SetActivationDuration(IPage page, TimeSpan duration)
    {
        try
        {
            // Look for duration dropdown or input field
            var durationSelector = "select[aria-label*='duration'], select[data-automation-id*='duration'], #duration";
            
            if (await page.Locator(durationSelector).CountAsync() > 0)
            {
                // If it's a dropdown, select the appropriate value
                var hours = (int)duration.TotalHours;
                await page.SelectOptionAsync(durationSelector, hours.ToString());
            }
            else
            {
                // Look for hour/minute input fields
                var hourInput = "input[aria-label*='hour'], input[data-automation-id*='hour']";
                var minuteInput = "input[aria-label*='minute'], input[data-automation-id*='minute']";
                
                if (await page.Locator(hourInput).CountAsync() > 0)
                {
                    await page.FillAsync(hourInput, ((int)duration.TotalHours).ToString());
                }
                
                if (await page.Locator(minuteInput).CountAsync() > 0)
                {
                    await page.FillAsync(minuteInput, duration.Minutes.ToString());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not set custom duration, using default duration");
        }
    }
}

/// <summary>
/// Builder for ActivatePIMRole task
/// </summary>
public class ActivatePIMRoleBuilder
{
    private readonly string _roleName;
    private string _justification = "Automated test execution";
    private TimeSpan _duration = TimeSpan.FromHours(8);

    /// <summary>
    /// Gets the name of the PIM role activation task
    /// </summary>
    public string Name => $"Activate PIM Role: {_roleName}";

    internal ActivatePIMRoleBuilder(string roleName)
    {
        _roleName = roleName;
    }

    /// <summary>
    /// Sets the justification for the PIM role activation
    /// </summary>
    /// <param name="justification">Justification text</param>
    /// <returns>Builder instance</returns>
    public ActivatePIMRoleBuilder WithJustification(string justification)
    {
        _justification = justification ?? throw new ArgumentNullException(nameof(justification));
        return this;
    }

    /// <summary>
    /// Sets the duration for the PIM role activation
    /// </summary>
    /// <param name="duration">Activation duration</param>
    /// <returns>Builder instance</returns>
    public ActivatePIMRoleBuilder ForDuration(TimeSpan duration)
    {
        _duration = duration;
        return this;
    }

    /// <summary>
    /// Sets the duration for the PIM role activation in hours
    /// </summary>
    /// <param name="hours">Activation duration in hours</param>
    /// <returns>Builder instance</returns>
    public ActivatePIMRoleBuilder ForDuration(int hours)
    {
        _duration = TimeSpan.FromHours(hours);
        return this;
    }

    /// <summary>
    /// Builds the ActivatePIMRole task
    /// </summary>
    /// <returns>ActivatePIMRole task</returns>
    public ActivatePIMRole Build()
    {
        var logger = AzureMLWorkspace.Tests.Framework.Abilities.TestContext.ServiceProvider.GetRequiredService<ILogger<ActivatePIMRole>>();
        return new ActivatePIMRole(_roleName, _justification, _duration, logger);
    }

    /// <summary>
    /// Implicit conversion to ActivatePIMRole task
    /// </summary>
    public static implicit operator ActivatePIMRole(ActivatePIMRoleBuilder builder)
    {
        return builder.Build();
    }
}