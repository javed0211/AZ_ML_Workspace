import { Page, Locator } from '@playwright/test';
import { logger, TestLogger } from '../helpers/logger';
import { config } from '../helpers/config';
import { cliRunner, CliResult } from '../helpers/cli-runner';
import * as path from 'path';
import * as fs from 'fs';

export interface NotebookCell {
  type: 'code' | 'markdown';
  source: string[];
  outputs?: any[];
  executionCount?: number;
}

export interface NotebookContent {
  cells: NotebookCell[];
  metadata: any;
  nbformat: number;
  nbformat_minor: number;
}

export interface CellExecutionResult {
  cellIndex: number;
  executionCount: number;
  outputs: any[];
  executionTime: number;
  success: boolean;
  error?: string;
}

export interface NotebookExecutionResult {
  success: boolean;
  totalCells: number;
  executedCells: number;
  failedCells: number;
  totalExecutionTime: number;
  cellResults: CellExecutionResult[];
  error?: string;
}

export class NotebookRunner {
  private page: Page;
  private testLogger?: TestLogger;

  constructor(page: Page, testLogger?: TestLogger) {
    this.page = page;
    this.testLogger = testLogger;
  }

  // UI-based notebook execution
  async executeNotebookUI(notebookPath: string): Promise<NotebookExecutionResult> {
    try {
      this.testLogger?.action(`Executing notebook via UI: ${notebookPath}`);
      
      // Navigate to notebook
      await this.openNotebook(notebookPath);
      
      // Get all code cells
      const cells = await this.getCodeCells();
      const cellResults: CellExecutionResult[] = [];
      let totalExecutionTime = 0;
      let failedCells = 0;

      this.testLogger?.info(`Found ${cells.length} code cells to execute`);

      // Execute each cell
      for (let i = 0; i < cells.length; i++) {
        try {
          const result = await this.executeCellUI(cells[i], i);
          cellResults.push(result);
          totalExecutionTime += result.executionTime;
          
          if (!result.success) {
            failedCells++;
          }

          this.testLogger?.info(`Cell ${i + 1}/${cells.length} executed`, {
            success: result.success,
            executionTime: result.executionTime,
            executionCount: result.executionCount,
          });
        } catch (error) {
          const errorResult: CellExecutionResult = {
            cellIndex: i,
            executionCount: 0,
            outputs: [],
            executionTime: 0,
            success: false,
            error: error.message,
          };
          cellResults.push(errorResult);
          failedCells++;
          
          this.testLogger?.error(`Cell ${i + 1} execution failed`, { error: error.message });
        }
      }

      const result: NotebookExecutionResult = {
        success: failedCells === 0,
        totalCells: cells.length,
        executedCells: cells.length,
        failedCells,
        totalExecutionTime,
        cellResults,
      };

      this.testLogger?.info('Notebook execution completed', {
        success: result.success,
        totalCells: result.totalCells,
        failedCells: result.failedCells,
        totalExecutionTime: result.totalExecutionTime,
      });

      return result;
    } catch (error) {
      this.testLogger?.error('Notebook execution failed', { error: error.message });
      throw error;
    }
  }

  // CLI-based notebook execution using nbconvert
  async executeNotebookCLI(notebookPath: string, outputPath?: string): Promise<NotebookExecutionResult> {
    try {
      this.testLogger?.action(`Executing notebook via CLI: ${notebookPath}`);
      
      const outputFile = outputPath || notebookPath.replace('.ipynb', '_executed.ipynb');
      
      const result = await cliRunner.run(
        'jupyter',
        ['nbconvert', '--to', 'notebook', '--execute', '--output', outputFile, notebookPath],
        {
          timeout: 600000, // 10 minutes
          testLogger: this.testLogger,
        }
      );

      if (result.exitCode !== 0) {
        throw new Error(`Notebook execution failed: ${result.stderr}`);
      }

      // Parse the executed notebook to get results
      const executedNotebook = this.loadNotebook(outputFile);
      const cellResults = this.analyzeCellResults(executedNotebook);
      
      const executionResult: NotebookExecutionResult = {
        success: cellResults.every(cell => cell.success),
        totalCells: executedNotebook.cells.length,
        executedCells: cellResults.filter(cell => cell.executionCount > 0).length,
        failedCells: cellResults.filter(cell => !cell.success).length,
        totalExecutionTime: cellResults.reduce((sum, cell) => sum + cell.executionTime, 0),
        cellResults,
      };

      this.testLogger?.info('CLI notebook execution completed', {
        success: executionResult.success,
        totalCells: executionResult.totalCells,
        failedCells: executionResult.failedCells,
      });

      return executionResult;
    } catch (error) {
      this.testLogger?.error('CLI notebook execution failed', { error: error.message });
      throw error;
    }
  }

