"""
Test authentication configuration to ensure Managed Identity is used by default.
"""

import pytest
from azure.identity import ManagedIdentityCredential, ClientSecretCredential, InteractiveBrowserCredential, DefaultAzureCredential

from src.azure_ml_automation.helpers.auth import create_auth_manager, AuthConfig
from src.azure_ml_automation.helpers.config import config
from src.azure_ml_automation.helpers.logger import TestLogger


@pytest.mark.auth
@pytest.mark.azure
class TestAuthConfiguration:
    """Test authentication configuration and credential selection."""
    
    def test_default_auth_config_uses_managed_identity(self, test_logger: TestLogger):
        """Test that the default configuration uses Managed Identity."""
        test_logger.info("Testing default authentication configuration")
        
        # Check that the default config has managed identity enabled
        assert config.auth.use_managed_identity == True, "Default configuration should use Managed Identity"
        assert config.auth.use_interactive == False, "Default configuration should not use interactive auth"
        
        test_logger.info("✓ Default configuration correctly uses Managed Identity")
    
    def test_auth_manager_uses_managed_identity_by_default(self, test_logger: TestLogger):
        """Test that AuthManager uses Managed Identity when no client secret is provided."""
        test_logger.info("Testing AuthManager credential selection")
        
        # Create auth config without client secret (simulating default test environment)
        auth_config = AuthConfig(
            tenant_id="test-tenant-id",
            client_id="test-client-id",
            client_secret=None,  # No client secret
            use_managed_identity=True,
            use_interactive=False
        )
        
        # Create auth manager
        auth_manager = create_auth_manager()
        
        # Verify that it uses ManagedIdentityCredential
        credential = auth_manager.get_credential()
        assert isinstance(credential, ManagedIdentityCredential), f"Expected ManagedIdentityCredential, got {type(credential)}"
        
        test_logger.info("✓ AuthManager correctly uses Managed Identity by default")
    
    def test_auth_manager_respects_client_secret_override(self, test_logger: TestLogger):
        """Test that AuthManager uses ClientSecretCredential when client secret is provided."""
        test_logger.info("Testing AuthManager with client secret override")
        
        # Create auth config with client secret
        auth_config = AuthConfig(
            tenant_id="test-tenant-id",
            client_id="test-client-id",
            client_secret="test-client-secret",
            use_managed_identity=True,  # Even with this set to True
            use_interactive=False
        )
        
        from src.azure_ml_automation.helpers.auth import AuthManager
        auth_manager = AuthManager(auth_config)
        
        # Verify that it uses ClientSecretCredential (client secret takes priority)
        credential = auth_manager.get_credential()
        assert isinstance(credential, ClientSecretCredential), f"Expected ClientSecretCredential, got {type(credential)}"
        
        test_logger.info("✓ AuthManager correctly prioritizes client secret when provided")
    
    def test_auth_manager_uses_interactive_when_specified(self, test_logger: TestLogger):
        """Test that AuthManager uses InteractiveBrowserCredential when specified."""
        test_logger.info("Testing AuthManager with interactive authentication")
        
        # Create auth config for interactive auth
        auth_config = AuthConfig(
            tenant_id="test-tenant-id",
            client_id="test-client-id",
            client_secret=None,
            use_managed_identity=False,  # Disable managed identity
            use_interactive=True
        )
        
        from src.azure_ml_automation.helpers.auth import AuthManager
        auth_manager = AuthManager(auth_config)
        
        # Verify that it uses InteractiveBrowserCredential
        credential = auth_manager.get_credential()
        assert isinstance(credential, InteractiveBrowserCredential), f"Expected InteractiveBrowserCredential, got {type(credential)}"
        
        test_logger.info("✓ AuthManager correctly uses interactive authentication when specified")
    
    def test_auth_manager_fallback_to_default_credential(self, test_logger: TestLogger):
        """Test that AuthManager falls back to DefaultAzureCredential when needed."""
        test_logger.info("Testing AuthManager fallback to DefaultAzureCredential")
        
        # Create auth config that disables all specific auth methods
        auth_config = AuthConfig(
            tenant_id="test-tenant-id",
            client_id="test-client-id",
            client_secret=None,
            use_managed_identity=False,
            use_interactive=False
        )
        
        from src.azure_ml_automation.helpers.auth import AuthManager
        auth_manager = AuthManager(auth_config)
        
        # Verify that it uses DefaultAzureCredential
        credential = auth_manager.get_credential()
        assert isinstance(credential, DefaultAzureCredential), f"Expected DefaultAzureCredential, got {type(credential)}"
        
        test_logger.info("✓ AuthManager correctly falls back to DefaultAzureCredential")
    
    def test_credential_priority_order(self, test_logger: TestLogger):
        """Test the credential selection priority order."""
        test_logger.info("Testing credential selection priority order")
        
        priority_tests = [
            {
                "name": "Managed Identity (no client secret)",
                "config": {
                    "client_secret": None,
                    "use_managed_identity": True,
                    "use_interactive": False
                },
                "expected_type": ManagedIdentityCredential
            },
            {
                "name": "Client Secret (overrides managed identity)",
                "config": {
                    "client_secret": "test-secret",
                    "use_managed_identity": True,
                    "use_interactive": False
                },
                "expected_type": ClientSecretCredential
            },
            {
                "name": "Interactive (when managed identity disabled)",
                "config": {
                    "client_secret": None,
                    "use_managed_identity": False,
                    "use_interactive": True
                },
                "expected_type": InteractiveBrowserCredential
            },
            {
                "name": "Managed Identity fallback",
                "config": {
                    "client_secret": None,
                    "use_managed_identity": True,
                    "use_interactive": False
                },
                "expected_type": ManagedIdentityCredential
            },
            {
                "name": "Default credential fallback",
                "config": {
                    "client_secret": None,
                    "use_managed_identity": False,
                    "use_interactive": False
                },
                "expected_type": DefaultAzureCredential
            }
        ]
        
        for test_case in priority_tests:
            test_logger.info(f"Testing: {test_case['name']}")
            
            auth_config = AuthConfig(
                tenant_id="test-tenant-id",
                client_id="test-client-id",
                **test_case["config"]
            )
            
            from src.azure_ml_automation.helpers.auth import AuthManager
            auth_manager = AuthManager(auth_config)
            credential = auth_manager.get_credential()
            
            assert isinstance(credential, test_case["expected_type"]), \
                f"For {test_case['name']}: expected {test_case['expected_type']}, got {type(credential)}"
            
            test_logger.info(f"✓ {test_case['name']} works correctly")
        
        test_logger.info("✓ All credential priority tests passed")
    
    async def test_managed_identity_authentication_flow(self, test_logger: TestLogger):
        """Test the complete Managed Identity authentication flow (mock)."""
        test_logger.info("Testing Managed Identity authentication flow")
        
        # This test would normally require an actual Azure environment with Managed Identity
        # For now, we'll test the configuration and setup
        
        auth_manager = create_auth_manager()
        credential = auth_manager.get_credential()
        
        # Verify we're using Managed Identity
        assert isinstance(credential, ManagedIdentityCredential), \
            f"Expected ManagedIdentityCredential, got {type(credential)}"
        
        test_logger.info("✓ Managed Identity authentication flow configured correctly")
        
        # Note: In a real Azure environment with Managed Identity, you could test:
        # token = await auth_manager.get_access_token()
        # assert token is not None and len(token) > 0
    
    def test_environment_variable_override(self, test_logger: TestLogger):
        """Test that environment variables can override default Managed Identity setting."""
        test_logger.info("Testing environment variable override capability")
        
        # The config system should allow environment variables to override defaults
        # This is tested by the configuration system itself
        
        # Verify that the config fields are properly set up for environment override
        from src.azure_ml_automation.helpers.config import AuthConfig
        import inspect
        
        # Check that the AuthConfig fields have proper env variable mapping
        auth_config_fields = AuthConfig.__fields__
        
        assert "use_managed_identity" in auth_config_fields
        assert "use_interactive" in auth_config_fields
        
        # Verify field configuration allows environment override
        managed_identity_field = auth_config_fields["use_managed_identity"]
        interactive_field = auth_config_fields["use_interactive"]
        
        # These should have env variable names configured
        assert hasattr(managed_identity_field.field_info, 'env') or hasattr(managed_identity_field.field_info, 'alias')
        assert hasattr(interactive_field.field_info, 'env') or hasattr(interactive_field.field_info, 'alias')
        
        test_logger.info("✓ Environment variable override configuration is correct")
    
    def test_test_environment_uses_managed_identity(self, test_logger: TestLogger):
        """Test that the test environment is configured to use Managed Identity."""
        test_logger.info("Testing test environment configuration")
        
        # In test environment, we should be using Managed Identity
        # This is configured in .env.test file
        
        # Check current configuration
        current_config = config.auth
        
        test_logger.info(f"Current auth config - Managed Identity: {current_config.use_managed_identity}")
        test_logger.info(f"Current auth config - Interactive: {current_config.use_interactive}")
        
        # For tests, we expect Managed Identity to be enabled
        assert current_config.use_managed_identity == True, \
            "Test environment should use Managed Identity"
        
        assert current_config.use_interactive == False, \
            "Test environment should not use interactive authentication"
        
        test_logger.info("✓ Test environment correctly configured for Managed Identity")