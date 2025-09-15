/**
 * Role-based testing configuration
 * Defines different user roles and their expected permissions for testing
 */

export interface RolePermission {
  action: string;
  resource: string;
  allowed: boolean;
  description?: string;
}

export interface RoleConfiguration {
  roleName: string;
  displayName: string;
  description: string;
  permissions: RolePermission[];
  testCredentials?: {
    tenantId?: string;
    clientId?: string;
    clientSecret?: string;
    username?: string;
    password?: string;
  };
  pimEligible?: boolean;
  pimSettings?: {
    roleName: string;
    scope: string;
    maxActivationDuration: number; // in minutes
    justificationRequired: boolean;
    approvalRequired: boolean;
  };
}

export const roleConfigurations: Record<string, RoleConfiguration> = {
  owner: {
    roleName: 'Owner',
    displayName: 'Workspace Owner',
    description: 'Full access to all resources and can manage access for others',
    permissions: [
      // Workspace Management
      { action: 'read', resource: 'workspace', allowed: true },
      { action: 'write', resource: 'workspace', allowed: true },
      { action: 'delete', resource: 'workspace', allowed: true },
      { action: 'manage-access', resource: 'workspace', allowed: true },
      
      // Compute Resources
      { action: 'read', resource: 'compute', allowed: true },
      { action: 'write', resource: 'compute', allowed: true },
      { action: 'delete', resource: 'compute', allowed: true },
      { action: 'start', resource: 'compute', allowed: true },
      { action: 'stop', resource: 'compute', allowed: true },
      
      // Data Assets
      { action: 'read', resource: 'data', allowed: true },
      { action: 'write', resource: 'data', allowed: true },
      { action: 'delete', resource: 'data', allowed: true },
      { action: 'upload', resource: 'data', allowed: true },
      { action: 'download', resource: 'data', allowed: true },
      
      // Models
      { action: 'read', resource: 'models', allowed: true },
      { action: 'write', resource: 'models', allowed: true },
      { action: 'delete', resource: 'models', allowed: true },
      { action: 'register', resource: 'models', allowed: true },
      { action: 'deploy', resource: 'models', allowed: true },
      
      // Experiments/Jobs
      { action: 'read', resource: 'experiments', allowed: true },
      { action: 'write', resource: 'experiments', allowed: true },
      { action: 'delete', resource: 'experiments', allowed: true },
      { action: 'submit', resource: 'experiments', allowed: true },
      { action: 'cancel', resource: 'experiments', allowed: true },
      
      // Endpoints
      { action: 'read', resource: 'endpoints', allowed: true },
      { action: 'write', resource: 'endpoints', allowed: true },
      { action: 'delete', resource: 'endpoints', allowed: true },
      { action: 'deploy', resource: 'endpoints', allowed: true },
      { action: 'test', resource: 'endpoints', allowed: true },
      
      // Security & Compliance
      { action: 'read', resource: 'security-settings', allowed: true },
      { action: 'write', resource: 'security-settings', allowed: true },
      { action: 'read', resource: 'audit-logs', allowed: true },
      { action: 'manage', resource: 'encryption', allowed: true },
      
      // Role Management
      { action: 'read', resource: 'role-assignments', allowed: true },
      { action: 'write', resource: 'role-assignments', allowed: true },
      { action: 'delete', resource: 'role-assignments', allowed: true },
    ],
    pimEligible: false, // Owners typically have permanent access
  },

  contributor: {
    roleName: 'Contributor',
    displayName: 'Workspace Contributor',
    description: 'Full access to all resources but cannot manage access for others',
    permissions: [
      // Workspace Management
      { action: 'read', resource: 'workspace', allowed: true },
      { action: 'write', resource: 'workspace', allowed: true },
      { action: 'delete', resource: 'workspace', allowed: false },
      { action: 'manage-access', resource: 'workspace', allowed: false },
      
      // Compute Resources
      { action: 'read', resource: 'compute', allowed: true },
      { action: 'write', resource: 'compute', allowed: true },
      { action: 'delete', resource: 'compute', allowed: true },
      { action: 'start', resource: 'compute', allowed: true },
      { action: 'stop', resource: 'compute', allowed: true },
      
      // Data Assets
      { action: 'read', resource: 'data', allowed: true },
      { action: 'write', resource: 'data', allowed: true },
      { action: 'delete', resource: 'data', allowed: true },
      { action: 'upload', resource: 'data', allowed: true },
      { action: 'download', resource: 'data', allowed: true },
      
      // Models
      { action: 'read', resource: 'models', allowed: true },
      { action: 'write', resource: 'models', allowed: true },
      { action: 'delete', resource: 'models', allowed: true },
      { action: 'register', resource: 'models', allowed: true },
      { action: 'deploy', resource: 'models', allowed: true },
      
      // Experiments/Jobs
      { action: 'read', resource: 'experiments', allowed: true },
      { action: 'write', resource: 'experiments', allowed: true },
      { action: 'delete', resource: 'experiments', allowed: true },
      { action: 'submit', resource: 'experiments', allowed: true },
      { action: 'cancel', resource: 'experiments', allowed: true },
      
      // Endpoints
      { action: 'read', resource: 'endpoints', allowed: true },
      { action: 'write', resource: 'endpoints', allowed: true },
      { action: 'delete', resource: 'endpoints', allowed: true },
      { action: 'deploy', resource: 'endpoints', allowed: true },
      { action: 'test', resource: 'endpoints', allowed: true },
      
      // Security & Compliance (Limited)
      { action: 'read', resource: 'security-settings', allowed: true },
      { action: 'write', resource: 'security-settings', allowed: false },
      { action: 'read', resource: 'audit-logs', allowed: true },
      { action: 'manage', resource: 'encryption', allowed: false },
      
      // Role Management (No access)
      { action: 'read', resource: 'role-assignments', allowed: false },
      { action: 'write', resource: 'role-assignments', allowed: false },
      { action: 'delete', resource: 'role-assignments', allowed: false },
    ],
    pimEligible: true,
    pimSettings: {
      roleName: 'Owner',
      scope: '/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}',
      maxActivationDuration: 480, // 8 hours
      justificationRequired: true,
      approvalRequired: false,
    },
  },

  reader: {
    roleName: 'Reader',
    displayName: 'Workspace Reader',
    description: 'Read-only access to all resources',
    permissions: [
      // Workspace Management
      { action: 'read', resource: 'workspace', allowed: true },
      { action: 'write', resource: 'workspace', allowed: false },
      { action: 'delete', resource: 'workspace', allowed: false },
      { action: 'manage-access', resource: 'workspace', allowed: false },
      
      // Compute Resources
      { action: 'read', resource: 'compute', allowed: true },
      { action: 'write', resource: 'compute', allowed: false },
      { action: 'delete', resource: 'compute', allowed: false },
      { action: 'start', resource: 'compute', allowed: false },
      { action: 'stop', resource: 'compute', allowed: false },
      
      // Data Assets
      { action: 'read', resource: 'data', allowed: true },
      { action: 'write', resource: 'data', allowed: false },
      { action: 'delete', resource: 'data', allowed: false },
      { action: 'upload', resource: 'data', allowed: false },
      { action: 'download', resource: 'data', allowed: true },
      
      // Models
      { action: 'read', resource: 'models', allowed: true },
      { action: 'write', resource: 'models', allowed: false },
      { action: 'delete', resource: 'models', allowed: false },
      { action: 'register', resource: 'models', allowed: false },
      { action: 'deploy', resource: 'models', allowed: false },
      
      // Experiments/Jobs
      { action: 'read', resource: 'experiments', allowed: true },
      { action: 'write', resource: 'experiments', allowed: false },
      { action: 'delete', resource: 'experiments', allowed: false },
      { action: 'submit', resource: 'experiments', allowed: false },
      { action: 'cancel', resource: 'experiments', allowed: false },
      
      // Endpoints
      { action: 'read', resource: 'endpoints', allowed: true },
      { action: 'write', resource: 'endpoints', allowed: false },
      { action: 'delete', resource: 'endpoints', allowed: false },
      { action: 'deploy', resource: 'endpoints', allowed: false },
      { action: 'test', resource: 'endpoints', allowed: true },
      
      // Security & Compliance (Limited)
      { action: 'read', resource: 'security-settings', allowed: true },
      { action: 'write', resource: 'security-settings', allowed: false },
      { action: 'read', resource: 'audit-logs', allowed: false },
      { action: 'manage', resource: 'encryption', allowed: false },
      
      // Role Management (No access)
      { action: 'read', resource: 'role-assignments', allowed: false },
      { action: 'write', resource: 'role-assignments', allowed: false },
      { action: 'delete', resource: 'role-assignments', allowed: false },
    ],
    pimEligible: true,
    pimSettings: {
      roleName: 'Contributor',
      scope: '/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}',
      maxActivationDuration: 240, // 4 hours
      justificationRequired: true,
      approvalRequired: true,
    },
  },

  dataScientist: {
    roleName: 'AzureML Data Scientist',
    displayName: 'Data Scientist',
    description: 'Can create and run experiments, manage models, but limited infrastructure access',
    permissions: [
      // Workspace Management
      { action: 'read', resource: 'workspace', allowed: true },
      { action: 'write', resource: 'workspace', allowed: false },
      { action: 'delete', resource: 'workspace', allowed: false },
      { action: 'manage-access', resource: 'workspace', allowed: false },
      
      // Compute Resources (Limited)
      { action: 'read', resource: 'compute', allowed: true },
      { action: 'write', resource: 'compute', allowed: false },
      { action: 'delete', resource: 'compute', allowed: false },
      { action: 'start', resource: 'compute', allowed: true },
      { action: 'stop', resource: 'compute', allowed: true },
      
      // Data Assets
      { action: 'read', resource: 'data', allowed: true },
      { action: 'write', resource: 'data', allowed: true },
      { action: 'delete', resource: 'data', allowed: true },
      { action: 'upload', resource: 'data', allowed: true },
      { action: 'download', resource: 'data', allowed: true },
      
      // Models
      { action: 'read', resource: 'models', allowed: true },
      { action: 'write', resource: 'models', allowed: true },
      { action: 'delete', resource: 'models', allowed: true },
      { action: 'register', resource: 'models', allowed: true },
      { action: 'deploy', resource: 'models', allowed: true },
      
      // Experiments/Jobs
      { action: 'read', resource: 'experiments', allowed: true },
      { action: 'write', resource: 'experiments', allowed: true },
      { action: 'delete', resource: 'experiments', allowed: true },
      { action: 'submit', resource: 'experiments', allowed: true },
      { action: 'cancel', resource: 'experiments', allowed: true },
      
      // Endpoints
      { action: 'read', resource: 'endpoints', allowed: true },
      { action: 'write', resource: 'endpoints', allowed: true },
      { action: 'delete', resource: 'endpoints', allowed: false },
      { action: 'deploy', resource: 'endpoints', allowed: true },
      { action: 'test', resource: 'endpoints', allowed: true },
      
      // Security & Compliance (Read-only)
      { action: 'read', resource: 'security-settings', allowed: true },
      { action: 'write', resource: 'security-settings', allowed: false },
      { action: 'read', resource: 'audit-logs', allowed: false },
      { action: 'manage', resource: 'encryption', allowed: false },
      
      // Role Management (No access)
      { action: 'read', resource: 'role-assignments', allowed: false },
      { action: 'write', resource: 'role-assignments', allowed: false },
      { action: 'delete', resource: 'role-assignments', allowed: false },
    ],
    pimEligible: true,
    pimSettings: {
      roleName: 'Contributor',
      scope: '/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}',
      maxActivationDuration: 480, // 8 hours
      justificationRequired: true,
      approvalRequired: false,
    },
  },

  computeOperator: {
    roleName: 'AzureML Compute Operator',
    displayName: 'Compute Operator',
    description: 'Can manage compute resources but limited access to data and models',
    permissions: [
      // Workspace Management
      { action: 'read', resource: 'workspace', allowed: true },
      { action: 'write', resource: 'workspace', allowed: false },
      { action: 'delete', resource: 'workspace', allowed: false },
      { action: 'manage-access', resource: 'workspace', allowed: false },
      
      // Compute Resources (Full access)
      { action: 'read', resource: 'compute', allowed: true },
      { action: 'write', resource: 'compute', allowed: true },
      { action: 'delete', resource: 'compute', allowed: true },
      { action: 'start', resource: 'compute', allowed: true },
      { action: 'stop', resource: 'compute', allowed: true },
      
      // Data Assets (Read-only)
      { action: 'read', resource: 'data', allowed: true },
      { action: 'write', resource: 'data', allowed: false },
      { action: 'delete', resource: 'data', allowed: false },
      { action: 'upload', resource: 'data', allowed: false },
      { action: 'download', resource: 'data', allowed: false },
      
      // Models (Read-only)
      { action: 'read', resource: 'models', allowed: true },
      { action: 'write', resource: 'models', allowed: false },
      { action: 'delete', resource: 'models', allowed: false },
      { action: 'register', resource: 'models', allowed: false },
      { action: 'deploy', resource: 'models', allowed: false },
      
      // Experiments/Jobs (Read-only)
      { action: 'read', resource: 'experiments', allowed: true },
      { action: 'write', resource: 'experiments', allowed: false },
      { action: 'delete', resource: 'experiments', allowed: false },
      { action: 'submit', resource: 'experiments', allowed: false },
      { action: 'cancel', resource: 'experiments', allowed: false },
      
      // Endpoints (Limited to compute management)
      { action: 'read', resource: 'endpoints', allowed: true },
      { action: 'write', resource: 'endpoints', allowed: false },
      { action: 'delete', resource: 'endpoints', allowed: false },
      { action: 'deploy', resource: 'endpoints', allowed: false },
      { action: 'test', resource: 'endpoints', allowed: true },
      
      // Security & Compliance (No access)
      { action: 'read', resource: 'security-settings', allowed: false },
      { action: 'write', resource: 'security-settings', allowed: false },
      { action: 'read', resource: 'audit-logs', allowed: false },
      { action: 'manage', resource: 'encryption', allowed: false },
      
      // Role Management (No access)
      { action: 'read', resource: 'role-assignments', allowed: false },
      { action: 'write', resource: 'role-assignments', allowed: false },
      { action: 'delete', resource: 'role-assignments', allowed: false },
    ],
    pimEligible: false,
  },

  mlopsEngineer: {
    roleName: 'AzureML MLOps Engineer',
    displayName: 'MLOps Engineer',
    description: 'Can manage ML pipelines, deployments, and monitoring',
    permissions: [
      // Workspace Management
      { action: 'read', resource: 'workspace', allowed: true },
      { action: 'write', resource: 'workspace', allowed: false },
      { action: 'delete', resource: 'workspace', allowed: false },
      { action: 'manage-access', resource: 'workspace', allowed: false },
      
      // Compute Resources
      { action: 'read', resource: 'compute', allowed: true },
      { action: 'write', resource: 'compute', allowed: true },
      { action: 'delete', resource: 'compute', allowed: false },
      { action: 'start', resource: 'compute', allowed: true },
      { action: 'stop', resource: 'compute', allowed: true },
      
      // Data Assets
      { action: 'read', resource: 'data', allowed: true },
      { action: 'write', resource: 'data', allowed: true },
      { action: 'delete', resource: 'data', allowed: false },
      { action: 'upload', resource: 'data', allowed: true },
      { action: 'download', resource: 'data', allowed: true },
      
      // Models
      { action: 'read', resource: 'models', allowed: true },
      { action: 'write', resource: 'models', allowed: true },
      { action: 'delete', resource: 'models', allowed: false },
      { action: 'register', resource: 'models', allowed: true },
      { action: 'deploy', resource: 'models', allowed: true },
      
      // Experiments/Jobs
      { action: 'read', resource: 'experiments', allowed: true },
      { action: 'write', resource: 'experiments', allowed: true },
      { action: 'delete', resource: 'experiments', allowed: false },
      { action: 'submit', resource: 'experiments', allowed: true },
      { action: 'cancel', resource: 'experiments', allowed: true },
      
      // Endpoints (Full access)
      { action: 'read', resource: 'endpoints', allowed: true },
      { action: 'write', resource: 'endpoints', allowed: true },
      { action: 'delete', resource: 'endpoints', allowed: true },
      { action: 'deploy', resource: 'endpoints', allowed: true },
      { action: 'test', resource: 'endpoints', allowed: true },
      
      // Security & Compliance (Read-only)
      { action: 'read', resource: 'security-settings', allowed: true },
      { action: 'write', resource: 'security-settings', allowed: false },
      { action: 'read', resource: 'audit-logs', allowed: true },
      { action: 'manage', resource: 'encryption', allowed: false },
      
      // Role Management (No access)
      { action: 'read', resource: 'role-assignments', allowed: false },
      { action: 'write', resource: 'role-assignments', allowed: false },
      { action: 'delete', resource: 'role-assignments', allowed: false },
    ],
    pimEligible: true,
    pimSettings: {
      roleName: 'Contributor',
      scope: '/subscriptions/{subscriptionId}/resourceGroups/{resourceGroup}',
      maxActivationDuration: 240, // 4 hours
      justificationRequired: true,
      approvalRequired: false,
    },
  },
};

