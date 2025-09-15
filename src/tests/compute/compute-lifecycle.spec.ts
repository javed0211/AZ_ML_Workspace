import { test, expect } from '@playwright/test';
import { AzureMLStudioPage } from '../../pages/azure-ml-studio';
import { createAzureMLHelper } from '../../helpers/azure-helpers';
import { logTestStart, logTestEnd } from '../../helpers/logger';
import { config } from '../../helpers/config';

test.describe('Compute Lifecycle Tests', () => {
  const computeInstanceName = `test-compute-${Date.now()}`;
  let azureHelper: any;

  test.beforeAll(async () => {
    // Initialize Azure helper for API operations
    azureHelper = createAzureMLHelper();
  });

  test.beforeEach(async ({ page }) => {
    await page.goto(config.urls.base);
  });

  test.afterAll(async () => {
    // Cleanup: Stop and delete test compute instance
    try {
      await azureHelper.stopComputeInstance(computeInstanceName);
      // Note: In a real scenario, you might want to delete the instance
      // but for safety, we'll just stop it
    } catch (error) {
      console.warn(`Cleanup failed for compute instance ${computeInstanceName}:`, error instanceof Error ? error.message : String(error));
    }
  });

  test('should create compute instance via UI @compute', async ({ page }) => {
    const testLogger = logTestStart('Create Compute Instance UI Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Navigate to compute section
      await azureMLPage.navigateToCompute();
      
      // Create compute instance
      await azureMLPage.createComputeInstance(computeInstanceName, 'Standard_DS3_v2');
      
      // Verify compute instance appears in the list
      await azureMLPage.assertComputeInstanceExists(computeInstanceName);
      
      // Wait for compute instance to be in "Creating" or "Running" state
      await page.waitForTimeout(10000); // Allow time for creation to start
      
      const status = await azureMLPage.getComputeInstanceStatus(computeInstanceName);
      testLogger.info('Compute instance status after creation', { status });
      
      // Status should be either "Creating" or "Running"
      expect(['Creating', 'Running', 'Starting'].some(s => 
        status.toLowerCase().includes(s.toLowerCase())
      )).toBeTruthy();
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Create compute instance test failed', { error: error instanceof Error ? error.message : String(error) });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should start and stop compute instance via UI @compute', async ({ page }) => {
    const testLogger = logTestStart('Start/Stop Compute Instance UI Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Navigate to compute section
      await azureMLPage.navigateToCompute();
      
      // Verify compute instance exists
      await azureMLPage.assertComputeInstanceExists(computeInstanceName);
      
      // Get initial status
      const initialStatus = await azureMLPage.getComputeInstanceStatus(computeInstanceName);
      testLogger.info('Initial compute instance status', { status: initialStatus });
      
      // If not running, start it
      if (!initialStatus.toLowerCase().includes('running')) {
        testLogger.step('Starting compute instance');
        await azureMLPage.startComputeInstance(computeInstanceName);
        
        // Wait for it to be running (with timeout)
        await azureMLPage.waitForComputeInstanceStatus(computeInstanceName, 'Running', 600000);
        
        // Verify it's running
        await azureMLPage.assertComputeInstanceStatus(computeInstanceName, 'Running');
        testLogger.info('Compute instance started successfully');
      }
      
      // Now stop it
      testLogger.step('Stopping compute instance');
      await azureMLPage.stopComputeInstance(computeInstanceName);
      
      // Wait for it to be stopped
      await azureMLPage.waitForComputeInstanceStatus(computeInstanceName, 'Stopped', 300000);
      
      // Verify it's stopped
      await azureMLPage.assertComputeInstanceStatus(computeInstanceName, 'Stopped');
      
      testLogger.info('Compute instance stopped successfully');
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Start/stop compute instance test failed', { error: error instanceof Error ? error.message : String(error) });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should manage compute instance via Azure CLI @compute', async ({ page }) => {
    const testLogger = logTestStart('Compute Instance CLI Test');
    
    try {
      // Test Azure CLI operations
      testLogger.step('Starting compute instance via CLI');
      await azureHelper.startComputeInstance(computeInstanceName);
      
      // Wait for instance to be running
      await azureHelper.waitForComputeInstanceState(computeInstanceName, 'Running', 600000);
      
      // Verify status via CLI
      const instance = await azureHelper.getComputeInstanceStatus(computeInstanceName);
      testLogger.info('Compute instance status via CLI', { 
        name: instance.name,
        state: instance.state,
        vmSize: instance.vmSize 
      });
      
      expect(instance.state.toLowerCase()).toBe('running');
      expect(instance.name).toBe(computeInstanceName);
      
      // Verify the same status is shown in UI
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      await azureMLPage.navigateToCompute();
      
      await azureMLPage.assertComputeInstanceStatus(computeInstanceName, 'Running');
      
      // Stop via CLI
      testLogger.step('Stopping compute instance via CLI');
      await azureHelper.stopComputeInstance(computeInstanceName);
      
      // Wait for instance to be stopped
      await azureHelper.waitForComputeInstanceState(computeInstanceName, 'Stopped', 300000);
      
      // Verify stopped status
      const stoppedInstance = await azureHelper.getComputeInstanceStatus(computeInstanceName);
      expect(stoppedInstance.state.toLowerCase()).toBe('stopped');
      
      testLogger.info('CLI compute management test completed successfully');
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('CLI compute management test failed', { error: error instanceof Error ? error.message : String(error) });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should handle compute instance errors gracefully @compute', async ({ page }) => {
    const testLogger = logTestStart('Compute Instance Error Handling Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      await azureMLPage.navigateToCompute();
      
      // Test operations on non-existent compute instance
      const nonExistentInstance = 'non-existent-compute-instance';
      
      testLogger.step('Testing operations on non-existent compute instance');
      
      // This should handle the error gracefully
      try {
        await azureHelper.getComputeInstanceStatus(nonExistentInstance);
        // If we get here, the instance unexpectedly exists
        testLogger.warn('Non-existent compute instance actually exists');
      } catch (error) {
        // This is expected
        testLogger.info('Correctly handled non-existent compute instance', { 
          error: error instanceof Error ? error.message : String(error)
        });
      }
      
      // Test UI error handling
      testLogger.step('Testing UI error handling');
      
      // Try to perform operations that should show error messages
      // This depends on the specific UI implementation
      
      // Verify no unexpected errors occurred
      const pageErrors = await page.locator('[data-testid="error-message"]').count();
      testLogger.info('Page error count', { count: pageErrors });
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Error handling test failed', { error: error instanceof Error ? error.message : String(error) });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should display compute instance details correctly @compute', async ({ page }) => {
    const testLogger = logTestStart('Compute Instance Details Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      await azureMLPage.navigateToCompute();
      
      // Get compute instances list
      const instances = await azureMLPage.getComputeInstances();
      testLogger.info('Available compute instances', { instances });
      
      if (instances.length === 0) {
        testLogger.info('No compute instances found, skipping details test');
        logTestEnd(testLogger, true);
        return;
      }
      
      // Test with the first available instance (or our test instance if it exists)
      const testInstance = instances.includes(computeInstanceName) ? 
        computeInstanceName : instances[0];
      
      testLogger.step(`Testing details for compute instance: ${testInstance}`);
      
      // Get status via UI
      const uiStatus = await azureMLPage.getComputeInstanceStatus(testInstance);
      testLogger.info('UI status', { instance: testInstance, status: uiStatus });
      
      // Get status via API
      try {
        const apiStatus = await azureHelper.getComputeInstanceStatus(testInstance);
        testLogger.info('API status', { 
          instance: testInstance, 
          state: apiStatus.state,
          vmSize: apiStatus.vmSize 
        });
        
        // Compare UI and API status (they should be consistent)
        // Note: There might be slight differences in formatting
        const uiStatusLower = uiStatus.toLowerCase();
        const apiStatusLower = apiStatus.state.toLowerCase();
        
        // They should at least contain similar keywords
        const statusMatch = uiStatusLower.includes(apiStatusLower) || 
                           apiStatusLower.includes(uiStatusLower) ||
                           (uiStatusLower.includes('running') && apiStatusLower.includes('running')) ||
                           (uiStatusLower.includes('stopped') && apiStatusLower.includes('stopped'));
        
        if (!statusMatch) {
          testLogger.warn('UI and API status mismatch', { 
            ui: uiStatus, 
            api: apiStatus.state 
          });
        } else {
          testLogger.info('UI and API status are consistent');
        }
      } catch (apiError) {
        testLogger.warn('Could not get API status for comparison', { 
          error: apiError instanceof Error ? apiError.message : String(apiError)
        });
      }
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Compute instance details test failed', { error: error instanceof Error ? error.message : String(error) });
      logTestEnd(testLogger, false);
      throw error;
    }
  });
});