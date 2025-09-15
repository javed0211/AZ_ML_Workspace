import { Page } from '@playwright/test';
import { BasePage } from './base-page';
import { TestLogger } from '../helpers/logger';

export class VSCodeWebPage extends BasePage {
  // Selectors
  private readonly selectors = {
    // Main interface
    workbench: '.monaco-workbench',
    activityBar: '.activitybar',
    sidebar: '.sidebar',
    editorContainer: '.editor-container',
    panelContainer: '.panel-container',
    statusBar: '.statusbar',
    
    // Activity bar
    explorerIcon: '[data-id="workbench.view.explorer"]',
    searchIcon: '[data-id="workbench.view.search"]',
    sourceControlIcon: '[data-id="workbench.view.scm"]',
    extensionsIcon: '[data-id="workbench.view.extensions"]',
    
    // Explorer
    explorerView: '.explorer-viewlet',
    fileTree: '.monaco-tree',
    fileItem: '.monaco-tree-row',
    folderIcon: '.folder-icon',
    fileIcon: '.file-icon',
    
    // Editor
    editor: '.monaco-editor',
    editorTab: '.tab',
    editorTabLabel: '.tab-label',
    editorContent: '.monaco-editor-background',
    
    // Terminal
    terminal: '.terminal-wrapper',
    terminalTabs: '.terminal-tabs',
    terminalContent: '.xterm-screen',
    
    // Command palette
    commandPalette: '.quick-input-widget',
    commandInput: '.quick-input-box input',
    commandList: '.quick-input-list',
    commandItem: '.quick-input-list-row',
    
    // Notifications
    notificationContainer: '.notifications-container',
    notification: '.notification-toast',
    notificationMessage: '.notification-toast-message',
    
    // Extensions
    extensionsView: '.extensions-viewlet',
    extensionsList: '.extensions-list',
    extensionItem: '.extension-list-item',
    installButton: '.install-button',
    
    // Settings
    settingsEditor: '.settings-editor',
    settingsSearch: '.settings-search-input',
    settingItem: '.setting-item',
    
    // Remote indicator
    remoteIndicator: '.remote-indicator',
    remoteStatus: '.remote-indicator-text',
    
    // Loading
    loadingIndicator: '.monaco-progress-container',
    spinnerIcon: '.codicon-loading',
  };

  constructor(page: Page, testLogger?: TestLogger) {
    super(page, testLogger);
  }

  async isPageLoaded(): Promise<boolean> {
    try {
      await this.waitForElement(this.selectors.workbench, 10000);
      await this.waitForElement(this.selectors.activityBar, 10000);
      return true;
    } catch {
      return false;
    }
  }

  getPageIdentifier(): string {
    return 'VS Code Web';
  }

  async waitForPageLoad(): Promise<void> {
    await super.waitForPageLoad();
    
    // Wait for VS Code interface to load
    await this.waitForElement(this.selectors.workbench);
    await this.waitForElement(this.selectors.activityBar);
    
    // Wait for loading indicators to disappear
    try {
      await this.page.waitForSelector(this.selectors.loadingIndicator, { 
        state: 'detached', 
        timeout: 30000 
      });
    } catch {
      // Loading indicator might not appear
    }
  }

  // Navigation methods
  async openExplorer(): Promise<void> {
    this.testLogger?.action('Opening Explorer view');
    await this.clickElement(this.selectors.explorerIcon);
    await this.waitForElement(this.selectors.explorerView);
  }

  async openSearch(): Promise<void> {
    this.testLogger?.action('Opening Search view');
    await this.clickElement(this.selectors.searchIcon);
  }

  async openSourceControl(): Promise<void> {
    this.testLogger?.action('Opening Source Control view');
    await this.clickElement(this.selectors.sourceControlIcon);
  }

  async openExtensions(): Promise<void> {
    this.testLogger?.action('Opening Extensions view');
    await this.clickElement(this.selectors.extensionsIcon);
    await this.waitForElement(this.selectors.extensionsView);
  }

