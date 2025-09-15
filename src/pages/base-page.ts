import { Page, Locator, expect } from '@playwright/test';
import { logger, TestLogger } from '../helpers/logger';
import { config } from '../helpers/config';

export abstract class BasePage {
  protected page: Page;
  protected testLogger?: TestLogger;
  protected baseUrl: string;

  constructor(page: Page, testLogger?: TestLogger) {
    this.page = page;
    this.testLogger = testLogger;
    this.baseUrl = config.urls.base;
  }

  // Navigation methods
  async goto(path: string = ''): Promise<void> {
    const url = path.startsWith('http') ? path : `${this.baseUrl}${path}`;
    this.testLogger?.action(`Navigating to: ${url}`);
    
    await this.page.goto(url, {
      waitUntil: 'domcontentloaded',
      timeout: config.timeouts.navigation,
    });
    
    await this.waitForPageLoad();
    this.testLogger?.info(`Navigation completed: ${url}`);
  }

  async reload(): Promise<void> {
    this.testLogger?.action('Reloading page');
    await this.page.reload({ waitUntil: 'domcontentloaded' });
    await this.waitForPageLoad();
    this.testLogger?.info('Page reloaded');
  }

  // Wait methods
  async waitForPageLoad(): Promise<void> {
    // Override in subclasses for specific page load indicators
    await this.page.waitForLoadState('domcontentloaded');
  }

  async waitForElement(selector: string, timeout: number = config.timeouts.default): Promise<Locator> {
    this.testLogger?.debug(`Waiting for element: ${selector}`);
    const element = this.page.locator(selector);
    await element.waitFor({ timeout });
    return element;
  }

  async waitForText(text: string, timeout: number = config.timeouts.default): Promise<Locator> {
    this.testLogger?.debug(`Waiting for text: ${text}`);
    const element = this.page.locator(`text=${text}`);
    await element.waitFor({ timeout });
    return element;
  }

  async waitForUrl(urlPattern: string | RegExp, timeout: number = config.timeouts.navigation): Promise<void> {
    this.testLogger?.debug(`Waiting for URL pattern: ${urlPattern}`);
    await this.page.waitForURL(urlPattern, { timeout });
  }

  // Interaction methods
  async clickElement(selector: string, options?: { timeout?: number; force?: boolean }): Promise<void> {
    this.testLogger?.action(`Clicking element: ${selector}`);
    const element = await this.waitForElement(selector, options?.timeout);
    await element.click({ force: options?.force });
  }

  async fillInput(selector: string, value: string, options?: { clear?: boolean }): Promise<void> {
    this.testLogger?.action(`Filling input: ${selector} with value: ${value}`);
    const element = await this.waitForElement(selector);
    
    if (options?.clear !== false) {
      await element.clear();
    }
    
    await element.fill(value);
  }

  async selectOption(selector: string, value: string): Promise<void> {
    this.testLogger?.action(`Selecting option: ${value} in ${selector}`);
    const element = await this.waitForElement(selector);
    await element.selectOption(value);
  }

  async uploadFile(selector: string, filePath: string): Promise<void> {
    this.testLogger?.action(`Uploading file: ${filePath} to ${selector}`);
    const element = await this.waitForElement(selector);
    await element.setInputFiles(filePath);
  }

  // Assertion methods
  async assertElementVisible(selector: string, timeout: number = config.timeouts.default): Promise<void> {
    this.testLogger?.assertion(`Element visible: ${selector}`, true);
    const element = this.page.locator(selector);
    await expect(element).toBeVisible({ timeout });
  }

  async assertElementHidden(selector: string, timeout: number = config.timeouts.default): Promise<void> {
    this.testLogger?.assertion(`Element hidden: ${selector}`, true);
    const element = this.page.locator(selector);
    await expect(element).toBeHidden({ timeout });
  }

  async assertElementText(selector: string, expectedText: string | RegExp): Promise<void> {
    this.testLogger?.assertion(`Element text: ${selector} contains ${expectedText}`, true);
    const element = this.page.locator(selector);
    await expect(element).toContainText(expectedText);
  }

  async assertElementValue(selector: string, expectedValue: string): Promise<void> {
    this.testLogger?.assertion(`Element value: ${selector} equals ${expectedValue}`, true);
    const element = this.page.locator(selector);
    await expect(element).toHaveValue(expectedValue);
  }

  async assertPageTitle(expectedTitle: string | RegExp): Promise<void> {
    this.testLogger?.assertion(`Page title: ${expectedTitle}`, true);
    await expect(this.page).toHaveTitle(expectedTitle);
  }

