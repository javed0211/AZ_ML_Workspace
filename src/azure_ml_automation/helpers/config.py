"""Configuration management for Azure ML automation framework."""

import os
from pathlib import Path
from typing import Optional, List
from pydantic import Field, validator
from pydantic_settings import BaseSettings, SettingsConfigDict
from dotenv import load_dotenv

# Load environment variables from .env file
load_dotenv()


class AzureConfig(BaseSettings):
    """Azure-specific configuration."""
    
    tenant_id: Optional[str] = Field(None, alias="AZURE_TENANT_ID")
    client_id: Optional[str] = Field(None, alias="AZURE_CLIENT_ID")
    client_secret: Optional[str] = Field(None, alias="AZURE_CLIENT_SECRET")
    subscription_id: Optional[str] = Field(None, alias="AZURE_SUBSCRIPTION_ID")
    resource_group: Optional[str] = Field(None, alias="AZURE_RESOURCE_GROUP")
    workspace_name: Optional[str] = Field(None, alias="AZURE_ML_WORKSPACE_NAME")
    key_vault_url: Optional[str] = Field(None, alias="AZURE_KEY_VAULT_URL")
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )


class AuthConfig(BaseSettings):
    """Authentication configuration."""
    
    use_interactive: bool = Field(False, alias="USE_INTERACTIVE_AUTH")
    use_managed_identity: bool = Field(True, alias="USE_MANAGED_IDENTITY")  # Default to Managed Identity
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )


class UrlConfig(BaseSettings):
    """URL configuration for different platforms."""
    
    base: str = Field("https://ml.azure.com", alias="BASE_URL")
    vscode_web: Optional[str] = Field(None, alias="VSCODE_WEB_URL")
    jupyter: Optional[str] = Field(None, alias="JUPYTER_URL")
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )


class TimeoutConfig(BaseSettings):
    """Timeout configuration."""
    
    default: int = Field(30000, alias="DEFAULT_TIMEOUT")  # milliseconds
    navigation: int = Field(60000, alias="NAVIGATION_TIMEOUT")
    test: int = Field(300000, alias="TEST_TIMEOUT")
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )


class RetryConfig(BaseSettings):
    """Retry configuration."""
    
    max_retries: int = Field(3, alias="MAX_RETRIES")
    delay: int = Field(1000, alias="RETRY_DELAY")  # milliseconds
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )


class ParallelConfig(BaseSettings):
    """Parallel execution configuration."""
    
    max_workers: int = Field(4, alias="MAX_WORKERS")
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )


class ArtifactsConfig(BaseSettings):
    """Artifacts configuration."""
    
    path: str = Field("./test-results", alias="ARTIFACTS_PATH")
    upload: bool = Field(False, alias="UPLOAD_ARTIFACTS")
    storage_account: Optional[str] = Field(None, alias="AZURE_STORAGE_ACCOUNT")
    container: str = Field("test-artifacts", alias="AZURE_STORAGE_CONTAINER")
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )


class LoggingConfig(BaseSettings):
    """Logging configuration."""
    
    level: str = Field("info", alias="LOG_LEVEL")
    format: str = Field("json", alias="LOG_FORMAT")
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )
    
    @validator("level")
    def validate_level(cls, v):
        valid_levels = ["error", "warn", "info", "debug"]
        if v.lower() not in valid_levels:
            raise ValueError(f"Log level must be one of: {valid_levels}")
        return v.lower()
    
    @validator("format")
    def validate_format(cls, v):
        valid_formats = ["json", "simple"]
        if v.lower() not in valid_formats:
            raise ValueError(f"Log format must be one of: {valid_formats}")
        return v.lower()


class ElectronConfig(BaseSettings):
    """Electron/VS Code configuration."""
    
    vscode_path: Optional[str] = Field(None, alias="VSCODE_PATH")
    user_data_dir: Optional[str] = Field(None, alias="ELECTRON_USER_DATA_DIR")
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )
    
    @validator("user_data_dir", pre=True, always=True)
    def set_default_user_data_dir(cls, v):
        if v is None:
            return str(Path.cwd() / ".vscode-test")
        return v


class ComputeConfig(BaseSettings):
    """Compute configuration."""
    
    instance_name: Optional[str] = Field(None, alias="COMPUTE_INSTANCE_NAME")
    cluster_name: Optional[str] = Field(None, alias="COMPUTE_CLUSTER_NAME")
    size: str = Field("Standard_DS3_v2", alias="COMPUTE_SIZE")
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )


class PIMConfig(BaseSettings):
    """Privileged Identity Management configuration."""
    
    role_name: Optional[str] = Field(None, alias="PIM_ROLE_NAME")
    scope: Optional[str] = Field(None, alias="PIM_SCOPE")
    justification: str = Field("Automated testing", alias="PIM_JUSTIFICATION")
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )


class NotificationsConfig(BaseSettings):
    """Notifications configuration."""
    
    teams_webhook: Optional[str] = Field(None, alias="TEAMS_WEBHOOK_URL")
    slack_webhook: Optional[str] = Field(None, alias="SLACK_WEBHOOK_URL")
    email: bool = Field(False, alias="EMAIL_NOTIFICATIONS")
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )


class Config(BaseSettings):
    """Main configuration class."""
    
    # Environment
    env: str = Field("development", alias="NODE_ENV")
    
    # Sub-configurations
    azure: AzureConfig = Field(default_factory=AzureConfig)
    auth: AuthConfig = Field(default_factory=AuthConfig)
    urls: UrlConfig = Field(default_factory=UrlConfig)
    timeouts: TimeoutConfig = Field(default_factory=TimeoutConfig)
    retry: RetryConfig = Field(default_factory=RetryConfig)
    parallel: ParallelConfig = Field(default_factory=ParallelConfig)
    artifacts: ArtifactsConfig = Field(default_factory=ArtifactsConfig)
    logging: LoggingConfig = Field(default_factory=LoggingConfig)
    electron: ElectronConfig = Field(default_factory=ElectronConfig)
    compute: ComputeConfig = Field(default_factory=ComputeConfig)
    pim: PIMConfig = Field(default_factory=PIMConfig)
    notifications: NotificationsConfig = Field(default_factory=NotificationsConfig)
    
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        case_sensitive=True,
        extra="ignore"
    )
        
    @validator("env")
    def validate_env(cls, v):
        valid_envs = ["development", "test", "production"]
        if v.lower() not in valid_envs:
            raise ValueError(f"Environment must be one of: {valid_envs}")
        return v.lower()
    
    def __init__(self, **kwargs):
        super().__init__(**kwargs)
        # Sub-configurations are already initialized by Pydantic using default_factory
        # No need to re-initialize them here as it would overwrite loaded values


# Global configuration instance
config = Config()

# Environment helpers
is_development = config.env == "development"
is_test = config.env == "test"
is_production = config.env == "production"
is_ci = bool(os.getenv("CI"))


def validate_required_config(keys: List[str]) -> None:
    """Validate that required configuration keys are present."""
    missing = []
    
    for key in keys:
        parts = key.split(".")
        value = config
        
        try:
            for part in parts:
                value = getattr(value, part)
            
            if value is None or (isinstance(value, str) and not value.strip()):
                missing.append(key)
        except AttributeError:
            missing.append(key)
    
    if missing:
        raise ValueError(f"Missing required configuration: {', '.join(missing)}")


# Export individual config sections for convenience
azure_config = config.azure
auth_config = config.auth
url_config = config.urls
timeout_config = config.timeouts