/**
 * Get role configuration by role name
 */
export function getRoleConfiguration(roleName: string): RoleConfiguration | undefined {
  return roleConfigurations[roleName.toLowerCase().replace(/\s+/g, '')];
}

/**
 * Check if a role has permission for a specific action on a resource
 */
export function hasPermission(roleName: string, action: string, resource: string): boolean {
  const roleConfig = getRoleConfiguration(roleName);
  if (!roleConfig) {
    return false;
  }

  const permission = roleConfig.permissions.find(
    p => p.action === action && p.resource === resource
  );

  return permission?.allowed || false;
}

/**
 * Get all permissions for a role
 */
export function getRolePermissions(roleName: string): RolePermission[] {
  const roleConfig = getRoleConfiguration(roleName);
  return roleConfig?.permissions || [];
}

/**
 * Get allowed actions for a role on a specific resource
 */
export function getAllowedActions(roleName: string, resource: string): string[] {
  const permissions = getRolePermissions(roleName);
  return permissions
    .filter(p => p.resource === resource && p.allowed)
    .map(p => p.action);
}

/**
 * Get denied actions for a role on a specific resource
 */
export function getDeniedActions(roleName: string, resource: string): string[] {
  const permissions = getRolePermissions(roleName);
  return permissions
    .filter(p => p.resource === resource && !p.allowed)
    .map(p => p.action);
}

