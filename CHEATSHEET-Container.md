# 🚀 Container Testing Cheat Sheet

## Quick Commands

### 🐳 Docker

```bash
# Build image
docker build -t azure-ml-tests:latest .

# Run all tests
docker run --rm -v $(pwd)/TestResults:/workspace/TestResults azure-ml-tests:latest

# Run BDD tests only
docker run --rm azure-ml-tests:latest \
  dotnet test --filter "Category=BDD"

# Interactive shell
docker run -it --entrypoint /bin/bash azure-ml-tests:latest

# View logs
docker logs azure-ml-test-runner

# Stop container
docker stop azure-ml-test-runner

# Clean up
docker system prune -af
```

### 🎭 Helper Script

```bash
# Build and run BDD tests
./run-tests-container.sh --build --category bdd

# Run UI tests with Firefox
./run-tests-container.sh --category ui --browser firefox

# Interactive debugging
./run-tests-container.sh --interactive

# Run with cleanup
./run-tests-container.sh --category all --cleanup

# Using Docker Compose
./run-tests-container.sh --compose --category bdd
```

### 🎼 Docker Compose

```bash
# Run all tests
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

# Stop all
docker-compose down

# Rebuild and run
docker-compose up --build
```

### ☁️ Azure Pipelines

```bash
# Trigger pipeline
az pipelines run --name azure-pipelines-container.yml

# List runs
az pipelines runs list

# View run details
az pipelines runs show --id <run-id>

# Download artifacts
az pipelines runs artifact download --run-id <run-id>
```

---

## 📁 File Structure

```
├── Dockerfile                          # Multi-stage Docker image
├── docker-compose.yml                  # Orchestration
├── .dockerignore                       # Build exclusions
├── .env.example                        # Config template
├── azure-pipelines-container.yml       # CI/CD pipeline
├── run-tests-container.sh              # Helper script
├── README-Container-Testing.md         # Full docs
├── QUICKSTART-Container.md             # Quick start
├── CONTAINER-SETUP-SUMMARY.md          # Setup guide
└── ARCHITECTURE-Container.md           # Architecture
```

---

## 🔧 Environment Variables

```bash
# Azure
AZURE_SUBSCRIPTION_ID=xxx
AZURE_TENANT_ID=xxx
AZURE_RESOURCE_GROUP=xxx
AZURE_WORKSPACE_NAME=xxx

# Test Settings
BROWSER_TYPE=chromium
HEADLESS_MODE=true
WORKERS=2
TIMEOUT=60000
```

---

## 📊 Test Categories

| Category | Filter | Description |
|----------|--------|-------------|
| BDD | `Category=BDD` | Behavior-driven tests |
| UI | `Category=UI` | User interface tests |
| API | `Category=API` | API integration tests |
| VSCode | `Category=VSCode` | VS Code Desktop tests (GUI) |
| All | (none) | All test categories |

---

## 🎯 Common Scenarios

### Local Testing
```bash
# 1. Create .env file
cp .env.example .env

# 2. Build image
docker build -t azure-ml-tests:latest .

# 3. Run tests
./run-tests-container.sh --category bdd

# 4. View results
open TestResults/BDD/index.html
```

### Azure Pipelines Setup
```bash
# 1. Add secrets in Azure DevOps
# 2. Update pipeline variables
# 3. Push code
git add .
git commit -m "Add container support"
git push origin main
```

### Debugging
```bash
# Interactive shell
./run-tests-container.sh --interactive

# View container logs
docker logs azure-ml-test-runner

# Check test results
cat TestResults/BDD/*.trx
```

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| Docker not running | `docker info` |
| Permission denied | `chmod +x run-tests-container.sh` |
| Tests failing | `./run-tests-container.sh --interactive` |
| Out of memory | Increase Docker memory to 8GB |
| Slow builds | Use `DOCKER_BUILDKIT=1` |

---

## 📚 Documentation

- **Quick Start**: [QUICKSTART-Container.md](QUICKSTART-Container.md)
- **Full Guide**: [README-Container-Testing.md](README-Container-Testing.md)
- **Architecture**: [ARCHITECTURE-Container.md](ARCHITECTURE-Container.md)
- **Setup Summary**: [CONTAINER-SETUP-SUMMARY.md](CONTAINER-SETUP-SUMMARY.md)

---

## ✅ Pre-Flight Checklist

- [ ] Docker installed and running
- [ ] `.env` file created (local)
- [ ] Azure secrets configured (pipeline)
- [ ] Image builds successfully
- [ ] Tests pass locally
- [ ] Pipeline YAML updated

---

## 🎓 Best Practices

✅ **DO:**
- Use multi-stage builds
- Mount volumes for results
- Use environment variables
- Run in headless mode
- Cache dependencies
- Scan for vulnerabilities

❌ **DON'T:**
- Commit secrets
- Run as root in production
- Use `latest` tag in production
- Store large files in images
- Ignore security warnings

---

## 🆘 Quick Help

```bash
# Show help
./run-tests-container.sh --help

# Check Docker
docker info
docker version

# View container status
docker ps -a

# View images
docker images

# Clean everything
docker system prune -af --volumes
```

---

**Need more help?** Check the full documentation or open an issue!