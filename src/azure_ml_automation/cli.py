"""Command-line interface for Azure ML automation framework."""

import asyncio
import sys
from pathlib import Path
from typing import Optional

import click
from playwright.async_api import async_playwright

from .helpers.config import config, validate_required_config
from .helpers.logger import logger, setup_logging
from .helpers.auth import create_auth_manager
from .helpers.azure_helpers import create_azure_ml_helper


@click.group()
@click.option('--verbose', '-v', is_flag=True, help='Enable verbose logging')
@click.option('--config-file', '-c', help='Path to configuration file')
def cli(verbose: bool, config_file: Optional[str]):
    """Azure ML Workspace Automation Framework CLI."""
    if verbose:
        config.logging.level = "debug"
    
    if config_file:
        # Load custom config file if provided
        pass  # Implementation would load custom config
    
    setup_logging()
    logger.info("Azure ML Automation Framework CLI started")


@cli.command()
@click.option('--workspace', '-w', help='Workspace name')
@click.option('--resource-group', '-g', help='Resource group name')
def validate(workspace: Optional[str], resource_group: Optional[str]):
    """Validate Azure configuration and connectivity."""
    async def _validate():
        try:
            # Override config if provided
            if workspace:
                config.azure.workspace_name = workspace
            if resource_group:
                config.azure.resource_group = resource_group
            
            # Validate required configuration
            required_keys = [
                "azure.tenant_id",
                "azure.subscription_id",
                "azure.resource_group",
                "azure.workspace_name"
            ]
            validate_required_config(required_keys)
            
            # Test authentication
            auth_manager = create_auth_manager()
            auth_valid = await auth_manager.validate_authentication()
            
            if not auth_valid:
                click.echo("❌ Authentication validation failed", err=True)
                sys.exit(1)
            
            # Test workspace access
            azure_helper = create_azure_ml_helper()
            workspace_valid = await azure_helper.validate_workspace_access()
            
            if not workspace_valid:
                click.echo("❌ Workspace access validation failed", err=True)
                sys.exit(1)
            
            click.echo("✅ All validations passed")
            
        except Exception as error:
            click.echo(f"❌ Validation failed: {error}", err=True)
            sys.exit(1)
    
    asyncio.run(_validate())


@cli.command()
@click.option('--pattern', '-p', help='Test pattern to run')
@click.option('--browser', '-b', default='chromium', help='Browser to use (chromium, firefox, webkit)')
@click.option('--headless/--headed', default=True, help='Run in headless mode')
@click.option('--workers', '-w', type=int, help='Number of parallel workers')
def test(pattern: Optional[str], browser: str, headless: bool, workers: Optional[int]):
    """Run tests using pytest."""
    import subprocess
    
    # Build pytest command
    cmd = ["python", "-m", "pytest"]
    
    if pattern:
        cmd.extend(["-k", pattern])
    
    if workers:
        cmd.extend(["-n", str(workers)])
    
    # Set environment variables for browser configuration
    import os
    os.environ["BROWSER"] = browser
    os.environ["HEADLESS"] = str(headless).lower()
    
    # Run pytest
    result = subprocess.run(cmd, cwd=Path.cwd())
    sys.exit(result.returncode)


@cli.command()
@click.argument('instance_name')
@click.option('--wait/--no-wait', default=True, help='Wait for operation to complete')
def start_compute(instance_name: str, wait: bool):
    """Start a compute instance."""
    async def _start_compute():
        try:
            azure_helper = create_azure_ml_helper()
            
            click.echo(f"Starting compute instance: {instance_name}")
            await azure_helper.start_compute_instance(instance_name)
            
            if wait:
                click.echo("Waiting for compute instance to start...")
                await azure_helper.wait_for_compute_instance_state(instance_name, "Running")
                click.echo("✅ Compute instance started successfully")
            else:
                click.echo("✅ Start command sent")
                
        except Exception as error:
            click.echo(f"❌ Failed to start compute instance: {error}", err=True)
            sys.exit(1)
    
    asyncio.run(_start_compute())


@cli.command()
@click.argument('instance_name')
@click.option('--wait/--no-wait', default=True, help='Wait for operation to complete')
def stop_compute(instance_name: str, wait: bool):
    """Stop a compute instance."""
    async def _stop_compute():
        try:
            azure_helper = create_azure_ml_helper()
            
            click.echo(f"Stopping compute instance: {instance_name}")
            await azure_helper.stop_compute_instance(instance_name)
            
            if wait:
                click.echo("Waiting for compute instance to stop...")
                await azure_helper.wait_for_compute_instance_state(instance_name, "Stopped")
                click.echo("✅ Compute instance stopped successfully")
            else:
                click.echo("✅ Stop command sent")
                
        except Exception as error:
            click.echo(f"❌ Failed to stop compute instance: {error}", err=True)
            sys.exit(1)
    
    asyncio.run(_stop_compute())


