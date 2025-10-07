# VS Code Desktop Testing in Linux Containers

## Overview

This guide explains how to run **VS Code Desktop tests** in Linux containers with full GUI support using **Xvfb (X Virtual Framebuffer)**.

## 🎯 Key Features

✅ **Full GUI Support** - VS Code Desktop runs in a virtual display  
✅ **Cross-Platform** - Works on Linux, Windows, and macOS hosts  
✅ **Headless Testing** - No physical display required  
✅ **Azure Pipelines Ready** - Runs on Linux agents  
✅ **Interactive Debugging** - VNC support for troubleshooting  

---

## 🏗️ Architecture

### How It Works

```
┌─────────────────────────────────────────────────────────┐
│                    Docker Container                      │
│                                                          │
│  ┌────────────────────────────────────────────────┐    │
│  │  Xvfb (Virtual Display :99)                    │    │
│  │  Resolution: 1920x1080x24                      │    │
│  └────────────────────────────────────────────────┘    │
│                         │                               │
│                         ▼                               │
│  ┌────────────────────────────────────────────────┐    │
│  │  VS Code Desktop                               │    │
│  │  - Installed via Microsoft repository          │    │
│  │  - Runs in virtual display                     │    │
│  │  - Controlled by Playwright                    │    │
│  └────────────────────────────────────────────────┘    │
│                         │                               │
│                         ▼                               │
│  ┌────────────────────────────────────────────────┐    │
│  │  Test Framework (Playwright + SpecFlow)        │    │
│  │  - Clicks VS Code links in Azure ML portal     │    │
│  │  - Verifies VS Code launches                   │    │
│  │  - Tests remote SSH connections                │    │
│  └────────────────────────────────────────────────┘    │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

### Components

1. **Xvfb** - Virtual X11 display server (no physical display needed)
2. **VS Code** - Full VS Code Desktop installation
3. **GUI Dependencies** - GTK, DBus, libsecret, etc.
4. **Test Framework** - Playwright for browser automation + VS Code interaction

---

## 🚀 Quick Start

### 1. Build the Container Image

```bash
# Build with VS Code and Xvfb support
docker build -t azure-ml-tests:latest .

# Verify VS Code is installed
docker run --rm azure-ml-tests:latest code --version
```

### 2. Run VS Code Tests

#### Option A: Using Helper Script (Recommended)

```bash
# Run VS Code tests
./run-tests-container.sh --category vscode

# Build and run
./run-tests-container.sh --build --category vscode

# Interactive debugging
./run-tests-container.sh --category vscode --interactive
```

#### Option B: Using Docker Compose

```bash
# Run VS Code tests
docker-compose up vscode-tests

# View logs
docker-compose logs -f vscode-tests

# Stop
docker-compose down
```

#### Option C: Using Docker Directly

```bash
# Run VS Code tests
docker run --rm \
  -v $(pwd)/TestResults:/workspace/TestResults \
  -e DISPLAY=:99 \
  -e VSCODE_TESTING=true \
  --privileged \
  azure-ml-tests:latest \
  /usr/local/bin/start-xvfb.sh \
  dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj \
  --filter "Category=VSCode" \
  --results-directory /workspace/TestResults/VSCode
```

---

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `DISPLAY` | X11 display number | `:99` |
| `DISPLAY_WIDTH` | Virtual display width | `1920` |
| `DISPLAY_HEIGHT` | Virtual display height | `1080` |
| `DISPLAY_DEPTH` | Color depth | `24` |
| `VSCODE_TESTING` | Enable VS Code testing mode | `false` |

### Docker Compose Configuration

```yaml
vscode-tests:
  environment:
    - DISPLAY=:99
    - DISPLAY_WIDTH=1920
    - DISPLAY_HEIGHT=1080
    - VSCODE_TESTING=true
  privileged: true  # Required for Xvfb
```

---

## 🧪 Test Scenarios

The framework tests the following VS Code Desktop scenarios:

### 1. **Prerequisites Validation**
```gherkin
Scenario: Validate Prerequisites for Automation
  Then VS Code should be installed and accessible
```

### 2. **VS Code Remote Connection**
```gherkin
@VSCode
Scenario: Setup VS Code Remote Connection
  Given I have SSH connection configured for "test-automation-instance"
  When I setup VS Code remote connection
  Then the required VS Code extensions should be installed
  And VS Code should be configured for remote SSH access
  And I should be able to open remote workspace
```

### 3. **VS Code Desktop Integration**
```gherkin
Scenario: Azure ML Workspace with VS Code Desktop Integration
  When I start VS code Desktop
  Then I check if I am able to interact with VS code
```

### 4. **Performance Testing**
```gherkin
@Performance
Scenario: Automation Performance Requirements
  Then the VS Code setup should complete within 2 minutes
