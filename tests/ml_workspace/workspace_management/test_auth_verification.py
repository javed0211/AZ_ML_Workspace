"""
Quick verification test to ensure authentication is configured correctly.
"""

import pytest
from azure.identity import ManagedIdentityCredential

from src.azure_ml_automation.helpers.auth import create_auth_manager, create_pim_manager
from src.azure_ml_automation.helpers.logger import TestLogger


@pytest.mark.auth
@pytest.mark.azure
class TestAuthVerification:
    """Quick tests to verify authentication configuration."""
    
    async def test_auth_manager_uses_managed_identity(self, auth_manager, test_logger: TestLogger):
        """Verify that the global auth_manager fixture uses Managed Identity."""
        test_logger.info("Testing global auth_manager fixture authentication method")
        
        credential = auth_manager.get_credential()
        
        assert isinstance(credential, ManagedIdentityCredential), \
            f"Expected ManagedIdentityCredential, but got {type(credential)}. " \
            f"Check .env.test file - AZURE_CLIENT_SECRET should not be set for Managed Identity."
        
        test_logger.info("✓ Global auth_manager correctly uses Managed Identity")
    
    async def test_pim_manager_inherits_managed_identity(self, test_logger: TestLogger):
        """Verify that PIM manager created from auth_manager uses Managed Identity."""
        test_logger.info("Testing PIM manager authentication inheritance")
        
        # Create auth manager and PIM manager
        auth_manager = create_auth_manager()
        pim_manager = create_pim_manager(auth_manager)
        
        # Check the underlying credential
        credential = pim_manager.auth_manager.get_credential()
        
        assert isinstance(credential, ManagedIdentityCredential), \
            f"Expected ManagedIdentityCredential, but got {type(credential)}. " \
            f"PIM manager should inherit Managed Identity from auth_manager."
        
        test_logger.info("✓ PIM manager correctly inherits Managed Identity authentication")
    
    async def test_no_client_secret_in_config(self, test_logger: TestLogger):
        """Verify that no client secret is configured (which would override Managed Identity)."""
        test_logger.info("Testing that client secret is not configured")
        
        from src.azure_ml_automation.helpers.config import config
        
        # Client secret should be None or empty for Managed Identity to be used
        client_secret = config.azure.client_secret
        
        assert client_secret is None or client_secret == "", \
            f"Client secret is configured ({client_secret}), which will override Managed Identity. " \
            f"Remove AZURE_CLIENT_SECRET from .env.test to use Managed Identity."
        
        test_logger.info("✓ No client secret configured - Managed Identity will be used")
    
    async def test_managed_identity_flag_enabled(self, test_logger: TestLogger):
        """Verify that the USE_MANAGED_IDENTITY flag is enabled."""
        test_logger.info("Testing USE_MANAGED_IDENTITY configuration flag")
        
        from src.azure_ml_automation.helpers.config import config
        
        assert config.auth.use_managed_identity == True, \
            f"USE_MANAGED_IDENTITY is {config.auth.use_managed_identity}, should be True. " \
            f"Check .env.test file."
        
        assert config.auth.use_interactive == False, \
            f"USE_INTERACTIVE_AUTH is {config.auth.use_interactive}, should be False for tests."
        
        test_logger.info("✓ Authentication flags correctly configured for Managed Identity")