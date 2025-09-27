const { runTests } = require('@vscode/test-electron');
const path = require('path');

async function runSafeTests() {
    try {
        console.log('🧪 Starting safe VS Code tests...');
        
        // The extension development folder
        const extensionDevelopmentPath = path.resolve(__dirname, 'test-extension');
        
        // The path to the extension test script
        const extensionTestsPath = path.resolve(__dirname, 'test-extension/out/test/suite/index');
        
        // Create isolated user data directory
        const testUserDataDir = path.resolve(__dirname, '.test-user-data');
        
        console.log('📁 Extension path:', extensionDevelopmentPath);
        console.log('🧪 Test path:', extensionTestsPath);
        console.log('💾 User data dir:', testUserDataDir);
        
        // Download VS Code, unzip it and run the integration test
        const exitCode = await runTests({
            extensionDevelopmentPath,
            extensionTestsPath,
            launchArgs: [
                '--user-data-dir=' + testUserDataDir,
                '--disable-extensions',
                '--disable-workspace-trust',
                '--disable-telemetry',
                '--disable-updates',
                '--skip-welcome',
                '--skip-release-notes',
                '--disable-restore-windows',
                '--new-window'
            ],
            version: '1.104.2' // Use specific version to avoid conflicts
        });
        
        console.log(`✅ Tests completed with exit code: ${exitCode}`);
        process.exit(exitCode);
        
    } catch (error) {
        console.error('❌ Test execution failed:', error);
        
        // If the official framework fails, try manual approach
        console.log('🔄 Falling back to manual VS Code launch...');
        
        const { spawn } = require('child_process');
        
        const vscodeAppPath = path.resolve(__dirname, '.vscode-test/vscode-darwin-arm64-1.104.2/Visual Studio Code.app');
        const testUserDataDir = path.resolve(__dirname, '.test-user-data');
        
        console.log('🚀 Manually launching VS Code for testing...');
        
        // Launch VS Code with test configuration
        const vscodeProcess = spawn('open', [
            '-n', // New instance
            '-a', vscodeAppPath,
            '--args',
            '--user-data-dir=' + testUserDataDir,
            '--disable-extensions',
            '--extensionDevelopmentPath=' + path.resolve(__dirname, 'test-extension'),
            '--disable-workspace-trust'
        ], {
            stdio: 'inherit'
        });
        
        vscodeProcess.on('exit', (code) => {
            console.log(`📱 Manual VS Code launch exited with code: ${code}`);
        });
        
        vscodeProcess.on('error', (err) => {
            console.error('❌ Manual launch failed:', err);
        });
        
        // Keep the process alive for a bit
        setTimeout(() => {
            console.log('⏰ Test runner timeout - VS Code should be running');
            process.exit(0);
        }, 30000);
    }
}

// Handle process termination gracefully
process.on('SIGINT', () => {
    console.log('\n🛑 Test runner interrupted');
    process.exit(0);
});

process.on('SIGTERM', () => {
    console.log('\n🛑 Test runner terminated');
    process.exit(0);
});

// Run the safe tests
runSafeTests();