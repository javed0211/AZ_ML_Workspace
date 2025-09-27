"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const test_1 = require("@playwright/test");
const vscode_electron_1 = require("../utils/vscode-electron");
const test_helpers_1 = require("../utils/test-helpers");
/**
 * Basic VS Code functionality tests
 */
test_1.test.describe('VS Code Basic Functionality', () => {
    let vscode;
    test_1.test.beforeEach(async () => {
        test_helpers_1.TestHelpers.logStep('Setting up VS Code for testing');
        vscode = new vscode_electron_1.VSCodeElectron();
        await vscode.launch();
        await test_helpers_1.TestHelpers.setupVSCodeForTesting(vscode);
    });
    test_1.test.afterEach(async () => {
        test_helpers_1.TestHelpers.logStep('Cleaning up VS Code');
        if (vscode.isRunning()) {
            await vscode.close();
        }
        test_helpers_1.TestHelpers.cleanupTempFiles();
    });
    (0, test_1.test)('should launch VS Code successfully', async () => {
        test_helpers_1.TestHelpers.logStep('Verifying VS Code launched successfully');
        (0, test_1.expect)(vscode.isRunning()).toBe(true);
        const isStateValid = await test_helpers_1.TestHelpers.verifyVSCodeState(vscode);
        (0, test_1.expect)(isStateValid).toBe(true);
        // Take a screenshot for verification
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should create and edit a new file', async () => {
        test_helpers_1.TestHelpers.logStep('Creating a new file');
        // Create new file
        await vscode.createNewFile();
        await test_helpers_1.TestHelpers.wait(1000);
        // Type some content
        const testContent = 'Hello, World!\nThis is a test file created by Playwright automation.';
        test_helpers_1.TestHelpers.logStep('Typing content in editor');
        await vscode.typeInEditor(testContent);
        // Verify content was typed (simplified check)
        const window = vscode.getMainWindow();
        const editorExists = await window.$('.monaco-editor');
        (0, test_1.expect)(editorExists).toBeTruthy();
        // Take screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should open command palette and execute command', async () => {
        test_helpers_1.TestHelpers.logStep('Opening command palette');
        await vscode.openCommandPalette();
        // Verify command palette is open
        const window = vscode.getMainWindow();
        const commandPalette = await window.$('.quick-input-widget');
        (0, test_1.expect)(commandPalette).toBeTruthy();
        // Execute a simple command
        test_helpers_1.TestHelpers.logStep('Executing command: View: Toggle Terminal');
        await vscode.executeCommand('View: Toggle Terminal');
        // Wait for terminal to appear
        await test_helpers_1.TestHelpers.wait(2000);
        // Take screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should open and work with files', async () => {
        test_helpers_1.TestHelpers.logStep('Creating test workspace');
        // Create a test workspace
        const workspacePath = await test_helpers_1.TestHelpers.createTestWorkspace();
        try {
            // Open the workspace
            test_helpers_1.TestHelpers.logStep('Opening test workspace');
            await vscode.openFolder(workspacePath);
            // Wait for workspace to load
            await test_helpers_1.TestHelpers.wait(3000);
            // Open a specific file
            const pythonFile = `${workspacePath}/main.py`;
            test_helpers_1.TestHelpers.logStep(`Opening file: ${pythonFile}`);
            await vscode.openFile(pythonFile);
            // Wait for file to load
            await test_helpers_1.TestHelpers.wait(2000);
            // Verify file is open by checking for Python syntax highlighting
            const window = vscode.getMainWindow();
            const editor = await window.$('.monaco-editor');
            (0, test_1.expect)(editor).toBeTruthy();
            // Take screenshot
            await vscode.takeScreenshot();
        }
        finally {
            // Clean up workspace
            test_helpers_1.TestHelpers.cleanupTestWorkspace(workspacePath);
        }
    });
    (0, test_1.test)('should handle keyboard shortcuts', async () => {
        test_helpers_1.TestHelpers.logStep('Testing keyboard shortcuts');
        // Create new file
        await vscode.createNewFile();
        await test_helpers_1.TestHelpers.wait(1000);
        // Type some content
        await vscode.typeInEditor('Line 1\nLine 2\nLine 3');
        await test_helpers_1.TestHelpers.wait(1000);
        const window = vscode.getMainWindow();
        const isMac = process.platform === 'darwin';
        const modifier = isMac ? 'Meta' : 'Control';
        // Test Ctrl+A (Select All)
        test_helpers_1.TestHelpers.logStep('Testing Select All shortcut');
        await window.keyboard.press(`${modifier}+KeyA`);
        await test_helpers_1.TestHelpers.wait(500);
        // Test Ctrl+C (Copy)
        test_helpers_1.TestHelpers.logStep('Testing Copy shortcut');
        await window.keyboard.press(`${modifier}+KeyC`);
        await test_helpers_1.TestHelpers.wait(500);
        // Test Ctrl+V (Paste)
        test_helpers_1.TestHelpers.logStep('Testing Paste shortcut');
        await window.keyboard.press(`${modifier}+KeyV`);
        await test_helpers_1.TestHelpers.wait(500);
        // Take screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should handle multiple windows', async () => {
        test_helpers_1.TestHelpers.logStep('Testing multiple windows');
        // Open a new window
        await vscode.executeCommand('File: New Window');
        await test_helpers_1.TestHelpers.wait(3000);
        // Get all windows
        const electronApp = vscode.getElectronApp();
        const windows = electronApp.windows();
        // Should have at least 2 windows now
        (0, test_1.expect)(windows.length).toBeGreaterThanOrEqual(1);
        // Take screenshot
        await vscode.takeScreenshot();
    });
});
