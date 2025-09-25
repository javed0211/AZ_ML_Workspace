import { test, expect } from '@playwright/test';
import { PlaywrightUtils } from '../Utils/PlaywrightUtils';
import { Logger } from '../Utils/Logger';
import { ConfigManager } from '../Utils/ConfigManager';

test.describe('Web Application Tests', () => {
  let utils: PlaywrightUtils;
  let logger: Logger;
  let config: ConfigManager;

  test.beforeEach(async ({ page }) => {
    utils = new PlaywrightUtils(page);
    logger = Logger.getInstance();
    config = ConfigManager.getInstance();
    
    logger.logTestStart('Web Application Test Setup');
  });

  test.afterEach(async ({ page }, testInfo) => {
    const status = testInfo.status === 'passed' ? 'PASSED' : 'FAILED';
    logger.logTestEnd(testInfo.title, status);
    
    if (testInfo.status === 'failed') {
      await utils.takeScreenshot(`failed-${testInfo.title.replace(/\s+/g, '-')}`);
    }
  });

  test('should navigate to homepage and verify title', async ({ page }) => {
    logger.logStep('Navigate to homepage');
    await utils.navigateTo(config.getCurrentEnvironment().BaseUrl);
    
    logger.logStep('Verify page title');
    const title = await utils.getTitle();
    expect(title).toBeTruthy();
    
    logger.logStep('Take screenshot of homepage');
    await utils.takeScreenshot('homepage');
  });

  test('should perform login flow', async ({ page }) => {
    const env = config.getCurrentEnvironment();
    
    logger.logStep('Navigate to login page');
    await utils.navigateTo(`${env.BaseUrl}/login`);
    
    logger.logStep('Fill login credentials');
    await utils.fill('#username', env.Username);
    await utils.fill('#password', env.Password);
    
    logger.logStep('Click login button');
    await utils.click('#login-button');
    
    logger.logStep('Wait for dashboard to load');
    await utils.waitForUrl(/dashboard/);
    
    logger.logStep('Verify successful login');
    await utils.assertElementVisible('.dashboard-header', 'Dashboard should be visible after login');
  });

  test('should test form interactions', async ({ page }) => {
    logger.logStep('Navigate to form page');
    await utils.navigateTo(`${config.getCurrentEnvironment().BaseUrl}/form`);
    
    logger.logStep('Fill text input');
    await utils.type('#name', 'John Doe');
    
    logger.logStep('Select dropdown option');
    await utils.selectByText('#country', 'United States');
    
    logger.logStep('Check checkbox');
    await utils.check('#agree-terms');
    
    logger.logStep('Upload file');
    await utils.uploadFile('#file-upload', './TestData/sample-file.txt');
    
    logger.logStep('Submit form');
    await utils.click('#submit-button');
    
    logger.logStep('Verify form submission');
    await utils.assertTextContains('.success-message', 'Form submitted successfully');
  });

  test('should test element interactions and assertions', async ({ page }) => {
    await utils.navigateTo(`${config.getCurrentEnvironment().BaseUrl}/elements`);
    
    // Test visibility assertions
    await utils.assertElementVisible('#visible-element');
    await utils.assertElementHidden('#hidden-element');
    
    // Test text assertions
    await utils.assertText('#welcome-message', 'Welcome to our site!');
    await utils.assertTextContains('#description', 'This is a test');
    
    // Test element states
    await utils.assertElementEnabled('#enabled-button');
    await utils.assertElementDisabled('#disabled-button');
    
    // Test element count
    await utils.assertElementCount('.list-item', 5);
    
    // Test hover and focus
    await utils.hover('#hover-element');
    await utils.focus('#focus-element');
    
    // Test scroll
    await utils.scrollToElement('#bottom-element');
    await utils.scrollToTop();
  });

  test('should test advanced interactions', async ({ page }) => {
    await utils.navigateTo(`${config.getCurrentEnvironment().BaseUrl}/advanced`);
    
    // Test drag and drop
    await utils.dragAndDrop('#draggable', '#droppable');
    
    // Test double click
    await utils.doubleClick('#double-click-element');
    
    // Test right click
    await utils.rightClick('#context-menu-element');
    
    // Test keyboard interactions
    await utils.pressKey('Escape');
    await utils.pressKeyOnElement('#input-field', 'Enter');
    
    // Test alert handling
    await utils.acceptAlert();
    
    // Test local storage
    await utils.setLocalStorage('testKey', 'testValue');
    const value = await utils.getLocalStorage('testKey');
    expect(value).toBe('testValue');
  });

  test('should test responsive design', async ({ page }) => {
    await utils.navigateTo(config.getCurrentEnvironment().BaseUrl);
    
    // Test mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await utils.assertElementVisible('.mobile-menu');
    
    // Test tablet viewport
    await page.setViewportSize({ width: 768, height: 1024 });
    await utils.assertElementVisible('.tablet-layout');
    
    // Test desktop viewport
    await page.setViewportSize({ width: 1920, height: 1080 });
    await utils.assertElementVisible('.desktop-layout');
  });
});