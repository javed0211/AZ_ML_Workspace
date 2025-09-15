import { Page } from '@playwright/test';
import { BasePage } from './base-page';
import { TestLogger } from '../helpers/logger';

export class AzureMLStudioPage extends BasePage {
  // Selectors
  private readonly selectors = {
    // Navigation
    homeButton: '[data-testid="home-button"]',
    workspaceSelector: '[data-testid="workspace-selector"]',
    navigationMenu: '[data-testid="navigation-menu"]',
    
    // Workspace
    workspaceName: '[data-testid="workspace-name"]',
    workspaceOverview: '[data-testid="workspace-overview"]',
    
    // Compute
    computeTab: '[data-testid="compute-tab"]',
    computeInstances: '[data-testid="compute-instances"]',
    computeClusters: '[data-testid="compute-clusters"]',
    createComputeButton: '[data-testid="create-compute-button"]',
    
    // Notebooks
    notebooksTab: '[data-testid="notebooks-tab"]',
    notebooksList: '[data-testid="notebooks-list"]',
    createNotebookButton: '[data-testid="create-notebook-button"]',
    uploadNotebookButton: '[data-testid="upload-notebook-button"]',
    
    // Jobs
    jobsTab: '[data-testid="jobs-tab"]',
    jobsList: '[data-testid="jobs-list"]',
    createJobButton: '[data-testid="create-job-button"]',
    
    // Data
    dataTab: '[data-testid="data-tab"]',
    dataAssets: '[data-testid="data-assets"]',
    datastores: '[data-testid="datastores"]',
    
    // Models
    modelsTab: '[data-testid="models-tab"]',
    modelsList: '[data-testid="models-list"]',
    
    // Endpoints
    endpointsTab: '[data-testid="endpoints-tab"]',
    endpointsList: '[data-testid="endpoints-list"]',
    
    // Common elements
    loadingSpinner: '[data-testid="loading-spinner"]',
    errorMessage: '[data-testid="error-message"]',
    successMessage: '[data-testid="success-message"]',
    confirmDialog: '[data-testid="confirm-dialog"]',
    confirmButton: '[data-testid="confirm-button"]',
    cancelButton: '[data-testid="cancel-button"]',
  };

  constructor(page: Page, testLogger?: TestLogger) {
    super(page, testLogger);
  }

  async isPageLoaded(): Promise<boolean> {
    try {
      await this.waitForElement(this.selectors.workspaceName, 10000);
      return true;
    } catch {
      return false;
    }
  }

  getPageIdentifier(): string {
    return 'Azure ML Studio';
  }

  async waitForPageLoad(): Promise<void> {
    await super.waitForPageLoad();
    
    // Wait for workspace to load
    await this.waitForElement(this.selectors.workspaceName);
    
    // Wait for loading spinner to disappear
    try {
      await this.page.waitForSelector(this.selectors.loadingSpinner, { 
        state: 'detached', 
        timeout: 30000 
      });
    } catch {
      // Loading spinner might not appear for fast loads
    }
  }

  // Navigation methods
  async navigateToCompute(): Promise<void> {
    this.testLogger?.step('Navigating to Compute section');
    await this.clickElement(this.selectors.computeTab);
    await this.waitForElement(this.selectors.computeInstances);
  }

  async navigateToNotebooks(): Promise<void> {
    this.testLogger?.step('Navigating to Notebooks section');
    await this.clickElement(this.selectors.notebooksTab);
    await this.waitForElement(this.selectors.notebooksList);
  }

  async navigateToJobs(): Promise<void> {
    this.testLogger?.step('Navigating to Jobs section');
    await this.clickElement(this.selectors.jobsTab);
    await this.waitForElement(this.selectors.jobsList);
  }

  async navigateToData(): Promise<void> {
    this.testLogger?.step('Navigating to Data section');
    await this.clickElement(this.selectors.dataTab);
    await this.waitForElement(this.selectors.dataAssets);
  }

  async navigateToModels(): Promise<void> {
    this.testLogger?.step('Navigating to Models section');
    await this.clickElement(this.selectors.modelsTab);
    await this.waitForElement(this.selectors.modelsList);
  }

  async navigateToEndpoints(): Promise<void> {
    this.testLogger?.step('Navigating to Endpoints section');
    await this.clickElement(this.selectors.endpointsTab);
    await this.waitForElement(this.selectors.endpointsList);
  }

  // Workspace methods
  async getWorkspaceName(): Promise<string> {
    return await this.getElementText(this.selectors.workspaceName);
  }

