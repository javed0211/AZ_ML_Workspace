const { spawn, exec } = require('child_process');
const path = require('path');

async function forceVSCodeVisible() {
    console.log('🚀 Force launching VS Code to be visible...');
    
    // Path to the VS Code downloaded by the official framework
    const vscodeAppPath = '/Users/oldguard/Documents/GitHub/AZ_ML_Workspace/NewFramework/ElectronTests/.vscode-test/vscode-darwin-arm64-1.104.2/Visual Studio Code.app';
    
    console.log('📍 VS Code path:', vscodeAppPath);
    
    // Step 1: Kill any existing VS Code processes to start fresh
    console.log('🔄 Step 1: Cleaning up existing VS Code processes...');
    exec('pkill -f "Visual Studio Code"', (error) => {
        // Don't worry if this fails - just means no VS Code was running
        
        setTimeout(() => {
            // Step 2: Launch VS Code with new instance and wait
            console.log('🎯 Step 2: Launching fresh VS Code instance...');
            
            const launchProcess = spawn('open', ['-n', '-W', '-a', vscodeAppPath], {
                stdio: 'pipe'
            });
            
            launchProcess.on('exit', (code) => {
                console.log(`📱 Launch process exited with code: ${code}`);
                
                // Step 3: Use AppleScript to force activation
                setTimeout(() => {
                    console.log('🎯 Step 3: Using AppleScript to force activation...');
                    
                    const appleScript = `
                        tell application "Visual Studio Code"
                            activate
                            set frontmost to true
                        end tell
                        
                        tell application "System Events"
                            tell process "Electron"
                                set frontmost to true
                            end tell
                        end tell
                    `;
                    
                    exec(`osascript -e '${appleScript}'`, (err, stdout, stderr) => {
                        if (err) {
                            console.log('⚠️  AppleScript method failed, trying alternative...');
                            
                            // Step 4: Alternative method using System Events
                            const altScript = `
                                tell application "System Events"
                                    set frontmost of first process whose name contains "Electron" to true
                                end tell
                            `;
                            
                            exec(`osascript -e '${altScript}'`, (err2) => {
                                if (!err2) {
                                    console.log('✅ Alternative method succeeded!');
                                } else {
                                    console.log('❌ All AppleScript methods failed');
                                }
                            });
                        } else {
                            console.log('✅ VS Code should now be visible and in front!');
                        }
                        
                        // Final status check
                        setTimeout(checkFinalStatus, 2000);
                    });
                }, 3000);
            });
            
            launchProcess.on('error', (error) => {
                console.error('❌ Launch failed:', error);
            });
            
        }, 2000);
    });
}

function checkFinalStatus() {
    console.log('\n📊 Final Status Check:');
    
    // Check processes
    exec('ps aux | grep -i "visual studio code\\|electron" | grep -v grep | wc -l', (error, stdout) => {
        const processCount = parseInt(stdout.trim());
        console.log(`✅ VS Code processes running: ${processCount}`);
    });
    
    // Check foreground app
    exec('osascript -e \'tell application "System Events" to get name of first application process whose frontmost is true\'', (error, stdout) => {
        console.log(`🎯 Current foreground app: ${stdout.trim()}`);
        
        if (stdout.includes('Visual Studio Code') || stdout.includes('Electron')) {
            console.log('🎉 SUCCESS! VS Code is now in the foreground!');
        } else {
            console.log('⚠️  VS Code may be running but not in foreground');
            console.log('💡 Try clicking on the VS Code icon in your dock');
        }
    });
    
    // Check if VS Code window exists
    exec('osascript -e \'tell application "System Events" to get windows of process "Electron"\'', (error, stdout) => {
        if (stdout && !stdout.includes('{}')) {
            console.log('✅ VS Code windows detected');
        } else {
            console.log('⚠️  No VS Code windows detected');
        }
    });
}

// Also create a simple dock activation function
function activateFromDock() {
    console.log('🎯 Attempting to activate VS Code from dock...');
    
    const dockScript = `
        tell application "System Events"
            tell dock preferences
                set dock items to dock items
            end tell
            
            tell process "Dock"
                try
                    click UI element "Visual Studio Code" of list 1
                on error
                    click UI element "Electron" of list 1
                end try
            end tell
        end tell
    `;
    
    exec(`osascript -e '${dockScript}'`, (error) => {
        if (error) {
            console.log('⚠️  Dock activation failed, VS Code might not be in dock');
        } else {
            console.log('✅ Dock activation attempted');
        }
    });
}

// Run the force launcher
forceVSCodeVisible();

// Also try dock activation after a delay
setTimeout(activateFromDock, 8000);