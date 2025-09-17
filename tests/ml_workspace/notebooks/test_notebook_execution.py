"""Tests for notebook execution functionality."""

import pytest
from playwright.async_api import Page

from src.azure_ml_automation.helpers.logger import TestLogger
from src.azure_ml_automation.pages.azure_ml_studio import AzureMLStudioPage


@pytest.mark.notebook
@pytest.mark.azure
class TestNotebookExecution:
    """Tests for notebook execution in Azure ML Studio."""
    
    async def test_create_new_notebook(
        self,
        azure_ml_page: AzureMLStudioPage,
        test_logger: TestLogger
    ):
        """Test creating a new notebook."""
        await azure_ml_page.navigate_to_studio()
        await azure_ml_page.navigate_to_notebooks()
        
        notebook_name = "test-notebook-creation"
        await azure_ml_page.create_new_notebook(notebook_name)
        
        test_logger.info(f"Successfully created notebook: {notebook_name}")
    
    async def test_upload_notebook(
        self,
        azure_ml_page: AzureMLStudioPage,
        test_logger: TestLogger
    ):
        """Test uploading a notebook file."""
        await azure_ml_page.navigate_to_studio()
        await azure_ml_page.navigate_to_notebooks()
        
        # Use sample notebook from test-data
        notebook_path = "c:\\Users\\admin-javed\\Repos\\AZ_ML_Workspace\\test-data\\sample-notebook.ipynb"
        await azure_ml_page.upload_notebook(notebook_path)
        
        test_logger.info("Successfully uploaded notebook")
    
    async def test_run_notebook_cell(
        self,
        azure_ml_page: AzureMLStudioPage,
        test_logger: TestLogger
    ):
        """Test running a notebook cell."""
        await azure_ml_page.navigate_to_studio()
        await azure_ml_page.navigate_to_notebooks()
        
        # Open existing notebook
        notebook_name = "sample-notebook"
        await azure_ml_page.open_notebook(notebook_name)
        
        # Run first cell
        await azure_ml_page.run_notebook_cell(0)
        
        # Get output
        output = await azure_ml_page.get_notebook_cell_output(0)
        assert output, "Cell should produce output"
        
        test_logger.info(f"Cell output: {output}")
    
    async def test_run_all_notebook_cells(
        self,
        azure_ml_page: AzureMLStudioPage,
        test_logger: TestLogger
    ):
        """Test running all cells in a notebook."""
        await azure_ml_page.navigate_to_studio()
        await azure_ml_page.navigate_to_notebooks()
        
        notebook_name = "sample-notebook"
        await azure_ml_page.open_notebook(notebook_name)
        
        await azure_ml_page.run_all_notebook_cells()
        
        test_logger.info("Successfully ran all notebook cells")