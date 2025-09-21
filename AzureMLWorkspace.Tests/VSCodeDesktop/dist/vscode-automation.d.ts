interface VSCodeAutomationParams {
    action: string;
    workspacePath?: string;
    timeout?: number;
    headless?: boolean;
    [key: string]: any;
}
interface AutomationResult {
    success: boolean;
    message: string;
    data?: any;
    error?: string;
}
declare class VSCodeDesktopAutomation {
    private app;
    private mainWindow;
    private readonly defaultTimeout;
    launch(params: VSCodeAutomationParams): Promise<AutomationResult>;
    checkInteractivity(): Promise<AutomationResult>;
    openWorkspace(workspacePath: string): Promise<AutomationResult>;
    connectToRemoteCompute(computeName: string): Promise<AutomationResult>;
    checkApplicationLinks(): Promise<AutomationResult>;
    takeScreenshot(filename?: string): Promise<AutomationResult>;
    close(): Promise<AutomationResult>;
    private findVSCodeExecutable;
    private waitForVSCodeReady;
}
export { VSCodeDesktopAutomation, VSCodeAutomationParams, AutomationResult };
//# sourceMappingURL=vscode-automation.d.ts.map