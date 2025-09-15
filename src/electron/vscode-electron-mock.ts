/**
 * Mock VS Code Electron Helper for Testing
 * This provides mock implementations of VS Code operations for testing purposes
 */

import { logger } from '../helpers/logger';
import * as path from 'path';
import * as fs from 'fs';

export interface VSCodeWindow {
  id: string;
  title: string;
  isVisible: boolean;
  workspaceFolder?: string;
}

export interface NotebookCell {
  type: 'code' | 'markdown';
  content: string;
  output?: string;
  executionCount?: number;
}

export class MockVSCodeElectronHelper {
  private isLaunched = false;
  private currentWorkspace?: string;
  private openFiles: string[] = [];
  private notebooks: Map<string, NotebookCell[]> = new Map();

  constructor() {
    logger.info('Mock VS Code Electron Helper initialized');
  }

  async launch(workspaceFolder?: string): Promise<void> {
    logger.info('Mock: Launching VS Code', { workspaceFolder });
    
    this.isLaunched = true;
    this.currentWorkspace = workspaceFolder;
    
    // Simulate launch delay
    await this.delay(1000);
    
    logger.info('Mock: VS Code launched successfully');
  }

  async close(): Promise<void> {
    logger.info('Mock: Closing VS Code');
    
    this.isLaunched = false;
    this.currentWorkspace = undefined;
    this.openFiles = [];
    this.notebooks.clear();
    
    logger.info('Mock: VS Code closed');
  }

  async openWorkspace(workspaceFolder: string): Promise<void> {
    logger.info('Mock: Opening workspace', { workspaceFolder });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    this.currentWorkspace = workspaceFolder;
    
    // Simulate workspace opening
    await this.delay(500);
    
    logger.info('Mock: Workspace opened successfully');
  }

  async openFile(filePath: string): Promise<void> {
    logger.info('Mock: Opening file', { filePath });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    // Check if file exists
    if (!fs.existsSync(filePath)) {
      throw new Error(`File not found: ${filePath}`);
    }
    
    if (!this.openFiles.includes(filePath)) {
      this.openFiles.push(filePath);
    }
    
    await this.delay(300);
    
    logger.info('Mock: File opened successfully');
  }

  async createFile(filePath: string, content: string): Promise<void> {
    logger.info('Mock: Creating file', { filePath });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    // Ensure directory exists
    const dir = path.dirname(filePath);
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
    }
    
    // Write file content
    fs.writeFileSync(filePath, content);
    
    // Add to open files
    if (!this.openFiles.includes(filePath)) {
      this.openFiles.push(filePath);
    }
    
    await this.delay(200);
    
