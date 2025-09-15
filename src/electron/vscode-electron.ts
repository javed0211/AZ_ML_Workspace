import { _electron as electron, ElectronApplication, Page } from '@playwright/test';
import { logger, TestLogger } from '../helpers/logger';
import { config } from '../helpers/config';
import * as path from 'path';
import * as fs from 'fs';
import * as os from 'os';

export interface VSCodeConfig {
  userDataDir?: string;
  extensionsDir?: string;
  workspaceFolder?: string;
  remoteConnection?: {
    type: 'ssh' | 'wsl' | 'container';
    target: string;
  };
  extensions?: string[];
  settings?: Record<string, any>;
}

export interface VSCodeWindow {
  page: Page;
  title: string;
  workspaceFolder?: string;
}

export class VSCodeElectronHelper {
  private app: ElectronApplication | null = null;
  private windows: Map<string, VSCodeWindow> = new Map();
  private testLogger?: TestLogger;
  private userDataDir: string;

  constructor(private vsCodeConfig: VSCodeConfig = {}, testLogger?: TestLogger) {
    this.testLogger = testLogger;
    this.userDataDir = vsCodeConfig.userDataDir || this.createTempUserDataDir();
  }

  async launch(): Promise<ElectronApplication> {
    try {
      this.testLogger?.action('Launching VS Code Electron');
      
      const vsCodePath = this.getVSCodePath();
      const args = this.buildLaunchArgs();
      
      this.testLogger?.debug('VS Code launch configuration', {
        executablePath: vsCodePath,
        args,
        userDataDir: this.userDataDir,
      });

      this.app = await electron.launch({
        executablePath: vsCodePath,
        args,
        env: {
          ...process.env,
          VSCODE_DEV: '1',
          ELECTRON_ENABLE_LOGGING: '1',
        },
      });

      // Wait for the main window
      const page = await this.app.firstWindow();
      await page.waitForLoadState('domcontentloaded');
      
      // Store the main window
      this.windows.set('main', {
        page,
        title: await page.title(),
        workspaceFolder: this.vsCodeConfig.workspaceFolder,
      });

      this.testLogger?.info('VS Code Electron launched successfully', {
        windowCount: this.windows.size,
        userDataDir: this.userDataDir,
      });

      return this.app;
    } catch (error) {
      this.testLogger?.error('Failed to launch VS Code Electron', { error });
      throw error;
    }
  }

  async close(): Promise<void> {
    try {
      this.testLogger?.action('Closing VS Code Electron');
      
      if (this.app) {
        await this.app.close();
        this.app = null;
      }
      
      this.windows.clear();
      
      // Clean up temporary user data directory
      if (this.userDataDir.includes('vscode-test-')) {
        await this.cleanupUserDataDir();
      }
      
      this.testLogger?.info('VS Code Electron closed successfully');
    } catch (error) {
      this.testLogger?.error('Failed to close VS Code Electron', { error });
      throw error;
    }
  }

  async getMainWindow(): Promise<VSCodeWindow> {
    const mainWindow = this.windows.get('main');
    if (!mainWindow) {
      throw new Error('Main window not found. Make sure VS Code is launched.');
    }
    return mainWindow;
  }

  async getAllWindows(): Promise<VSCodeWindow[]> {
    return Array.from(this.windows.values());
  }

  // Workspace management
  async openWorkspace(workspacePath: string): Promise<void> {
    try {
      this.testLogger?.action(`Opening workspace: ${workspacePath}`);
      
      const mainWindow = await this.getMainWindow();
      const page = mainWindow.page;
      
      // Use command palette to open folder
      await this.openCommandPalette(page);
      await page.keyboard.type('File: Open Folder');
      await page.keyboard.press('Enter');
      
      // Wait for file dialog and enter path
      await page.waitForTimeout(1000);
      await page.keyboard.type(workspacePath);
      await page.keyboard.press('Enter');
      
      // Wait for workspace to load
      await page.waitForSelector('[data-testid="workbench.view.explorer"]', { timeout: 30000 });
      
      this.testLogger?.info(`Workspace opened: ${workspacePath}`);
    } catch (error) {
      this.testLogger?.error(`Failed to open workspace: ${workspacePath}`, { error });
      throw error;
    }
  }

  async connectToRemote(connectionString: string): Promise<void> {
    try {
      this.testLogger?.action(`Connecting to remote: ${connectionString}`);
      
      const mainWindow = await this.getMainWindow();
      const page = mainWindow.page;
      
      // Open command palette
      await this.openCommandPalette(page);
      await page.keyboard.type('Remote-SSH: Connect to Host');
      await page.keyboard.press('Enter');
      
      // Enter connection string
      await page.waitForTimeout(1000);
      await page.keyboard.type(connectionString);
      await page.keyboard.press('Enter');
      
      // Wait for connection to establish
      await page.waitForSelector('.remote-indicator', { timeout: 60000 });
      
      this.testLogger?.info(`Connected to remote: ${connectionString}`);
    } catch (error) {
      this.testLogger?.error(`Failed to connect to remote: ${connectionString}`, { error });
      throw error;
    }
  }

