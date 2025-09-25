import { test, expect } from '@playwright/test';
import { ElectronUtils } from '../Utils/ElectronUtils';
import { PlaywrightUtils } from '../Utils/PlaywrightUtils';
import { Logger } from '../Utils/Logger';

test.describe('Electron Application Tests - VS Code @electron', () => {
  let electronUtils: ElectronUtils;
  let logger: Logger;

  test.beforeAll(async () => {
    electronUtils = new ElectronUtils();
    logger = Logger.getInstance();
  });

  test.afterAll(async () => {
    await electronUtils.closeApp();
  });

  test('should launch VS Code and verify main window', async () => {
    logger.logTestStart('VS Code Launch Test');
    
    logger.logStep('Launch VS Code Electron app');
    const electronApp = await electronUtils.launchElectronApp();
    
    logger.logStep('Get main window');
    const mainWindow = await electronUtils.getMainWindow();
    const utils = new PlaywrightUtils(mainWindow);
    
    logger.logStep('Verify VS Code window is visible');
    const isVisible = await electronUtils.isWindowVisible(mainWindow);
    expect(isVisible).toBe(true);
    
    logger.logStep('Get window title');
    const title = await electronUtils.getWindowTitle(mainWindow);
    expect(title).toContain('Visual Studio Code');
    
    logger.logStep('Take screenshot of VS Code');
    await electronUtils.takeScreenshotOfWindow(mainWindow, 'vscode-main-window');
    
    logger.logTestEnd('VS Code Launch Test', 'PASSED');
  });

  test('should interact with VS Code interface', async () => {
    logger.logTestStart('VS Code Interface Test');
    
    const electronApp = await electronUtils.launchElectronApp();
    const mainWindow = await electronUtils.getMainWindow();
    const utils = new PlaywrightUtils(mainWindow);
    
    // Wait for VS Code to fully load
    await utils.waitForLoadState('networkidle');
    await utils.sleep(3000); // Additional wait for VS Code initialization
    
    logger.logStep('Verify VS Code welcome screen or editor');
    // Check for common VS Code elements
    const welcomeTab = await utils.isVisible('[aria-label*="Welcome"]');
    const editorArea = await utils.isVisible('.monaco-editor');
    const activityBar = await utils.isVisible('.activitybar');
    
    // At least one of these should be visible
    expect(welcomeTab || editorArea || activityBar).toBe(true);
    
    if (activityBar) {
      logger.logStep('Activity bar is visible - VS Code loaded successfully');
      await utils.takeElementScreenshot('.activitybar', 'vscode-activity-bar');
    }
    
    logger.logStep('Check for file explorer');
    const explorerIcon = await utils.isVisible('[aria-label*="Explorer"]');
    if (explorerIcon) {
      logger.logStep('Click on Explorer icon');
      await utils.click('[aria-label*="Explorer"]');
      await utils.sleep(1000);
    }
    
    logger.logStep('Verify menu bar');
    const menuBar = await utils.isVisible('.menubar');
    if (menuBar) {
      logger.logStep('Menu bar is visible');
      await utils.assertElementVisible('.menubar', 'Menu bar should be visible');
    }
    
    logger.logTestEnd('VS Code Interface Test', 'PASSED');
  });

  test('should test VS Code menu interactions', async () => {
    logger.logTestStart('VS Code Menu Test');
    
    const electronApp = await electronUtils.launchElectronApp();
    const mainWindow = await electronUtils.getMainWindow();
    const utils = new PlaywrightUtils(mainWindow);
    
    await utils.waitForLoadState('networkidle');
    await utils.sleep(3000);
    
    logger.logStep('Test keyboard shortcuts');
    // Try to open command palette
    await utils.pressKey('Meta+Shift+P'); // Cmd+Shift+P on Mac
    await utils.sleep(1000);
    
    // Check if command palette opened
    const commandPalette = await utils.isVisible('.quick-input-widget');
    if (commandPalette) {
      logger.logStep('Command palette opened successfully');
      await utils.pressKey('Escape'); // Close command palette
    }
    
    logger.logStep('Test file menu access');
    // Try to access file operations through keyboard
    await utils.pressKey('Meta+N'); // Cmd+N for new file on Mac
    await utils.sleep(1000);
    
    logger.logStep('Take final screenshot');
    await electronUtils.takeScreenshotOfWindow(mainWindow, 'vscode-after-interactions');
    
    logger.logTestEnd('VS Code Menu Test', 'PASSED');
  });

  test('should test window management', async () => {
    logger.logTestStart('VS Code Window Management Test');
    
    const electronApp = await electronUtils.launchElectronApp();
    const mainWindow = await electronUtils.getMainWindow();
    
    logger.logStep('Get app information');
    const appInfo = await electronUtils.getAppInfo();
    expect(appInfo.name).toBeTruthy();
    expect(appInfo.version).toBeTruthy();
    logger.info('App Info:', appInfo);
    
    logger.logStep('Test window resize');
    await electronUtils.setWindowSize(mainWindow, 1200, 800);
    await electronUtils.takeScreenshotOfWindow(mainWindow, 'vscode-resized');
    
    logger.logStep('Test window maximize');
    await electronUtils.maximizeWindow(mainWindow);
    await electronUtils.takeScreenshotOfWindow(mainWindow, 'vscode-maximized');
    
    logger.logStep('Focus window');
    await electronUtils.focusWindow(mainWindow);
    
    logger.logStep('Check all windows');
    const allWindows = await electronUtils.getAllWindows();
    expect(allWindows.length).toBeGreaterThanOrEqual(1);
    logger.info(`Total windows: ${allWindows.length}`);
    
    logger.logTestEnd('VS Code Window Management Test', 'PASSED');
  });

  test('should capture console logs and errors', async () => {
    logger.logTestStart('VS Code Console Monitoring Test');
    
    const electronApp = await electronUtils.launchElectronApp();
    const mainWindow = await electronUtils.getMainWindow();
    
    logger.logStep('Setup console log capture');
    await electronUtils.captureConsoleLogs(mainWindow);
    
    logger.logStep('Setup error capture');
    await electronUtils.captureErrors(mainWindow);
    
    // Wait and interact to generate some logs
    await mainWindow.waitForLoadState('networkidle');
    await mainWindow.waitForTimeout(5000);
    
    logger.logStep('Perform some interactions to generate logs');
    const utils = new PlaywrightUtils(mainWindow);
    
    // Try to trigger some VS Code functionality
    await utils.pressKey('Meta+Shift+P');
    await utils.sleep(1000);
    await utils.pressKey('Escape');
    
    logger.logTestEnd('VS Code Console Monitoring Test', 'PASSED');
  });
});