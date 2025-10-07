"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const test_1 = require("@playwright/test");
/**
 * Playwright configuration for Electron/VS Code testing
 * @see https://playwright.dev/docs/test-configuration
 */
exports.default = (0, test_1.defineConfig)({
    testDir: './tests',
    /* Run tests in files in parallel */
    fullyParallel: false,
    /* Fail the build on CI if you accidentally left test.only in the source code. */
    forbidOnly: !!process.env.CI,
    /* Retry on CI only */
    retries: process.env.CI ? 2 : 0,
    /* Opt out of parallel tests on CI. */
    workers: process.env.CI ? 1 : 5,
    /* Reporter to use. See https://playwright.dev/docs/test-reporters */
    reporter: [
        ['html', { outputFolder: './test-results/html-report' }],
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
    },
    /* Configure projects for Electron testing */
    projects: [
        {
            name: 'electron-vscode',
            use: {
                // Remove browser-specific settings for Electron
                // Electron apps don't use browser devices
                headless: false, // Always run in headed mode for Electron
                launchOptions: {
                    // Electron-specific launch options
                    slowMo: 100, // Slow down actions for better visibility
                }
            },
        },
    ],
    /* Global setup and teardown */
    globalSetup: require.resolve('./utils/global-setup.js'),
    globalTeardown: require.resolve('./utils/global-teardown.js'),
});
