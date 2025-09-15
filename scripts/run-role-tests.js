#!/usr/bin/env node

/**
 * Role-based test runner
 * Executes tests with different user roles and credentials
 */

const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');
const readline = require('readline');

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

function question(prompt) {
  return new Promise((resolve) => {
    rl.question(prompt, resolve);
  });
}

// Role configurations for testing
const roleConfigurations = {
  owner: {
    name: 'Owner',
    description: 'Full workspace access including role management',
    testTags: '@roles @owner',
    envOverrides: {
      TEST_ROLE: 'owner',
      TEST_ROLE_DESCRIPTION: 'Owner role testing',
    }
  },
  contributor: {
    name: 'Contributor', 
    description: 'Full resource access but no role management',
    testTags: '@roles @contributor',
    envOverrides: {
      TEST_ROLE: 'contributor',
      TEST_ROLE_DESCRIPTION: 'Contributor role testing',
    }
  },
  reader: {
    name: 'Reader',
    description: 'Read-only access to all resources',
    testTags: '@roles @reader',
    envOverrides: {
      TEST_ROLE: 'reader',
      TEST_ROLE_DESCRIPTION: 'Reader role testing',
    }
  },
  dataScientist: {
    name: 'Data Scientist',
    description: 'ML experiment and model management access',
    testTags: '@roles @data-scientist',
    envOverrides: {
      TEST_ROLE: 'dataScientist',
      TEST_ROLE_DESCRIPTION: 'Data Scientist role testing',
    }
  },
  computeOperator: {
    name: 'Compute Operator',
    description: 'Compute resource management access',
    testTags: '@roles @compute-operator',
    envOverrides: {
      TEST_ROLE: 'computeOperator',
      TEST_ROLE_DESCRIPTION: 'Compute Operator role testing',
    }
  },
  pim: {
    name: 'PIM Integration',
    description: 'Privileged Identity Management testing',
    testTags: '@roles @pim',
    envOverrides: {
      TEST_ROLE: 'pim',
      TEST_ROLE_DESCRIPTION: 'PIM integration testing',
      PIM_ENABLED: 'true',
    }
  }
};

async function main() {
  console.log('🔐 Azure ML Role-Based Access Control Test Runner\n');

  try {
    // Parse command line arguments
    const args = process.argv.slice(2);
    const options = parseArguments(args);

    if (options.help) {
      showHelp();
      return;
    }

    if (options.listRoles) {
      listAvailableRoles();
      return;
    }

    // Interactive mode if no role specified
    if (!options.role && !options.all) {
      options.role = await selectRole();
    }

    // Validate environment
    await validateEnvironment();

    // Setup test environment
    await setupTestEnvironment(options);

    // Run tests
    if (options.all) {
      await runAllRoleTests(options);
    } else {
      await runRoleTests(options.role, options);
    }

    console.log('\n✅ Role-based testing completed successfully!');

  } catch (error) {
    console.error('\n❌ Role-based testing failed:', error.message);
    process.exit(1);
  } finally {
    rl.close();
  }
}

function parseArguments(args) {
  const options = {
    role: null,
    all: false,
    parallel: false,
    headed: false,
    debug: false,
    report: true,
    help: false,
    listRoles: false,
    credentialsFile: null,
    outputDir: './test-results/role-tests',
  };

  for (let i = 0; i < args.length; i++) {
    const arg = args[i];
    
    switch (arg) {
      case '--role':
      case '-r':
        options.role = args[++i];
        break;
      case '--all':
      case '-a':
        options.all = true;
        break;
      case '--parallel':
      case '-p':
        options.parallel = true;
        break;
      case '--headed':
        options.headed = true;
        break;
      case '--debug':
      case '-d':
        options.debug = true;
        break;
      case '--no-report':
        options.report = false;
        break;
      case '--credentials':
      case '-c':
        options.credentialsFile = args[++i];
        break;
      case '--output':
      case '-o':
        options.outputDir = args[++i];
        break;
      case '--list-roles':
      case '-l':
        options.listRoles = true;
        break;
      case '--help':
      case '-h':
        options.help = true;
        break;
      default:
        if (arg.startsWith('--')) {
          console.warn(`Unknown option: ${arg}`);
        }
    }
  }

  return options;
}

