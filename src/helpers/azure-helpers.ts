import { ComputeManagementClient } from '@azure/arm-compute';
import { MachineLearningWorkspacesManagementClient } from '@azure/arm-machinelearning';
import { ResourceManagementClient } from '@azure/arm-resources';
import { BlobServiceClient } from '@azure/storage-blob';
import axios, { AxiosResponse } from 'axios';
import { authManager } from './auth';
import { logger, TestLogger } from './logger';
import { config } from './config';
import { cliRunner, CliResult } from './cli-runner';

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

export class AzureMLHelper {
  private mlClient: MachineLearningWorkspacesManagementClient;
  private computeClient: ComputeManagementClient;
  private resourceClient: ResourceManagementClient;
  private blobClient?: BlobServiceClient;

  constructor(private testLogger?: TestLogger) {
    const credential = authManager.getCredential();
    
    this.mlClient = new MachineLearningWorkspacesManagementClient(
      credential,
      config.azure.subscriptionId
    );
    
    this.computeClient = new ComputeManagementClient(
      credential,
      config.azure.subscriptionId
    );
    
    this.resourceClient = new ResourceManagementClient(
      credential,
      config.azure.subscriptionId
    );

    if (config.artifacts.storageAccount) {
      this.blobClient = new BlobServiceClient(
        `https://${config.artifacts.storageAccount}.blob.core.windows.net`,
        credential
      );
    }
  }

  // Compute Instance Management
  async startComputeInstance(instanceName: string): Promise<void> {
    try {
      this.testLogger?.action(`Starting compute instance: ${instanceName}`);
      
      const result = await cliRunner.azMlComputeStart(
        instanceName,
        config.azure.workspaceName,
        config.azure.resourceGroup,
        { testLogger: this.testLogger }
      );

      if (result.exitCode !== 0) {
        throw new Error(`Failed to start compute instance: ${result.stderr}`);
      }

      this.testLogger?.info(`Compute instance ${instanceName} start command completed`);
    } catch (error) {
      this.testLogger?.error(`Failed to start compute instance ${instanceName}`, { error });
      throw error;
    }
  }

  async stopComputeInstance(instanceName: string): Promise<void> {
    try {
      this.testLogger?.action(`Stopping compute instance: ${instanceName}`);
      
      const result = await cliRunner.azMlComputeStop(
        instanceName,
        config.azure.workspaceName,
        config.azure.resourceGroup,
        { testLogger: this.testLogger }
      );

      if (result.exitCode !== 0) {
        throw new Error(`Failed to stop compute instance: ${result.stderr}`);
      }

      this.testLogger?.info(`Compute instance ${instanceName} stop command completed`);
    } catch (error) {
      this.testLogger?.error(`Failed to stop compute instance ${instanceName}`, { error });
      throw error;
    }
  }

  async getComputeInstanceStatus(instanceName: string): Promise<ComputeInstance> {
    try {
      this.testLogger?.action(`Getting compute instance status: ${instanceName}`);
      
      const result = await cliRunner.azMlComputeShow(
        instanceName,
        config.azure.workspaceName,
        config.azure.resourceGroup,
        { testLogger: this.testLogger }
      );

      if (result.exitCode !== 0) {
        throw new Error(`Failed to get compute instance status: ${result.stderr}`);
      }

      const computeData = cliRunner.parseJsonOutput(result);
      
      const computeInstance: ComputeInstance = {
        name: computeData.name,
        state: computeData.properties?.state || 'Unknown',
        vmSize: computeData.properties?.vmSize || 'Unknown',
        location: computeData.location,
        createdBy: computeData.properties?.createdBy?.userObjectId || 'Unknown',
        createdOn: computeData.properties?.createdOn || 'Unknown',
        lastModifiedBy: computeData.properties?.lastModifiedBy?.userObjectId || 'Unknown',
        lastModifiedOn: computeData.properties?.lastModifiedOn || 'Unknown',
      };

      this.testLogger?.info(`Compute instance ${instanceName} status retrieved`, {
        state: computeInstance.state,
        vmSize: computeInstance.vmSize,
      });

      return computeInstance;
    } catch (error) {
      this.testLogger?.error(`Failed to get compute instance status for ${instanceName}`, { error });
      throw error;
    }
  }

  async waitForComputeInstanceState(
    instanceName: string,
    targetState: string,
    timeoutMs: number = 600000 // 10 minutes
  ): Promise<ComputeInstance> {
    const startTime = Date.now();
    const pollInterval = 30000; // 30 seconds

    this.testLogger?.info(`Waiting for compute instance ${instanceName} to reach state: ${targetState}`, {
      timeout: timeoutMs,
      pollInterval,
    });

    while (Date.now() - startTime < timeoutMs) {
      const instance = await this.getComputeInstanceStatus(instanceName);
      
      if (instance.state.toLowerCase() === targetState.toLowerCase()) {
        this.testLogger?.info(`Compute instance ${instanceName} reached target state: ${targetState}`, {
          duration: Date.now() - startTime,
        });
        return instance;
      }

      this.testLogger?.debug(`Compute instance ${instanceName} current state: ${instance.state}, waiting...`);
      await this.sleep(pollInterval);
    }

    throw new Error(
      `Timeout waiting for compute instance ${instanceName} to reach state ${targetState} after ${timeoutMs}ms`
    );
  }