  // File operations
  async openFile(fileName: string): Promise<void> {
    this.testLogger?.action(`Opening file: ${fileName}`);
    
    await this.openExplorer();
    
    // Find and click the file in the explorer
    const fileItem = this.page.locator(`${this.selectors.fileItem}:has-text("${fileName}")`);
    await fileItem.click();
    
    // Wait for file to open in editor
    await this.waitForElement(`${this.selectors.editorTab}:has-text("${fileName}")`);
    
    this.testLogger?.info(`File opened: ${fileName}`);
  }

  async createFile(fileName: string, content: string = ''): Promise<void> {
    this.testLogger?.action(`Creating file: ${fileName}`);
    
    // Use command palette to create new file
    await this.openCommandPalette();
    await this.typeText('File: New File');
    await this.pressKey('Enter');
    
    // Type content if provided
    if (content) {
      await this.typeText(content);
    }
    
    // Save file with name
    await this.pressKey('Control+s');
    await this.page.waitForTimeout(1000);
    await this.typeText(fileName);
    await this.pressKey('Enter');
    
    this.testLogger?.info(`File created: ${fileName}`);
  }

  async saveFile(): Promise<void> {
    this.testLogger?.action('Saving current file');
    await this.pressKey('Control+s');
    this.testLogger?.info('File saved');
  }

  async closeFile(fileName?: string): Promise<void> {
    if (fileName) {
      this.testLogger?.action(`Closing file: ${fileName}`);
      
      // Find and close specific tab
      const tab = this.page.locator(`${this.selectors.editorTab}:has-text("${fileName}")`);
      const closeButton = tab.locator('.codicon-close');
      await closeButton.click();
    } else {
      this.testLogger?.action('Closing current file');
      await this.pressKey('Control+w');
    }
    
    this.testLogger?.info(`File closed${fileName ? `: ${fileName}` : ''}`);
  }

  // Editor operations
  async getEditorContent(): Promise<string> {
    const editor = await this.waitForElement(this.selectors.editor);
    return await editor.textContent() || '';
  }

  async setEditorContent(content: string): Promise<void> {
    this.testLogger?.action('Setting editor content');
    
    // Select all and replace
    await this.pressKey('Control+a');
    await this.typeText(content);
    
    this.testLogger?.info('Editor content set');
  }

  async insertTextAtCursor(text: string): Promise<void> {
    this.testLogger?.action(`Inserting text: ${text}`);
    await this.typeText(text);
  }

  async goToLine(lineNumber: number): Promise<void> {
    this.testLogger?.action(`Going to line: ${lineNumber}`);
    
    await this.pressKey('Control+g');
    await this.typeText(lineNumber.toString());
    await this.pressKey('Enter');
    
    this.testLogger?.info(`Navigated to line: ${lineNumber}`);
  }

  async findAndReplace(searchText: string, replaceText: string): Promise<void> {
    this.testLogger?.action(`Find and replace: "${searchText}" -> "${replaceText}"`);
    
    // Open find and replace
    await this.pressKey('Control+h');
    
    // Enter search text
    await this.typeText(searchText);
    await this.pressKey('Tab');
    
    // Enter replace text
    await this.typeText(replaceText);
    
    // Replace all
    await this.pressKey('Control+Alt+Enter');
    
    // Close find widget
    await this.pressKey('Escape');
    
    this.testLogger?.info(`Find and replace completed`);
  }

  // Terminal operations
  async openTerminal(): Promise<void> {
    this.testLogger?.action('Opening terminal');
    
    await this.pressKey('Control+`');
    await this.waitForElement(this.selectors.terminal);
    
    this.testLogger?.info('Terminal opened');
  }

