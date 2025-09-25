# Test Automation Framework

A comprehensive Test Automation Framework built with Playwright that supports both C# and TypeScript test cases. This framework is designed to be beginner-friendly while providing powerful features for advanced testing scenarios.

## 🚀 Features

- **Multi-Language Support**: Write tests in both C# and TypeScript
- **Cross-Platform**: Works on Windows, macOS, and Linux
- **Multi-Browser**: Support for Chromium, Firefox, and WebKit
- **Electron Testing**: Built-in support for Electron applications (VS Code example included)
- **Configuration Management**: Environment-specific settings via `appsettings.json`
- **Rich Utilities**: Comprehensive helper methods for common test actions
- **Logging**: Console and file logging with different log levels
- **Reporting**: HTML, JSON, and JUnit test reports
- **Parallel Execution**: Run tests in parallel for faster execution
- **Screenshot Support**: Automatic screenshots on failure and manual capture
- **Easy CLI**: Simple command-line interface for running tests

## 📁 Project Structure

```
NewFramework/
├── Config/
│   └── appsettings.json          # Environment configurations
├── CSharpTests/                  # C# test files
│   ├── Tests/
│   │   └── ExampleWebTests.cs    # Sample C# tests
│   ├── Utils/
│   │   ├── ConfigManager.cs      # Configuration management
│   │   ├── Logger.cs             # Logging utilities
│   │   └── PlaywrightUtils.cs    # Test helper methods
│   └── PlaywrightFramework.csproj
├── TypeScriptTests/              # TypeScript test files
│   ├── example-web.spec.ts       # Sample web tests
│   └── example-electron.spec.ts  # Sample Electron tests
├── Utils/                        # Shared utilities
│   ├── ConfigManager.ts          # Configuration management
│   ├── Logger.ts                 # Logging utilities
│   ├── PlaywrightUtils.ts        # Test helper methods
│   └── ElectronUtils.ts          # Electron-specific utilities
├── Reports/                      # Test reports and logs
├── Documentation/                # Additional documentation
├── run-tests.sh                  # Unix/Linux test runner
├── run-tests.bat                 # Windows test runner
├── package.json                  # Node.js dependencies
├── playwright.config.ts          # Playwright configuration
└── README.md                     # This file
```

## 🛠️ Installation

### Prerequisites

