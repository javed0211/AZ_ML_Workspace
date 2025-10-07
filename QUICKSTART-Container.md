# 🚀 Quick Start - Container Testing

## Run Tests in 3 Steps

### 1️⃣ Build the Container
```bash
docker build -t azure-ml-tests:latest .
```

### 2️⃣ Run Your Tests
```bash
# Using the helper script (recommended)
./run-tests-container.sh --category bdd

# Or using Docker directly
docker run --rm -v $(pwd)/TestResults:/workspace/TestResults azure-ml-tests:latest
```

### 3️⃣ View Results
```bash
open TestResults/BDD/index.html
```

---

## 🎯 Common Commands

### Run Specific Test Categories

```bash
# BDD Tests
./run-tests-container.sh --category bdd

# UI Tests
./run-tests-container.sh --category ui

# API Tests
./run-tests-container.sh --category api

# VS Code Desktop Tests (with GUI support)
./run-tests-container.sh --category vscode

# All Tests
./run-tests-container.sh --category all
```

### Using Docker Compose

```bash
# Run all tests in parallel
docker-compose up

# Run specific category
docker-compose up bdd-tests
docker-compose up ui-tests
docker-compose up api-tests
docker-compose up vscode-tests  # VS Code Desktop with GUI

# Run in background
docker-compose up -d

# View logs
docker-compose logs -f bdd-tests

# Stop everything
docker-compose down
```

### With Azure Credentials

1. Create `.env` file:
```bash
cat > .env << EOF
AZURE_SUBSCRIPTION_ID=your-subscription-id
AZURE_TENANT_ID=your-tenant-id
AZURE_RESOURCE_GROUP=your-resource-group
AZURE_WORKSPACE_NAME=your-workspace-name
EOF
```

2. Run tests:
```bash
docker-compose --env-file .env up bdd-tests
```

---

## 🔧 Azure Pipelines Setup

### Option 1: Use Existing Pipeline
Your current `azure-pipelines.yml` already supports Linux containers!

Just push your code:
```bash
git add .
git commit -m "Add container support"
git push origin main
```

### Option 2: Use New Container Pipeline

1. **Rename the new pipeline:**
```bash
# Use the container-optimized pipeline
cp azure-pipelines-container.yml azure-pipelines.yml
```

2. **Update variables in the pipeline:**
```yaml
variables:
  containerRegistry: 'youracr.azurecr.io'  # Your ACR name
  imageRepository: 'azure-ml-test-automation'
```

3. **Add secrets in Azure DevOps:**
   - Go to Pipelines → Library → Variable Groups
   - Create group: `AzureML-Secrets`
   - Add variables:
     - `AZURE_SUBSCRIPTION_ID`
     - `AZURE_TENANT_ID`
     - `AZURE_RESOURCE_GROUP`
     - `AZURE_WORKSPACE_NAME`

4. **Run the pipeline:**
   - Push to `main` or `develop` branch
   - Or trigger manually in Azure DevOps

---

## 📊 Test Results

### Local Testing
Results are saved in `./TestResults/` directory:
```
TestResults/
├── BDD/
│   ├── *.trx
│   └── coverage.cobertura.xml
├── UI/
├── API/
└── Coverage/
```

### Azure Pipelines
- View in **Tests** tab
- Download from **Artifacts**
- Check **Code Coverage** tab

---

## 🐛 Troubleshooting

### Container won't start?
```bash
# Check Docker is running
docker info

# View container logs
docker logs azure-ml-test-runner

# Run interactively for debugging
./run-tests-container.sh --interactive
```

### Tests failing?
```bash
# Run with verbose output
docker run --rm azure-ml-tests:latest \
  dotnet test --logger "console;verbosity=detailed"

# Check test results
cat TestResults/BDD/*.trx
```

### Permission issues?
```bash
# Fix TestResults permissions
sudo chown -R $USER:$USER TestResults/

# Or run as current user
docker run --user $(id -u):$(id -g) azure-ml-tests:latest
```

---

## 🎓 Next Steps

1. **Read the full guide:** [README-Container-Testing.md](README-Container-Testing.md)
2. **Configure Azure credentials:** Create `.env` file
3. **Set up Azure Pipelines:** Follow Azure Pipelines section
4. **Customize tests:** Modify feature files in `NewFramework/CSharpTests/Features/`

---

## 📞 Need Help?

- 📖 Full Documentation: [README-Container-Testing.md](README-Container-Testing.md)
- 🐳 Docker Docs: https://docs.docker.com/
- 🎭 Playwright Docs: https://playwright.dev/
- 🔵 Azure Pipelines: https://docs.microsoft.com/azure/devops/pipelines/

---

## ✅ Checklist

Before running in Azure Pipelines:

- [ ] Docker image builds successfully locally
- [ ] Tests pass in local container
- [ ] `.env` file configured (for local testing)
- [ ] Azure DevOps secrets configured
- [ ] Service connection to Azure created
- [ ] Pipeline YAML updated with your settings
- [ ] `.dockerignore` and `.gitignore` configured

**You're ready to go!** 🎉