  async runTerminalCommand(command: string): Promise<void> {
    this.testLogger?.action(`Running terminal command: ${command}`);
    
    // Ensure terminal is open
    if (!(await this.isElementVisible(this.selectors.terminal))) {
      await this.openTerminal();
    }
    
    // Click in terminal to focus
    await this.clickElement(this.selectors.terminalContent);
    
    // Type command and press Enter
    await this.typeText(command);
    await this.pressKey('Enter');
    
    this.testLogger?.info(`Terminal command executed: ${command}`);
  }

  async clearTerminal(): Promise<void> {
    this.testLogger?.action('Clearing terminal');
    
    await this.clickElement(this.selectors.terminalContent);
    await this.pressKey('Control+l');
    
    this.testLogger?.info('Terminal cleared');
  }

  // Extension operations
  async installExtension(extensionId: string): Promise<void> {
    this.testLogger?.action(`Installing extension: ${extensionId}`);
    
    await this.openExtensions();
    
    // Search for extension
    const searchBox = await this.waitForElement('.extensions-search-box input');
    await searchBox.fill(extensionId);
    await this.pressKey('Enter');
    
    // Find and install extension
    const extensionItem = this.page.locator(`${this.selectors.extensionItem}:has-text("${extensionId}")`);
    const installButton = extensionItem.locator(this.selectors.installButton);
    
    if (await installButton.isVisible()) {
      await installButton.click();
      
      // Wait for installation to complete
      await this.page.waitForSelector(`${this.selectors.extensionItem}:has-text("${extensionId}") .reload-button`, {
        timeout: 60000
      });
      
      this.testLogger?.info(`Extension installed: ${extensionId}`);
    } else {
      this.testLogger?.info(`Extension already installed: ${extensionId}`);
    }
  }

  async uninstallExtension(extensionId: string): Promise<void> {
    this.testLogger?.action(`Uninstalling extension: ${extensionId}`);
    
    await this.openExtensions();
    
    // Search for extension
    const searchBox = await this.waitForElement('.extensions-search-box input');
    await searchBox.fill(extensionId);
    await this.pressKey('Enter');
    
    // Find and uninstall extension
    const extensionItem = this.page.locator(`${this.selectors.extensionItem}:has-text("${extensionId}")`);
    const uninstallButton = extensionItem.locator('.uninstall-button');
    
    if (await uninstallButton.isVisible()) {
      await uninstallButton.click();
      this.testLogger?.info(`Extension uninstalled: ${extensionId}`);
    } else {
      this.testLogger?.info(`Extension not installed: ${extensionId}`);
    }
  }

  // Command palette operations
  async openCommandPalette(): Promise<void> {
    await this.pressKey('Control+Shift+p');
    await this.waitForElement(this.selectors.commandPalette);
  }

  async runCommand(commandName: string): Promise<void> {
    this.testLogger?.action(`Running command: ${commandName}`);
    
    await this.openCommandPalette();
    await this.typeText(commandName);
    await this.pressKey('Enter');
    
    this.testLogger?.info(`Command executed: ${commandName}`);
  }

  // Settings operations
  async openSettings(): Promise<void> {
    this.testLogger?.action('Opening settings');
    
    await this.runCommand('Preferences: Open Settings (UI)');
    await this.waitForElement(this.selectors.settingsEditor);
    
    this.testLogger?.info('Settings opened');
  }

  async changeSetting(settingName: string, value: string): Promise<void> {
    this.testLogger?.action(`Changing setting: ${settingName} = ${value}`);
    
    await this.openSettings();
    
    // Search for setting
    const searchBox = await this.waitForElement(this.selectors.settingsSearch);
    await searchBox.fill(settingName);
    
    // Find and modify setting
    const settingItem = this.page.locator(`${this.selectors.settingItem}:has-text("${settingName}")`);
    const input = settingItem.locator('input, select');
    
    if (await input.isVisible()) {
      await input.fill(value);
      this.testLogger?.info(`Setting changed: ${settingName} = ${value}`);
    } else {
      throw new Error(`Setting not found or not editable: ${settingName}`);
    }
  }

