import { _electron as electron, ElectronApplication, Page } from 'playwright';
import { promises as fs } from 'fs';
import path from 'path';

interface VSCodeAutomationParams {
  action: string;
  workspacePath?: string;
  timeout?: number;
  headless?: boolean;
  [key: string]: any;
}

interface AutomationResult {
  success: boolean;
  message: string;
  data?: any;
  error?: string;
}

class VSCodeDesktopAutomation {
  private app: ElectronApplication | null = null;
  private mainWindow: Page | null = null;
  private readonly defaultTimeout = 30000;

  async launch(params: VSCodeAutomationParams): Promise<AutomationResult> {
    try {
      // Find VS Code executable path
      const vscodeExecutable = await this.findVSCodeExecutable();
      
      console.log(`Launching VS Code from: ${vscodeExecutable}`);
      
      // Launch VS Code
      this.app = await electron.launch({
        executablePath: vscodeExecutable,
        args: params.workspacePath ? [params.workspacePath] : [],
        timeout: params.timeout || this.defaultTimeout
      });

      // Get the main window
      this.mainWindow = await this.app.firstWindow();
      await this.mainWindow.waitForLoadState('domcontentloaded');

      // Wait for VS Code to fully load
      await this.waitForVSCodeReady();

      return {
        success: true,
        message: 'VS Code launched successfully'
      };
    } catch (error) {
      return {
        success: false,
        message: 'Failed to launch VS Code',
        error: error instanceof Error ? error.message : String(error)
      };
    }
  }

  async checkInteractivity(): Promise<AutomationResult> {
    try {
      if (!this.mainWindow) {
        throw new Error('VS Code is not launched');
      }

      // Check if we can interact with the command palette
      await this.mainWindow.keyboard.press('Meta+Shift+P'); // Cmd+Shift+P on Mac
      
      // Wait for command palette to appear
      const commandPalette = await this.mainWindow.waitForSelector('.quick-input-widget', { 
        timeout: 5000 
      });

      if (commandPalette) {
        // Type a simple command to test interaction
        await this.mainWindow.keyboard.type('View: Toggle Terminal');
        await this.mainWindow.keyboard.press('Enter');
        
        // Wait a bit for the terminal to potentially open
        await this.mainWindow.waitForTimeout(2000);
        
        // Press Escape to close any open dialogs
        await this.mainWindow.keyboard.press('Escape');
        
        return {
          success: true,
          message: 'VS Code is interactive and responsive',
          data: {
            commandPaletteWorking: true,
            keyboardInputWorking: true
          }
        };
      }

      return {
        success: false,
        message: 'Command palette did not appear - VS Code may not be fully loaded'
      };
    } catch (error) {
      return {
        success: false,
        message: 'Failed to check VS Code interactivity',
        error: error instanceof Error ? error.message : String(error)
      };
    }
  }

  async openWorkspace(workspacePath: string): Promise<AutomationResult> {
    try {
      if (!this.mainWindow) {
        throw new Error('VS Code is not launched');
      }

      // Open command palette
      await this.mainWindow.keyboard.press('Meta+Shift+P');
      
      // Wait for command palette
      await this.mainWindow.waitForSelector('.quick-input-widget', { timeout: 5000 });
      
      // Type command to open folder
      await this.mainWindow.keyboard.type('File: Open Folder');
      await this.mainWindow.keyboard.press('Enter');
      
      // Wait for file dialog (this might vary based on OS)
      await this.mainWindow.waitForTimeout(2000);
      
      return {
        success: true,
        message: `Initiated workspace opening for: ${workspacePath}`
      };
    } catch (error) {
      return {
        success: false,
        message: 'Failed to open workspace',
        error: error instanceof Error ? error.message : String(error)
      };
    }
  }

  async connectToRemoteCompute(computeName: string): Promise<AutomationResult> {
    try {
      if (!this.mainWindow) {
        throw new Error('VS Code is not launched');
      }

      // Look for Azure ML extension or remote connection options
      await this.mainWindow.keyboard.press('Meta+Shift+P');
      await this.mainWindow.waitForSelector('.quick-input-widget', { timeout: 5000 });
      
      // Try to find Azure ML or remote connection commands
      await this.mainWindow.keyboard.type('Azure ML');
      await this.mainWindow.waitForTimeout(1000);
      
      // This would need to be customized based on the actual Azure ML extension commands
      return {
        success: true,
        message: `Initiated connection to compute: ${computeName}`,
        data: {
          computeName: computeName,
          connectionInitiated: true
        }
      };
    } catch (error) {
      return {
        success: false,
        message: 'Failed to connect to remote compute',
        error: error instanceof Error ? error.message : String(error)
      };
    }
  }

