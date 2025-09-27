import { VSCodeElectron } from './vscode-electron';
/**
 * Test helper utilities for VS Code Electron testing
 */
export declare class TestHelpers {
    /**
     * Get the workspace root directory
     */
    static getWorkspaceRoot(): string;
    /**
     * Get test data directory
     */
    static getTestDataDir(): string;
    /**
     * Get a test file path
     */
    static getTestFilePath(filename: string): string;
    /**
     * Create a temporary test file
     */
    static createTempFile(content: string, extension?: string): Promise<string>;
    /**
     * Clean up temporary files
     */
    static cleanupTempFiles(): void;
    /**
     * Wait for a specified amount of time
     */
    static wait(ms: number): Promise<void>;
    /**
     * Retry an operation with exponential backoff
     */
    static retry<T>(operation: () => Promise<T>, maxAttempts?: number, baseDelay?: number): Promise<T>;
    /**
     * Setup VS Code for testing
     */
    static setupVSCodeForTesting(vscode: VSCodeElectron): Promise<void>;
    /**
     * Verify VS Code is in expected state
     */
    static verifyVSCodeState(vscode: VSCodeElectron): Promise<boolean>;
    /**
     * Get current timestamp for logging
     */
    static getTimestamp(): string;
    /**
     * Ensure directory exists, create if it doesn't
     */
    static ensureDirectoryExists(dirPath: string): Promise<void>;
    /**
     * Create test extension files for VS Code official framework
     */
    static createTestExtensionFiles(extensionPath: string): Promise<void>;
    /**
     * Log test step
     */
    static logStep(step: string): void;
    /**
     * Create a test workspace with sample files
     */
    static createTestWorkspace(): Promise<string>;
    /**
     * Clean up test workspace
     */
    static cleanupTestWorkspace(workspacePath: string): void;
}
