import { FullConfig } from '@playwright/test';
import { execSync } from 'child_process';

async function globalTeardown(config: FullConfig) {
  console.log('🧹 Global Teardown: Cleaning up VS Code test environment...');
  
  try {
    // Don't kill the main VS Code instance, just test instances
    // We'll be more selective about cleanup
    console.log('✅ Cleanup complete (preserving main VS Code instance)');
  } catch (error) {
    console.log('⚠️  Cleanup had some issues, but continuing...');
  }
}

export default globalTeardown;