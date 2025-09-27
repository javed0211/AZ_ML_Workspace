import * as os from 'os';
import * as path from 'path';
import * as fs from 'fs';

/**
 * VS Code Electron configuration for different platforms
 */
export interface VSCodeConfig {
  executablePath: string;
  userDataDir: string;
  extensionsDir: string;
  workspaceDir: string;
}

/**
 * Check if a file exists
 */
function fileExists(filePath: string): boolean {
  try {
    return fs.existsSync(filePath);
  } catch {
    return false;
  }
}

/**
 * Get VS Code executable path based on the current platform
 */
export function getVSCodeExecutablePath(): string {
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
export function validateVSCodeInstallation(): { isValid: boolean; message: string; suggestedPath?: string } {
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
export function getDefaultVSCodeConfig(): VSCodeConfig {
  const homeDir = os.homedir();
  const platform = os.platform();
  
  let userDataDir: string;
  let extensionsDir: string;
  
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
export const getVSCodeLaunchArgs = (config: VSCodeConfig, additionalArgs: string[] = []): string[] => {
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