import { Page } from '@playwright/test';
import { BasePage } from './base-page';
import { TestLogger } from '../helpers/logger';

export class JupyterLabPage extends BasePage {
  // Selectors
  private readonly selectors = {
    // Main interface
    mainArea: '.jp-MainAreaWidget',
    sidebar: '.jp-SideBar',
    menuBar: '.jp-MenuBar',
    statusBar: '.jp-StatusBar',
    
    // File browser
    fileBrowser: '.jp-FileBrowser',
    fileBrowserToolbar: '.jp-FileBrowser-toolbar',
    fileList: '.jp-DirListing',
    fileItem: '.jp-DirListing-item',
    
    // Notebook
    notebook: '.jp-Notebook',
    notebookPanel: '.jp-NotebookPanel',
    cell: '.jp-Cell',
    codeCell: '.jp-Cell[data-type="code"]',
    markdownCell: '.jp-Cell[data-type="markdown"]',
    cellInput: '.jp-InputArea-editor',
    cellOutput: '.jp-OutputArea',
    cellPrompt: '.jp-InputPrompt',
    
    // Kernel
    kernelName: '.jp-Toolbar-kernelName',
    kernelStatus: '.jp-Toolbar-kernelStatus',
    kernelIndicator: '.jp-KernelIndicator',
    
    // Toolbar
    toolbar: '.jp-Toolbar',
    runButton: '[data-command="notebook:run-cell-and-select-next"]',
    stopButton: '[data-command="notebook:interrupt-kernel"]',
    restartButton: '[data-command="notebook:restart-kernel"]',
    
    // Launcher
    launcher: '.jp-Launcher',
    launcherCard: '.jp-LauncherCard',
    
    // Dialogs
    dialog: '.jp-Dialog',
    dialogContent: '.jp-Dialog-content',
    dialogButton: '.jp-Dialog-button',
    
    // Command palette
    commandPalette: '.jp-CommandPalette',
    commandInput: '.jp-CommandPalette-input',
    commandItem: '.jp-CommandPalette-item',
    
    // Loading
    loadingIndicator: '.jp-SpinnerContent',
    busyIndicator: '[data-status="busy"]',
  };

  constructor(page: Page, testLogger?: TestLogger) {
    super(page, testLogger);
  }

  async isPageLoaded(): Promise<boolean> {
    try {
      await this.waitForElement(this.selectors.mainArea, 10000);
      await this.waitForElement(this.selectors.sidebar, 10000);
      return true;
    } catch {
      return false;
    }
  }

  getPageIdentifier(): string {
    return 'JupyterLab';
  }

  async waitForPageLoad(): Promise<void> {
    await super.waitForPageLoad();
    
    // Wait for JupyterLab interface to load
    await this.waitForElement(this.selectors.mainArea);
    await this.waitForElement(this.selectors.sidebar);
    
    // Wait for loading indicators to disappear
    try {
      await this.page.waitForSelector(this.selectors.loadingIndicator, { 
        state: 'detached', 
        timeout: 30000 
      });
    } catch {
      // Loading indicator might not appear
    }
  }

  // File operations
  async openFile(fileName: string): Promise<void> {
    this.testLogger?.action(`Opening file: ${fileName}`);
    
    // Navigate to file in browser
    await this.waitForElement(this.selectors.fileBrowser);
    
    // Find and double-click the file
    const fileItem = this.page.locator(`${this.selectors.fileItem}:has-text("${fileName}")`);
    await fileItem.dblclick();
    
    // Wait for file to open
    await this.waitForElement(`[data-jp-code-runner]:has-text("${fileName}")`, 10000);
    this.testLogger?.info(`File opened: ${fileName}`);
  }

