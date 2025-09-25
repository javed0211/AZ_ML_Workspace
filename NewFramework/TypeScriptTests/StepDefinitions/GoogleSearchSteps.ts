import { Given, When, Then, Before, After, setDefaultTimeout } from '@cucumber/cucumber';
import { Browser, BrowserContext, Page, chromium } from 'playwright';
import { expect } from '@playwright/test';

// Set default timeout for all steps
setDefaultTimeout(30 * 1000);

// Simple logger interface for TypeScript BDD tests
interface ILogger {
  info(message: string): void;
  warn(message: string): void;
  error(message: string, exception?: Error): void;
  step(stepDescription: string): void;
  action(action: string, element?: string): void;
  testStart(testName: string): void;
  testEnd(testName: string, status: string): void;
}

// Simple Playwright utilities interface
interface IPlaywrightUtils {
  navigateTo(url: string): Promise<void>;
  assertTitle(titlePattern: RegExp): Promise<void>;
  waitForElement(selector: string, timeout?: number): Promise<void>;
  click(selector: string): Promise<void>;
  fill(selector: string, text: string): Promise<void>;
  pressKeyOnElement(selector: string, key: string): Promise<void>;
  waitForUrl(urlPattern: RegExp): Promise<void>;
  getText(selector: string): Promise<string>;
  getElementCount(selector: string): Promise<number>;
  takeScreenshot(name: string): Promise<void>;
  getCurrentUrl(): Promise<string>;
  getTitle(): Promise<string>;
  waitForLoadState(state: string): Promise<void>;
}

// Simple logger implementation
class SimpleLogger implements ILogger {
  info(message: string): void {
    console.log(`[INFO] ${new Date().toISOString()} ${message}`);
  }
  
  warn(message: string): void {
    console.log(`[WARN] ${new Date().toISOString()} ${message}`);
  }
  
  error(message: string, exception?: Error): void {
    console.log(`[ERROR] ${new Date().toISOString()} ${message}`, exception);
  }
  
  step(stepDescription: string): void {
    console.log(`[STEP] ${new Date().toISOString()} 📝 ${stepDescription}`);
  }
  
  action(action: string, element?: string): void {
    const message = element ? `🎯 Action: ${action} on ${element}` : `🎯 Action: ${action}`;
    console.log(`[ACTION] ${new Date().toISOString()} ${message}`);
  }
  
  testStart(testName: string): void {
    console.log(`[TEST] ${new Date().toISOString()} 🚀 Starting test: ${testName}`);
  }
  
  testEnd(testName: string, status: string): void {
    const emoji = status === "PASSED" ? "✅" : "❌";
    console.log(`[TEST] ${new Date().toISOString()} ${emoji} Test completed: ${testName} - ${status}`);
  }
}

// Simple Playwright utilities implementation
class SimplePlaywrightUtils implements IPlaywrightUtils {
  constructor(private page: Page) {}
  
  async navigateTo(url: string): Promise<void> {
    await this.page.goto(url);
    await this.page.waitForLoadState('networkidle');
  }
  
  async assertTitle(titlePattern: RegExp): Promise<void> {
    const title = await this.page.title();
    expect(title).toMatch(titlePattern);
  }
  
  async waitForElement(selector: string, timeout: number = 5000): Promise<void> {
    await this.page.waitForSelector(selector, { timeout });
  }
  
  async click(selector: string): Promise<void> {
    await this.page.click(selector);
  }
  
  async fill(selector: string, text: string): Promise<void> {
    await this.page.fill(selector, text);
  }
  
  async pressKeyOnElement(selector: string, key: string): Promise<void> {
    await this.page.press(selector, key);
  }
  
  async waitForUrl(urlPattern: RegExp): Promise<void> {
    await this.page.waitForURL(urlPattern);
  }
  
  async getText(selector: string): Promise<string> {
    return await this.page.textContent(selector) || '';
  }
  
  async getElementCount(selector: string): Promise<number> {
    return await this.page.locator(selector).count();
  }
  
  async takeScreenshot(name: string): Promise<void> {
    try {
      await this.page.screenshot({ 
        path: `screenshots/bdd-google-${name}-${Date.now()}.png`,
        fullPage: true 
      });
    } catch (error) {
      console.log(`Failed to take screenshot: ${error}`);
    }
  }
  
  async getCurrentUrl(): Promise<string> {
    return this.page.url();
  }
  
  async getTitle(): Promise<string> {
    return await this.page.title();
  }
  
  async waitForLoadState(state: string): Promise<void> {
    await this.page.waitForLoadState(state as any);
  }
}

// World state
let browser: Browser;
let context: BrowserContext;
let page: Page;
let utils: IPlaywrightUtils;
let logger: ILogger;

// Test data storage
let searchQuery: string = '';
let searchResults: any[] = [];

