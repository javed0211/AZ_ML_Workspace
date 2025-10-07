# Azure ML Test Automation - Container-Based Testing Guide

## Overview

This guide explains how to run the Azure ML BDD Test Automation Framework in Linux containers, both locally and in Azure Pipelines.

## 🐳 Container Support

The framework is fully containerized and supports:

- ✅ **Linux Containers** (Ubuntu-based)
- ✅ **Docker & Docker Compose**
- ✅ **Azure Pipelines Container Jobs**
- ✅ **Azure Container Registry (ACR)**
- ✅ **VS Code Desktop Testing** (with Xvfb GUI support)
- ✅ **Kubernetes (future support)**

---

## 📋 Prerequisites

### Local Development
- Docker Desktop or Docker Engine (20.10+)
- Docker Compose (v2.0+)
- 8GB RAM minimum (16GB recommended)
- 20GB free disk space

### Azure Pipelines
- Azure DevOps organization
- Azure Container Registry (optional, for image caching)
- Service connection to Azure (for Azure ML tests)

---

## 🚀 Quick Start

### 1. Build Docker Image

```bash
# Build the test image
docker build -t azure-ml-tests:latest .

# Build specific stage
docker build --target test -t azure-ml-tests:test .
```

### 2. Run Tests Locally

#### Option A: Using Docker Directly

```bash
# Run all tests
docker run --rm \
  -v $(pwd)/TestResults:/workspace/TestResults \
  -e HEADLESS_MODE=true \
  -e BROWSER_TYPE=chromium \
  azure-ml-tests:latest

# Run BDD tests only
docker run --rm \
  -v $(pwd)/TestResults:/workspace/TestResults \
  -e HEADLESS_MODE=true \
  azure-ml-tests:latest \
  dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj \
  --filter "Category=BDD" \
  --results-directory /workspace/TestResults/BDD
```

#### Option B: Using Docker Compose (Recommended)

```bash
# Run all test services
docker-compose up

# Run specific test category
docker-compose up bdd-tests
docker-compose up ui-tests
docker-compose up api-tests
docker-compose up vscode-tests  # VS Code Desktop tests with GUI support

# Run in detached mode
docker-compose up -d

# View logs
docker-compose logs -f bdd-tests

# Stop all services
docker-compose down
```

### 3. Run with Azure Credentials

Create a `.env` file in the project root:

```env
# Azure Credentials
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_WORKSPACE_NAME=your-workspace-name

# Azure AI Search
AZUREAISEARCH_SERVICENAME=your-search-service
AZUREAISEARCH_INDEXNAME=your-index-name

# Test Settings
BROWSER_TYPE=chromium
HEADLESS_MODE=true
WORKERS=2
TIMEOUT=60000
```

Then run:

```bash
docker-compose --env-file .env up
```

---

## 🏗️ Docker Image Architecture

### Multi-Stage Build

The Dockerfile uses multi-stage builds for optimization:

1. **base** - Base image with .NET, Node.js, Python, Azure CLI
2. **playwright-deps** - Playwright browser dependencies
3. **build** - Build and compile the test framework
4. **test** - Test execution environment (default)
5. **runtime** - Minimal production image (optional)

### Image Sizes

| Stage | Size | Purpose |
|-------|------|---------|
| base | ~2.5GB | Development base |
| test | ~4.5GB | Full test environment |
| runtime | ~500MB | Production minimal |

---

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `HEADLESS_MODE` | Run browsers in headless mode | `true` |
| `BROWSER_TYPE` | Browser to use (chromium/firefox/webkit) | `chromium` |
| `WORKERS` | Number of parallel workers | `2` |
| `TIMEOUT` | Test timeout in milliseconds | `60000` |
| `DISPLAY` | X11 display for GUI apps | `:99` |
| `PLAYWRIGHT_BROWSERS_PATH` | Playwright browser cache | `/ms-playwright` |

### Volume Mounts

```yaml
volumes:
  # Test results
  - ./TestResults:/workspace/TestResults
  
  # SSH keys for Azure ML compute
  - ~/.ssh:/root/.ssh:ro
  
  # Source code (for development)
  - ./NewFramework:/workspace/NewFramework
```

---

## 🎯 Azure Pipelines Integration

### Option 1: Use Existing Pipeline (azure-pipelines.yml)

The existing pipeline already supports Linux containers via `ubuntu-latest` agents.

### Option 2: Use Container-Based Pipeline (azure-pipelines-container.yml)

This new pipeline provides:
- Docker image building and caching
- Container-based test execution
- Docker Compose parallel testing
- Azure Container Registry integration

#### Setup Steps:

1. **Create Azure Container Registry** (optional):
```bash
az acr create --resource-group myResourceGroup \
  --name myacrname --sku Basic
```

2. **Create Service Connection** in Azure DevOps:
   - Go to Project Settings → Service Connections
   - Create "Docker Registry" connection
   - Connect to your ACR

3. **Update Pipeline Variables**:
```yaml
variables:
  dockerRegistryServiceConnection: 'YourServiceConnectionName'
  containerRegistry: 'youracr.azurecr.io'
  imageRepository: 'azure-ml-test-automation'
```

4. **Add Pipeline Secrets**:
   - `AZURE_SUBSCRIPTION_ID`
   - `AZURE_TENANT_ID`
   - `AZURE_RESOURCE_GROUP`
   - `AZURE_WORKSPACE_NAME`
   - `AZUREAISEARCH_SERVICENAME`
   - `AZUREAISEARCH_INDEXNAME`

