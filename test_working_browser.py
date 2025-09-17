#!/usr/bin/env python3
"""
Working browser test that bypasses the problematic conftest.py fixtures.
"""

import pytest
import asyncio
from playwright.async_api import async_playwright
from src.azure_ml_automation.helpers.logger import TestLogger, log_test_start
from src.azure_ml_automation.pages.azure_ml_studio import AzureMLStudioPage

class TestWorkingBrowser:
    """Test class with working browser setup."""
    
    WORKSPACE_URL = "https://ml.azure.com/workspaces?tid=afd0e3db-52d8-42c3-9648-3e3a1c3c1d5f"
    
    @pytest.mark.asyncio
    async def test_working_navigation(self):
        """Test that works without problematic fixtures."""
        test_logger = log_test_start("test_working_navigation")
        test_logger.info("Starting working navigation test")
        
        async with async_playwright() as p:
            # Launch browser
            browser = await p.chromium.launch(headless=True)
            test_logger.info("Browser launched")
            
            # Create context
            context = await browser.new_context(
                viewport={"width": 1920, "height": 1080},
                ignore_https_errors=True
            )
            test_logger.info("Context created")
            
            # Create page
            page = await context.new_page()
            test_logger.info("Page created")
            
            # Create Azure ML page object
            azure_ml_page = AzureMLStudioPage(page, test_logger)
            test_logger.info("Azure ML page object created")
            
            try:
                # Navigate to Azure ML
                test_logger.info(f"Navigating to: {self.WORKSPACE_URL}")
                await page.goto(self.WORKSPACE_URL, timeout=30000)
                test_logger.info("Navigation completed")
                
                # Wait for network idle
                try:
                    await page.wait_for_load_state("networkidle", timeout=10000)
                    test_logger.info("Network idle reached")
                except Exception as e:
                    test_logger.warning(f"Network idle timeout: {e}")
                
                # Get page info
                title = await azure_ml_page.get_page_title()
                current_url = page.url
                
                test_logger.info(f"Page title: {title}")
                test_logger.info(f"Current URL: {current_url}")
                
                # Verify we reached Azure ML
                assert "azure" in title.lower() or "ml" in title.lower(), f"Expected Azure ML page, got: {title}"
                
                test_logger.info("✓ Working navigation test completed successfully")
                
            finally:
                # Clean up
                await context.close()
                await browser.close()
                test_logger.info("Browser closed")

if __name__ == "__main__":
    pytest.main([__file__, "-v", "-s"])