  // File operations
  async openFile(filePath: string): Promise<void> {
    try {
      this.testLogger?.action(`Opening file: ${filePath}`);
      
      const mainWindow = await this.getMainWindow();
      const page = mainWindow.page;
      
      // Use command palette to open file
      await this.openCommandPalette(page);
      await page.keyboard.type('File: Open File');
      await page.keyboard.press('Enter');
      
      // Enter file path
      await page.waitForTimeout(1000);
      await page.keyboard.type(filePath);
      await page.keyboard.press('Enter');
      
      // Wait for file to open
      await page.waitForSelector('.monaco-editor', { timeout: 10000 });
      
      this.testLogger?.info(`File opened: ${filePath}`);
    } catch (error) {
      this.testLogger?.error(`Failed to open file: ${filePath}`, { error });
      throw error;
    }
  }

  async createFile(fileName: string, content: string = ''): Promise<void> {
    try {
      this.testLogger?.action(`Creating file: ${fileName}`);
      
      const mainWindow = await this.getMainWindow();
      const page = mainWindow.page;
      
      // Use command palette to create new file
      await this.openCommandPalette(page);
      await page.keyboard.type('File: New File');
      await page.keyboard.press('Enter');
      
      // Type content if provided
      if (content) {
        await page.keyboard.type(content);
      }
      
      // Save file
      await page.keyboard.press('Control+S');
      await page.waitForTimeout(1000);
      await page.keyboard.type(fileName);
      await page.keyboard.press('Enter');
      
      this.testLogger?.info(`File created: ${fileName}`);
    } catch (error) {
      this.testLogger?.error(`Failed to create file: ${fileName}`, { error });
      throw error;
    }
  }

  // Extension management
  async installExtension(extensionId: string): Promise<void> {
    try {
      this.testLogger?.action(`Installing extension: ${extensionId}`);
      
      const mainWindow = await this.getMainWindow();
      const page = mainWindow.page;
      
      // Open extensions view
      await this.openCommandPalette(page);
      await page.keyboard.type('Extensions: Install Extensions');
      await page.keyboard.press('Enter');
      
      // Search for extension
      await page.waitForSelector('input[placeholder*="Search Extensions"]');
      await page.fill('input[placeholder*="Search Extensions"]', extensionId);
      await page.keyboard.press('Enter');
      
      // Install extension
      await page.waitForSelector(`[data-extension-id="${extensionId}"] .install-button`);
      await page.click(`[data-extension-id="${extensionId}"] .install-button`);
      
      // Wait for installation to complete
      await page.waitForSelector(`[data-extension-id="${extensionId}"] .reload-button`, { timeout: 60000 });
      
      this.testLogger?.info(`Extension installed: ${extensionId}`);
    } catch (error) {
      this.testLogger?.error(`Failed to install extension: ${extensionId}`, { error });
      throw error;
    }
  }

  // Terminal operations
  async openTerminal(): Promise<void> {
    try {
      this.testLogger?.action('Opening terminal');
      
      const mainWindow = await this.getMainWindow();
      const page = mainWindow.page;
      
      await this.openCommandPalette(page);
      await page.keyboard.type('Terminal: Create New Terminal');
      await page.keyboard.press('Enter');
      
      // Wait for terminal to appear
      await page.waitForSelector('.terminal-wrapper', { timeout: 10000 });
      
      this.testLogger?.info('Terminal opened');
    } catch (error) {
      this.testLogger?.error('Failed to open terminal', { error });
      throw error;
    }
  }

  async runTerminalCommand(command: string): Promise<void> {
    try {
      this.testLogger?.action(`Running terminal command: ${command}`);
      
      const mainWindow = await this.getMainWindow();
      const page = mainWindow.page;
      
      // Ensure terminal is focused
      await page.click('.terminal-wrapper');
      
      // Type command
      await page.keyboard.type(command);
      await page.keyboard.press('Enter');
      
      this.testLogger?.info(`Terminal command executed: ${command}`);
    } catch (error) {
      this.testLogger?.error(`Failed to run terminal command: ${command}`, { error });
      throw error;
    }
  }

  // Utility methods
  private async openCommandPalette(page: Page): Promise<void> {
    await page.keyboard.press('Control+Shift+P');
    await page.waitForSelector('.quick-input-widget', { timeout: 5000 });
  }

