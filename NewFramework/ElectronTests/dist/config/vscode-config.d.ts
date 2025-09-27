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
 * Get VS Code executable path based on the current platform
 */
export declare function getVSCodeExecutablePath(): string;
/**
 * Validate VS Code installation and provide helpful error messages
 */
export declare function validateVSCodeInstallation(): {
    isValid: boolean;
    message: string;
    suggestedPath?: string;
};
/**
 * Get default VS Code configuration
 */
export declare function getDefaultVSCodeConfig(): VSCodeConfig;
/**
 * VS Code launch arguments for testing
 * Note: VS Code should be launched via 'open' command on macOS, not directly
 */
export declare const getVSCodeLaunchArgs: (config: VSCodeConfig, additionalArgs?: string[]) => string[];