function showHelp() {
  console.log(`
Azure ML Role-Based Access Control Test Runner

Usage: node run-role-tests.js [options]

Options:
  -r, --role <role>        Run tests for specific role (owner, contributor, reader, dataScientist, computeOperator, pim)
  -a, --all               Run tests for all roles
  -p, --parallel          Run tests in parallel (faster but uses more resources)
  --headed                Run tests in headed mode (visible browser)
  -d, --debug             Enable debug mode with verbose logging
  --no-report             Skip generating HTML report
  -c, --credentials <file> Use custom credentials file
  -o, --output <dir>      Output directory for test results
  -l, --list-roles        List available roles and exit
  -h, --help              Show this help message

Examples:
  node run-role-tests.js --role owner
  node run-role-tests.js --all --parallel
  node run-role-tests.js --role dataScientist --headed --debug
  node run-role-tests.js --list-roles

Environment Variables:
  AZURE_TENANT_ID         Azure tenant ID
  AZURE_CLIENT_ID         Azure client ID (service principal)
  AZURE_CLIENT_SECRET     Azure client secret
  AZURE_SUBSCRIPTION_ID   Azure subscription ID
  AZURE_RESOURCE_GROUP    Azure resource group name
  AZURE_ML_WORKSPACE_NAME Azure ML workspace name
  
Role-specific credentials can be provided via:
  OWNER_CLIENT_ID, OWNER_CLIENT_SECRET
  CONTRIBUTOR_CLIENT_ID, CONTRIBUTOR_CLIENT_SECRET
  READER_CLIENT_ID, READER_CLIENT_SECRET
  etc.
`);
}

function listAvailableRoles() {
  console.log('Available roles for testing:\n');
  
  Object.entries(roleConfigurations).forEach(([key, config]) => {
    console.log(`  ${key.padEnd(15)} - ${config.name}`);
    console.log(`  ${' '.repeat(15)}   ${config.description}`);
    console.log(`  ${' '.repeat(15)}   Tags: ${config.testTags}`);
    console.log('');
  });
}

async function selectRole() {
  console.log('Available roles:');
  Object.entries(roleConfigurations).forEach(([key, config], index) => {
    console.log(`  ${index + 1}. ${config.name} - ${config.description}`);
  });
  console.log(`  ${Object.keys(roleConfigurations).length + 1}. All roles`);

  const choice = await question('\nSelect a role to test (number): ');
  const roleKeys = Object.keys(roleConfigurations);
  const choiceIndex = parseInt(choice) - 1;

  if (choiceIndex === roleKeys.length) {
    return 'all';
  } else if (choiceIndex >= 0 && choiceIndex < roleKeys.length) {
    return roleKeys[choiceIndex];
  } else {
    throw new Error('Invalid role selection');
  }
}

async function validateEnvironment() {
  console.log('🔍 Validating environment...');

  // Check required environment variables
  const requiredEnvVars = [
    'AZURE_TENANT_ID',
    'AZURE_SUBSCRIPTION_ID',
    'AZURE_RESOURCE_GROUP',
    'AZURE_ML_WORKSPACE_NAME'
  ];

  const missingVars = requiredEnvVars.filter(varName => !process.env[varName]);
  
  if (missingVars.length > 0) {
    throw new Error(`Missing required environment variables: ${missingVars.join(', ')}`);
  }

  // Check if Playwright is installed
  try {
    execSync('npx playwright --version', { stdio: 'pipe' });
    console.log('✓ Playwright is installed');
  } catch (error) {
    throw new Error('Playwright is not installed. Run: npm run install:browsers');
  }

  // Check if Azure CLI is available
  try {
    execSync('az --version', { stdio: 'pipe' });
    console.log('✓ Azure CLI is available');
  } catch (error) {
    console.warn('⚠️  Azure CLI not found. Some tests may fail.');
  }

  console.log('✓ Environment validation completed\n');
}

