import { FullConfig } from '@playwright/test';
import { execSync } from 'child_process';
import path from 'path';

async function globalSetup(config: FullConfig) {
  console.log('🚀 Global Setup: Preparing VS Code test environment...');
  
  // Ensure code command is available
  const codePath = '/Users/oldguard/.local/bin/code';
  process.env.PATH = `/Users/oldguard/.local/bin:${process.env.PATH}`;
  
  try {
    // Test if code command works
    execSync('code --version', { stdio: 'pipe' });
    console.log('✅ VS Code CLI is available');
  } catch (error) {
    console.log('❌ VS Code CLI not available, tests may fail');
  }
  
  // Create temp directory for test workspaces
  const tempDir = path.join(__dirname, '..', 'temp');
  try {
    execSync(`mkdir -p "${tempDir}"`, { stdio: 'pipe' });
    console.log('✅ Temp directory created');
  } catch (error) {
    console.log('⚠️  Could not create temp directory');
  }
  
  console.log('✅ Global setup complete');
}

export default globalSetup;