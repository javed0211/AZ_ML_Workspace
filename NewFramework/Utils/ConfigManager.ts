import * as fs from 'fs';
import * as path from 'path';

export interface AzureConfig {
  SubscriptionId: string;
  TenantId: string;
  ResourceGroup: string;
  WorkspaceName: string;
  MLWorkspaceDisplayName: string;
  Region: string;
}

export interface MFAConfig {
  Enabled: boolean;
  AutoSubmitOTP: boolean;
  OTPTimeoutSeconds: number;
  TOTPSecretKey: string;
}

export interface AuthenticationConfig {
  Username: string;
  Password: string;
  UseDefaultCredentials: boolean;
  TimeoutSeconds: number;
  MFA?: MFAConfig;
}

export interface EnvironmentConfig {
  BaseUrl: string;
  Username: string;
  Password: string;
  DatabaseConnection: string;
  Azure?: AzureConfig;
  Authentication?: AuthenticationConfig;
}

export interface BrowserConfig {
  Type: string;
  Headless: boolean;
  SlowMo: number;
  Timeout: number;
  ViewportWidth: number;
  ViewportHeight: number;
}

export interface TestConfig {
  DefaultTimeout: number;
  RetryCount: number;
  ParallelWorkers: number;
  ScreenshotOnFailure: boolean;
  VideoOnFailure: boolean;
  TraceOnFailure: boolean;
}

export interface LoggingConfig {
  LogLevel: string;
  LogToFile: boolean;
  LogToConsole: boolean;
  LogFilePath: string;
}

export interface ElectronConfig {
  ExecutablePath: string;
  WindowsExecutablePath: string;
  LinuxExecutablePath: string;
  Args: string[];
}

export interface AppConfig {
  Environment: string;
  Environments: {
    [key: string]: EnvironmentConfig;
  };
  Browser: BrowserConfig;
  TestSettings: TestConfig;
  Logging: LoggingConfig;
  ElectronApp: ElectronConfig;
}

export class ConfigManager {
  private static instance: ConfigManager;
  private config: AppConfig;

  private constructor() {
    this.loadConfig();
  }

  public static getInstance(): ConfigManager {
    if (!ConfigManager.instance) {
      ConfigManager.instance = new ConfigManager();
    }
    return ConfigManager.instance;
  }

  private loadConfig(): void {
    try {
      const configPath = path.join(__dirname, '..', 'Config', 'appsettings.json');
      const configFile = fs.readFileSync(configPath, 'utf8');
      this.config = JSON.parse(configFile);
      
      // Override environment from environment variable if set
      const envFromProcess = process.env.TEST_ENV;
      if (envFromProcess && this.config.Environments[envFromProcess]) {
        this.config.Environment = envFromProcess;
      }
    } catch (error) {
      throw new Error(`Failed to load configuration: ${error}`);
    }
  }

  public getCurrentEnvironment(): EnvironmentConfig {
    const currentEnv = this.config.Environment;
    if (!this.config.Environments[currentEnv]) {
      throw new Error(`Environment '${currentEnv}' not found in configuration`);
    }
    return this.config.Environments[currentEnv];
  }

  public getBrowserSettings(): BrowserConfig {
    return this.config.Browser;
  }

  public getTestSettings(): TestConfig {
    return this.config.TestSettings;
  }

  public getLoggingSettings(): LoggingConfig {
    return this.config.Logging;
  }

  public getAzureSettings(): AzureConfig {
    const currentEnv = this.getCurrentEnvironment();
    if (!currentEnv.Azure) {
      throw new Error(`Azure configuration not found for environment '${this.config.Environment}'`);
    }
    return currentEnv.Azure;
  }

  public getAuthenticationSettings(): AuthenticationConfig {
    const currentEnv = this.getCurrentEnvironment();
    if (!currentEnv.Authentication) {
      throw new Error(`Authentication configuration not found for environment '${this.config.Environment}'`);
    }
    return currentEnv.Authentication;
  }

  public getElectronSettings(): ElectronConfig {
    return this.config.ElectronApp;
  }

  public getEnvironmentConfig(envName: string): EnvironmentConfig {
    if (!this.config.Environments[envName]) {
      throw new Error(`Environment '${envName}' not found in configuration`);
    }
    return this.config.Environments[envName];
  }

  public setEnvironment(envName: string): void {
    if (!this.config.Environments[envName]) {
      throw new Error(`Environment '${envName}' not found in configuration`);
    }
    this.config.Environment = envName;
  }

  public getAllEnvironments(): string[] {
    return Object.keys(this.config.Environments);
  }

  public getConfig(): AppConfig {
    return this.config;
  }
}