import { Page, expect } from '@playwright/test';
import { PlaywrightUtils } from './PlaywrightUtils';
import { ConfigManager } from './ConfigManager';
import { Logger } from './Logger';
import { ElectronUtils } from './ElectronUtils';

export class AzureMLUtils {
  private page: Page;
  private utils: PlaywrightUtils;
  private config: ConfigManager;
  private logger: Logger;
  private electronUtils: ElectronUtils;

  constructor(page: Page) {
    this.page = page;
    this.utils = new PlaywrightUtils(page);
    this.config = ConfigManager.getInstance();
    this.logger = Logger.getInstance();
    this.electronUtils = new ElectronUtils();
  }

  // Navigation Methods
  async navigateToWorkspace(workspaceName: string): Promise<void> {
    this.logger.logAction(`Navigate to Azure ML workspace: ${workspaceName}`);
    const baseUrl = 'https://ml.azure.com';
    const workspaceUrl = `${baseUrl}/workspaces/${workspaceName}`;
    
    await this.utils.navigateTo(workspaceUrl);
    await this.utils.waitForLoadState('networkidle');
    
    // Wait for the page to fully load
    await this.utils.sleep(3000);
  }

  async navigateToWorkspaceUrl(url: string): Promise<void> {
    this.logger.logAction(`Navigate to Azure ML workspace URL: ${url}`);
    await this.utils.navigateTo(url);
    await this.utils.waitForLoadState('networkidle');
  }

  async navigateToComputeSection(): Promise<void> {
    this.logger.logAction('Navigate to compute section');
    
    // Look for compute navigation link
    const computeSelectors = [
      'a[href*="compute"]',
      'button:has-text("Compute")',
      '[data-testid="compute-nav"]',
      'nav a:has-text("Compute")',
      '.nav-item:has-text("Compute")'
    ];

    let computeFound = false;
    for (const selector of computeSelectors) {
      try {
        await this.utils.waitForElement(selector, 5000);
        await this.utils.click(selector);
        computeFound = true;
        break;
      } catch (error) {
        this.logger.logWarning(`Compute selector not found: ${selector}`);
      }
    }

    if (!computeFound) {
      // Try to find compute in sidebar or menu
      const menuSelectors = [
        '[role="navigation"]',
        '.sidebar',
        '.nav-menu',
        '.navigation'
      ];

      for (const menuSelector of menuSelectors) {
        try {
          const computeInMenu = `${menuSelector} a:has-text("Compute"), ${menuSelector} button:has-text("Compute")`;
          await this.utils.waitForElement(computeInMenu, 3000);
          await this.utils.click(computeInMenu);
          computeFound = true;
          break;
        } catch (error) {
          continue;
        }
      }
    }

    if (!computeFound) {
      throw new Error('Could not find compute navigation option');
    }

    await this.utils.waitForLoadState('networkidle');
    await this.utils.sleep(2000);
  }

