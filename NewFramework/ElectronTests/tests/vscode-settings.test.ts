import { test, expect } from '@playwright/test';
import { VSCodeElectron } from '../utils/vscode-electron';
import { TestHelpers } from '../utils/test-helpers';

/**
 * VS Code settings and configuration tests
 */
test.describe('VS Code Settings and Configuration', () => {
  let vscode: VSCodeElectron;

  test.beforeEach(async () => {
    TestHelpers.logStep('Setting up VS Code for settings testing');
    vscode = new VSCodeElectron();
    await vscode.launch();
    await TestHelpers.setupVSCodeForTesting(vscode);
  });

  test.afterEach(async () => {
    TestHelpers.logStep('Cleaning up VS Code settings test');
    if (vscode.isRunning()) {
      await vscode.close();
    }
    TestHelpers.cleanupTempFiles();
  });

  test('should open and navigate settings', async () => {
    TestHelpers.logStep('Opening VS Code settings');
    
    // Open settings
    await vscode.executeCommand('Preferences: Open Settings (UI)');
    await TestHelpers.wait(2000);
    
    const window = vscode.getMainWindow();
    
    // Verify settings page is open
    const settingsEditor = await window.$('.settings-editor');
    expect(settingsEditor).toBeTruthy();
    
    // Try to search for a setting
    const searchBox = await window.$('.settings-header-widget .suggest-input-container input');
    if (searchBox) {
      await searchBox.click();
      await window.keyboard.type('python');
      await TestHelpers.wait(1000);
    }
    
    // Take screenshot
    await vscode.takeScreenshot();
  });

  test('should modify editor settings', async () => {
    TestHelpers.logStep('Modifying editor settings');
    
    // Open settings JSON
    await vscode.executeCommand('Preferences: Open Settings (JSON)');
    await TestHelpers.wait(2000);
    
    const window = vscode.getMainWindow();
    
    // Check if settings.json is open
    const editor = await window.$('.monaco-editor');
    expect(editor).toBeTruthy();
    
    // Add some test settings
    const settingsContent = `{
    "editor.fontSize": 14,
    "editor.tabSize": 4,
    "editor.insertSpaces": true,
    "editor.wordWrap": "on",
    "python.defaultInterpreterPath": "/usr/bin/python3",
    "files.autoSave": "afterDelay",
    "terminal.integrated.fontSize": 12
}`;
    
    // Clear existing content and add new settings
    const isMac = process.platform === 'darwin';
    const modifier = isMac ? 'Meta' : 'Control';
    
    await window.keyboard.press(`${modifier}+KeyA`);
    await window.keyboard.type(settingsContent);
    await TestHelpers.wait(1000);
    
    // Save settings
    await vscode.saveFile();
    await TestHelpers.wait(1000);
    
    // Take screenshot
    await vscode.takeScreenshot();
  });

  test('should configure Python interpreter', async () => {
    TestHelpers.logStep('Configuring Python interpreter');
    
    // Open command palette and search for Python interpreter
    await vscode.executeCommand('Python: Select Interpreter');
    await TestHelpers.wait(2000);
    
    const window = vscode.getMainWindow();
    
    // Check if interpreter selection is shown
    const quickPick = await window.$('.quick-input-widget');
    if (quickPick) {
      TestHelpers.logStep('Python interpreter selection dialog opened');
      
      // Press Escape to close without selecting
      await window.keyboard.press('Escape');
      await TestHelpers.wait(500);
    } else {
      TestHelpers.logStep('Python extension may not be installed');
    }
    
    // Take screenshot
    await vscode.takeScreenshot();
  });

  test('should manage extensions', async () => {
    TestHelpers.logStep('Managing VS Code extensions');
    
    // Open extensions view
    await vscode.executeCommand('View: Show Extensions');
    await TestHelpers.wait(2000);
    
    const window = vscode.getMainWindow();
    
    // Verify extensions view is open
    const extensionsView = await window.$('.extensions-viewlet');
    expect(extensionsView).toBeTruthy();
    
    // Search for Python extension
    const searchBox = await window.$('.extensions-viewlet .suggest-input-container input');
    if (searchBox) {
      await searchBox.click();
      await window.keyboard.type('Python');
      await TestHelpers.wait(2000);
    }
    
    // Take screenshot
    await vscode.takeScreenshot();
  });

  test('should configure workspace settings', async () => {
    TestHelpers.logStep('Configuring workspace settings');
    
    // Create a test workspace first
    const workspacePath = await TestHelpers.createTestWorkspace();
    
    try {
      // Open the workspace
      await vscode.openFolder(workspacePath);
      await TestHelpers.wait(3000);
      
      // Open workspace settings
      await vscode.executeCommand('Preferences: Open Workspace Settings (JSON)');
      await TestHelpers.wait(2000);
      
      const window = vscode.getMainWindow();
      
      // Add workspace-specific settings
      const workspaceSettings = `{
    "python.defaultInterpreterPath": "./venv/bin/python",
    "editor.rulers": [80, 120],
    "files.exclude": {
        "**/__pycache__": true,
        "**/*.pyc": true
    },
    "python.linting.enabled": true,
    "python.linting.pylintEnabled": true
}`;
      
      // Type workspace settings
      await vscode.typeInEditor(workspaceSettings);
      await TestHelpers.wait(1000);
      
      // Save settings
      await vscode.saveFile();
      await TestHelpers.wait(1000);
      
      // Take screenshot
      await vscode.takeScreenshot();
      
    } finally {
      // Clean up workspace
      TestHelpers.cleanupTestWorkspace(workspacePath);
    }
  });

  test('should configure keybindings', async () => {
    TestHelpers.logStep('Configuring keybindings');
    
    // Open keybindings
    await vscode.executeCommand('Preferences: Open Keyboard Shortcuts');
    await TestHelpers.wait(2000);
    
    const window = vscode.getMainWindow();
    
    // Verify keybindings editor is open
    const keybindingsEditor = await window.$('.keybindings-editor');
    expect(keybindingsEditor).toBeTruthy();
    
    // Search for a specific command
    const searchBox = await window.$('.keybindings-header-widget .suggest-input-container input');
    if (searchBox) {
      await searchBox.click();
      await window.keyboard.type('toggle terminal');
      await TestHelpers.wait(1000);
    }
    
    // Take screenshot
    await vscode.takeScreenshot();
  });

  test('should configure color theme', async () => {
    TestHelpers.logStep('Configuring color theme');
    
    // Open color theme selector
    await vscode.executeCommand('Preferences: Color Theme');
    await TestHelpers.wait(2000);
    
    const window = vscode.getMainWindow();
    
    // Check if theme selector is open
    const quickPick = await window.$('.quick-input-widget');
    if (quickPick) {
      TestHelpers.logStep('Color theme selector opened');
      
      // Navigate through a few themes
      await window.keyboard.press('ArrowDown');
      await TestHelpers.wait(500);
      await window.keyboard.press('ArrowDown');
      await TestHelpers.wait(500);
      
      // Press Escape to close without selecting
      await window.keyboard.press('Escape');
      await TestHelpers.wait(500);
    }
    
    // Take screenshot
    await vscode.takeScreenshot();
  });

  test('should handle user snippets', async () => {
    TestHelpers.logStep('Managing user snippets');
    
    // Open user snippets
    await vscode.executeCommand('Preferences: Configure User Snippets');
    await TestHelpers.wait(2000);
    
    const window = vscode.getMainWindow();
    
    // Check if snippet selector is open
    const quickPick = await window.$('.quick-input-widget');
    if (quickPick) {
      TestHelpers.logStep('User snippets selector opened');
      
      // Type to search for Python snippets
      await window.keyboard.type('python');
      await TestHelpers.wait(1000);
      
      // Press Enter to select (or Escape to cancel)
      await window.keyboard.press('Escape');
      await TestHelpers.wait(500);
    }
    
    // Take screenshot
    await vscode.takeScreenshot();
  });
});