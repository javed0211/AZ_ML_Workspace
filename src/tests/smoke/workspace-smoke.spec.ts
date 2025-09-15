import { test, expect } from '@playwright/test';
import { AzureMLStudioPage } from '../../pages/azure-ml-studio';
import { logTestStart, logTestEnd } from '../../helpers/logger';
import { config } from '../../helpers/config';

test.describe('Azure ML Workspace Smoke Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to Azure ML Studio
    await page.goto(config.urls.base);
  });

  test('should load workspace successfully @smoke', async ({ page }) => {
    const testLogger = logTestStart('Workspace Load Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      
      // Wait for page to load
      await azureMLPage.waitForPageLoad();
      
      // Verify workspace is loaded
      await azureMLPage.assertWorkspaceLoaded(config.azure.workspaceName);
      
      // Verify main navigation elements are present
      await azureMLPage.assertElementVisible('[data-testid="navigation-menu"]');
      await azureMLPage.assertElementVisible('[data-testid="workspace-name"]');
      
      // Take screenshot for verification
      await azureMLPage.takeScreenshot('workspace-loaded');
      
      testLogger.info('Workspace loaded successfully');
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Workspace load test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should navigate to all main sections @smoke', async ({ page }) => {
    const testLogger = logTestStart('Navigation Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Test navigation to each main section
      const sections = [
        { name: 'Compute', method: 'navigateToCompute' },
        { name: 'Notebooks', method: 'navigateToNotebooks' },
        { name: 'Jobs', method: 'navigateToJobs' },
        { name: 'Data', method: 'navigateToData' },
        { name: 'Models', method: 'navigateToModels' },
        { name: 'Endpoints', method: 'navigateToEndpoints' },
      ];
      
      for (const section of sections) {
        testLogger.step(`Navigating to ${section.name}`);
        await (azureMLPage as any)[section.method]();
        
        // Verify section loaded
        await page.waitForTimeout(2000); // Allow time for section to load
        await azureMLPage.takeScreenshot(`${section.name.toLowerCase()}-section`);
        
        testLogger.info(`Successfully navigated to ${section.name}`);
      }
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Navigation test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should display workspace information @smoke', async ({ page }) => {
    const testLogger = logTestStart('Workspace Information Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Get workspace name
      const workspaceName = await azureMLPage.getWorkspaceName();
      testLogger.info('Workspace name retrieved', { workspaceName });
      
      // Verify workspace name matches configuration
      expect(workspaceName).toBe(config.azure.workspaceName);
      
      // Verify page title contains workspace name
      await azureMLPage.assertPageTitle(new RegExp(workspaceName, 'i'));
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Workspace information test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should handle authentication properly @smoke', async ({ page }) => {
    const testLogger = logTestStart('Authentication Test');
    
    try {
      // This test verifies that we can access the workspace
      // which implies authentication is working
      
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Verify we're not on a login page
      const currentUrl = page.url();
      testLogger.info('Current URL', { url: currentUrl });
      
      // Should not be redirected to login
      expect(currentUrl).not.toContain('login');
      expect(currentUrl).not.toContain('signin');
      
      // Should be able to see workspace content
      await azureMLPage.assertElementVisible('[data-testid="workspace-name"]');
      
      testLogger.info('Authentication verified successfully');
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Authentication test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should load without errors @smoke', async ({ page }) => {
    const testLogger = logTestStart('Error-free Load Test');
    
    try {
      // Listen for console errors
      const consoleErrors: string[] = [];
      page.on('console', msg => {
        if (msg.type() === 'error') {
          consoleErrors.push(msg.text());
        }
      });
      
      // Listen for page errors
      const pageErrors: Error[] = [];
      page.on('pageerror', error => {
        pageErrors.push(error);
      });
      
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Wait a bit more to catch any delayed errors
      await page.waitForTimeout(5000);
      
      // Check for errors
      if (consoleErrors.length > 0) {
        testLogger.warn('Console errors detected', { errors: consoleErrors });
      }
      
      if (pageErrors.length > 0) {
        testLogger.error('Page errors detected', { errors: pageErrors.map(e => e.message) });
        throw new Error(`Page errors detected: ${pageErrors.map(e => e.message).join(', ')}`);
      }
      
      // Verify no error messages are displayed on the page
      const errorElements = await page.locator('[data-testid="error-message"]').count();
      expect(errorElements).toBe(0);
      
      testLogger.info('Page loaded without errors');
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Error-free load test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });
});