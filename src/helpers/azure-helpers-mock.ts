/**
 * Mock Azure ML Helper for Testing
 * This provides mock implementations of Azure ML operations for testing purposes
 */

import { logger, TestLogger } from './logger';
import { config } from './config';

export interface ComputeInstance {
  name: string;
  state: string;
  vmSize: string;
  location: string;
  createdBy: string;
  createdOn: string;
  lastModifiedBy: string;
  lastModifiedOn: string;
}

export interface ComputeCluster {
  name: string;
  state: string;
  vmSize: string;
  minNodes: number;
  maxNodes: number;
  currentNodes: number;
  location: string;
}

export interface MLJob {
  id: string;
  name: string;
  status: string;
  startTime: string;
  endTime?: string;
  computeTarget: string;
}

export class MockAzureMLHelper {
  constructor(private testLogger?: TestLogger) {
    this.log('Mock Azure ML Helper initialized');
  }

  private log(message: string, data?: any): void {
    if (this.testLogger) {
      this.testLogger.info(message, data);
    } else {
      logger.info(message, data);
    }
  }

  async validateWorkspaceAccess(): Promise<boolean> {
    this.log('Mock: Validating workspace access');
    // Simulate successful validation in test mode
    return true;
  }

  async getWorkspaceInfo(): Promise<any> {
    this.log('Mock: Getting workspace info');
    return {
      name: config.azure.workspaceName,
      resourceGroup: config.azure.resourceGroup,
      location: 'eastus',
      subscriptionId: config.azure.subscriptionId,
      status: 'Ready'
    };
  }

  async listComputeInstances(): Promise<ComputeInstance[]> {
    this.log('Mock: Listing compute instances');
    return [
      {
        name: 'test-compute-instance',
        state: 'Running',
        vmSize: 'Standard_DS3_v2',
        location: 'eastus',
        createdBy: 'test-user',
        createdOn: new Date().toISOString(),
        lastModifiedBy: 'test-user',
        lastModifiedOn: new Date().toISOString()
      }
    ];
  }

  async listComputeClusters(): Promise<ComputeCluster[]> {
    this.log('Mock: Listing compute clusters');
    return [
      {
        name: 'test-cpu-cluster',
        state: 'Running',
        vmSize: 'Standard_DS3_v2',
        minNodes: 0,
        maxNodes: 4,
        currentNodes: 2,
        location: 'eastus'
      }
    ];
  }

  async createComputeInstance(name: string, vmSize: string = 'Standard_DS3_v2'): Promise<ComputeInstance> {
    this.log('Mock: Creating compute instance', { name, vmSize });
    return {
      name,
      state: 'Creating',
      vmSize,
      location: 'eastus',
      createdBy: 'test-user',
      createdOn: new Date().toISOString(),
      lastModifiedBy: 'test-user',
      lastModifiedOn: new Date().toISOString()
    };
  }

  async deleteComputeInstance(name: string): Promise<void> {
    this.log('Mock: Deleting compute instance', { name });
    // Simulate successful deletion
  }

  async startComputeInstance(name: string): Promise<void> {
    this.log('Mock: Starting compute instance', { name });
    // Simulate successful start
  }

  async stopComputeInstance(name: string): Promise<void> {
    this.log('Mock: Stopping compute instance', { name });
    // Simulate successful stop
  }

  async submitJob(jobConfig: any): Promise<MLJob> {
    this.log('Mock: Submitting job', { jobConfig });
    const jobId = `test-job-${Date.now()}`;
    return {
      id: jobId,
      name: jobConfig.name || 'test-job',
      status: 'Running',
      startTime: new Date().toISOString(),
      computeTarget: jobConfig.computeTarget || 'test-cpu-cluster'
    };
  }