  async selectWorkspace(workspaceName: string): Promise<void> {
    this.testLogger?.action(`Selecting workspace: ${workspaceName}`);
    
    await this.clickElement(this.selectors.workspaceSelector);
    await this.waitForText(workspaceName);
    await this.clickElement(`text=${workspaceName}`);
    
    // Wait for workspace to load
    await this.waitForPageLoad();
  }

  // Compute methods
  async getComputeInstances(): Promise<string[]> {
    await this.navigateToCompute();
    
    const instances = await this.page.locator(`${this.selectors.computeInstances} [data-testid="compute-instance-name"]`).all();
    const names = [];
    
    for (const instance of instances) {
      const name = await instance.textContent();
      if (name) names.push(name.trim());
    }
    
    return names;
  }

  async createComputeInstance(name: string, vmSize: string = 'Standard_DS3_v2'): Promise<void> {
    this.testLogger?.action(`Creating compute instance: ${name}`);
    
    await this.navigateToCompute();
    await this.clickElement(this.selectors.createComputeButton);
    
    // Fill compute instance form
    await this.fillInput('[data-testid="compute-name-input"]', name);
    await this.selectOption('[data-testid="vm-size-select"]', vmSize);
    
    // Submit form
    await this.clickElement('[data-testid="create-button"]');
    
    // Wait for creation to complete
    await this.waitForText(`${name}`);
    this.testLogger?.info(`Compute instance created: ${name}`);
  }

  async startComputeInstance(name: string): Promise<void> {
    this.testLogger?.action(`Starting compute instance: ${name}`);
    
    await this.navigateToCompute();
    
    // Find the compute instance row and click start button
    const instanceRow = this.page.locator(`[data-testid="compute-instance-row"]:has-text("${name}")`);
    await instanceRow.locator('[data-testid="start-button"]').click();
    
    // Wait for status to change to "Running"
    await this.waitForComputeInstanceStatus(name, 'Running');
    this.testLogger?.info(`Compute instance started: ${name}`);
  }

  async stopComputeInstance(name: string): Promise<void> {
    this.testLogger?.action(`Stopping compute instance: ${name}`);
    
    await this.navigateToCompute();
    
    // Find the compute instance row and click stop button
    const instanceRow = this.page.locator(`[data-testid="compute-instance-row"]:has-text("${name}")`);
    await instanceRow.locator('[data-testid="stop-button"]').click();
    
    // Confirm stop action
    await this.clickElement(this.selectors.confirmButton);
    
    // Wait for status to change to "Stopped"
    await this.waitForComputeInstanceStatus(name, 'Stopped');
    this.testLogger?.info(`Compute instance stopped: ${name}`);
  }

  async getComputeInstanceStatus(name: string): Promise<string> {
    await this.navigateToCompute();
    
    const instanceRow = this.page.locator(`[data-testid="compute-instance-row"]:has-text("${name}")`);
    const statusElement = instanceRow.locator('[data-testid="compute-status"]');
    
    return await statusElement.textContent() || 'Unknown';
  }

  async waitForComputeInstanceStatus(name: string, expectedStatus: string, timeout: number = 600000): Promise<void> {
    this.testLogger?.info(`Waiting for compute instance ${name} to reach status: ${expectedStatus}`);
    
    const startTime = Date.now();
    
    while (Date.now() - startTime < timeout) {
      const currentStatus = await this.getComputeInstanceStatus(name);
      
      if (currentStatus.toLowerCase() === expectedStatus.toLowerCase()) {
        this.testLogger?.info(`Compute instance ${name} reached status: ${expectedStatus}`);
        return;
      }
      
      this.testLogger?.debug(`Compute instance ${name} current status: ${currentStatus}, waiting...`);
      await this.page.waitForTimeout(10000); // Wait 10 seconds between checks
    }
    
    throw new Error(`Timeout waiting for compute instance ${name} to reach status ${expectedStatus}`);
  }

  // Notebook methods
  async getNotebooks(): Promise<string[]> {
    await this.navigateToNotebooks();
    
    const notebooks = await this.page.locator(`${this.selectors.notebooksList} [data-testid="notebook-name"]`).all();
    const names = [];
    
    for (const notebook of notebooks) {
      const name = await notebook.textContent();
      if (name) names.push(name.trim());
    }
    
    return names;
  }

