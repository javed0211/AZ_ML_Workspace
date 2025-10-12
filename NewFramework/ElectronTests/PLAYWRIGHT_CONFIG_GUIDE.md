# Playwright Configuration Guide

This guide explains how to use the enhanced Playwright configuration for running tests across different browsers and modes.

## Table of Contents
- [Overview](#overview)
- [Environment Variables](#environment-variables)
- [NPM Scripts](#npm-scripts)
- [Browser Selection](#browser-selection)
- [Headless vs Headed Mode](#headless-vs-headed-mode)
- [Advanced Usage](#advanced-usage)
- [Examples](#examples)

## Overview

The Playwright configuration has been enhanced to support:
- ✅ Multiple browsers (Chromium, Firefox, WebKit)
- ✅ Electron/VS Code testing
- ✅ Headless and headed modes
- ✅ Configurable execution speed (slow-mo)
- ✅ Flexible worker and retry configuration
- ✅ Environment variable-based configuration

## Environment Variables

You can control test execution using the following environment variables:

| Variable | Description | Default | Options |
|----------|-------------|---------|---------|
| `HEADLESS` | Run tests in headless mode | `false` | `true`, `false` |
| `BROWSER` | Select which browser(s) to test | `all` | `chromium`, `firefox`, `webkit`, `all` |
| `SLOW_MO` | Slow down operations (milliseconds) | `0` | Any number (e.g., `500`, `1000`) |
| `WORKERS` | Number of parallel workers | `3` (local), `1` (CI) | Any number |
| `RETRIES` | Number of retries on failure | `0` (local), `2` (CI) | Any number |
| `CI` | CI environment flag | `false` | `true`, `false` |

## NPM Scripts

### Basic Test Commands

```bash
# Run all tests (default configuration)
npm test

# Run tests in headed mode (see browser window)
npm run test:headed

# Run tests in headless mode (no browser window)
npm run test:headless
```

### Browser-Specific Tests

```bash
# Run tests only on Chromium
npm run test:chromium

# Run tests only on Firefox
npm run test:firefox

# Run tests only on WebKit (Safari)
npm run test:webkit

# Run tests on all browsers
npm run test:all-browsers

# Run tests only on Electron (VS Code)
npm run test:electron
```

### Debug and Development

```bash
# Run tests in debug mode with slow motion
npm run test:debug

# Run tests with Playwright UI mode
npm run test:ui

# Run tests with slow motion (1 second delay)
npm run test:slow

# View the last test report
npm run test:report
```

### Setup Commands

```bash
# Install all required browsers
npm run install-browsers

# Full setup (install dependencies and browsers)
npm run setup

# Clean build artifacts
npm run clean
```

## Browser Selection

### Using Environment Variables

```bash
# Test on Chromium only
BROWSER=chromium npm test

# Test on Firefox only
BROWSER=firefox npm test

# Test on WebKit only
BROWSER=webkit npm test

# Test on all browsers
BROWSER=all npm test
```

### Using Playwright CLI

```bash
# Run specific project
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
npx playwright test --project=electron-vscode
```

## Headless vs Headed Mode

### Headless Mode (No Browser Window)
Faster execution, suitable for CI/CD pipelines.

```bash
# Using environment variable
HEADLESS=true npm test

# Using npm script
npm run test:headless
```

### Headed Mode (Visible Browser Window)
Better for debugging and development.

```bash
# Using environment variable
HEADLESS=false npm test

# Using npm script
npm run test:headed
```

## Advanced Usage

### Combining Multiple Options

```bash
# Run Firefox tests in headed mode with slow motion
BROWSER=firefox HEADLESS=false SLOW_MO=500 npm test

# Run Chromium tests with 5 workers and 2 retries
BROWSER=chromium WORKERS=5 RETRIES=2 npm test

# Debug mode with specific browser
BROWSER=webkit HEADLESS=false SLOW_MO=1000 npx playwright test --debug
```

### Running Specific Tests

```bash
# Run a specific test file
npm test tests/vscode-basic.test.ts

# Run tests matching a pattern
npm test -- --grep "Azure ML"

# Run a specific test in headed mode
HEADLESS=false npm test tests/jupyter-notebook.test.ts
```

### CI/CD Configuration

For CI/CD pipelines, set the `CI` environment variable:

```bash
# CI mode (headless, 1 worker, 2 retries)
CI=true npm test

# Or explicitly configure
HEADLESS=true WORKERS=1 RETRIES=2 npm test
```

## Examples

### Example 1: Local Development
```bash
# Run tests with visible browser and slow motion for debugging
npm run test:headed
# or
HEADLESS=false SLOW_MO=500 npm test
```

### Example 2: Quick Smoke Test
```bash
# Run tests on Chromium only in headless mode
npm run test:chromium
```

### Example 3: Cross-Browser Testing
```bash
# Run tests on all browsers in headless mode
HEADLESS=true npm run test:all-browsers
```

### Example 4: Debugging a Specific Test
```bash
# Debug a specific test with slow motion
HEADLESS=false SLOW_MO=1000 npm test tests/azure-ml-integration.test.ts
```

### Example 5: CI Pipeline
```bash
# Typical CI configuration
CI=true HEADLESS=true WORKERS=1 RETRIES=2 npm test
```

### Example 6: Electron/VS Code Testing
```bash
# Run only Electron tests in headed mode
HEADLESS=false npm run test:electron
```

## Configuration File

The main configuration is in `playwright.config.ts`. Key features:

- **Projects**: Separate configurations for Electron, Chromium, Firefox, and WebKit
- **Reporters**: HTML, JSON, JUnit, and console list reporters
- **Artifacts**: Screenshots, videos, and traces on failure
- **Timeouts**: Configured for Electron apps (10s action, 30s navigation)
- **Global Setup/Teardown**: Custom setup and cleanup scripts

## Troubleshooting

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

## Additional Resources

- [Playwright Documentation](https://playwright.dev/docs/intro)
- [Playwright Test Configuration](https://playwright.dev/docs/test-configuration)
- [Playwright CLI](https://playwright.dev/docs/test-cli)
- [Debugging Tests](https://playwright.dev/docs/debug)

## Support

For issues or questions, please refer to the project documentation or contact the development team.