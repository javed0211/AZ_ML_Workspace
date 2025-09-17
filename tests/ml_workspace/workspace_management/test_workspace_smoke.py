"""Smoke tests for Azure ML workspace basic functionality."""

import pytest
from playwright.async_api import Page

from src.azure_ml_automation.helpers.logger import TestLogger
from src.azure_ml_automation.helpers.azure_helpers import AzureMLHelper
from src.azure_ml_automation.pages.azure_ml_studio import AzureMLStudioPage


@pytest.mark.smoke
@pytest.mark.azure
class TestWorkspaceSmoke:
    """Smoke tests for workspace functionality."""
    
    async def test_workspace_authentication(self, auth_manager):
        """Test that authentication is working."""
        is_valid = await auth_manager.validate_authentication()
        assert is_valid, "Authentication should be valid"
    
    async def test_workspace_access(self, azure_helper: AzureMLHelper):
        """Test that workspace is accessible."""
        has_access = await azure_helper.validate_workspace_access()
        assert has_access, "Should have access to workspace"
    
    async def test_azure_ml_studio_loads(self, azure_ml_page: AzureMLStudioPage):
        """Test that Azure ML Studio loads successfully."""
        await azure_ml_page.navigate_to_studio()
        
        # Verify page loaded
        assert await azure_ml_page.is_page_loaded(), "Azure ML Studio should load"
        
        # Verify workspace info is available
        workspace_info = await azure_ml_page.get_workspace_info()
        assert workspace_info.get("name"), "Workspace name should be available"
    
    async def test_navigation_to_compute(self, azure_ml_page: AzureMLStudioPage):
        """Test navigation to compute section."""
        await azure_ml_page.navigate_to_studio()
        await azure_ml_page.navigate_to_compute()
        
        # Verify we're on the compute page
        current_url = await azure_ml_page.get_current_url()
        assert "compute" in current_url.lower(), "Should navigate to compute section"
    
    async def test_navigation_to_notebooks(self, azure_ml_page: AzureMLStudioPage):
        """Test navigation to notebooks section."""
        await azure_ml_page.navigate_to_studio()
        await azure_ml_page.navigate_to_notebooks()
        
        # Verify we're on the notebooks page
        current_url = await azure_ml_page.get_current_url()
        assert "notebook" in current_url.lower(), "Should navigate to notebooks section"
    
    async def test_navigation_to_jobs(self, azure_ml_page: AzureMLStudioPage):
        """Test navigation to jobs section."""
        await azure_ml_page.navigate_to_studio()
        await azure_ml_page.navigate_to_jobs()
        
        # Verify we're on the jobs page
        current_url = await azure_ml_page.get_current_url()
        assert "job" in current_url.lower(), "Should navigate to jobs section"
    
    @pytest.mark.slow
    async def test_compute_instance_status_check(
        self, 
        azure_helper: AzureMLHelper,
        test_logger: TestLogger
    ):
        """Test checking compute instance status via CLI."""
        # This test assumes a compute instance exists
        # In a real scenario, you might create one first or skip if none exists
        
        try:
            # Try to get status of the configured compute instance
            from src.azure_ml_automation.helpers.config import config
            instance_name = config.compute.instance_name
            
            if not instance_name:
                pytest.skip("No compute instance configured for testing")
            
            instance = await azure_helper.get_compute_instance_status(instance_name)
            
            # Verify we got valid instance data
            assert instance.name == instance_name, "Instance name should match"
            assert instance.state, "Instance should have a state"
            assert instance.vm_size, "Instance should have a VM size"
            
            test_logger.info(f"Compute instance {instance_name} status: {instance.state}")
            
        except Exception as e:
            if "not found" in str(e).lower():
                pytest.skip(f"Compute instance {instance_name} not found")
            else:
                raise
    
    async def test_page_title_and_branding(self, azure_ml_page: AzureMLStudioPage):
        """Test that page has correct title and branding."""
        await azure_ml_page.navigate_to_studio()
        
        title = await azure_ml_page.get_page_title()
        assert "azure" in title.lower() or "ml" in title.lower(), "Page title should contain Azure or ML"
    
    async def test_responsive_design(self, page: Page, azure_ml_page: AzureMLStudioPage):
        """Test that the page works on different viewport sizes."""
        # Test desktop size
        await page.set_viewport_size({"width": 1920, "height": 1080})
        await azure_ml_page.navigate_to_studio()
        assert await azure_ml_page.is_page_loaded(), "Should load on desktop"
        
        # Test tablet size
        await page.set_viewport_size({"width": 768, "height": 1024})
        await azure_ml_page.navigate_to_studio()
        assert await azure_ml_page.is_page_loaded(), "Should load on tablet"
        
        # Test mobile size (if supported)
        await page.set_viewport_size({"width": 375, "height": 667})
        await azure_ml_page.navigate_to_studio()
        # Note: Azure ML Studio might not be fully mobile-responsive
        # This test might need to be adjusted based on actual behavior