  async getJobStatus(jobId: string): Promise<MLJob> {
    this.log('Mock: Getting job status', { jobId });
    return {
      id: jobId,
      name: 'test-job',
      status: 'Completed',
      startTime: new Date(Date.now() - 300000).toISOString(), // 5 minutes ago
      endTime: new Date().toISOString(),
      computeTarget: 'test-cpu-cluster'
    };
  }

  async listJobs(): Promise<MLJob[]> {
    this.log('Mock: Listing jobs');
    return [
      {
        id: 'test-job-1',
        name: 'sample-training-job',
        status: 'Completed',
        startTime: new Date(Date.now() - 600000).toISOString(),
        endTime: new Date(Date.now() - 300000).toISOString(),
        computeTarget: 'test-cpu-cluster'
      },
      {
        id: 'test-job-2',
        name: 'data-preprocessing-job',
        status: 'Running',
        startTime: new Date(Date.now() - 180000).toISOString(),
        computeTarget: 'test-compute-instance'
      }
    ];
  }

  async uploadFile(localPath: string, remotePath: string): Promise<string> {
    this.log('Mock: Uploading file', { localPath, remotePath });
    return `https://mock-storage.blob.core.windows.net/uploads/${remotePath}`;
  }

  async downloadFile(remotePath: string, localPath: string): Promise<void> {
    this.log('Mock: Downloading file', { remotePath, localPath });
    // Simulate successful download
  }

  async createDataset(name: string, dataPath: string): Promise<any> {
    this.log('Mock: Creating dataset', { name, dataPath });
    return {
      id: `dataset-${Date.now()}`,
      name,
      dataPath,
      version: 1,
      createdTime: new Date().toISOString()
    };
  }

  async listDatasets(): Promise<any[]> {
    this.log('Mock: Listing datasets');
    return [
      {
        id: 'dataset-1',
        name: 'sample-dataset',
        dataPath: '/data/sample.csv',
        version: 1,
        createdTime: new Date().toISOString()
      }
    ];
  }

  async createEnvironment(name: string, dockerImage: string, condaFile?: string): Promise<any> {
    this.log('Mock: Creating environment', { name, dockerImage, condaFile });
    return {
      id: `env-${Date.now()}`,
      name,
      dockerImage,
      condaFile,
      version: 1,
      createdTime: new Date().toISOString()
    };
  }

  async listEnvironments(): Promise<any[]> {
    this.log('Mock: Listing environments');
    return [
      {
        id: 'env-1',
        name: 'python-sklearn',
        dockerImage: 'mcr.microsoft.com/azureml/sklearn:latest',
        version: 1,
        createdTime: new Date().toISOString()
      }
    ];
  }

  async getComputeInstanceConnectionInfo(name: string): Promise<any> {
    this.log('Mock: Getting compute instance connection info', { name });
    return {
      name,
      connectionType: 'ssh',
      host: 'mock-compute-instance.eastus.cloudapp.azure.com',
      port: 22,
      username: 'azureuser',
      status: 'Ready'
    };
  }

  async executeRemoteCommand(computeName: string, command: string): Promise<any> {
    this.log('Mock: Executing remote command', { computeName, command });
    return {
      exitCode: 0,
      stdout: `Mock execution of: ${command}\nCommand completed successfully`,
      stderr: '',
      executionTime: 1500
    };
  }

  async cleanup(): Promise<void> {
    this.log('Mock: Cleaning up resources');
    // Simulate cleanup
  }
}

export function createMockAzureMLHelper(testLogger?: TestLogger): MockAzureMLHelper {
  return new MockAzureMLHelper(testLogger);
}

// Export the mock as the default helper in test mode
export const createAzureMLHelper = (testLogger?: TestLogger) => {
  if (process.env.NODE_ENV === 'test' || process.env.MOCK_AZURE_SERVICES === 'true') {
    return createMockAzureMLHelper(testLogger);
  }
  
  // In non-test mode, this would import and return the real helper
  throw new Error('Real Azure ML Helper not available in test mode');
};