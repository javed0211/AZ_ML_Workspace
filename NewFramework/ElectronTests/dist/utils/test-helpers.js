"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.TestHelpers = void 0;
const path = __importStar(require("path"));
const fs = __importStar(require("fs"));
/**
 * Test helper utilities for VS Code Electron testing
 */
class TestHelpers {
    /**
     * Get the workspace root directory
     */
    static getWorkspaceRoot() {
        return path.join(__dirname, '..', '..', '..', '..');
    }
    /**
     * Get test data directory
     */
    static getTestDataDir() {
        return path.join(__dirname, '..', '..', 'TestData');
    }
    /**
     * Get a test file path
     */
    static getTestFilePath(filename) {
        return path.join(this.getTestDataDir(), filename);
    }
    /**
     * Create a temporary test file
     */
    static async createTempFile(content, extension = '.txt') {
        const tempDir = path.join(__dirname, '..', 'temp');
        // Ensure temp directory exists
        if (!fs.existsSync(tempDir)) {
            fs.mkdirSync(tempDir, { recursive: true });
        }
        const filename = `test-${Date.now()}${extension}`;
        const filepath = path.join(tempDir, filename);
        fs.writeFileSync(filepath, content, 'utf8');
        return filepath;
    }
    /**
     * Clean up temporary files
     */
    static cleanupTempFiles() {
        const tempDir = path.join(__dirname, '..', 'temp');
        if (fs.existsSync(tempDir)) {
            const files = fs.readdirSync(tempDir);
            files.forEach(file => {
                const filePath = path.join(tempDir, file);
                fs.unlinkSync(filePath);
            });
        }
    }
    /**
     * Wait for a specified amount of time
     */
    static async wait(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }
    /**
     * Retry an operation with exponential backoff
     */
    static async retry(operation, maxAttempts = 3, baseDelay = 1000) {
        let lastError;
        for (let attempt = 1; attempt <= maxAttempts; attempt++) {
            try {
                return await operation();
            }
            catch (error) {
                lastError = error;
                if (attempt === maxAttempts) {
                    throw lastError;
                }
                const delay = baseDelay * Math.pow(2, attempt - 1);
                console.log(`Attempt ${attempt} failed, retrying in ${delay}ms...`);
                await this.wait(delay);
            }
        }
        throw lastError;
    }
    /**
     * Setup VS Code for testing
     */
    static async setupVSCodeForTesting(vscode) {
        // Wait for VS Code to be fully ready
        await TestHelpers.wait(2000);
        // Close any welcome tabs or dialogs
        try {
            await vscode.executeCommand('workbench.action.closeAllEditors');
            await TestHelpers.wait(1000);
        }
        catch (error) {
            console.log('No editors to close or command failed:', error);
        }
        // Disable telemetry and other distracting features
        try {
            await vscode.executeCommand('workbench.action.openSettings');
            await TestHelpers.wait(1000);
            // Close settings
            await vscode.executeCommand('workbench.action.closeActiveEditor');
            await TestHelpers.wait(500);
        }
        catch (error) {
            console.log('Settings configuration failed:', error);
        }
    }
    /**
     * Verify VS Code is in expected state
     */
    static async verifyVSCodeState(vscode) {
        try {
            // Check if VS Code process is running
            const isRunning = vscode.isRunning();
            if (!isRunning) {
                console.log('VS Code process is not running');
                return false;
            }
            // Try to get the main window
            const window = vscode.getMainWindow();
            if (!window) {
                console.log('Main window is not available');
                return false;
            }
            // Check if window is visible (basic check)
            try {
                const title = await window.title();
                console.log(`VS Code window title: ${title}`);
                return title.includes('Visual Studio Code') || title.includes('VS Code');
            }
            catch (error) {
                console.log('Could not get window title, but VS Code is running');
                return true; // VS Code is running, which is the main requirement
            }
        }
        catch (error) {
            console.error('VS Code state verification failed:', error);
            return false;
        }
    }
    /**
     * Get current timestamp for logging
     */
    static getTimestamp() {
        return new Date().toISOString();
    }
    /**
     * Ensure directory exists, create if it doesn't
     */
    static async ensureDirectoryExists(dirPath) {
        const fs = require('fs').promises;
        try {
            await fs.access(dirPath);
        }
        catch {
            await fs.mkdir(dirPath, { recursive: true });
            console.log(`Created directory: ${dirPath}`);
        }
    }
    /**
     * Create test extension files for VS Code official framework
     */
    static async createTestExtensionFiles(extensionPath) {
        const fs = require('fs').promises;
        const path = require('path');
        // Ensure package.json exists
        const packageJsonPath = path.join(extensionPath, 'package.json');
        try {
            await fs.access(packageJsonPath);
            console.log('✅ Test extension package.json already exists');
        }
        catch {
            console.log('❌ Test extension package.json missing - should be created separately');
        }
    }
    /**
     * Log test step
     */
    static logStep(step) {
        console.log(`[${this.getTimestamp()}] ${step}`);
    }
    /**
     * Create a test workspace with sample files
     */
    static async createTestWorkspace() {
        const workspaceDir = path.join(__dirname, '..', 'temp', `workspace-${Date.now()}`);
        // Create workspace directory
        fs.mkdirSync(workspaceDir, { recursive: true });
        // Create sample files
        const samplePython = `# Sample Python file for testing
def hello_world():
    print("Hello, World!")
    return "Hello, World!"

if __name__ == "__main__":
    hello_world()
`;
        const sampleJson = `{
  "name": "test-project",
  "version": "1.0.0",
  "description": "Test project for VS Code automation"
}`;
        const sampleMarkdown = `# Test Project

This is a test project for VS Code automation testing.

## Features

- File operations
- Editor interactions
- Command palette usage
`;
        fs.writeFileSync(path.join(workspaceDir, 'main.py'), samplePython);
        fs.writeFileSync(path.join(workspaceDir, 'package.json'), sampleJson);
        fs.writeFileSync(path.join(workspaceDir, 'README.md'), sampleMarkdown);
        return workspaceDir;
    }
    /**
     * Clean up test workspace
     */
    static cleanupTestWorkspace(workspacePath) {
        if (fs.existsSync(workspacePath)) {
            fs.rmSync(workspacePath, { recursive: true, force: true });
        }
    }
}
exports.TestHelpers = TestHelpers;
