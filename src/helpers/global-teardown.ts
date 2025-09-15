import { FullConfig } from '@playwright/test';
import { logger } from './logger';
import { config } from './config';
import { cliRunner } from './cli-runner';
import { createAzureMLHelper } from './azure-helpers';
import * as fs from 'fs';
import * as path from 'path';

async function globalTeardown(config: FullConfig) {
  logger.info('Starting global teardown for Azure ML Workspace automation tests');

  try {
    // Cleanup Azure resources
    await cleanupAzureResources();

    // Cleanup CLI processes
    await cleanupCLIProcesses();

    // Cleanup temporary files
    await cleanupTempFiles();

    // Upload artifacts if configured
    await uploadArtifacts();

    // Generate final reports
    await generateReports();

    logger.info('Global teardown completed successfully');
  } catch (error) {
    logger.error('Global teardown failed', { error: error.message, stack: error.stack });
    // Don't throw error in teardown to avoid masking test failures
  }
}

async function cleanupAzureResources(): Promise<void> {
  logger.info('Cleaning up Azure resources');

  try {
    const azureHelper = createAzureMLHelper();

    // Stop any running compute instances that were started during tests
    await cleanupComputeInstances(azureHelper);

    // Cancel any running jobs
    await cleanupRunningJobs(azureHelper);

    // Cleanup test resource groups if they were created
    await cleanupTestResourceGroups(azureHelper);

    logger.info('Azure resources cleanup completed');
  } catch (error) {
    logger.error('Azure resources cleanup failed', { error: error.message });
  }
}

async function cleanupComputeInstances(azureHelper: any): Promise<void> {
  logger.info('Cleaning up compute instances');

  try {
    // Get list of compute instances that might have been started during tests
    const testComputeInstances = [
      config.compute.instanceName,
      'test-compute-instance',
      'automation-test-compute',
    ].filter(Boolean);

    for (const instanceName of testComputeInstances) {
      try {
        const instance = await azureHelper.getComputeInstanceStatus(instanceName);
        
        if (instance.state.toLowerCase() === 'running') {
          logger.info(`Stopping compute instance: ${instanceName}`);
          await azureHelper.stopComputeInstance(instanceName);
          
          // Wait for instance to stop (with timeout)
          try {
            await azureHelper.waitForComputeInstanceState(instanceName, 'stopped', 300000); // 5 minutes
            logger.info(`Compute instance stopped: ${instanceName}`);
          } catch (timeoutError) {
            logger.warn(`Timeout waiting for compute instance to stop: ${instanceName}`);
          }
        }
      } catch (error) {
        // Instance might not exist, which is fine
        logger.debug(`Compute instance not found or already stopped: ${instanceName}`);
      }
    }
  } catch (error) {
    logger.error('Failed to cleanup compute instances', { error: error.message });
  }
}

async function cleanupRunningJobs(azureHelper: any): Promise<void> {
  logger.info('Cleaning up running jobs');

  try {
    // This would require implementing job listing and cancellation
    // For now, we'll just log that we're checking
    logger.info('Checking for running jobs to cancel...');
    
    // Implementation would depend on Azure ML SDK capabilities
    // Example:
    // const runningJobs = await azureHelper.getRunningJobs();
    // for (const job of runningJobs) {
    //   if (job.name.includes('test') || job.name.includes('automation')) {
    //     await azureHelper.cancelJob(job.id);
    //   }
    // }
    
    logger.info('Job cleanup check completed');
  } catch (error) {
    logger.error('Failed to cleanup running jobs', { error: error.message });
  }
}

async function cleanupTestResourceGroups(azureHelper: any): Promise<void> {
  logger.info('Cleaning up test resource groups');

  try {
    // Only cleanup resource groups that were explicitly created for testing
    const testResourceGroups = [
      'rg-automation-test',
      'rg-temp-test',
    ];

    for (const rgName of testResourceGroups) {
      try {
        logger.info(`Checking if test resource group exists: ${rgName}`);
        // Only delete if it was created during this test run
        // This would require tracking which RGs were created
        // For safety, we'll skip automatic deletion of RGs
        logger.info(`Skipping deletion of resource group: ${rgName} (safety measure)`);
      } catch (error) {
        logger.debug(`Resource group not found: ${rgName}`);
      }
    }
  } catch (error) {
    logger.error('Failed to cleanup test resource groups', { error: error.message });
  }
}

async function cleanupCLIProcesses(): Promise<void> {
  logger.info('Cleaning up CLI processes');

  try {
    await cliRunner.cleanup();
    logger.info('CLI processes cleanup completed');
  } catch (error) {
    logger.error('Failed to cleanup CLI processes', { error: error.message });
  }
}

async function cleanupTempFiles(): Promise<void> {
  logger.info('Cleaning up temporary files');

  try {
    const tempDir = path.join(config.artifacts.path, 'temp');
    
    if (fs.existsSync(tempDir)) {
      // Clean up temporary files older than 1 hour
      const files = fs.readdirSync(tempDir);
      const oneHourAgo = Date.now() - (60 * 60 * 1000);
      
      for (const file of files) {
        const filePath = path.join(tempDir, file);
        const stats = fs.statSync(filePath);
        
        if (stats.mtime.getTime() < oneHourAgo) {
          fs.unlinkSync(filePath);
          logger.debug(`Deleted temporary file: ${filePath}`);
        }
      }
    }

    // Clean up VS Code test user data directories
    await cleanupVSCodeTempDirs();

    logger.info('Temporary files cleanup completed');
  } catch (error) {
    logger.error('Failed to cleanup temporary files', { error: error.message });
  }
}