```

---

## 🐛 Troubleshooting

### Issue: VS Code Fails to Launch

**Symptoms:**
```
Error: Could not find VS Code Desktop link
```

**Solutions:**

1. **Verify VS Code is installed:**
   ```bash
   docker run --rm azure-ml-tests:latest code --version
   ```

2. **Check Xvfb is running:**
   ```bash
   docker run --rm azure-ml-tests:latest ps aux | grep Xvfb
   ```

3. **Test display:**
   ```bash
   docker run --rm -e DISPLAY=:99 azure-ml-tests:latest \
     /usr/local/bin/start-xvfb.sh xdpyinfo
   ```

### Issue: Display Not Available

**Symptoms:**
```
Error: cannot open display :99
```

**Solutions:**

1. **Ensure Xvfb starts before tests:**
   ```bash
   # Use the start-xvfb.sh wrapper
   /usr/local/bin/start-xvfb.sh <your-command>
   ```

2. **Check DISPLAY variable:**
   ```bash
   docker run --rm azure-ml-tests:latest env | grep DISPLAY
   ```

3. **Run with privileged mode:**
   ```bash
   docker run --rm --privileged azure-ml-tests:latest
   ```

### Issue: VS Code Protocol Handler Not Working

**Symptoms:**
```
vscode:// links don't launch VS Code
```

**Solutions:**

1. **This is expected in containers** - The `vscode://` protocol is handled by the host OS, not the container

2. **For testing, we simulate the behavior:**
   - Click the link in the browser (Playwright)
   - Verify the link exists and is enabled
   - Launch VS Code directly in the container

3. **Update test expectations:**
   ```csharp
   // Instead of waiting for VS Code to launch via protocol
   // Launch VS Code directly
   await _utils.ExecuteCommandAsync("code --remote ssh://compute-instance");
   ```

### Issue: Permission Denied

**Symptoms:**
```
Error: Permission denied accessing /tmp/.X11-unix
```

**Solutions:**

1. **Run with privileged mode:**
   ```bash
   docker run --rm --privileged azure-ml-tests:latest
   ```

2. **Or mount X11 socket:**
   ```bash
   docker run --rm \
     -v /tmp/.X11-unix:/tmp/.X11-unix \
     -e DISPLAY=$DISPLAY \
     azure-ml-tests:latest
   ```

---

## 🔍 Interactive Debugging

### Access Container Shell

```bash
# Start interactive session
./run-tests-container.sh --interactive

# Or with Docker directly
docker run --rm -it azure-ml-tests:latest /bin/bash
```

### Test VS Code Manually

```bash
# Inside container
/usr/local/bin/start-xvfb.sh code --version
/usr/local/bin/start-xvfb.sh code --list-extensions
```

### Enable VNC for Visual Debugging

```bash
# Start container with VNC
docker run --rm -it \
  -p 5900:5900 \
  -e DISPLAY=:99 \
  azure-ml-tests:latest \
  bash -c "
    /usr/local/bin/start-xvfb.sh &
    x11vnc -display :99 -forever -nopw &
    /bin/bash
  "

# Connect with VNC client to localhost:5900
```

---

## 📊 Performance Considerations

### Container Size

| Stage | Size | Purpose |
|-------|------|---------|
| Base | ~2.5GB | .NET SDK + Node.js + Python + Azure CLI |
| + Xvfb | ~2.8GB | GUI dependencies |
| + VS Code | ~3.2GB | VS Code Desktop |
| + Playwright | ~4.5GB | Browser binaries |
| Test Stage | **~4.5GB** | Full test environment |
| Runtime | ~500MB | Minimal runtime (no VS Code) |

### Resource Requirements

| Resource | Minimum | Recommended |
|----------|---------|-------------|
| CPU | 2 cores | 4 cores |
| RAM | 4GB | 8GB |
| Disk | 10GB | 20GB |

### Test Execution Time

| Test Category | Duration | Notes |
|---------------|----------|-------|
| VS Code Prerequisites | ~5s | Quick validation |
| VS Code Launch | ~30s | First launch is slower |
| Remote Connection | ~2min | Includes SSH setup |
| Full Integration | ~5min | Complete workflow |

---

## 🔐 Security Considerations

### 1. **Privileged Mode**

VS Code tests require `--privileged` for Xvfb:

```yaml
# docker-compose.yml
vscode-tests:
  privileged: true  # Required for virtual display
```

**Risk:** Privileged containers have elevated permissions  
**Mitigation:** Only use for testing, not production

### 2. **SSH Keys**

VS Code remote connections require SSH keys:

```yaml
volumes:
  - ~/.ssh:/root/.ssh:ro  # Read-only mount
```

**Risk:** SSH keys exposed to container  
**Mitigation:** Mount as read-only, use test keys only