  async createNotebook(kernelName: string = 'Python 3'): Promise<void> {
    this.testLogger?.action(`Creating new notebook with kernel: ${kernelName}`);
    
    // Open launcher if not visible
    if (!(await this.isElementVisible(this.selectors.launcher))) {
      await this.openCommandPalette();
      await this.typeText('launcher');
      await this.pressKey('Enter');
    }
    
    // Find and click the notebook launcher card
    const notebookCard = this.page.locator(`${this.selectors.launcherCard}:has-text("${kernelName}")`);
    await notebookCard.click();
    
    // Wait for notebook to be created
    await this.waitForElement(this.selectors.notebook);
    this.testLogger?.info(`Notebook created with kernel: ${kernelName}`);
  }

  async saveNotebook(fileName?: string): Promise<void> {
    this.testLogger?.action(`Saving notebook${fileName ? `: ${fileName}` : ''}`);
    
    // Use Ctrl+S to save
    await this.pressKey('Control+s');
    
    // If filename provided, enter it in the save dialog
    if (fileName) {
      await this.waitForElement(this.selectors.dialog);
      await this.fillInput('.jp-Dialog-content input', fileName);
      await this.clickElement('.jp-Dialog-button.jp-mod-accept');
    }
    
    this.testLogger?.info(`Notebook saved${fileName ? `: ${fileName}` : ''}`);
  }

  // Cell operations
  async addCodeCell(): Promise<void> {
    this.testLogger?.action('Adding code cell');
    
    // Use 'b' key to add cell below
    await this.pressKey('b');
    
    // Ensure it's a code cell
    await this.pressKey('y');
    
    this.testLogger?.info('Code cell added');
  }

  async addMarkdownCell(): Promise<void> {
    this.testLogger?.action('Adding markdown cell');
    
    // Use 'b' key to add cell below
    await this.pressKey('b');
    
    // Convert to markdown cell
    await this.pressKey('m');
    
    this.testLogger?.info('Markdown cell added');
  }

  async selectCell(cellIndex: number): Promise<void> {
    this.testLogger?.action(`Selecting cell: ${cellIndex}`);
    
    const cells = await this.page.locator(this.selectors.cell).all();
    if (cellIndex >= cells.length) {
      throw new Error(`Cell index ${cellIndex} out of range. Total cells: ${cells.length}`);
    }
    
    await cells[cellIndex].click();
    this.testLogger?.info(`Cell selected: ${cellIndex}`);
  }

  async editCell(cellIndex: number, content: string): Promise<void> {
    this.testLogger?.action(`Editing cell ${cellIndex} with content: ${content}`);
    
    await this.selectCell(cellIndex);
    
    // Enter edit mode
    await this.pressKey('Enter');
    
    // Clear existing content and type new content
    await this.pressKey('Control+a');
    await this.typeText(content);
    
    // Exit edit mode
    await this.pressKey('Escape');
    
    this.testLogger?.info(`Cell ${cellIndex} edited`);
  }

  async runCell(cellIndex?: number): Promise<void> {
    if (cellIndex !== undefined) {
      await this.selectCell(cellIndex);
      this.testLogger?.action(`Running cell: ${cellIndex}`);
    } else {
      this.testLogger?.action('Running current cell');
    }
    
    // Use Shift+Enter to run cell
    await this.pressKey('Shift+Enter');
    
    // Wait for execution to complete
    await this.waitForCellExecution();
    
    this.testLogger?.info(`Cell execution completed`);
  }

  async runAllCells(): Promise<void> {
    this.testLogger?.action('Running all cells');
    
    await this.openCommandPalette();
    await this.typeText('Run All Cells');
    await this.pressKey('Enter');
    
    // Wait for all executions to complete
    await this.waitForAllCellsExecution();
    
    this.testLogger?.info('All cells executed');
  }

  async clearCellOutput(cellIndex: number): Promise<void> {
    this.testLogger?.action(`Clearing output for cell: ${cellIndex}`);
    
    await this.selectCell(cellIndex);
    
    await this.openCommandPalette();
    await this.typeText('Clear Cell Output');
    await this.pressKey('Enter');
    
    this.testLogger?.info(`Cell ${cellIndex} output cleared`);
  }