/**
 * Validate role configuration
 */
export function validateRoleConfiguration(roleName: string): { valid: boolean; errors: string[] } {
  const roleConfig = getRoleConfiguration(roleName);
  const errors: string[] = [];

  if (!roleConfig) {
    errors.push(`Role configuration not found: ${roleName}`);
    return { valid: false, errors };
  }

  // Check required fields
  if (!roleConfig.roleName) {
    errors.push('Role name is required');
  }

  if (!roleConfig.displayName) {
    errors.push('Display name is required');
  }

  if (!roleConfig.description) {
    errors.push('Description is required');
  }

  if (!roleConfig.permissions || roleConfig.permissions.length === 0) {
    errors.push('Permissions are required');
  }

  // Validate permissions
  roleConfig.permissions?.forEach((permission, index) => {
    if (!permission.action) {
      errors.push(`Permission ${index}: action is required`);
    }

    if (!permission.resource) {
      errors.push(`Permission ${index}: resource is required`);
    }

    if (typeof permission.allowed !== 'boolean') {
      errors.push(`Permission ${index}: allowed must be boolean`);
    }
  });

  // Validate PIM settings if present
  if (roleConfig.pimEligible && roleConfig.pimSettings) {
    if (!roleConfig.pimSettings.roleName) {
      errors.push('PIM role name is required when PIM is enabled');
    }

    if (!roleConfig.pimSettings.scope) {
      errors.push('PIM scope is required when PIM is enabled');
    }

    if (!roleConfig.pimSettings.maxActivationDuration || roleConfig.pimSettings.maxActivationDuration <= 0) {
      errors.push('PIM max activation duration must be positive');
    }
  }

  return { valid: errors.length === 0, errors };
}

/**
 * Get all available roles
 */
export function getAvailableRoles(): string[] {
  return Object.keys(roleConfigurations);
}

/**
 * Get role display names
 */
export function getRoleDisplayNames(): Record<string, string> {
  const displayNames: Record<string, string> = {};
  Object.entries(roleConfigurations).forEach(([key, config]) => {
    displayNames[key] = config.displayName;
  });
  return displayNames;
}