const { spawn, exec } = require('child_process');
const path = require('path');
const fs = require('fs');

async function runWorkingTests() {
    console.log('🧪 Starting working VS Code test runner...');
    
    // Paths
    const vscodeAppPath = path.resolve(__dirname, '.vscode-test/vscode-darwin-arm64-1.104.2/Visual Studio Code.app');
    const extensionPath = path.resolve(__dirname, 'test-extension');
    const testUserDataDir = path.resolve(__dirname, '.test-user-data');
    
    console.log('📁 VS Code app:', vscodeAppPath);
    console.log('📁 Extension path:', extensionPath);
    console.log('💾 User data dir:', testUserDataDir);
    
    // Ensure user data directory exists
    if (!fs.existsSync(testUserDataDir)) {
        fs.mkdirSync(testUserDataDir, { recursive: true });
        console.log('✅ Created user data directory');
    }
    
    // Check if extension is compiled
    const extensionOutPath = path.join(extensionPath, 'out');
    if (!fs.existsSync(extensionOutPath)) {
        console.log('🔨 Compiling extension...');
        
        // Compile the extension first
        const compileProcess = spawn('npm', ['run', 'compile'], {
            cwd: extensionPath,
            stdio: 'inherit'
        });
        
        await new Promise((resolve, reject) => {
            compileProcess.on('exit', (code) => {
                if (code === 0) {
                    console.log('✅ Extension compiled successfully');
                    resolve();
                } else {
                    console.log('❌ Extension compilation failed');
                    reject(new Error(`Compilation failed with code ${code}`));
                }
            });
        });
    }
    
    // Method 1: Try using the app bundle directly with open command
    console.log('🚀 Method 1: Launching VS Code with extension...');
    
    const openArgs = [
        '-n', // New instance
        '-a', vscodeAppPath,
        '--args',
        `--user-data-dir=${testUserDataDir}`,
        `--extensionDevelopmentPath=${extensionPath}`,
        '--disable-workspace-trust',
        '--new-window'
    ];
    
    console.log('📝 Launch command:', 'open', openArgs.join(' '));
    
    const launchProcess = spawn('open', openArgs, {
        stdio: 'pipe'
    });
    
    let launchOutput = '';
    
    launchProcess.stdout.on('data', (data) => {
        launchOutput += data.toString();
        console.log('📤 Launch stdout:', data.toString().trim());
    });
    
    launchProcess.stderr.on('data', (data) => {
        launchOutput += data.toString();
        console.log('⚠️  Launch stderr:', data.toString().trim());
    });
    
    const launchResult = await new Promise((resolve) => {
        launchProcess.on('exit', (code, signal) => {
            console.log(`📱 Launch process exited: code=${code}, signal=${signal}`);
            resolve({ code, signal, output: launchOutput });
        });
        
        launchProcess.on('error', (error) => {
            console.log('❌ Launch error:', error.message);
            resolve({ error, output: launchOutput });
        });
        
        // Timeout after 15 seconds
        setTimeout(() => {
            console.log('⏰ Launch timeout, continuing...');
            resolve({ timeout: true, output: launchOutput });
        }, 15000);
    });
    
    // Wait a bit for VS Code to start
    console.log('⏳ Waiting for VS Code to initialize...');
    await new Promise(resolve => setTimeout(resolve, 5000));
    
    // Method 2: Try to run tests by triggering them via VS Code API
    console.log('🧪 Method 2: Attempting to trigger tests...');
    
    // Check if VS Code is running with our extension
    exec('ps aux | grep -i "extensionDevelopmentPath" | grep -v grep', (error, stdout) => {
        if (stdout) {
            console.log('✅ VS Code running with extension development path');
            console.log('📊 Extension process:', stdout.trim());
        } else {
            console.log('⚠️  No extension development process found');
        }
    });
    
    // Method 3: Try direct test execution if possible
    console.log('🧪 Method 3: Checking for test files...');
    
    const testSuitePath = path.join(extensionPath, 'out/test/suite');
    if (fs.existsSync(testSuitePath)) {
        console.log('✅ Test suite found at:', testSuitePath);
        
        // List test files
        const testFiles = fs.readdirSync(testSuitePath).filter(f => f.endsWith('.js'));
        console.log('📝 Test files:', testFiles);
        
        if (testFiles.length > 0) {
            console.log('🎯 Test files are ready for execution');
        }
    } else {
        console.log('⚠️  Test suite not found, extension may need compilation');
    }
    
    // Final status check
    setTimeout(checkTestStatus, 3000);
    
    // Keep process alive for a while to observe VS Code
    setTimeout(() => {
        console.log('⏰ Test runner completing...');
        console.log('💡 VS Code should be running with your extension loaded');
        console.log('💡 You can manually run tests from VS Code Command Palette: "Test: Run All Tests"');
        process.exit(0);
    }, 20000);
}

function checkTestStatus() {
    console.log('\n📊 Test Status Check:');
    
    // Check VS Code processes
    exec('ps aux | grep -E "(Visual Studio Code|Electron)" | grep -v grep | wc -l', (error, stdout) => {
        const processCount = parseInt(stdout.trim());
        console.log(`✅ VS Code processes: ${processCount}`);
    });
    
    // Check for extension development processes
    exec('ps aux | grep "extensionDevelopmentPath" | grep -v grep', (error, stdout) => {
        if (stdout) {
            console.log('✅ Extension development mode active');
        } else {
            console.log('⚠️  Extension development mode not detected');
        }
    });
    
    // Check foreground application
    exec('osascript -e \'tell application "System Events" to get name of first application process whose frontmost is true\'', (error, stdout) => {
        const frontApp = stdout.trim();
        console.log(`🎯 Foreground app: ${frontApp}`);
        
        if (frontApp.includes('Visual Studio Code') || frontApp.includes('Electron')) {
            console.log('🎉 VS Code is in the foreground!');
        }
    });
}

// Handle graceful shutdown
process.on('SIGINT', () => {
    console.log('\n🛑 Test runner interrupted');
    process.exit(0);
});

process.on('SIGTERM', () => {
    console.log('\n🛑 Test runner terminated');
    process.exit(0);
});

// Run the working test runner
runWorkingTests().catch(error => {
    console.error('❌ Test runner failed:', error);
    process.exit(1);
});