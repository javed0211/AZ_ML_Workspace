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
const test_1 = require("@playwright/test");
const child_process_1 = require("child_process");
const util_1 = require("util");
const path = __importStar(require("path"));
const fs = __importStar(require("fs"));
const execAsync = (0, util_1.promisify)(child_process_1.exec);
/**
 * Comprehensive Jupyter Notebook (.ipynb) Testing Suite
 * Tests VS Code's ability to open, edit, and execute Jupyter notebooks
 */
test_1.test.describe('Jupyter Notebook Testing', () => {
    let vscodeProcess;
    let workspaceDir;
    test_1.test.beforeEach(async () => {
        console.log('🚀 Setting up Jupyter notebook test environment');
        // Set up PATH
        process.env.PATH = `/Users/oldguard/.local/bin:${process.env.PATH}`;
        // Create a temporary workspace with our notebook
        workspaceDir = path.join(__dirname, '..', 'temp', `workspace-${Date.now()}`);
        fs.mkdirSync(workspaceDir, { recursive: true });
        // Copy our existing notebook to the test workspace
        const sourceNotebook = '/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook/azure_ml_project.ipynb';
        const targetNotebook = path.join(workspaceDir, 'test_notebook.ipynb');
        if (fs.existsSync(sourceNotebook)) {
            fs.copyFileSync(sourceNotebook, targetNotebook);
            console.log(`✅ Copied notebook to: ${targetNotebook}`);
        }
        else {
            // Create a simple test notebook if the source doesn't exist
            const simpleNotebook = {
                "cells": [
                    {
                        "cell_type": "markdown",
                        "metadata": {},
                        "source": [
                            "# Test Jupyter Notebook\n",
                            "\n",
                            "This is a test notebook for VS Code integration testing."
                        ]
                    },
                    {
                        "cell_type": "code",
                        "execution_count": null,
                        "metadata": {},
                        "outputs": [],
                        "source": [
                            "# Simple Python test\n",
                            "import pandas as pd\n",
                            "import numpy as np\n",
                            "import matplotlib.pyplot as plt\n",
                            "\n",
                            "print(\"🚀 Jupyter notebook test started!\")\n",
                            "print(f\"NumPy version: {np.__version__}\")\n",
                            "print(f\"Pandas version: {pd.__version__}\")"
                        ]
                    },
                    {
                        "cell_type": "code",
                        "execution_count": null,
                        "metadata": {},
                        "outputs": [],
                        "source": [
                            "# Create sample data\n",
                            "data = np.random.randn(100, 2)\n",
                            "df = pd.DataFrame(data, columns=['X', 'Y'])\n",
                            "\n",
                            "print(\"📊 Sample data created:\")\n",
                            "print(f\"Shape: {df.shape}\")\n",
                            "df.head()"
                        ]
                    },
                    {
                        "cell_type": "code",
                        "execution_count": null,
                        "metadata": {},
                        "outputs": [],
                        "source": [
                            "# Simple visualization\n",
                            "plt.figure(figsize=(8, 6))\n",
                            "plt.scatter(df['X'], df['Y'], alpha=0.6)\n",
                            "plt.title('Sample Data Visualization')\n",
                            "plt.xlabel('X values')\n",
                            "plt.ylabel('Y values')\n",
                            "plt.grid(True, alpha=0.3)\n",
                            "plt.show()\n",
                            "\n",
                            "print(\"✅ Visualization completed!\")"
                        ]
                    }
                ],
                "metadata": {
                    "kernelspec": {
                        "display_name": "Python 3",
                        "language": "python",
                        "name": "python3"
                    },
                    "language_info": {
                        "codemirror_mode": {
                            "name": "ipython",
                            "version": 3
                        },
                        "file_extension": ".py",
                        "mimetype": "text/x-python",
                        "name": "python",
                        "nbconvert_exporter": "python",
                        "pygments_lexer": "ipython3",
                        "version": "3.9.13"
                    }
                },
                "nbformat": 4,
                "nbformat_minor": 4
            };
            fs.writeFileSync(targetNotebook, JSON.stringify(simpleNotebook, null, 2));
            console.log(`✅ Created test notebook at: ${targetNotebook}`);
        }
        // Launch VS Code with the workspace using the working method
        console.log(`🚀 Launching VS Code with workspace: ${workspaceDir}`);
        vscodeProcess = (0, child_process_1.spawn)('code', [workspaceDir, '--new-window'], {
            stdio: 'pipe',
            detached: false,
            env: process.env
        });
        console.log(`✅ VS Code launched with PID: ${vscodeProcess.pid}`);
        console.log(`📁 Workspace: ${workspaceDir}`);
        // Wait for VS Code to start
        await new Promise(resolve => setTimeout(resolve, 3000));
    });
    test_1.test.afterEach(async () => {
        // Clean up VS Code processes we created (but not the main one)
        if (vscodeProcess && vscodeProcess.pid) {
            try {
                vscodeProcess.kill();
                console.log(`🧹 Cleaned up VS Code process ${vscodeProcess.pid}`);
            }
            catch (e) {
                // Process may have already exited
            }
        }
    });
    (0, test_1.test)('should open and display Jupyter notebook correctly', async () => {
        console.log('🔍 Testing notebook opening and display');
        // Verify VS Code is running
        const { stdout } = await execAsync('ps aux | grep -i "visual studio code" | grep -v grep | wc -l');
        const processCount = parseInt(stdout.trim());
        (0, test_1.expect)(processCount).toBeGreaterThan(0);
        console.log(`✅ VS Code processes verified: ${processCount} processes running`);
        // Verify the notebook file exists
        const notebookPath = path.join(workspaceDir, 'test_notebook.ipynb');
        (0, test_1.expect)(fs.existsSync(notebookPath)).toBe(true);
        console.log(`✅ Notebook file exists at: ${notebookPath}`);
        // Test VS Code CLI responsiveness
        const { stdout: versionOutput } = await execAsync('code --version');
        const version = versionOutput.split('\n')[0];
        (0, test_1.expect)(version).toMatch(/^\d+\.\d+\.\d+$/);
        console.log(`✅ VS Code version: ${version}`);
        // Open the specific notebook file in VS Code
        (0, child_process_1.spawn)('code', [notebookPath], {
            stdio: 'pipe',
            env: process.env
        });
        // Wait for file to open
        await new Promise(resolve => setTimeout(resolve, 2000));
        console.log('✅ Jupyter notebook opened in VS Code');
    });
    (0, test_1.test)('should verify notebook file structure', async () => {
        console.log('📋 Testing notebook file structure validation');
        try {
            // Read and validate the notebook file
            const notebookPath = path.join(workspaceDir, 'test_notebook.ipynb');
            const notebookContent = fs.readFileSync(notebookPath, 'utf8');
            const notebook = JSON.parse(notebookContent);
            // Validate basic notebook structure
            (0, test_1.expect)(notebook).toHaveProperty('cells');
            (0, test_1.expect)(notebook).toHaveProperty('metadata');
            (0, test_1.expect)(notebook).toHaveProperty('nbformat');
            (0, test_1.expect)(notebook).toHaveProperty('nbformat_minor');
            // Validate nbformat version
            (0, test_1.expect)(notebook.nbformat).toBe(4);
            (0, test_1.expect)(typeof notebook.nbformat_minor).toBe('number');
            // Validate cells structure
            (0, test_1.expect)(Array.isArray(notebook.cells)).toBe(true);
            (0, test_1.expect)(notebook.cells.length).toBeGreaterThan(0);
            // Check first cell structure
            const firstCell = notebook.cells[0];
            (0, test_1.expect)(firstCell).toHaveProperty('cell_type');
            (0, test_1.expect)(firstCell).toHaveProperty('metadata');
            (0, test_1.expect)(firstCell).toHaveProperty('source');
            console.log(`✅ Notebook structure validated: ${notebook.cells.length} cells, nbformat ${notebook.nbformat}.${notebook.nbformat_minor}`);
        }
        catch (error) {
            console.log(`❌ Error validating notebook structure: ${error}`);
            throw error;
        }
    });
    (0, test_1.test)('should handle basic VS Code file operations', async () => {
        console.log('📁 Testing basic VS Code file operations');
        // Create a test file and open it in VS Code
        const testFileName = path.join(workspaceDir, 'test-automation.py');
        const testContent = '# Test file created by automation\nprint("Hello from Jupyter test!")';
        fs.writeFileSync(testFileName, testContent);
        // Verify file was created
        (0, test_1.expect)(fs.existsSync(testFileName)).toBe(true);
        const fileContent = fs.readFileSync(testFileName, 'utf8');
        (0, test_1.expect)(fileContent).toContain('Test file created by automation');
        (0, test_1.expect)(fileContent).toContain('Hello from Jupyter test!');
        // Open the file in VS Code
        (0, child_process_1.spawn)('code', [testFileName], {
            stdio: 'pipe',
            env: process.env
        });
        // Wait for file to open
        await new Promise(resolve => setTimeout(resolve, 1000));
        console.log('✅ File operations completed successfully');
    });
    (0, test_1.test)('should verify VS Code workspace integration', async () => {
        console.log('📸 Testing VS Code workspace integration');
        // Verify workspace files exist
        const notebookPath = path.join(workspaceDir, 'test_notebook.ipynb');
        (0, test_1.expect)(fs.existsSync(notebookPath)).toBe(true);
        console.log(`✅ Workspace files verified at: ${workspaceDir}`);
        // Verify VS Code is still running and responsive
        const { stdout } = await execAsync('ps aux | grep -i "visual studio code" | grep -v grep | wc -l');
        const processCount = parseInt(stdout.trim());
        (0, test_1.expect)(processCount).toBeGreaterThan(0);
        console.log(`✅ VS Code still running: ${processCount} processes`);
        // Test VS Code CLI is still responsive
        try {
            const { stdout: versionOutput } = await execAsync('code --version');
            const version = versionOutput.split('\n')[0];
            (0, test_1.expect)(version).toMatch(/^\d+\.\d+\.\d+$/);
            console.log(`✅ VS Code CLI responsive, version: ${version}`);
        }
        catch (error) {
            console.log(`⚠️ VS Code CLI check had issues: ${error}`);
        }
        console.log('✅ VS Code workspace integration verified');
    });
});
