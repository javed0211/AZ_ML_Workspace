import { Page, Locator, expect, BrowserContext } from '@playwright/test';
import { Logger } from './Logger';
import { ConfigManager } from './ConfigManager';
import * as path from 'path';
import * as fs from 'fs';

export class PlaywrightUtils {
  private page: Page;
  private logger: Logger;
  private config: ConfigManager;

  constructor(page: Page) {
    this.page = page;
    this.logger = Logger.getInstance();
    this.config = ConfigManager.getInstance();
  }

  // Navigation Methods
  async navigateTo(url: string): Promise<void> {
    this.logger.logAction(`Navigate to: ${url}`);
    await this.page.goto(url);
    await this.page.waitForLoadState('networkidle');
  }

  async goBack(): Promise<void> {
    this.logger.logAction('Navigate back');
    await this.page.goBack();
  }

  async goForward(): Promise<void> {
    this.logger.logAction('Navigate forward');
    await this.page.goForward();
  }

  async refresh(): Promise<void> {
    this.logger.logAction('Refresh page');
    await this.page.reload();
  }

  // Element Interaction Methods
  async click(selector: string, options?: { timeout?: number; force?: boolean }): Promise<void> {
    this.logger.logAction('Click', selector);
    await this.page.locator(selector).click(options);
  }

  async doubleClick(selector: string): Promise<void> {
    this.logger.logAction('Double click', selector);
    await this.page.locator(selector).dblclick();
  }

  async rightClick(selector: string): Promise<void> {
    this.logger.logAction('Right click', selector);
    await this.page.locator(selector).click({ button: 'right' });
  }

  async type(selector: string, text: string, options?: { delay?: number }): Promise<void> {
    this.logger.logAction(`Type '${text}'`, selector);
    await this.page.locator(selector).fill(''); // Clear first
    await this.page.locator(selector).type(text, options);
  }

  async fill(selector: string, text: string): Promise<void> {
    this.logger.logAction(`Fill '${text}'`, selector);
    await this.page.locator(selector).fill(text);
  }

  async clear(selector: string): Promise<void> {
    this.logger.logAction('Clear', selector);
    await this.page.locator(selector).fill('');
  }

  async pressKey(key: string): Promise<void> {
    this.logger.logAction(`Press key: ${key}`);
    await this.page.keyboard.press(key);
  }

  async pressKeyOnElement(selector: string, key: string): Promise<void> {
    this.logger.logAction(`Press key '${key}'`, selector);
    await this.page.locator(selector).press(key);
  }

  // Dropdown and Select Methods
  async selectByValue(selector: string, value: string): Promise<void> {
    this.logger.logAction(`Select by value '${value}'`, selector);
    await this.page.locator(selector).selectOption({ value });
  }

  async selectByText(selector: string, text: string): Promise<void> {
    this.logger.logAction(`Select by text '${text}'`, selector);
    await this.page.locator(selector).selectOption({ label: text });
  }

  async selectByIndex(selector: string, index: number): Promise<void> {
    this.logger.logAction(`Select by index ${index}`, selector);
    await this.page.locator(selector).selectOption({ index });
  }

  // Checkbox and Radio Methods
  async check(selector: string): Promise<void> {
    this.logger.logAction('Check', selector);
    await this.page.locator(selector).check();
  }

  async uncheck(selector: string): Promise<void> {
    this.logger.logAction('Uncheck', selector);
    await this.page.locator(selector).uncheck();
  }

  async isChecked(selector: string): Promise<boolean> {
    return await this.page.locator(selector).isChecked();
  }

  // Wait Methods
  async waitForElement(selector: string, timeout?: number): Promise<void> {
    this.logger.logAction('Wait for element', selector);
    await this.page.locator(selector).waitFor({ 
      timeout: timeout || this.config.getTestSettings().DefaultTimeout 
    });
  }

  async waitForElementToBeVisible(selector: string, timeout?: number): Promise<void> {
    this.logger.logAction('Wait for element to be visible', selector);
    await this.page.locator(selector).waitFor({ 
      state: 'visible',
      timeout: timeout || this.config.getTestSettings().DefaultTimeout 
    });
  }

  async waitForElementToBeHidden(selector: string, timeout?: number): Promise<void> {
    this.logger.logAction('Wait for element to be hidden', selector);
    await this.page.locator(selector).waitFor({ 
      state: 'hidden',
      timeout: timeout || this.config.getTestSettings().DefaultTimeout 
    });
  }

  async waitForText(selector: string, text: string, timeout?: number): Promise<void> {
    this.logger.logAction(`Wait for text '${text}'`, selector);
    await this.page.locator(selector).filter({ hasText: text }).waitFor({
      timeout: timeout || this.config.getTestSettings().DefaultTimeout
    });
  }

