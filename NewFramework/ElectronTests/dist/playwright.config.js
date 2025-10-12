"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const test_1 = require("@playwright/test");
/**
 * Playwright configuration for Electron/VS Code testing
 * @see https://playwright.dev/docs/test-configuration
 *
 * Environment Variables:
 * - HEADLESS: Set to 'true' to run in headless mode (default: false)
 * - BROWSER: Specify browser - 'chromium', 'firefox', 'webkit', or 'all' (default: all)
 * - SLOW_MO: Slow down operations by specified milliseconds (default: 0)
 * - WORKERS: Number of parallel workers (default: 1 for CI, 3 for local)
 * - RETRIES: Number of retries on failure (default: 2 for CI, 0 for local)
 */
// Read environment variables with defaults
const headless = process.env.HEADLESS === 'true';
const browserType = process.env.BROWSER || 'all';
const slowMo = parseInt(process.env.SLOW_MO || '0', 10);
const isCI = !!process.env.CI;
exports.default = (0, test_1.defineConfig)({
    testDir: './tests',
    /* Run tests in files in parallel */
    fullyParallel: false,
    /* Fail the build on CI if you accidentally left test.only in the source code. */
    forbidOnly: isCI,
    /* Retry configuration - can be overridden via RETRIES env var */
    retries: process.env.RETRIES ? parseInt(process.env.RETRIES, 10) : (isCI ? 2 : 0),
    /* Worker configuration - can be overridden via WORKERS env var */
    workers: process.env.WORKERS ? parseInt(process.env.WORKERS, 10) : (isCI ? 1 : 3),
    /* Reporter to use. See https://playwright.dev/docs/test-reporters */
    reporter: [
        ['html', { outputFolder: './playwright-report' }],
        ['json', { outputFile: './test-results/results.json' }],
        ['junit', { outputFile: './test-results/junit-results.xml' }],
        ['list']
    ],
    /* Shared settings for all the projects below. See https://playwright.dev/docs/api/class-testoptions. */
    use: {
        /* Collect trace when retrying the failed test. See https://playwright.dev/docs/trace-viewer */
        trace: 'on-first-retry',
        /* Take screenshot on failure */
        screenshot: 'only-on-failure',
        /* Record video on failure */
        video: 'retain-on-failure',
        /* Increase timeouts for Electron apps */
        actionTimeout: 10000,
        navigationTimeout: 30000,
        /* Base URL if needed for web testing */
        // baseURL: 'http://localhost:3000',
    },
    /* Configure projects for different browsers and Electron */
    projects: [
        // Electron project for VS Code testing
        {
            name: 'electron-vscode',
            use: {
                headless: headless,
                launchOptions: {
                    slowMo: slowMo,
                }
            },
        },
        // Chromium project
        ...(browserType === 'chromium' || browserType === 'all' ? [{
                name: 'chromium',
                use: {
                    ...test_1.devices['Desktop Chrome'],
                    headless: headless,
                    launchOptions: {
                        slowMo: slowMo,
                    }
                },
            }] : []),
        // Firefox project
        ...(browserType === 'firefox' || browserType === 'all' ? [{
                name: 'firefox',
                use: {
                    ...test_1.devices['Desktop Firefox'],
                    headless: headless,
                    launchOptions: {
                        slowMo: slowMo,
                    }
                },
            }] : []),
        // WebKit project
        ...(browserType === 'webkit' || browserType === 'all' ? [{
                name: 'webkit',
                use: {
                    ...test_1.devices['Desktop Safari'],
                    headless: headless,
                    launchOptions: {
                        slowMo: slowMo,
                    }
                },
            }] : []),
        /* Test against mobile viewports. */
        // {
        //   name: 'Mobile Chrome',
        //   use: { 
        //     ...devices['Pixel 5'],
        //     headless: headless,
        //   },
        // },
        // {
        //   name: 'Mobile Safari',
        //   use: { 
        //     ...devices['iPhone 12'],
        //     headless: headless,
        //   },
        // },
        /* Test against branded browsers. */
        // {
        //   name: 'Microsoft Edge',
        //   use: { 
        //     ...devices['Desktop Edge'], 
        //     channel: 'msedge',
        //     headless: headless,
        //   },
        // },
        // {
        //   name: 'Google Chrome',
        //   use: { 
        //     ...devices['Desktop Chrome'], 
        //     channel: 'chrome',
        //     headless: headless,
        //   },
        // },
    ],
    /* Global setup and teardown */
    globalSetup: require.resolve('./utils/global-setup.ts'),
    globalTeardown: require.resolve('./utils/global-teardown.ts'),
});
