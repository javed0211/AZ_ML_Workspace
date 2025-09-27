const { spawn, exec } = require('child_process');
const path = require('path');

async function safeLaunchVSCode() {
    console.log('🚀 Safely launching VS Code test instance...');
    
    // Path to the VS Code downloaded by the official framework
    const vscodeAppPath = '/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/ElectronTests/.vscode-test/vscode-darwin-arm64-1.104.2/Visual Studio Code.app';
    
    console.log('📍 VS Code path:', vscodeAppPath);
    
    // Step 1: Check current VS Code processes (but don't kill them)
    console.log('🔍 Step 1: Checking existing VS Code processes...');
    exec('ps aux | grep -i "visual studio code\\|electron" | grep -v grep', (error, stdout) => {
        if (stdout) {
            console.log('📊 Current VS Code processes:');
            console.log(stdout);
        }
        
        // Step 2: Launch VS Code with specific user data directory to avoid conflicts
        console.log('🎯 Step 2: Launching VS Code with isolated user data...');
        
        // Create a unique user data directory for testing
        const testUserDataDir = path.join(__dirname, '.test-user-data');
        
        // Launch VS Code with isolated settings and new instance
        const launchArgs = [
            '-n', // New instance
            '-W', // Wait for application to exit
            '--args',
            '--user-data-dir=' + testUserDataDir,
            '--disable-extensions',
            '--disable-workspace-trust',
            '--new-window'
        ];
        
        const launchProcess = spawn('open', ['-a', vscodeAppPath].concat(launchArgs), {
            stdio: 'pipe',
            detached: false
        });
        
        launchProcess.stdout.on('data', (data) => {
            console.log('📤 Launch output:', data.toString());
        });
        
        launchProcess.stderr.on('data', (data) => {
            console.log('⚠️  Launch stderr:', data.toString());
        });
        
        launchProcess.on('exit', (code, signal) => {
            console.log(`📱 Launch process exited with code: ${code}, signal: ${signal}`);
            
            if (signal === 'SIGTERM') {
                console.log('⚠️  Process was terminated - this might be expected');
            }
            
            // Step 3: Use AppleScript to activate the new instance
            setTimeout(() => {
                console.log('🎯 Step 3: Using AppleScript to activate test instance...');
                
                // More specific AppleScript that targets the test instance
                const appleScript = `
                    tell application "System Events"
                        set vscodeProcesses to every process whose name contains "Electron"
                        repeat with proc in vscodeProcesses
                            try
                                set frontmost of proc to true
                                delay 0.5
                                exit repeat
                            end try
                        end repeat
                    end tell
                `;
                
                exec(`osascript -e '${appleScript}'`, (err, stdout, stderr) => {
                    if (err) {
                        console.log('⚠️  AppleScript activation failed:', err.message);
                    } else {
                        console.log('✅ AppleScript activation attempted');
                    }
                    
                    // Final status check
                    setTimeout(checkSafeStatus, 2000);
                });
            }, 2000);
        });
        
        launchProcess.on('error', (error) => {
            console.error('❌ Launch failed:', error);
        });
        
        // Don't let the process hang indefinitely
        setTimeout(() => {
            if (!launchProcess.killed) {
                console.log('⏰ Launch taking too long, continuing...');
                checkSafeStatus();
            }
        }, 10000);
    });
}

function checkSafeStatus() {
    console.log('\n📊 Safe Status Check:');
    
    // Check all VS Code processes
    exec('ps aux | grep -i "visual studio code\\|electron" | grep -v grep', (error, stdout) => {
        if (stdout) {
            const lines = stdout.trim().split('\n');
            console.log(`✅ Total VS Code/Electron processes: ${lines.length}`);
            
            // Show process details
            lines.forEach((line, index) => {
                const parts = line.split(/\s+/);
                const pid = parts[1];
                const command = parts.slice(10).join(' ');
                console.log(`   ${index + 1}. PID ${pid}: ${command.substring(0, 80)}...`);
            });
        } else {
            console.log('❌ No VS Code processes found');
        }
    });
    
    // Check foreground app
    exec('osascript -e \'tell application "System Events" to get name of first application process whose frontmost is true\'', (error, stdout) => {
        console.log(`🎯 Current foreground app: ${stdout.trim()}`);
    });
    
    // Check for VS Code windows
    exec('osascript -e \'tell application "System Events" to count windows of every process whose name contains "Electron"\'', (error, stdout) => {
        if (stdout) {
            const windowCount = stdout.trim();
            console.log(`🪟 VS Code windows detected: ${windowCount}`);
        }
    });
    
    console.log('\n💡 If VS Code test instance is not visible:');
    console.log('   1. Check your dock for VS Code icons');
    console.log('   2. Use Cmd+Tab to switch between applications');
    console.log('   3. The test instance should have isolated settings');
}

// Alternative method: Launch without waiting
function quickLaunch() {
    console.log('⚡ Quick launch method...');
    
    const vscodeAppPath = '/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/ElectronTests/.vscode-test/vscode-darwin-arm64-1.104.2/Visual Studio Code.app';
    const testUserDataDir = path.join(__dirname, '.test-user-data');
    
    // Use direct executable path for more control
    const executablePath = path.join(vscodeAppPath, 'Contents/MacOS/Electron');
    
    const vscodeProcess = spawn(executablePath, [
        '--user-data-dir=' + testUserDataDir,
        '--disable-extensions',
        '--disable-workspace-trust',
        '--new-window'
    ], {
        stdio: 'ignore',
        detached: true
    });
    
    vscodeProcess.unref(); // Don't wait for it
    
    console.log(`🚀 VS Code launched with PID: ${vscodeProcess.pid}`);
    
    // Try to activate after a short delay
    setTimeout(() => {
        const activateScript = `
            tell application "System Events"
                set frontmost of first process whose name contains "Electron" to true
            end tell
        `;
        
        exec(`osascript -e '${activateScript}'`, () => {
            console.log('✅ Activation attempted');
            setTimeout(checkSafeStatus, 1000);
        });
    }, 3000);
}

// Run the safe launcher
console.log('🛡️  Using safe VS Code launcher to avoid conflicts...');
safeLaunchVSCode();

// Also provide quick launch option
setTimeout(() => {
    console.log('\n🔄 Trying alternative quick launch...');
    quickLaunch();
}, 15000);