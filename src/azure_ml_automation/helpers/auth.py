"""Authentication management for Azure ML automation framework."""

from typing import List, Optional, Dict, Any
from dataclasses import dataclass

from azure.identity import (
    DefaultAzureCredential,
    ClientSecretCredential, 
    InteractiveBrowserCredential,
    ManagedIdentityCredential
)
from azure.keyvault.secrets import SecretClient
from azure.core.credentials import AccessToken
import requests

from .config import config
from .logger import logger


@dataclass
class AuthConfig:
    """Authentication configuration."""
    tenant_id: str
    client_id: str
    client_secret: Optional[str] = None
    key_vault_url: Optional[str] = None
    use_interactive: bool = False
    use_managed_identity: bool = False


class AuthManager:
    """Manages Azure authentication for the automation framework."""
    
    def __init__(self, auth_config: AuthConfig):
        self.auth_config = auth_config
        self.credential = None
        self._initialize_credential()
    
    def _initialize_credential(self) -> None:
        """Initialize the appropriate Azure credential based on configuration."""
        try:
            # Priority order: Managed Identity (default) > Client Secret > Interactive > Default
            if self.auth_config.use_managed_identity and not self.auth_config.client_secret:
                logger.info("Using Managed Identity for authentication")
                self.credential = ManagedIdentityCredential()
            elif self.auth_config.client_secret:
                logger.info("Using Service Principal with client secret")
                self.credential = ClientSecretCredential(
                    tenant_id=self.auth_config.tenant_id,
                    client_id=self.auth_config.client_id,
                    client_secret=self.auth_config.client_secret
                )
            elif self.auth_config.use_interactive:
                logger.info("Using Interactive Browser authentication")
                self.credential = InteractiveBrowserCredential(
                    tenant_id=self.auth_config.tenant_id,
                    client_id=self.auth_config.client_id
                )
            elif self.auth_config.use_managed_identity:
                logger.info("Using Managed Identity for authentication (fallback)")
                self.credential = ManagedIdentityCredential()
            else:
                logger.info("Using Default Azure Credential")
                self.credential = DefaultAzureCredential()
                
        except Exception as error:
            logger.error("Failed to initialize credential", error=str(error))
            raise
    
    async def get_access_token(
        self, 
        scopes: List[str] = None
    ) -> str:
        """Get an access token for the specified scopes."""
        if scopes is None:
            scopes = ["https://management.azure.com/.default"]
        
        try:
            token_response: AccessToken = self.credential.get_token(*scopes)
            return token_response.token
        except Exception as error:
            logger.error("Failed to get access token", error=str(error), scopes=scopes)
            raise
    
    async def get_secret_from_key_vault(self, secret_name: str) -> str:
        """Retrieve a secret from Azure Key Vault."""
        if not self.auth_config.key_vault_url:
            raise ValueError("Key Vault URL not configured")
        
        try:
            client = SecretClient(
                vault_url=self.auth_config.key_vault_url,
                credential=self.credential
            )
            secret = client.get_secret(secret_name)
            logger.info(f"Retrieved secret: {secret_name}")
            return secret.value or ""
        except Exception as error:
            logger.error(
                f"Failed to retrieve secret: {secret_name}", 
                error=str(error)
            )
            raise
    
    def get_credential(self):
        """Get the underlying Azure credential object."""
        return self.credential
    
    async def validate_authentication(self) -> bool:
        """Validate that authentication is working."""
        try:
            await self.get_access_token()
            logger.info("Authentication validation successful")
            return True
        except Exception as error:
            logger.error("Authentication validation failed", error=str(error))
            return False
    
    async def authenticate_for_pim(self) -> Dict[str, Any]:
        """Authenticate for Privileged Identity Management operations."""
        try:
            scopes = ["https://graph.microsoft.com/.default"]
            token = await self.get_access_token(scopes)
            
            logger.info("PIM authentication successful")
            return {
                "access_token": token,
                "token_type": "Bearer"
            }
        except Exception as error:
            logger.error("PIM authentication failed", error=str(error))
            raise


class PIMManager:
    """Manages Privileged Identity Management operations."""
    
    def __init__(self, auth_manager: AuthManager):
        self.auth_manager = auth_manager
        self.graph_api_base = "https://graph.microsoft.com/v1.0"
    
    async def request_pim_activation(
        self,
        role_name: str,
        scope: str,
        justification: str = None
    ) -> str:
        """Request PIM role activation."""
        if justification is None:
            justification = config.pim.justification
        
        try:
            logger.info("Requesting PIM role activation", role=role_name, scope=scope)
            
            auth_result = await self.auth_manager.authenticate_for_pim()
            access_token = auth_result["access_token"]
            
            headers = {
                "Authorization": f"Bearer {access_token}",
                "Content-Type": "application/json"
            }
            
            payload = {
                "roleDefinitionId": role_name,
                "resourceId": scope,
                "subjectId": "me",
                "assignmentState": "Active",
                "type": "UserAdd",
                "reason": justification,
                "schedule": {
                    "type": "Once",
                    "startDateTime": None,  # Start immediately
                    "duration": "PT8H"  # 8 hours
                }
            }
            
            response = requests.post(
                f"{self.graph_api_base}/privilegedAccess/azureResources/roleAssignmentRequests",
                headers=headers,
                json=payload
            )
            response.raise_for_status()
            
            request_data = response.json()
            request_id = request_data.get("id")
            
            logger.info(
                "PIM activation request submitted",
                role=role_name,
                scope=scope,
                request_id=request_id
            )
            
            return request_id
            
        except Exception as error:
            logger.error(
                "Failed to request PIM activation",
                error=str(error),
                role=role_name,
                scope=scope
            )
            raise
    
    async def check_pim_activation_status(self, request_id: str) -> str:
        """Check the status of a PIM activation request."""
        try:
            auth_result = await self.auth_manager.authenticate_for_pim()
            access_token = auth_result["access_token"]
            
            headers = {
                "Authorization": f"Bearer {access_token}"
            }
            
            response = requests.get(
                f"{self.graph_api_base}/privilegedAccess/azureResources/roleAssignmentRequests/{request_id}",
                headers=headers
            )
            response.raise_for_status()
            
            request_data = response.json()
            status = request_data.get("status", "Unknown")
            
            logger.info("PIM activation status checked", request_id=request_id, status=status)
            return status
            
        except Exception as error:
            logger.error(
                "Failed to check PIM activation status",
                error=str(error),
                request_id=request_id
            )
            raise


def create_auth_manager() -> AuthManager:
    """Factory function to create auth manager based on environment configuration."""
    auth_config = AuthConfig(
        tenant_id=config.azure.tenant_id,
        client_id=config.azure.client_id,
        client_secret=config.azure.client_secret,
        key_vault_url=config.azure.key_vault_url,
        use_interactive=config.auth.use_interactive,
        use_managed_identity=config.auth.use_managed_identity
    )
    
    return AuthManager(auth_config)


def create_pim_manager(auth_manager: AuthManager = None) -> PIMManager:
    """Factory function to create PIM manager."""
    if auth_manager is None:
        auth_manager = create_auth_manager()
    return PIMManager(auth_manager)


# Global auth manager instance
auth_manager = create_auth_manager()
pim_manager = create_pim_manager(auth_manager)