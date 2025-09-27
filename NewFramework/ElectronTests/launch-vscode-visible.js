const { spawn, exec } = require('child_process');
const path = require('path');

async function launchVSCodeVisible() {
    // Path to the VS Code downloaded by the official framework
    const vscodeAppPath = '/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/ElectronTests/.vscode-test/vscode-darwin-arm64-1.104.2/Visual Studio Code.app';
    
    console.log('🚀 Launching VS Code with visibility...');
    console.log('📍 VS Code path:', vscodeAppPath);
    
    try {
        // First, check if VS Code is already running
        exec('ps aux | grep -i "visual studio code" | grep -v grep', (error, stdout, stderr) => {
            if (stdout) {
                console.log('⚠️  VS Code is already running. Bringing to front...');
                // Bring VS Code to front if already running
                exec('osascript -e \'tell application "Visual Studio Code" to activate\'', (err) => {
                    if (err) {
                        console.log('Could not bring VS Code to front, launching new instance...');
                        launchNewInstance();
                    } else {
                        console.log('✅ VS Code brought to front!');
                    }
                });
            } else {
                console.log('📱 No VS Code running, launching new instance...');
                launchNewInstance();
            }
        });
        
        function launchNewInstance() {
            // Use multiple approaches to ensure visibility
            console.log('🎯 Method 1: Using open command with new instance...');
            
            // Method 1: Open with new instance flag
            const openProcess = spawn('open', ['-n', '-a', vscodeAppPath], {
                stdio: 'pipe',
                detached: false
            });
            
            openProcess.on('exit', (code) => {
                if (code === 0) {
                    console.log('✅ VS Code launched successfully!');
                    
                    // Method 2: Use AppleScript to ensure it's visible and in front
                    setTimeout(() => {
                        console.log('🎯 Method 2: Bringing VS Code to front with AppleScript...');
                        exec('osascript -e \'tell application "Visual Studio Code" to activate\'', (err) => {
                            if (!err) {
                                console.log('✅ VS Code is now visible and in front!');
                            }
                        });
                    }, 2000);
                    
                } else {
                    console.log(`❌ VS Code launch failed with code: ${code}`);
                    
                    // Method 3: Try direct executable launch as fallback
                    console.log('🎯 Method 3: Trying direct executable launch...');
                    const executablePath = path.join(vscodeAppPath, 'Contents/MacOS/Electron');
                    const directProcess = spawn(executablePath, [], {
                        stdio: 'pipe',
                        detached: true
                    });
                    
                    directProcess.unref();
                    console.log('🔄 Direct launch attempted...');
                }
            });
            
            openProcess.on('error', (error) => {
                console.error('❌ Failed to launch VS Code with open command:', error);
            });
        }
        
    } catch (error) {
        console.error('❌ Error in launch process:', error);
    }
}

// Also provide a function to check VS Code status
function checkVSCodeStatus() {
    console.log('🔍 Checking VS Code status...');
    
    exec('ps aux | grep -i "visual studio code\\|electron" | grep -v grep', (error, stdout, stderr) => {
        if (stdout) {
            console.log('✅ VS Code processes found:');
            const lines = stdout.split('\n').filter(line => line.trim());
            lines.forEach((line, index) => {
                if (line.includes('Visual Studio Code') || line.includes('Electron')) {
                    console.log(`   ${index + 1}. ${line.split(' ').slice(-1)[0]}`);
                }
            });
        } else {
            console.log('❌ No VS Code processes found');
        }
    });
    
    // Check if VS Code app is in the foreground
    exec('osascript -e \'tell application "System Events" to get name of first application process whose frontmost is true\'', (error, stdout) => {
        if (stdout && stdout.includes('Visual Studio Code')) {
            console.log('✅ VS Code is currently in the foreground');
        } else {
            console.log('⚠️  VS Code is not in the foreground. Current app:', stdout.trim());
        }
    });
}

// Run the launcher
console.log('🚀 Starting VS Code visibility launcher...');
launchVSCodeVisible();

// Check status after a delay
setTimeout(() => {
    console.log('\n📊 Status check after launch:');
    checkVSCodeStatus();
}, 5000);