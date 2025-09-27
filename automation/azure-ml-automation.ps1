# Azure ML Compute Instance Automation - PowerShell Script
# This script provides Windows-compatible automation for Azure ML compute instances

param(
    [string]$ConfigPath = "config/azure-ml-automation-config.json",
    [switch]$CreateOnly,
    [switch]$VSCodeOnly,
    [switch]$SyncOnly,
    [switch]$Cleanup,
    [switch]$InstallDependencies
)

# Set error action preference
$ErrorActionPreference = "Stop"

# Function to write colored output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Function to check if running as administrator
function Test-Administrator {
    $currentUser = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = New-Object Security.Principal.WindowsPrincipal($currentUser)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

# Function to install required Python packages
function Install-PythonDependencies {
    Write-ColorOutput "Installing required Python packages..." "Yellow"
    
    $packages = @(
        "azure-ai-ml",
        "azure-identity", 
        "paramiko",
        "watchdog",
        "psutil",
        "requests"
    )
    
    foreach ($package in $packages) {
        try {
            Write-ColorOutput "Installing $package..." "Cyan"
            pip install $package --upgrade
        }
        catch {
            Write-ColorOutput "Failed to install $package: $_" "Red"
        }
    }
}

# Function to check VS Code installation
function Test-VSCodeInstallation {
    $vscodePaths = @(
        "${env:LOCALAPPDATA}\Programs\Microsoft VS Code\Code.exe",
        "${env:PROGRAMFILES}\Microsoft VS Code\Code.exe",
        "${env:PROGRAMFILES(X86)}\Microsoft VS Code\Code.exe"
    )
    
    foreach ($path in $vscodePaths) {
        if (Test-Path $path) {
            return $path
        }
    }
    
    # Check if code is in PATH
    try {
        $null = Get-Command "code" -ErrorAction Stop
        return "code"
    }
    catch {
        return $null
    }
}

# Function to install VS Code extensions
function Install-VSCodeExtensions {
    param([string]$VSCodePath)
    
    $extensions = @(
        "ms-python.python",
        "ms-toolsai.jupyter",
        "ms-vscode-remote.remote-ssh",
        "ms-azuretools.vscode-azureml",
        "ms-vscode.powershell"
    )
    
    Write-ColorOutput "Installing VS Code extensions..." "Yellow"
    
    foreach ($extension in $extensions) {
        try {
            Write-ColorOutput "Installing extension: $extension" "Cyan"
            & $VSCodePath --install-extension $extension --force
        }
        catch {
            Write-ColorOutput "Failed to install extension $extension: $_" "Red"
        }
    }
}

# Function to generate SSH key pair
function New-SSHKeyPair {
    $sshDir = "$env:USERPROFILE\.ssh"
    
    if (-not (Test-Path $sshDir)) {
        New-Item -ItemType Directory -Path $sshDir -Force | Out-Null
    }
    
    $privateKeyPath = "$sshDir\id_rsa"
    $publicKeyPath = "$sshDir\id_rsa.pub"
    
    if (-not (Test-Path $privateKeyPath)) {
        Write-ColorOutput "Generating SSH key pair..." "Yellow"
        
        # Use ssh-keygen if available, otherwise use PowerShell method
        try {
            ssh-keygen -t rsa -b 2048 -f $privateKeyPath -N '""' -C "azureml-automation-$(Get-Date -Format 'yyyyMMdd')"
        }
        catch {
            Write-ColorOutput "ssh-keygen not found. Please install OpenSSH or Git for Windows." "Red"
            return $false
        }
    }
    
    return $true
}

# Function to create SSH config
function New-SSHConfig {
    param(
        [string]$Hostname,
        [int]$Port,
        [string]$Username
    )
    
    $sshConfigPath = "$env:USERPROFILE\.ssh\config"
    
    $configEntry = @"

# Azure ML Compute Instance
Host azureml-compute
    HostName $Hostname
    Port $Port
    User $Username
    IdentityFile ~/.ssh/id_rsa
    StrictHostKeyChecking no
    UserKnownHostsFile /dev/null
    ServerAliveInterval 60
    ServerAliveCountMax 3
"@
    
    # Read existing config
    $existingConfig = ""
    if (Test-Path $sshConfigPath) {
        $existingConfig = Get-Content $sshConfigPath -Raw
    }
    
    # Remove existing azureml-compute entry
    $lines = $existingConfig -split "`n"
    $filteredLines = @()
    $skipSection = $false
    
    foreach ($line in $lines) {
        if ($line.Trim() -match "^Host azureml-compute") {
            $skipSection = $true
            continue
        }
        elseif ($line.Trim() -match "^Host " -and $skipSection) {
            $skipSection = $false
        }
        
        if (-not $skipSection) {
            $filteredLines += $line
        }
    }
    
    # Add new config entry
    $newConfig = ($filteredLines -join "`n") + $configEntry
    
    Set-Content -Path $sshConfigPath -Value $newConfig -Encoding UTF8
    
    Write-ColorOutput "SSH config updated: $sshConfigPath" "Green"
}

# Function to launch VS Code with remote SSH
function Start-VSCodeRemote {
    param(
        [string]$VSCodePath,
        [string]$RemotePath
    )
    
    try {
        Write-ColorOutput "Launching VS Code with remote SSH connection..." "Yellow"
        
        $arguments = @(
            "--new-window",
            "--remote", "ssh-remote+azureml-compute",
            $RemotePath
        )
        
        Start-Process -FilePath $VSCodePath -ArgumentList $arguments -NoNewWindow
        
        Start-Sleep -Seconds 5
        Write-ColorOutput "VS Code launched successfully with remote connection" "Green"
        return $true
    }
    catch {
        Write-ColorOutput "Failed to launch VS Code remote: $_" "Red"
        return $false
    }
}

# Function to start file synchronization
function Start-FileSync {
    param([string]$ConfigPath)
    
    Write-ColorOutput "Starting file synchronization..." "Yellow"
    
    try {
        python -c "
import sys
sys.path.append('.')
from azure_ml_compute_automation import AzureMLComputeAutomation
automation = AzureMLComputeAutomation('$ConfigPath')
automation.authenticate()
automation.setup_ssh_connection()
# Start file sync here
print('File synchronization started')
"
    }
    catch {
        Write-ColorOutput "Failed to start file synchronization: $_" "Red"
    }
}

# Main execution
function Main {
    Write-ColorOutput "Azure ML Compute Instance Automation - PowerShell Edition" "Cyan"
    Write-ColorOutput "============================================================" "Cyan"
    
    # Install dependencies if requested
    if ($InstallDependencies) {
        Install-PythonDependencies
        return
    }
    
    # Check VS Code installation
    $vscodePath = Test-VSCodeInstallation
    if (-not $vscodePath) {
        Write-ColorOutput "VS Code not found. Please install VS Code first." "Red"
        Write-ColorOutput "Download from: https://code.visualstudio.com/" "Yellow"
        return
    }
    
    Write-ColorOutput "VS Code found at: $vscodePath" "Green"
    
    # Install VS Code extensions
    Install-VSCodeExtensions -VSCodePath $vscodePath
    
    # Generate SSH key if needed
    if (-not (New-SSHKeyPair)) {
        Write-ColorOutput "Failed to generate SSH key pair" "Red"
        return
    }
    
    # Check if Python script exists
    $pythonScript = "azure-ml-compute-automation.py"
    if (-not (Test-Path $pythonScript)) {
        Write-ColorOutput "Python automation script not found: $pythonScript" "Red"
        return
    }
    
    # Build Python command arguments
    $pythonArgs = @("$pythonScript", "--config", $ConfigPath)
    
    if ($CreateOnly) { $pythonArgs += "--create-only" }
    if ($VSCodeOnly) { $pythonArgs += "--vscode-only" }
    if ($SyncOnly) { $pythonArgs += "--sync-only" }
    if ($Cleanup) { $pythonArgs += "--cleanup" }
    
    try {
        Write-ColorOutput "Running Python automation script..." "Yellow"
        Write-ColorOutput "Command: python $($pythonArgs -join ' ')" "Cyan"
        
        # Run the Python script
        $process = Start-Process -FilePath "python" -ArgumentList $pythonArgs -NoNewWindow -PassThru -Wait
        
        if ($process.ExitCode -eq 0) {
            Write-ColorOutput "Automation completed successfully!" "Green"
            
            if (-not $CreateOnly -and -not $Cleanup) {
                Write-ColorOutput "VS Code should now be connected to your Azure ML compute instance." "Green"
                Write-ColorOutput "You can also manually connect using: ssh azureml-compute" "Yellow"
            }
        }
        else {
            Write-ColorOutput "Automation failed with exit code: $($process.ExitCode)" "Red"
        }
    }
    catch {
        Write-ColorOutput "Failed to run automation: $_" "Red"
    }
}

# Run main function
try {
    Main
}
catch {
    Write-ColorOutput "Script execution failed: $_" "Red"
    exit 1
}

Write-ColorOutput "Script execution completed." "Cyan"