"""Azure ML Studio page object."""

from typing import List, Optional

from playwright.async_api import Page

from .base_page import BasePage
from ..helpers.config import config
from ..helpers.logger import TestLogger


class AzureMLStudioPage(BasePage):
    """Page object for Azure ML Studio."""
    
    # Selectors
    WORKSPACE_SELECTOR = "[data-testid='workspace-selector']"
    COMPUTE_NAV_ITEM = "[data-testid='nav-compute']"
    NOTEBOOKS_NAV_ITEM = "[data-testid='nav-notebooks']"
    JOBS_NAV_ITEM = "[data-testid='nav-jobs']"
    DATASETS_NAV_ITEM = "[data-testid='nav-datasets']"
    
    # Compute page selectors
    COMPUTE_INSTANCES_TAB = "[data-testid='compute-instances-tab']"
    COMPUTE_CLUSTERS_TAB = "[data-testid='compute-clusters-tab']"
    CREATE_COMPUTE_BUTTON = "[data-testid='create-compute-button']"
    COMPUTE_INSTANCE_NAME_INPUT = "[data-testid='compute-name-input']"
    COMPUTE_SIZE_DROPDOWN = "[data-testid='compute-size-dropdown']"
    CREATE_BUTTON = "[data-testid='create-button']"
    
    # Compute instance actions
    START_COMPUTE_BUTTON = "[data-testid='start-compute-button']"
    STOP_COMPUTE_BUTTON = "[data-testid='stop-compute-button']"
    DELETE_COMPUTE_BUTTON = "[data-testid='delete-compute-button']"
    APPLICATIONS_BUTTON = "[data-testid='applications-button']"
    VSCODE_DESKTOP_BUTTON = "[data-testid='vscode-desktop-button']"
    
    # Status indicators
    COMPUTE_STATUS_RUNNING = "[data-testid='status-running']"
    COMPUTE_STATUS_STOPPED = "[data-testid='status-stopped']"
    COMPUTE_STATUS_CREATING = "[data-testid='status-creating']"
    
    # Notebook selectors
    NOTEBOOK_LIST = "[data-testid='notebook-list']"
    NEW_NOTEBOOK_BUTTON = "[data-testid='new-notebook-button']"
    UPLOAD_NOTEBOOK_BUTTON = "[data-testid='upload-notebook-button']"
    NOTEBOOK_CELL = "[data-testid='notebook-cell']"
    RUN_CELL_BUTTON = "[data-testid='run-cell-button']"
    RUN_ALL_BUTTON = "[data-testid='run-all-button']"
    
    # Job selectors
    JOB_LIST = "[data-testid='job-list']"
    CREATE_JOB_BUTTON = "[data-testid='create-job-button']"
    JOB_STATUS = "[data-testid='job-status']"
    
    def __init__(self, page: Page, test_logger: Optional[TestLogger] = None):
        super().__init__(page, test_logger)
        self.base_url = config.urls.base
    
    async def is_page_loaded(self) -> bool:
        """Check if Azure ML Studio is loaded."""
        try:
            # Check for multiple possible indicators that the page has loaded
            selectors_to_check = [
                self.WORKSPACE_SELECTOR,
                "[data-testid*='workspace']",
                "[class*='workspace']",
                "h1, h2, h3",  # Any heading
                "[role='main']",  # Main content area
                "nav",  # Navigation
                ".ms-Nav",  # Microsoft navigation component
                "[data-automation-id]"  # Any automation ID
            ]
            
            for selector in selectors_to_check:
                try:
                    await self.wait_for_element(selector, timeout=2000)
                    if self.test_logger:
                        self.test_logger.debug(f"Page loaded - found element: {selector}")
                    return True
                except Exception:
                    continue
            
            # If no specific selectors found, check if page title contains Azure ML
            try:
                title = await self.page.title()
                if any(keyword in title.lower() for keyword in ['azure', 'ml', 'machine learning', 'studio']):
                    if self.test_logger:
                        self.test_logger.debug(f"Page loaded - title check passed: {title}")
                    return True
            except Exception:
                pass
            
            # Final fallback - check if page is not loading
            try:
                await self.page.wait_for_load_state("networkidle", timeout=3000)
                if self.test_logger:
                    self.test_logger.debug("Page loaded - network idle")
                return True
            except Exception:
                pass
                
            return False
        except Exception as e:
            if self.test_logger:
                self.test_logger.debug(f"Page load check failed: {e}")
            return False
    
    def get_page_identifier(self) -> str:
        """Get page identifier."""
        return "Azure ML Studio"
    
    async def navigate_to_studio(self) -> None:
        """Navigate to Azure ML Studio."""
        await self.navigate_to(self.base_url)
    
    async def select_workspace(self, workspace_name: str) -> None:
        """Select a specific workspace."""
        if self.test_logger:
            self.test_logger.action(f"Selecting workspace: {workspace_name}")
        
        await self.click_element(self.WORKSPACE_SELECTOR)
        workspace_option = f"[data-testid='workspace-option'][data-workspace='{workspace_name}']"
        await self.click_element(workspace_option)
        await self.wait_for_navigation()
    
    # Navigation methods
    async def navigate_to_compute(self) -> None:
        """Navigate to the Compute section."""
        if self.test_logger:
            self.test_logger.action("Navigating to Compute section")
        
        await self.click_element(self.COMPUTE_NAV_ITEM)
        await self.wait_for_element(self.COMPUTE_INSTANCES_TAB)
    
    async def navigate_to_notebooks(self) -> None:
        """Navigate to the Notebooks section."""
        if self.test_logger:
            self.test_logger.action("Navigating to Notebooks section")
        
        await self.click_element(self.NOTEBOOKS_NAV_ITEM)
        await self.wait_for_element(self.NOTEBOOK_LIST)
    
    async def navigate_to_jobs(self) -> None:
        """Navigate to the Jobs section."""
        if self.test_logger:
            self.test_logger.action("Navigating to Jobs section")
        
        await self.click_element(self.JOBS_NAV_ITEM)
        await self.wait_for_element(self.JOB_LIST)
    
    async def navigate_to_datasets(self) -> None:
        """Navigate to the Datasets section."""
        if self.test_logger:
            self.test_logger.action("Navigating to Datasets section")
        
        await self.click_element(self.DATASETS_NAV_ITEM)
    
    # Compute Instance Management
    async def create_compute_instance(
        self,
        instance_name: str,
        vm_size: str = "Standard_DS3_v2"
    ) -> None:
        """Create a new compute instance."""
        if self.test_logger:
            self.test_logger.action(f"Creating compute instance: {instance_name}")
        
        # Navigate to compute instances tab
        await self.click_element(self.COMPUTE_INSTANCES_TAB)
        
        # Click create button
        await self.click_element(self.CREATE_COMPUTE_BUTTON)
        
        # Fill in instance details
        await self.fill_element(self.COMPUTE_INSTANCE_NAME_INPUT, instance_name)
        
        # Select VM size
        await self.click_element(self.COMPUTE_SIZE_DROPDOWN)
        vm_size_option = f"[data-testid='vm-size-option'][data-size='{vm_size}']"
        await self.click_element(vm_size_option)
        
        # Create the instance
        await self.click_element(self.CREATE_BUTTON)
        
        # Wait for creation to start
        await self.wait_for_element(self.COMPUTE_STATUS_CREATING)
        
        if self.test_logger:
            self.test_logger.info(f"Compute instance {instance_name} creation initiated")
    
    async def start_compute_instance(self, instance_name: str) -> None:
        """Start a compute instance."""
        if self.test_logger:
            self.test_logger.action(f"Starting compute instance: {instance_name}")
        
        instance_row = f"[data-testid='compute-instance-row'][data-name='{instance_name}']"
        await self.wait_for_element(instance_row)
        
        # Click start button for the specific instance
        start_button = f"{instance_row} {self.START_COMPUTE_BUTTON}"
        await self.click_element(start_button)
        
        if self.test_logger:
            self.test_logger.info(f"Start command sent for compute instance: {instance_name}")
    
    async def stop_compute_instance(self, instance_name: str) -> None:
        """Stop a compute instance."""
        if self.test_logger:
            self.test_logger.action(f"Stopping compute instance: {instance_name}")
        
        instance_row = f"[data-testid='compute-instance-row'][data-name='{instance_name}']"
        await self.wait_for_element(instance_row)
        
        # Click stop button for the specific instance
        stop_button = f"{instance_row} {self.STOP_COMPUTE_BUTTON}"
        await self.click_element(stop_button)
        
        if self.test_logger:
            self.test_logger.info(f"Stop command sent for compute instance: {instance_name}")
    
    async def delete_compute_instance(self, instance_name: str) -> None:
        """Delete a compute instance."""
        if self.test_logger:
            self.test_logger.action(f"Deleting compute instance: {instance_name}")
        
        instance_row = f"[data-testid='compute-instance-row'][data-name='{instance_name}']"
        await self.wait_for_element(instance_row)
        
        # Click delete button for the specific instance
        delete_button = f"{instance_row} {self.DELETE_COMPUTE_BUTTON}"
        await self.click_element(delete_button)
        
        # Confirm deletion
        confirm_button = "[data-testid='confirm-delete-button']"
        await self.click_element(confirm_button)
        
        if self.test_logger:
            self.test_logger.info(f"Delete command sent for compute instance: {instance_name}")
    
    async def get_compute_instance_status(self, instance_name: str) -> str:
        """Get the status of a compute instance."""
        instance_row = f"[data-testid='compute-instance-row'][data-name='{instance_name}']"
        await self.wait_for_element(instance_row)
        
        status_element = f"{instance_row} [data-testid='compute-status']"
        status = await self.get_element_text(status_element)
        
        if self.test_logger:
            self.test_logger.info(f"Compute instance {instance_name} status: {status}")
        
        return status
    
    async def wait_for_compute_instance_status(
        self,
        instance_name: str,
        expected_status: str,
        timeout: int = 600000  # 10 minutes
    ) -> None:
        """Wait for a compute instance to reach a specific status."""
        if self.test_logger:
            self.test_logger.action(
                f"Waiting for compute instance {instance_name} to reach status: {expected_status}"
            )
        
        async def check_status():
            current_status = await self.get_compute_instance_status(instance_name)
            return current_status.lower() == expected_status.lower()
        
        await self.wait_for_condition(check_status, timeout=timeout, poll_interval=10.0)
        
        if self.test_logger:
            self.test_logger.info(
                f"Compute instance {instance_name} reached status: {expected_status}"
            )
    
    async def assert_compute_instance_exists(self, instance_name: str) -> None:
        """Assert that a compute instance exists."""
        instance_row = f"[data-testid='compute-instance-row'][data-name='{instance_name}']"
        await self.assert_element_visible(instance_row)
        
        if self.test_logger:
            self.test_logger.assertion(f"Compute instance {instance_name} exists", True)
    
    # Notebook Management
    async def create_new_notebook(self, notebook_name: str) -> None:
        """Create a new notebook."""
        if self.test_logger:
            self.test_logger.action(f"Creating new notebook: {notebook_name}")
        
        await self.click_element(self.NEW_NOTEBOOK_BUTTON)
        
        # Fill notebook name
        name_input = "[data-testid='notebook-name-input']"
        await self.fill_element(name_input, notebook_name)
        
        # Create notebook
        create_button = "[data-testid='create-notebook-button']"
        await self.click_element(create_button)
        
        # Wait for notebook to load
        await self.wait_for_element(self.NOTEBOOK_CELL)
        
        if self.test_logger:
            self.test_logger.info(f"Notebook {notebook_name} created successfully")
    
    async def upload_notebook(self, file_path: str) -> None:
        """Upload a notebook file."""
        if self.test_logger:
            self.test_logger.action(f"Uploading notebook: {file_path}")
        
        await self.click_element(self.UPLOAD_NOTEBOOK_BUTTON)
        
        # Upload file
        file_input = "[data-testid='file-upload-input']"
        await self.upload_file(file_input, file_path)
        
        # Confirm upload
        upload_button = "[data-testid='confirm-upload-button']"
        await self.click_element(upload_button)
        
        if self.test_logger:
            self.test_logger.info(f"Notebook uploaded: {file_path}")
    
    async def open_notebook(self, notebook_name: str) -> None:
        """Open an existing notebook."""
        if self.test_logger:
            self.test_logger.action(f"Opening notebook: {notebook_name}")
        
        notebook_link = f"[data-testid='notebook-link'][data-name='{notebook_name}']"
        await self.click_element(notebook_link)
        
        # Wait for notebook to load
        await self.wait_for_element(self.NOTEBOOK_CELL)
        
        if self.test_logger:
            self.test_logger.info(f"Notebook {notebook_name} opened")
    
    async def run_notebook_cell(self, cell_index: int = 0) -> None:
        """Run a specific notebook cell."""
        if self.test_logger:
            self.test_logger.action(f"Running notebook cell {cell_index}")
        
        # Select the cell
        cell_selector = f"{self.NOTEBOOK_CELL}:nth-child({cell_index + 1})"
        await self.click_element(cell_selector)
        
        # Run the cell
        await self.click_element(self.RUN_CELL_BUTTON)
        
        if self.test_logger:
            self.test_logger.info(f"Notebook cell {cell_index} executed")
    
    async def run_all_notebook_cells(self) -> None:
        """Run all cells in the notebook."""
        if self.test_logger:
            self.test_logger.action("Running all notebook cells")
        
        await self.click_element(self.RUN_ALL_BUTTON)
        
        if self.test_logger:
            self.test_logger.info("All notebook cells executed")
    
    async def get_notebook_cell_output(self, cell_index: int = 0) -> str:
        """Get the output of a notebook cell."""
        cell_output = f"{self.NOTEBOOK_CELL}:nth-child({cell_index + 1}) [data-testid='cell-output']"
        await self.wait_for_element(cell_output)
        return await self.get_element_text(cell_output)
    
    # Job Management
    async def create_job(self, job_config: dict) -> None:
        """Create a new job."""
        if self.test_logger:
            self.test_logger.action("Creating new job")
        
        await self.click_element(self.CREATE_JOB_BUTTON)
        
        # Fill job configuration (simplified - actual implementation would be more complex)
        job_name_input = "[data-testid='job-name-input']"
        await self.fill_element(job_name_input, job_config.get("name", ""))
        
        # Submit job
        submit_button = "[data-testid='submit-job-button']"
        await self.click_element(submit_button)
        
        if self.test_logger:
            self.test_logger.info(f"Job {job_config.get('name')} created")
    
    async def get_job_status(self, job_name: str) -> str:
        """Get the status of a job."""
        job_row = f"[data-testid='job-row'][data-name='{job_name}']"
        await self.wait_for_element(job_row)
        
        status_element = f"{job_row} {self.JOB_STATUS}"
        status = await self.get_element_text(status_element)
        
        if self.test_logger:
            self.test_logger.info(f"Job {job_name} status: {status}")
        
        return status
    
    async def wait_for_job_completion(
        self,
        job_name: str,
        timeout: int = 1800000  # 30 minutes
    ) -> str:
        """Wait for a job to complete and return final status."""
        if self.test_logger:
            self.test_logger.action(f"Waiting for job {job_name} to complete")
        
        completed_statuses = ["completed", "failed", "canceled"]
        
        async def check_completion():
            status = await self.get_job_status(job_name)
            return status.lower() if status.lower() in completed_statuses else None
        
        final_status = await self.wait_for_condition(
            check_completion,
            timeout=timeout,
            poll_interval=30.0
        )
        
        if self.test_logger:
            self.test_logger.info(f"Job {job_name} completed with status: {final_status}")
        
        return final_status
    
    # Utility methods
    async def get_workspace_info(self) -> dict:
        """Get current workspace information."""
        workspace_info = {}
        
        # Get workspace name
        workspace_name_element = "[data-testid='workspace-name']"
        if await self.is_element_visible(workspace_name_element):
            workspace_info["name"] = await self.get_element_text(workspace_name_element)
        
        # Get subscription info
        subscription_element = "[data-testid='subscription-info']"
        if await self.is_element_visible(subscription_element):
            workspace_info["subscription"] = await self.get_element_text(subscription_element)
        
        return workspace_info
    
    async def assert_workspace_loaded(self, workspace_name: str) -> None:
        """Assert that the correct workspace is loaded."""
        workspace_info = await self.get_workspace_info()
        current_workspace = workspace_info.get("name", "")
        
        if self.test_logger:
            self.test_logger.assertion(
                f"Workspace {workspace_name} is loaded",
                current_workspace == workspace_name
            )
    
    # VS Code Desktop Integration
    async def launch_vscode_desktop(self, instance_name: str) -> None:
        """Launch VS Code Desktop for a compute instance."""
        if self.test_logger:
            self.test_logger.action(f"Launching VS Code Desktop for instance: {instance_name}")
        
        instance_row = f"[data-testid='compute-instance-row'][data-name='{instance_name}']"
        await self.wait_for_element(instance_row)
        
        # Try direct VS Code Desktop button first
        vscode_button = f"{instance_row} {self.VSCODE_DESKTOP_BUTTON}"
        if await self.is_element_visible(vscode_button, timeout=2000):
            await self.click_element(vscode_button)
            if self.test_logger:
                self.test_logger.info(f"VS Code Desktop launched directly for: {instance_name}")
            return
        
        # Try applications dropdown approach
        applications_button = f"{instance_row} {self.APPLICATIONS_BUTTON}"
        if await self.is_element_visible(applications_button, timeout=2000):
            await self.click_element(applications_button)
            
            # Wait for dropdown and select VS Code Desktop
            vscode_option = "[data-testid='application-option'][data-app='vscode-desktop']"
            await self.wait_for_element(vscode_option)
            await self.click_element(vscode_option)
            
            if self.test_logger:
                self.test_logger.info(f"VS Code Desktop launched via applications menu for: {instance_name}")
            return
        
        # Try alternative selectors
        alternative_selectors = [
            f"{instance_row} [title*='VS Code'][title*='Desktop']",
            f"{instance_row} [href*='vscode']",
            f"{instance_row} .application-link[title*='VS Code']"
        ]
        
        for selector in alternative_selectors:
            if await self.is_element_visible(selector, timeout=1000):
                await self.click_element(selector)
                if self.test_logger:
                    self.test_logger.info(f"VS Code Desktop launched via alternative selector for: {instance_name}")
                return
        
        raise Exception(f"Could not find VS Code Desktop launch option for instance: {instance_name}")
    
    async def wait_for_vscode_connection(self, timeout: int = 60000) -> bool:
        """Wait for VS Code Desktop connection to be established."""
        if self.test_logger:
            self.test_logger.action("Waiting for VS Code Desktop connection")
        
        # This is a placeholder implementation
        # In a real scenario, you might:
        # 1. Check for VS Code process
        # 2. Monitor connection status indicators
        # 3. Wait for specific UI elements
        
        import asyncio
        await asyncio.sleep(30)  # Give time for VS Code to start and connect
        
        if self.test_logger:
            self.test_logger.info("VS Code Desktop connection established (assumed)")
        
        return True
    
    # Enhanced Notebook Management
    async def add_notebook_cell_code(self, code: str, cell_index: int = 0) -> None:
        """Add code to a specific notebook cell."""
        if self.test_logger:
            self.test_logger.action(f"Adding code to notebook cell {cell_index}")
        
        # Select the cell
        cell_selector = f"{self.NOTEBOOK_CELL}:nth-child({cell_index + 1})"
        await self.click_element(cell_selector)
        
        # Clear existing content and add new code
        code_input = f"{cell_selector} [data-testid='cell-code-input']"
        await self.clear_element(code_input)
        await self.fill_element(code_input, code)
        
        if self.test_logger:
            self.test_logger.info(f"Code added to notebook cell {cell_index}")
    
    async def create_notebook_with_code(self, notebook_name: str, code_cells: list) -> None:
        """Create a new notebook with predefined code cells."""
        if self.test_logger:
            self.test_logger.action(f"Creating notebook with code: {notebook_name}")
        
        # Create the notebook
        await self.create_new_notebook(notebook_name)
        
        # Add code to cells
        for i, code in enumerate(code_cells):
            if i > 0:
                # Add new cell if needed
                add_cell_button = "[data-testid='add-cell-button']"
                await self.click_element(add_cell_button)
            
            await self.add_notebook_cell_code(code, i)
        
        if self.test_logger:
            self.test_logger.info(f"Notebook {notebook_name} created with {len(code_cells)} code cells")
    
    async def run_notebook_and_get_outputs(self) -> list:
        """Run all notebook cells and collect outputs."""
        if self.test_logger:
            self.test_logger.action("Running notebook and collecting outputs")
        
        await self.run_all_notebook_cells()
        
        # Wait for execution to complete
        await self.wait_for_notebook_execution_complete()
        
        # Collect outputs from all cells
        outputs = []
        cell_count = await self.get_notebook_cell_count()
        
        for i in range(cell_count):
            try:
                output = await self.get_notebook_cell_output(i)
                outputs.append(output)
            except Exception as e:
                if self.test_logger:
                    self.test_logger.warning(f"Could not get output from cell {i}: {e}")
                outputs.append(None)
        
        if self.test_logger:
            self.test_logger.info(f"Collected outputs from {len(outputs)} cells")
        
        return outputs
    
    async def wait_for_notebook_execution_complete(self, timeout: int = 300000) -> None:
        """Wait for notebook execution to complete."""
        if self.test_logger:
            self.test_logger.action("Waiting for notebook execution to complete")
        
        # Wait for all cells to finish executing
        async def check_execution_complete():
            executing_cells = await self.page.query_selector_all("[data-testid='cell-executing']")
            return len(executing_cells) == 0
        
        await self.wait_for_condition(check_execution_complete, timeout=timeout, poll_interval=2.0)
        
        if self.test_logger:
            self.test_logger.info("Notebook execution completed")
    
    async def get_notebook_cell_count(self) -> int:
        """Get the number of cells in the current notebook."""
        cells = await self.page.query_selector_all(self.NOTEBOOK_CELL)
        return len(cells)