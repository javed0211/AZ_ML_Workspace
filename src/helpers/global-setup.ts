import { FullConfig } from '@playwright/test';
import { logger } from './logger';
import { config, validateRequiredConfig } from './config';
import { authManager } from './auth';
import { cliRunner } from './cli-runner';
import { createAzureMLHelper } from './azure-helpers';
import * as fs from 'fs';
import * as path from 'path';

async function globalSetup(config: FullConfig) {
  logger.info('Starting global setup for Azure ML Workspace automation tests');

  try {
    // Validate required configuration
    await validateConfiguration();

    // Setup directories
    await setupDirectories();

    // Authenticate with Azure
    await authenticateWithAzure();

    // Validate Azure access
    await validateAzureAccess();

    // Setup test environment
    await setupTestEnvironment();

    logger.info('Global setup completed successfully');
  } catch (error) {
    logger.error('Global setup failed', { error: error.message, stack: error.stack });
    throw error;
  }
}

async function validateConfiguration(): Promise<void> {
  logger.info('Validating configuration');

  const requiredConfigs = [
    'azure.tenantId',
    'azure.clientId',
    'azure.subscriptionId',
    'azure.resourceGroup',
    'azure.workspaceName',
  ];

  try {
    validateRequiredConfig(requiredConfigs);
    logger.info('Configuration validation passed');
  } catch (error) {
    logger.error('Configuration validation failed', { error: error.message });
    throw error;
  }
}

async function setupDirectories(): Promise<void> {
  logger.info('Setting up test directories');

  const directories = [
    config.artifacts.path,
    path.join(config.artifacts.path, 'logs'),
    path.join(config.artifacts.path, 'screenshots'),
    path.join(config.artifacts.path, 'videos'),
    path.join(config.artifacts.path, 'traces'),
    path.join(config.artifacts.path, 'reports'),
    path.join(config.artifacts.path, 'notebooks'),
    path.join(config.artifacts.path, 'temp'),
  ];

  for (const dir of directories) {
    if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
      logger.debug(`Created directory: ${dir}`);
    }
  }

  logger.info('Test directories setup completed');
}

async function authenticateWithAzure(): Promise<void> {
  logger.info('Authenticating with Azure');

  // Skip authentication in test mode
  if (process.env.NODE_ENV === 'test' || process.env.SKIP_AZURE_AUTH === 'true') {
    logger.info('Skipping Azure authentication in test mode');
    return;
  }

  try {
    // Validate authentication
    const isValid = await authManager.validateAuthentication();
    if (!isValid) {
      throw new Error('Azure authentication failed');
    }

    // If using service principal, login via CLI
    if (config.azure.clientSecret) {
      await cliRunner.azLoginServicePrincipal(
        config.azure.clientId,
        config.azure.clientSecret,
        config.azure.tenantId
      );
    } else if (!config.auth.useManagedIdentity) {
      // Interactive login for development
      await cliRunner.azLogin();
    }

    // Set subscription
    await cliRunner.azSetSubscription(config.azure.subscriptionId);

    logger.info('Azure authentication completed');
  } catch (error) {
    logger.error('Azure authentication failed', { error: error.message });
    throw error;
  }
}

async function validateAzureAccess(): Promise<void> {
  logger.info('Validating Azure access');

  // Skip Azure access validation in test mode
  if (process.env.NODE_ENV === 'test' || process.env.SKIP_AZURE_AUTH === 'true') {
    logger.info('Skipping Azure access validation in test mode');
    return;
  }

  try {
    const azureHelper = createAzureMLHelper();
    
    // Validate workspace access
    const hasAccess = await azureHelper.validateWorkspaceAccess();
    if (!hasAccess) {
      throw new Error('Cannot access Azure ML workspace');
    }

    logger.info('Azure access validation passed');
  } catch (error) {
    logger.error('Azure access validation failed', { error: error.message });
    throw error;
  }
}

async function setupTestEnvironment(): Promise<void> {
  logger.info('Setting up test environment');

  try {
    // Install required CLI extensions if needed
    await installRequiredExtensions();

    // Setup VS Code user data directory for Electron tests
    await setupVSCodeEnvironment();

    // Create test data files
    await createTestData();

    logger.info('Test environment setup completed');
  } catch (error) {
    logger.error('Test environment setup failed', { error: error.message });
    throw error;
  }
}