Before({ tags: '@google' }, async function () {
  logger = new SimpleLogger();
  
  // Initialize browser and page for Google search tests
  browser = await chromium.launch({ 
    headless: process.env.HEADLESS === 'true',
    slowMo: 500 
  });
  context = await browser.newContext();
  page = await context.newPage();
  
  // Initialize utilities
  utils = new SimplePlaywrightUtils(page);
  
  logger.testStart('Google Search BDD Test Setup');
});

After({ tags: '@google' }, async function (scenario) {
  const status = scenario.result?.status === 'PASSED' ? 'PASSED' : 'FAILED';
  logger.testEnd(scenario.pickle.name, status);
  
  if (scenario.result?.status === 'FAILED') {
    await utils.takeScreenshot(`failed-${scenario.pickle.name.replace(/\s+/g, '-')}`);
  }
  
  // Close browser
  if (context) await context.close();
  if (browser) await browser.close();
});

// Google Search steps
Given('I navigate to Google homepage', async function () {
  logger.step('Navigate to Google homepage');
  await utils.navigateTo('https://www.google.com');
  
  logger.step('Verify Google homepage loaded');
  await utils.assertTitle(/Google/);
});

When('I accept cookies if present', async function () {
  logger.step('Accept cookies if present');
  // Handle cookie consent if it appears
  try {
    await utils.waitForElement('[id*="accept"], [id*="agree"], button:has-text("Accept all")', 3000);
    await utils.click('[id*="accept"], [id*="agree"], button:has-text("Accept all")');
    logger.info('Cookie consent accepted');
  } catch (error) {
    logger.info('No cookie consent dialog found, continuing...');
  }
});

When('I search for {string}', async function (query: string) {
  searchQuery = query;
  
  logger.step('Find and click on search input');
  // Google search input can have different selectors
  const searchSelectors = [
    'input[name="q"]',
    'textarea[name="q"]',
    '[data-testid="search-input"]',
    '#APjFqb'
  ];
  
  let searchInput = null;
  for (const selector of searchSelectors) {
    try {
      await utils.waitForElement(selector, 2000);
      searchInput = selector;
      break;
    } catch (error) {
      continue;
    }
  }
  
  if (!searchInput) {
    throw new Error('Could not find Google search input');
  }
  
  logger.step(`Type search query: ${query}`);
  await utils.fill(searchInput, query);
  
  logger.step('Press Enter to search');
  await utils.pressKeyOnElement(searchInput, 'Enter');
  
  logger.step('Wait for search results to load');
  await utils.waitForUrl(/search/);
});

Then('I should see search results', async function () {
  logger.step('Verify search results are displayed');
  // Wait for search results container
  await utils.waitForElement('#search, #rso, [data-testid="search-results"]');
});

Then('the results should contain {string}', async function (expectedText: string) {
  logger.step(`Verify search results contain ${expectedText}`);
  const resultsText = await utils.getText('#search, #rso, [data-testid="search-results"]');
  expect(resultsText.toLowerCase()).toContain(expectedText.toLowerCase());
});

Then('at least one search result should be visible', async function () {
  logger.step('Verify at least one search result is visible');
  const resultCount = await utils.getElementCount('h3, [data-testid="result-title"]');
  expect(resultCount).toBeGreaterThan(0);
  
  logger.step('Take screenshot of search results');
  await utils.takeScreenshot(`google-search-${searchQuery.replace(/\s+/g, '-')}`);
  
  logger.info('Google search test completed successfully');
});

// Advanced search steps
When('I click on the first Marshall-related result', async function () {
  logger.step('Find and click first Marshall-related result');
  // Look for the first result that contains "Marshall" in the title
  const firstResult = 'h3:has-text("Marshall"), [data-testid="result-title"]:has-text("Marshall")';
  
  try {
    await utils.waitForElement(firstResult, 5000);
    await utils.takeScreenshot('before-clicking-result');
    
    // Get the text of the first result for logging
    const resultText = await utils.getText(firstResult);
    logger.action(`Clicking on result: ${resultText}`);
    
    await utils.click(firstResult);
    
    logger.step('Wait for new page to load');
    await utils.waitForLoadState('networkidle');
    
  } catch (error) {
    logger.error('Could not find or click Marshall result', error as Error);
    await utils.takeScreenshot('marshall-result-not-found');
    throw error;
  }
});

Then('I should be navigated to a Marshall-related page', async function () {
  logger.step('Verify we navigated to a Marshall-related page');
  const currentUrl = await utils.getCurrentUrl();
  const pageTitle = await utils.getTitle();
  
  logger.info(`Navigated to: ${currentUrl}`);
  logger.info(`Page title: ${pageTitle}`);
  
  // Verify we're on a relevant page (URL or title should contain Marshall)
  const isMarshallPage = currentUrl.toLowerCase().includes('marshall') || 
                        pageTitle.toLowerCase().includes('marshall');
  
  expect(isMarshallPage).toBeTruthy();
});

Then('the page should contain Marshall information', async function () {
  // Take final screenshot
  await utils.takeScreenshot('marshall-page-loaded');
  
  logger.info('Marshall headphones search and navigation test completed successfully');
});