    logger.info('Mock: File created successfully');
  }

  async createNotebook(notebookPath: string, cells: NotebookCell[]): Promise<void> {
    logger.info('Mock: Creating notebook', { notebookPath, cellCount: cells.length });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    // Store notebook cells
    this.notebooks.set(notebookPath, cells);
    
    // Create notebook file
    const notebookContent = {
      cells: cells.map(cell => ({
        cell_type: cell.type,
        source: [cell.content],
        metadata: {},
        outputs: cell.output ? [{ output_type: 'stream', text: [cell.output] }] : [],
        execution_count: cell.executionCount || null
      })),
      metadata: {
        kernelspec: {
          display_name: 'Python 3',
          language: 'python',
          name: 'python3'
        }
      },
      nbformat: 4,
      nbformat_minor: 4
    };
    
    // Ensure directory exists
    const dir = path.dirname(notebookPath);
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
    }
    
    fs.writeFileSync(notebookPath, JSON.stringify(notebookContent, null, 2));
    
    // Add to open files
    if (!this.openFiles.includes(notebookPath)) {
      this.openFiles.push(notebookPath);
    }
    
    await this.delay(500);
    
    logger.info('Mock: Notebook created successfully');
  }

  async executeNotebookCell(notebookPath: string, cellIndex: number): Promise<string> {
    logger.info('Mock: Executing notebook cell', { notebookPath, cellIndex });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    const cells = this.notebooks.get(notebookPath);
    if (!cells || cellIndex >= cells.length) {
      throw new Error('Invalid cell index');
    }
    
    const cell = cells[cellIndex];
    
    // Simulate execution delay
    await this.delay(1000);
    
    // Mock execution output based on cell content
    let output = '';
    if (cell.content.includes('print(')) {
      const match = cell.content.match(/print\(['"](.+?)['"]\)/);
      output = match ? match[1] : 'Mock output';
    } else if (cell.content.includes('import')) {
      output = 'Libraries imported successfully';
    } else if (cell.content.includes('=')) {
      output = 'Variable assigned';
    } else {
      output = 'Cell executed successfully';
    }
    
    // Update cell with output
    cell.output = output;
    cell.executionCount = (cell.executionCount || 0) + 1;
    
    logger.info('Mock: Cell executed successfully', { output });
    
    return output;
  }

  async executeAllNotebookCells(notebookPath: string): Promise<string[]> {
    logger.info('Mock: Executing all notebook cells', { notebookPath });
    
    const cells = this.notebooks.get(notebookPath);
    if (!cells) {
      throw new Error('Notebook not found');
    }
    
    const outputs: string[] = [];
    
    for (let i = 0; i < cells.length; i++) {
      if (cells[i].type === 'code') {
        const output = await this.executeNotebookCell(notebookPath, i);
        outputs.push(output);
      }
    }
    
    logger.info('Mock: All cells executed successfully');
    
    return outputs;
  }

  async connectToRemoteCompute(computeName: string): Promise<void> {
    logger.info('Mock: Connecting to remote compute', { computeName });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    // Simulate connection delay
    await this.delay(2000);
    
    logger.info('Mock: Connected to remote compute successfully');
  }

  async executeRemoteCommand(command: string): Promise<string> {
    logger.info('Mock: Executing remote command', { command });
    
    // Simulate command execution
    await this.delay(1500);
    
    const output = `Mock execution of: ${command}\nCommand completed successfully`;
    
    logger.info('Mock: Remote command executed', { output });
    
    return output;
  }

  async installExtension(extensionId: string): Promise<void> {
    logger.info('Mock: Installing extension', { extensionId });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    // Simulate installation delay
    await this.delay(3000);
    
    logger.info('Mock: Extension installed successfully');
  }

  async getOpenFiles(): Promise<string[]> {
    return [...this.openFiles];
  }

  async getCurrentWorkspace(): Promise<string | undefined> {
    return this.currentWorkspace;
  }

  async isRunning(): Promise<boolean> {
    return this.isLaunched;
  }

  async getWindows(): Promise<VSCodeWindow[]> {
    if (!this.isLaunched) {
      return [];
    }
    
    return [
      {
        id: 'main-window',
        title: 'Visual Studio Code',
        isVisible: true,
        workspaceFolder: this.currentWorkspace
      }
    ];
  }

  async waitForWorkbenchLoad(): Promise<void> {
    logger.info('Mock: Waiting for workbench to load');
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    // Simulate workbench loading time
    await this.delay(2000);
    
    logger.info('Mock: Workbench loaded successfully');
  }

  async openCommandPalette(): Promise<void> {
    logger.info('Mock: Opening command palette');
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(300);
    
    logger.info('Mock: Command palette opened');
  }

  async executeCommand(command: string): Promise<void> {
    logger.info('Mock: Executing command', { command });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    // Simulate command execution
    await this.delay(1000);
    
    logger.info('Mock: Command executed successfully');
  }

  async waitForExtensionHost(): Promise<void> {
    logger.info('Mock: Waiting for extension host');
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(1500);
    
    logger.info('Mock: Extension host ready');
  }

  async openTerminal(): Promise<void> {
    logger.info('Mock: Opening terminal');
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(500);
    
    logger.info('Mock: Terminal opened');
  }

  async runTerminalCommand(command: string): Promise<string> {
    logger.info('Mock: Running terminal command', { command });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(1000);
    
    const output = `Mock terminal output for: ${command}`;
    
    logger.info('Mock: Terminal command completed', { output });
    
    return output;
  }

  async selectKernel(kernelName: string): Promise<void> {
    logger.info('Mock: Selecting kernel', { kernelName });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(800);
    
    logger.info('Mock: Kernel selected successfully');
  }

  async waitForNotebookLoad(notebookPath: string): Promise<void> {
    logger.info('Mock: Waiting for notebook to load', { notebookPath });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(1500);
    
    logger.info('Mock: Notebook loaded successfully');
  }

  async waitForExtensionActivation(extensionId: string): Promise<void> {
    logger.info('Mock: Waiting for extension activation', { extensionId });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(2000);
    
    logger.info('Mock: Extension activated successfully');
  }

  async connectToAzure(config: any): Promise<void> {
    logger.info('Mock: Connecting to Azure', { config });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(3000);
    
    logger.info('Mock: Connected to Azure successfully');
  }

  async selectAzureSubscription(subscriptionId: string): Promise<void> {
    logger.info('Mock: Selecting Azure subscription', { subscriptionId });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(1000);
    
    logger.info('Mock: Azure subscription selected');
  }

  async selectAzureMLWorkspace(workspaceName: string): Promise<void> {
    logger.info('Mock: Selecting Azure ML workspace', { workspaceName });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(1500);
    
    logger.info('Mock: Azure ML workspace selected');
  }

  async verifyAzureConnection(): Promise<boolean> {
    logger.info('Mock: Verifying Azure connection');
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(1000);
    
    logger.info('Mock: Azure connection verified');
    
    return true;
  }

  async createPythonFile(filePath: string, content: string): Promise<void> {
    logger.info('Mock: Creating Python file', { filePath });
    
    await this.createFile(filePath, content);
    
    logger.info('Mock: Python file created successfully');
  }

  async runPythonCode(code: string): Promise<string> {
    logger.info('Mock: Running Python code', { code });
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(2000);
    
    const output = `Mock Python execution output for: ${code.substring(0, 50)}...`;
    
    logger.info('Mock: Python code executed', { output });
    
    return output;
  }

  async getAzureMLWorkspaceStatus(): Promise<any> {
    logger.info('Mock: Getting Azure ML workspace status');
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(500);
    
    const status = {
      connected: true,
      workspaceName: 'test-workspace',
      subscriptionId: 'test-subscription-id',
      resourceGroup: 'test-resource-group',
      location: 'eastus',
      status: 'Ready'
    };
    
    logger.info('Mock: Azure ML workspace status retrieved', { status });
    
    return status;
  }

  async getAvailableCommands(): Promise<string[]> {
    logger.info('Mock: Getting available commands');
    
    if (!this.isLaunched) {
      throw new Error('VS Code is not launched');
    }
    
    await this.delay(300);
    
    const commands = [
      'azureml.createWorkspace',
      'azureml.connectToWorkspace',
      'azureml.createComputeInstance',
      'azureml.submitJob',
      'azureml.viewExperiments',
      'Azure ML: Create New Experiment',
      'Azure ML: Upload Data',
      'Azure ML: Train Model',
      'workbench.action.openSettings',
      'workbench.action.showCommands',
      'python.createTerminal',
      'jupyter.createNewNotebook'
    ];
    
    logger.info('Mock: Available commands retrieved', { commandCount: commands.length });
    
    return commands;
  }

  async takeScreenshot(outputPath: string): Promise<void> {
    logger.info('Mock: Taking screenshot', { outputPath });
    
    // Create a mock screenshot file
    const mockScreenshot = 'Mock screenshot data';
    fs.writeFileSync(outputPath, mockScreenshot);
    
    logger.info('Mock: Screenshot saved');
  }

  private async delay(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }
}

export function createMockVSCodeElectronHelper(): MockVSCodeElectronHelper {
  return new MockVSCodeElectronHelper();
}

// Export the mock as the default helper in test mode
export const createVSCodeElectronHelper = () => {
  if (process.env.NODE_ENV === 'test' || process.env.MOCK_VSCODE === 'true') {
    return createMockVSCodeElectronHelper();
  }
  
  // In non-test mode, this would import and return the real helper
  throw new Error('Real VS Code Electron Helper not available in test mode');
};