async function installRequiredExtensions(): Promise<void> {
  logger.info('Installing required Azure CLI extensions');

  // Skip CLI extension installation in test mode
  if (process.env.NODE_ENV === 'test' || process.env.SKIP_AZURE_AUTH === 'true') {
    logger.info('Skipping Azure CLI extension installation in test mode');
    return;
  }

  const extensions = ['ml', 'ssh'];

  for (const extension of extensions) {
    try {
      const result = await cliRunner.run('az', ['extension', 'show', '--name', extension], {
        expectNonZeroExit: true,
        retries: 0,
      });

      if (result.exitCode !== 0) {
        logger.info(`Installing Azure CLI extension: ${extension}`);
        await cliRunner.run('az', ['extension', 'add', '--name', extension]);
        logger.info(`Azure CLI extension installed: ${extension}`);
      } else {
        logger.debug(`Azure CLI extension already installed: ${extension}`);
      }
    } catch (error) {
      logger.warn(`Failed to install Azure CLI extension: ${extension}`, { error: error.message });
    }
  }
}

async function setupVSCodeEnvironment(): Promise<void> {
  logger.info('Setting up VS Code environment for Electron tests');

  try {
    const userDataDir = config.electron.userDataDir;
    
    if (!fs.existsSync(userDataDir)) {
      fs.mkdirSync(userDataDir, { recursive: true });
    }

    // Create User settings directory
    const userDir = path.join(userDataDir, 'User');
    if (!fs.existsSync(userDir)) {
      fs.mkdirSync(userDir, { recursive: true });
    }

    // Create default settings.json
    const settingsPath = path.join(userDir, 'settings.json');
    const defaultSettings = {
      'workbench.startupEditor': 'none',
      'extensions.autoUpdate': false,
      'extensions.autoCheckUpdates': false,
      'update.mode': 'none',
      'telemetry.telemetryLevel': 'off',
      'workbench.enableExperiments': false,
      'workbench.settings.enableNaturalLanguageSearch': false,
    };

    fs.writeFileSync(settingsPath, JSON.stringify(defaultSettings, null, 2));

    logger.info('VS Code environment setup completed');
  } catch (error) {
    logger.error('VS Code environment setup failed', { error: error.message });
    throw error;
  }
}

async function createTestData(): Promise<void> {
  logger.info('Creating test data files');

  try {
    const testDataDir = path.join(config.artifacts.path, 'test-data');
    if (!fs.existsSync(testDataDir)) {
      fs.mkdirSync(testDataDir, { recursive: true });
    }

    // Create sample notebook
    const sampleNotebook = {
      cells: [
        {
          cell_type: 'markdown',
          metadata: {},
          source: ['# Sample Test Notebook\n', '\n', 'This is a test notebook for automation.'],
        },
        {
          cell_type: 'code',
          execution_count: null,
          metadata: {},
          outputs: [],
          source: ['print("Hello, Azure ML!")'],
        },
        {
          cell_type: 'code',
          execution_count: null,
          metadata: {},
          outputs: [],
          source: ['import pandas as pd\n', 'import numpy as np\n', '\n', 'print("Libraries imported successfully")'],
        },
      ],
      metadata: {
        kernelspec: {
          display_name: 'Python 3',
          language: 'python',
          name: 'python3',
        },
        language_info: {
          name: 'python',
          version: '3.8.0',
        },
      },
      nbformat: 4,
      nbformat_minor: 4,
    };

    const notebookPath = path.join(testDataDir, 'sample-test-notebook.ipynb');
    fs.writeFileSync(notebookPath, JSON.stringify(sampleNotebook, null, 2));

    // Create sample Python script
    const sampleScript = `#!/usr/bin/env python3
"""
Sample Python script for testing Azure ML automation.
"""

import os
import sys
from datetime import datetime

def main():
    print(f"Test script executed at: {datetime.now()}")
    print(f"Python version: {sys.version}")
    print(f"Current directory: {os.getcwd()}")
    
    # Test Azure ML SDK import
    try:
        import azureml.core
        print(f"Azure ML SDK version: {azureml.core.VERSION}")
    except ImportError:
        print("Azure ML SDK not available")
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
`;

    const scriptPath = path.join(testDataDir, 'sample-test-script.py');
    fs.writeFileSync(scriptPath, sampleScript);

    // Create sample job configuration
    const sampleJobConfig = {
      $schema: 'https://azuremlschemas.azureedge.net/latest/commandJob.schema.json',
      command: 'python sample-test-script.py',
      environment: 'azureml:AzureML-sklearn-1.0-ubuntu20.04-py38-cpu:1',
      compute: 'cpu-cluster',
      experiment_name: 'automation-test',
      description: 'Sample job for automation testing',
    };

    const jobConfigPath = path.join(testDataDir, 'sample-job.yml');
    fs.writeFileSync(jobConfigPath, JSON.stringify(sampleJobConfig, null, 2));

    logger.info('Test data files created successfully');
  } catch (error) {
    logger.error('Failed to create test data files', { error: error.message });
    throw error;
  }
}

export default globalSetup;