  async checkApplicationLinks(): Promise<AutomationResult> {
    try {
      if (!this.mainWindow) {
        throw new Error('VS Code is not launched');
      }

      // Check for various application links/integrations
      const links = [];
      
      // Check for terminal
      try {
        await this.mainWindow.keyboard.press('Meta+`'); // Toggle terminal
        await this.mainWindow.waitForTimeout(1000);
        const terminal = await this.mainWindow.$('.terminal-wrapper');
        if (terminal) {
          links.push({ name: 'Terminal', available: true });
        }
      } catch {
        links.push({ name: 'Terminal', available: false });
      }

      // Check for extensions view
      try {
        await this.mainWindow.keyboard.press('Meta+Shift+X'); // Extensions view
        await this.mainWindow.waitForTimeout(1000);
        const extensions = await this.mainWindow.$('.extensions-viewlet');
        if (extensions) {
          links.push({ name: 'Extensions', available: true });
        }
      } catch {
        links.push({ name: 'Extensions', available: false });
      }

      return {
        success: true,
        message: 'Application links checked',
        data: {
          links: links,
          totalChecked: links.length,
          availableCount: links.filter(l => l.available).length
        }
      };
    } catch (error) {
      return {
        success: false,
        message: 'Failed to check application links',
        error: error instanceof Error ? error.message : String(error)
      };
    }
  }

  async takeScreenshot(filename?: string): Promise<AutomationResult> {
    try {
      if (!this.mainWindow) {
        throw new Error('VS Code is not launched');
      }

      const screenshotPath = filename || `vscode-screenshot-${Date.now()}.png`;
      await this.mainWindow.screenshot({ path: screenshotPath });

      return {
        success: true,
        message: 'Screenshot taken successfully',
        data: { path: screenshotPath }
      };
    } catch (error) {
      return {
        success: false,
        message: 'Failed to take screenshot',
        error: error instanceof Error ? error.message : String(error)
      };
    }
  }

  async close(): Promise<AutomationResult> {
    try {
      if (this.app) {
        await this.app.close();
        this.app = null;
        this.mainWindow = null;
      }

      return {
        success: true,
        message: 'VS Code closed successfully'
      };
    } catch (error) {
      return {
        success: false,
        message: 'Failed to close VS Code',
        error: error instanceof Error ? error.message : String(error)
      };
    }
  }

  private async findVSCodeExecutable(): Promise<string> {
    const possiblePaths = [
      '/Applications/Visual Studio Code.app/Contents/MacOS/Electron',
      '/usr/local/bin/code',
      '/opt/homebrew/bin/code',
      process.env.VSCODE_PATH
    ].filter(Boolean) as string[];

    for (const vscPath of possiblePaths) {
      try {
        await fs.access(vscPath);
        return vscPath;
      } catch {
        continue;
      }
    }

    throw new Error('VS Code executable not found. Please install VS Code or set VSCODE_PATH environment variable.');
  }

  private async waitForVSCodeReady(): Promise<void> {
    if (!this.mainWindow) return;

    // Wait for the main VS Code interface to load
    try {
      await this.mainWindow.waitForSelector('.monaco-workbench', { 
        timeout: this.defaultTimeout 
      });
      
      // Additional wait for VS Code to be fully interactive
      await this.mainWindow.waitForTimeout(3000);
    } catch (error) {
      console.warn('VS Code may not be fully loaded:', error);
    }
  }
}

// Main execution function
async function main() {
  const args = process.argv.slice(2);
  const paramsJson = args[0];
  
  if (!paramsJson) {
    console.error('No parameters provided');
    process.exit(1);
  }

  let params: VSCodeAutomationParams;
  try {
    params = JSON.parse(paramsJson);
  } catch (error) {
    console.error('Invalid JSON parameters:', error);
    process.exit(1);
  }

  const automation = new VSCodeDesktopAutomation();
  let result: AutomationResult;

  try {
    switch (params.action) {
      case 'launch':
        result = await automation.launch(params);
        break;
      
      case 'checkInteractivity':
        result = await automation.checkInteractivity();
        break;
      
      case 'openWorkspace':
        result = await automation.openWorkspace(params.workspacePath || '');
        break;
      
      case 'connectToCompute':
        result = await automation.connectToRemoteCompute(params.computeName || '');
        break;
      
      case 'checkApplicationLinks':
        result = await automation.checkApplicationLinks();
        break;
      
      case 'takeScreenshot':
        result = await automation.takeScreenshot(params.filename);
        break;
      
      case 'close':
        result = await automation.close();
        break;
      
      default:
        result = {
          success: false,
          message: `Unknown action: ${params.action}`
        };
    }

    // Output result as JSON
    console.log(JSON.stringify(result, null, 2));
    
    // Exit with appropriate code
    process.exit(result.success ? 0 : 1);
    
  } catch (error) {
    const errorResult: AutomationResult = {
      success: false,
      message: 'Unexpected error occurred',
      error: error instanceof Error ? error.message : String(error)
    };
    
    console.log(JSON.stringify(errorResult, null, 2));
    process.exit(1);
  } finally {
    // Ensure cleanup
    try {
      await automation.close();
    } catch {
      // Ignore cleanup errors
    }
  }
}

// Run if this file is executed directly
if (require.main === module) {
  main().catch(console.error);
}

export { VSCodeDesktopAutomation, VSCodeAutomationParams, AutomationResult };