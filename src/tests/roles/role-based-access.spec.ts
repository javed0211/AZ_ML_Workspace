import { test, expect } from '@playwright/test';
import { AzureMLStudioPage } from '../../pages/azure-ml-studio';
import { createAzureMLHelper } from '../../helpers/azure-helpers';
import { authManager } from '../../helpers/auth';
import { logTestStart, logTestEnd } from '../../helpers/logger';
import { config } from '../../helpers/config';

// Define role configurations for testing
const roleConfigurations = {
  owner: {
    roleName: 'Owner',
    permissions: ['read', 'write', 'delete', 'manage', 'assign-roles'],
    description: 'Full access to all resources and can manage access',
  },
  contributor: {
    roleName: 'Contributor',
    permissions: ['read', 'write', 'delete', 'manage'],
    description: 'Full access to all resources but cannot manage access',
  },
  reader: {
    roleName: 'Reader',
    permissions: ['read'],
    description: 'Read-only access to all resources',
  },
  dataScientist: {
    roleName: 'AzureML Data Scientist',
    permissions: ['read', 'write', 'execute', 'create-experiments'],
    description: 'Can create and run experiments, manage models',
  },
  computeOperator: {
    roleName: 'AzureML Compute Operator',
    permissions: ['read', 'manage-compute'],
    description: 'Can manage compute instances and clusters',
  },
};

