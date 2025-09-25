import { test, expect } from '@playwright/test';
import { PlaywrightUtils } from '../Utils/PlaywrightUtils';
import { Logger } from '../Utils/Logger';

test.describe('Google Search Tests', () => {
  let utils: PlaywrightUtils;
  let logger: Logger;

  test.beforeEach(async ({ page }) => {
    utils = new PlaywrightUtils(page);
    logger = Logger.getInstance();
    
    logger.logTestStart('Google Search Test Setup');
  });

  test.afterEach(async ({ page }, testInfo) => {
    const status = testInfo.status === 'passed' ? 'PASSED' : 'FAILED';
    logger.logTestEnd(testInfo.title, status);
    
    if (testInfo.status === 'failed') {
      await utils.takeScreenshot(`failed-${testInfo.title.replace(/\s+/g, '-')}`);
    }
  });

  test('should search for Marshall headphones on Google', async ({ page }) => {
    logger.logStep('Navigate to Google homepage');
    await utils.navigateTo('https://www.google.com');
    
    logger.logStep('Verify Google homepage loaded');
    await utils.assertTitle(/Google/);
    
    logger.logStep('Accept cookies if present');
    // Handle cookie consent if it appears
    try {
      await utils.waitForElement('[id*="accept"], [id*="agree"], button:has-text("Accept all")', 3000);
      await utils.click('[id*="accept"], [id*="agree"], button:has-text("Accept all")');
      logger.info('Cookie consent accepted');
    } catch (error) {
      logger.info('No cookie consent dialog found, continuing...');
    }
    
    logger.logStep('Find and click on search input');
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
    
    logger.logStep('Type search query: marshall headphones');
    await utils.fill(searchInput, 'marshall headphones');
    
    logger.logStep('Press Enter to search');
    await utils.pressKeyOnElement(searchInput, 'Enter');
    
    logger.logStep('Wait for search results to load');
    await utils.waitForUrl(/search/);
    
    logger.logStep('Verify search results are displayed');
    // Wait for search results container
    await utils.waitForElement('#search, #rso, [data-testid="search-results"]');
    
    logger.logStep('Verify search results contain Marshall');
    const resultsText = await utils.getText('#search, #rso, [data-testid="search-results"]');
    expect(resultsText.toLowerCase()).toContain('marshall');
    
    logger.logStep('Take screenshot of search results');
    await utils.takeScreenshot('google-search-marshall-headphones');
    
    logger.logStep('Verify at least one search result is visible');
    const resultCount = await utils.getElementCount('h3, [data-testid="result-title"]');
    expect(resultCount).toBeGreaterThan(0);
    
    logger.info('Google search test completed successfully');
  });

  test('should search and click on first Marshall result', async ({ page }) => {
    logger.logStep('Navigate to Google homepage');
    await utils.navigateTo('https://www.google.com');
    
    logger.logStep('Handle cookie consent if present');
    try {
      await utils.waitForElement('[id*="accept"], [id*="agree"], button:has-text("Accept all")', 3000);
      await utils.click('[id*="accept"], [id*="agree"], button:has-text("Accept all")');
    } catch (error) {
      logger.info('No cookie consent dialog found');
    }
    
    logger.logStep('Search for Marshall headphones');
    const searchInput = 'input[name="q"], textarea[name="q"], #APjFqb';
    await utils.waitForElement(searchInput);
    await utils.fill(searchInput, 'marshall headphones');
    await utils.pressKeyOnElement(searchInput, 'Enter');
    
    logger.logStep('Wait for search results');
    await utils.waitForUrl(/search/);
    await utils.waitForElement('#search, #rso');
    
    logger.logStep('Find and click first Marshall-related result');
    // Look for the first result that contains "Marshall" in the title
    const firstResult = 'h3:has-text("Marshall"), [data-testid="result-title"]:has-text("Marshall")';
    
    try {
      await utils.waitForElement(firstResult, 5000);
      await utils.takeScreenshot('before-clicking-result');
      
      // Get the text of the first result for logging
      const resultText = await utils.getText(firstResult);
      logger.logAction(`Clicking on result: ${resultText}`);
      
      await utils.click(firstResult);
      
      logger.logStep('Wait for new page to load');
      await utils.waitForLoadState('networkidle');
      
      logger.logStep('Verify we navigated to a Marshall-related page');
      const currentUrl = await utils.getCurrentUrl();
      const pageTitle = await utils.getTitle();
      
      logger.info(`Navigated to: ${currentUrl}`);
      logger.info(`Page title: ${pageTitle}`);
      
      // Take final screenshot
      await utils.takeScreenshot('marshall-page-loaded');
      
      // Verify we're on a relevant page (URL or title should contain Marshall)
      const isMarshallPage = currentUrl.toLowerCase().includes('marshall') || 
                            pageTitle.toLowerCase().includes('marshall');
      
      expect(isMarshallPage).toBeTruthy();
      
    } catch (error) {
      logger.error('Could not find or click Marshall result', error);
      await utils.takeScreenshot('marshall-result-not-found');
      throw error;
    }
    
    logger.info('Marshall headphones search and navigation test completed successfully');
  });
});