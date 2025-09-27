import { ElectronApplication, Page } from 'playwright';
import { VSCodeConfig } from '../config/vscode-config';
/**
 * VS Code Electron Application wrapper for testing
 */
export declare class VSCodeElectron {
    private electronApp;
    private mainWindow;
    private config;
    private vsCodeProcess;
    constructor(config?: Partial<VSCodeConfig>);
    /**
     * Launch VS Code Electron application
     */
    launch(workspaceOrFile?: string, additionalArgs?: string[]): Promise<void>;
    /**
     * Launch VS Code manually using child_process and create a mock Playwright interface
     */
    private launchVSCodeManually;
    /**
     * Create a mock window interface for basic testing compatibility
     */
    private createMockWindow;
    /**
     * Wait for VS Code to be fully loaded and ready
     */
    private waitForVSCodeReady;
    /**
     * Get the main VS Code window
     */
    getMainWindow(): Page;
    /**
     * Get the Electron application instance
     */
    getElectronApp(): ElectronApplication;
    /**
     * Open a file in VS Code
     */
    openFile(filePath: string): Promise<void>;
    /**
     * Open a folder/workspace in VS Code
     */
    openFolder(folderPath: string): Promise<void>;
    /**
     * Create a new file
     */
    createNewFile(): Promise<void>;
    /**
     * Save current file
     */
    saveFile(): Promise<void>;
    /**
     * Type text in the editor
     */
    typeInEditor(text: string): Promise<void>;
    /**
     * Get text content from the active editor
     */
    getEditorContent(): Promise<string>;
    /**
     * Open command palette
     */
    openCommandPalette(): Promise<void>;
    /**
     * Execute a command via command palette
     */
    executeCommand(command: string): Promise<void>;
    /**
     * Take a screenshot
     */
    takeScreenshot(path?: string): Promise<Buffer>;
    /**
     * Close VS Code
     */
    close(): Promise<void>;
    /**
     * Wait for a specific element to be visible
     */
    waitForElement(selector: string, timeout?: number): Promise<void>;
    /**
     * Click on an element
     */
    clickElement(selector: string): Promise<void>;
    /**
     * Check if VS Code is running
     */
    isRunning(): boolean;
}
