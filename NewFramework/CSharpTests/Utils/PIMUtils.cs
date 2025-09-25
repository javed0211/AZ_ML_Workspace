using Microsoft.Playwright;
using PlaywrightFramework.Utils;

namespace PlaywrightFramework.Utils
{
    /// <summary>
    /// Utility class for Privileged Identity Management (PIM) role activation through Azure Portal UI
    /// </summary>
    public class PIMUtils
    {
        private readonly IPage _page;
        private readonly Logger _logger;
        private readonly ConfigManager _config;

        public PIMUtils(IPage page, Logger logger)
        {
            _page = page ?? throw new ArgumentNullException(nameof(page));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = ConfigManager.Instance;
        }

        /// <summary>
        /// Activates a PIM role through the Azure Portal UI
        /// </summary>
        /// <param name="roleName">Name of the role to activate (e.g., "PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST")</param>
        /// <param name="justification">Justification for role activation</param>
        /// <param name="durationHours">Duration in hours (default: 8)</param>
        public async Task ActivatePIMRoleAsync(string roleName, string justification = "Automated test execution", int durationHours = 8)
        {
            _logger.LogInfo($"🔐 Starting PIM role activation for role: {roleName}");

            try
            {
                // Navigate to the PIM activation page
                _logger.LogStep("Navigate to Azure Portal PIM activation page");
                await _page.GotoAsync("https://portal.azure.com/?feature.msaljs=true#view/Microsoft_Azure_PIMCommon/ActivationMenuBlade/~/aadgroup/provider/aadgroup");

                // Wait for the page to load
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Wait for PIM roles list to load
                _logger.LogStep("Wait for PIM roles list to load");
                var gridSelectors = new[]
                {
                    "//table[@class='azc-grid-full']/tbody[@class='azc-grid-groupdata']",
                    ".azc-grid-full tbody",
                    "[data-automation-id*='grid'] tbody",
                    "table tbody"
                };
                
                await WaitForAnyElementAsync(gridSelectors, 15000);

                // Check active assignments first (faster check)
                _logger.LogStep("Check if role is already active");
                await _page.ClickAsync("//span[contains(text(),'Active assignments')]/parent::span/parent::div[@role='tab']");
                await _page.WaitForTimeoutAsync(1000);
                
                var activeRoles = await _page.QuerySelectorAllAsync($"//td[@aria-colindex='2']//div[text()='{roleName}']");
                if (activeRoles.Count > 0)
                {
                    _logger.LogInfo($"✅ {roleName} is already active - skipping activation");
                    return;
                }

                // Switch to eligible assignments tab
                _logger.LogStep("Switch to eligible assignments");
                await _page.ClickAsync("//span[contains(text(),'Eligible assignments')]/parent::span/parent::div[@role='tab']");
                await _page.WaitForTimeoutAsync(1000);

                // Find and click on the specific role
                _logger.LogStep($"Look for eligible role: {roleName}");
                var roleSelectors = new[]
                {
                    $"//div[contains(text(),'{roleName}')]",
                    $"//td[contains(text(),'{roleName}')]",
                    $"//*[contains(text(),'{roleName}')]"
                };

                var roleElement = await WaitForAnyElementAsync(roleSelectors, 10000);
                if (roleElement == null)
                {
                    throw new InvalidOperationException($"Could not find PIM role: {roleName}");
                }

                // Click on the role to select it
                await roleElement.ClickAsync();
                _logger.LogAction($"Selected PIM role: {roleName}");

                // Click the Activate button
                _logger.LogStep("Click Activate button");
                var activateSelectors = new[]
                {
                    $"//div[contains(text(),'{roleName}')]/../../../following-sibling::td//a",
                    "//a[contains(text(),'Activate')]",
                    "//button[contains(text(),'Activate')]",
                    "[data-automation-id*='activate']"
                };
                
                var activateButton = await WaitForAnyElementAsync(activateSelectors, 8000);
                await activateButton.ClickAsync();
                _logger.LogAction("Clicked Activate button");

                // Wait for the activation form
                _logger.LogStep("Wait for activation form");
                var formSelectors = new[]
                {
                    "//div[@aria-labelledby='fxsContextPaneTitle']",
                    ".fxs-contextpane-content",
                    "[aria-labelledby*='contextpane']",
                    ".activation-form"
                };
                
                await WaitForAnyElementAsync(formSelectors, 8000);

                // Fill in the justification
                _logger.LogStep($"Fill in justification: {justification}");
                var justificationSelectors = new[]
                {
                    "//label[contains(text(),'Reason')]//../..//textarea",
                    "textarea[aria-label*='reason']",
                    "textarea[placeholder*='reason']",
                    "textarea"
                };
                
                var justificationField = await WaitForAnyElementAsync(justificationSelectors, 5000);
                await justificationField.FillAsync(justification);

                // Set the duration if different from default
                if (durationHours != 8)
                {
                    _logger.LogStep($"Set activation duration to: {durationHours} hours");
                    await SetActivationDurationAsync(durationHours);
                }

                // Submit the activation request
                _logger.LogStep("Submit PIM role activation request");
                var submitSelectors = new[]
                {
                    "//div[contains(.,'Activate') and @role='button']",
                    "//button[contains(text(),'Activate')]",
                    "[data-automation-id*='activate-button']",
                    ".primary-button"
                };
                
                var submitButton = await WaitForAnyElementAsync(submitSelectors, 5000);
                await submitButton.ClickAsync();

                // Wait for confirmation
                _logger.LogStep("Wait for activation confirmation");
                var confirmationSelectors = new[]
                {
                    "//span[contains(text(),'Your active assignments have changed')]",
                    "//span[contains(text(),'successfully activated')]",
                    ".success-message",
                    "[data-automation-id*='success']"
                };
                
                await WaitForAnyElementAsync(confirmationSelectors, 30000);

                // Navigate back to active assignments to verify
                await _page.ClickAsync("//span[contains(text(),'Active assignments')]/parent::span/parent::div[@role='tab']");
                
                _logger.LogInfo($"✅ PIM role activation completed successfully for role: {roleName}");
            }
            catch (TimeoutException ex)
            {
                _logger.LogError($"❌ Timeout occurred during PIM role activation for role: {roleName}");
                throw new InvalidOperationException($"Timeout during PIM role activation for {roleName}. The page may have taken too long to load or the role may not be available.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Failed to activate PIM role: {roleName} - {ex.Message}");
                throw new InvalidOperationException($"Failed to activate PIM role: {roleName}", ex);
            }
        }