  // Authentication Methods
  async handleAuthenticationIfRequired(): Promise<void> {
    this.logger.logAction('Handle authentication if required');
    
    // Check if we're on a login page
    const loginIndicators = [
      'input[type="email"]',
      'input[name="loginfmt"]',
      'input[id="i0116"]',
      '.login-form',
      '[data-testid="login"]'
    ];

    let loginRequired = false;
    for (const selector of loginIndicators) {
      try {
        await this.utils.waitForElement(selector, 3000);
        loginRequired = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (loginRequired) {
      await this.performLogin();
    } else {
      this.logger.logInfo('No authentication required - already logged in');
    }
  }

  async handleLoginIfRequired(userName: string): Promise<void> {
    this.logger.logAction(`Handle login if required for user: ${userName}`);
    await this.handleAuthenticationIfRequired();
  }

  private async performLogin(): Promise<void> {
    this.logger.logAction('Performing Azure authentication');
    
    const authConfig = this.config.getAuthenticationSettings();
    
    // Enter email
    const emailSelectors = [
      'input[type="email"]',
      'input[name="loginfmt"]',
      'input[id="i0116"]'
    ];

    for (const selector of emailSelectors) {
      try {
        await this.utils.waitForElement(selector, 5000);
        await this.utils.fill(selector, authConfig.Username);
        await this.utils.pressKeyOnElement(selector, 'Enter');
        break;
      } catch (error) {
        continue;
      }
    }

    await this.utils.sleep(2000);

    // Enter password
    const passwordSelectors = [
      'input[type="password"]',
      'input[name="passwd"]',
      'input[id="i0118"]'
    ];

    for (const selector of passwordSelectors) {
      try {
        await this.utils.waitForElement(selector, 5000);
        await this.utils.fill(selector, authConfig.Password);
        await this.utils.pressKeyOnElement(selector, 'Enter');
        break;
      } catch (error) {
        continue;
      }
    }

    // Handle MFA if enabled
    if (authConfig.MFA?.Enabled) {
      await this.handleMFA();
    }

    // Wait for successful login
    await this.utils.waitForLoadState('networkidle');
    await this.utils.sleep(3000);
  }

  private async handleMFA(): Promise<void> {
    this.logger.logAction('Handling MFA authentication');
    
    const authConfig = this.config.getAuthenticationSettings();
    
    // Look for MFA input
    const mfaSelectors = [
      'input[name="otc"]',
      'input[id="idTxtBx_SAOTCC_OTC"]',
      'input[type="tel"]',
      '.form-control[placeholder*="code"]'
    ];

    let mfaFound = false;
    for (const selector of mfaSelectors) {
      try {
        await this.utils.waitForElement(selector, 10000);
        
        if (authConfig.MFA?.AutoSubmitOTP && authConfig.MFA?.TOTPSecretKey) {
          // Generate TOTP code (simplified - in real implementation you'd use a proper TOTP library)
          const otpCode = this.generateTOTPCode(authConfig.MFA.TOTPSecretKey);
          await this.utils.fill(selector, otpCode);
          await this.utils.pressKeyOnElement(selector, 'Enter');
        } else {
          this.logger.logWarning('MFA code required but auto-submit not configured');
          // Wait for manual input
          await this.utils.sleep(30000);
        }
        
        mfaFound = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (mfaFound) {
      await this.utils.waitForLoadState('networkidle');
      await this.utils.sleep(2000);
    }
  }

  private generateTOTPCode(secret: string): string {
    // Simplified TOTP generation - in real implementation use proper TOTP library
    // This is a placeholder that returns a 6-digit code
    return Math.floor(100000 + Math.random() * 900000).toString();
  }

  // Workspace Verification Methods
  async verifyWorkspaceAccess(workspaceName: string): Promise<void> {
    this.logger.logAction(`Verify workspace access: ${workspaceName}`);
    
    // Check for workspace indicators
    const workspaceIndicators = [
      `h1:has-text("${workspaceName}")`,
      `[title*="${workspaceName}"]`,
      `.workspace-name:has-text("${workspaceName}")`,
      `[data-testid="workspace-name"]:has-text("${workspaceName}")`
    ];

    let workspaceFound = false;
    for (const selector of workspaceIndicators) {
      try {
        await this.utils.waitForElement(selector, 10000);
        workspaceFound = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (!workspaceFound) {
      // Check page title or URL
      const currentUrl = await this.utils.getCurrentUrl();
      const pageTitle = await this.utils.getTitle();
      
      if (currentUrl.includes(workspaceName) || pageTitle.includes(workspaceName)) {
        workspaceFound = true;
      }
    }

    if (!workspaceFound) {
      throw new Error(`Could not verify access to workspace: ${workspaceName}`);
    }

    this.logger.logInfo(`✅ Successfully verified access to workspace: ${workspaceName}`);
  }

  async verifyWorkspaceAvailable(): Promise<void> {
    this.logger.logAction('Verify workspace is available');
    
    // Check for error indicators
    const errorIndicators = [
      '.error-message',
      '.alert-danger',
      '[role="alert"]',
      '.notification-error'
    ];

    for (const selector of errorIndicators) {
      try {
        const isVisible = await this.utils.isVisible(selector);
        if (isVisible) {
          const errorText = await this.utils.getText(selector);
          throw new Error(`Workspace error detected: ${errorText}`);
        }
      } catch (error) {
        // If element doesn't exist, that's good
        continue;
      }
    }

    // Check for positive indicators
    const availabilityIndicators = [
      '.workspace-dashboard',
      '.workspace-content',
      '[data-testid="workspace-main"]',
      '.main-content'
    ];

    let workspaceAvailable = false;
    for (const selector of availabilityIndicators) {
      try {
        await this.utils.waitForElement(selector, 5000);
        workspaceAvailable = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (!workspaceAvailable) {
      // Check if page has loaded successfully
      const currentUrl = await this.utils.getCurrentUrl();
      if (currentUrl.includes('ml.azure.com') && !currentUrl.includes('error')) {
        workspaceAvailable = true;
      }
    }

    if (!workspaceAvailable) {
      throw new Error('Workspace does not appear to be available');
    }

    this.logger.logInfo('✅ Workspace is available and accessible');
  }

  // Compute Instance Management Methods
  async startComputeInstance(computeName: string): Promise<void> {
    this.logger.logAction(`Start compute instance: ${computeName}`);
    
    // Find the compute instance in the list
    await this.findComputeInstance(computeName);
    
    // Look for start button
    const startSelectors = [
      `[data-compute-name="${computeName}"] button:has-text("Start")`,
      `tr:has-text("${computeName}") button:has-text("Start")`,
      `.compute-row:has-text("${computeName}") .start-button`,
      `[aria-label*="Start ${computeName}"]`
    ];

    let startButtonFound = false;
    for (const selector of startSelectors) {
      try {
        await this.utils.waitForElement(selector, 5000);
        await this.utils.click(selector);
        startButtonFound = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (!startButtonFound) {
      // Try generic approach - find compute row and then start button
      const computeRowSelectors = [
        `tr:has-text("${computeName}")`,
        `.compute-item:has-text("${computeName}")`,
        `[data-testid="compute-row"]:has-text("${computeName}")`
      ];

      for (const rowSelector of computeRowSelectors) {
        try {
          await this.utils.waitForElement(rowSelector, 5000);
          
          // Look for start button within this row
          const startInRow = `${rowSelector} button:has-text("Start"), ${rowSelector} [aria-label*="Start"]`;
          await this.utils.waitForElement(startInRow, 3000);
          await this.utils.click(startInRow);
          startButtonFound = true;
          break;
        } catch (error) {
          continue;
        }
      }
    }

    if (!startButtonFound) {
      throw new Error(`Could not find start button for compute instance: ${computeName}`);
    }

    // Wait for the start operation to begin
    await this.utils.sleep(3000);
    
    // Wait for compute to start (this can take several minutes)
    await this.waitForComputeStatus(computeName, 'Running', 300000); // 5 minutes timeout
    
    this.logger.logInfo(`✅ Successfully started compute instance: ${computeName}`);
  }

  async stopComputeInstance(computeName: string): Promise<void> {
    this.logger.logAction(`Stop compute instance: ${computeName}`);
    
    // Find the compute instance in the list
    await this.findComputeInstance(computeName);
    
    // Look for stop button
    const stopSelectors = [
      `[data-compute-name="${computeName}"] button:has-text("Stop")`,
      `tr:has-text("${computeName}") button:has-text("Stop")`,
      `.compute-row:has-text("${computeName}") .stop-button`,
      `[aria-label*="Stop ${computeName}"]`
    ];

    let stopButtonFound = false;
    for (const selector of stopSelectors) {
      try {
        await this.utils.waitForElement(selector, 5000);
        await this.utils.click(selector);
        stopButtonFound = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (!stopButtonFound) {
      // Try generic approach
      const computeRowSelectors = [
        `tr:has-text("${computeName}")`,
        `.compute-item:has-text("${computeName}")`,
        `[data-testid="compute-row"]:has-text("${computeName}")`
      ];

      for (const rowSelector of computeRowSelectors) {
        try {
          await this.utils.waitForElement(rowSelector, 5000);
          
          const stopInRow = `${rowSelector} button:has-text("Stop"), ${rowSelector} [aria-label*="Stop"]`;
          await this.utils.waitForElement(stopInRow, 3000);
          await this.utils.click(stopInRow);
          stopButtonFound = true;
          break;
        } catch (error) {
          continue;
        }
      }
    }

    if (!stopButtonFound) {
      throw new Error(`Could not find stop button for compute instance: ${computeName}`);
    }

    // Confirm stop if dialog appears
    try {
      await this.utils.waitForElement('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Stop")', 3000);
      await this.utils.click('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Stop")');
    } catch (error) {
      // No confirmation dialog
    }

    // Wait for the stop operation to complete
    await this.waitForComputeStatus(computeName, 'Stopped', 120000); // 2 minutes timeout
    
    this.logger.logInfo(`✅ Successfully stopped compute instance: ${computeName}`);
  }

  async ensureComputeInstanceRunning(computeName: string): Promise<void> {
    this.logger.logAction(`Ensure compute instance is running: ${computeName}`);
    
    const status = await this.getComputeInstanceStatus(computeName);
    
    if (status !== 'Running') {
      this.logger.logInfo(`Compute instance ${computeName} is ${status}, starting it...`);
      await this.startComputeInstance(computeName);
    } else {
      this.logger.logInfo(`Compute instance ${computeName} is already running`);
    }
  }

  async startComputeIfNotRunning(computeName: string): Promise<void> {
    this.logger.logAction(`Start compute if not running: ${computeName}`);
    await this.ensureComputeInstanceRunning(computeName);
  }

  async stopAllComputeInstances(): Promise<void> {
    this.logger.logAction('Stop all compute instances');
    
    // Get list of running compute instances
    const runningInstances = await this.getRunningComputeInstances();
    
    for (const computeName of runningInstances) {
      try {
        await this.stopComputeInstance(computeName);
      } catch (error) {
        this.logger.logWarning(`Failed to stop compute instance ${computeName}: ${error}`);
      }
    }
    
    this.logger.logInfo('✅ Stopped all compute instances');
  }

  // Compute Status and Verification Methods
  async verifyComputeInstanceStatus(computeName: string, expectedStatus: string): Promise<void> {
    this.logger.logAction(`Verify compute instance status: ${computeName} should be ${expectedStatus}`);
    
    const actualStatus = await this.getComputeInstanceStatus(computeName);
    
    if (actualStatus !== expectedStatus) {
      throw new Error(`Expected compute instance ${computeName} to be ${expectedStatus}, but it is ${actualStatus}`);
    }
    
    this.logger.logInfo(`✅ Compute instance ${computeName} is ${expectedStatus} as expected`);
  }

  async verifyComputeInstanceConnectivity(computeName: string): Promise<void> {
    this.logger.logAction(`Verify compute instance connectivity: ${computeName}`);
    
    // Look for connectivity indicators
    const connectivityIndicators = [
      `[data-compute-name="${computeName}"] .connectivity-status.connected`,
      `tr:has-text("${computeName}") .status-connected`,
      `[aria-label*="Connected ${computeName}"]`
    ];

    let connectivityVerified = false;
    for (const selector of connectivityIndicators) {
      try {
        await this.utils.waitForElement(selector, 10000);
        connectivityVerified = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (!connectivityVerified) {
      // Check if there are any connection options available
      const connectionOptions = [
        `[data-compute-name="${computeName}"] button:has-text("Connect")`,
        `tr:has-text("${computeName}") a:has-text("Jupyter")`,
        `tr:has-text("${computeName}") a:has-text("Terminal")`
      ];

      for (const selector of connectionOptions) {
        try {
          await this.utils.waitForElement(selector, 5000);
          connectivityVerified = true;
          break;
        } catch (error) {
          continue;
        }
      }
    }

    if (!connectivityVerified) {
      this.logger.logWarning(`Could not verify connectivity for compute instance: ${computeName}`);
    } else {
      this.logger.logInfo(`✅ Compute instance ${computeName} connectivity verified`);
    }
  }

  private async findComputeInstance(computeName: string): Promise<void> {
    this.logger.logAction(`Find compute instance: ${computeName}`);
    
    // Wait for compute list to load
    const computeListSelectors = [
      '.compute-list',
      '.compute-instances',
      '[data-testid="compute-list"]',
      'table tbody'
    ];

    let listFound = false;
    for (const selector of computeListSelectors) {
      try {
        await this.utils.waitForElement(selector, 10000);
        listFound = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (!listFound) {
      throw new Error('Could not find compute instances list');
    }

    // Look for the specific compute instance
    const computeSelectors = [
      `tr:has-text("${computeName}")`,
      `.compute-item:has-text("${computeName}")`,
      `[data-compute-name="${computeName}"]`,
      `[data-testid="compute-row"]:has-text("${computeName}")`
    ];

    let computeFound = false;
    for (const selector of computeSelectors) {
      try {
        await this.utils.waitForElement(selector, 5000);
        computeFound = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (!computeFound) {
      throw new Error(`Could not find compute instance: ${computeName}`);
    }
  }

  private async getComputeInstanceStatus(computeName: string): Promise<string> {
    await this.findComputeInstance(computeName);
    
    // Look for status indicators
    const statusSelectors = [
      `[data-compute-name="${computeName}"] .status`,
      `tr:has-text("${computeName}") .compute-status`,
      `tr:has-text("${computeName}") td:nth-child(3)`, // Assuming status is in 3rd column
      `.compute-item:has-text("${computeName}") .status`
    ];

    for (const selector of statusSelectors) {
      try {
        await this.utils.waitForElement(selector, 5000);
        const statusText = await this.utils.getText(selector);
        
        // Normalize status text
        const normalizedStatus = statusText.trim().toLowerCase();
        if (normalizedStatus.includes('running') || normalizedStatus.includes('succeeded')) {
          return 'Running';
        } else if (normalizedStatus.includes('stopped') || normalizedStatus.includes('deallocated')) {
          return 'Stopped';
        } else if (normalizedStatus.includes('starting')) {
          return 'Starting';
        } else if (normalizedStatus.includes('stopping')) {
          return 'Stopping';
        }
        
        return statusText.trim();
      } catch (error) {
        continue;
      }
    }

    throw new Error(`Could not determine status for compute instance: ${computeName}`);
  }

  private async waitForComputeStatus(computeName: string, expectedStatus: string, timeoutMs: number = 300000): Promise<void> {
    this.logger.logAction(`Wait for compute ${computeName} to reach status: ${expectedStatus}`);
    
    const startTime = Date.now();
    
    while (Date.now() - startTime < timeoutMs) {
      try {
        const currentStatus = await this.getComputeInstanceStatus(computeName);
        
        if (currentStatus === expectedStatus) {
          this.logger.logInfo(`✅ Compute instance ${computeName} reached status: ${expectedStatus}`);
          return;
        }
        
        this.logger.logInfo(`Compute instance ${computeName} status: ${currentStatus}, waiting for ${expectedStatus}...`);
        await this.utils.sleep(10000); // Wait 10 seconds before checking again
        
        // Refresh the page to get updated status
        await this.utils.refresh();
        await this.utils.sleep(3000);
        
      } catch (error) {
        this.logger.logWarning(`Error checking compute status: ${error}`);
        await this.utils.sleep(5000);
      }
    }
    
    throw new Error(`Timeout waiting for compute instance ${computeName} to reach status ${expectedStatus}`);
  }

  // Alias for waitForComputeStatus to match validation expectations
  async waitForComputeState(computeName: string, expectedState: string, timeoutMs: number = 300000): Promise<void> {
    return this.waitForComputeStatus(computeName, expectedState, timeoutMs);
  }

  private async getRunningComputeInstances(): Promise<string[]> {
    const runningInstances: string[] = [];
    
    // This is a simplified implementation
    // In a real scenario, you'd parse the compute instances table
    try {
      const computeRows = await this.page.locator('tr:has-text("Running"), tr:has-text("Succeeded")').all();
      
      for (const row of computeRows) {
        try {
          const computeName = await row.locator('td:first-child').textContent();
          if (computeName) {
            runningInstances.push(computeName.trim());
          }
        } catch (error) {
          continue;
        }
      }
    } catch (error) {
      this.logger.logWarning(`Could not get running compute instances: ${error}`);
    }
    
    return runningInstances;
  }

  // Workspace Selection and Navigation Methods
  async selectWorkspace(workspaceName: string): Promise<void> {
    this.logger.logAction(`Select workspace: ${workspaceName}`);
    
    // Look for workspace selector
    const workspaceSelectors = [
      `button:has-text("${workspaceName}")`,
      `a:has-text("${workspaceName}")`,
      `.workspace-item:has-text("${workspaceName}")`,
      `[data-testid="workspace-selector"] option:has-text("${workspaceName}")`
    ];

    let workspaceSelected = false;
    for (const selector of workspaceSelectors) {
      try {
        await this.utils.waitForElement(selector, 10000);
        await this.utils.click(selector);
        workspaceSelected = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (!workspaceSelected) {
      throw new Error(`Could not select workspace: ${workspaceName}`);
    }

    await this.utils.waitForLoadState('networkidle');
    await this.utils.sleep(3000);
    
    this.logger.logInfo(`✅ Selected workspace: ${workspaceName}`);
  }

  async chooseComputeOption(): Promise<void> {
    this.logger.logAction('Choose compute option');
    
    const computeOptionSelectors = [
      'a:has-text("Compute")',
      'button:has-text("Compute")',
      '[href*="compute"]',
      '.nav-item:has-text("Compute")',
      '[data-testid="compute-option"]'
    ];

    let computeOptionFound = false;
    for (const selector of computeOptionSelectors) {
      try {
        await this.utils.waitForElement(selector, 10000);
        await this.utils.click(selector);
        computeOptionFound = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (!computeOptionFound) {
      throw new Error('Could not find compute option');
    }

    await this.utils.waitForLoadState('networkidle');
    await this.utils.sleep(2000);
    
    this.logger.logInfo('✅ Selected compute option');
  }

  async openCompute(computeName: string): Promise<void> {
    this.logger.logAction(`Open compute: ${computeName}`);
    
    // Find and click on the compute instance
    await this.findComputeInstance(computeName);
    
    const openSelectors = [
      `[data-compute-name="${computeName}"] a`,
      `tr:has-text("${computeName}") a:first-child`,
      `.compute-item:has-text("${computeName}") .compute-name`
    ];

    let computeOpened = false;
    for (const selector of openSelectors) {
      try {
        await this.utils.waitForElement(selector, 5000);
        await this.utils.click(selector);
        computeOpened = true;
        break;
      } catch (error) {
        continue;
      }
    }

    if (!computeOpened) {
      // Try clicking on the compute name directly
      const computeNameSelectors = [
        `tr:has-text("${computeName}") td:first-child`,
        `.compute-row:has-text("${computeName}") .name`
      ];

      for (const selector of computeNameSelectors) {
        try {
          await this.utils.waitForElement(selector, 5000);
          await this.utils.click(selector);
          computeOpened = true;
          break;
        } catch (error) {
          continue;
        }
      }
    }

    if (!computeOpened) {
      throw new Error(`Could not open compute instance: ${computeName}`);
    }

    await this.utils.waitForLoadState('networkidle');
    await this.utils.sleep(3000);
    
    this.logger.logInfo(`✅ Opened compute: ${computeName}`);
  }

  // Application Links and VS Code Integration Methods
  async checkApplicationLinksEnabled(): Promise<boolean> {
    this.logger.logAction('Check if application links are enabled');
    
    const applicationLinkSelectors = [
      '.application-links',
      '.app-links',
      'a:has-text("VS Code")',
      'a:has-text("Jupyter")',
      'a:has-text("Terminal")',
      '[data-testid="application-links"]'
    ];

    for (const selector of applicationLinkSelectors) {
      try {
        await this.utils.waitForElement(selector, 5000);
        const isVisible = await this.utils.isVisible(selector);
        if (isVisible) {
          this.logger.logInfo('✅ Application links are enabled');
          return true;
        }
      } catch (error) {
        continue;
      }
    }

    this.logger.logWarning('Application links do not appear to be enabled');
    return false;
  }

  async startVSCodeDesktop(): Promise<void> {
    this.logger.logAction('Start VS Code Desktop');
    
    // Look for VS Code Desktop link
    const vsCodeSelectors = [
      'a:has-text("VS Code Desktop")',
      'button:has-text("VS Code Desktop")',
      'a:has-text("VS Code")',
      '[data-testid="vscode-desktop"]',
      '.app-link:has-text("VS Code")'
    ];

    let vsCodeLinkFound = false;
    for (const selector of vsCodeSelectors) {
      try {
        await this.utils.waitForElement(selector, 10000);
        
        // Get the href if it's a link
        const href = await this.utils.getAttribute(selector, 'href');
        
        if (href && href.startsWith('vscode://')) {
          // This is a VS Code protocol link
          await this.utils.click(selector);
          vsCodeLinkFound = true;
          break;
        } else {
          // Regular link or button
          await this.utils.click(selector);
          vsCodeLinkFound = true;
          break;
        }
      } catch (error) {
        continue;
      }
    }

    if (!vsCodeLinkFound) {
      throw new Error('Could not find VS Code Desktop link');
    }

    // Wait for VS Code to launch
    await this.utils.sleep(5000);
    
    this.logger.logInfo('✅ VS Code Desktop launch initiated');
  }

  async verifyVSCodeInteraction(): Promise<boolean> {
    this.logger.logAction('Verify VS Code Desktop interaction');
    
    try {
      // Try to interact with VS Code using Electron utils
      const vsCodeRunning = await this.electronUtils.isVSCodeRunning();
      
      if (vsCodeRunning) {
        // Try to perform a simple interaction
        await this.electronUtils.openVSCodeFile('test.txt');
        await this.utils.sleep(2000);
        
        this.logger.logInfo('✅ VS Code Desktop is interactive');
        return true;
      } else {
        this.logger.logWarning('VS Code Desktop does not appear to be running');
        return false;
      }
    } catch (error) {
      this.logger.logWarning(`VS Code interaction failed: ${error}`);
      return false;
    }
  }
}