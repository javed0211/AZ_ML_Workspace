import * as dotenv from 'dotenv';
import * as Joi from 'joi';
import * as path from 'path';

// Load environment variables
dotenv.config();

// Configuration schema validation
const configSchema = Joi.object({
  // Environment
  NODE_ENV: Joi.string().valid('development', 'test', 'production').default('development'),
  
  // Azure Configuration
  AZURE_TENANT_ID: Joi.string().when('NODE_ENV', {
    is: 'production',
    then: Joi.required(),
    otherwise: Joi.optional()
  }),
  AZURE_CLIENT_ID: Joi.string().when('NODE_ENV', {
    is: 'production',
    then: Joi.required(),
    otherwise: Joi.optional()
  }),
  AZURE_CLIENT_SECRET: Joi.string().optional(),
  AZURE_SUBSCRIPTION_ID: Joi.string().when('NODE_ENV', {
    is: 'production',
    then: Joi.required(),
    otherwise: Joi.optional()
  }),
  AZURE_RESOURCE_GROUP: Joi.string().when('NODE_ENV', {
    is: 'production',
    then: Joi.required(),
    otherwise: Joi.optional()
  }),
  AZURE_ML_WORKSPACE_NAME: Joi.string().when('NODE_ENV', {
    is: 'production',
    then: Joi.required(),
    otherwise: Joi.optional()
  }),
  AZURE_KEY_VAULT_URL: Joi.string().uri().optional(),
  
  // Authentication
  USE_INTERACTIVE_AUTH: Joi.boolean().default(false),
  USE_MANAGED_IDENTITY: Joi.boolean().default(false),
  
  // Test Configuration
  BASE_URL: Joi.string().uri().default('https://ml.azure.com'),
  VSCODE_WEB_URL: Joi.string().uri().optional(),
  JUPYTER_URL: Joi.string().uri().optional(),
  
  // Timeouts (in milliseconds)
  DEFAULT_TIMEOUT: Joi.number().default(30000),
  NAVIGATION_TIMEOUT: Joi.number().default(60000),
  TEST_TIMEOUT: Joi.number().default(300000),
  
  // Retry Configuration
  MAX_RETRIES: Joi.number().default(3),
  RETRY_DELAY: Joi.number().default(1000),
  
  // Parallel Execution
  MAX_WORKERS: Joi.number().default(4),
  
  // Artifacts
  ARTIFACTS_PATH: Joi.string().default('./test-results'),
  UPLOAD_ARTIFACTS: Joi.boolean().default(false),
  AZURE_STORAGE_ACCOUNT: Joi.string().optional(),
  AZURE_STORAGE_CONTAINER: Joi.string().default('test-artifacts'),
  
  // Logging
  LOG_LEVEL: Joi.string().valid('error', 'warn', 'info', 'debug').default('info'),
  LOG_FORMAT: Joi.string().valid('json', 'simple').default('json'),
  
  // Electron
  VSCODE_PATH: Joi.string().optional(),
  ELECTRON_USER_DATA_DIR: Joi.string().optional(),
  
  // Compute
  COMPUTE_INSTANCE_NAME: Joi.string().optional(),
  COMPUTE_CLUSTER_NAME: Joi.string().optional(),
  COMPUTE_SIZE: Joi.string().default('Standard_DS3_v2'),
  
  // PIM
  PIM_ROLE_NAME: Joi.string().optional(),
  PIM_SCOPE: Joi.string().optional(),
  PIM_JUSTIFICATION: Joi.string().default('Automated testing'),
  
  // Notifications
  TEAMS_WEBHOOK_URL: Joi.string().uri().optional(),
  SLACK_WEBHOOK_URL: Joi.string().uri().optional(),
  EMAIL_NOTIFICATIONS: Joi.boolean().default(false),
}).unknown();

const { error, value: envVars } = configSchema.validate(process.env);

if (error) {
  throw new Error(`Config validation error: ${error.message}`);
}

export const config = {
  env: envVars.NODE_ENV,
  
  azure: {
    tenantId: envVars.AZURE_TENANT_ID,
    clientId: envVars.AZURE_CLIENT_ID,
    clientSecret: envVars.AZURE_CLIENT_SECRET,
    subscriptionId: envVars.AZURE_SUBSCRIPTION_ID,
    resourceGroup: envVars.AZURE_RESOURCE_GROUP,
    workspaceName: envVars.AZURE_ML_WORKSPACE_NAME,
    keyVaultUrl: envVars.AZURE_KEY_VAULT_URL,
  },
  
  auth: {
    useInteractive: envVars.USE_INTERACTIVE_AUTH,
    useManagedIdentity: envVars.USE_MANAGED_IDENTITY,
  },
  
  urls: {
    base: envVars.BASE_URL,
    vscodeWeb: envVars.VSCODE_WEB_URL,
    jupyter: envVars.JUPYTER_URL,
  },
  
  timeouts: {
    default: envVars.DEFAULT_TIMEOUT,
    navigation: envVars.NAVIGATION_TIMEOUT,
    test: envVars.TEST_TIMEOUT,
  },
  
  retry: {
    maxRetries: envVars.MAX_RETRIES,
    delay: envVars.RETRY_DELAY,
  },
  
  parallel: {
    maxWorkers: envVars.MAX_WORKERS,
  },
  
  artifacts: {
    path: envVars.ARTIFACTS_PATH,
    upload: envVars.UPLOAD_ARTIFACTS,
    storageAccount: envVars.AZURE_STORAGE_ACCOUNT,
    container: envVars.AZURE_STORAGE_CONTAINER,
  },
  
  logging: {
    level: envVars.LOG_LEVEL,
    format: envVars.LOG_FORMAT,
  },
  
  electron: {
    vscodePath: envVars.VSCODE_PATH,
    userDataDir: envVars.ELECTRON_USER_DATA_DIR || path.join(process.cwd(), '.vscode-test'),
  },
  
  compute: {
    instanceName: envVars.COMPUTE_INSTANCE_NAME,
    clusterName: envVars.COMPUTE_CLUSTER_NAME,
    size: envVars.COMPUTE_SIZE,
  },
  
  pim: {
    roleName: envVars.PIM_ROLE_NAME,
    scope: envVars.PIM_SCOPE,
    justification: envVars.PIM_JUSTIFICATION,
  },
  
  notifications: {
    teamsWebhook: envVars.TEAMS_WEBHOOK_URL,
    slackWebhook: envVars.SLACK_WEBHOOK_URL,
    email: envVars.EMAIL_NOTIFICATIONS,
  },
};

// Environment-specific configurations
export const isDevelopment = config.env === 'development';
export const isTest = config.env === 'test';
export const isProduction = config.env === 'production';
export const isCi = !!process.env.CI;

// Validation helpers
export function validateRequiredConfig(keys: string[]): void {
  const missing = keys.filter(key => {
    const value = key.split('.').reduce((obj, k) => obj?.[k], config as any);
    return !value;
  });
  
  if (missing.length > 0) {
    throw new Error(`Missing required configuration: ${missing.join(', ')}`);
  }
}

// Export individual config sections for convenience
export const azureConfig = config.azure;
export const authConfig = config.auth;
export const urlConfig = config.urls;
export const timeoutConfig = config.timeouts;