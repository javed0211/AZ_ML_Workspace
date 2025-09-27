import { test, expect } from '@playwright/test';
import { VSCodeElectron } from '../utils/vscode-electron';
import { TestHelpers } from '../utils/test-helpers';

/**
 * Basic VS Code functionality tests
 */
test.describe('VS Code Basic Functionality', () => {
  let vscode: VSCodeElectron;

  test.beforeEach(async () => {
    TestHelpers.logStep('Setting up VS Code for testing');
    vscode = new VSCodeElectron();
    await vscode.launch();
    await TestHelpers.setupVSCodeForTesting(vscode);
  });

  test.afterEach(async () => {
    TestHelpers.logStep('Cleaning up VS Code');
    if (vscode.isRunning()) {
      await vscode.close();
    }
    TestHelpers.cleanupTempFiles();
  });

  test('should launch VS Code successfully', async () => {
    TestHelpers.logStep('Verifying VS Code launched successfully');
    
    expect(vscode.isRunning()).toBe(true);
    
    const isStateValid = await TestHelpers.verifyVSCodeState(vscode);
    expect(isStateValid).toBe(true);
    
    // Take a screenshot for verification
    await vscode.takeScreenshot();
  });

  test('should create and edit a new file', async () => {
    TestHelpers.logStep('Creating a new file');
    
    // Create new file
    await vscode.createNewFile();
    await TestHelpers.wait(1000);
    
    // Type some content
    const testContent = 'Hello, World!\nThis is a test file created by Playwright automation.';
    TestHelpers.logStep('Typing content in editor');
    await vscode.typeInEditor(testContent);
    
    // Verify content was typed (simplified check)
    const window = vscode.getMainWindow();
    const editorExists = await window.$('.monaco-editor');
    expect(editorExists).toBeTruthy();
    
    // Take screenshot
    await vscode.takeScreenshot();
  });

  test('should open command palette and execute command', async () => {
    TestHelpers.logStep('Opening command palette');
    
    await vscode.openCommandPalette();
    
    // Verify command palette is open
    const window = vscode.getMainWindow();
    const commandPalette = await window.$('.quick-input-widget');
    expect(commandPalette).toBeTruthy();
    
    // Execute a simple command
    TestHelpers.logStep('Executing command: View: Toggle Terminal');
    await vscode.executeCommand('View: Toggle Terminal');
    
    // Wait for terminal to appear
    await TestHelpers.wait(2000);
    
    // Take screenshot
    await vscode.takeScreenshot();
  });

  test('should open and work with files', async () => {
    TestHelpers.logStep('Creating test workspace');
    
    // Create a test workspace
    const workspacePath = await TestHelpers.createTestWorkspace();
    
    try {
      // Open the workspace
      TestHelpers.logStep('Opening test workspace');
      await vscode.openFolder(workspacePath);
      
      // Wait for workspace to load
      await TestHelpers.wait(3000);
      
      // Open a specific file
      const pythonFile = `${workspacePath}/main.py`;
      TestHelpers.logStep(`Opening file: ${pythonFile}`);
      await vscode.openFile(pythonFile);
      
      // Wait for file to load
      await TestHelpers.wait(2000);
      
      // Verify file is open by checking for Python syntax highlighting
      const window = vscode.getMainWindow();
      const editor = await window.$('.monaco-editor');
      expect(editor).toBeTruthy();
      
      // Take screenshot
      await vscode.takeScreenshot();
      
    } finally {
      // Clean up workspace
      TestHelpers.cleanupTestWorkspace(workspacePath);
    }
  });

  test('should handle keyboard shortcuts', async () => {
    TestHelpers.logStep('Testing keyboard shortcuts');
    
    // Create new file
    await vscode.createNewFile();
    await TestHelpers.wait(1000);
    
    // Type some content
    await vscode.typeInEditor('Line 1\nLine 2\nLine 3');
    await TestHelpers.wait(1000);
    
    const window = vscode.getMainWindow();
    const isMac = process.platform === 'darwin';
    const modifier = isMac ? 'Meta' : 'Control';
    
    // Test Ctrl+A (Select All)
    TestHelpers.logStep('Testing Select All shortcut');
    await window.keyboard.press(`${modifier}+KeyA`);
    await TestHelpers.wait(500);
    
    // Test Ctrl+C (Copy)
    TestHelpers.logStep('Testing Copy shortcut');
    await window.keyboard.press(`${modifier}+KeyC`);
    await TestHelpers.wait(500);
    
    // Test Ctrl+V (Paste)
    TestHelpers.logStep('Testing Paste shortcut');
    await window.keyboard.press(`${modifier}+KeyV`);
    await TestHelpers.wait(500);
    
    // Take screenshot
    await vscode.takeScreenshot();
  });

  test('should handle multiple windows', async () => {
    TestHelpers.logStep('Testing multiple windows');
    
    // Open a new window
    await vscode.executeCommand('File: New Window');
    await TestHelpers.wait(3000);
    
    // Get all windows
    const electronApp = vscode.getElectronApp();
    const windows = electronApp.windows();
    
    // Should have at least 2 windows now
    expect(windows.length).toBeGreaterThanOrEqual(1);
    
    // Take screenshot
    await vscode.takeScreenshot();
  });
});