  async clearAllOutputs(): Promise<void> {
    this.testLogger?.action('Clearing all cell outputs');
    
    await this.openCommandPalette();
    await this.typeText('Clear All Outputs');
    await this.pressKey('Enter');
    
    this.testLogger?.info('All cell outputs cleared');
  }

  // Kernel operations
  async getKernelName(): Promise<string> {
    const kernelElement = await this.waitForElement(this.selectors.kernelName);
    return await kernelElement.textContent() || 'Unknown';
  }

  async getKernelStatus(): Promise<string> {
    const statusElement = await this.waitForElement(this.selectors.kernelStatus);
    return await statusElement.textContent() || 'Unknown';
  }

  async restartKernel(): Promise<void> {
    this.testLogger?.action('Restarting kernel');
    
    await this.openCommandPalette();
    await this.typeText('Restart Kernel');
    await this.pressKey('Enter');
    
    // Confirm restart in dialog
    await this.waitForElement(this.selectors.dialog);
    await this.clickElement('.jp-Dialog-button.jp-mod-accept');
    
    // Wait for kernel to restart
    await this.waitForKernelReady();
    
    this.testLogger?.info('Kernel restarted');
  }

  async interruptKernel(): Promise<void> {
    this.testLogger?.action('Interrupting kernel');
    
    await this.clickElement(this.selectors.stopButton);
    
    this.testLogger?.info('Kernel interrupted');
  }

  async changeKernel(kernelName: string): Promise<void> {
    this.testLogger?.action(`Changing kernel to: ${kernelName}`);
    
    await this.clickElement(this.selectors.kernelName);
    
    // Wait for kernel selector dialog
    await this.waitForElement(this.selectors.dialog);
    
    // Select the desired kernel
    await this.clickElement(`${this.selectors.dialogContent} text=${kernelName}`);
    
    // Confirm selection
    await this.clickElement('.jp-Dialog-button.jp-mod-accept');
    
    // Wait for kernel to be ready
    await this.waitForKernelReady();
    
    this.testLogger?.info(`Kernel changed to: ${kernelName}`);
  }

  // Utility methods
  async openCommandPalette(): Promise<void> {
    await this.pressKey('Control+Shift+c');
    await this.waitForElement(this.selectors.commandPalette);
  }

  async waitForCellExecution(timeout: number = 300000): Promise<void> {
    // Wait for busy indicator to appear and then disappear
    try {
      await this.page.waitForSelector(this.selectors.busyIndicator, { timeout: 5000 });
      await this.page.waitForSelector(this.selectors.busyIndicator, { 
        state: 'detached', 
        timeout 
      });
    } catch {
      // Cell might execute too quickly for busy indicator to appear
      await this.page.waitForTimeout(1000);
    }
  }

  async waitForAllCellsExecution(timeout: number = 600000): Promise<void> {
    const startTime = Date.now();
    
    while (Date.now() - startTime < timeout) {
      const busyIndicators = await this.page.locator(this.selectors.busyIndicator).count();
      if (busyIndicators === 0) {
        return;
      }
      await this.page.waitForTimeout(2000);
    }
    
    throw new Error('Timeout waiting for all cells to complete execution');
  }

  async waitForKernelReady(timeout: number = 60000): Promise<void> {
    const startTime = Date.now();
    
    while (Date.now() - startTime < timeout) {
      const status = await this.getKernelStatus();
      if (status.toLowerCase().includes('idle') || status.toLowerCase().includes('ready')) {
        return;
      }
      await this.page.waitForTimeout(2000);
    }
    
    throw new Error('Timeout waiting for kernel to be ready');
  }

  async getCellCount(): Promise<number> {
    const cells = await this.page.locator(this.selectors.cell).all();
    return cells.length;
  }