  async waitForUrl(url: string | RegExp, timeout?: number): Promise<void> {
    this.logger.logAction(`Wait for URL: ${url}`);
    await this.page.waitForURL(url, { 
      timeout: timeout || this.config.getTestSettings().DefaultTimeout 
    });
  }

  async waitForLoadState(state: 'load' | 'domcontentloaded' | 'networkidle' = 'networkidle'): Promise<void> {
    this.logger.logAction(`Wait for load state: ${state}`);
    await this.page.waitForLoadState(state);
  }

  async sleep(milliseconds: number): Promise<void> {
    this.logger.logAction(`Sleep for ${milliseconds}ms`);
    await this.page.waitForTimeout(milliseconds);
  }

  // Get Methods
  async getText(selector: string): Promise<string> {
    const text = await this.page.locator(selector).textContent() || '';
    this.logger.logAction(`Get text: '${text}'`, selector);
    return text;
  }

  async getValue(selector: string): Promise<string> {
    const value = await this.page.locator(selector).inputValue();
    this.logger.logAction(`Get value: '${value}'`, selector);
    return value;
  }

  async getAttribute(selector: string, attribute: string): Promise<string | null> {
    const value = await this.page.locator(selector).getAttribute(attribute);
    this.logger.logAction(`Get attribute '${attribute}': '${value}'`, selector);
    return value;
  }

  async getTitle(): Promise<string> {
    const title = await this.page.title();
    this.logger.logAction(`Get page title: '${title}'`);
    return title;
  }

  async getCurrentUrl(): Promise<string> {
    const url = this.page.url();
    this.logger.logAction(`Get current URL: '${url}'`);
    return url;
  }

  // Element State Methods
  async isVisible(selector: string): Promise<boolean> {
    return await this.page.locator(selector).isVisible();
  }

  async isHidden(selector: string): Promise<boolean> {
    return await this.page.locator(selector).isHidden();
  }

  async isEnabled(selector: string): Promise<boolean> {
    return await this.page.locator(selector).isEnabled();
  }

  async isDisabled(selector: string): Promise<boolean> {
    return await this.page.locator(selector).isDisabled();
  }

  async getElementCount(selector: string): Promise<number> {
    const count = await this.page.locator(selector).count();
    this.logger.logAction(`Element count: ${count}`, selector);
    return count;
  }

  // File Upload Methods
  async uploadFile(selector: string, filePath: string): Promise<void> {
    this.logger.logAction(`Upload file: ${filePath}`, selector);
    await this.page.locator(selector).setInputFiles(filePath);
  }

  async uploadMultipleFiles(selector: string, filePaths: string[]): Promise<void> {
    this.logger.logAction(`Upload multiple files: ${filePaths.join(', ')}`, selector);
    await this.page.locator(selector).setInputFiles(filePaths);
  }

  // Screenshot Methods
  async takeScreenshot(fileName?: string): Promise<string> {
    const screenshotDir = './Reports/screenshots';
    if (!fs.existsSync(screenshotDir)) {
      fs.mkdirSync(screenshotDir, { recursive: true });
    }
    
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const screenshotName = fileName || `screenshot-${timestamp}.png`;
    const screenshotPath = path.join(screenshotDir, screenshotName);
    
    await this.page.screenshot({ path: screenshotPath, fullPage: true });
    this.logger.logAction(`Screenshot saved: ${screenshotPath}`);
    return screenshotPath;
  }

  async takeElementScreenshot(selector: string, fileName?: string): Promise<string> {
    const screenshotDir = './Reports/screenshots';
    if (!fs.existsSync(screenshotDir)) {
      fs.mkdirSync(screenshotDir, { recursive: true });
    }
    
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const screenshotName = fileName || `element-screenshot-${timestamp}.png`;
    const screenshotPath = path.join(screenshotDir, screenshotName);
    
    await this.page.locator(selector).screenshot({ path: screenshotPath });
    this.logger.logAction(`Element screenshot saved: ${screenshotPath}`, selector);
    return screenshotPath;
  }

  // Assertion Helper Methods
  async assertElementVisible(selector: string, message?: string): Promise<void> {
    this.logger.logAction('Assert element visible', selector);
    await expect(this.page.locator(selector), message).toBeVisible();
  }

  async assertElementHidden(selector: string, message?: string): Promise<void> {
    this.logger.logAction('Assert element hidden', selector);
    await expect(this.page.locator(selector), message).toBeHidden();
  }

  async assertElementEnabled(selector: string, message?: string): Promise<void> {
    this.logger.logAction('Assert element enabled', selector);
    await expect(this.page.locator(selector), message).toBeEnabled();
  }

  async assertElementDisabled(selector: string, message?: string): Promise<void> {
    this.logger.logAction('Assert element disabled', selector);
    await expect(this.page.locator(selector), message).toBeDisabled();
  }

