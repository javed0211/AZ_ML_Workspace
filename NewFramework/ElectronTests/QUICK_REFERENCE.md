# Playwright Quick Reference

## 🚀 Most Common Commands

### Run Tests
```bash
npm test                    # Run all tests (default config)
npm run test:headed         # Run with visible browser
npm run test:headless       # Run without browser window
```

### Browser Selection
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

## 🎯 Quick Environment Variable Usage

```bash
# Headed mode with slow motion
HEADLESS=false SLOW_MO=500 npm test

# Test specific browser
BROWSER=firefox npm test

# Run with multiple workers
WORKERS=5 npm test

# CI mode
CI=true npm test
```

## 📋 Environment Variables Cheat Sheet

| Variable | Values | Default |
|----------|--------|---------|
| `HEADLESS` | `true` / `false` | `false` |
| `BROWSER` | `chromium` / `firefox` / `webkit` / `all` | `all` |
| `SLOW_MO` | `0` - `5000` (ms) | `0` |
| `WORKERS` | `1` - `10` | `3` |
| `RETRIES` | `0` - `5` | `0` |

## 🔧 Common Scenarios

### Scenario 1: I want to see what's happening
```bash
npm run test:headed
```

### Scenario 2: I need to debug a failing test
```bash
npm run test:debug
# or
HEADLESS=false SLOW_MO=1000 npm test tests/my-test.test.ts
```

### Scenario 3: Quick smoke test
```bash
npm run test:chromium
```

### Scenario 4: Full cross-browser test
```bash
npm run test:all-browsers
```

### Scenario 5: CI/CD pipeline
```bash
CI=true HEADLESS=true npm test
```

## 📦 Setup Commands

```bash
npm install                 # Install dependencies
npm run install-browsers    # Install Playwright browsers
npm run setup              # Full setup (deps + browsers)
npm run clean              # Clean build artifacts
```

## 🎨 Playwright CLI Options

```bash
# Run specific test file
npm test tests/my-test.test.ts

# Run tests matching pattern
npm test -- --grep "pattern"

# Run specific project
npx playwright test --project=chromium

# List all tests
npx playwright test --list

# Show test report
npx playwright show-report
```

## 💡 Pro Tips

1. **Use headed mode during development**: `npm run test:headed`
2. **Use slow motion for debugging**: `SLOW_MO=1000 npm test`
3. **Test on specific browser first**: `npm run test:chromium`
4. **Use UI mode for interactive debugging**: `npm run test:ui`
5. **Check reports after failures**: `npm run test:report`

## 📚 More Information

See `PLAYWRIGHT_CONFIG_GUIDE.md` for detailed documentation.