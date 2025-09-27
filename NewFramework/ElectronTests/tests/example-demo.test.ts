import { test, expect } from '@playwright/test';
import { VSCodeElectron } from '../utils/vscode-electron';
import { TestHelpers } from '../utils/test-helpers';

/**
 * Example demonstration test showing key features of the Electron testing framework
 * This test serves as a comprehensive example of VS Code automation capabilities
 */
test.describe('VS Code Electron Framework Demo', () => {
  let vscode: VSCodeElectron;

  test.beforeEach(async () => {
    TestHelpers.logStep('🚀 Launching VS Code for demo');
    vscode = new VSCodeElectron();
    await vscode.launch();
    await TestHelpers.setupVSCodeForTesting(vscode);
  });

  test.afterEach(async () => {
    TestHelpers.logStep('🧹 Cleaning up demo environment');
    if (vscode.isRunning()) {
      await vscode.close();
    }
    TestHelpers.cleanupTempFiles();
  });

  test('Complete VS Code automation workflow demo', async () => {
    TestHelpers.logStep('📋 Starting complete workflow demonstration');
    
    // Step 1: Verify VS Code is running
    TestHelpers.logStep('1️⃣ Verifying VS Code launch');
    expect(vscode.isRunning()).toBe(true);
    await vscode.takeScreenshot();
    
    // Step 2: Create a new Python file
    TestHelpers.logStep('2️⃣ Creating new Python file');
    await vscode.createNewFile();
    await TestHelpers.wait(1000);
    
    // Step 3: Write a complete Python script
    TestHelpers.logStep('3️⃣ Writing Python ML script');
    const pythonScript = `#!/usr/bin/env python3
"""
Azure ML Demo Script
This script demonstrates a simple machine learning workflow
that could be used with Azure ML services.
"""

import pandas as pd
import numpy as np
from datetime import datetime
import json

class AzureMLDemo:
    """Demo class for Azure ML integration"""
    
    def __init__(self, experiment_name: str = "demo-experiment"):
        self.experiment_name = experiment_name
        self.start_time = datetime.now()
        print(f"🧪 Starting experiment: {experiment_name}")
    
    def generate_sample_data(self, n_samples: int = 1000) -> pd.DataFrame:
        """Generate sample dataset for demonstration"""
        print(f"📊 Generating {n_samples} sample data points...")
        
        np.random.seed(42)  # For reproducibility
        
        data = {
            'feature_1': np.random.normal(0, 1, n_samples),
            'feature_2': np.random.normal(2, 1.5, n_samples),
            'feature_3': np.random.exponential(1, n_samples),
            'category': np.random.choice(['A', 'B', 'C'], n_samples),
            'target': np.random.randint(0, 2, n_samples)
        }
        
        df = pd.DataFrame(data)
        print(f"✅ Generated dataset with shape: {df.shape}")
        return df
    
    def analyze_data(self, df: pd.DataFrame) -> dict:
        """Perform basic data analysis"""
        print("🔍 Analyzing data...")
        
        analysis = {
            'shape': df.shape,
            'columns': list(df.columns),
            'missing_values': df.isnull().sum().to_dict(),
            'numeric_summary': df.describe().to_dict(),
            'category_counts': df['category'].value_counts().to_dict()
        }
        
        print("✅ Data analysis completed")
        return analysis
    
    def save_results(self, analysis: dict, filename: str = "analysis_results.json"):
        """Save analysis results"""
        print(f"💾 Saving results to {filename}")
        
        # Add metadata
        results = {
            'experiment_name': self.experiment_name,
            'timestamp': self.start_time.isoformat(),
            'analysis': analysis
        }
        
        # In a real scenario, this would save to Azure ML workspace
        with open(filename, 'w') as f:
            json.dump(results, f, indent=2, default=str)
        
        print("✅ Results saved successfully")
        return results

def main():
    """Main execution function"""
    print("🚀 Azure ML Demo Script Starting...")
    
    # Initialize demo
    demo = AzureMLDemo("vscode-automation-demo")
    
    # Generate and analyze data
    df = demo.generate_sample_data(500)
    analysis = demo.analyze_data(df)
    results = demo.save_results(analysis)
    
    print("🎉 Demo completed successfully!")
    print(f"📈 Processed {df.shape[0]} samples with {df.shape[1]} features")
    
    return results

if __name__ == "__main__":
    results = main()
    print("\\n📋 Experiment Summary:")
    print(f"   Experiment: {results['experiment_name']}")
    print(f"   Timestamp: {results['timestamp']}")
    print(f"   Data Shape: {results['analysis']['shape']}")
`;
    
    await vscode.typeInEditor(pythonScript);
    await TestHelpers.wait(2000);
    await vscode.takeScreenshot();
    
    // Step 4: Save the file
    TestHelpers.logStep('4️⃣ Saving Python file');
    const window = vscode.getMainWindow();
    const isMac = process.platform === 'darwin';
    const modifier = isMac ? 'Meta' : 'Control';
    
    await window.keyboard.press(`${modifier}+Shift+KeyS`); // Save As
    await TestHelpers.wait(1000);
    await window.keyboard.type('azure_ml_demo.py');
    await window.keyboard.press('Enter');
    await TestHelpers.wait(2000);
    await vscode.takeScreenshot();
    
    // Step 5: Open terminal and run the script
    TestHelpers.logStep('5️⃣ Opening terminal');
    await vscode.executeCommand('Terminal: Create New Terminal');
    await TestHelpers.wait(2000);
    
    // Verify terminal is open
    const terminal = await window.$('.terminal-wrapper');
    expect(terminal).toBeTruthy();
    await vscode.takeScreenshot();
    
    // Step 6: Execute Python script in terminal
    TestHelpers.logStep('6️⃣ Executing Python script');
    try {
      await window.click('.xterm-screen');
      await TestHelpers.wait(500);
      
      // Check Python version first
      await window.keyboard.type('python3 --version');
      await window.keyboard.press('Enter');
      await TestHelpers.wait(2000);
      
      // Run the script (if Python is available)
      await window.keyboard.type('python3 azure_ml_demo.py');
      await window.keyboard.press('Enter');
      await TestHelpers.wait(3000);
      
    } catch (error) {
      TestHelpers.logStep('⚠️ Python execution skipped (Python may not be available)');
    }
    
    await vscode.takeScreenshot();
    
    // Step 7: Demonstrate command palette usage
    TestHelpers.logStep('7️⃣ Demonstrating command palette');
    await vscode.openCommandPalette();
    await TestHelpers.wait(1000);
    
    // Search for Python-related commands
    await window.keyboard.type('Python: Select Interpreter');
    await TestHelpers.wait(1000);
    await vscode.takeScreenshot();
    
    // Close command palette
    await window.keyboard.press('Escape');
    await TestHelpers.wait(500);
    
    // Step 8: Create a configuration file
    TestHelpers.logStep('8️⃣ Creating Azure ML configuration');
    await vscode.createNewFile();
    await TestHelpers.wait(1000);
    
    const configContent = `{
  "azure_ml_config": {
    "subscription_id": "your-subscription-id-here",
    "resource_group": "your-resource-group",
    "workspace_name": "your-ml-workspace",
    "location": "eastus",
    "compute_targets": {
      "training": {
        "name": "cpu-cluster",
        "type": "AmlCompute",
        "min_nodes": 0,
        "max_nodes": 4,
        "vm_size": "STANDARD_D2_V2"
      },
      "inference": {
        "name": "aci-service",
        "type": "ACI",
        "cpu_cores": 1,
        "memory_gb": 1
      }
    },
    "environments": {
      "training_env": {
        "name": "sklearn-env",
        "python_version": "3.8",
        "conda_dependencies": [
          "scikit-learn==1.0.2",
          "pandas==1.4.2",
          "numpy==1.21.5",
          "joblib==1.1.0"
        ],
        "pip_dependencies": [
          "azureml-sdk==1.43.0",
          "azureml-core==1.43.0"
        ]
      }
    },
    "datasets": {
      "training_data": {
        "name": "demo-training-data",
        "path": "./data/training.csv",
        "description": "Training dataset for demo model"
      }
    },
    "experiments": {
      "demo_experiment": {
        "name": "vscode-automation-demo",
        "description": "Demonstration experiment created via VS Code automation"
      }
    }
  },
  "local_settings": {
    "data_folder": "./data",
    "output_folder": "./outputs",
    "model_folder": "./models",
    "logs_folder": "./logs"
  }
}`;
    
    await vscode.typeInEditor(configContent);
    await TestHelpers.wait(1000);
    
    // Save configuration file
    await window.keyboard.press(`${modifier}+Shift+KeyS`);
    await TestHelpers.wait(1000);
    await window.keyboard.type('azure_ml_config.json');
    await window.keyboard.press('Enter');
    await TestHelpers.wait(2000);
    await vscode.takeScreenshot();
    
    // Step 9: Demonstrate file navigation
    TestHelpers.logStep('9️⃣ Demonstrating file navigation');
    
    // Open file explorer
    await vscode.executeCommand('View: Show Explorer');
    await TestHelpers.wait(2000);
    
    // Try to navigate in explorer
    try {
      const explorer = await window.$('.explorer-viewlet');
      if (explorer) {
        TestHelpers.logStep('✅ File explorer is visible');
      }
    } catch (error) {
      TestHelpers.logStep('⚠️ File explorer navigation skipped');
    }
    
    await vscode.takeScreenshot();
    
    // Step 10: Final verification and cleanup
    TestHelpers.logStep('🔟 Final verification');
    
    // Verify VS Code is still responsive
    const isStateValid = await TestHelpers.verifyVSCodeState(vscode);
    expect(isStateValid).toBe(true);
    
    // Take final screenshot
    await vscode.takeScreenshot();
    
    TestHelpers.logStep('🎉 Demo workflow completed successfully!');
    
    // Log summary
    console.log(`
📊 Demo Summary:
   ✅ VS Code launched and verified
   ✅ Python ML script created (${pythonScript.split('\\n').length} lines)
   ✅ Configuration file created
   ✅ Terminal operations demonstrated
   ✅ Command palette usage shown
   ✅ File operations completed
   ✅ Screenshots captured for verification
   
🔧 Framework Features Demonstrated:
   • Cross-platform VS Code automation
   • File creation and editing
   • Terminal integration
   • Command palette automation
   • Keyboard shortcuts
   • Screenshot capture
   • Error handling and verification
   
📁 Files Created:
   • azure_ml_demo.py (Python ML script)
   • azure_ml_config.json (Configuration file)
   
🎯 This demo shows how the framework can be used for:
   • Azure ML workflow automation
   • Code generation and editing
   • Configuration management
   • Testing VS Code extensions
   • CI/CD integration
    `);
  });
});