  async getCellContent(cellIndex: number): Promise<string> {
    const cells = await this.page.locator(this.selectors.cell).all();
    if (cellIndex >= cells.length) {
      throw new Error(`Cell index ${cellIndex} out of range. Total cells: ${cells.length}`);
    }
    
    const cellInput = cells[cellIndex].locator(this.selectors.cellInput);
    return await cellInput.textContent() || '';
  }

  async getCellOutput(cellIndex: number): Promise<string> {
    const cells = await this.page.locator(this.selectors.cell).all();
    if (cellIndex >= cells.length) {
      throw new Error(`Cell index ${cellIndex} out of range. Total cells: ${cells.length}`);
    }
    
    const cellOutput = cells[cellIndex].locator(this.selectors.cellOutput);
    return await cellOutput.textContent() || '';
  }

  async getCellType(cellIndex: number): Promise<string> {
    const cells = await this.page.locator(this.selectors.cell).all();
    if (cellIndex >= cells.length) {
      throw new Error(`Cell index ${cellIndex} out of range. Total cells: ${cells.length}`);
    }
    
    const cellType = await cells[cellIndex].getAttribute('data-type');
    return cellType || 'unknown';
  }

  async hasCellError(cellIndex: number): Promise<boolean> {
    const cells = await this.page.locator(this.selectors.cell).all();
    if (cellIndex >= cells.length) {
      throw new Error(`Cell index ${cellIndex} out of range. Total cells: ${cells.length}`);
    }
    
    const errorOutput = cells[cellIndex].locator('.jp-OutputArea-output[data-mime-type="application/vnd.jupyter.stderr"]');
    return await errorOutput.count() > 0;
  }

  // Assertion methods
  async assertKernelReady(): Promise<void> {
    const status = await this.getKernelStatus();
    if (!status.toLowerCase().includes('idle') && !status.toLowerCase().includes('ready')) {
      throw new Error(`Expected kernel to be ready, but status is: ${status}`);
    }
    
    this.testLogger?.assertion('Kernel is ready', true, { status });
  }

  async assertCellCount(expectedCount: number): Promise<void> {
    const actualCount = await this.getCellCount();
    if (actualCount !== expectedCount) {
      throw new Error(`Expected ${expectedCount} cells, but found ${actualCount}`);
    }
    
    this.testLogger?.assertion(`Cell count is ${expectedCount}`, true, { actualCount });
  }

  async assertCellOutput(cellIndex: number, expectedOutput: string | RegExp): Promise<void> {
    const actualOutput = await this.getCellOutput(cellIndex);
    
    if (typeof expectedOutput === 'string') {
      if (!actualOutput.includes(expectedOutput)) {
        throw new Error(`Expected cell ${cellIndex} output to contain "${expectedOutput}", but got: ${actualOutput}`);
      }
    } else {
      if (!expectedOutput.test(actualOutput)) {
        throw new Error(`Expected cell ${cellIndex} output to match pattern, but got: ${actualOutput}`);
      }
    }
    
    this.testLogger?.assertion(`Cell ${cellIndex} output matches expected`, true, {
      expected: expectedOutput.toString(),
      actual: actualOutput,
    });
  }

  async assertNoCellErrors(): Promise<void> {
    const cellCount = await this.getCellCount();
    const errorCells = [];
    
    for (let i = 0; i < cellCount; i++) {
      if (await this.hasCellError(i)) {
        errorCells.push(i);
      }
    }
    
    if (errorCells.length > 0) {
      throw new Error(`Found errors in cells: ${errorCells.join(', ')}`);
    }
    
    this.testLogger?.assertion('No cell errors found', true, { cellCount });
  }

  async assertCellType(cellIndex: number, expectedType: string): Promise<void> {
    const actualType = await this.getCellType(cellIndex);
    if (actualType !== expectedType) {
      throw new Error(`Expected cell ${cellIndex} to be type "${expectedType}", but got "${actualType}"`);
    }
    
    this.testLogger?.assertion(`Cell ${cellIndex} is type ${expectedType}`, true, { actualType });
  }
}