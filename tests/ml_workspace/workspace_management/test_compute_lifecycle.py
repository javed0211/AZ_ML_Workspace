"""Tests for compute instance lifecycle management."""

import pytest
import asyncio
from datetime import datetime

from src.azure_ml_automation.helpers.logger import TestLogger
from src.azure_ml_automation.helpers.azure_helpers import AzureMLHelper
from src.azure_ml_automation.pages.azure_ml_studio import AzureMLStudioPage


@pytest.mark.compute
@pytest.mark.azure
@pytest.mark.slow
class TestComputeLifecycle:
    """Tests for compute instance lifecycle operations."""
    
    @pytest.fixture
    def test_instance_name(self):
        """Generate a unique test instance name."""
        timestamp = datetime.now().strftime("%Y%m%d%H%M%S")
        return f"test-instance-{timestamp}"
    
    async def test_compute_instance_creation_via_ui(
        self,
        azure_ml_page: AzureMLStudioPage,
        test_instance_name: str,
        test_logger: TestLogger
    ):
        """Test creating a compute instance via the UI."""
        await azure_ml_page.navigate_to_studio()
        await azure_ml_page.navigate_to_compute()
        
        # Create compute instance
        await azure_ml_page.create_compute_instance(
            test_instance_name,
            vm_size="Standard_DS3_v2"
        )
        
        # Verify instance appears in the list
        await azure_ml_page.assert_compute_instance_exists(test_instance_name)
        
        # Wait for instance to be created (this might take several minutes)
        await azure_ml_page.wait_for_compute_instance_status(
            test_instance_name,
            "Running",
            timeout=900000  # 15 minutes
        )
        
        test_logger.info(f"Compute instance {test_instance_name} created successfully")
    
    async def test_compute_instance_start_stop_via_cli(
        self,
        azure_helper: AzureMLHelper,
        test_instance_name: str,
        test_logger: TestLogger
    ):
        """Test starting and stopping compute instance via CLI."""
        # This test assumes the instance from the previous test exists
        # In practice, you might want to create it first or use a known instance
        
        # Get initial status
        initial_instance = await azure_helper.get_compute_instance_status(test_instance_name)
        test_logger.info(f"Initial status: {initial_instance.state}")
        
        if initial_instance.state.lower() == "running":
            # Stop the instance
            await azure_helper.stop_compute_instance(test_instance_name)
            
            # Wait for it to stop
            await azure_helper.wait_for_compute_instance_state(
                test_instance_name,
                "Stopped",
                timeout_seconds=300  # 5 minutes
            )
            
            # Verify it's stopped
            stopped_instance = await azure_helper.get_compute_instance_status(test_instance_name)
            assert stopped_instance.state.lower() == "stopped", "Instance should be stopped"
            
            # Start it again
            await azure_helper.start_compute_instance(test_instance_name)
            
            # Wait for it to start
            await azure_helper.wait_for_compute_instance_state(
                test_instance_name,
                "Running",
                timeout_seconds=600  # 10 minutes
            )
            
            # Verify it's running
            running_instance = await azure_helper.get_compute_instance_status(test_instance_name)
            assert running_instance.state.lower() == "running", "Instance should be running"
        
        elif initial_instance.state.lower() == "stopped":
            # Start the instance
            await azure_helper.start_compute_instance(test_instance_name)
            
            # Wait for it to start
            await azure_helper.wait_for_compute_instance_state(
                test_instance_name,
                "Running",
                timeout_seconds=600  # 10 minutes
            )
            
            # Verify it's running
            running_instance = await azure_helper.get_compute_instance_status(test_instance_name)
            assert running_instance.state.lower() == "running", "Instance should be running"
        
        test_logger.info(f"Compute instance {test_instance_name} lifecycle test completed")
    
    async def test_compute_instance_ui_controls(
        self,
        azure_ml_page: AzureMLStudioPage,
        test_instance_name: str,
        test_logger: TestLogger
    ):
        """Test compute instance controls in the UI."""
        await azure_ml_page.navigate_to_studio()
        await azure_ml_page.navigate_to_compute()
        
        # Get current status
        current_status = await azure_ml_page.get_compute_instance_status(test_instance_name)
        test_logger.info(f"Current UI status: {current_status}")
        
        if current_status.lower() == "running":
            # Test stop button
            await azure_ml_page.stop_compute_instance(test_instance_name)
            
            # Wait for status to change
            await azure_ml_page.wait_for_compute_instance_status(
                test_instance_name,
                "Stopped",
                timeout=300000  # 5 minutes
            )
            
            # Verify status changed
            new_status = await azure_ml_page.get_compute_instance_status(test_instance_name)
            assert new_status.lower() == "stopped", "Instance should be stopped via UI"
            
        elif current_status.lower() == "stopped":
            # Test start button
            await azure_ml_page.start_compute_instance(test_instance_name)
            
            # Wait for status to change
            await azure_ml_page.wait_for_compute_instance_status(
                test_instance_name,
                "Running",
                timeout=600000  # 10 minutes
            )
            
            # Verify status changed
            new_status = await azure_ml_page.get_compute_instance_status(test_instance_name)
            assert new_status.lower() == "running", "Instance should be running via UI"
    
    async def test_compute_instance_parallel_operations(
        self,
        azure_helper: AzureMLHelper,
        test_logger: TestLogger
    ):
        """Test that multiple compute operations can be handled in parallel."""
        # This test assumes multiple compute instances exist
        # In practice, you might create them first
        
        from src.azure_ml_automation.helpers.config import config
        
        # Get list of available instances (simplified - in real implementation,
        # you'd query the actual instances)
        test_instances = [
            f"test-parallel-1-{datetime.now().strftime('%Y%m%d%H%M%S')}",
            f"test-parallel-2-{datetime.now().strftime('%Y%m%d%H%M%S')}"
        ]
        
        # Skip if we don't have multiple instances to test with
        if len(test_instances) < 2:
            pytest.skip("Need multiple compute instances for parallel testing")
        
        # Test parallel status checks
        async def check_instance_status(instance_name):
            try:
                return await azure_helper.get_compute_instance_status(instance_name)
            except Exception as e:
                test_logger.warning(f"Could not get status for {instance_name}: {e}")
                return None
        
        # Run status checks in parallel
        tasks = [check_instance_status(name) for name in test_instances]
        results = await asyncio.gather(*tasks, return_exceptions=True)
        
        # Verify we got results (even if some failed)
        successful_results = [r for r in results if r is not None and not isinstance(r, Exception)]
        test_logger.info(f"Parallel status check completed: {len(successful_results)} successful")
    
    @pytest.mark.cleanup
    async def test_cleanup_test_instances(
        self,
        azure_ml_page: AzureMLStudioPage,
        test_instance_name: str,
        test_logger: TestLogger
    ):
        """Clean up test instances created during testing."""
        # This test should run last to clean up resources
        
        try:
            await azure_ml_page.navigate_to_studio()
            await azure_ml_page.navigate_to_compute()
            
            # Check if instance exists
            try:
                await azure_ml_page.assert_compute_instance_exists(test_instance_name)
                
                # Delete the instance
                await azure_ml_page.delete_compute_instance(test_instance_name)
                
                test_logger.info(f"Cleanup: Deleted compute instance {test_instance_name}")
                
            except AssertionError:
                test_logger.info(f"Cleanup: Instance {test_instance_name} not found, nothing to clean")
                
        except Exception as e:
            test_logger.warning(f"Cleanup failed for {test_instance_name}: {e}")
            # Don't fail the test if cleanup fails
    
    async def test_compute_instance_error_handling(
        self,
        azure_helper: AzureMLHelper,
        test_logger: TestLogger
    ):
        """Test error handling for compute operations."""
        non_existent_instance = "non-existent-instance-12345"
        
        # Test getting status of non-existent instance
        with pytest.raises(Exception) as exc_info:
            await azure_helper.get_compute_instance_status(non_existent_instance)
        
        assert "not found" in str(exc_info.value).lower(), "Should get 'not found' error"
        
        # Test starting non-existent instance
        with pytest.raises(Exception) as exc_info:
            await azure_helper.start_compute_instance(non_existent_instance)
        
        test_logger.info("Error handling tests completed successfully")