import { _electron as electron, ElectronApplication, Page } from '@playwright/test';
import { ConfigManager } from './ConfigManager';
import { Logger } from './Logger';
import * as os from 'os';

export class ElectronUtils {
  private logger: Logger;
  private config: ConfigManager;
  private electronApp: ElectronApplication | null = null;

  constructor() {
    this.logger = Logger.getInstance();
    this.config = ConfigManager.getInstance();
  }

  async launchElectronApp(appPath?: string): Promise<ElectronApplication> {
    const electronConfig = this.config.getElectronSettings();
    let executablePath = appPath;

    if (!executablePath) {
      // Determine executable path based on OS
      const platform = os.platform();
      switch (platform) {
        case 'darwin':
          executablePath = electronConfig.ExecutablePath;
          break;
        case 'win32':
          executablePath = electronConfig.WindowsExecutablePath.replace('%USERNAME%', os.userInfo().username);
          break;
        case 'linux':
          executablePath = electronConfig.LinuxExecutablePath;
          break;
        default:
          throw new Error(`Unsupported platform: ${platform}`);
      }
    }

    this.logger.logAction(`Launching Electron app: ${executablePath}`);
    
    this.electronApp = await electron.launch({
      executablePath,
      args: electronConfig.Args
    });

    // Wait for the app to be ready
    await this.electronApp.evaluate(async ({ app }) => {
      return app.whenReady();
    });

    this.logger.info('Electron app launched successfully');
    return this.electronApp;
  }

  async getMainWindow(): Promise<Page> {
    if (!this.electronApp) {
      throw new Error('Electron app not launched. Call launchElectronApp() first.');
    }

    this.logger.logAction('Getting main window');
    const page = await this.electronApp.firstWindow();
    
    // Wait for the window to be ready
    await page.waitForLoadState('domcontentloaded');
    
    return page;
  }

  async getAllWindows(): Promise<Page[]> {
    if (!this.electronApp) {
      throw new Error('Electron app not launched. Call launchElectronApp() first.');
    }

    this.logger.logAction('Getting all windows');
    const windows = this.electronApp.windows();
    this.logger.info(`Found ${windows.length} windows`);
    return windows;
  }

  async waitForNewWindow(): Promise<Page> {
    if (!this.electronApp) {
      throw new Error('Electron app not launched. Call launchElectronApp() first.');
    }

    this.logger.logAction('Waiting for new window');
    const newWindow = await this.electronApp.waitForEvent('window');
    await newWindow.waitForLoadState('domcontentloaded');
    return newWindow;
  }

  async closeApp(): Promise<void> {
    if (this.electronApp) {
      this.logger.logAction('Closing Electron app');
      await this.electronApp.close();
      this.electronApp = null;
      this.logger.info('Electron app closed');
    }
  }

  async getAppInfo(): Promise<any> {
    if (!this.electronApp) {
      throw new Error('Electron app not launched. Call launchElectronApp() first.');
    }

    const appInfo = await this.electronApp.evaluate(async ({ app }) => {
      return {
        name: app.getName(),
        version: app.getVersion(),
        path: app.getAppPath(),
        isReady: app.isReady()
      };
    });

    this.logger.info('App info retrieved', appInfo);
    return appInfo;
  }

  async takeScreenshotOfWindow(window: Page, fileName?: string): Promise<string> {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const screenshotName = fileName || `electron-screenshot-${timestamp}.png`;
    const screenshotPath = `./Reports/screenshots/${screenshotName}`;
    
    await window.screenshot({ path: screenshotPath, fullPage: true });
    this.logger.logAction(`Electron window screenshot saved: ${screenshotPath}`);
    return screenshotPath;
  }

  async maximizeWindow(window: Page): Promise<void> {
    this.logger.logAction('Maximizing window');
    await this.electronApp?.evaluate(async ({ BrowserWindow }) => {
      const win = BrowserWindow.getFocusedWindow();
      if (win) {
        win.maximize();
      }
    });
  }

  async minimizeWindow(window: Page): Promise<void> {
    this.logger.logAction('Minimizing window');
    await this.electronApp?.evaluate(async ({ BrowserWindow }) => {
      const win = BrowserWindow.getFocusedWindow();
      if (win) {
        win.minimize();
      }
    });
  }

