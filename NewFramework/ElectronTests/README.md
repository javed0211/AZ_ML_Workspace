# Playwright Test Framework for Electron/VS Code

A comprehensive Playwright testing framework for Electron applications and VS Code automation with multi-browser support.

## 🚀 Quick Start

```bash
# Install dependencies and browsers
npm run setup

# Run all tests (default: all browsers, headed mode)
npm test

# Run tests with visible browser
npm run test:headed

# Run tests in headless mode
npm run test:headless
```

## ✨ Features

- ✅ **Multi-Browser Support**: Test on Chromium, Firefox, and WebKit
- ✅ **Electron/VS Code Testing**: Specialized support for Electron apps
- ✅ **Flexible Modes**: Easy switching between headless and headed modes
- ✅ **Environment Configuration**: Control via environment variables
- ✅ **Debug Support**: Slow motion, debug mode, and UI mode
- ✅ **Comprehensive Reporting**: HTML, JSON, and JUnit reports
- ✅ **CI/CD Ready**: Optimized configurations for CI environments

## 📋 Available Commands

### Basic Testing
```bash
npm test                    # Run all tests
npm run test:headed         # Run with visible browser
npm run test:headless       # Run without browser window
```

### Browser-Specific
```bash
npm run test:chromium       # Test on Chromium only
npm run test:firefox        # Test on Firefox only
npm run test:webkit         # Test on WebKit/Safari only
npm run test:all-browsers   # Test on all browsers
npm run test:electron       # Test Electron/VS Code only
```

### Debug & Development
```bash
npm run test:debug          # Debug mode with slow motion
npm run test:ui             # Interactive UI mode
npm run test:slow           # Run with 1s delay between actions
npm run test:report         # View last test report
```

### Setup & Maintenance
```bash
npm install                 # Install dependencies
npm run install-browsers    # Install Playwright browsers
npm run setup              # Full setup (deps + browsers)
npm run build              # Build TypeScript files
npm run clean              # Clean build artifacts
```

## 🎯 Environment Variables

Control test execution using environment variables:

| Variable | Description | Default | Options |
|----------|-------------|---------|---------|
| `HEADLESS` | Run in headless mode | `false` | `true`, `false` |
| `BROWSER` | Browser selection | `all` | `chromium`, `firefox`, `webkit`, `all` |
| `SLOW_MO` | Slow motion delay (ms) | `0` | Any number |
| `WORKERS` | Parallel workers | `3` (local), `1` (CI) | Any number |
| `RETRIES` | Retry attempts | `0` (local), `2` (CI) | Any number |

### Examples

```bash
# Run Firefox tests in headed mode with slow motion
BROWSER=firefox HEADLESS=false SLOW_MO=500 npm test

# Run Chromium tests with 5 workers
BROWSER=chromium WORKERS=5 npm test

# CI mode
CI=true HEADLESS=true npm test
```

## 📁 Project Structure

```
ElectronTests/
├── tests/                          # Test files
│   ├── azure-ai-services.test.ts
│   ├── azure-ml-integration.test.ts
│   ├── jupyter-notebook.test.ts
│   └── vscode-*.test.ts
├── utils/                          # Utility files
│   ├── global-setup.ts
│   ├── global-teardown.ts
│   ├── test-helpers.ts
│   └── vscode-electron.ts
├── config/                         # Configuration files
├── playwright.config.ts            # Main Playwright config
├── package.json                    # NPM scripts and dependencies
├── tsconfig.json                   # TypeScript configuration
├── .env.example                    # Environment variable examples
├── README.md                       # This file
├── PLAYWRIGHT_CONFIG_GUIDE.md      # Detailed configuration guide
├── QUICK_REFERENCE.md              # Quick command reference
└── CONFIGURATION_CHANGES.md        # Change log
```

## 🔧 Configuration

The framework is configured via `playwright.config.ts` with support for:

- **Multiple Projects**: Electron, Chromium, Firefox, WebKit
- **Flexible Reporters**: HTML, JSON, JUnit, and console
- **Artifacts**: Screenshots, videos, and traces on failure
- **Timeouts**: Optimized for Electron apps
- **Global Setup/Teardown**: Custom initialization and cleanup

See [PLAYWRIGHT_CONFIG_GUIDE.md](./PLAYWRIGHT_CONFIG_GUIDE.md) for detailed configuration options.

## 📚 Documentation

- **[PLAYWRIGHT_CONFIG_GUIDE.md](./PLAYWRIGHT_CONFIG_GUIDE.md)** - Comprehensive configuration guide
- **[QUICK_REFERENCE.md](./QUICK_REFERENCE.md)** - Quick command reference
- **[CONFIGURATION_CHANGES.md](./CONFIGURATION_CHANGES.md)** - Recent changes and migration guide
- **[.env.example](./.env.example)** - Environment variable examples

## 🎨 Common Scenarios

### Scenario 1: Local Development
```bash
# See what's happening with visible browser
npm run test:headed
```

### Scenario 2: Debugging a Failing Test
```bash
# Debug with slow motion
npm run test:debug

# Or with custom slow motion
HEADLESS=false SLOW_MO=1000 npm test tests/my-test.test.ts
```

### Scenario 3: Quick Smoke Test
```bash
# Test on Chromium only
npm run test:chromium
```

### Scenario 4: Cross-Browser Testing
```bash
# Test on all browsers
npm run test:all-browsers
```

### Scenario 5: CI/CD Pipeline
```bash
# Optimized for CI
CI=true HEADLESS=true npm test
```

## 🐛 Troubleshooting

### Browsers Not Installed
```bash
npm run install-browsers
```

### Tests Running Too Fast
```bash
SLOW_MO=1000 npm test
```

### Need to See Browser Window
```bash
npm run test:headed
```

### Tests Failing Intermittently
```bash
RETRIES=2 npm test
```

### Want to Run Tests Sequentially
```bash
WORKERS=1 npm test
```

## 📊 Test Reports

After running tests, view the HTML report:

```bash
npm run test:report
```

Reports are generated in:
- HTML: `./playwright-report/`
- JSON: `./test-results/results.json`
- JUnit: `./test-results/junit-results.xml`

## 🔍 Running Specific Tests

```bash
# Run a specific test file
npm test tests/vscode-basic.test.ts

# Run tests matching a pattern
npm test -- --grep "Azure ML"

# Run a specific project
npx playwright test --project=chromium

# List all tests
npx playwright test --list
```

## 💡 Pro Tips

1. **Use headed mode during development**: See what's happening in real-time
2. **Use slow motion for debugging**: Easier to follow test execution
3. **Test on specific browser first**: Faster feedback during development
4. **Use UI mode for interactive debugging**: Best debugging experience
5. **Check reports after failures**: Detailed information with screenshots

## 🤝 Contributing

When adding new tests:
1. Place test files in the `tests/` directory
2. Use descriptive test names
3. Follow existing patterns and conventions
4. Test on multiple browsers when applicable
5. Update documentation as needed

## 📝 License

MIT

## 🆘 Support

For issues or questions:
1. Check the documentation files
2. Review the [Playwright documentation](https://playwright.dev)
3. Contact the development team

---

**Last Updated**: 2025
**Framework Version**: 1.0.0
**Playwright Version**: ^1.40.0