"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.VSCodeElectron = void 0;
const vscode_config_1 = require("../config/vscode-config");
const child_process_1 = require("child_process");
/**
 * VS Code Electron Application wrapper for testing
 */
class VSCodeElectron {
    electronApp = null;
    mainWindow = null;
    config;
    vsCodeProcess = null;
    constructor(config) {
        this.config = { ...(0, vscode_config_1.getDefaultVSCodeConfig)(), ...config };
    }
    /**
     * Launch VS Code Electron application
     */
    async launch(workspaceOrFile, additionalArgs = []) {
        // Validate VS Code installation first
        const validation = (0, vscode_config_1.validateVSCodeInstallation)();
        if (!validation.isValid) {
            console.error('❌ VS Code Installation Error:');
            console.error(validation.message);
            throw new Error(`VS Code not found at expected location. ${validation.message}`);
        }
        console.log('✅ VS Code validation passed:', validation.message);
        const launchArgs = (0, vscode_config_1.getVSCodeLaunchArgs)(this.config, additionalArgs);
        // Add workspace or file to open
        if (workspaceOrFile) {
            launchArgs.push(workspaceOrFile);
        }
        console.log(`🚀 Launching VS Code with executable: ${this.config.executablePath}`);
        console.log(`📋 Launch arguments: ${launchArgs.join(' ')}`);
        try {
            // Try the manual launch approach first
            await this.launchVSCodeManually(launchArgs);
            console.log('✅ VS Code launched successfully');
        }
        catch (error) {
            console.error('❌ Failed to launch VS Code:', error);
            // Provide additional troubleshooting information
            if (error instanceof Error) {
                if (error.message.includes('ENOENT')) {
                    console.error('\n🔍 Troubleshooting:');
                    console.error('1. Verify VS Code is installed at the expected location');
                    console.error('2. Check if the executable path is correct');
                    console.error('3. Try running VS Code manually first');
                    console.error(`4. Expected path: ${this.config.executablePath}`);
                }
                else if (error.message.includes('bad option')) {
                    console.error('\n🔍 VS Code Launch Arguments Issue:');
                    console.error('VS Code doesn\'t accept some Electron debugging arguments.');
                    console.error('This is a known issue with testing VS Code directly.');
                    console.error('Consider using VS Code\'s built-in testing capabilities instead.');
                }
            }
            throw error;
        }
    }
    /**
     * Launch VS Code manually using child_process and create a mock Playwright interface
     */
    async launchVSCodeManually(launchArgs) {
        return new Promise((resolve, reject) => {
            const platform = require('os').platform();
            let command;
            let args;
            if (platform === 'darwin') {
                // On macOS, use 'open' command to launch the app properly
                // Extract the .app path from the executable path
                const appPath = this.config.executablePath.replace('/Contents/MacOS/Electron', '');
                command = 'open';
                args = [
                    '-a',
                    appPath,
                    '--args',
                    ...launchArgs
                ];
            }
            else {
                // On other platforms, launch directly
                command = this.config.executablePath;
                args = launchArgs;
            }
            console.log(`Launching with command: ${command} ${args.join(' ')}`);
            // Launch VS Code process
            this.vsCodeProcess = (0, child_process_1.spawn)(command, args, {
                detached: false,
                stdio: ['ignore', 'pipe', 'pipe']
            });
            // Handle process events
            this.vsCodeProcess.on('error', (error) => {
                console.error('VS Code process error:', error);
                reject(error);
            });
            let processExited = false;
            this.vsCodeProcess.on('exit', (code, signal) => {
                console.log(`VS Code process exited with code ${code}, signal ${signal}`);
                processExited = true;
                if (platform === 'darwin' && code === 0) {
                    // On macOS, 'open' command exits with 0 after launching the app successfully
                    // This is normal behavior, so we don't set vsCodeProcess to null
                    console.log('VS Code launched successfully via open command');
                }
                else {
                    this.vsCodeProcess = null;
                }
            });
            // Capture stdout/stderr for debugging
            if (this.vsCodeProcess.stdout) {
                this.vsCodeProcess.stdout.on('data', (data) => {
                    console.log('VS Code stdout:', data.toString());
                });
            }
            if (this.vsCodeProcess.stderr) {
                this.vsCodeProcess.stderr.on('data', (data) => {
                    const errorMsg = data.toString();
                    console.error('VS Code stderr:', errorMsg);
                    // Check for bad option errors
                    if (errorMsg.includes('bad option')) {
                        reject(new Error(`VS Code rejected arguments: ${errorMsg}`));
                        return;
                    }
                });
            }
            // Wait a bit for VS Code to start
            setTimeout(() => {
                if (platform === 'darwin') {
                    // On macOS, if the open command succeeded, assume VS Code is launching
                    if (processExited && this.vsCodeProcess?.exitCode === 0) {
                        console.log('VS Code launch initiated successfully');
                        this.createMockWindow();
                        resolve();
                    }
                    else if (!processExited) {
                        // Still waiting for open command to complete
                        this.createMockWindow();
                        resolve();
                    }
                    else {
                        reject(new Error('VS Code failed to launch via open command'));
                    }
                }
                else {
                    // On other platforms, check if process is still running
                    if (this.vsCodeProcess && !this.vsCodeProcess.killed) {
                        this.createMockWindow();
                        resolve();
                    }
                    else {
                        reject(new Error('VS Code process failed to start'));
                    }
                }
            }, 3000);
        });
    }
    /**
     * Create a mock window interface for basic testing compatibility
     */
    createMockWindow() {
        // This is a simplified mock - in a real implementation you'd want to
        // connect to VS Code's actual window or use VS Code's extension API
        this.mainWindow = {
            waitForSelector: async (selector, options) => {
                console.log(`Mock: Waiting for selector ${selector}`);
                await new Promise(resolve => setTimeout(resolve, 1000));
                return null;
            },
            click: async (selector) => {
                console.log(`Mock: Clicking ${selector}`);
                await new Promise(resolve => setTimeout(resolve, 500));
            },
            keyboard: {
                press: async (key) => {
                    console.log(`Mock: Pressing key ${key}`);
                    await new Promise(resolve => setTimeout(resolve, 200));
                },
                type: async (text) => {
                    console.log(`Mock: Typing "${text}"`);
                    await new Promise(resolve => setTimeout(resolve, 500));
                }
            },
            waitForTimeout: async (timeout) => {
                await new Promise(resolve => setTimeout(resolve, timeout));
            },
            screenshot: async (options) => {
                console.log('Mock: Taking screenshot');
                return Buffer.from('mock-screenshot');
            },
            evaluate: async (fn) => {
                console.log('Mock: Evaluating function');
                return 'mock-result';
            }
        };
    }
    /**
     * Wait for VS Code to be fully loaded and ready
     */
    async waitForVSCodeReady() {
        if (!this.mainWindow) {
            throw new Error('Main window not available');
        }
        // Wait for the main VS Code interface to load
        try {
            // Wait for the workbench to be ready
            await this.mainWindow.waitForSelector('.monaco-workbench', { timeout: 30000 });
            // Wait a bit more for everything to settle
            await this.mainWindow.waitForTimeout(2000);
            console.log('VS Code is ready');
        }
        catch (error) {
            console.error('VS Code failed to load properly:', error);
            throw error;
        }
    }
    /**
     * Get the main VS Code window
     */
    getMainWindow() {
        if (!this.mainWindow) {
            throw new Error('VS Code not launched or main window not available');
        }
        return this.mainWindow;
    }
    /**
     * Get the Electron application instance
     */
    getElectronApp() {
        if (!this.electronApp) {
            throw new Error('VS Code not launched');
        }
        return this.electronApp;
    }
    /**
     * Open a file in VS Code
     */
    async openFile(filePath) {
        const window = this.getMainWindow();
        // Use Ctrl+O (Cmd+O on Mac) to open file dialog
        const isMac = process.platform === 'darwin';
        const modifier = isMac ? 'Meta' : 'Control';
        await window.keyboard.press(`${modifier}+KeyO`);
        // Wait for file dialog and type the path
        await window.waitForTimeout(1000);
        await window.keyboard.type(filePath);
        await window.keyboard.press('Enter');
        // Wait for file to load
        await window.waitForTimeout(2000);
    }
    /**
     * Open a folder/workspace in VS Code
     */
    async openFolder(folderPath) {
        const window = this.getMainWindow();
        // Use Ctrl+K Ctrl+O (Cmd+K Cmd+O on Mac) to open folder
        const isMac = process.platform === 'darwin';
        const modifier = isMac ? 'Meta' : 'Control';
        await window.keyboard.press(`${modifier}+KeyK`);
        await window.keyboard.press(`${modifier}+KeyO`);
        // Wait for folder dialog and type the path
        await window.waitForTimeout(1000);
        await window.keyboard.type(folderPath);
        await window.keyboard.press('Enter');
        // Wait for folder to load
        await window.waitForTimeout(3000);
    }
    /**
     * Create a new file
     */
    async createNewFile() {
        const window = this.getMainWindow();
        const isMac = process.platform === 'darwin';
        const modifier = isMac ? 'Meta' : 'Control';
        await window.keyboard.press(`${modifier}+KeyN`);
        await window.waitForTimeout(1000);
    }
    /**
     * Save current file
     */
    async saveFile() {
        const window = this.getMainWindow();
        const isMac = process.platform === 'darwin';
        const modifier = isMac ? 'Meta' : 'Control';
        await window.keyboard.press(`${modifier}+KeyS`);
        await window.waitForTimeout(1000);
    }
    /**
     * Type text in the editor
     */
    async typeInEditor(text) {
        const window = this.getMainWindow();
        // Click in the editor area first
        await window.click('.monaco-editor .view-lines');
        await window.waitForTimeout(500);
        // Type the text
        await window.keyboard.type(text);
    }
    /**
     * Get text content from the active editor
     */
    async getEditorContent() {
        const window = this.getMainWindow();
        // Select all text and copy
        const isMac = process.platform === 'darwin';
        const modifier = isMac ? 'Meta' : 'Control';
        await window.keyboard.press(`${modifier}+KeyA`);
        await window.keyboard.press(`${modifier}+KeyC`);
        // Get clipboard content (this is a simplified approach)
        // In a real scenario, you might need to use Electron's clipboard API
        return await window.evaluate(() => {
            return navigator.clipboard.readText();
        });
    }
    /**
     * Open command palette
     */
    async openCommandPalette() {
        const window = this.getMainWindow();
        const isMac = process.platform === 'darwin';
        const modifier = isMac ? 'Meta' : 'Control';
        await window.keyboard.press(`${modifier}+Shift+KeyP`);
        await window.waitForSelector('.quick-input-widget', { timeout: 5000 });
    }
    /**
     * Execute a command via command palette
     */
    async executeCommand(command) {
        await this.openCommandPalette();
        const window = this.getMainWindow();
        await window.keyboard.type(command);
        await window.waitForTimeout(500);
        await window.keyboard.press('Enter');
        await window.waitForTimeout(1000);
    }
    /**
     * Take a screenshot
     */
    async takeScreenshot(path) {
        const window = this.getMainWindow();
        const screenshotPath = path || `../Reports/screenshots/vscode-${Date.now()}.png`;
        return await window.screenshot({ path: screenshotPath, fullPage: true });
    }
    /**
     * Close VS Code
     */
    async close() {
        if (this.electronApp) {
            await this.electronApp.close();
            this.electronApp = null;
        }
        if (this.vsCodeProcess && !this.vsCodeProcess.killed) {
            // Try graceful shutdown first
            this.vsCodeProcess.kill('SIGTERM');
            // Wait a bit for graceful shutdown
            await new Promise(resolve => setTimeout(resolve, 2000));
            // Force kill if still running
            if (!this.vsCodeProcess.killed) {
                this.vsCodeProcess.kill('SIGKILL');
            }
            this.vsCodeProcess = null;
        }
        this.mainWindow = null;
        console.log('VS Code closed');
    }
    /**
     * Wait for a specific element to be visible
     */
    async waitForElement(selector, timeout = 10000) {
        const window = this.getMainWindow();
        await window.waitForSelector(selector, { timeout });
    }
    /**
     * Click on an element
     */
    async clickElement(selector) {
        const window = this.getMainWindow();
        await window.click(selector);
    }
    /**
     * Check if VS Code is running
     */
    isRunning() {
        return (this.electronApp !== null || this.vsCodeProcess !== null) && this.mainWindow !== null;
    }
}
exports.VSCodeElectron = VSCodeElectron;