5. **Run Pipeline**:
```bash
# Trigger manually
az pipelines run --name azure-pipelines-container.yml

# Or commit and push to trigger automatically
git add azure-pipelines-container.yml
git commit -m "Add container-based pipeline"
git push origin main
```

---

## 📊 Test Results

### Accessing Results

#### Local Docker:
```bash
# Results are in ./TestResults directory
ls -la TestResults/

# View test report
open TestResults/BDD/index.html
```

#### Docker Compose:
```bash
# Copy results from container
docker cp azure-ml-bdd-tests:/workspace/TestResults ./TestResults

# Or use mounted volumes (automatic)
ls -la TestResults/BDD/
```

#### Azure Pipelines:
- View in "Tests" tab
- Download artifacts from "Artifacts" section
- Check code coverage in "Code Coverage" tab

---

## 🐛 Troubleshooting

### Issue: Playwright browsers not found

**Solution:**
```bash
# Rebuild with browser installation
docker build --no-cache --target test -t azure-ml-tests:latest .
```

### Issue: Permission denied on test results

**Solution:**
```bash
# Fix permissions
sudo chown -R $USER:$USER TestResults/

# Or run container with user
docker run --user $(id -u):$(id -g) azure-ml-tests:latest
```

### Issue: Out of memory

**Solution:**
```bash
# Increase Docker memory limit
# Docker Desktop → Settings → Resources → Memory: 8GB

# Or limit test parallelism
docker run -e WORKERS=1 azure-ml-tests:latest
```

### Issue: Container exits immediately

**Solution:**
```bash
# Check logs
docker logs azure-ml-test-runner

# Run interactively
docker run -it azure-ml-tests:latest /bin/bash

# Debug build
docker build --progress=plain --no-cache .
```

---

## 🔒 Security Best Practices

### 1. Don't Commit Secrets
```bash
# Add to .gitignore
echo ".env" >> .gitignore
echo "*.pfx" >> .gitignore
echo "*.key" >> .gitignore
```

### 2. Use Azure Key Vault
```yaml
# In Azure Pipelines
- task: AzureKeyVault@2
  inputs:
    azureSubscription: 'YourSubscription'
    KeyVaultName: 'YourKeyVault'
    SecretsFilter: '*'
```

### 3. Scan Images for Vulnerabilities
```bash
# Using Trivy
docker run --rm -v /var/run/docker.sock:/var/run/docker.sock \
  aquasec/trivy:latest image azure-ml-tests:latest

# Using Snyk
snyk container test azure-ml-tests:latest
```

### 4. Use Non-Root User
```dockerfile
# Add to Dockerfile
RUN useradd -m -u 1000 testuser
USER testuser
```

---

## 📈 Performance Optimization

### 1. Layer Caching
```bash
# Use BuildKit for better caching
DOCKER_BUILDKIT=1 docker build -t azure-ml-tests:latest .
```

### 2. Multi-Stage Builds
```bash
# Build only what you need
docker build --target runtime -t azure-ml-tests:runtime .
```

### 3. Parallel Test Execution
```yaml
# In docker-compose.yml
environment:
  - WORKERS=4  # Increase parallel workers
```

### 4. Resource Limits
```yaml
deploy:
  resources:
    limits:
      cpus: '4'
      memory: 8G
```

---

## 🔄 CI/CD Workflow

### Complete Pipeline Flow:

```
1. Code Push → GitHub/Azure Repos
         ↓
2. Trigger Pipeline (azure-pipelines-container.yml)
         ↓
3. Build Docker Image
         ↓
4. Push to Azure Container Registry
         ↓
5. Run Tests in Containers (Parallel)
   ├── BDD Tests
   ├── UI Tests
   └── API Tests
         ↓
6. Collect Test Results
         ↓
7. Generate Coverage Reports
         ↓
8. Publish Artifacts
         ↓
9. Notify Team (Teams/Slack)
```

---

## 📚 Additional Resources

### Documentation
- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Azure Pipelines Container Jobs](https://docs.microsoft.com/azure/devops/pipelines/process/container-phases)
- [Playwright in Docker](https://playwright.dev/docs/docker)

### Example Commands

```bash
# Build and test in one command
docker build -t azure-ml-tests:latest . && \
docker run --rm -v $(pwd)/TestResults:/workspace/TestResults azure-ml-tests:latest

# Run specific test scenario
docker run --rm azure-ml-tests:latest \
  dotnet test --filter "Name~'Validate Prerequisites'"

# Interactive debugging
docker run -it --entrypoint /bin/bash azure-ml-tests:latest

# Clean up everything
docker-compose down -v
docker system prune -af
```

---

## 🎓 Best Practices Summary

✅ **DO:**
- Use multi-stage builds for smaller images
- Mount volumes for test results
- Use environment variables for configuration
- Run containers in headless mode
- Implement proper error handling
- Use Docker Compose for local testing
- Cache dependencies in CI/CD
- Scan images for vulnerabilities

❌ **DON'T:**
- Commit secrets to version control
- Run containers as root in production
- Use `latest` tag in production
- Store large files in images
- Ignore security warnings
- Skip image scanning

---

## 🆘 Support

For issues or questions:
1. Check the [Troubleshooting](#-troubleshooting) section
2. Review Docker logs: `docker logs <container-name>`
3. Check Azure Pipeline logs in Azure DevOps
4. Open an issue in the repository

---

## 📝 License

This framework is part of the Azure ML Test Automation project.