  async assertPageUrl(expectedUrl: string | RegExp): Promise<void> {
    this.testLogger?.assertion(`Page URL: ${expectedUrl}`, true);
    await expect(this.page).toHaveURL(expectedUrl);
  }

  // Utility methods
  async getElementText(selector: string): Promise<string> {
    const element = await this.waitForElement(selector);
    return await element.textContent() || '';
  }

  async getElementValue(selector: string): Promise<string> {
    const element = await this.waitForElement(selector);
    return await element.inputValue();
  }

  async getElementAttribute(selector: string, attribute: string): Promise<string | null> {
    const element = await this.waitForElement(selector);
    return await element.getAttribute(attribute);
  }

  async isElementVisible(selector: string): Promise<boolean> {
    try {
      const element = this.page.locator(selector);
      return await element.isVisible();
    } catch {
      return false;
    }
  }

  async isElementEnabled(selector: string): Promise<boolean> {
    try {
      const element = this.page.locator(selector);
      return await element.isEnabled();
    } catch {
      return false;
    }
  }

  // Screenshot methods
  async takeScreenshot(name?: string): Promise<Buffer> {
    const screenshotName = name || `screenshot-${Date.now()}`;
    this.testLogger?.action(`Taking screenshot: ${screenshotName}`);
    
    return await this.page.screenshot({
      fullPage: true,
      path: `${config.artifacts.path}/screenshots/${screenshotName}.png`,
    });
  }

  async takeElementScreenshot(selector: string, name?: string): Promise<Buffer> {
    const screenshotName = name || `element-screenshot-${Date.now()}`;
    this.testLogger?.action(`Taking element screenshot: ${screenshotName} for ${selector}`);
    
    const element = await this.waitForElement(selector);
    return await element.screenshot({
      path: `${config.artifacts.path}/screenshots/${screenshotName}.png`,
    });
  }

  // Error handling
  async handleError(error: Error, context?: string): Promise<void> {
    const errorContext = context || 'Unknown operation';
    this.testLogger?.error(`Error in ${errorContext}`, { 
      error: error.message,
      stack: error.stack,
      url: this.page.url(),
    });
    
    // Take screenshot on error
    try {
      await this.takeScreenshot(`error-${Date.now()}`);
    } catch (screenshotError) {
      this.testLogger?.warn('Failed to take error screenshot', { error: screenshotError });
    }
    
    throw error;
  }

  // Browser context methods
  async clearCookies(): Promise<void> {
    this.testLogger?.action('Clearing cookies');
    await this.page.context().clearCookies();
  }

  async clearLocalStorage(): Promise<void> {
    this.testLogger?.action('Clearing local storage');
    await this.page.evaluate(() => localStorage.clear());
  }

  async clearSessionStorage(): Promise<void> {
    this.testLogger?.action('Clearing session storage');
    await this.page.evaluate(() => sessionStorage.clear());
  }

  // Network methods
  async waitForResponse(urlPattern: string | RegExp, timeout: number = config.timeouts.default): Promise<void> {
    this.testLogger?.debug(`Waiting for response: ${urlPattern}`);
    await this.page.waitForResponse(urlPattern, { timeout });
  }

  async waitForRequest(urlPattern: string | RegExp, timeout: number = config.timeouts.default): Promise<void> {
    this.testLogger?.debug(`Waiting for request: ${urlPattern}`);
    await this.page.waitForRequest(urlPattern, { timeout });
  }

  // Keyboard and mouse methods
  async pressKey(key: string): Promise<void> {
    this.testLogger?.action(`Pressing key: ${key}`);
    await this.page.keyboard.press(key);
  }

  async typeText(text: string, delay?: number): Promise<void> {
    this.testLogger?.action(`Typing text: ${text}`);
    await this.page.keyboard.type(text, { delay });
  }

  async scrollToElement(selector: string): Promise<void> {
    this.testLogger?.action(`Scrolling to element: ${selector}`);
    const element = await this.waitForElement(selector);
    await element.scrollIntoViewIfNeeded();
  }

  async scrollToTop(): Promise<void> {
    this.testLogger?.action('Scrolling to top');
    await this.page.evaluate(() => window.scrollTo(0, 0));
  }

  async scrollToBottom(): Promise<void> {
    this.testLogger?.action('Scrolling to bottom');
    await this.page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  }

  // Abstract methods to be implemented by subclasses
  abstract isPageLoaded(): Promise<boolean>;
  abstract getPageIdentifier(): string;
}