  private async openNotebook(notebookPath: string): Promise<void> {
    // This would depend on the specific UI (VS Code Web, JupyterLab, etc.)
    // Implementation would vary based on the platform
    this.testLogger?.step(`Opening notebook: ${notebookPath}`);
    
    // Example for JupyterLab
    await this.page.goto(`/lab/tree/${notebookPath}`);
    await this.page.waitForSelector('[data-jp-kernel-name]', { timeout: 30000 });
  }

  private async getCodeCells(): Promise<Locator[]> {
    // Get all code cells in the notebook
    const cells = await this.page.locator('.jp-Cell[data-cell-type="code"]').all();
    this.testLogger?.debug(`Found ${cells.length} code cells`);
    return cells;
  }

  private async executeCellUI(cell: Locator, cellIndex: number): Promise<CellExecutionResult> {
    const startTime = Date.now();
    
    try {
      // Click on the cell to select it
      await cell.click();
      
      // Execute the cell (Shift+Enter)
      await this.page.keyboard.press('Shift+Enter');
      
      // Wait for execution to complete
      await this.waitForCellExecution(cell);
      
      // Get execution results
      const executionCount = await this.getCellExecutionCount(cell);
      const outputs = await this.getCellOutputs(cell);
      const hasError = await this.cellHasError(cell);
      
      const executionTime = Date.now() - startTime;
      
      return {
        cellIndex,
        executionCount,
        outputs,
        executionTime,
        success: !hasError,
      };
    } catch (error) {
      return {
        cellIndex,
        executionCount: 0,
        outputs: [],
        executionTime: Date.now() - startTime,
        success: false,
        error: error.message,
      };
    }
  }

  private async waitForCellExecution(cell: Locator, timeout: number = 300000): Promise<void> {
    // Wait for the cell to finish executing
    // Look for the execution indicator to disappear
    try {
      await cell.locator('.jp-InputPrompt[data-execution-state="busy"]').waitFor({
        state: 'detached',
        timeout,
      });
    } catch (error) {
      // If the busy indicator doesn't appear, the cell might execute too quickly
      // Wait a short time and continue
      await this.page.waitForTimeout(1000);
    }
  }

  private async getCellExecutionCount(cell: Locator): Promise<number> {
    try {
      const promptText = await cell.locator('.jp-InputPrompt').textContent();
      const match = promptText?.match(/\[(\d+)\]/);
      return match ? parseInt(match[1]) : 0;
    } catch {
      return 0;
    }
  }

  private async getCellOutputs(cell: Locator): Promise<any[]> {
    try {
      const outputs = await cell.locator('.jp-OutputArea-output').all();
      const outputData = [];
      
      for (const output of outputs) {
        const text = await output.textContent();
        const html = await output.innerHTML();
        outputData.push({ text, html });
      }
      
      return outputData;
    } catch {
      return [];
    }
  }

  private async cellHasError(cell: Locator): Promise<boolean> {
    try {
      const errorOutput = cell.locator('.jp-OutputArea-output[data-mime-type="application/vnd.jupyter.stderr"]');
      return await errorOutput.count() > 0;
    } catch {
      return false;
    }
  }