  // Job Management
  async createJob(jobDefinition: any): Promise<MLJob> {
    try {
      this.testLogger?.action('Creating ML job', { jobDefinition });
      
      // Create temporary job file
      const jobFile = `/tmp/job-${Date.now()}.yml`;
      const fs = require('fs');
      fs.writeFileSync(jobFile, JSON.stringify(jobDefinition, null, 2));

      const result = await cliRunner.azMlJobCreate(
        jobFile,
        config.azure.workspaceName,
        config.azure.resourceGroup,
        { testLogger: this.testLogger }
      );

      // Clean up temp file
      fs.unlinkSync(jobFile);

      if (result.exitCode !== 0) {
        throw new Error(`Failed to create job: ${result.stderr}`);
      }

      const jobData = cliRunner.parseJsonOutput(result);
      
      const job: MLJob = {
        id: jobData.id,
        name: jobData.name,
        status: jobData.status,
        startTime: jobData.startTime,
        endTime: jobData.endTime,
        computeTarget: jobData.computeTarget,
      };

      this.testLogger?.info('ML job created successfully', { jobId: job.id, jobName: job.name });
      return job;
    } catch (error) {
      this.testLogger?.error('Failed to create ML job', { error });
      throw error;
    }
  }

  // PIM (Privileged Identity Management) Helper
  async requestPIMActivation(
    roleName: string,
    scope: string,
    justification: string = config.pim.justification
  ): Promise<void> {
    try {
      this.testLogger?.action('Requesting PIM role activation', { roleName, scope });
      
      const accessToken = await authManager.getAccessToken(['https://graph.microsoft.com/.default']);
      
      const response = await axios.post(
        'https://graph.microsoft.com/v1.0/privilegedAccess/azureResources/roleAssignmentRequests',
        {
          roleDefinitionId: roleName,
          resourceId: scope,
          subjectId: 'me',
          assignmentState: 'Active',
          type: 'UserAdd',
          reason: justification,
          schedule: {
            type: 'Once',
            startDateTime: new Date().toISOString(),
            duration: 'PT8H', // 8 hours
          },
        },
        {
          headers: {
            Authorization: `Bearer ${accessToken}`,
            'Content-Type': 'application/json',
          },
        }
      );

      this.testLogger?.info('PIM activation request submitted', {
        roleName,
        scope,
        requestId: response.data.id,
      });
    } catch (error) {
      this.testLogger?.error('Failed to request PIM activation', { error, roleName, scope });
      throw error;
    }
  }

  async checkPIMActivationStatus(requestId: string): Promise<string> {
    try {
      const accessToken = await authManager.getAccessToken(['https://graph.microsoft.com/.default']);
      
      const response = await axios.get(
        `https://graph.microsoft.com/v1.0/privilegedAccess/azureResources/roleAssignmentRequests/${requestId}`,
        {
          headers: {
            Authorization: `Bearer ${accessToken}`,
          },
        }
      );

      const status = response.data.status;
      this.testLogger?.info('PIM activation status checked', { requestId, status });
      return status;
    } catch (error) {
      this.testLogger?.error('Failed to check PIM activation status', { error, requestId });
      throw error;
    }
  }

  // Resource Management
  async createResourceGroup(resourceGroupName: string, location: string = 'eastus'): Promise<void> {
    try {
      this.testLogger?.action(`Creating resource group: ${resourceGroupName}`);
      
      await this.resourceClient.resourceGroups.createOrUpdate(resourceGroupName, {
        location,
        tags: {
          purpose: 'automation-testing',
          createdBy: 'azure-ml-automation-framework',
        },
      });

      this.testLogger?.info(`Resource group ${resourceGroupName} created successfully`);
    } catch (error) {
      this.testLogger?.error(`Failed to create resource group ${resourceGroupName}`, { error });
      throw error;
    }
  }

  async deleteResourceGroup(resourceGroupName: string): Promise<void> {
    try {
      this.testLogger?.action(`Deleting resource group: ${resourceGroupName}`);
      
      await this.resourceClient.resourceGroups.beginDeleteAndWait(resourceGroupName);

      this.testLogger?.info(`Resource group ${resourceGroupName} deleted successfully`);
    } catch (error) {
      this.testLogger?.error(`Failed to delete resource group ${resourceGroupName}`, { error });
      throw error;
    }
  }

  // Artifact Management
  async uploadArtifact(filePath: string, containerName: string = config.artifacts.container): Promise<string> {
    if (!this.blobClient) {
      throw new Error('Blob client not initialized. Storage account not configured.');
    }

    try {
      this.testLogger?.action(`Uploading artifact: ${filePath}`);
      
      const containerClient = this.blobClient.getContainerClient(containerName);
      await containerClient.createIfNotExists();

      const blobName = `${Date.now()}-${require('path').basename(filePath)}`;
      const blockBlobClient = containerClient.getBlockBlobClient(blobName);

      const fs = require('fs');
      const fileContent = fs.readFileSync(filePath);
      
      await blockBlobClient.upload(fileContent, fileContent.length);

      const artifactUrl = blockBlobClient.url;
      this.testLogger?.info('Artifact uploaded successfully', { filePath, artifactUrl });
      
      return artifactUrl;
    } catch (error) {
      this.testLogger?.error(`Failed to upload artifact: ${filePath}`, { error });
      throw error;
    }
  }

  // Utility methods
  private sleep(ms: number): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  async validateWorkspaceAccess(): Promise<boolean> {
    try {
      this.testLogger?.action('Validating workspace access');
      
      const workspace = await this.mlClient.workspaces.get(
        config.azure.resourceGroup,
        config.azure.workspaceName
      );

      this.testLogger?.info('Workspace access validated', {
        workspaceName: workspace.name,
        location: workspace.location,
      });
      
      return true;
    } catch (error) {
      this.testLogger?.error('Failed to validate workspace access', { error });
      return false;
    }
  }
}

// Factory function to create Azure ML helper
export function createAzureMLHelper(testLogger?: TestLogger): AzureMLHelper {
  return new AzureMLHelper(testLogger);
}