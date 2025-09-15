import { test, expect } from '@playwright/test';
import { createVSCodeElectronHelper } from '../../electron/vscode-electron-mock';
import { createAzureMLHelper } from '../../helpers/azure-helpers-mock';
import { logTestStart, logTestEnd } from '../../helpers/logger';
import { config } from '../../helpers/config';
import * as path from 'path';

test.describe('VS Code Desktop Integration Tests', () => {
  let vscodeHelper: any;
  let azureHelper: any;
  const testWorkspaceFolder = path.join(process.cwd(), 'test-results', 'temp', 'vscode-workspace');

  test.beforeAll(async () => {
    // Initialize helpers
    azureHelper = createAzureMLHelper();
    
    // Ensure we have a running compute instance for remote development
    try {
      await azureHelper.startComputeInstance(config.compute.instanceName || 'test-compute-instance');
      // Mock helper doesn't need to wait for state changes
    } catch (error) {
      console.warn('Could not start compute instance for tests:', error.message);
    }
  });

  test.beforeEach(async () => {
    // Create fresh workspace for each test
    const fs = require('fs');
    if (fs.existsSync(testWorkspaceFolder)) {
      fs.rmSync(testWorkspaceFolder, { recursive: true, force: true });
    }
    fs.mkdirSync(testWorkspaceFolder, { recursive: true });
  });

  test.afterEach(async () => {
    // Cleanup VS Code instance
    if (vscodeHelper) {
      try {
        await vscodeHelper.close();
      } catch (error) {
        console.warn('Error closing VS Code:', error.message);
      }
    }
  });

  test('should launch VS Code and connect to Azure ML workspace @electron @integration', async () => {
    const testLogger = logTestStart('VS Code Desktop Launch and Azure ML Connection');
    
    try {
      // Initialize VS Code helper
      vscodeHelper = createVSCodeElectronHelper({
        workspaceFolder: testWorkspaceFolder,
        userDataDir: path.join(process.cwd(), '.vscode-test', 'user-data'),
        extensionsDir: path.join(process.cwd(), '.vscode-test', 'extensions'),
      }, testLogger);

      testLogger.step('Launching VS Code desktop application');
      await vscodeHelper.launch();
      
      // Wait for VS Code to fully load
      await vscodeHelper.waitForWorkbenchLoad();
      
      testLogger.step('Installing Azure ML extension');
      await vscodeHelper.installExtension('ms-toolsai.vscode-ai');
      
      // Wait for extension to be installed and activated
      await vscodeHelper.waitForExtensionActivation('ms-toolsai.vscode-ai');
      
      testLogger.step('Connecting to Azure account');
      await vscodeHelper.connectToAzure({
        tenantId: config.azure.tenantId,
        subscriptionId: config.azure.subscriptionId,
      });
      
      testLogger.step('Selecting Azure ML workspace');
      await vscodeHelper.selectAzureMLWorkspace(
        config.azure.subscriptionId,
        config.azure.resourceGroup,
        config.azure.workspaceName
      );
      
      // Verify workspace connection
      const workspaceStatus = await vscodeHelper.getAzureMLWorkspaceStatus();
      expect(workspaceStatus.connected).toBe(true);
      expect(workspaceStatus.workspaceName).toBe(config.azure.workspaceName);
      
      testLogger.step('Verifying Azure ML features are available');
      
      // Check that Azure ML commands are available
      const availableCommands = await vscodeHelper.getAvailableCommands();
      const azureMLCommands = availableCommands.filter(cmd => 
        cmd.includes('azureml') || cmd.includes('Azure ML')
      );
      
      expect(azureMLCommands.length).toBeGreaterThan(0);
      testLogger.info('Azure ML commands available', { count: azureMLCommands.length });
      
      // Verify compute instances are visible
      const computeInstances = await vscodeHelper.getVisibleComputeInstances();
      testLogger.info('Visible compute instances', { instances: computeInstances });
      
      // Take screenshot for verification
      await vscodeHelper.takeScreenshot('azure-ml-connected');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('VS Code Azure ML connection test failed', { error: error.message });
      await vscodeHelper?.takeScreenshot('azure-ml-connection-failed');
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should create and execute Python notebook in VS Code @electron @notebook', async () => {
    const testLogger = logTestStart('VS Code Desktop Notebook Creation and Execution');
    
    try {
      vscodeHelper = createVSCodeElectronHelper({
        workspaceFolder: testWorkspaceFolder,
        userDataDir: path.join(process.cwd(), '.vscode-test', 'user-data'),
      }, testLogger);

      await vscodeHelper.launch();
      await vscodeHelper.waitForWorkbenchLoad();
      
      // Install required extensions
      testLogger.step('Installing required extensions');
      await vscodeHelper.installExtension('ms-python.python');
      await vscodeHelper.installExtension('ms-toolsai.jupyter');
      await vscodeHelper.installExtension('ms-toolsai.vscode-ai');
      
      // Wait for extensions to activate
      await vscodeHelper.waitForExtensionActivation('ms-python.python');
      await vscodeHelper.waitForExtensionActivation('ms-toolsai.jupyter');
      
      testLogger.step('Creating new Jupyter notebook');
      const notebookPath = await vscodeHelper.createNewNotebook('test-notebook.ipynb');
      
      // Add cells to notebook
      testLogger.step('Adding cells to notebook');
      
      // Cell 1: Import libraries
      await vscodeHelper.addNotebookCell('code', `
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from datetime import datetime

print(f"Libraries imported successfully at {datetime.now()}")
print(f"NumPy version: {np.__version__}")
print(f"Pandas version: {pd.__version__}")
`);

      // Cell 2: Create sample data
      await vscodeHelper.addNotebookCell('code', `
# Create sample dataset
np.random.seed(42)
data = {
    'feature1': np.random.normal(0, 1, 100),
    'feature2': np.random.normal(2, 1.5, 100),
    'target': np.random.choice([0, 1], 100)
}

df = pd.DataFrame(data)
print(f"Dataset created with shape: {df.shape}")
print("\\nFirst 5 rows:")
print(df.head())
`);

      // Cell 3: Basic analysis
      await vscodeHelper.addNotebookCell('code', `
# Basic statistical analysis
print("Dataset Statistics:")
print(df.describe())

print("\\nTarget distribution:")
print(df['target'].value_counts())

# Create a simple plot
plt.figure(figsize=(10, 6))
plt.subplot(1, 2, 1)
plt.scatter(df['feature1'], df['feature2'], c=df['target'], alpha=0.6)
plt.xlabel('Feature 1')
plt.ylabel('Feature 2')
plt.title('Feature Distribution by Target')

plt.subplot(1, 2, 2)
plt.hist(df['feature1'], bins=20, alpha=0.7, label='Feature 1')
plt.hist(df['feature2'], bins=20, alpha=0.7, label='Feature 2')
plt.xlabel('Value')
plt.ylabel('Frequency')
plt.title('Feature Distributions')
plt.legend()

plt.tight_layout()
plt.show()

print("Analysis completed successfully!")
`);

      testLogger.step('Selecting Python kernel');
      await vscodeHelper.selectNotebookKernel('Python 3');
      
      testLogger.step('Executing notebook cells');
      const executionResults = await vscodeHelper.executeAllNotebookCells();
      
      // Verify execution results
      expect(executionResults.totalCells).toBe(3);
      expect(executionResults.successfulCells).toBe(3);
      expect(executionResults.failedCells).toBe(0);
      
      testLogger.info('Notebook execution completed', {
        totalCells: executionResults.totalCells,
        successful: executionResults.successfulCells,
        failed: executionResults.failedCells,
      });
      
      // Verify cell outputs contain expected content
      const cellOutputs = await vscodeHelper.getNotebookCellOutputs();
      
      // Check first cell output
      expect(cellOutputs[0]).toContain('Libraries imported successfully');
      expect(cellOutputs[0]).toContain('NumPy version');
      expect(cellOutputs[0]).toContain('Pandas version');
      
      // Check second cell output
      expect(cellOutputs[1]).toContain('Dataset created with shape: (100, 3)');
      expect(cellOutputs[1]).toContain('First 5 rows:');
      
      // Check third cell output
      expect(cellOutputs[2]).toContain('Dataset Statistics:');
      expect(cellOutputs[2]).toContain('Analysis completed successfully!');
      
      testLogger.step('Saving notebook');
      await vscodeHelper.saveNotebook();
      
      // Verify notebook file exists
      const fs = require('fs');
      expect(fs.existsSync(notebookPath)).toBe(true);
      
      // Take screenshot of completed notebook
      await vscodeHelper.takeScreenshot('notebook-executed');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('VS Code notebook execution test failed', { error: error.message });
      await vscodeHelper?.takeScreenshot('notebook-execution-failed');
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should connect to remote compute instance and execute code @electron @remote', async () => {
    const testLogger = logTestStart('VS Code Desktop Remote Compute Connection');
    
    try {
      vscodeHelper = createVSCodeElectronHelper({
        workspaceFolder: testWorkspaceFolder,
        userDataDir: path.join(process.cwd(), '.vscode-test', 'user-data'),
      }, testLogger);

      await vscodeHelper.launch();
      await vscodeHelper.waitForWorkbenchLoad();
      
      // Install required extensions
      testLogger.step('Installing remote development extensions');
      await vscodeHelper.installExtension('ms-vscode-remote.remote-ssh');
      await vscodeHelper.installExtension('ms-toolsai.vscode-ai');
      await vscodeHelper.installExtension('ms-python.python');
      
      await vscodeHelper.waitForExtensionActivation('ms-vscode-remote.remote-ssh');
      
      testLogger.step('Connecting to Azure account');
      await vscodeHelper.connectToAzure({
        tenantId: config.azure.tenantId,
        subscriptionId: config.azure.subscriptionId,
      });
      
      testLogger.step('Connecting to remote compute instance');
      
      // Get compute instance connection details
      const computeInstance = await azureHelper.getComputeInstanceStatus(config.compute.instanceName);
      expect(computeInstance.state.toLowerCase()).toBe('running');
      
      const connectionString = await azureHelper.getComputeInstanceSSHConnection(config.compute.instanceName);
      testLogger.info('Compute instance connection details', { 
        name: computeInstance.name,
        state: computeInstance.state,
        connectionString: connectionString.replace(/:[^:]*@/, ':***@') // Hide password in logs
      });
      
      // Connect to remote compute
      await vscodeHelper.connectToRemoteSSH(connectionString);
      
      // Wait for remote connection to establish
      await vscodeHelper.waitForRemoteConnection(60000);
      
      // Verify remote connection
      const remoteStatus = await vscodeHelper.getRemoteConnectionStatus();
      expect(remoteStatus.connected).toBe(true);
      expect(remoteStatus.host).toContain(config.compute.instanceName);
      
      testLogger.step('Creating Python script on remote compute');
      
      // Create a test Python script
      const scriptContent = `#!/usr/bin/env python3
"""
Test script for remote execution on Azure ML compute instance.
"""

import os
import sys
import platform
import subprocess
from datetime import datetime

def main():
    print("=== Remote Execution Test ===")
    print(f"Execution time: {datetime.now()}")
    print(f"Python version: {sys.version}")
    print(f"Platform: {platform.platform()}")
    print(f"Current directory: {os.getcwd()}")
    print(f"User: {os.getenv('USER', 'unknown')}")
    
    # Check if we're on Azure ML compute
    print("\\n=== Environment Check ===")
    if os.path.exists('/mnt/azmnt'):
        print("✓ Azure ML compute instance detected")
    else:
        print("⚠ Not on Azure ML compute instance")
    
    # Check available Python packages
    print("\\n=== Package Check ===")
    packages_to_check = ['numpy', 'pandas', 'scikit-learn', 'matplotlib', 'azureml-core']
    
    for package in packages_to_check:
        try:
            __import__(package)
            print(f"✓ {package} is available")
        except ImportError:
            print(f"✗ {package} is not available")
    
    # Simple computation test
    print("\\n=== Computation Test ===")
    import numpy as np
    
    # Generate random data and perform computation
    data = np.random.random((1000, 1000))
    result = np.mean(data)
    
    print(f"Generated 1000x1000 random matrix")
    print(f"Mean value: {result:.6f}")
    print(f"Expected ~0.5, actual: {result:.6f}")
    
    if 0.45 <= result <= 0.55:
        print("✓ Computation test passed")
    else:
        print("✗ Computation test failed")
    
    print("\\n=== Test Completed Successfully ===")
    return 0

if __name__ == "__main__":
    sys.exit(main())
`;

      const scriptPath = await vscodeHelper.createFile('remote_test.py', scriptContent);
      
      testLogger.step('Executing Python script on remote compute');
      
      // Open integrated terminal
      await vscodeHelper.openIntegratedTerminal();
      
      // Execute the script
      const executionResult = await vscodeHelper.executeTerminalCommand(`python ${scriptPath}`);
      
      // Verify execution results
      expect(executionResult.exitCode).toBe(0);
      expect(executionResult.output).toContain('Remote Execution Test');
      expect(executionResult.output).toContain('Azure ML compute instance detected');
      expect(executionResult.output).toContain('Test Completed Successfully');
      
      testLogger.info('Remote script execution completed', {
        exitCode: executionResult.exitCode,
        outputLength: executionResult.output.length,
      });
      
      testLogger.step('Testing file synchronization');
      
      // Create a local file and verify it syncs to remote
      const localTestFile = await vscodeHelper.createFile('sync_test.txt', 'This is a sync test file');
      
      // Wait for sync and verify file exists on remote
      await vscodeHelper.waitForFileSyncToRemote('sync_test.txt', 10000);
      
      const remoteFileExists = await vscodeHelper.executeTerminalCommand('ls sync_test.txt');
      expect(remoteFileExists.exitCode).toBe(0);
      
      testLogger.step('Testing remote debugging');
      
      // Create a simple Python script for debugging
      const debugScript = `
import time

def fibonacci(n):
    if n <= 1:
        return n
    return fibonacci(n-1) + fibonacci(n-2)

def main():
    print("Starting Fibonacci calculation...")
    for i in range(10):
        result = fibonacci(i)
        print(f"fibonacci({i}) = {result}")
        time.sleep(0.1)  # Breakpoint opportunity
    print("Calculation completed!")

if __name__ == "__main__":
    main()
`;

      const debugScriptPath = await vscodeHelper.createFile('debug_test.py', debugScript);
      
      // Set breakpoint and start debugging
      await vscodeHelper.setBreakpoint(debugScriptPath, 12); // Line with time.sleep
      await vscodeHelper.startDebugging(debugScriptPath);
      
      // Wait for breakpoint to be hit
      const debugSession = await vscodeHelper.waitForDebugBreakpoint(30000);
      expect(debugSession.active).toBe(true);
      expect(debugSession.line).toBe(12);
      
      // Continue execution
      await vscodeHelper.continueDebugging();
      await vscodeHelper.waitForDebugCompletion(30000);
      
      testLogger.info('Remote debugging test completed successfully');
      
      // Take screenshot of remote development environment
      await vscodeHelper.takeScreenshot('remote-development');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('VS Code remote compute test failed', { error: error.message });
      await vscodeHelper?.takeScreenshot('remote-compute-failed');
      logTestEnd(testLogger, false);
      throw error;
    }
  });
});