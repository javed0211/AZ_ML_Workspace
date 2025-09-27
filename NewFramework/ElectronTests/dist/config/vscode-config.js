"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.getVSCodeLaunchArgs = void 0;
exports.getVSCodeExecutablePath = getVSCodeExecutablePath;
exports.validateVSCodeInstallation = validateVSCodeInstallation;
exports.getDefaultVSCodeConfig = getDefaultVSCodeConfig;
const os = __importStar(require("os"));
const path = __importStar(require("path"));
const fs = __importStar(require("fs"));
/**
 * Check if a file exists
 */
function fileExists(filePath) {
    try {
        return fs.existsSync(filePath);
    }
    catch {
        return false;
    }
}
/**
 * Get VS Code executable path based on the current platform
 */
function getVSCodeExecutablePath() {
    const platform = os.platform();
    switch (platform) {
        case 'win32':
            // Windows paths (try multiple common locations)
            const windowsPaths = [
                'C:\\Program Files\\Microsoft VS Code\\Code.exe',
                'C:\\Program Files (x86)\\Microsoft VS Code\\Code.exe',
                path.join(os.homedir(), 'AppData', 'Local', 'Programs', 'Microsoft VS Code', 'Code.exe')
            ];
            // Return the first existing path
            for (const execPath of windowsPaths) {
                if (fileExists(execPath)) {
                    return execPath;
                }
            }
            return windowsPaths[0]; // Default to first option if none found
        case 'darwin':
            // macOS paths (try multiple common locations)
            // Note: Newer VS Code versions use "Visual Studio Code" instead of "Electron"
            const homeDir = os.homedir();
            const macPaths = [
                '/Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code',
                '/Applications/Visual Studio Code.app/Contents/MacOS/Electron',
                '/Applications/Visual Studio Code - Insiders.app/Contents/MacOS/Visual Studio Code - Insiders',
                '/Applications/Visual Studio Code - Insiders.app/Contents/MacOS/Electron',
                // Also check Downloads folder (common when VS Code is downloaded but not moved to Applications)
                path.join(homeDir, 'Downloads', 'Visual Studio Code.app', 'Contents', 'MacOS', 'Visual Studio Code'),
                path.join(homeDir, 'Downloads', 'Visual Studio Code.app', 'Contents', 'MacOS', 'Electron'),
                path.join(homeDir, 'Downloads', 'Visual Studio Code - Insiders.app', 'Contents', 'MacOS', 'Visual Studio Code - Insiders'),
                path.join(homeDir, 'Downloads', 'Visual Studio Code - Insiders.app', 'Contents', 'MacOS', 'Electron')
            ];
            // Return the first existing path
            for (const execPath of macPaths) {
                if (fileExists(execPath)) {
                    return execPath;
                }
            }
            return macPaths[0]; // Default to first option if none found
        case 'linux':
            // Linux paths (try multiple common locations)
            const linuxPaths = [
                '/usr/share/code/code',
                '/usr/bin/code',
                '/snap/code/current/usr/share/code/code',
                '/opt/visual-studio-code/code',
                path.join(os.homedir(), '.local/share/applications/code')
            ];
            // Return the first existing path
            for (const execPath of linuxPaths) {
                if (fileExists(execPath)) {
                    return execPath;
                }
            }
            return linuxPaths[0]; // Default to first option if none found
        default:
            throw new Error(`Unsupported platform: ${platform}`);
    }
}
/**
 * Validate VS Code installation and provide helpful error messages
 */
function validateVSCodeInstallation() {
    const executablePath = getVSCodeExecutablePath();
    const platform = os.platform();
    if (fileExists(executablePath)) {
        return {
            isValid: true,
            message: `VS Code found at: ${executablePath}`
        };
    }
    // Provide platform-specific installation instructions
    let installInstructions = '';
    let suggestedPath = executablePath;
    switch (platform) {
        case 'darwin':
            installInstructions = `VS Code not found. Install it using one of these methods:
1. Download from: https://code.visualstudio.com/
2. Install via Homebrew: brew install --cask visual-studio-code
3. Install VS Code Insiders: brew install --cask visual-studio-code-insiders

Expected locations:
- /Applications/Visual Studio Code.app/Contents/MacOS/Visual Studio Code
- /Applications/Visual Studio Code.app/Contents/MacOS/Electron
- ~/Downloads/Visual Studio Code.app/Contents/MacOS/Visual Studio Code
- ~/Downloads/Visual Studio Code.app/Contents/MacOS/Electron

Note: If VS Code is in Downloads, move it to Applications:
sudo mv "~/Downloads/Visual Studio Code.app" /Applications/`;
            break;
        case 'win32':
            installInstructions = `VS Code not found. Install it using one of these methods:
1. Download from: https://code.visualstudio.com/
2. Install via winget: winget install Microsoft.VisualStudioCode
3. Install via Chocolatey: choco install vscode

Expected locations:
- C:\\Program Files\\Microsoft VS Code\\Code.exe
- C:\\Program Files (x86)\\Microsoft VS Code\\Code.exe`;
            break;
        case 'linux':
            installInstructions = `VS Code not found. Install it using one of these methods:
1. Download .deb/.rpm from: https://code.visualstudio.com/
2. Install via snap: sudo snap install code --classic
3. Install via package manager (Ubuntu/Debian): sudo apt install code

Expected locations:
- /usr/share/code/code
- /usr/bin/code
- /snap/code/current/usr/share/code/code`;
            break;
    }
    return {
        isValid: false,
        message: installInstructions,
        suggestedPath
    };
}
/**
 * Get default VS Code configuration
 */
function getDefaultVSCodeConfig() {
    const homeDir = os.homedir();
    const platform = os.platform();
    let userDataDir;
    let extensionsDir;
    switch (platform) {
        case 'win32':
            userDataDir = path.join(homeDir, 'AppData', 'Roaming', 'Code');
            extensionsDir = path.join(homeDir, '.vscode', 'extensions');
            break;
        case 'darwin':
            userDataDir = path.join(homeDir, 'Library', 'Application Support', 'Code');
            extensionsDir = path.join(homeDir, '.vscode', 'extensions');
            break;
        case 'linux':
            userDataDir = path.join(homeDir, '.config', 'Code');
            extensionsDir = path.join(homeDir, '.vscode', 'extensions');
            break;
        default:
            throw new Error(`Unsupported platform: ${platform}`);
    }
    return {
        executablePath: getVSCodeExecutablePath(),
        userDataDir,
        extensionsDir,
        workspaceDir: path.join(__dirname, '..', '..', '..'), // Points to AZ_ML_Workspace root
    };
}
/**
 * VS Code launch arguments for testing
 * Note: VS Code should be launched via 'open' command on macOS, not directly
 */
const getVSCodeLaunchArgs = (config, additionalArgs = []) => {
    // For macOS, we'll use 'open' command with --args
    const platform = require('os').platform();
    if (platform === 'darwin') {
        // Return minimal arguments that work with 'open' command
        return [
            ...additionalArgs
        ];
    }
    // For other platforms, try basic arguments
    return [
        '--disable-extensions',
        ...additionalArgs
    ];
};
exports.getVSCodeLaunchArgs = getVSCodeLaunchArgs;