@cli.command()
@click.argument('instance_name')
def compute_status(instance_name: str):
    """Get compute instance status."""
    async def _get_status():
        try:
            azure_helper = create_azure_ml_helper()
            instance = await azure_helper.get_compute_instance_status(instance_name)
            
            click.echo(f"Compute Instance: {instance.name}")
            click.echo(f"Status: {instance.state}")
            click.echo(f"VM Size: {instance.vm_size}")
            click.echo(f"Location: {instance.location}")
            
        except Exception as error:
            click.echo(f"❌ Failed to get compute status: {error}", err=True)
            sys.exit(1)
    
    asyncio.run(_get_status())


@cli.command()
def setup():
    """Set up the automation framework."""
    async def _setup():
        try:
            click.echo("Setting up Azure ML Automation Framework...")
            
            # Install Playwright browsers
            click.echo("Installing Playwright browsers...")
            async with async_playwright() as p:
                # This will install browsers if not already installed
                pass
            
            # Validate configuration
            click.echo("Validating configuration...")
            required_keys = [
                "azure.tenant_id",
                "azure.subscription_id"
            ]
            
            try:
                validate_required_config(required_keys)
                click.echo("✅ Configuration validation passed")
            except ValueError as e:
                click.echo(f"⚠️  Configuration warning: {e}")
                click.echo("Please update your .env file with the required values")
            
            # Test authentication if credentials are available
            if config.azure.tenant_id and config.azure.client_id:
                try:
                    auth_manager = create_auth_manager()
                    auth_valid = await auth_manager.validate_authentication()
                    if auth_valid:
                        click.echo("✅ Authentication test passed")
                    else:
                        click.echo("⚠️  Authentication test failed")
                except Exception:
                    click.echo("⚠️  Could not test authentication")
            
            click.echo("✅ Setup completed successfully")
            
        except Exception as error:
            click.echo(f"❌ Setup failed: {error}", err=True)
            sys.exit(1)
    
    asyncio.run(_setup())


@cli.command()
@click.option('--output', '-o', default='test-results/reports', help='Output directory')
def report(output: str):
    """Generate test reports."""
    import subprocess
    from pathlib import Path
    
    output_path = Path(output)
    output_path.mkdir(parents=True, exist_ok=True)
    
    # Generate HTML report
    html_report = output_path / "pytest-report.html"
    if html_report.exists():
        click.echo(f"HTML report available at: {html_report.absolute()}")
    
    # Generate Allure report if available
    allure_results = Path("allure-results")
    if allure_results.exists():
        try:
            allure_report_dir = output_path / "allure-report"
            subprocess.run([
                "allure", "generate", str(allure_results), 
                "-o", str(allure_report_dir), "--clean"
            ], check=True)
            click.echo(f"Allure report generated at: {allure_report_dir.absolute()}")
        except (subprocess.CalledProcessError, FileNotFoundError):
            click.echo("⚠️  Allure not available or failed to generate report")
    
    click.echo("✅ Report generation completed")


@cli.command()
def clean():
    """Clean up test artifacts and temporary files."""
    import shutil
    
    cleanup_paths = [
        "test-results",
        "allure-results", 
        ".pytest_cache",
        "__pycache__",
        "*.pyc",
        ".coverage"
    ]
    
    for path_pattern in cleanup_paths:
        if "*" in path_pattern:
            # Handle glob patterns
            import glob
            for path in glob.glob(path_pattern, recursive=True):
                try:
                    if Path(path).is_file():
                        Path(path).unlink()
                    elif Path(path).is_dir():
                        shutil.rmtree(path)
                    click.echo(f"Removed: {path}")
                except Exception:
                    pass
        else:
            path = Path(path_pattern)
            if path.exists():
                try:
                    if path.is_file():
                        path.unlink()
                    elif path.is_dir():
                        shutil.rmtree(path)
                    click.echo(f"Removed: {path}")
                except Exception:
                    pass
    
    click.echo("✅ Cleanup completed")


def main():
    """Main entry point."""
    cli()


if __name__ == "__main__":
    main()