  private getVSCodePath(): string {
    if (config.electron.vscodePath) {
      return config.electron.vscodePath;
    }

    // Try to find VS Code in common locations
    const platform = os.platform();
    const possiblePaths = [];

    if (platform === 'win32') {
      possiblePaths.push(
        'C:\\Program Files\\Microsoft VS Code\\Code.exe',
        'C:\\Program Files (x86)\\Microsoft VS Code\\Code.exe',
        path.join(os.homedir(), 'AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe')
      );
    } else if (platform === 'darwin') {
      possiblePaths.push(
        '/Applications/Visual Studio Code.app/Contents/MacOS/Electron',
        '/usr/local/bin/code'
      );
    } else {
      possiblePaths.push(
        '/usr/bin/code',
        '/usr/local/bin/code',
        '/snap/bin/code'
      );
    }

    for (const vsCodePath of possiblePaths) {
      if (fs.existsSync(vsCodePath)) {
        return vsCodePath;
      }
    }

    throw new Error('VS Code executable not found. Please set VSCODE_PATH environment variable.');
  }

  private buildLaunchArgs(): string[] {
    const args = [
      `--user-data-dir=${this.userDataDir}`,
      '--no-sandbox',
      '--disable-dev-shm-usage',
      '--disable-gpu',
      '--disable-web-security',
      '--disable-features=VizDisplayCompositor',
    ];

    if (this.vsCodeConfig.extensionsDir) {
      args.push(`--extensions-dir=${this.vsCodeConfig.extensionsDir}`);
    }

    if (this.vsCodeConfig.workspaceFolder) {
      args.push(this.vsCodeConfig.workspaceFolder);
    }

    return args;
  }

  private createTempUserDataDir(): string {
    const tempDir = path.join(os.tmpdir(), `vscode-test-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`);
    fs.mkdirSync(tempDir, { recursive: true });
    
    // Create settings.json if provided
    if (this.vsCodeConfig.settings) {
      const settingsDir = path.join(tempDir, 'User');
      fs.mkdirSync(settingsDir, { recursive: true });
      fs.writeFileSync(
        path.join(settingsDir, 'settings.json'),
        JSON.stringify(this.vsCodeConfig.settings, null, 2)
      );
    }
    
    return tempDir;
  }

  private async cleanupUserDataDir(): Promise<void> {
    try {
      if (fs.existsSync(this.userDataDir)) {
        fs.rmSync(this.userDataDir, { recursive: true, force: true });
        this.testLogger?.debug('User data directory cleaned up', { userDataDir: this.userDataDir });
      }
    } catch (error) {
      this.testLogger?.warn('Failed to cleanup user data directory', { 
        userDataDir: this.userDataDir, 
        error: error.message 
      });
    }
  }

  // Assertion helpers
  async assertWindowTitle(expectedTitle: string | RegExp): Promise<void> {
    const mainWindow = await this.getMainWindow();
    const actualTitle = await mainWindow.page.title();
    
    if (typeof expectedTitle === 'string') {
      if (!actualTitle.includes(expectedTitle)) {
        throw new Error(`Expected window title to contain "${expectedTitle}", but got "${actualTitle}"`);
      }
    } else {
      if (!expectedTitle.test(actualTitle)) {
        throw new Error(`Expected window title to match pattern, but got "${actualTitle}"`);
      }
    }
    
    this.testLogger?.assertion('Window title matches expected', true, {
      expected: expectedTitle.toString(),
      actual: actualTitle,
    });
  }

  async assertFileOpen(fileName: string): Promise<void> {
    const mainWindow = await this.getMainWindow();
    const page = mainWindow.page;
    
    try {
      await page.waitForSelector(`[title*="${fileName}"]`, { timeout: 5000 });
      this.testLogger?.assertion(`File ${fileName} is open`, true);
    } catch (error) {
      this.testLogger?.assertion(`File ${fileName} is open`, false, { error: error.message });
      throw new Error(`File ${fileName} is not open`);
    }
  }

  async assertExtensionInstalled(extensionId: string): Promise<void> {
    const mainWindow = await this.getMainWindow();
    const page = mainWindow.page;
    
    // Open extensions view
    await this.openCommandPalette(page);
    await page.keyboard.type('Extensions: Show Installed Extensions');
    await page.keyboard.press('Enter');
    
    try {
      await page.waitForSelector(`[data-extension-id="${extensionId}"]`, { timeout: 10000 });
      this.testLogger?.assertion(`Extension ${extensionId} is installed`, true);
    } catch (error) {
      this.testLogger?.assertion(`Extension ${extensionId} is installed`, false, { error: error.message });
      throw new Error(`Extension ${extensionId} is not installed`);
    }
  }
}

// Factory function
export function createVSCodeElectronHelper(
  config: VSCodeConfig = {},
  testLogger?: TestLogger
): VSCodeElectronHelper {
  return new VSCodeElectronHelper(config, testLogger);
}