import { DefaultAzureCredential, ClientSecretCredential, InteractiveBrowserCredential } from '@azure/identity';
import { PublicClientApplication, Configuration, AuthenticationResult } from '@azure/msal-node';
import { SecretClient } from '@azure/keyvault-secrets';
import { logger } from './logger';
import { config } from './config';

export interface AuthConfig {
  tenantId: string;
  clientId: string;
  clientSecret?: string;
  keyVaultUrl?: string;
  useInteractive?: boolean;
  useManagedIdentity?: boolean;
}

export class AuthManager {
  private credential: any;
  private msalApp: PublicClientApplication | null = null;
  private authResult: AuthenticationResult | null = null;

  constructor(private authConfig: AuthConfig) {
    this.initializeCredential();
  }

  private initializeCredential(): void {
    try {
      if (this.authConfig.useManagedIdentity) {
        logger.info('Using Managed Identity for authentication');
        this.credential = new DefaultAzureCredential();
      } else if (this.authConfig.clientSecret) {
        logger.info('Using Service Principal with client secret');
        this.credential = new ClientSecretCredential(
          this.authConfig.tenantId,
          this.authConfig.clientId,
          this.authConfig.clientSecret
        );
      } else if (this.authConfig.useInteractive) {
        logger.info('Using Interactive Browser authentication');
        this.credential = new InteractiveBrowserCredential({
          tenantId: this.authConfig.tenantId,
          clientId: this.authConfig.clientId,
        });
      } else {
        logger.info('Using Default Azure Credential');
        this.credential = new DefaultAzureCredential();
      }
    } catch (error) {
      logger.error('Failed to initialize credential', { error });
      throw error;
    }
  }

  async getAccessToken(scopes: string[] = ['https://management.azure.com/.default']): Promise<string> {
    try {
      const tokenResponse = await this.credential.getToken(scopes);
      return tokenResponse.token;
    } catch (error) {
      logger.error('Failed to get access token', { error, scopes });
      throw error;
    }
  }

  async initializeMSAL(): Promise<void> {
    const msalConfig: Configuration = {
      auth: {
        clientId: this.authConfig.clientId,
        authority: `https://login.microsoftonline.com/${this.authConfig.tenantId}`,
      },
    };

    this.msalApp = new PublicClientApplication(msalConfig);
  }

  async authenticateForPIM(): Promise<AuthenticationResult> {
    if (!this.msalApp) {
      await this.initializeMSAL();
    }

    try {
      const clientCredentialRequest = {
        scopes: ['https://graph.microsoft.com/.default'],
        skipCache: false,
      };

      this.authResult = await this.msalApp!.acquireTokenSilent(clientCredentialRequest);
      logger.info('PIM authentication successful');
      return this.authResult;
    } catch (error) {
      logger.error('PIM authentication failed', { error });
      throw error;
    }
  }

  async getSecretFromKeyVault(secretName: string): Promise<string> {
    if (!this.authConfig.keyVaultUrl) {
      throw new Error('Key Vault URL not configured');
    }

    try {
      const client = new SecretClient(this.authConfig.keyVaultUrl, this.credential);
      const secret = await client.getSecret(secretName);
      logger.info(`Retrieved secret: ${secretName}`);
      return secret.value || '';
    } catch (error) {
      logger.error(`Failed to retrieve secret: ${secretName}`, { error });
      throw error;
    }
  }

  getCredential() {
    return this.credential;
  }

  async validateAuthentication(): Promise<boolean> {
    try {
      await this.getAccessToken();
      logger.info('Authentication validation successful');
      return true;
    } catch (error) {
      logger.error('Authentication validation failed', { error });
      return false;
    }
  }
}

// Factory function to create auth manager based on environment
export function createAuthManager(): AuthManager {
  const authConfig: AuthConfig = {
    tenantId: config.azure.tenantId,
    clientId: config.azure.clientId,
    clientSecret: config.azure.clientSecret,
    keyVaultUrl: config.azure.keyVaultUrl,
    useInteractive: config.auth.useInteractive,
    useManagedIdentity: config.auth.useManagedIdentity,
  };

  return new AuthManager(authConfig);
}

// Global auth manager instance
export const authManager = createAuthManager();