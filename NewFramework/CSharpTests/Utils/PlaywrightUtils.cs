using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace PlaywrightFramework.Utils
{
    public class PlaywrightUtils
    {
        private readonly IPage _page;
        private readonly Logger _logger;
        private readonly ConfigManager _config;

        public PlaywrightUtils(IPage page)
        {
            _page = page;
            _logger = Logger.Instance;
            _config = ConfigManager.Instance;
        }

        // Navigation Methods
        public async Task NavigateToAsync(string url)
        {
            _logger.LogAction($"Navigate to: {url}");
            await _page.GotoAsync(url);
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        public async Task GoBackAsync()
        {
            _logger.LogAction("Navigate back");
            await _page.GoBackAsync();
        }

        public async Task GoForwardAsync()
        {
            _logger.LogAction("Navigate forward");
            await _page.GoForwardAsync();
        }

        public async Task RefreshAsync()
        {
            _logger.LogAction("Refresh page");
            await _page.ReloadAsync();
        }

        // Element Interaction Methods
        public async Task ClickAsync(string selector, PageClickOptions? options = null)
        {
            _logger.LogAction("Click", selector);
            await _page.ClickAsync(selector, options);
        }

        public async Task DoubleClickAsync(string selector)
        {
            _logger.LogAction("Double click", selector);
            await _page.DblClickAsync(selector);
        }

        public async Task RightClickAsync(string selector)
        {
            _logger.LogAction("Right click", selector);
            await _page.ClickAsync(selector, new PageClickOptions { Button = MouseButton.Right });
        }

        public async Task TypeAsync(string selector, string text, PageTypeOptions? options = null)
        {
            _logger.LogAction($"Type '{text}'", selector);
            await _page.FillAsync(selector, ""); // Clear first
            await _page.TypeAsync(selector, text, options);
        }

        public async Task FillAsync(string selector, string text)
        {
            _logger.LogAction($"Fill '{text}'", selector);
            await _page.FillAsync(selector, text);
        }

        public async Task ClearAsync(string selector)
        {
            _logger.LogAction("Clear", selector);
            await _page.FillAsync(selector, "");
        }

        public async Task PressKeyAsync(string key)
        {
            _logger.LogAction($"Press key: {key}");
            await _page.Keyboard.PressAsync(key);
        }

        public async Task PressKeyOnElementAsync(string selector, string key)
        {
            _logger.LogAction($"Press key '{key}'", selector);
            await _page.PressAsync(selector, key);
        }

        // Dropdown and Select Methods
        public async Task SelectByValueAsync(string selector, string value)
        {
            _logger.LogAction($"Select by value '{value}'", selector);
            await _page.SelectOptionAsync(selector, new SelectOptionValue { Value = value });
        }

        public async Task SelectByTextAsync(string selector, string text)
        {
            _logger.LogAction($"Select by text '{text}'", selector);
            await _page.SelectOptionAsync(selector, new SelectOptionValue { Label = text });
        }

        public async Task SelectByIndexAsync(string selector, int index)
        {
            _logger.LogAction($"Select by index {index}", selector);
            await _page.SelectOptionAsync(selector, new SelectOptionValue { Index = index });
        }

        // Checkbox and Radio Methods
        public async Task CheckAsync(string selector)
        {
            _logger.LogAction("Check", selector);
            await _page.CheckAsync(selector);
        }

        public async Task UncheckAsync(string selector)
        {
            _logger.LogAction("Uncheck", selector);
            await _page.UncheckAsync(selector);
        }

        public async Task<bool> IsCheckedAsync(string selector)
        {
            return await _page.IsCheckedAsync(selector);
        }

        // Wait Methods
        public async Task WaitForElementAsync(string selector, int? timeout = null)
        {
            _logger.LogAction("Wait for element", selector);
            await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions 
            { 
                Timeout = timeout ?? _config.GetTestSettings().DefaultTimeout 
            });
        }

        public async Task WaitForElementToBeVisibleAsync(string selector, int? timeout = null)
        {
            _logger.LogAction("Wait for element to be visible", selector);
            await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions 
            { 
                State = WaitForSelectorState.Visible,
                Timeout = timeout ?? _config.GetTestSettings().DefaultTimeout 
            });
        }

        public async Task WaitForElementToBeHiddenAsync(string selector, int? timeout = null)
        {
            _logger.LogAction("Wait for element to be hidden", selector);
            await _page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions 
            { 
                State = WaitForSelectorState.Hidden,
                Timeout = timeout ?? _config.GetTestSettings().DefaultTimeout 
            });
        }

        public async Task WaitForTextAsync(string selector, string text, int? timeout = null)
        {
            _logger.LogAction($"Wait for text '{text}'", selector);
            await _page.Locator(selector).Filter(new LocatorFilterOptions { HasText = text })
                .WaitForAsync(new LocatorWaitForOptions 
                { 
                    Timeout = timeout ?? _config.GetTestSettings().DefaultTimeout 
                });
        }

        public async Task WaitForUrlAsync(string url, int? timeout = null)
        {
            _logger.LogAction($"Wait for URL: {url}");
            await _page.WaitForURLAsync(url, new PageWaitForURLOptions 
            { 
                Timeout = timeout ?? _config.GetTestSettings().DefaultTimeout 
            });
        }

        public async Task WaitForUrlAsync(Regex urlPattern, int? timeout = null)
        {
            _logger.LogAction($"Wait for URL pattern: {urlPattern}");
            await _page.WaitForURLAsync(urlPattern, new PageWaitForURLOptions 
            { 
                Timeout = timeout ?? _config.GetTestSettings().DefaultTimeout 
            });
        }

        public async Task WaitForLoadStateAsync(LoadState state = LoadState.NetworkIdle)
        {
            _logger.LogAction($"Wait for load state: {state}");
            await _page.WaitForLoadStateAsync(state);
        }

        public async Task SleepAsync(int milliseconds)
        {
            _logger.LogAction($"Sleep for {milliseconds}ms");
            await _page.WaitForTimeoutAsync(milliseconds);
        }

        // Get Methods
        public async Task<string> GetTextAsync(string selector)
        {
            var text = await _page.TextContentAsync(selector) ?? "";
            _logger.LogAction($"Get text: '{text}'", selector);
            return text;
        }

        public async Task<string> GetValueAsync(string selector)
        {
            var value = await _page.InputValueAsync(selector);
            _logger.LogAction($"Get value: '{value}'", selector);
            return value;
        }

        public async Task<string?> GetAttributeAsync(string selector, string attribute)
        {
            var value = await _page.GetAttributeAsync(selector, attribute);
            _logger.LogAction($"Get attribute '{attribute}': '{value}'", selector);
            return value;
        }

        public async Task<string> GetTitleAsync()
        {
            var title = await _page.TitleAsync();
            _logger.LogAction($"Get page title: '{title}'");
            return title;
        }

        public string GetCurrentUrl()
        {
            var url = _page.Url;
            _logger.LogAction($"Get current URL: '{url}'");
            return url;
        }

        public async Task<string> GetCurrentUrlAsync()
        {
            var url = _page.Url;
            _logger.LogAction($"Get current URL: '{url}'");
            return url;
        }

        // Element State Methods
        public async Task<bool> IsVisibleAsync(string selector)
        {
            return await _page.IsVisibleAsync(selector);
        }

        public async Task<bool> IsHiddenAsync(string selector)
        {
            return await _page.IsHiddenAsync(selector);
        }

        public async Task<bool> IsEnabledAsync(string selector)
        {
            return await _page.IsEnabledAsync(selector);
        }

        public async Task<bool> IsDisabledAsync(string selector)
        {
            return await _page.IsDisabledAsync(selector);
        }

        public async Task<int> GetElementCountAsync(string selector)
        {
            var count = await _page.Locator(selector).CountAsync();
            _logger.LogAction($"Element count: {count}", selector);
            return count;
        }

        // File Upload Methods
        public async Task UploadFileAsync(string selector, string filePath)
        {
            _logger.LogAction($"Upload file: {filePath}", selector);
            await _page.SetInputFilesAsync(selector, filePath);
        }

        public async Task UploadMultipleFilesAsync(string selector, string[] filePaths)
        {
            _logger.LogAction($"Upload multiple files: {string.Join(", ", filePaths)}", selector);
            await _page.SetInputFilesAsync(selector, filePaths);
        }

        // Screenshot Methods
        public async Task<string> TakeScreenshotAsync(string? fileName = null)
        {
            var screenshotDir = "./Reports/screenshots";
            if (!Directory.Exists(screenshotDir))
            {
                Directory.CreateDirectory(screenshotDir);
            }
            
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var screenshotName = fileName ?? $"screenshot-{timestamp}.png";
            var screenshotPath = Path.Combine(screenshotDir, screenshotName);
            
            await _page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
            _logger.LogAction($"Screenshot saved: {screenshotPath}");
            return screenshotPath;
        }

        public async Task<string> TakeElementScreenshotAsync(string selector, string? fileName = null)
        {
            var screenshotDir = "./Reports/screenshots";
            if (!Directory.Exists(screenshotDir))
            {
                Directory.CreateDirectory(screenshotDir);
            }
            
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var screenshotName = fileName ?? $"element-screenshot-{timestamp}.png";
            var screenshotPath = Path.Combine(screenshotDir, screenshotName);
            
            await _page.Locator(selector).ScreenshotAsync(new LocatorScreenshotOptions { Path = screenshotPath });
            _logger.LogAction($"Element screenshot saved: {screenshotPath}", selector);
            return screenshotPath;
        }

        // Assertion Helper Methods
        public async Task AssertElementVisibleAsync(string selector, string? message = null)
        {
            _logger.LogAction("Assert element visible", selector);
            await Assertions.Expect(_page.Locator(selector)).ToBeVisibleAsync();
        }

        public async Task AssertElementHiddenAsync(string selector, string? message = null)
        {
            _logger.LogAction("Assert element hidden", selector);
            await Assertions.Expect(_page.Locator(selector)).ToBeHiddenAsync();
        }

        public async Task AssertElementEnabledAsync(string selector, string? message = null)
        {
            _logger.LogAction("Assert element enabled", selector);
            await Assertions.Expect(_page.Locator(selector)).ToBeEnabledAsync();
        }

        public async Task AssertElementDisabledAsync(string selector, string? message = null)
        {
            _logger.LogAction("Assert element disabled", selector);
            await Assertions.Expect(_page.Locator(selector)).ToBeDisabledAsync();
        }

        public async Task AssertTextAsync(string selector, string expectedText, string? message = null)
        {
            _logger.LogAction($"Assert text equals '{expectedText}'", selector);
            await Assertions.Expect(_page.Locator(selector)).ToHaveTextAsync(expectedText);
        }

        public async Task AssertTextContainsAsync(string selector, string expectedText, string? message = null)
        {
            _logger.LogAction($"Assert text contains '{expectedText}'", selector);
            await Assertions.Expect(_page.Locator(selector)).ToContainTextAsync(expectedText);
        }

        public async Task AssertValueAsync(string selector, string expectedValue, string? message = null)
        {
            _logger.LogAction($"Assert value equals '{expectedValue}'", selector);
            await Assertions.Expect(_page.Locator(selector)).ToHaveValueAsync(expectedValue);
        }

        public async Task AssertTitleAsync(string expectedTitle, string? message = null)
        {
            _logger.LogAction($"Assert page title equals '{expectedTitle}'");
            await Assertions.Expect(_page).ToHaveTitleAsync(expectedTitle);
        }

        public async Task AssertTitleContainsAsync(string expectedText, string? message = null)
        {
            _logger.LogAction($"Assert page title contains '{expectedText}'");
            await Assertions.Expect(_page).ToHaveTitleAsync(new Regex($".*{Regex.Escape(expectedText)}.*"));
        }

        public async Task AssertUrlAsync(string expectedUrl, string? message = null)
        {
            _logger.LogAction($"Assert URL equals '{expectedUrl}'");
            await Assertions.Expect(_page).ToHaveURLAsync(expectedUrl);
        }

        public async Task AssertUrlAsync(Regex expectedUrlPattern, string? message = null)
        {
            _logger.LogAction($"Assert URL matches pattern '{expectedUrlPattern}'");
            await Assertions.Expect(_page).ToHaveURLAsync(expectedUrlPattern);
        }

        public async Task AssertElementCountAsync(string selector, int expectedCount, string? message = null)
        {
            _logger.LogAction($"Assert element count equals {expectedCount}", selector);
            await Assertions.Expect(_page.Locator(selector)).ToHaveCountAsync(expectedCount);
        }

        // Scroll Methods
        public async Task ScrollToElementAsync(string selector)
        {
            _logger.LogAction("Scroll to element", selector);
            await _page.Locator(selector).ScrollIntoViewIfNeededAsync();
        }

        public async Task ScrollToTopAsync()
        {
            _logger.LogAction("Scroll to top");
            await _page.EvaluateAsync("window.scrollTo(0, 0)");
        }

        public async Task ScrollToBottomAsync()
        {
            _logger.LogAction("Scroll to bottom");
            await _page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
        }

        // Drag and Drop
        public async Task DragAndDropAsync(string sourceSelector, string targetSelector)
        {
            _logger.LogAction($"Drag and drop from '{sourceSelector}' to '{targetSelector}'");
            await _page.Locator(sourceSelector).DragToAsync(_page.Locator(targetSelector));
        }

        // Hover Methods
        public async Task HoverAsync(string selector)
        {
            _logger.LogAction("Hover", selector);
            await _page.HoverAsync(selector);
        }

        // Focus Methods
        public async Task FocusAsync(string selector)
        {
            _logger.LogAction("Focus", selector);
            await _page.FocusAsync(selector);
        }
    }
}