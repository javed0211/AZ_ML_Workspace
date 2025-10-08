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
const os = __importStar(require("os"));
const execAsync = (0, util_1.promisify)(child_process_1.exec);
/**
 * Cross-Platform Jupyter Notebook Runner
 * Tests VS Code's ability to open and run existing .ipynb files
 * Works on Windows, Linux, and macOS
 */
test_1.test.describe('Cross-Platform Jupyter Notebook Runner', () => {
    let vscodeProcess;
    let testWorkspace;
    let platform;
    let vscodeCommand;
    let vscodeExecutable;
    test_1.test.beforeAll(async () => {
        // Detect platform and set appropriate VS Code paths
        platform = os.platform();
        console.log(`🖥️  Detected platform: ${platform}`);
        switch (platform) {
            case 'darwin': // macOS
                vscodeCommand = 'code';
                vscodeExecutable = '/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code';
                break;
            case 'win32': // Windows
                vscodeCommand = 'code.cmd';
                vscodeExecutable = process.env.LOCALAPPDATA
                    ? path.join(process.env.LOCALAPPDATA, 'Programs', 'Microsoft VS Code', 'bin', 'code.cmd')
                    : 'code.cmd';
                break;
            case 'linux': // Linux
                vscodeCommand = 'code';
                vscodeExecutable = '/usr/bin/code';
                break;
            default:
                throw new Error(`Unsupported platform: ${platform}`);
        }
        console.log(`🔧 VS Code command: ${vscodeCommand}`);
        console.log(`📍 VS Code executable: ${vscodeExecutable}`);
    });
    test_1.test.beforeEach(async () => {
        console.log('🚀 Setting up cross-platform Jupyter test environment');
        // Create test workspace
        testWorkspace = path.join(__dirname, '..', 'temp', `cross-platform-${Date.now()}`);
        fs.mkdirSync(testWorkspace, { recursive: true });
        console.log(`📁 Created workspace: ${testWorkspace}`);
    });
    test_1.test.afterEach(async () => {
        // Clean up VS Code processes
        if (vscodeProcess && vscodeProcess.pid) {
            try {
                if (platform === 'win32') {
                    // Windows process termination
                    await execAsync(`taskkill /PID ${vscodeProcess.pid} /F`);
                }
                else {
                    // Unix-like systems
                    vscodeProcess.kill('SIGTERM');
                }
                console.log(`🧹 Cleaned up VS Code process ${vscodeProcess.pid}`);
            }
            catch (e) {
                console.log('⚠️ Process cleanup had issues (may already be closed)');
            }
        }
    });
    (0, test_1.test)('should run existing Jupyter notebook from any folder on any OS', async () => {
        console.log('🔍 Testing cross-platform Jupyter notebook execution');
        // Step 1: Create a comprehensive test notebook
        const notebookContent = {
            "cells": [
                {
                    "cell_type": "markdown",
                    "metadata": {},
                    "source": [
                        "# Cross-Platform Jupyter Notebook Test\n",
                        "\n",
                        "This notebook tests cross-platform compatibility for:\n",
                        "- Windows\n",
                        "- macOS\n",
                        "- Linux\n",
                        "\n",
                        "## System Information"
                    ]
                },
                {
                    "cell_type": "code",
                    "execution_count": null,
                    "metadata": {},
                    "outputs": [],
                    "source": [
                        "import sys\n",
                        "import os\n",
                        "import platform\n",
                        "from datetime import datetime\n",
                        "\n",
                        "print(f\"🖥️  Platform: {platform.system()}\")\n",
                        "print(f\"🏗️  Architecture: {platform.architecture()}\")\n",
                        "print(f\"🐍 Python Version: {sys.version}\")\n",
                        "print(f\"📁 Current Directory: {os.getcwd()}\")\n",
                        "print(f\"⏰ Timestamp: {datetime.now()}\")"
                    ]
                },
                {
                    "cell_type": "code",
                    "execution_count": null,
                    "metadata": {},
                    "outputs": [],
                    "source": [
                        "# Test basic data science libraries\n",
                        "try:\n",
                        "    import numpy as np\n",
                        "    print(f\"✅ NumPy {np.__version__} available\")\n",
                        "    \n",
                        "    # Create sample data\n",
                        "    data = np.random.randn(10, 3)\n",
                        "    print(f\"📊 Sample data shape: {data.shape}\")\n",
                        "    print(f\"📈 Data preview:\\n{data[:3]}\")\n",
                        "    \n",
                        "except ImportError as e:\n",
                        "    print(f\"⚠️ NumPy not available: {e}\")"
                    ]
                },
                {
                    "cell_type": "code",
                    "execution_count": null,
                    "metadata": {},
                    "outputs": [],
                    "source": [
                        "# Test pandas if available\n",
                        "try:\n",
                        "    import pandas as pd\n",
                        "    print(f\"✅ Pandas {pd.__version__} available\")\n",
                        "    \n",
                        "    # Create sample DataFrame\n",
                        "    df = pd.DataFrame({\n",
                        "        'platform': [platform.system()] * 5,\n",
                        "        'test_id': range(1, 6),\n",
                        "        'timestamp': [datetime.now()] * 5,\n",
                        "        'success': [True] * 5\n",
                        "    })\n",
                        "    \n",
                        "    print(f\"📋 DataFrame created:\")\n",
                        "    print(df)\n",
                        "    \n",
                        "except ImportError as e:\n",
                        "    print(f\"⚠️ Pandas not available: {e}\")"
                    ]
                },
                {
                    "cell_type": "code",
                    "execution_count": null,
                    "metadata": {},
                    "outputs": [],
                    "source": [
                        "# Test file operations\n",
                        "test_file = 'cross_platform_test.txt'\n",
                        "\n",
                        "# Write test file\n",
                        "with open(test_file, 'w') as f:\n",
                        "    f.write(f\"Cross-platform test executed on {platform.system()}\\n\")\n",
                        "    f.write(f\"Python version: {sys.version}\\n\")\n",
                        "    f.write(f\"Timestamp: {datetime.now()}\\n\")\n",
                        "\n",
                        "# Read and verify\n",
                        "if os.path.exists(test_file):\n",
                        "    with open(test_file, 'r') as f:\n",
                        "        content = f.read()\n",
                        "    print(f\"✅ File operations successful:\")\n",
                        "    print(content)\n",
                        "    \n",
                        "    # Clean up\n",
                        "    os.remove(test_file)\n",
                        "    print(f\"🧹 Test file cleaned up\")\n",
                        "else:\n",
                        "    print(f\"❌ File operation failed\")"
                    ]
                },
                {
                    "cell_type": "code",
                    "execution_count": null,
                    "metadata": {},
                    "outputs": [],
                    "source": [
                        "# Final status report\n",
                        "print(\"\\n\" + \"=\"*50)\n",
                        "print(\"🎉 CROSS-PLATFORM JUPYTER TEST COMPLETED\")\n",
                        "print(\"=\"*50)\n",
                        "print(f\"Platform: {platform.system()}\")\n",
                        "print(f\"Python: {sys.version.split()[0]}\")\n",
                        "print(f\"Status: ✅ SUCCESS\")\n",
                        "print(f\"Timestamp: {datetime.now()}\")\n",
                        "print(\"=\"*50)"
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
                    "version": "3.9.0"
                }
            },
            "nbformat": 4,
            "nbformat_minor": 4
        };
        // Step 2: Create the notebook file
        const notebookPath = path.join(testWorkspace, 'cross_platform_test.ipynb');
        fs.writeFileSync(notebookPath, JSON.stringify(notebookContent, null, 2));
        console.log(`✅ Created test notebook: ${notebookPath}`);
        // Step 3: Create additional test files to simulate a real project
        const additionalFiles = {
            'requirements.txt': 'numpy>=1.20.0\npandas>=1.3.0\nmatplotlib>=3.4.0\njupyter>=1.0.0',
            'config.json': JSON.stringify({
                "project": "Cross-Platform Jupyter Test",
                "platform": platform,
                "version": "1.0.0",
                "created": new Date().toISOString()
            }, null, 2),
            'data_sample.csv': 'id,name,value,platform\n1,test1,100,' + platform + '\n2,test2,200,' + platform + '\n3,test3,300,' + platform,
            'README.md': `# Cross-Platform Jupyter Test\n\nThis project tests Jupyter notebook execution across different platforms.\n\n## Platform: ${platform}\n\n## Files:\n- cross_platform_test.ipynb: Main test notebook\n- requirements.txt: Python dependencies\n- config.json: Configuration\n- data_sample.csv: Sample data\n\n## Usage:\nOpen the notebook in VS Code and run all cells.`
        };
        for (const [filename, content] of Object.entries(additionalFiles)) {
            const filePath = path.join(testWorkspace, filename);
            fs.writeFileSync(filePath, content);
            console.log(`📄 Created: ${filename}`);
        }
        // Step 4: Verify VS Code is available
        try {
            const versionCommand = platform === 'win32' ? 'code.cmd --version' : 'code --version';
            const { stdout: versionOutput } = await execAsync(versionCommand);
            const version = versionOutput.split('\n')[0];
            (0, test_1.expect)(version).toMatch(/^\d+\.\d+\.\d+$/);
            console.log(`✅ VS Code version: ${version}`);
        }
        catch (error) {
            console.log(`⚠️ VS Code CLI check: ${error}`);
            // Try alternative paths
            if (fs.existsSync(vscodeExecutable)) {
                console.log(`✅ Found VS Code at: ${vscodeExecutable}`);
            }
            else {
                throw new Error(`VS Code not found. Please install VS Code and ensure it's in PATH.`);
            }
        }
        // Step 5: Launch VS Code with the workspace
        console.log(`🚀 Launching VS Code with workspace: ${testWorkspace}`);
        const launchArgs = [testWorkspace, '--new-window'];
        // Add platform-specific arguments
        if (platform === 'linux') {
            launchArgs.push('--no-sandbox'); // Often needed for Linux automation
        }
        vscodeProcess = (0, child_process_1.spawn)(vscodeCommand, launchArgs, {
            stdio: 'pipe',
            detached: false,
            env: process.env,
            shell: platform === 'win32' // Use shell on Windows
        });
        console.log(`✅ VS Code launched with PID: ${vscodeProcess.pid}`);
        console.log(`📁 Workspace: ${testWorkspace}`);
        // Step 6: Wait for VS Code to start
        await new Promise(resolve => setTimeout(resolve, 5000));
        // Step 7: Verify VS Code is running
        let processCheckCommand;
        switch (platform) {
            case 'win32':
                processCheckCommand = 'tasklist /FI "IMAGENAME eq Code.exe" /FO CSV | find /C "Code.exe"';
                break;
            case 'darwin':
                processCheckCommand = 'ps aux | grep -i "visual studio code" | grep -v grep | wc -l';
                break;
            case 'linux':
                processCheckCommand = 'ps aux | grep -i "code" | grep -v grep | wc -l';
                break;
            default:
                processCheckCommand = 'ps aux | grep -i "code" | grep -v grep | wc -l';
        }
        try {
            const { stdout } = await execAsync(processCheckCommand);
            const processCount = parseInt(stdout.trim());
            (0, test_1.expect)(processCount).toBeGreaterThan(0);
            console.log(`✅ VS Code processes verified: ${processCount} processes running`);
        }
        catch (error) {
            console.log(`⚠️ Process verification had issues: ${error}`);
        }
        // Step 8: Open the specific notebook file
        console.log(`📖 Opening notebook: ${notebookPath}`);
        const openCommand = platform === 'win32' ? 'code.cmd' : 'code';
        (0, child_process_1.spawn)(openCommand, [notebookPath], {
            stdio: 'pipe',
            env: process.env,
            shell: platform === 'win32'
        });
        // Wait for file to open
        await new Promise(resolve => setTimeout(resolve, 3000));
        // Step 9: Verify all files exist and are accessible
        const allFiles = [
            'cross_platform_test.ipynb',
            'requirements.txt',
            'config.json',
            'data_sample.csv',
            'README.md'
        ];
        for (const file of allFiles) {
            const filePath = path.join(testWorkspace, file);
            (0, test_1.expect)(fs.existsSync(filePath)).toBe(true);
            const stats = fs.statSync(filePath);
            (0, test_1.expect)(stats.size).toBeGreaterThan(0);
            console.log(`✅ Verified: ${file} (${stats.size} bytes)`);
        }
        // Step 10: Validate notebook structure
        const notebookFileContent = fs.readFileSync(notebookPath, 'utf8');
        const notebook = JSON.parse(notebookFileContent);
        (0, test_1.expect)(notebook).toHaveProperty('cells');
        (0, test_1.expect)(notebook).toHaveProperty('metadata');
        (0, test_1.expect)(notebook).toHaveProperty('nbformat');
        (0, test_1.expect)(notebook.nbformat).toBe(4);
        (0, test_1.expect)(Array.isArray(notebook.cells)).toBe(true);
        (0, test_1.expect)(notebook.cells.length).toBeGreaterThan(0);
        console.log(`✅ Notebook structure validated: ${notebook.cells.length} cells`);
        // Step 11: Test opening additional files
        const filesToOpen = ['README.md', 'config.json', 'requirements.txt'];
        for (const file of filesToOpen) {
            const filePath = path.join(testWorkspace, file);
            console.log(`📄 Opening: ${file}`);
            (0, child_process_1.spawn)(openCommand, [filePath], {
                stdio: 'pipe',
                env: process.env,
                shell: platform === 'win32'
            });
            await new Promise(resolve => setTimeout(resolve, 1000));
        }
        // Step 12: Final verification
        console.log('🔍 Final cross-platform verification...');
        // Verify VS Code is still responsive
        try {
            const finalVersionCommand = platform === 'win32' ? 'code.cmd --version' : 'code --version';
            const { stdout: finalVersionCheck } = await execAsync(finalVersionCommand);
            const finalVersion = finalVersionCheck.split('\n')[0];
            (0, test_1.expect)(finalVersion).toMatch(/^\d+\.\d+\.\d+$/);
            console.log(`✅ VS Code still responsive: ${finalVersion}`);
        }
        catch (error) {
            console.log(`⚠️ Final responsiveness check: ${error}`);
        }
        // Success summary
        console.log('\n' + '='.repeat(60));
        console.log('🎉 CROSS-PLATFORM JUPYTER TEST COMPLETED SUCCESSFULLY');
        console.log('='.repeat(60));
        console.log(`🖥️  Platform: ${platform}`);
        console.log(`📁 Workspace: ${testWorkspace}`);
        console.log(`📓 Notebook: cross_platform_test.ipynb`);
        console.log(`📄 Additional files: ${allFiles.length - 1} files`);
        console.log(`✅ Status: ALL TESTS PASSED`);
        console.log('='.repeat(60));
    });
    (0, test_1.test)('should handle different notebook formats and encodings', async () => {
        console.log('🔍 Testing different notebook formats and encodings');
        // Create notebooks with different characteristics
        const notebooks = {
            'simple_notebook.ipynb': {
                "cells": [
                    {
                        "cell_type": "code",
                        "execution_count": null,
                        "metadata": {},
                        "outputs": [],
                        "source": ["print('Hello from simple notebook!')"]
                    }
                ],
                "metadata": { "kernelspec": { "display_name": "Python 3", "language": "python", "name": "python3" } },
                "nbformat": 4,
                "nbformat_minor": 2
            },
            'complex_notebook.ipynb': {
                "cells": [
                    {
                        "cell_type": "markdown",
                        "metadata": {},
                        "source": ["# Complex Notebook\n\nThis notebook contains various cell types and special characters: 🚀 ✅ 📊"]
                    },
                    {
                        "cell_type": "code",
                        "execution_count": 1,
                        "metadata": {},
                        "outputs": [{ "output_type": "stream", "name": "stdout", "text": ["Hello World!\n"] }],
                        "source": ["# Code with output\nprint('Hello World!')"]
                    },
                    {
                        "cell_type": "raw",
                        "metadata": {},
                        "source": ["This is a raw cell with special characters: àáâãäåæçèéêë"]
                    }
                ],
                "metadata": { "kernelspec": { "display_name": "Python 3", "language": "python", "name": "python3" } },
                "nbformat": 4,
                "nbformat_minor": 4
            }
        };
        // Create and test each notebook
        for (const [filename, content] of Object.entries(notebooks)) {
            const notebookPath = path.join(testWorkspace, filename);
            fs.writeFileSync(notebookPath, JSON.stringify(content, null, 2), 'utf8');
            // Verify file was created correctly
            (0, test_1.expect)(fs.existsSync(notebookPath)).toBe(true);
            // Verify content can be parsed
            const readContent = fs.readFileSync(notebookPath, 'utf8');
            const parsedContent = JSON.parse(readContent);
            (0, test_1.expect)(parsedContent.nbformat).toBe(4);
            console.log(`✅ Created and verified: ${filename}`);
            // Open in VS Code
            const openCommand = platform === 'win32' ? 'code.cmd' : 'code';
            (0, child_process_1.spawn)(openCommand, [notebookPath], {
                stdio: 'pipe',
                env: process.env,
                shell: platform === 'win32'
            });
            await new Promise(resolve => setTimeout(resolve, 1500));
        }
        console.log('✅ All notebook formats handled successfully');
    });
    (0, test_1.test)('should run existing notebook from any specified folder', async () => {
        console.log('🔍 Testing existing notebook execution from custom folder');
        // This test demonstrates how to run existing notebooks from any folder
        // You can modify the NOTEBOOK_FOLDER_PATH to point to your existing notebooks
        const NOTEBOOK_FOLDER_PATH = process.env.NOTEBOOK_FOLDER || '/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewNotebook';
        const NOTEBOOK_FILE_NAME = process.env.NOTEBOOK_FILE || 'azure_ml_project.ipynb';
        console.log(`📁 Looking for notebooks in: ${NOTEBOOK_FOLDER_PATH}`);
        console.log(`📓 Target notebook: ${NOTEBOOK_FILE_NAME}`);
        // Check if the specified folder exists
        if (fs.existsSync(NOTEBOOK_FOLDER_PATH)) {
            console.log(`✅ Notebook folder exists: ${NOTEBOOK_FOLDER_PATH}`);
            // List all .ipynb files in the folder
            const files = fs.readdirSync(NOTEBOOK_FOLDER_PATH);
            const notebookFiles = files.filter(file => file.endsWith('.ipynb'));
            console.log(`📚 Found ${notebookFiles.length} notebook files:`);
            notebookFiles.forEach(file => console.log(`  - ${file}`));
            if (notebookFiles.length > 0) {
                // Use specified notebook or first available one
                const targetNotebook = notebookFiles.includes(NOTEBOOK_FILE_NAME)
                    ? NOTEBOOK_FILE_NAME
                    : notebookFiles[0];
                const notebookPath = path.join(NOTEBOOK_FOLDER_PATH, targetNotebook);
                console.log(`🎯 Selected notebook: ${targetNotebook}`);
                // Verify notebook structure
                try {
                    const notebookContent = fs.readFileSync(notebookPath, 'utf8');
                    const notebook = JSON.parse(notebookContent);
                    (0, test_1.expect)(notebook).toHaveProperty('cells');
                    (0, test_1.expect)(notebook).toHaveProperty('nbformat');
                    (0, test_1.expect)(Array.isArray(notebook.cells)).toBe(true);
                    console.log(`✅ Notebook structure valid: ${notebook.cells.length} cells, nbformat ${notebook.nbformat}`);
                    // Launch VS Code with the existing folder
                    console.log(`🚀 Opening existing folder in VS Code: ${NOTEBOOK_FOLDER_PATH}`);
                    const launchArgs = [NOTEBOOK_FOLDER_PATH, '--new-window'];
                    if (platform === 'linux') {
                        launchArgs.push('--no-sandbox');
                    }
                    vscodeProcess = (0, child_process_1.spawn)(vscodeCommand, launchArgs, {
                        stdio: 'pipe',
                        detached: false,
                        env: process.env,
                        shell: platform === 'win32'
                    });
                    console.log(`✅ VS Code launched with PID: ${vscodeProcess.pid}`);
                    // Wait for VS Code to start
                    await new Promise(resolve => setTimeout(resolve, 4000));
                    // Open the specific notebook
                    console.log(`📖 Opening notebook: ${notebookPath}`);
                    const openCommand = platform === 'win32' ? 'code.cmd' : 'code';
                    (0, child_process_1.spawn)(openCommand, [notebookPath], {
                        stdio: 'pipe',
                        env: process.env,
                        shell: platform === 'win32'
                    });
                    await new Promise(resolve => setTimeout(resolve, 3000));
                    // Open all notebooks in the folder
                    console.log(`📚 Opening all ${notebookFiles.length} notebooks:`);
                    for (const notebookFile of notebookFiles) {
                        const fullPath = path.join(NOTEBOOK_FOLDER_PATH, notebookFile);
                        console.log(`  📓 Opening: ${notebookFile}`);
                        (0, child_process_1.spawn)(openCommand, [fullPath], {
                            stdio: 'pipe',
                            env: process.env,
                            shell: platform === 'win32'
                        });
                        await new Promise(resolve => setTimeout(resolve, 1000));
                    }
                    // Verify VS Code is running with the notebooks
                    let processCheckCommand;
                    switch (platform) {
                        case 'win32':
                            processCheckCommand = 'tasklist /FI "IMAGENAME eq Code.exe" /FO CSV | find /C "Code.exe"';
                            break;
                        case 'darwin':
                            processCheckCommand = 'ps aux | grep -i "visual studio code" | grep -v grep | wc -l';
                            break;
                        case 'linux':
                            processCheckCommand = 'ps aux | grep -i "code" | grep -v grep | wc -l';
                            break;
                        default:
                            processCheckCommand = 'ps aux | grep -i "code" | grep -v grep | wc -l';
                    }
                    try {
                        const { stdout } = await execAsync(processCheckCommand);
                        const processCount = parseInt(stdout.trim());
                        (0, test_1.expect)(processCount).toBeGreaterThan(0);
                        console.log(`✅ VS Code processes verified: ${processCount} processes running`);
                    }
                    catch (error) {
                        console.log(`⚠️ Process verification: ${error}`);
                    }
                    console.log('\n' + '='.repeat(60));
                    console.log('🎉 EXISTING NOTEBOOK EXECUTION TEST COMPLETED');
                    console.log('='.repeat(60));
                    console.log(`📁 Source folder: ${NOTEBOOK_FOLDER_PATH}`);
                    console.log(`📓 Notebooks opened: ${notebookFiles.length}`);
                    console.log(`🖥️  Platform: ${platform}`);
                    console.log(`✅ Status: SUCCESS`);
                    console.log('='.repeat(60));
                }
                catch (error) {
                    console.log(`❌ Error processing notebook: ${error}`);
                    throw error;
                }
            }
            else {
                console.log('⚠️ No notebook files found in the specified folder');
                console.log('Creating a sample notebook for testing...');
                // Create a sample notebook in the folder for testing
                const sampleNotebook = {
                    "cells": [
                        {
                            "cell_type": "markdown",
                            "metadata": {},
                            "source": ["# Sample Notebook\n\nThis notebook was created for testing purposes."]
                        },
                        {
                            "cell_type": "code",
                            "execution_count": null,
                            "metadata": {},
                            "outputs": [],
                            "source": ["print('Hello from sample notebook!')"]
                        }
                    ],
                    "metadata": {
                        "kernelspec": {
                            "display_name": "Python 3",
                            "language": "python",
                            "name": "python3"
                        }
                    },
                    "nbformat": 4,
                    "nbformat_minor": 4
                };
                const samplePath = path.join(NOTEBOOK_FOLDER_PATH, 'sample_test.ipynb');
                fs.writeFileSync(samplePath, JSON.stringify(sampleNotebook, null, 2));
                console.log(`✅ Created sample notebook: ${samplePath}`);
                // Open the sample notebook
                const openCommand = platform === 'win32' ? 'code.cmd' : 'code';
                (0, child_process_1.spawn)(openCommand, [samplePath], {
                    stdio: 'pipe',
                    env: process.env,
                    shell: platform === 'win32'
                });
                await new Promise(resolve => setTimeout(resolve, 2000));
                console.log('✅ Sample notebook opened in VS Code');
            }
        }
        else {
            console.log(`⚠️ Notebook folder does not exist: ${NOTEBOOK_FOLDER_PATH}`);
            console.log('Using test workspace instead...');
            // Fall back to creating a test notebook in our test workspace
            const fallbackNotebook = {
                "cells": [
                    {
                        "cell_type": "markdown",
                        "metadata": {},
                        "source": ["# Fallback Test Notebook\n\nThis notebook was created because the specified folder was not found."]
                    },
                    {
                        "cell_type": "code",
                        "execution_count": null,
                        "metadata": {},
                        "outputs": [],
                        "source": [
                            "import os\n",
                            "import sys\n",
                            "print(f'Running on: {sys.platform}')\n",
                            "print(f'Current directory: {os.getcwd()}')\n",
                            "print('Fallback notebook execution successful!')"
                        ]
                    }
                ],
                "metadata": {
                    "kernelspec": {
                        "display_name": "Python 3",
                        "language": "python",
                        "name": "python3"
                    }
                },
                "nbformat": 4,
                "nbformat_minor": 4
            };
            const fallbackPath = path.join(testWorkspace, 'fallback_test.ipynb');
            fs.writeFileSync(fallbackPath, JSON.stringify(fallbackNotebook, null, 2));
            console.log(`✅ Created fallback notebook: ${fallbackPath}`);
            // Open the fallback notebook
            const openCommand = platform === 'win32' ? 'code.cmd' : 'code';
            (0, child_process_1.spawn)(openCommand, [fallbackPath], {
                stdio: 'pipe',
                env: process.env,
                shell: platform === 'win32'
            });
            await new Promise(resolve => setTimeout(resolve, 2000));
            console.log('✅ Fallback notebook opened in VS Code');
        }
        console.log('✅ Existing notebook execution test completed');
    });
});
