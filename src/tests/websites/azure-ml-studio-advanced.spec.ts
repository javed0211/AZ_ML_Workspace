import { test, expect } from '@playwright/test';
import { AzureMLStudioPage } from '../../pages/azure-ml-studio';
import { createAzureMLHelper } from '../../helpers/azure-helpers';
import { logTestStart, logTestEnd } from '../../helpers/logger';
import { config } from '../../helpers/config';

test.describe('Azure ML Studio Advanced Website Tests', () => {
  let azureHelper: any;

  test.beforeAll(async () => {
    azureHelper = createAzureMLHelper();
  });

  test.beforeEach(async ({ page }) => {
    await page.goto(config.urls.base);
  });

  test('should manage complete ML experiment lifecycle @website @integration', async ({ page }) => {
    const testLogger = logTestStart('Complete ML Experiment Lifecycle Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      const experimentName = `automation-experiment-${Date.now()}`;
      const datasetName = `test-dataset-${Date.now()}`;
      const modelName = `test-model-${Date.now()}`;
      
      // Step 1: Create and register a dataset
      testLogger.step('Creating and registering dataset');
      await azureMLPage.navigateToData();
      
      // Create dataset from web files (using a public dataset)
      await azureMLPage.createDatasetFromWeb(
        datasetName,
        'https://raw.githubusercontent.com/microsoft/MLOps/main/examples/sample_data/diabetes.csv',
        'Diabetes dataset for testing'
      );
      
      // Wait for dataset registration to complete
      await azureMLPage.waitForDatasetRegistration(datasetName, 300000);
      
      // Verify dataset is available
      await azureMLPage.assertDatasetExists(datasetName);
      
      // Step 2: Create an experiment
      testLogger.step('Creating ML experiment');
      await azureMLPage.navigateToJobs();
      
      // Create a new experiment using AutoML
      await azureMLPage.createAutoMLExperiment({
        experimentName: experimentName,
        datasetName: datasetName,
        targetColumn: 'target',
        taskType: 'classification',
        computeTarget: config.compute.clusterName || 'cpu-cluster',
        trainingTime: 15, // 15 minutes
        maxConcurrentIterations: 2,
      });
      
      // Verify experiment is created and submitted
      await azureMLPage.assertExperimentExists(experimentName);
      
      const experimentStatus = await azureMLPage.getExperimentStatus(experimentName);
      expect(['Submitted', 'Running', 'Preparing'].some(status => 
        experimentStatus.toLowerCase().includes(status.toLowerCase())
      )).toBeTruthy();
      
      testLogger.info('Experiment created and submitted', { 
        name: experimentName, 
        status: experimentStatus 
      });
      
      // Step 3: Monitor experiment progress
      testLogger.step('Monitoring experiment progress');
      
      // Wait for experiment to start running (not complete, just start)
      await azureMLPage.waitForExperimentStatus(experimentName, 'Running', 600000);
      
      // Get experiment details
      const experimentDetails = await azureMLPage.getExperimentDetails(experimentName);
      testLogger.info('Experiment details', experimentDetails);
      
      // Verify experiment metrics are being tracked
      const metricsAvailable = await azureMLPage.hasExperimentMetrics(experimentName);
      if (metricsAvailable) {
        const metrics = await azureMLPage.getExperimentMetrics(experimentName);
        testLogger.info('Experiment metrics', metrics);
      }
      
      // Step 4: Test experiment management features
      testLogger.step('Testing experiment management features');
      
      // Test experiment filtering and search
      await azureMLPage.filterExperimentsByStatus('Running');
      const runningExperiments = await azureMLPage.getVisibleExperiments();
      expect(runningExperiments).toContain(experimentName);
      
      // Test experiment details view
      await azureMLPage.openExperimentDetails(experimentName);
      
      // Verify experiment details page loads
      await azureMLPage.assertElementVisible('[data-testid="experiment-details"]');
      
      // Check if child runs are visible
      const childRuns = await azureMLPage.getExperimentChildRuns(experimentName);
      testLogger.info('Child runs found', { count: childRuns.length });
      
      if (childRuns.length > 0) {
        // Open first child run details
        await azureMLPage.openChildRunDetails(childRuns[0]);
        
        // Verify child run details
        await azureMLPage.assertElementVisible('[data-testid="run-details"]');
        
        // Check for logs
        const logsAvailable = await azureMLPage.hasRunLogs(childRuns[0]);
        if (logsAvailable) {
          const logs = await azureMLPage.getRunLogs(childRuns[0]);
          testLogger.info('Run logs available', { logLength: logs.length });
        }
      }
      
      // Step 5: Test model management (if experiment produces models)
      testLogger.step('Testing model management features');
      await azureMLPage.navigateToModels();
      
      // Check if any models are available from our experiment
      const availableModels = await azureMLPage.getAvailableModels();
      testLogger.info('Available models', { count: availableModels.length });
      
      if (availableModels.length > 0) {
        // Test model details view
        await azureMLPage.openModelDetails(availableModels[0]);
        
        // Verify model details page
        await azureMLPage.assertElementVisible('[data-testid="model-details"]');
        
        // Check model properties
        const modelProperties = await azureMLPage.getModelProperties(availableModels[0]);
        testLogger.info('Model properties', modelProperties);
      }
      
      // Step 6: Test endpoints management
      testLogger.step('Testing endpoints management');
      await azureMLPage.navigateToEndpoints();
      
      // Get existing endpoints
      const existingEndpoints = await azureMLPage.getAvailableEndpoints();
      testLogger.info('Existing endpoints', { count: existingEndpoints.length });
      
      // Test endpoint creation UI (don't actually create to avoid costs)
      await azureMLPage.openCreateEndpointDialog();
      await azureMLPage.assertElementVisible('[data-testid="create-endpoint-dialog"]');
      
      // Cancel the dialog
      await azureMLPage.cancelCreateEndpoint();
      
      // Take screenshot of the complete workflow
      await azureMLPage.takeScreenshot('ml-experiment-lifecycle');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('ML experiment lifecycle test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should test collaborative features and workspace sharing @website @collaboration', async ({ page }) => {
    const testLogger = logTestStart('Collaborative Features and Workspace Sharing Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Step 1: Test workspace information and settings
      testLogger.step('Testing workspace information and settings');
      
      // Navigate to workspace settings
      await azureMLPage.openWorkspaceSettings();
      
      // Verify workspace details
      const workspaceInfo = await azureMLPage.getWorkspaceInformation();
      expect(workspaceInfo.name).toBe(config.azure.workspaceName);
      expect(workspaceInfo.resourceGroup).toBe(config.azure.resourceGroup);
      expect(workspaceInfo.subscription).toBe(config.azure.subscriptionId);
      
      testLogger.info('Workspace information verified', workspaceInfo);
      
      // Step 2: Test access control and permissions
      testLogger.step('Testing access control and permissions');
      
      // Navigate to access control
      await azureMLPage.navigateToAccessControl();
      
      // Get current user permissions
      const currentUserPermissions = await azureMLPage.getCurrentUserPermissions();
      testLogger.info('Current user permissions', currentUserPermissions);
      
      // Verify user has necessary permissions for testing
      const requiredPermissions = ['read', 'write', 'execute'];
      for (const permission of requiredPermissions) {
        expect(currentUserPermissions).toContain(permission);
      }
      
      // Test role assignments view
      const roleAssignments = await azureMLPage.getRoleAssignments();
      testLogger.info('Role assignments', { count: roleAssignments.length });
      
      // Verify current user appears in role assignments
      const currentUserInRoles = roleAssignments.some(assignment => 
        assignment.principalName.includes(config.azure.clientId) ||
        assignment.principalType === 'ServicePrincipal'
      );
      expect(currentUserInRoles).toBeTruthy();
      
      // Step 3: Test shared resources and assets
      testLogger.step('Testing shared resources and assets');
      
      // Test shared datasets
      await azureMLPage.navigateToData();
      const sharedDatasets = await azureMLPage.getSharedDatasets();
      testLogger.info('Shared datasets', { count: sharedDatasets.length });
      
      // Test shared models
      await azureMLPage.navigateToModels();
      const sharedModels = await azureMLPage.getSharedModels();
      testLogger.info('Shared models', { count: sharedModels.length });
      
      // Test shared compute resources
      await azureMLPage.navigateToCompute();
      const sharedCompute = await azureMLPage.getSharedComputeResources();
      testLogger.info('Shared compute resources', sharedCompute);
      
      // Step 4: Test notebook sharing and collaboration
      testLogger.step('Testing notebook sharing and collaboration');
      
      await azureMLPage.navigateToNotebooks();
      
      // Create a shared notebook
      const sharedNotebookName = `shared-notebook-${Date.now()}.ipynb`;
      await azureMLPage.createNotebook(sharedNotebookName);
      
      // Add content to the notebook
      await azureMLPage.addNotebookCell('markdown', `
# Shared Notebook for Collaboration Testing

This notebook is created for testing collaborative features.

## Test Information
- Created: ${new Date().toISOString()}
- Purpose: Automation testing
- Workspace: ${config.azure.workspaceName}
`);

      await azureMLPage.addNotebookCell('code', `
# Collaborative code example
import pandas as pd
import numpy as np
from datetime import datetime

print(f"Notebook executed at: {datetime.now()}")
print(f"Workspace: ${config.azure.workspaceName}")

# Create sample data for collaboration
data = pd.DataFrame({
    'timestamp': [datetime.now()],
    'user': ['automation-test'],
    'action': ['notebook-creation'],
    'workspace': ['${config.azure.workspaceName}']
})

print("Collaboration data:")
print(data)
`);

      // Save the notebook
      await azureMLPage.saveNotebook();
      
      // Test notebook sharing options
      await azureMLPage.openNotebookSharingOptions(sharedNotebookName);
      
      // Verify sharing options are available
      await azureMLPage.assertElementVisible('[data-testid="sharing-options"]');
      
      // Test different sharing permissions
      const sharingOptions = await azureMLPage.getAvailableSharingOptions();
      testLogger.info('Available sharing options', sharingOptions);
      
      expect(sharingOptions).toContain('read');
      expect(sharingOptions).toContain('write');
      
      // Step 5: Test workspace activity and audit logs
      testLogger.step('Testing workspace activity and audit logs');
      
      // Navigate to activity logs
      await azureMLPage.navigateToActivityLogs();
      
      // Get recent activities
      const recentActivities = await azureMLPage.getRecentActivities(10);
      testLogger.info('Recent activities', { count: recentActivities.length });
      
      // Verify our test activities appear in logs
      const testActivities = recentActivities.filter(activity => 
        activity.description.includes('automation') ||
        activity.description.includes(sharedNotebookName)
      );
      
      expect(testActivities.length).toBeGreaterThan(0);
      testLogger.info('Test activities found in logs', { count: testActivities.length });
      
      // Step 6: Test workspace quotas and usage
      testLogger.step('Testing workspace quotas and usage');
      
      // Navigate to quotas and usage
      await azureMLPage.navigateToQuotasAndUsage();
      
      // Get current usage information
      const usageInfo = await azureMLPage.getWorkspaceUsage();
      testLogger.info('Workspace usage information', usageInfo);
      
      // Verify usage information is available
      expect(usageInfo.compute).toBeDefined();
      expect(usageInfo.storage).toBeDefined();
      
      // Check quota limits
      const quotaLimits = await azureMLPage.getQuotaLimits();
      testLogger.info('Quota limits', quotaLimits);
      
      // Verify we're not exceeding quotas
      for (const [resource, usage] of Object.entries(usageInfo)) {
        if (quotaLimits[resource]) {
          const utilizationPercent = (usage.current / quotaLimits[resource].limit) * 100;
          testLogger.info(`${resource} utilization`, { 
            current: usage.current,
            limit: quotaLimits[resource].limit,
            percentage: utilizationPercent.toFixed(2)
          });
          
          // Warn if utilization is high
          if (utilizationPercent > 80) {
            testLogger.warn(`High ${resource} utilization: ${utilizationPercent.toFixed(2)}%`);
          }
        }
      }
      
      // Take screenshot of collaboration features
      await azureMLPage.takeScreenshot('collaboration-features');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Collaboration features test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });

  test('should test advanced security and compliance features @website @security', async ({ page }) => {
    const testLogger = logTestStart('Advanced Security and Compliance Features Test');
    
    try {
      const azureMLPage = new AzureMLStudioPage(page, testLogger);
      await azureMLPage.waitForPageLoad();
      
      // Step 1: Test network security features
      testLogger.step('Testing network security features');
      
      // Navigate to network settings
      await azureMLPage.navigateToNetworkSettings();
      
      // Get current network configuration
      const networkConfig = await azureMLPage.getNetworkConfiguration();
      testLogger.info('Network configuration', networkConfig);
      
      // Verify network security settings
      if (networkConfig.privateEndpoints) {
        expect(networkConfig.privateEndpoints.enabled).toBeDefined();
        testLogger.info('Private endpoints configuration', networkConfig.privateEndpoints);
      }
      
      if (networkConfig.firewallRules) {
        testLogger.info('Firewall rules', { count: networkConfig.firewallRules.length });
      }
      
      // Step 2: Test data encryption and key management
      testLogger.step('Testing data encryption and key management');
      
      // Navigate to encryption settings
      await azureMLPage.navigateToEncryptionSettings();
      
      // Get encryption configuration
      const encryptionConfig = await azureMLPage.getEncryptionConfiguration();
      testLogger.info('Encryption configuration', encryptionConfig);
      
      // Verify encryption is enabled
      expect(encryptionConfig.dataEncryption.enabled).toBe(true);
      
      if (encryptionConfig.customerManagedKeys) {
        testLogger.info('Customer managed keys configuration', encryptionConfig.customerManagedKeys);
      }
      
      // Step 3: Test compliance and audit features
      testLogger.step('Testing compliance and audit features');
      
      // Navigate to compliance dashboard
      await azureMLPage.navigateToComplianceDashboard();
      
      // Get compliance status
      const complianceStatus = await azureMLPage.getComplianceStatus();
      testLogger.info('Compliance status', complianceStatus);
      
      // Verify compliance checks
      const complianceChecks = [
        'dataGovernance',
        'accessControl',
        'auditLogging',
        'dataEncryption'
      ];
      
      for (const check of complianceChecks) {
        if (complianceStatus[check]) {
          expect(complianceStatus[check].status).toBe('compliant');
          testLogger.info(`${check} compliance verified`);
        }
      }
      
      // Step 4: Test audit logging and monitoring
      testLogger.step('Testing audit logging and monitoring');
      
      // Navigate to audit logs
      await azureMLPage.navigateToAuditLogs();
      
      // Get audit log configuration
      const auditConfig = await azureMLPage.getAuditLogConfiguration();
      testLogger.info('Audit log configuration', auditConfig);
      
      // Verify audit logging is enabled
      expect(auditConfig.enabled).toBe(true);
      
      // Get recent audit events
      const auditEvents = await azureMLPage.getAuditEvents(24); // Last 24 hours
      testLogger.info('Recent audit events', { count: auditEvents.length });
      
      // Verify audit events contain required information
      if (auditEvents.length > 0) {
        const sampleEvent = auditEvents[0];
        expect(sampleEvent.timestamp).toBeDefined();
        expect(sampleEvent.user).toBeDefined();
        expect(sampleEvent.action).toBeDefined();
        expect(sampleEvent.resource).toBeDefined();
        
        testLogger.info('Sample audit event', sampleEvent);
      }
      
      // Step 5: Test data lineage and governance
      testLogger.step('Testing data lineage and governance');
      
      // Navigate to data governance
      await azureMLPage.navigateToDataGovernance();
      
      // Get data lineage information
      const dataLineage = await azureMLPage.getDataLineageInformation();
      testLogger.info('Data lineage information', dataLineage);
      
      // Test data classification features
      if (dataLineage.dataClassification) {
        const classifiedAssets = await azureMLPage.getClassifiedDataAssets();
        testLogger.info('Classified data assets', { count: classifiedAssets.length });
        
        // Verify data classification labels
        for (const asset of classifiedAssets.slice(0, 5)) { // Check first 5
          expect(asset.classification).toBeDefined();
          expect(asset.sensitivityLevel).toBeDefined();
        }
      }
      
      // Step 6: Test security alerts and recommendations
      testLogger.step('Testing security alerts and recommendations');
      
      // Navigate to security center
      await azureMLPage.navigateToSecurityCenter();
      
      // Get security alerts
      const securityAlerts = await azureMLPage.getSecurityAlerts();
      testLogger.info('Security alerts', { count: securityAlerts.length });
      
      // Check for high-priority alerts
      const highPriorityAlerts = securityAlerts.filter(alert => 
        alert.severity === 'high' || alert.severity === 'critical'
      );
      
      if (highPriorityAlerts.length > 0) {
        testLogger.warn('High priority security alerts found', { 
          count: highPriorityAlerts.length,
          alerts: highPriorityAlerts.map(a => ({ id: a.id, title: a.title, severity: a.severity }))
        });
      }
      
      // Get security recommendations
      const securityRecommendations = await azureMLPage.getSecurityRecommendations();
      testLogger.info('Security recommendations', { count: securityRecommendations.length });
      
      // Step 7: Test identity and access management integration
      testLogger.step('Testing identity and access management integration');
      
      // Navigate to identity settings
      await azureMLPage.navigateToIdentitySettings();
      
      // Get identity provider configuration
      const identityConfig = await azureMLPage.getIdentityConfiguration();
      testLogger.info('Identity configuration', identityConfig);
      
      // Verify Azure AD integration
      expect(identityConfig.azureAD.enabled).toBe(true);
      
      // Test conditional access policies
      if (identityConfig.conditionalAccess) {
        const policies = await azureMLPage.getConditionalAccessPolicies();
        testLogger.info('Conditional access policies', { count: policies.length });
        
        // Verify policies are properly configured
        for (const policy of policies) {
          expect(policy.state).toBe('enabled');
          expect(policy.conditions).toBeDefined();
        }
      }
      
      // Take screenshot of security features
      await azureMLPage.takeScreenshot('security-compliance-features');
      
      logTestEnd(testLogger, true);
    } catch (error) {
      testLogger.error('Security and compliance test failed', { error: error.message });
      logTestEnd(testLogger, false);
      throw error;
    }
  });
});