  // Remote operations
  async connectToRemote(connectionString: string): Promise<void> {
    this.testLogger?.action(`Connecting to remote: ${connectionString}`);
    
    await this.runCommand('Remote-SSH: Connect to Host...');
    
    // Enter connection string
    await this.page.waitForTimeout(1000);
    await this.typeText(connectionString);
    await this.pressKey('Enter');
    
    // Wait for connection to establish
    await this.waitForElement(this.selectors.remoteIndicator, 60000);
    
    this.testLogger?.info(`Connected to remote: ${connectionString}`);
  }

  async getRemoteStatus(): Promise<string> {
    try {
      const statusElement = await this.waitForElement(this.selectors.remoteStatus, 5000);
      return await statusElement.textContent() || 'Not connected';
    } catch {
      return 'Not connected';
    }
  }

  // Utility methods
  async getOpenTabs(): Promise<string[]> {
    const tabs = await this.page.locator(this.selectors.editorTab).all();
    const tabNames = [];
    
    for (const tab of tabs) {
      const label = await tab.locator(this.selectors.editorTabLabel).textContent();
      if (label) tabNames.push(label.trim());
    }
    
    return tabNames;
  }

  async isFileOpen(fileName: string): Promise<boolean> {
    const openTabs = await this.getOpenTabs();
    return openTabs.includes(fileName);
  }

  async waitForFileToOpen(fileName: string, timeout: number = 10000): Promise<void> {
    await this.waitForElement(`${this.selectors.editorTab}:has-text("${fileName}")`, timeout);
  }

  // Assertion methods
  async assertFileOpen(fileName: string): Promise<void> {
    const isOpen = await this.isFileOpen(fileName);
    if (!isOpen) {
      const openTabs = await this.getOpenTabs();
      throw new Error(`File "${fileName}" is not open. Open tabs: ${openTabs.join(', ')}`);
    }
    
    this.testLogger?.assertion(`File is open: ${fileName}`, true);
  }

  async assertEditorContent(expectedContent: string | RegExp): Promise<void> {
    const actualContent = await this.getEditorContent();
    
    if (typeof expectedContent === 'string') {
      if (!actualContent.includes(expectedContent)) {
        throw new Error(`Expected editor content to contain "${expectedContent}"`);
      }
    } else {
      if (!expectedContent.test(actualContent)) {
        throw new Error(`Expected editor content to match pattern`);
      }
    }
    
    this.testLogger?.assertion('Editor content matches expected', true);
  }

  async assertExtensionInstalled(extensionId: string): Promise<void> {
    await this.openExtensions();
    
    // Search for extension
    const searchBox = await this.waitForElement('.extensions-search-box input');
    await searchBox.fill(extensionId);
    await this.pressKey('Enter');
    
    // Check if extension is installed
    const extensionItem = this.page.locator(`${this.selectors.extensionItem}:has-text("${extensionId}")`);
    const isInstalled = await extensionItem.locator('.extension-action-label:has-text("Uninstall")').isVisible();
    
    if (!isInstalled) {
      throw new Error(`Extension "${extensionId}" is not installed`);
    }
    
    this.testLogger?.assertion(`Extension is installed: ${extensionId}`, true);
  }

  async assertRemoteConnected(expectedConnection?: string): Promise<void> {
    const status = await this.getRemoteStatus();
    
    if (status === 'Not connected') {
      throw new Error('Not connected to any remote');
    }
    
    if (expectedConnection && !status.includes(expectedConnection)) {
      throw new Error(`Expected connection to "${expectedConnection}" but got "${status}"`);
    }
    
    this.testLogger?.assertion('Remote connection established', true, { status });
  }

  async assertTerminalOpen(): Promise<void> {
    const isOpen = await this.isElementVisible(this.selectors.terminal);
    if (!isOpen) {
      throw new Error('Terminal is not open');
    }
    
    this.testLogger?.assertion('Terminal is open', true);
  }
}