import { test, expect } from '@playwright/test';
import { PlaywrightUtils } from '../Utils/PlaywrightUtils';
import { ConfigManager } from '../Utils/ConfigManager';
import { Logger } from '../Utils/Logger';
import { AzureMLUtils } from '../Utils/AzureMLUtils';

test.describe('Azure ML Workspace Management', () => {
  let utils: PlaywrightUtils;
  let azureMLUtils: AzureMLUtils;
  let config: ConfigManager;
  let logger: Logger;

  test.beforeEach(async ({ page }) => {
    utils = new PlaywrightUtils(page);
    azureMLUtils = new AzureMLUtils(page);
    config = ConfigManager.getInstance();
    logger = Logger.getInstance();
    
    logger.logInfo('🚀 Starting Azure ML Workspace test setup');
    
    // Setup data scientist context
    logger.logStep('Setup data scientist context');
    logger.logInfo('Data scientist: Javed');
    logger.logInfo('PIM role: Data Scientist (activated)');
  });

  test.afterEach(async ({ page }) => {
    logger.logInfo('🧹 Cleaning up after test');
    
    // Take final screenshot
    await utils.takeScreenshot('test-cleanup');
    
    // Stop any running compute instances
    try {
      await azureMLUtils.stopAllComputeInstances();
    } catch (error) {
      logger.logWarning(`Failed to stop compute instances during cleanup: ${error}`);
    }
    
    logger.logInfo('✅ Test cleanup completed');
  });

  test('should access Azure ML Workspace', async ({ page }) => {
    logger.logInfo('🎯 Test: Access Azure ML Workspace');
    
    // Step 1: Navigate to Azure ML workspace
    logger.logStep('Navigate to Azure ML workspace');
    const workspaceName = config.getAzureSettings().WorkspaceName;
    await azureMLUtils.navigateToWorkspace(workspaceName);
    
    // Step 2: Handle authentication if required
    logger.logStep('Handle authentication if required');
    await azureMLUtils.handleAuthenticationIfRequired();
    
    // Step 3: Verify workspace access
    logger.logStep('Verify workspace access');
    await azureMLUtils.verifyWorkspaceAccess(workspaceName);
    
    // Step 4: Verify workspace is available
    logger.logStep('Verify workspace is available');
    await azureMLUtils.verifyWorkspaceAvailable();
    
    // Take screenshot of successful access
    await utils.takeScreenshot('workspace-access-success');
    
    logger.logInfo('✅ Successfully accessed Azure ML workspace');
  });

  test('should start compute instance', async ({ page }) => {
    logger.logInfo('🎯 Test: Start Compute Instance');
    
    // Step 1: Open workspace
    logger.logStep('Open workspace');
    const workspaceName = config.getAzureSettings().WorkspaceName;
    await azureMLUtils.navigateToWorkspace(workspaceName);
    await azureMLUtils.handleAuthenticationIfRequired();
    
    // Step 2: Navigate to compute section
    logger.logStep('Navigate to compute section');
    await azureMLUtils.navigateToComputeSection();
    
    // Step 3: Start compute instance
    logger.logStep('Start compute instance');
    const computeName = 'test-compute';
    await azureMLUtils.startComputeInstance(computeName);
    
    // Step 4: Verify compute instance is running
    logger.logStep('Verify compute instance is running');
    await azureMLUtils.verifyComputeInstanceStatus(computeName, 'Running');
    
    // Step 5: Verify connectivity
    logger.logStep('Verify compute instance connectivity');
    await azureMLUtils.verifyComputeInstanceConnectivity(computeName);
    
    // Take screenshot of running compute
    await utils.takeScreenshot('compute-instance-running');
    
    logger.logInfo('✅ Successfully started and verified compute instance');
  });

  test('should stop compute instance', async ({ page }) => {
    logger.logInfo('🎯 Test: Stop Compute Instance');
    
    // Step 1: Open workspace and ensure compute is running
    logger.logStep('Setup: Ensure compute instance is running');
    const workspaceName = config.getAzureSettings().WorkspaceName;
    const computeName = 'test-compute';
    
    await azureMLUtils.navigateToWorkspace(workspaceName);
    await azureMLUtils.handleAuthenticationIfRequired();
    await azureMLUtils.navigateToComputeSection();
    
    // Ensure compute is running first
    await azureMLUtils.ensureComputeInstanceRunning(computeName);
    
    // Step 2: Stop compute instance
    logger.logStep('Stop compute instance');
    await azureMLUtils.stopComputeInstance(computeName);
    
    // Step 3: Verify compute instance is stopped
    logger.logStep('Verify compute instance is stopped');
    await azureMLUtils.verifyComputeInstanceStatus(computeName, 'Stopped');
    
    // Take screenshot of stopped compute
    await utils.takeScreenshot('compute-instance-stopped');
    
    logger.logInfo('✅ Successfully stopped compute instance');
  });

  test('should manage multiple compute instances', async ({ page }) => {
    logger.logInfo('🎯 Test: Manage Multiple Compute Instances');
    
    // Step 1: Open workspace
    logger.logStep('Open workspace');
    const workspaceName = config.getAzureSettings().WorkspaceName;
    await azureMLUtils.navigateToWorkspace(workspaceName);
    await azureMLUtils.handleAuthenticationIfRequired();
    await azureMLUtils.navigateToComputeSection();
    
    // Step 2: Start multiple compute instances
    logger.logStep('Start multiple compute instances');
    const computeInstances = ['test-compute-1', 'test-compute-2'];
    
    for (const computeName of computeInstances) {
      logger.logAction(`Starting compute instance: ${computeName}`);
      await azureMLUtils.startComputeInstance(computeName);
    }
    
    // Step 3: Verify all instances are running
    logger.logStep('Verify all compute instances are running');
    for (const computeName of computeInstances) {
      await azureMLUtils.verifyComputeInstanceStatus(computeName, 'Running');
    }
    
    // Take screenshot of multiple running instances
    await utils.takeScreenshot('multiple-compute-instances-running');
    
    // Step 4: Stop all compute instances
    logger.logStep('Stop all compute instances');
    for (const computeName of computeInstances) {
      logger.logAction(`Stopping compute instance: ${computeName}`);
      await azureMLUtils.stopComputeInstance(computeName);
    }
    
    // Step 5: Verify all instances are stopped
    logger.logStep('Verify all compute instances are stopped');
    for (const computeName of computeInstances) {
      await azureMLUtils.verifyComputeInstanceStatus(computeName, 'Stopped');
    }
    
    // Take screenshot of all stopped instances
    await utils.takeScreenshot('multiple-compute-instances-stopped');
    
    logger.logInfo('✅ Successfully managed multiple compute instances');
  });

  test('should integrate with VS Code Desktop', async ({ page }) => {
    logger.logInfo('🎯 Test: Azure ML Workspace with VS Code Desktop Integration');
    
    // Step 1: Verify Contributor access and navigate to workspace
    logger.logStep('Navigate to Azure ML workspace with Contributor access');
    await azureMLUtils.navigateToWorkspaceUrl('https://ml.azure.com/workspaces');
    
    // Step 2: Handle login if required
    logger.logStep('Handle login if required');
    await azureMLUtils.handleLoginIfRequired('Javed Khan');
    
    // Step 3: Select workspace
    logger.logStep('Select workspace');
    await azureMLUtils.selectWorkspace('CTO-workspace');
    
    // Step 4: Choose compute option
    logger.logStep('Choose compute option');
    await azureMLUtils.chooseComputeOption();
    
    // Step 5: Open specific compute
    logger.logStep('Open compute instance');
    const computeName = 'com-jk';
    await azureMLUtils.openCompute(computeName);
    
    // Step 6: Start compute if not running
    logger.logStep('Start compute if not running');
    await azureMLUtils.startComputeIfNotRunning(computeName);
    
    // Step 7: Check if application links are enabled
    logger.logStep('Check if application links are enabled');
    const linksEnabled = await azureMLUtils.checkApplicationLinksEnabled();
    logger.logInfo(`Application links enabled: ${linksEnabled}`);
    
    // Take screenshot of workspace with application links
    await utils.takeScreenshot('workspace-application-links');
    
    // Step 8: Start VS Code Desktop
    logger.logStep('Start VS Code Desktop');
    await azureMLUtils.startVSCodeDesktop();
    
    // Step 9: Verify VS Code interaction
    logger.logStep('Verify VS Code Desktop interaction');
    const vsCodeInteractive = await azureMLUtils.verifyVSCodeInteraction();
    
    if (!vsCodeInteractive) {
      throw new Error('VS Code Desktop is not interactive or not responding properly');
    }
    
    // Take screenshot of VS Code Desktop integration
    await utils.takeScreenshot('vscode-desktop-integration');
    
    logger.logInfo('✅ Successfully integrated with VS Code Desktop');
  });
});