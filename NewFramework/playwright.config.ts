import { defineConfig, devices } from '@playwright/test';
import { ConfigManager } from './Utils/ConfigManager';

const config = ConfigManager.getInstance();

export default defineConfig({
  testDir: './TypeScriptTests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: config.getTestSettings().RetryCount,
  workers: process.env.CI ? 1 : config.getTestSettings().ParallelWorkers,
  reporter: [
    ['html', { outputFolder: './Reports/html-report' }],
    ['json', { outputFile: './Reports/test-results.json' }],
    ['junit', { outputFile: './Reports/junit-results.xml' }],
    ['list']
  ],
  use: {
    baseURL: config.getCurrentEnvironment().BaseUrl,
    trace: config.getTestSettings().TraceOnFailure ? 'on-first-retry' : 'off',
    screenshot: config.getTestSettings().ScreenshotOnFailure ? 'only-on-failure' : 'off',
    video: config.getTestSettings().VideoOnFailure ? 'retain-on-failure' : 'off',
    actionTimeout: config.getBrowserSettings().Timeout,
    navigationTimeout: config.getBrowserSettings().Timeout,
  },
  projects: [
    {
      name: 'chromium',
      use: { 
        ...devices['Desktop Chrome'],
        viewport: {
          width: config.getBrowserSettings().ViewportWidth,
          height: config.getBrowserSettings().ViewportHeight
        }
      },
    },
    {
      name: 'firefox',
      use: { 
        ...devices['Desktop Firefox'],
        viewport: {
          width: config.getBrowserSettings().ViewportWidth,
          height: config.getBrowserSettings().ViewportHeight
        }
      },
    },
    {
      name: 'webkit',
      use: { 
        ...devices['Desktop Safari'],
        viewport: {
          width: config.getBrowserSettings().ViewportWidth,
          height: config.getBrowserSettings().ViewportHeight
        }
      },
    },
    {
      name: 'electron',
      use: {
        ...devices['Desktop Chrome'],
      },
      testMatch: '**/*electron*.spec.ts',
    },
  ],
  outputDir: './Reports/test-results/',
});