async function setupTestEnvironment(options) {
  console.log('🔧 Setting up test environment...');

  // Create output directory
  if (!fs.existsSync(options.outputDir)) {
    fs.mkdirSync(options.outputDir, { recursive: true });
    console.log(`✓ Created output directory: ${options.outputDir}`);
  }

  // Create role-specific directories
  Object.keys(roleConfigurations).forEach(role => {
    const roleDir = path.join(options.outputDir, role);
    if (!fs.existsSync(roleDir)) {
      fs.mkdirSync(roleDir, { recursive: true });
    }
  });

  console.log('✓ Test environment setup completed\n');
}

async function runAllRoleTests(options) {
  console.log('🚀 Running tests for all roles...\n');

  const roles = Object.keys(roleConfigurations);
  const results = {};

  if (options.parallel) {
    console.log('Running tests in parallel...');
    
    // Run all roles in parallel
    const promises = roles.map(role => 
      runRoleTestsInternal(role, options).catch(error => ({ role, error }))
    );
    
    const parallelResults = await Promise.all(promises);
    
    parallelResults.forEach(result => {
      if (result.error) {
        results[result.role] = { success: false, error: result.error };
      } else {
        results[result.role] = { success: true };
      }
    });
    
  } else {
    console.log('Running tests sequentially...');
    
    // Run roles sequentially
    for (const role of roles) {
      try {
        console.log(`\n--- Testing role: ${roleConfigurations[role].name} ---`);
        await runRoleTestsInternal(role, options);
        results[role] = { success: true };
        console.log(`✅ ${roleConfigurations[role].name} tests completed`);
      } catch (error) {
        results[role] = { success: false, error };
        console.error(`❌ ${roleConfigurations[role].name} tests failed:`, error.message);
      }
    }
  }

  // Print summary
  console.log('\n📊 Test Results Summary:');
  Object.entries(results).forEach(([role, result]) => {
    const status = result.success ? '✅' : '❌';
    console.log(`  ${status} ${roleConfigurations[role].name}`);
    if (!result.success) {
      console.log(`      Error: ${result.error.message}`);
    }
  });

  // Generate combined report
  if (options.report) {
    await generateCombinedReport(options.outputDir, results);
  }
}

async function runRoleTests(role, options) {
  if (role === 'all') {
    return runAllRoleTests(options);
  }

  if (!roleConfigurations[role]) {
    throw new Error(`Unknown role: ${role}. Use --list-roles to see available roles.`);
  }

  console.log(`🚀 Running tests for role: ${roleConfigurations[role].name}\n`);
  
  await runRoleTestsInternal(role, options);
  
  if (options.report) {
    await generateRoleReport(role, options.outputDir);
  }
}

async function runRoleTestsInternal(role, options) {
  const roleConfig = roleConfigurations[role];
  
  // Prepare environment variables
  const testEnv = {
    ...process.env,
    ...roleConfig.envOverrides,
    TEST_OUTPUT_DIR: path.join(options.outputDir, role),
  };

  // Add role-specific credentials if available
  const roleCredentials = getRoleCredentials(role, options.credentialsFile);
  Object.assign(testEnv, roleCredentials);

  // Build Playwright command
  const playwrightArgs = [
    'npx', 'playwright', 'test',
    '--grep', roleConfig.testTags,
    '--output-dir', path.join(options.outputDir, role),
  ];

  if (options.headed) {
    playwrightArgs.push('--headed');
  }

  if (options.debug) {
    playwrightArgs.push('--debug');
  }

  if (!options.parallel) {
    playwrightArgs.push('--workers', '1');
  }

  // Add reporter
  playwrightArgs.push('--reporter', 'html,json,junit');

  console.log(`Executing: ${playwrightArgs.join(' ')}`);
  
  try {
    execSync(playwrightArgs.join(' '), {
      stdio: 'inherit',
      env: testEnv,
      cwd: process.cwd(),
    });
  } catch (error) {
    throw new Error(`Test execution failed for role ${role}: ${error.message}`);
  }
}