        /// <summary>
        /// Activates the Data Scientist PIM role with predefined settings
        /// </summary>
        public async Task ActivateDataScientistRoleAsync()
        {
            const string roleName = "PIM_UKIN_CTAO_AI_PLATFORM_DEV_DATA_SCIENTIST";
            const string justification = "Automated test setup - activating Data Scientist role for test execution";
            
            await ActivatePIMRoleAsync(roleName, justification, 8);
        }

        /// <summary>
        /// Checks if a PIM role is currently active
        /// </summary>
        /// <param name="roleName">Name of the role to check</param>
        /// <returns>True if the role is active, false otherwise</returns>
        public async Task<bool> IsRoleActiveAsync(string roleName)
        {
            try
            {
                _logger.LogInfo($"🔍 Checking if PIM role is active: {roleName}");

                // Navigate to PIM page
                await _page.GotoAsync("https://portal.azure.com/?feature.msaljs=true#view/Microsoft_Azure_PIMCommon/ActivationMenuBlade/~/aadgroup/provider/aadgroup");
                await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Click on active assignments tab
                await _page.ClickAsync("//span[contains(text(),'Active assignments')]/parent::span/parent::div[@role='tab']");
                await _page.WaitForTimeoutAsync(2000);

                // Check if role is in active assignments
                var activeRoles = await _page.QuerySelectorAllAsync($"//td[@aria-colindex='2']//div[text()='{roleName}']");
                bool isActive = activeRoles.Count > 0;

                _logger.LogInfo($"PIM role {roleName} active status: {(isActive ? "✅ Active" : "❌ Not Active")}");
                return isActive;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ Could not check PIM role status: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Waits for any of the provided selectors to become available
        /// </summary>
        private async Task<IElementHandle?> WaitForAnyElementAsync(string[] selectors, int timeoutMs)
        {
            var tasks = selectors.Select(async selector =>
            {
                try
                {
                    var element = await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
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
        private async Task SetActivationDurationAsync(int hours)
        {
            try
            {
                // Look for duration dropdown or input field
                var durationSelector = "select[aria-label*='duration'], select[data-automation-id*='duration'], #duration";
                
                if (await _page.Locator(durationSelector).CountAsync() > 0)
                {
                    // If it's a dropdown, select the appropriate value
                    await _page.SelectOptionAsync(durationSelector, hours.ToString());
                }
                else
                {
                    // Look for hour input field
                    var hourInput = "input[aria-label*='hour'], input[data-automation-id*='hour']";
                    
                    if (await _page.Locator(hourInput).CountAsync() > 0)
                    {
                        await _page.FillAsync(hourInput, hours.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ Could not set custom duration, using default duration: {ex.Message}");
            }
        }
    }
}