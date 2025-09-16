"""
Azure ML Workspace Automation Framework

A comprehensive end-to-end testing framework for Azure Machine Learning workspaces 
using Playwright and Python. This framework provides automated testing capabilities 
for Azure ML Studio, VS Code Web, JupyterLab, and VS Code Desktop environments.
"""

__version__ = "1.0.0"
__author__ = "Zencoder"
__email__ = "support@zencoder.com"

from .helpers.config import config
from .helpers.logger import logger, TestLogger
from .helpers.auth import AuthManager, create_auth_manager
from .helpers.azure_helpers import AzureMLHelper, create_azure_ml_helper
from .helpers.cli_runner import CliRunner, CliResult

__all__ = [
    "config",
    "logger", 
    "TestLogger",
    "AuthManager",
    "create_auth_manager",
    "AzureMLHelper", 
    "create_azure_ml_helper",
    "CliRunner",
    "CliResult",
]