  async assertText(selector: string, expectedText: string, message?: string): Promise<void> {
    this.logger.logAction(`Assert text equals '${expectedText}'`, selector);
    await expect(this.page.locator(selector), message).toHaveText(expectedText);
  }

  async assertTextContains(selector: string, expectedText: string, message?: string): Promise<void> {
    this.logger.logAction(`Assert text contains '${expectedText}'`, selector);
    await expect(this.page.locator(selector), message).toContainText(expectedText);
  }

  async assertValue(selector: string, expectedValue: string, message?: string): Promise<void> {
    this.logger.logAction(`Assert value equals '${expectedValue}'`, selector);
    await expect(this.page.locator(selector), message).toHaveValue(expectedValue);
  }

  async assertTitle(expectedTitle: string, message?: string): Promise<void> {
    this.logger.logAction(`Assert page title equals '${expectedTitle}'`);
    await expect(this.page, message).toHaveTitle(expectedTitle);
  }

  async assertUrl(expectedUrl: string | RegExp, message?: string): Promise<void> {
    this.logger.logAction(`Assert URL equals '${expectedUrl}'`);
    await expect(this.page, message).toHaveURL(expectedUrl);
  }

  async assertElementCount(selector: string, expectedCount: number, message?: string): Promise<void> {
    this.logger.logAction(`Assert element count equals ${expectedCount}`, selector);
    await expect(this.page.locator(selector), message).toHaveCount(expectedCount);
  }

  // Scroll Methods
  async scrollToElement(selector: string): Promise<void> {
    this.logger.logAction('Scroll to element', selector);
    await this.page.locator(selector).scrollIntoViewIfNeeded();
  }

  async scrollToTop(): Promise<void> {
    this.logger.logAction('Scroll to top');
    await this.page.evaluate(() => window.scrollTo(0, 0));
  }

  async scrollToBottom(): Promise<void> {
    this.logger.logAction('Scroll to bottom');
    await this.page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  }

  // Drag and Drop
  async dragAndDrop(sourceSelector: string, targetSelector: string): Promise<void> {
    this.logger.logAction(`Drag and drop from '${sourceSelector}' to '${targetSelector}'`);
    await this.page.locator(sourceSelector).dragTo(this.page.locator(targetSelector));
  }

  // Hover Methods
  async hover(selector: string): Promise<void> {
    this.logger.logAction('Hover', selector);
    await this.page.locator(selector).hover();
  }

  // Focus Methods
  async focus(selector: string): Promise<void> {
    this.logger.logAction('Focus', selector);
    await this.page.locator(selector).focus();
  }

  // Alert/Dialog Methods
  async acceptAlert(): Promise<void> {
    this.logger.logAction('Accept alert');
    this.page.on('dialog', dialog => dialog.accept());
  }

  async dismissAlert(): Promise<void> {
    this.logger.logAction('Dismiss alert');
    this.page.on('dialog', dialog => dialog.dismiss());
  }

  async getAlertText(): Promise<string> {
    return new Promise((resolve) => {
      this.page.on('dialog', dialog => {
        const message = dialog.message();
        this.logger.logAction(`Alert text: '${message}'`);
        resolve(message);
        dialog.accept();
      });
    });
  }

  // Frame Methods
  async switchToFrame(frameSelector: string): Promise<void> {
    this.logger.logAction('Switch to frame', frameSelector);
    const frame = this.page.frameLocator(frameSelector);
    // Return frame for chaining operations
    return frame as any;
  }

  // Cookie Methods
  async addCookie(name: string, value: string, domain?: string): Promise<void> {
    this.logger.logAction(`Add cookie: ${name}=${value}`);
    await this.page.context().addCookies([{
      name,
      value,
      domain: domain || new URL(this.page.url()).hostname,
      path: '/'
    }]);
  }

  async getCookies(): Promise<any[]> {
    const cookies = await this.page.context().cookies();
    this.logger.logAction(`Retrieved ${cookies.length} cookies`);
    return cookies;
  }

  async clearCookies(): Promise<void> {
    this.logger.logAction('Clear all cookies');
    await this.page.context().clearCookies();
  }

  // Local Storage Methods
  async setLocalStorage(key: string, value: string): Promise<void> {
    this.logger.logAction(`Set localStorage: ${key}=${value}`);
    await this.page.evaluate(([k, v]) => localStorage.setItem(k, v), [key, value]);
  }

  async getLocalStorage(key: string): Promise<string | null> {
    const value = await this.page.evaluate(k => localStorage.getItem(k), key);
    this.logger.logAction(`Get localStorage '${key}': '${value}'`);
    return value;
  }

  async clearLocalStorage(): Promise<void> {
    this.logger.logAction('Clear localStorage');
    await this.page.evaluate(() => localStorage.clear());
  }
}