  // Kernel management
  async selectKernel(kernelName: string): Promise<void> {
    try {
      this.testLogger?.action(`Selecting kernel: ${kernelName}`);
      
      // Click on kernel selector
      await this.page.click('[data-jp-kernel-name]');
      
      // Wait for kernel list
      await this.page.waitForSelector('.jp-Dialog-content');
      
      // Select the desired kernel
      await this.page.click(`text=${kernelName}`);
      
      // Confirm selection
      await this.page.click('button:has-text("Select")');
      
      this.testLogger?.info(`Kernel selected: ${kernelName}`);
    } catch (error) {
      this.testLogger?.error(`Failed to select kernel: ${kernelName}`, { error });
      throw error;
    }
  }

  async restartKernel(): Promise<void> {
    try {
      this.testLogger?.action('Restarting kernel');
      
      // Open kernel menu
      await this.page.click('text=Kernel');
      
      // Click restart
      await this.page.click('text=Restart Kernel');
      
      // Confirm restart
      await this.page.click('button:has-text("Restart")');
      
      // Wait for kernel to be ready
      await this.page.waitForSelector('[data-jp-kernel-name]', { timeout: 30000 });
      
      this.testLogger?.info('Kernel restarted successfully');
    } catch (error) {
      this.testLogger?.error('Failed to restart kernel', { error });
      throw error;
    }
  }

  // Utility methods
  private loadNotebook(filePath: string): NotebookContent {
    const content = fs.readFileSync(filePath, 'utf8');
    return JSON.parse(content);
  }

  private analyzeCellResults(notebook: NotebookContent): CellExecutionResult[] {
    const results: CellExecutionResult[] = [];
    
    notebook.cells.forEach((cell, index) => {
      if (cell.type === 'code') {
        const hasError = cell.outputs?.some(output => 
          output.output_type === 'error' || 
          (output.name === 'stderr' && output.text)
        ) || false;
        
        results.push({
          cellIndex: index,
          executionCount: cell.executionCount || 0,
          outputs: cell.outputs || [],
          executionTime: 0, // Not available from static analysis
          success: !hasError,
        });
      }
    });
    
    return results;
  }

  // Assertion helpers
  async assertCellOutput(cellIndex: number, expectedOutput: string | RegExp): Promise<void> {
    const cells = await this.getCodeCells();
    if (cellIndex >= cells.length) {
      throw new Error(`Cell index ${cellIndex} out of range. Total cells: ${cells.length}`);
    }
    
    const outputs = await this.getCellOutputs(cells[cellIndex]);
    const outputText = outputs.map(o => o.text).join('\n');
    
    if (typeof expectedOutput === 'string') {
      if (!outputText.includes(expectedOutput)) {
        throw new Error(`Expected output "${expectedOutput}" not found in cell ${cellIndex}. Actual: ${outputText}`);
      }
    } else {
      if (!expectedOutput.test(outputText)) {
        throw new Error(`Expected output pattern not found in cell ${cellIndex}. Actual: ${outputText}`);
      }
    }
    
    this.testLogger?.assertion(`Cell ${cellIndex} output matches expected`, true, {
      expectedOutput: expectedOutput.toString(),
      actualOutput: outputText,
    });
  }

  async assertNoCellErrors(): Promise<void> {
    const cells = await this.getCodeCells();
    const errorCells = [];
    
    for (let i = 0; i < cells.length; i++) {
      const hasError = await this.cellHasError(cells[i]);
      if (hasError) {
        errorCells.push(i);
      }
    }
    
    if (errorCells.length > 0) {
      throw new Error(`Found errors in cells: ${errorCells.join(', ')}`);
    }
    
    this.testLogger?.assertion('No cell errors found', true, {
      totalCells: cells.length,
    });
  }

  async assertKernelReady(): Promise<void> {
    try {
      await this.page.waitForSelector('[data-jp-kernel-name]:not([data-kernel-status="busy"])', {
        timeout: 30000,
      });
      
      this.testLogger?.assertion('Kernel is ready', true);
    } catch (error) {
      this.testLogger?.assertion('Kernel is ready', false, { error: error.message });
      throw new Error('Kernel is not ready');
    }
  }
}

// Factory function
export function createNotebookRunner(page: Page, testLogger?: TestLogger): NotebookRunner {
  return new NotebookRunner(page, testLogger);
}