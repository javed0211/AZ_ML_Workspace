"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const test_1 = require("@playwright/test");
const vscode_electron_1 = require("../utils/vscode-electron");
const test_helpers_1 = require("../utils/test-helpers");
/**
 * Azure ML integration tests for VS Code
 */
test_1.test.describe('Azure ML VS Code Integration', () => {
    let vscode;
    let testWorkspace;
    test_1.test.beforeEach(async () => {
        test_helpers_1.TestHelpers.logStep('Setting up VS Code for Azure ML testing');
        vscode = new vscode_electron_1.VSCodeElectron();
        // Launch VS Code with the main workspace
        const workspaceRoot = test_helpers_1.TestHelpers.getWorkspaceRoot();
        await vscode.launch(workspaceRoot);
        await test_helpers_1.TestHelpers.setupVSCodeForTesting(vscode);
        // Create test workspace
        testWorkspace = await test_helpers_1.TestHelpers.createTestWorkspace();
    });
    test_1.test.afterEach(async () => {
        test_helpers_1.TestHelpers.logStep('Cleaning up Azure ML test environment');
        if (vscode.isRunning()) {
            await vscode.close();
        }
        test_helpers_1.TestHelpers.cleanupTempFiles();
        if (testWorkspace) {
            test_helpers_1.TestHelpers.cleanupTestWorkspace(testWorkspace);
        }
    });
    (0, test_1.test)('should open Azure ML workspace configuration', async () => {
        test_helpers_1.TestHelpers.logStep('Opening Azure ML configuration');
        // Try to open the Azure ML config file
        const configPath = test_helpers_1.TestHelpers.getTestFilePath('azure-ml-config.json');
        try {
            await vscode.openFile(configPath);
            await test_helpers_1.TestHelpers.wait(2000);
            // Verify file is open
            const window = vscode.getMainWindow();
            const editor = await window.$('.monaco-editor');
            (0, test_1.expect)(editor).toBeTruthy();
            // Take screenshot
            await vscode.takeScreenshot();
        }
        catch (error) {
            console.log('Azure ML config file not found, creating sample config');
            // Create new file with sample Azure ML config
            await vscode.createNewFile();
            await test_helpers_1.TestHelpers.wait(1000);
            const sampleConfig = `{
  "subscription_id": "your-subscription-id",
  "resource_group": "your-resource-group",
  "workspace_name": "your-workspace-name",
  "compute_target": "your-compute-target",
  "environment": {
    "name": "test-environment",
    "python_version": "3.8",
    "conda_dependencies": [
      "scikit-learn",
      "pandas",
      "numpy"
    ]
  }
}`;
            await vscode.typeInEditor(sampleConfig);
            await test_helpers_1.TestHelpers.wait(1000);
            // Save the file
            await vscode.saveFile();
            await test_helpers_1.TestHelpers.wait(1000);
            // Take screenshot
            await vscode.takeScreenshot();
        }
    });
    (0, test_1.test)('should create and edit Python ML script', async () => {
        test_helpers_1.TestHelpers.logStep('Creating Python ML script');
        // Create new Python file
        await vscode.createNewFile();
        await test_helpers_1.TestHelpers.wait(1000);
        const mlScript = `import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score
import joblib

# Sample Azure ML training script
def train_model():
    """Train a simple logistic regression model"""
    
    # Generate sample data
    np.random.seed(42)
    X = np.random.randn(1000, 4)
    y = (X[:, 0] + X[:, 1] > 0).astype(int)
    
    # Split data
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=42
    )
    
    # Train model
    model = LogisticRegression()
    model.fit(X_train, y_train)
    
    # Evaluate
    y_pred = model.predict(X_test)
    accuracy = accuracy_score(y_test, y_pred)
    
    print(f"Model accuracy: {accuracy:.4f}")
    
    # Save model
    joblib.dump(model, 'model.pkl')
    
    return model, accuracy

if __name__ == "__main__":
    model, accuracy = train_model()
    print("Training completed successfully!")
`;
        test_helpers_1.TestHelpers.logStep('Typing ML script content');
        await vscode.typeInEditor(mlScript);
        await test_helpers_1.TestHelpers.wait(2000);
        // Save the file as Python file
        const window = vscode.getMainWindow();
        const isMac = process.platform === 'darwin';
        const modifier = isMac ? 'Meta' : 'Control';
        await window.keyboard.press(`${modifier}+Shift+KeyS`); // Save As
        await test_helpers_1.TestHelpers.wait(1000);
        // Type filename
        await window.keyboard.type('azure_ml_training.py');
        await window.keyboard.press('Enter');
        await test_helpers_1.TestHelpers.wait(2000);
        // Take screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should work with Jupyter notebooks', async () => {
        test_helpers_1.TestHelpers.logStep('Working with Jupyter notebooks');
        // Try to create a new Jupyter notebook
        try {
            await vscode.executeCommand('Jupyter: Create New Jupyter Notebook');
            await test_helpers_1.TestHelpers.wait(3000);
            // If successful, add some notebook content
            const notebookContent = `# Azure ML Notebook Test

## Data Loading
import pandas as pd
import numpy as np

# Load sample data
data = pd.DataFrame({
    'feature1': np.random.randn(100),
    'feature2': np.random.randn(100),
    'target': np.random.randint(0, 2, 100)
})

print("Data shape:", data.shape)
data.head()`;
            await vscode.typeInEditor(notebookContent);
            await test_helpers_1.TestHelpers.wait(2000);
            // Take screenshot
            await vscode.takeScreenshot();
        }
        catch (error) {
            console.log('Jupyter extension not available, skipping notebook test');
            // Create a regular Python file instead
            await vscode.createNewFile();
            await test_helpers_1.TestHelpers.wait(1000);
            const pythonContent = `# Azure ML Data Analysis Script
import pandas as pd
import numpy as np

# This would be a Jupyter notebook in a real scenario
print("Azure ML data analysis script")

# Sample data processing
data = pd.DataFrame({
    'feature1': np.random.randn(100),
    'feature2': np.random.randn(100),
    'target': np.random.randint(0, 2, 100)
})

print("Data shape:", data.shape)
print(data.head())
`;
            await vscode.typeInEditor(pythonContent);
            await test_helpers_1.TestHelpers.wait(1000);
            // Take screenshot
            await vscode.takeScreenshot();
        }
    });
    (0, test_1.test)('should handle Azure ML extension commands', async () => {
        test_helpers_1.TestHelpers.logStep('Testing Azure ML extension commands');
        // Try various Azure ML related commands
        const azureMLCommands = [
            'Azure Machine Learning: Sign in to Azure',
            'Azure Machine Learning: Create New Experiment',
            'Azure Machine Learning: Submit Run',
            'Python: Select Interpreter'
        ];
        for (const command of azureMLCommands) {
            try {
                test_helpers_1.TestHelpers.logStep(`Attempting command: ${command}`);
                await vscode.openCommandPalette();
                await test_helpers_1.TestHelpers.wait(1000);
                const window = vscode.getMainWindow();
                await window.keyboard.type(command);
                await test_helpers_1.TestHelpers.wait(1000);
                // Check if command is available
                const commandItem = await window.$('.quick-input-list .monaco-list-row');
                if (commandItem) {
                    console.log(`Command "${command}" is available`);
                    // Press Escape to close command palette without executing
                    await window.keyboard.press('Escape');
                }
                else {
                    console.log(`Command "${command}" not found`);
                    await window.keyboard.press('Escape');
                }
                await test_helpers_1.TestHelpers.wait(500);
            }
            catch (error) {
                console.log(`Command "${command}" failed:`, error);
                // Try to close any open dialogs
                const window = vscode.getMainWindow();
                await window.keyboard.press('Escape');
                await test_helpers_1.TestHelpers.wait(500);
            }
        }
        // Take final screenshot
        await vscode.takeScreenshot();
    });
    (0, test_1.test)('should open and navigate project structure', async () => {
        test_helpers_1.TestHelpers.logStep('Testing project navigation');
        // Open the main workspace folder
        const workspaceRoot = test_helpers_1.TestHelpers.getWorkspaceRoot();
        await vscode.openFolder(workspaceRoot);
        await test_helpers_1.TestHelpers.wait(3000);
        // Try to expand file explorer
        try {
            const window = vscode.getMainWindow();
            // Look for file explorer
            const explorer = await window.$('.explorer-viewlet');
            if (explorer) {
                test_helpers_1.TestHelpers.logStep('File explorer found');
                // Try to expand some folders
                const folders = await window.$$('.monaco-list-row[aria-level="1"]');
                if (folders.length > 0) {
                    // Click on first folder to expand
                    await folders[0].click();
                    await test_helpers_1.TestHelpers.wait(1000);
                }
            }
            // Take screenshot of project structure
            await vscode.takeScreenshot();
        }
        catch (error) {
            console.log('File explorer navigation failed:', error);
            await vscode.takeScreenshot();
        }
    });
    (0, test_1.test)('should handle terminal operations', async () => {
        test_helpers_1.TestHelpers.logStep('Testing terminal operations');
        // Open terminal
        await vscode.executeCommand('Terminal: Create New Terminal');
        await test_helpers_1.TestHelpers.wait(2000);
        const window = vscode.getMainWindow();
        // Check if terminal is open
        const terminal = await window.$('.terminal-wrapper');
        (0, test_1.expect)(terminal).toBeTruthy();
        // Try to type a simple command
        try {
            // Click in terminal area
            await window.click('.xterm-screen');
            await test_helpers_1.TestHelpers.wait(500);
            // Type a simple command
            await window.keyboard.type('echo "Hello from VS Code automation!"');
            await window.keyboard.press('Enter');
            await test_helpers_1.TestHelpers.wait(2000);
            // Type Python version check
            await window.keyboard.type('python --version');
            await window.keyboard.press('Enter');
            await test_helpers_1.TestHelpers.wait(2000);
        }
        catch (error) {
            console.log('Terminal interaction failed:', error);
        }
        // Take screenshot
        await vscode.takeScreenshot();
    });
});