### 3. **Azure Credentials**

Use environment variables, not hardcoded:

```bash
# .env file (never commit!)
AZURE_SUBSCRIPTION_ID=xxx
AZURE_TENANT_ID=xxx
```

---

## 🌐 Cross-Platform Support

### Linux Host

✅ **Fully Supported** - Native Docker support

```bash
docker build -t azure-ml-tests:latest .
docker run --rm azure-ml-tests:latest
```

### Windows Host

✅ **Fully Supported** - Docker Desktop with WSL2

```powershell
# PowerShell
docker build -t azure-ml-tests:latest .
docker run --rm azure-ml-tests:latest
```

**Note:** Windows containers are NOT supported. Use Linux containers on Windows via Docker Desktop.

### macOS Host

✅ **Fully Supported** - Docker Desktop

```bash
docker build -t azure-ml-tests:latest .
docker run --rm azure-ml-tests:latest
```

---

## 🚀 Azure Pipelines Integration

### Pipeline Configuration

```yaml
# azure-pipelines-container.yml
stages:
  - stage: VSCodeTests
    displayName: 'VS Code Desktop Tests'
    jobs:
      - job: VSCodeTestsJob
        displayName: 'Run VS Code Tests in Container'
        pool:
          vmImage: 'ubuntu-latest'
        
        steps:
          - task: Docker@2
            displayName: 'Build Test Image'
            inputs:
              command: 'build'
              Dockerfile: 'Dockerfile'
              tags: 'azure-ml-tests:$(Build.BuildId)'
          
          - script: |
              docker run --rm \
                --privileged \
                -v $(Build.ArtifactStagingDirectory):/workspace/TestResults \
                -e DISPLAY=:99 \
                -e VSCODE_TESTING=true \
                -e AZURE_SUBSCRIPTION_ID=$(AZURE_SUBSCRIPTION_ID) \
                -e AZURE_TENANT_ID=$(AZURE_TENANT_ID) \
                azure-ml-tests:$(Build.BuildId) \
                /usr/local/bin/start-xvfb.sh \
                dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj \
                --filter "Category=VSCode" \
                --logger trx \
                --results-directory /workspace/TestResults/VSCode
            displayName: 'Run VS Code Tests'
          
          - task: PublishTestResults@2
            displayName: 'Publish Test Results'
            inputs:
              testResultsFormat: 'VSTest'
              testResultsFiles: '**/TestResults/VSCode/*.trx'
```

---

## 📚 Additional Resources

- [Xvfb Documentation](https://www.x.org/releases/X11R7.6/doc/man/man1/Xvfb.1.xhtml)
- [VS Code Remote Development](https://code.visualstudio.com/docs/remote/remote-overview)
- [Docker Privileged Mode](https://docs.docker.com/engine/reference/run/#runtime-privilege-and-linux-capabilities)
- [Playwright Documentation](https://playwright.dev/)

---

## 🎓 Best Practices

1. **Use Helper Scripts** - Simplifies complex Docker commands
2. **Tag Tests Properly** - Use `@VSCode` tag for VS Code-specific tests
3. **Monitor Resources** - VS Code + browsers consume significant memory
4. **Cache Images** - Use Azure Container Registry for faster builds
5. **Separate Concerns** - Run VS Code tests separately from other tests
6. **Test Locally First** - Verify tests work locally before CI/CD
7. **Use VNC for Debugging** - Visual debugging helps troubleshoot issues
8. **Keep Images Updated** - Regularly update base images and VS Code

---

## ✅ Checklist

Before running VS Code tests in containers:

- [ ] Docker installed and running
- [ ] Dockerfile includes Xvfb and VS Code
- [ ] Tests tagged with `@VSCode` category
- [ ] Azure credentials configured
- [ ] SSH keys available (if testing remote connections)
- [ ] Sufficient resources (4GB RAM, 2 CPU cores)
- [ ] Privileged mode enabled (for Xvfb)
- [ ] Test results directory mounted
- [ ] Environment variables set

---

## 🆘 Getting Help

If you encounter issues:

1. **Check logs:**
   ```bash
   docker-compose logs vscode-tests
   ```

2. **Run interactively:**
   ```bash
   ./run-tests-container.sh --interactive --category vscode
   ```

3. **Enable verbose logging:**
   ```bash
   docker run --rm -e DEBUG=true azure-ml-tests:latest
   ```

4. **Test components individually:**
   ```bash
   # Test Xvfb
   docker run --rm azure-ml-tests:latest /usr/local/bin/start-xvfb.sh xdpyinfo
   
   # Test VS Code
   docker run --rm azure-ml-tests:latest code --version
   
   # Test display
   docker run --rm azure-ml-tests:latest env | grep DISPLAY
   ```

---

**Happy Testing!** 🚀