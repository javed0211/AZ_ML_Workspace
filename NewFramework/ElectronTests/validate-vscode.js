#!/usr/bin/env node

/**
 * VS Code Installation Validator
 * Run this script to check if VS Code is properly installed and configured
 */

const { validateVSCodeInstallation, getVSCodeExecutablePath } = require('./dist/config/vscode-config');
const fs = require('fs');
const path = require('path');

console.log('🔍 VS Code Installation Validator\n');

try {
  // Check if the config is compiled
  const configPath = path.join(__dirname, 'dist', 'config', 'vscode-config.js');
  if (!fs.existsSync(configPath)) {
    console.log('⚠️  TypeScript not compiled yet. Compiling...');
    const { execSync } = require('child_process');
    execSync('npm run build', { stdio: 'inherit', cwd: __dirname });
  }

  // Get the expected executable path
  const executablePath = getVSCodeExecutablePath();
  console.log(`🎯 Expected VS Code executable: ${executablePath}`);

  // Validate installation
  const validation = validateVSCodeInstallation();
  
  if (validation.isValid) {
    console.log('✅ VS Code installation is valid!');
    console.log(`📍 Found at: ${executablePath}`);
    
    // Try to get version info
    try {
      const { execSync } = require('child_process');
      const version = execSync(`"${executablePath}" --version`, { encoding: 'utf8', timeout: 5000 });
      console.log('📋 Version info:');
      console.log(version.trim());
    } catch (versionError) {
      console.log('⚠️  Could not get version info, but executable exists');
    }
    
    console.log('\n🚀 Ready to run Electron tests!');
    console.log('Run: npm test');
    
  } else {
    console.log('❌ VS Code installation not found');
    console.log('\n📋 Installation Instructions:');
    console.log(validation.message);
    
    // Check if Homebrew installation is in progress
    try {
      const { execSync } = require('child_process');
      const brewProcesses = execSync('ps aux | grep "visual-studio-code" | grep -v grep', { encoding: 'utf8' });
      if (brewProcesses.trim()) {
        console.log('\n⏳ Homebrew installation appears to be in progress...');
        console.log('Please wait for the installation to complete and run this script again.');
      }
    } catch (e) {
      // No brew processes found
    }
    
    process.exit(1);
  }

} catch (error) {
  console.error('❌ Error during validation:', error.message);
  process.exit(1);
}