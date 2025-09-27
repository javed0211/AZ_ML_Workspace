"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const test_1 = require("@playwright/test");
const vscode_electron_1 = require("../utils/vscode-electron");
const test_helpers_1 = require("../utils/test-helpers");
/**
 * VS Code settings and configuration tests
 */
test_1.test.describe('VS Code Settings and Configuration', () => {
    let vscode;
    test_1.test.beforeEach(async () => {
        test_helpers_1.TestHelpers.logStep('Setting up VS Code for settings testing');
        vscode = new vscode_electron_1.VSCodeElectron();
        await vscode.launch();
        await test_helpers_1.TestHelpers.setupVSCodeForTesting(vscode);
    });
    test_1.test.afterEach(async () => {
        test_helpers_1.TestHelpers.logStep('Cleaning up VS Code settings test');
        if (vscode.isRunning()) {
            await vscode.close();
        }
        test_helpers_1.TestHelpers.cleanupTempFiles();
    });
    (0, test_1.test)('should open and navigate settings', async () => {
        test_helpers_1.TestHelpers.logStep('Opening VS Code settings');
        // Open settings
        await vscode.executeCommand('Preferences: Open Settings (UI)');
        await test_helpers_1.TestHelpers.wait(2000);
        const window = vscode.getMainWindow();
        // Verify settings page is open
        const settingsEditor = await window.$('.settings-editor');
        (0, test_1.expect)(settingsEditor).toBeTruthy();
        // Try to search for a setting
        const searchBox = await window.$('.settings-header-widget .suggest-input-container input');
        if (searchBox) {
            await searchBox.click();
            await window.keyboard.type('python');
            await test_helpers_1.TestHelpers.wait(1000);
        }
        // Take screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should modify editor settings', async () => {
        test_helpers_1.TestHelpers.logStep('Modifying editor settings');
        // Open settings JSON
        await vscode.executeCommand('Preferences: Open Settings (JSON)');
        await test_helpers_1.TestHelpers.wait(2000);
        const window = vscode.getMainWindow();
        // Check if settings.json is open
        const editor = await window.$('.monaco-editor');
        (0, test_1.expect)(editor).toBeTruthy();
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
        await test_helpers_1.TestHelpers.wait(1000);
        // Save settings
        await vscode.saveFile();
        await test_helpers_1.TestHelpers.wait(1000);
        // Take screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should configure Python interpreter', async () => {
        test_helpers_1.TestHelpers.logStep('Configuring Python interpreter');
        // Open command palette and search for Python interpreter
        await vscode.executeCommand('Python: Select Interpreter');
        await test_helpers_1.TestHelpers.wait(2000);
        const window = vscode.getMainWindow();
        // Check if interpreter selection is shown
        const quickPick = await window.$('.quick-input-widget');
        if (quickPick) {
            test_helpers_1.TestHelpers.logStep('Python interpreter selection dialog opened');
            // Press Escape to close without selecting
            await window.keyboard.press('Escape');
            await test_helpers_1.TestHelpers.wait(500);
        }
        else {
            test_helpers_1.TestHelpers.logStep('Python extension may not be installed');
        }
        // Take screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should manage extensions', async () => {
        test_helpers_1.TestHelpers.logStep('Managing VS Code extensions');
        // Open extensions view
        await vscode.executeCommand('View: Show Extensions');
        await test_helpers_1.TestHelpers.wait(2000);
        const window = vscode.getMainWindow();
        // Verify extensions view is open
        const extensionsView = await window.$('.extensions-viewlet');
        (0, test_1.expect)(extensionsView).toBeTruthy();
        // Search for Python extension
        const searchBox = await window.$('.extensions-viewlet .suggest-input-container input');
        if (searchBox) {
            await searchBox.click();
            await window.keyboard.type('Python');
            await test_helpers_1.TestHelpers.wait(2000);
        }
        // Take screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should configure workspace settings', async () => {
        test_helpers_1.TestHelpers.logStep('Configuring workspace settings');
        // Create a test workspace first
        const workspacePath = await test_helpers_1.TestHelpers.createTestWorkspace();
        try {
            // Open the workspace
            await vscode.openFolder(workspacePath);
            await test_helpers_1.TestHelpers.wait(3000);
            // Open workspace settings
            await vscode.executeCommand('Preferences: Open Workspace Settings (JSON)');
            await test_helpers_1.TestHelpers.wait(2000);
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
            await test_helpers_1.TestHelpers.wait(1000);
            // Save settings
            await vscode.saveFile();
            await test_helpers_1.TestHelpers.wait(1000);
            // Take screenshot
            await vscode.takeScreenshot();
        }
        finally {
            // Clean up workspace
            test_helpers_1.TestHelpers.cleanupTestWorkspace(workspacePath);
        }
    });
    (0, test_1.test)('should configure keybindings', async () => {
        test_helpers_1.TestHelpers.logStep('Configuring keybindings');
        // Open keybindings
        await vscode.executeCommand('Preferences: Open Keyboard Shortcuts');
        await test_helpers_1.TestHelpers.wait(2000);
        const window = vscode.getMainWindow();
        // Verify keybindings editor is open
        const keybindingsEditor = await window.$('.keybindings-editor');
        (0, test_1.expect)(keybindingsEditor).toBeTruthy();
        // Search for a specific command
        const searchBox = await window.$('.keybindings-header-widget .suggest-input-container input');
        if (searchBox) {
            await searchBox.click();
            await window.keyboard.type('toggle terminal');
            await test_helpers_1.TestHelpers.wait(1000);
        }
        // Take screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should configure color theme', async () => {
        test_helpers_1.TestHelpers.logStep('Configuring color theme');
        // Open color theme selector
        await vscode.executeCommand('Preferences: Color Theme');
        await test_helpers_1.TestHelpers.wait(2000);
        const window = vscode.getMainWindow();
        // Check if theme selector is open
        const quickPick = await window.$('.quick-input-widget');
        if (quickPick) {
            test_helpers_1.TestHelpers.logStep('Color theme selector opened');
            // Navigate through a few themes
            await window.keyboard.press('ArrowDown');
            await test_helpers_1.TestHelpers.wait(500);
            await window.keyboard.press('ArrowDown');
            await test_helpers_1.TestHelpers.wait(500);
            // Press Escape to close without selecting
            await window.keyboard.press('Escape');
            await test_helpers_1.TestHelpers.wait(500);
        }
        // Take screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should handle user snippets', async () => {
        test_helpers_1.TestHelpers.logStep('Managing user snippets');
        // Open user snippets
        await vscode.executeCommand('Preferences: Configure User Snippets');
        await test_helpers_1.TestHelpers.wait(2000);
        const window = vscode.getMainWindow();
        // Check if snippet selector is open
        const quickPick = await window.$('.quick-input-widget');
        if (quickPick) {
            test_helpers_1.TestHelpers.logStep('User snippets selector opened');
            // Type to search for Python snippets
            await window.keyboard.type('python');
            await test_helpers_1.TestHelpers.wait(1000);
            // Press Enter to select (or Escape to cancel)
            await window.keyboard.press('Escape');
            await test_helpers_1.TestHelpers.wait(500);
        }
        // Take screenshot
        await vscode.takeScreenshot();
    });
});