function getRoleCredentials(role, credentialsFile) {
  const credentials = {};

  // Try to load from credentials file first
  if (credentialsFile && fs.existsSync(credentialsFile)) {
    try {
      const fileCredentials = JSON.parse(fs.readFileSync(credentialsFile, 'utf8'));
      if (fileCredentials[role]) {
        Object.assign(credentials, fileCredentials[role]);
      }
    } catch (error) {
      console.warn(`Failed to load credentials from file: ${error.message}`);
    }
  }

  // Try to load from environment variables
  const roleUpper = role.toUpperCase();
  const envCredentials = {
    [`${roleUpper}_CLIENT_ID`]: process.env[`${roleUpper}_CLIENT_ID`],
    [`${roleUpper}_CLIENT_SECRET`]: process.env[`${roleUpper}_CLIENT_SECRET`],
    [`${roleUpper}_USERNAME`]: process.env[`${roleUpper}_USERNAME`],
    [`${roleUpper}_PASSWORD`]: process.env[`${roleUpper}_PASSWORD`],
  };

  // Filter out undefined values
  Object.entries(envCredentials).forEach(([key, value]) => {
    if (value) {
      credentials[key] = value;
    }
  });

  return credentials;
}

async function generateRoleReport(role, outputDir) {
  console.log(`📊 Generating report for role: ${roleConfigurations[role].name}`);
  
  const roleDir = path.join(outputDir, role);
  const reportPath = path.join(roleDir, 'index.html');
  
  if (fs.existsSync(reportPath)) {
    console.log(`✓ Report generated: ${reportPath}`);
  } else {
    console.warn(`⚠️  Report not found: ${reportPath}`);
  }
}

async function generateCombinedReport(outputDir, results) {
  console.log('📊 Generating combined report...');
  
  const reportHtml = `
<!DOCTYPE html>
<html>
<head>
    <title>Azure ML Role-Based Access Control Test Report</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .header { background: #0078d4; color: white; padding: 20px; border-radius: 5px; }
        .summary { margin: 20px 0; }
        .role-result { margin: 10px 0; padding: 15px; border-radius: 5px; }
        .success { background: #d4edda; border: 1px solid #c3e6cb; }
        .failure { background: #f8d7da; border: 1px solid #f5c6cb; }
        .role-name { font-weight: bold; font-size: 1.1em; }
        .error-message { color: #721c24; margin-top: 10px; font-family: monospace; }
        .timestamp { color: #666; font-size: 0.9em; }
    </style>
</head>
<body>
    <div class="header">
        <h1>Azure ML Role-Based Access Control Test Report</h1>
        <p class="timestamp">Generated: ${new Date().toISOString()}</p>
    </div>
    
    <div class="summary">
        <h2>Test Summary</h2>
        <p>Total roles tested: ${Object.keys(results).length}</p>
        <p>Successful: ${Object.values(results).filter(r => r.success).length}</p>
        <p>Failed: ${Object.values(results).filter(r => !r.success).length}</p>
    </div>
    
    <div class="results">
        <h2>Role Test Results</h2>
        ${Object.entries(results).map(([role, result]) => `
            <div class="role-result ${result.success ? 'success' : 'failure'}">
                <div class="role-name">${roleConfigurations[role].name}</div>
                <div>${roleConfigurations[role].description}</div>
                ${result.success ? 
                    '<div style="color: #155724;">✅ All tests passed</div>' : 
                    `<div style="color: #721c24;">❌ Tests failed</div>
                     <div class="error-message">${result.error.message}</div>`
                }
                <div><a href="${role}/index.html">View detailed report</a></div>
            </div>
        `).join('')}
    </div>
</body>
</html>
`;

  const combinedReportPath = path.join(outputDir, 'index.html');
  fs.writeFileSync(combinedReportPath, reportHtml);
  
  console.log(`✓ Combined report generated: ${combinedReportPath}`);
}

// Run the script if executed directly
if (require.main === module) {
  main().catch(console.error);
}

module.exports = { main, roleConfigurations };