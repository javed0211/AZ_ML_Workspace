#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');
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

async function main() {
  console.log('🚀 Azure ML Workspace Automation Framework Setup\n');

  try {
    // Check prerequisites
    await checkPrerequisites();

    // Setup environment
    await setupEnvironment();

    // Install dependencies
    await installDependencies();

    // Setup directories
    await setupDirectories();

    // Validate configuration
    await validateConfiguration();

    console.log('\n✅ Setup completed successfully!');
    console.log('\nNext steps:');
    console.log('1. Review and update .env file with your Azure configuration');
    console.log('2. Run: npm run test:smoke');
    console.log('3. Check the README.md for detailed usage instructions');

  } catch (error) {
    console.error('\n❌ Setup failed:', error.message);
    process.exit(1);
  } finally {
    rl.close();
  }
}

async function checkPrerequisites() {
  console.log('📋 Checking prerequisites...');

  // Check Node.js version
  const nodeVersion = process.version;
  const majorVersion = parseInt(nodeVersion.slice(1).split('.')[0]);
  
  if (majorVersion < 18) {
    throw new Error(`Node.js 18+ required. Current version: ${nodeVersion}`);
  }
  console.log(`✓ Node.js ${nodeVersion}`);

  // Check npm
  try {
    const npmVersion = execSync('npm --version', { encoding: 'utf8' }).trim();
    console.log(`✓ npm ${npmVersion}`);
  } catch (error) {
    throw new Error('npm not found. Please install Node.js with npm.');
  }

  // Check Azure CLI
  try {
    const azVersion = execSync('az --version', { encoding: 'utf8' });
    const versionMatch = azVersion.match(/azure-cli\s+(\d+\.\d+\.\d+)/);
    if (versionMatch) {
      console.log(`✓ Azure CLI ${versionMatch[1]}`);
    } else {
      console.log('✓ Azure CLI (version unknown)');
    }
  } catch (error) {
    console.log('⚠️  Azure CLI not found. Please install it from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli');
  }

  // Check Git
  try {
    const gitVersion = execSync('git --version', { encoding: 'utf8' }).trim();
    console.log(`✓ ${gitVersion}`);
  } catch (error) {
    console.log('⚠️  Git not found. Some features may not work properly.');
  }

  console.log('');
}

async function setupEnvironment() {
  console.log('🔧 Setting up environment...');

  const envPath = path.join(process.cwd(), '.env');
  const envExamplePath = path.join(process.cwd(), '.env.example');

  if (!fs.existsSync(envPath)) {
    if (fs.existsSync(envExamplePath)) {
      fs.copyFileSync(envExamplePath, envPath);
      console.log('✓ Created .env file from .env.example');
    } else {
      // Create basic .env file
      const basicEnv = `# Azure Configuration
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_ML_WORKSPACE_NAME=your-workspace-name

# Environment
NODE_ENV=development
LOG_LEVEL=info
`;
      fs.writeFileSync(envPath, basicEnv);
      console.log('✓ Created basic .env file');
    }
  } else {
    console.log('✓ .env file already exists');
  }

  // Interactive configuration
  const shouldConfigure = await question('Would you like to configure Azure settings now? (y/n): ');
  
  if (shouldConfigure.toLowerCase() === 'y') {
    await configureAzureSettings(envPath);
  }

  console.log('');
}

async function configureAzureSettings(envPath) {
  console.log('\n🔐 Azure Configuration');
  console.log('You can find these values in the Azure Portal or by running: az account show');

  const tenantId = await question('Azure Tenant ID: ');
  const clientId = await question('Azure Client ID: ');
  const clientSecret = await question('Azure Client Secret (optional): ');
  const subscriptionId = await question('Azure Subscription ID: ');
  const resourceGroup = await question('Azure Resource Group: ');
  const workspaceName = await question('Azure ML Workspace Name: ');

  // Update .env file
  let envContent = fs.readFileSync(envPath, 'utf8');
  
  envContent = envContent.replace(/AZURE_TENANT_ID=.*/, `AZURE_TENANT_ID=${tenantId}`);
  envContent = envContent.replace(/AZURE_CLIENT_ID=.*/, `AZURE_CLIENT_ID=${clientId}`);
  if (clientSecret) {
    envContent = envContent.replace(/AZURE_CLIENT_SECRET=.*/, `AZURE_CLIENT_SECRET=${clientSecret}`);
  }
  envContent = envContent.replace(/AZURE_SUBSCRIPTION_ID=.*/, `AZURE_SUBSCRIPTION_ID=${subscriptionId}`);
  envContent = envContent.replace(/AZURE_RESOURCE_GROUP=.*/, `AZURE_RESOURCE_GROUP=${resourceGroup}`);
  envContent = envContent.replace(/AZURE_ML_WORKSPACE_NAME=.*/, `AZURE_ML_WORKSPACE_NAME=${workspaceName}`);

  fs.writeFileSync(envPath, envContent);
  console.log('✓ Azure configuration updated');
}

async function installDependencies() {
  console.log('📦 Installing dependencies...');

  try {
    console.log('Installing npm packages...');
    execSync('npm install', { stdio: 'inherit' });
    console.log('✓ npm packages installed');

    console.log('Installing Playwright browsers...');
    execSync('npx playwright install', { stdio: 'inherit' });
    console.log('✓ Playwright browsers installed');

    console.log('Installing Playwright system dependencies...');
    execSync('npx playwright install-deps', { stdio: 'inherit' });
    console.log('✓ Playwright system dependencies installed');

  } catch (error) {
    throw new Error(`Failed to install dependencies: ${error.message}`);
  }

  console.log('');
}

async function setupDirectories() {
  console.log('📁 Setting up directories...');

  const directories = [
    'test-results',
    'test-results/logs',
    'test-results/screenshots',
    'test-results/videos',
    'test-results/traces',
    'test-results/reports',
    'test-results/notebooks',
    'test-results/temp',
    '.vscode-test'
  ];

  for (const dir of directories) {
    const dirPath = path.join(process.cwd(), dir);
    if (!fs.existsSync(dirPath)) {
      fs.mkdirSync(dirPath, { recursive: true });
      console.log(`✓ Created directory: ${dir}`);
    }
  }

  console.log('');
}

async function validateConfiguration() {
  console.log('✅ Validating configuration...');

  try {
    // Try to load and validate the configuration
    const { config } = require('../src/helpers/config');
    
    console.log('✓ Configuration loaded successfully');
    
    // Check if required Azure settings are present
    const requiredSettings = [
      'azure.tenantId',
      'azure.clientId',
      'azure.subscriptionId',
      'azure.resourceGroup',
      'azure.workspaceName'
    ];

    const missingSettings = [];
    for (const setting of requiredSettings) {
      const value = setting.split('.').reduce((obj, key) => obj?.[key], config);
      if (!value || value.startsWith('your-')) {
        missingSettings.push(setting);
      }
    }

    if (missingSettings.length > 0) {
      console.log('⚠️  Missing or placeholder values for:');
      missingSettings.forEach(setting => console.log(`   - ${setting}`));
      console.log('   Please update your .env file with actual values');
    } else {
      console.log('✓ All required settings configured');
    }

  } catch (error) {
    console.log(`⚠️  Configuration validation failed: ${error.message}`);
    console.log('   Please check your .env file and fix any issues');
  }

  console.log('');
}

// Run setup if this script is executed directly
if (require.main === module) {
  main().catch(console.error);
}

module.exports = { main };