- **Node.js** (v16 or higher)
- **.NET 8.0 SDK** (for C# tests)
- **PowerShell** (for .NET Playwright installation)

### Quick Setup

1. **Clone or download the framework**
2. **Install dependencies**:

   **On Unix/Linux/macOS:**
   ```bash
   chmod +x run-tests.sh
   ./run-tests.sh --install
   ```

   **On Windows:**
   ```cmd
   run-tests.bat --install
   ```

   **Manual installation:**
   ```bash
   # Install Node.js dependencies
   npm install
   npx playwright install
   npx playwright install-deps

   # Install .NET dependencies (if using C# tests)
   cd CSharpTests
   dotnet restore
   pwsh bin/Debug/net8.0/playwright.ps1 install
   cd ..
   ```

## ⚙️ Configuration

### Environment Settings

Edit `Config/appsettings.json` to configure your test environments:

```json
{
  "Environment": "dev",
  "Environments": {
    "dev": {
      "BaseUrl": "https://dev.example.com",
      "Username": "dev_user",
      "Password": "dev_password",
      "DatabaseConnection": "dev_connection_string"
    },
    "qa": {
      "BaseUrl": "https://qa.example.com",
      "Username": "qa_user",
      "Password": "qa_password",
      "DatabaseConnection": "qa_connection_string"
    }
  },
  "Browser": {
    "Type": "chromium",
    "Headless": false,
    "Timeout": 30000,
    "ViewportWidth": 1920,
    "ViewportHeight": 1080
  },
  "TestSettings": {
    "RetryCount": 2,
    "ParallelWorkers": 4,
    "ScreenshotOnFailure": true
  }
}
```

### Key Configuration Options

- **Environment**: Default environment to use
- **Environments**: Environment-specific settings (URLs, credentials, etc.)
- **Browser**: Browser settings (type, headless mode, timeouts, viewport)
- **TestSettings**: Test execution settings (retries, parallel workers, screenshots)
- **Logging**: Logging configuration (level, file/console output)
- **ElectronApp**: Electron application paths for different operating systems

## 🏃‍♂️ Running Tests

### Using the CLI Runner (Recommended)

**Basic usage:**
```bash
# Run TypeScript tests in dev environment
./run-tests.sh

# Run C# tests in QA environment with Firefox
./run-tests.sh --env qa --browser firefox --language csharp

# Run tests in headed mode with parallel execution
./run-tests.sh --headed --parallel --workers 4

# Run specific test pattern
./run-tests.sh --test "login" --env dev

# Run Electron tests only
./run-tests.sh --electron --headed
```

**Windows:**
```cmd
run-tests.bat --env qa --browser firefox --language csharp
```

### CLI Options

| Option | Description | Default |
|--------|-------------|---------|
| `--env` | Test environment (dev, qa, prod) | dev |
| `--browser` | Browser (chromium, firefox, webkit) | chromium |
| `--headed` | Run in headed mode | headless |
| `--parallel` | Enable parallel execution | sequential |
| `--workers` | Number of parallel workers | 1 |
| `--language` | Test language (typescript, csharp, both) | typescript |
| `--test` | Test pattern to run | all tests |
| `--electron` | Run Electron tests only | false |
| `--install` | Install dependencies | - |
| `--report` | Open test report | - |

### Direct Playwright Commands

**TypeScript tests:**
```bash
# Run all TypeScript tests
npx playwright test

# Run with specific browser
npx playwright test --project=firefox

# Run in headed mode
npx playwright test --headed

# Run specific test file
npx playwright test example-web.spec.ts
```

**C# tests:**
```bash
cd CSharpTests
dotnet test
```

## 📝 Writing Tests

### TypeScript Tests

```typescript
import { test, expect } from '@playwright/test';
import { PlaywrightUtils } from '../Utils/PlaywrightUtils';
import { Logger } from '../Utils/Logger';
import { ConfigManager } from '../Utils/ConfigManager';

test.describe('My Test Suite', () => {
  let utils: PlaywrightUtils;
  let logger: Logger;
  let config: ConfigManager;

  test.beforeEach(async ({ page }) => {
    utils = new PlaywrightUtils(page);
    logger = Logger.getInstance();
    config = ConfigManager.getInstance();
  });

  test('should perform login', async ({ page }) => {
    const env = config.getCurrentEnvironment();
    
    await utils.navigateTo(`${env.BaseUrl}/login`);
    await utils.fill('#username', env.Username);
    await utils.fill('#password', env.Password);
    await utils.click('#login-button');
    
    await utils.assertElementVisible('.dashboard');
  });
});
```

### C# Tests

```csharp
using Microsoft.Playwright.NUnit;
using PlaywrightFramework.Utils;

[TestFixture]
public class MyTests : PageTest
{
    private PlaywrightUtils _utils;
    private Logger _logger;
    private ConfigManager _config;

    [SetUp]
    public async Task Setup()
    {
        _utils = new PlaywrightUtils(Page);
        _logger = Logger.Instance;
        _config = ConfigManager.Instance;
    }

    [Test]
    public async Task ShouldPerformLogin()
    {
        var env = _config.GetCurrentEnvironment();
        
        await _utils.NavigateToAsync($"{env.BaseUrl}/login");
        await _utils.FillAsync("#username", env.Username);
        await _utils.FillAsync("#password", env.Password);
        await _utils.ClickAsync("#login-button");
        
        await _utils.AssertElementVisibleAsync(".dashboard");
    }
}
```

## 🛠️ Available Utilities

### Navigation Methods
- `navigateTo(url)` / `NavigateToAsync(url)`
- `goBack()` / `GoBackAsync()`
- `refresh()` / `RefreshAsync()`

### Element Interactions
- `click(selector)` / `ClickAsync(selector)`
- `type(selector, text)` / `TypeAsync(selector, text)`
- `fill(selector, text)` / `FillAsync(selector, text)`
- `selectByText(selector, text)` / `SelectByTextAsync(selector, text)`
- `check(selector)` / `CheckAsync(selector)`
- `uploadFile(selector, path)` / `UploadFileAsync(selector, path)`

### Wait Methods
- `waitForElement(selector)` / `WaitForElementAsync(selector)`
- `waitForText(selector, text)` / `WaitForTextAsync(selector, text)`
- `waitForUrl(url)` / `WaitForUrlAsync(url)`

### Assertions
- `assertElementVisible(selector)` / `AssertElementVisibleAsync(selector)`
- `assertText(selector, text)` / `AssertTextAsync(selector, text)`
- `assertTitle(title)` / `AssertTitleAsync(title)`

### Screenshots
- `takeScreenshot(filename)` / `TakeScreenshotAsync(filename)`
- `takeElementScreenshot(selector, filename)` / `TakeElementScreenshotAsync(selector, filename)`

### Advanced Features
- `dragAndDrop(source, target)` / `DragAndDropAsync(source, target)`
- `hover(selector)` / `HoverAsync(selector)`
- `scrollToElement(selector)` / `ScrollToElementAsync(selector)`

## 🖥️ Electron Testing

The framework includes built-in support for testing Electron applications. Example with VS Code:

```typescript
import { ElectronUtils } from '../Utils/ElectronUtils';

test('should test VS Code', async () => {
  const electronUtils = new ElectronUtils();
  
  // Launch VS Code
  const app = await electronUtils.launchElectronApp();
  const window = await electronUtils.getMainWindow();
  
  // Test interactions
  const title = await electronUtils.getWindowTitle(window);
  expect(title).toContain('Visual Studio Code');
  
  // Take screenshot
  await electronUtils.takeScreenshotOfWindow(window);
  
  // Close app
  await electronUtils.closeApp();
});
```

## 📊 Reports and Logging

### Test Reports

After running tests, reports are generated in the `Reports/` directory:

- **HTML Report**: `Reports/html-report/index.html` - Interactive HTML report
- **JSON Report**: `Reports/test-results.json` - Machine-readable results
- **JUnit Report**: `Reports/junit-results.xml` - For CI/CD integration

**View reports:**
```bash
./run-tests.sh --report
```

### Logging

Logs are written to both console and file (`Reports/logs/test-execution.log`):

```typescript
logger.logStep('Performing login');
logger.logAction('Click login button', '#login-btn');
logger.info('Test completed successfully');
```

## 🎯 Quick Start Example

### Google Search Test

We've included a simple Google search test to get you started quickly:

**Run the Google search test:**
```bash
# Run in headless mode
./run-google-test.sh

# Run in headed mode (visible browser)
./run-google-test.sh --headed

# Run with Firefox
./run-google-test.sh --headed --browser firefox
```

**Or using the main runner:**
```bash
# TypeScript version
./run-tests.sh --test "google-search" --headed

# C# version
./run-tests.sh --language csharp --test "GoogleSearch" --headed
```

This test demonstrates:
- Opening a browser and navigating to Google
- Handling cookie consent dialogs
- Searching for "marshall headphones"
- Verifying search results
- Taking screenshots
- Clicking on search results

## 🔧 Customization

### Adding New Environments

1. Edit `Config/appsettings.json`
2. Add new environment configuration:

```json
{
  "Environments": {
    "staging": {
      "BaseUrl": "https://staging.example.com",
      "Username": "staging_user",
      "Password": "staging_password"
    }
  }
}
```

3. Use in tests:
```bash
./run-tests.sh --env staging
```

### Adding Custom Utilities

Extend the `PlaywrightUtils` class with your own methods:

```typescript
// In PlaywrightUtils.ts
async customLogin(username: string, password: string): Promise<void> {
  await this.fill('#username', username);
  await this.fill('#password', password);
  await this.click('#login-button');
  await this.waitForUrl(/dashboard/);
}
```

### Custom Test Data

Create test data files and reference them in your tests:

```typescript
// TestData/users.json
{
  "validUser": {
    "username": "testuser",
    "password": "testpass"
  }
}

// In your test
import userData from '../TestData/users.json';
await utils.fill('#username', userData.validUser.username);
```

## 🚀 CI/CD Integration

### GitHub Actions Example

```yaml
name: Test Automation
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      - name: Install dependencies
        run: ./run-tests.sh --install
      - name: Run tests
        run: ./run-tests.sh --env qa --parallel --workers 2
      - name: Upload test results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: test-results
          path: Reports/
```

## 🐛 Troubleshooting

### Common Issues

1. **Browser not found**
   ```bash
   npx playwright install
   ```

2. **Permission denied on scripts**
   ```bash
   chmod +x run-tests.sh
   ```

3. **Electron app path issues**
   - Update `ElectronApp` paths in `appsettings.json`
   - Ensure the application is installed

4. **Tests timing out**
   - Increase timeout in `appsettings.json`
   - Check network connectivity
   - Verify selectors are correct

### Debug Mode

Run tests in debug mode:
```bash
npx playwright test --debug
```

## 📚 Additional Resources

- [Playwright Documentation](https://playwright.dev/)
- [NUnit Documentation](https://nunit.org/)
- [Serilog Documentation](https://serilog.net/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Happy Testing! 🎉**