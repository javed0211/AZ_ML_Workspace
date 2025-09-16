#!/usr/bin/env python3
"""
Basic usage example for Azure ML Automation Framework.

This script demonstrates how to use the framework programmatically.
"""

import asyncio
import sys
from pathlib import Path

# Add src to path for development
sys.path.insert(0, str(Path(__file__).parent.parent / "src"))

from azure_ml_automation import create_azure_ml_helper
from azure_ml_automation.helpers.browser_manager import BrowserManager
from azure_ml_automation.helpers.config import config
from azure_ml_automation.helpers.logger import get_logger

logger = get_logger(__name__)


async def main():
    """Main example function."""
    logger.info("Starting Azure ML Automation Framework example")
    
    try:
        # Create Azure ML helper
        logger.info("Creating Azure ML helper...")
        helper = await create_azure_ml_helper()
        
        # Get workspace information
        logger.info("Getting workspace information...")
        try:
            workspace = await helper.get_workspace()
            logger.info(f"Connected to workspace: {workspace.name}")
            logger.info(f"Resource group: {workspace.resource_group}")
            logger.info(f"Location: {workspace.location}")
        except Exception as e:
            logger.warning(f"Could not get workspace info: {e}")
        
        # List compute instances
        logger.info("Listing compute instances...")
        try:
            compute_instances = await helper.list_compute_instances()
            if compute_instances:
                for compute in compute_instances:
                    logger.info(f"Compute: {compute.name} - Status: {compute.provisioning_state}")
            else:
                logger.info("No compute instances found")
        except Exception as e:
            logger.warning(f"Could not list compute instances: {e}")
        
        # Browser automation example
        logger.info("Starting browser automation example...")
        try:
            async with BrowserManager(headless=True) as browser_manager:
                browser = await browser_manager.get_browser()
                page = await browser.new_page()
                
                # Navigate to Azure ML Studio
                logger.info("Navigating to Azure ML Studio...")
                await page.goto("https://ml.azure.com", wait_until="networkidle")
                
                # Get page title
                title = await page.title()
                logger.info(f"Page title: {title}")
                
                # Take a screenshot
                screenshot_path = Path("example_screenshot.png")
                await page.screenshot(path=str(screenshot_path))
                logger.info(f"Screenshot saved to: {screenshot_path}")
                
        except Exception as e:
            logger.error(f"Browser automation failed: {e}")
        
        logger.info("Example completed successfully!")
        
    except Exception as e:
        logger.error(f"Example failed: {e}")
        return 1
    
    return 0


if __name__ == "__main__":
    exit_code = asyncio.run(main())
    sys.exit(exit_code)