  async setWindowSize(window: Page, width: number, height: number): Promise<void> {
    this.logger.logAction(`Setting window size to ${width}x${height}`);
    await this.electronApp?.evaluate(async ({ BrowserWindow }, { width, height }) => {
      const win = BrowserWindow.getFocusedWindow();
      if (win) {
        win.setSize(width, height);
      }
    }, { width, height });
  }

  async getWindowTitle(window: Page): Promise<string> {
    const title = await window.title();
    this.logger.logAction(`Window title: ${title}`);
    return title;
  }

  async isWindowVisible(window: Page): Promise<boolean> {
    const isVisible = await this.electronApp?.evaluate(async ({ BrowserWindow }) => {
      const win = BrowserWindow.getFocusedWindow();
      return win ? win.isVisible() : false;
    });
    
    this.logger.logAction(`Window visible: ${isVisible}`);
    return isVisible || false;
  }

  async focusWindow(window: Page): Promise<void> {
    this.logger.logAction('Focusing window');
    await this.electronApp?.evaluate(async ({ BrowserWindow }) => {
      const win = BrowserWindow.getFocusedWindow();
      if (win) {
        win.focus();
      }
    });
  }

  // Menu interaction methods
  async clickMenuItem(menuPath: string[]): Promise<void> {
    this.logger.logAction(`Clicking menu item: ${menuPath.join(' > ')}`);
    
    await this.electronApp?.evaluate(async ({ Menu }, menuPath) => {
      const menu = Menu.getApplicationMenu();
      if (menu) {
        let currentMenu = menu;
        for (let i = 0; i < menuPath.length - 1; i++) {
          const menuItem = currentMenu.items.find((item: any) => item.label === menuPath[i]);
          if (menuItem && menuItem.submenu) {
            currentMenu = menuItem.submenu;
          } else {
            throw new Error(`Menu item not found: ${menuPath[i]}`);
          }
        }
        
        const finalMenuItem = currentMenu.items.find((item: any) => item.label === menuPath[menuPath.length - 1]);
        if (finalMenuItem && finalMenuItem.click) {
          finalMenuItem.click();
        } else {
          throw new Error(`Menu item not found: ${menuPath[menuPath.length - 1]}`);
        }
      }
    }, menuPath);
  }

  // Dialog handling
  async handleFileDialog(filePath: string): Promise<void> {
    this.logger.logAction(`Handling file dialog with: ${filePath}`);
    
    if (this.electronApp) {
      this.electronApp.on('window', async (window) => {
        window.on('filechooser', async (fileChooser) => {
          await fileChooser.setFiles(filePath);
        });
      });
    }
  }

  // Console log capture
  async captureConsoleLogs(window: Page): Promise<void> {
    window.on('console', msg => {
      this.logger.info(`Console ${msg.type()}: ${msg.text()}`);
    });
  }

  // Error handling
  async captureErrors(window: Page): Promise<void> {
    window.on('pageerror', error => {
      this.logger.error(`Page error: ${error.message}`);
    });
  }

  // VS Code specific methods
  async isVSCodeRunning(): Promise<boolean> {
    this.logger.logAction('Checking if VS Code is running');
    
    try {
      // Check if VS Code window exists
      const windows = await this.electronApp?.windows();
      if (windows && windows.length > 0) {
        // Look for VS Code specific elements or title
        const vsCodeWindow = windows.find(window => 
          window.url().includes('vscode') || 
          window.url().includes('code')
        );
        return !!vsCodeWindow;
      }
      return false;
    } catch (error) {
      this.logger.logWarning(`Error checking VS Code status: ${error}`);
      return false;
    }
  }

  async openVSCodeFile(fileName: string): Promise<void> {
    this.logger.logAction(`Opening file in VS Code: ${fileName}`);
    
    try {
      // Simulate opening a file in VS Code
      const windows = await this.electronApp?.windows();
      if (windows && windows.length > 0) {
        const vsCodeWindow = windows[0];
        
        // Try to use keyboard shortcut to open file
        await vsCodeWindow.keyboard.press('Control+O'); // Ctrl+O to open file
        await vsCodeWindow.waitForTimeout(1000);
        
        // Type the filename
        await vsCodeWindow.keyboard.type(fileName);
        await vsCodeWindow.keyboard.press('Enter');
        
        this.logger.logInfo(`Successfully opened file: ${fileName}`);
      } else {
        throw new Error('No VS Code windows found');
      }
    } catch (error) {
      this.logger.logError(`Failed to open file in VS Code: ${error}`);
      throw error;
    }
  }
}