async function cleanupVSCodeTempDirs(): Promise<void> {
  try {
    const tempDir = require('os').tmpdir();
    const files = fs.readdirSync(tempDir);
    
    for (const file of files) {
      if (file.startsWith('vscode-test-')) {
        const dirPath = path.join(tempDir, file);
        try {
          fs.rmSync(dirPath, { recursive: true, force: true });
          logger.debug(`Deleted VS Code temp directory: ${dirPath}`);
        } catch (error) {
          logger.warn(`Failed to delete VS Code temp directory: ${dirPath}`, { error: error.message });
        }
      }
    }
  } catch (error) {
    logger.warn('Failed to cleanup VS Code temp directories', { error: error.message });
  }
}

async function uploadArtifacts(): Promise<void> {
  if (!config.artifacts.upload || !config.artifacts.storageAccount) {
    logger.info('Artifact upload not configured, skipping');
    return;
  }

  logger.info('Uploading test artifacts');

  try {
    const azureHelper = createAzureMLHelper();
    const artifactsDir = config.artifacts.path;
    
    // Upload key artifacts
    const artifactFiles = [
      'logs/combined.log',
      'logs/error.log',
      'reports/html-report/index.html',
      'allure-results',
    ];

    for (const artifactPath of artifactFiles) {
      const fullPath = path.join(artifactsDir, artifactPath);
      
      if (fs.existsSync(fullPath)) {
        try {
          const uploadUrl = await azureHelper.uploadArtifact(fullPath);
          logger.info(`Artifact uploaded: ${artifactPath} -> ${uploadUrl}`);
        } catch (uploadError) {
          logger.error(`Failed to upload artifact: ${artifactPath}`, { error: uploadError.message });
        }
      }
    }

    logger.info('Artifact upload completed');
  } catch (error) {
    logger.error('Failed to upload artifacts', { error: error.message });
  }
}

async function generateReports(): Promise<void> {
  logger.info('Generating final reports');

  try {
    // Generate test summary
    await generateTestSummary();

    // Generate performance report
    await generatePerformanceReport();

    // Generate resource usage report
    await generateResourceUsageReport();

    logger.info('Report generation completed');
  } catch (error) {
    logger.error('Failed to generate reports', { error: error.message });
  }
}

async function generateTestSummary(): Promise<void> {
  try {
    const reportsDir = path.join(config.artifacts.path, 'reports');
    if (!fs.existsSync(reportsDir)) {
      fs.mkdirSync(reportsDir, { recursive: true });
    }

    const summary = {
      timestamp: new Date().toISOString(),
      environment: {
        nodeVersion: process.version,
        platform: process.platform,
        arch: process.arch,
      },
      configuration: {
        workspace: config.azure.workspaceName,
        resourceGroup: config.azure.resourceGroup,
        subscription: config.azure.subscriptionId,
      },
      artifacts: {
        logsPath: path.join(config.artifacts.path, 'logs'),
        screenshotsPath: path.join(config.artifacts.path, 'screenshots'),
        videosPath: path.join(config.artifacts.path, 'videos'),
        tracesPath: path.join(config.artifacts.path, 'traces'),
      },
    };

    const summaryPath = path.join(reportsDir, 'test-summary.json');
    fs.writeFileSync(summaryPath, JSON.stringify(summary, null, 2));

    logger.info(`Test summary generated: ${summaryPath}`);
  } catch (error) {
    logger.error('Failed to generate test summary', { error: error.message });
  }
}

async function generatePerformanceReport(): Promise<void> {
  try {
    // This would analyze performance logs and generate a report
    // For now, we'll create a placeholder
    const reportsDir = path.join(config.artifacts.path, 'reports');
    
    const performanceReport = {
      timestamp: new Date().toISOString(),
      metrics: {
        // These would be collected during test execution
        averagePageLoadTime: 'N/A',
        averageComputeStartTime: 'N/A',
        averageNotebookExecutionTime: 'N/A',
      },
      recommendations: [
        'Performance metrics collection not yet implemented',
        'Consider implementing performance monitoring in future versions',
      ],
    };

    const reportPath = path.join(reportsDir, 'performance-report.json');
    fs.writeFileSync(reportPath, JSON.stringify(performanceReport, null, 2));

    logger.info(`Performance report generated: ${reportPath}`);
  } catch (error) {
    logger.error('Failed to generate performance report', { error: error.message });
  }
}

async function generateResourceUsageReport(): Promise<void> {
  try {
    // This would analyze resource usage during tests
    const reportsDir = path.join(config.artifacts.path, 'reports');
    
    const resourceReport = {
      timestamp: new Date().toISOString(),
      computeInstances: {
        // This would track which instances were used
        used: [],
        created: [],
        deleted: [],
      },
      jobs: {
        // This would track jobs that were run
        submitted: [],
        completed: [],
        failed: [],
      },
      costs: {
        // This would estimate costs if possible
        estimated: 'N/A',
        currency: 'USD',
      },
    };

    const reportPath = path.join(reportsDir, 'resource-usage-report.json');
    fs.writeFileSync(reportPath, JSON.stringify(resourceReport, null, 2));

    logger.info(`Resource usage report generated: ${reportPath}`);
  } catch (error) {
    logger.error('Failed to generate resource usage report', { error: error.message });
  }
}

export default globalTeardown;