  async createNotebook(name: string, kernelType: string = 'Python 3.8'): Promise<void> {
    this.testLogger?.action(`Creating notebook: ${name}`);
    
    await this.navigateToNotebooks();
    await this.clickElement(this.selectors.createNotebookButton);
    
    // Fill notebook form
    await this.fillInput('[data-testid="notebook-name-input"]', name);
    await this.selectOption('[data-testid="kernel-select"]', kernelType);
    
    // Submit form
    await this.clickElement('[data-testid="create-button"]');
    
    // Wait for notebook to be created and opened
    await this.waitForUrl(/.*notebooks.*/, 30000);
    this.testLogger?.info(`Notebook created: ${name}`);
  }

  async openNotebook(name: string): Promise<void> {
    this.testLogger?.action(`Opening notebook: ${name}`);
    
    await this.navigateToNotebooks();
    
    // Find and click the notebook
    const notebookRow = this.page.locator(`[data-testid="notebook-row"]:has-text("${name}")`);
    await notebookRow.click();
    
    // Wait for notebook to open
    await this.waitForUrl(/.*notebooks.*/, 30000);
    this.testLogger?.info(`Notebook opened: ${name}`);
  }

  async uploadNotebook(filePath: string): Promise<void> {
    this.testLogger?.action(`Uploading notebook: ${filePath}`);
    
    await this.navigateToNotebooks();
    await this.clickElement(this.selectors.uploadNotebookButton);
    
    // Upload file
    await this.uploadFile('[data-testid="file-upload-input"]', filePath);
    
    // Submit upload
    await this.clickElement('[data-testid="upload-button"]');
    
    // Wait for upload to complete
    await this.waitForElement(this.selectors.successMessage);
    this.testLogger?.info(`Notebook uploaded: ${filePath}`);
  }

  // Job methods
  async getJobs(): Promise<string[]> {
    await this.navigateToJobs();
    
    const jobs = await this.page.locator(`${this.selectors.jobsList} [data-testid="job-name"]`).all();
    const names = [];
    
    for (const job of jobs) {
      const name = await job.textContent();
      if (name) names.push(name.trim());
    }
    
    return names;
  }

  async createJob(jobConfig: any): Promise<void> {
    this.testLogger?.action('Creating new job');
    
    await this.navigateToJobs();
    await this.clickElement(this.selectors.createJobButton);
    
    // This would depend on the specific job creation UI
    // Implementation would vary based on job type and UI structure
    
    this.testLogger?.info('Job creation initiated');
  }

  async getJobStatus(jobName: string): Promise<string> {
    await this.navigateToJobs();
    
    const jobRow = this.page.locator(`[data-testid="job-row"]:has-text("${jobName}")`);
    const statusElement = jobRow.locator('[data-testid="job-status"]');
    
    return await statusElement.textContent() || 'Unknown';
  }

  // Assertion methods
  async assertWorkspaceLoaded(workspaceName: string): Promise<void> {
    const actualName = await this.getWorkspaceName();
    if (actualName !== workspaceName) {
      throw new Error(`Expected workspace "${workspaceName}" but got "${actualName}"`);
    }
    
    this.testLogger?.assertion(`Workspace loaded: ${workspaceName}`, true);
  }

  async assertComputeInstanceExists(name: string): Promise<void> {
    const instances = await this.getComputeInstances();
    if (!instances.includes(name)) {
      throw new Error(`Compute instance "${name}" not found. Available instances: ${instances.join(', ')}`);
    }
    
    this.testLogger?.assertion(`Compute instance exists: ${name}`, true);
  }

  async assertComputeInstanceStatus(name: string, expectedStatus: string): Promise<void> {
    const actualStatus = await this.getComputeInstanceStatus(name);
    if (actualStatus.toLowerCase() !== expectedStatus.toLowerCase()) {
      throw new Error(`Expected compute instance "${name}" status "${expectedStatus}" but got "${actualStatus}"`);
    }
    
    this.testLogger?.assertion(`Compute instance ${name} status is ${expectedStatus}`, true);
  }

  async assertNotebookExists(name: string): Promise<void> {
    const notebooks = await this.getNotebooks();
    if (!notebooks.includes(name)) {
      throw new Error(`Notebook "${name}" not found. Available notebooks: ${notebooks.join(', ')}`);
    }
    
    this.testLogger?.assertion(`Notebook exists: ${name}`, true);
  }

  async assertJobExists(name: string): Promise<void> {
    const jobs = await this.getJobs();
    if (!jobs.includes(name)) {
      throw new Error(`Job "${name}" not found. Available jobs: ${jobs.join(', ')}`);
    }
    
    this.testLogger?.assertion(`Job exists: ${name}`, true);
  }
}