"""Azure ML helper utilities for automation framework."""

import asyncio
import json
import tempfile
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Dict, List, Optional

import requests
from azure.mgmt.compute import ComputeManagementClient
from azure.mgmt.machinelearningservices import AzureMachineLearningWorkspaces
from azure.mgmt.resource import ResourceManagementClient
from azure.storage.blob import BlobServiceClient

from .auth import auth_manager
from .cli_runner import cli_runner, CliOptions
from .config import config
from .logger import logger, TestLogger


@dataclass
class ComputeInstance:
    """Represents an Azure ML compute instance."""
    name: str
    state: str
    vm_size: str
    location: str
    created_by: str
    created_on: str
    last_modified_by: str
    last_modified_on: str


@dataclass
class ComputeCluster:
    """Represents an Azure ML compute cluster."""
    name: str
    state: str
    vm_size: str
    min_nodes: int
    max_nodes: int
    current_nodes: int
    location: str


@dataclass
class MLJob:
    """Represents an Azure ML job."""
    id: str
    name: str
    status: str
    start_time: str
    end_time: Optional[str]
    compute_target: str


class AzureMLHelper:
    """Helper class for Azure ML operations."""
    
    def __init__(self, test_logger: Optional[TestLogger] = None):
        self.test_logger = test_logger
        self.credential = auth_manager.get_credential()
        
        # Initialize Azure clients
        self.ml_client = AzureMachineLearningWorkspaces(
            credential=self.credential,
            subscription_id=config.azure.subscription_id
        )
        
        self.compute_client = ComputeManagementClient(
            credential=self.credential,
            subscription_id=config.azure.subscription_id
        )
        
        self.resource_client = ResourceManagementClient(
            credential=self.credential,
            subscription_id=config.azure.subscription_id
        )
        
        # Initialize blob client if storage account is configured
        self.blob_client: Optional[BlobServiceClient] = None
        if config.artifacts.storage_account:
            self.blob_client = BlobServiceClient(
                account_url=f"https://{config.artifacts.storage_account}.blob.core.windows.net",
                credential=self.credential
            )
    
    # Compute Instance Management
    async def start_compute_instance(self, instance_name: str) -> None:
        """Start a compute instance."""
        try:
            if self.test_logger:
                self.test_logger.action(f"Starting compute instance: {instance_name}")
            
            options = CliOptions(test_logger=self.test_logger)
            result = await cli_runner.az_ml_compute_start(
                instance_name,
                config.azure.workspace_name,
                config.azure.resource_group,
                options
            )
            
            if result.exit_code != 0:
                raise RuntimeError(f"Failed to start compute instance: {result.stderr}")
            
            if self.test_logger:
                self.test_logger.info(f"Compute instance {instance_name} start command completed")
                
        except Exception as error:
            if self.test_logger:
                self.test_logger.error(
                    f"Failed to start compute instance {instance_name}",
                    error=str(error)
                )
            raise
    
    async def stop_compute_instance(self, instance_name: str) -> None:
        """Stop a compute instance."""
        try:
            if self.test_logger:
                self.test_logger.action(f"Stopping compute instance: {instance_name}")
            
            options = CliOptions(test_logger=self.test_logger)
            result = await cli_runner.az_ml_compute_stop(
                instance_name,
                config.azure.workspace_name,
                config.azure.resource_group,
                options
            )
            
            if result.exit_code != 0:
                raise RuntimeError(f"Failed to stop compute instance: {result.stderr}")
            
            if self.test_logger:
                self.test_logger.info(f"Compute instance {instance_name} stop command completed")
                
        except Exception as error:
            if self.test_logger:
                self.test_logger.error(
                    f"Failed to stop compute instance {instance_name}",
                    error=str(error)
                )
            raise
    
    async def get_compute_instance_status(self, instance_name: str) -> ComputeInstance:
        """Get the status of a compute instance."""
        try:
            if self.test_logger:
                self.test_logger.action(f"Getting compute instance status: {instance_name}")
            
            options = CliOptions(test_logger=self.test_logger)
            result = await cli_runner.az_ml_compute_show(
                instance_name,
                config.azure.workspace_name,
                config.azure.resource_group,
                options
            )
            
            if result.exit_code != 0:
                raise RuntimeError(f"Failed to get compute instance status: {result.stderr}")
            
            compute_data = cli_runner.parse_json_output(result)
            
            compute_instance = ComputeInstance(
                name=compute_data.get("name", ""),
                state=compute_data.get("properties", {}).get("state", "Unknown"),
                vm_size=compute_data.get("properties", {}).get("vmSize", "Unknown"),
                location=compute_data.get("location", ""),
                created_by=compute_data.get("properties", {}).get("createdBy", {}).get("userObjectId", "Unknown"),
                created_on=compute_data.get("properties", {}).get("createdOn", "Unknown"),
                last_modified_by=compute_data.get("properties", {}).get("lastModifiedBy", {}).get("userObjectId", "Unknown"),
                last_modified_on=compute_data.get("properties", {}).get("lastModifiedOn", "Unknown")
            )
            
            if self.test_logger:
                self.test_logger.info(
                    f"Compute instance {instance_name} status retrieved",
                    state=compute_instance.state,
                    vm_size=compute_instance.vm_size
                )
            
            return compute_instance
            
        except Exception as error:
            if self.test_logger:
                self.test_logger.error(
                    f"Failed to get compute instance status for {instance_name}",
                    error=str(error)
                )
            raise
    
    async def wait_for_compute_instance_state(
        self,
        instance_name: str,
        target_state: str,
        timeout_seconds: int = 600  # 10 minutes
    ) -> ComputeInstance:
        """Wait for a compute instance to reach a target state."""
        start_time = asyncio.get_event_loop().time()
        poll_interval = 30  # 30 seconds
        
        if self.test_logger:
            self.test_logger.info(
                f"Waiting for compute instance {instance_name} to reach state: {target_state}",
                timeout=timeout_seconds,
                poll_interval=poll_interval
            )
        
        while asyncio.get_event_loop().time() - start_time < timeout_seconds:
            instance = await self.get_compute_instance_status(instance_name)
            
            if instance.state.lower() == target_state.lower():
                if self.test_logger:
                    self.test_logger.info(
                        f"Compute instance {instance_name} reached target state: {target_state}",
                        duration=asyncio.get_event_loop().time() - start_time
                    )
                return instance
            
            if self.test_logger:
                self.test_logger.debug(
                    f"Compute instance {instance_name} current state: {instance.state}, waiting..."
                )
            
            await asyncio.sleep(poll_interval)
        
        raise TimeoutError(
            f"Timeout waiting for compute instance {instance_name} to reach state {target_state} "
            f"after {timeout_seconds}s"
        )
    
    # Job Management
    async def create_job(self, job_definition: Dict[str, Any]) -> MLJob:
        """Create an ML job."""
        try:
            if self.test_logger:
                self.test_logger.action("Creating ML job", job_definition=job_definition)
            
            # Create temporary job file
            with tempfile.NamedTemporaryFile(mode='w', suffix='.yml', delete=False) as job_file:
                json.dump(job_definition, job_file, indent=2)
                job_file_path = job_file.name
            
            try:
                options = CliOptions(test_logger=self.test_logger)
                result = await cli_runner.az_ml_job_create(
                    job_file_path,
                    config.azure.workspace_name,
                    config.azure.resource_group,
                    options
                )
                
                if result.exit_code != 0:
                    raise RuntimeError(f"Failed to create job: {result.stderr}")
                
                job_data = cli_runner.parse_json_output(result)
                
                job = MLJob(
                    id=job_data.get("id", ""),
                    name=job_data.get("name", ""),
                    status=job_data.get("status", ""),
                    start_time=job_data.get("startTime", ""),
                    end_time=job_data.get("endTime"),
                    compute_target=job_data.get("computeTarget", "")
                )
                
                if self.test_logger:
                    self.test_logger.info(
                        "ML job created successfully",
                        job_id=job.id,
                        job_name=job.name
                    )
                
                return job
                
            finally:
                # Clean up temp file
                Path(job_file_path).unlink(missing_ok=True)
                
        except Exception as error:
            if self.test_logger:
                self.test_logger.error("Failed to create ML job", error=str(error))
            raise
    
    # PIM (Privileged Identity Management) Helper
    async def request_pim_activation(
        self,
        role_name: str,
        scope: str,
        justification: Optional[str] = None
    ) -> None:
        """Request PIM role activation."""
        if justification is None:
            justification = config.pim.justification
        
        try:
            if self.test_logger:
                self.test_logger.action(
                    "Requesting PIM role activation",
                    role=role_name,
                    scope=scope
                )
            
            access_token = await auth_manager.get_access_token(["https://graph.microsoft.com/.default"])
            
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
                    "startDateTime": None,
                    "duration": "PT8H"  # 8 hours
                }
            }
            
            response = requests.post(
                "https://graph.microsoft.com/v1.0/privilegedAccess/azureResources/roleAssignmentRequests",
                headers=headers,
                json=payload
            )
            response.raise_for_status()
            
            request_data = response.json()
            request_id = request_data.get("id")
            
            if self.test_logger:
                self.test_logger.info(
                    "PIM activation request submitted",
                    role=role_name,
                    scope=scope,
                    request_id=request_id
                )
                
        except Exception as error:
            if self.test_logger:
                self.test_logger.error(
                    "Failed to request PIM activation",
                    error=str(error),
                    role=role_name,
                    scope=scope
                )
            raise
    
    async def check_pim_activation_status(self, request_id: str) -> str:
        """Check the status of a PIM activation request."""
        try:
            access_token = await auth_manager.get_access_token(["https://graph.microsoft.com/.default"])
            
            headers = {
                "Authorization": f"Bearer {access_token}"
            }
            
            response = requests.get(
                f"https://graph.microsoft.com/v1.0/privilegedAccess/azureResources/roleAssignmentRequests/{request_id}",
                headers=headers
            )
            response.raise_for_status()
            
            request_data = response.json()
            status = request_data.get("status", "Unknown")
            
            if self.test_logger:
                self.test_logger.info(
                    "PIM activation status checked",
                    request_id=request_id,
                    status=status
                )
            
            return status
            
        except Exception as error:
            if self.test_logger:
                self.test_logger.error(
                    "Failed to check PIM activation status",
                    error=str(error),
                    request_id=request_id
                )
            raise
    
    # Resource Management
    async def create_resource_group(
        self,
        resource_group_name: str,
        location: str = "eastus"
    ) -> None:
        """Create a resource group."""
        try:
            if self.test_logger:
                self.test_logger.action(f"Creating resource group: {resource_group_name}")
            
            self.resource_client.resource_groups.create_or_update(
                resource_group_name,
                {
                    "location": location,
                    "tags": {
                        "purpose": "automation-testing",
                        "created_by": "azure-ml-automation-framework"
                    }
                }
            )
            
            if self.test_logger:
                self.test_logger.info(f"Resource group {resource_group_name} created successfully")
                
        except Exception as error:
            if self.test_logger:
                self.test_logger.error(
                    f"Failed to create resource group {resource_group_name}",
                    error=str(error)
                )
            raise
    
    async def delete_resource_group(self, resource_group_name: str) -> None:
        """Delete a resource group."""
        try:
            if self.test_logger:
                self.test_logger.action(f"Deleting resource group: {resource_group_name}")
            
            # This is a long-running operation
            delete_operation = self.resource_client.resource_groups.begin_delete(resource_group_name)
            delete_operation.wait()
            
            if self.test_logger:
                self.test_logger.info(f"Resource group {resource_group_name} deleted successfully")
                
        except Exception as error:
            if self.test_logger:
                self.test_logger.error(
                    f"Failed to delete resource group {resource_group_name}",
                    error=str(error)
                )
            raise
    
    # Artifact Management
    async def upload_artifact(
        self,
        file_path: str,
        container_name: Optional[str] = None
    ) -> str:
        """Upload an artifact to Azure Blob Storage."""
        if not self.blob_client:
            raise RuntimeError("Blob client not initialized. Storage account not configured.")
        
        if container_name is None:
            container_name = config.artifacts.container
        
        try:
            if self.test_logger:
                self.test_logger.action(f"Uploading artifact: {file_path}")
            
            container_client = self.blob_client.get_container_client(container_name)
            
            # Create container if it doesn't exist
            try:
                container_client.create_container()
            except Exception:
                pass  # Container might already exist
            
            # Generate unique blob name
            import time
            blob_name = f"{int(time.time())}-{Path(file_path).name}"
            blob_client = container_client.get_blob_client(blob_name)
            
            # Upload file
            with open(file_path, "rb") as data:
                blob_client.upload_blob(data, overwrite=True)
            
            artifact_url = blob_client.url
            
            if self.test_logger:
                self.test_logger.info(
                    "Artifact uploaded successfully",
                    file_path=file_path,
                    artifact_url=artifact_url
                )
            
            return artifact_url
            
        except Exception as error:
            if self.test_logger:
                self.test_logger.error(
                    f"Failed to upload artifact: {file_path}",
                    error=str(error)
                )
            raise
    
    # Workspace validation
    async def validate_workspace_access(self) -> bool:
        """Validate access to the Azure ML workspace."""
        try:
            if self.test_logger:
                self.test_logger.action("Validating workspace access")
            
            workspace = self.ml_client.workspaces.get(
                config.azure.resource_group,
                config.azure.workspace_name
            )
            
            if self.test_logger:
                self.test_logger.info(
                    "Workspace access validated",
                    workspace_name=workspace.name,
                    location=workspace.location
                )
            
            return True
            
        except Exception as error:
            if self.test_logger:
                self.test_logger.error("Failed to validate workspace access", error=str(error))
            return False


def create_azure_ml_helper(test_logger: Optional[TestLogger] = None) -> AzureMLHelper:
    """Factory function to create Azure ML helper."""
    return AzureMLHelper(test_logger)