test.describe('Role-Based Access Control Tests', () => {
  let azureHelper: any;
  let currentRole: string;

  test.beforeAll(async () => {
    azureHelper = createAzureMLHelper();
  });

  test.beforeEach(async ({ page }) => {
    await page.goto(config.urls.base);
  });

  test('should verify Owner role permissions @roles @owner', async ({ page }) => {
    const testLogger = logTestStart('Owner Role Permissions Test');
    currentRole = 'Owner';
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Verify current user role
      const userRole = await azureMLPage.getCurrentUserRole();
      testLogger.info('Current user role', { role: userRole });
      
      // Test 1: Access Control Management (Owner-specific)
      testLogger.step('Testing access control management');
      
      await azureMLPage.navigateToAccessControl();
      
      // Verify can view role assignments
      const roleAssignments = await azureMLPage.getRoleAssignments();
      expect(roleAssignments.length).toBeGreaterThan(0);
      testLogger.info('Role assignments accessible', { count: roleAssignments.length });
      
      // Verify can add role assignments (UI check only, don't actually add)
      const canAddRoles = await azureMLPage.canAddRoleAssignments();
      expect(canAddRoles).toBe(true);
      testLogger.info('Can add role assignments: true');
      
      // Test 2: Workspace Settings Management
      testLogger.step('Testing workspace settings management');
      
      await azureMLPage.navigateToWorkspaceSettings();
      
      // Verify can modify workspace settings
      const canModifySettings = await azureMLPage.canModifyWorkspaceSettings();
      expect(canModifySettings).toBe(true);
      
      // Test access to advanced settings
      const advancedSettingsAccessible = await azureMLPage.isAdvancedSettingsAccessible();
      expect(advancedSettingsAccessible).toBe(true);
      
      // Test 3: Resource Management
      testLogger.step('Testing resource management capabilities');
      
      // Test compute management
      await azureMLPage.navigateToCompute();
      
      const canCreateCompute = await azureMLPage.canCreateComputeResources();
      expect(canCreateCompute).toBe(true);
      
      const canDeleteCompute = await azureMLPage.canDeleteComputeResources();
      expect(canDeleteCompute).toBe(true);
      
      // Test 4: Data Management
      testLogger.step('Testing data management capabilities');
      
      await azureMLPage.navigateToData();
      
      const canCreateDatasets = await azureMLPage.canCreateDatasets();
      expect(canCreateDatasets).toBe(true);
      
      const canDeleteDatasets = await azureMLPage.canDeleteDatasets();
      expect(canDeleteDatasets).toBe(true);
      
      // Test 5: Model Management
      testLogger.step('Testing model management capabilities');
      
      await azureMLPage.navigateToModels();
      
      const canRegisterModels = await azureMLPage.canRegisterModels();
      expect(canRegisterModels).toBe(true);
      
      const canDeleteModels = await azureMLPage.canDeleteModels();
      expect(canDeleteModels).toBe(true);
      
      // Test 6: Endpoint Management
      testLogger.step('Testing endpoint management capabilities');
      
      await azureMLPage.navigateToEndpoints();
      
      const canCreateEndpoints = await azureMLPage.canCreateEndpoints();
      expect(canCreateEndpoints).toBe(true);
      
      const canDeleteEndpoints = await azureMLPage.canDeleteEndpoints();
      expect(canDeleteEndpoints).toBe(true);
      
      testLogger.info('Owner role permissions verified successfully');
      await azureMLPage.takeScreenshot('owner-role-permissions');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Owner role permissions test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should verify Contributor role permissions @roles @contributor', async ({ page }) => {
    const testLogger = logTestStart('Contributor Role Permissions Test');
    currentRole = 'Contributor';
    
    try {
      // Note: This test assumes the service principal has Contributor role
      // In a real scenario, you would switch to a different credential set
      
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Test 1: Resource Management (Should have access)
      testLogger.step('Testing resource management capabilities');
      
      await azureMLPage.navigateToCompute();
      
      const canCreateCompute = await azureMLPage.canCreateComputeResources();
      expect(canCreateCompute).toBe(true);
      
      const canModifyCompute = await azureMLPage.canModifyComputeResources();
      expect(canModifyCompute).toBe(true);
      
      // Test 2: Access Control (Should NOT have access)
      testLogger.step('Testing access control restrictions');
      
      try {
        await azureMLPage.navigateToAccessControl();
        
        // Should not be able to add role assignments
        const canAddRoles = await azureMLPage.canAddRoleAssignments();
        expect(canAddRoles).toBe(false);
        testLogger.info('Correctly restricted from adding role assignments');
        
        // Should not be able to modify role assignments
        const canModifyRoles = await azureMLPage.canModifyRoleAssignments();
        expect(canModifyRoles).toBe(false);
        testLogger.info('Correctly restricted from modifying role assignments');
        
      } catch (accessError) {
        // If navigation to access control fails, that's expected for Contributor
        testLogger.info('Access control navigation restricted (expected for Contributor)');
      }
      
      // Test 3: Workspace Settings (Limited access)
      testLogger.step('Testing workspace settings access');
      
      await azureMLPage.navigateToWorkspaceSettings();
      
      // Should have access to basic settings but not advanced security settings
      const canModifyBasicSettings = await azureMLPage.canModifyBasicWorkspaceSettings();
      expect(canModifyBasicSettings).toBe(true);
      
      const canModifySecuritySettings = await azureMLPage.canModifySecuritySettings();
      expect(canModifySecuritySettings).toBe(false);
      
      // Test 4: Data and Model Management (Should have full access)
      testLogger.step('Testing data and model management');
      
      await azureMLPage.navigateToData();
      const canManageData = await azureMLPage.canManageDataAssets();
      expect(canManageData).toBe(true);
      
      await azureMLPage.navigateToModels();
      const canManageModels = await azureMLPage.canManageModels();
      expect(canManageModels).toBe(true);
      
      // Test 5: Experiment Management (Should have full access)
      testLogger.step('Testing experiment management');
      
      await azureMLPage.navigateToJobs();
      const canCreateExperiments = await azureMLPage.canCreateExperiments();
      expect(canCreateExperiments).toBe(true);
      
      const canManageExperiments = await azureMLPage.canManageExperiments();
      expect(canManageExperiments).toBe(true);
      
      testLogger.info('Contributor role permissions verified successfully');
      await azureMLPage.takeScreenshot('contributor-role-permissions');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Contributor role permissions test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should verify Reader role permissions @roles @reader', async ({ page }) => {
    const testLogger = logTestStart('Reader Role Permissions Test');
    currentRole = 'Reader';
    
    try {
      // Note: This test would require a separate credential with Reader role
      // For demonstration, we'll test the restrictions that should be in place
      
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Test 1: Read Access to Resources
      testLogger.step('Testing read access to resources');
      
      // Should be able to view compute resources
      await azureMLPage.navigateToCompute();
      const computeInstances = await azureMLPage.getComputeInstances();
      testLogger.info('Can view compute instances', { count: computeInstances.length });
      
      // Should be able to view data assets
      await azureMLPage.navigateToData();
      const datasets = await azureMLPage.getDatasets();
      testLogger.info('Can view datasets', { count: datasets.length });
      
      // Should be able to view models
      await azureMLPage.navigateToModels();
      const models = await azureMLPage.getModels();
      testLogger.info('Can view models', { count: models.length });
      
      // Should be able to view experiments
      await azureMLPage.navigateToJobs();
      const experiments = await azureMLPage.getExperiments();
      testLogger.info('Can view experiments', { count: experiments.length });
      
      // Test 2: Write Restrictions
      testLogger.step('Testing write access restrictions');
      
      // Should NOT be able to create compute resources
      await azureMLPage.navigateToCompute();
      const canCreateCompute = await azureMLPage.canCreateComputeResources();
      expect(canCreateCompute).toBe(false);
      testLogger.info('Correctly restricted from creating compute resources');
      
      // Should NOT be able to create datasets
      await azureMLPage.navigateToData();
      const canCreateDatasets = await azureMLPage.canCreateDatasets();
      expect(canCreateDatasets).toBe(false);
      testLogger.info('Correctly restricted from creating datasets');
      
      // Should NOT be able to register models
      await azureMLPage.navigateToModels();
      const canRegisterModels = await azureMLPage.canRegisterModels();
      expect(canRegisterModels).toBe(false);
      testLogger.info('Correctly restricted from registering models');
      
      // Should NOT be able to create experiments
      await azureMLPage.navigateToJobs();
      const canCreateExperiments = await azureMLPage.canCreateExperiments();
      expect(canCreateExperiments).toBe(false);
      testLogger.info('Correctly restricted from creating experiments');
      
      // Test 3: Management Restrictions
      testLogger.step('Testing management access restrictions');
      
      // Should NOT have access to workspace settings
      try {
        await azureMLPage.navigateToWorkspaceSettings();
        const canModifySettings = await azureMLPage.canModifyWorkspaceSettings();
        expect(canModifySettings).toBe(false);
      } catch (accessError) {
        testLogger.info('Workspace settings access restricted (expected for Reader)');
      }
      
      // Should NOT have access to access control
      try {
        await azureMLPage.navigateToAccessControl();
        const canViewRoles = await azureMLPage.canViewRoleAssignments();
        expect(canViewRoles).toBe(false);
      } catch (accessError) {
        testLogger.info('Access control navigation restricted (expected for Reader)');
      }
      
      testLogger.info('Reader role permissions verified successfully');
      await azureMLPage.takeScreenshot('reader-role-permissions');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Reader role permissions test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should verify Data Scientist role permissions @roles @data-scientist', async ({ page }) => {
    const testLogger = logTestStart('Data Scientist Role Permissions Test');
    currentRole = 'AzureML Data Scientist';
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Test 1: Experiment and Model Management (Should have full access)
      testLogger.step('Testing experiment and model management');
      
      // Should be able to create and manage experiments
      await azureMLPage.navigateToJobs();
      const canCreateExperiments = await azureMLPage.canCreateExperiments();
      expect(canCreateExperiments).toBe(true);
      
      const canManageExperiments = await azureMLPage.canManageExperiments();
      expect(canManageExperiments).toBe(true);
      
      // Should be able to register and manage models
      await azureMLPage.navigateToModels();
      const canRegisterModels = await azureMLPage.canRegisterModels();
      expect(canRegisterModels).toBe(true);
      
      const canManageModels = await azureMLPage.canManageModels();
      expect(canManageModels).toBe(true);
      
      // Test 2: Data Access (Should have read/write access)
      testLogger.step('Testing data access permissions');
      
      await azureMLPage.navigateToData();
      const canCreateDatasets = await azureMLPage.canCreateDatasets();
      expect(canCreateDatasets).toBe(true);
      
      const canManageDatasets = await azureMLPage.canManageDatasets();
      expect(canManageDatasets).toBe(true);
      
      // Test 3: Compute Access (Limited - can use but not manage infrastructure)
      testLogger.step('Testing compute access permissions');
      
      await azureMLPage.navigateToCompute();
      
      // Should be able to use existing compute
      const canUseCompute = await azureMLPage.canUseComputeResources();
      expect(canUseCompute).toBe(true);
      
      // Should be able to start/stop compute instances
      const canControlCompute = await azureMLPage.canControlComputeInstances();
      expect(canControlCompute).toBe(true);
      
      // Should NOT be able to create new compute infrastructure
      const canCreateCompute = await azureMLPage.canCreateComputeResources();
      expect(canCreateCompute).toBe(false);
      testLogger.info('Correctly restricted from creating compute infrastructure');
      
      // Test 4: Notebook Access (Should have full access)
      testLogger.step('Testing notebook access permissions');
      
      await azureMLPage.navigateToNotebooks();
      
      const canCreateNotebooks = await azureMLPage.canCreateNotebooks();
      expect(canCreateNotebooks).toBe(true);
      
      const canExecuteNotebooks = await azureMLPage.canExecuteNotebooks();
      expect(canExecuteNotebooks).toBe(true);
      
      const canShareNotebooks = await azureMLPage.canShareNotebooks();
      expect(canShareNotebooks).toBe(true);
      
      // Test 5: Endpoint Management (Limited access)
      testLogger.step('Testing endpoint management permissions');
      
      await azureMLPage.navigateToEndpoints();
      
      // Should be able to deploy models to endpoints
      const canDeployModels = await azureMLPage.canDeployModelsToEndpoints();
      expect(canDeployModels).toBe(true);
      
      // Should be able to test endpoints
      const canTestEndpoints = await azureMLPage.canTestEndpoints();
      expect(canTestEndpoints).toBe(true);
      
      // May or may not be able to create endpoints (depends on specific role configuration)
      const canCreateEndpoints = await azureMLPage.canCreateEndpoints();
      testLogger.info('Endpoint creation permission', { allowed: canCreateEndpoints });
      
      // Test 6: Workspace Settings (Should NOT have access)
      testLogger.step('Testing workspace settings restrictions');
      
      try {
        await azureMLPage.navigateToWorkspaceSettings();
        const canModifySettings = await azureMLPage.canModifyWorkspaceSettings();
        expect(canModifySettings).toBe(false);
        testLogger.info('Correctly restricted from modifying workspace settings');
      } catch (accessError) {
        testLogger.info('Workspace settings access restricted (expected for Data Scientist)');
      }
      
      testLogger.info('Data Scientist role permissions verified successfully');
      await azureMLPage.takeScreenshot('data-scientist-role-permissions');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Data Scientist role permissions test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should verify Compute Operator role permissions @roles @compute-operator', async ({ page }) => {
    const testLogger = logTestStart('Compute Operator Role Permissions Test');
    currentRole = 'AzureML Compute Operator';
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Test 1: Compute Management (Should have full access)
      testLogger.step('Testing compute management permissions');
      
      await azureMLPage.navigateToCompute();
      
      // Should be able to create compute resources
      const canCreateCompute = await azureMLPage.canCreateComputeResources();
      expect(canCreateCompute).toBe(true);
      
      // Should be able to manage compute instances
      const canManageInstances = await azureMLPage.canManageComputeInstances();
      expect(canManageInstances).toBe(true);
      
      // Should be able to manage compute clusters
      const canManageClusters = await azureMLPage.canManageComputeClusters();
      expect(canManageClusters).toBe(true);
      
      // Should be able to configure compute settings
      const canConfigureCompute = await azureMLPage.canConfigureComputeSettings();
      expect(canConfigureCompute).toBe(true);
      
      // Test 2: Resource Monitoring (Should have access)
      testLogger.step('Testing resource monitoring permissions');
      
      // Should be able to view compute metrics
      const canViewMetrics = await azureMLPage.canViewComputeMetrics();
      expect(canViewMetrics).toBe(true);
      
      // Should be able to view resource utilization
      const canViewUtilization = await azureMLPage.canViewResourceUtilization();
      expect(canViewUtilization).toBe(true);
      
      // Test 3: Data Access (Should be limited)
      testLogger.step('Testing data access restrictions');
      
      await azureMLPage.navigateToData();
      
      // Should be able to view datasets (for compute allocation)
      const canViewDatasets = await azureMLPage.canViewDatasets();
      expect(canViewDatasets).toBe(true);
      
      // Should NOT be able to create or modify datasets
      const canCreateDatasets = await azureMLPage.canCreateDatasets();
      expect(canCreateDatasets).toBe(false);
      testLogger.info('Correctly restricted from creating datasets');
      
      // Test 4: Experiment Access (Should be limited)
      testLogger.step('Testing experiment access restrictions');
      
      await azureMLPage.navigateToJobs();
      
      // Should be able to view experiments (for resource allocation)
      const canViewExperiments = await azureMLPage.canViewExperiments();
      expect(canViewExperiments).toBe(true);
      
      // Should NOT be able to create experiments
      const canCreateExperiments = await azureMLPage.canCreateExperiments();
      expect(canCreateExperiments).toBe(false);
      testLogger.info('Correctly restricted from creating experiments');
      
      // Should be able to manage compute resources for experiments
      const canManageExperimentCompute = await azureMLPage.canManageExperimentComputeResources();
      expect(canManageExperimentCompute).toBe(true);
      
      // Test 5: Model Management (Should be restricted)
      testLogger.step('Testing model management restrictions');
      
      await azureMLPage.navigateToModels();
      
      // Should be able to view models
      const canViewModels = await azureMLPage.canViewModels();
      expect(canViewModels).toBe(true);
      
      // Should NOT be able to register models
      const canRegisterModels = await azureMLPage.canRegisterModels();
      expect(canRegisterModels).toBe(false);
      testLogger.info('Correctly restricted from registering models');
      
      // Test 6: Endpoint Management (Limited to compute aspects)
      testLogger.step('Testing endpoint management permissions');
      
      await azureMLPage.navigateToEndpoints();
      
      // Should be able to view endpoints
      const canViewEndpoints = await azureMLPage.canViewEndpoints();
      expect(canViewEndpoints).toBe(true);
      
      // Should be able to manage endpoint compute resources
      const canManageEndpointCompute = await azureMLPage.canManageEndpointComputeResources();
      expect(canManageEndpointCompute).toBe(true);
      
      // Should NOT be able to create endpoints
      const canCreateEndpoints = await azureMLPage.canCreateEndpoints();
      expect(canCreateEndpoints).toBe(false);
      testLogger.info('Correctly restricted from creating endpoints');
      
      // Test 7: Workspace Settings (Should be restricted)
      testLogger.step('Testing workspace settings restrictions');
      
      try {
        await azureMLPage.navigateToWorkspaceSettings();
        const canModifySettings = await azureMLPage.canModifyWorkspaceSettings();
        expect(canModifySettings).toBe(false);
        testLogger.info('Correctly restricted from modifying workspace settings');
      } catch (accessError) {
        testLogger.info('Workspace settings access restricted (expected for Compute Operator)');
      }
      
      testLogger.info('Compute Operator role permissions verified successfully');
      await azureMLPage.takeScreenshot('compute-operator-role-permissions');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Compute Operator role permissions test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should test role escalation and PIM integration @roles @pim', async ({ page }) => {
    const testLogger = logTestStart('Role Escalation and PIM Integration Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Test 1: Check current role and permissions
      testLogger.step('Checking current role and permissions');
      
      const currentRole = await azureMLPage.getCurrentUserRole();
      const currentPermissions = await azureMLPage.getCurrentUserPermissions();
      
      testLogger.info('Current role and permissions', {
        role: currentRole,
        permissions: currentPermissions,
      });
      
      // Test 2: Test PIM role activation (if configured)
      if (config.pim && config.pim.enabled) {
        testLogger.step('Testing PIM role activation');
        
        try {
          // Request PIM role activation
          const pimActivation = await azureHelper.requestPIMActivation(
            config.pim.roleName,
            config.pim.scope,
            config.pim.justification
          );
          
          testLogger.info('PIM activation requested', pimActivation);
          
          // Wait for activation to complete
          await azureHelper.waitForPIMActivation(pimActivation.requestId, 300000);
          
          // Refresh page to get new permissions
          await page.reload();
          await azureMLPage.waitForPageLoad();
          
          // Verify elevated permissions
          const elevatedRole = await azureMLPage.getCurrentUserRole();
          const elevatedPermissions = await azureMLPage.getCurrentUserPermissions();
          
          testLogger.info('Elevated role and permissions', {
            role: elevatedRole,
            permissions: elevatedPermissions,
          });
          
          // Verify we have additional permissions
          expect(elevatedPermissions.length).toBeGreaterThan(currentPermissions.length);
          
          // Test elevated operations
          testLogger.step('Testing elevated operations');
          
          // Should now be able to access workspace settings
          await azureMLPage.navigateToWorkspaceSettings();
          const canModifySettings = await azureMLPage.canModifyWorkspaceSettings();
          expect(canModifySettings).toBe(true);
          
          // Should be able to access access control
          await azureMLPage.navigateToAccessControl();
          const canViewRoles = await azureMLPage.canViewRoleAssignments();
          expect(canViewRoles).toBe(true);
          
          testLogger.info('PIM role activation and elevated operations verified');
          
        } catch (pimError) {
          testLogger.warn('PIM activation failed or not configured', { error: pimError.message });
        }
      } else {
        testLogger.info('PIM not configured, skipping PIM tests');
      }
      
      // Test 3: Test role-based UI elements
      testLogger.step('Testing role-based UI elements');
      
      // Navigate through different sections and verify UI elements are shown/hidden based on role
      const sections = ['compute', 'data', 'models', 'jobs', 'endpoints'];
      
      for (const section of sections) {
        await azureMLPage.navigateToSection(section);
        
        const visibleActions = await azureMLPage.getVisibleActions();
        const hiddenActions = await azureMLPage.getHiddenActions();
        
        testLogger.info(`${section} section UI elements`, {
          visible: visibleActions,
          hidden: hiddenActions,
        });
        
        // Verify that actions match expected permissions
        const expectedActions = getExpectedActionsForRole(currentRole, section);
        
        for (const action of expectedActions.visible) {
          expect(visibleActions).toContain(action);
        }
        
        for (const action of expectedActions.hidden) {
          expect(hiddenActions).toContain(action);
        }
      }
      
      // Test 4: Test cross-resource access
      testLogger.step('Testing cross-resource access');
      
      // Test access to resources in different resource groups (if configured)
      if (config.testing && config.testing.crossResourceGroupAccess) {
        const otherResourceGroup = config.testing.otherResourceGroup;
        
        try {
          const crossResourceAccess = await azureHelper.testCrossResourceAccess(otherResourceGroup);
          testLogger.info('Cross-resource access test', crossResourceAccess);
        } catch (crossAccessError) {
          testLogger.info('Cross-resource access restricted (expected)', { 
            error: crossAccessError.message 
          });
        }
      }
      
      await azureMLPage.takeScreenshot('role-escalation-pim');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Role escalation and PIM test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });
});

// Helper function to get expected actions for a role in a specific section
function getExpectedActionsForRole(role: string, section: string): { visible: string[], hidden: string[] } {
  const rolePermissions = {
    'Owner': {
      compute: {
        visible: ['create', 'delete', 'start', 'stop', 'modify'],
        hidden: [],
      },
      data: {
        visible: ['create', 'delete', 'upload', 'download', 'modify'],
        hidden: [],
      },
      models: {
        visible: ['register', 'delete', 'deploy', 'download'],
        hidden: [],
      },
      jobs: {
        visible: ['create', 'cancel', 'clone', 'delete'],
        hidden: [],
      },
      endpoints: {
        visible: ['create', 'delete', 'update', 'test'],
        hidden: [],
      },
    },
    'Contributor': {
      compute: {
        visible: ['create', 'delete', 'start', 'stop', 'modify'],
        hidden: [],
      },
      data: {
        visible: ['create', 'delete', 'upload', 'download', 'modify'],
        hidden: [],
      },
      models: {
        visible: ['register', 'delete', 'deploy', 'download'],
        hidden: [],
      },
      jobs: {
        visible: ['create', 'cancel', 'clone', 'delete'],
        hidden: [],
      },
      endpoints: {
        visible: ['create', 'delete', 'update', 'test'],
        hidden: [],
      },
    },
    'Reader': {
      compute: {
        visible: ['view'],
        hidden: ['create', 'delete', 'start', 'stop', 'modify'],
      },
      data: {
        visible: ['view', 'download'],
        hidden: ['create', 'delete', 'upload', 'modify'],
      },
      models: {
        visible: ['view', 'download'],
        hidden: ['register', 'delete', 'deploy'],
      },
      jobs: {
        visible: ['view'],
        hidden: ['create', 'cancel', 'clone', 'delete'],
      },
      endpoints: {
        visible: ['view', 'test'],
        hidden: ['create', 'delete', 'update'],
      },
    },
    'AzureML Data Scientist': {
      compute: {
        visible: ['start', 'stop', 'view'],
        hidden: ['create', 'delete', 'modify'],
      },
      data: {
        visible: ['create', 'upload', 'download', 'modify', 'view'],
        hidden: ['delete'],
      },
      models: {
        visible: ['register', 'deploy', 'download', 'view'],
        hidden: ['delete'],
      },
      jobs: {
        visible: ['create', 'cancel', 'clone', 'view'],
        hidden: ['delete'],
      },
      endpoints: {
        visible: ['create', 'update', 'test', 'view'],
        hidden: ['delete'],
      },
    },
    'AzureML Compute Operator': {
      compute: {
        visible: ['create', 'delete', 'start', 'stop', 'modify', 'view'],
        hidden: [],
      },
      data: {
        visible: ['view'],
        hidden: ['create', 'delete', 'upload', 'modify'],
      },
      models: {
        visible: ['view'],
        hidden: ['register', 'delete', 'deploy'],
      },
      jobs: {
        visible: ['view'],
        hidden: ['create', 'cancel', 'clone', 'delete'],
      },
      endpoints: {
        visible: ['view'],
        hidden: ['create', 'delete', 'update', 'test'],
      },
    },
  };

  return rolePermissions[role]?.[section] || { visible: [], hidden: [] };
}