# Container Architecture - Azure ML Test Automation

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         AZURE DEVOPS / AZURE PIPELINES                       │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                    azure-pipelines-container.yml                     │   │
│  │                                                                       │   │
│  │  Stage 1: Build Image                                                │   │
│  │  ├─ Build Dockerfile                                                 │   │
│  │  ├─ Push to Azure Container Registry (ACR)                           │   │
│  │  └─ Security Scan (Trivy)                                            │   │
│  │                                                                       │   │
│  │  Stage 2: Container Tests (Parallel)                                 │   │
│  │  ├─ Job: BDD Tests    (Linux Container)                              │   │
│  │  ├─ Job: UI Tests     (Linux Container)                              │   │
│  │  └─ Job: API Tests    (Linux Container)                              │   │
│  │                                                                       │   │
│  │  Stage 3: Docker Compose Tests                                       │   │
│  │  └─ Run all services in parallel                                     │   │
│  │                                                                       │   │
│  │  Stage 4: Cleanup                                                    │   │
│  │  └─ Remove containers and images                                     │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         Test Results                                 │   │
│  │  • Test Reports (TRX)                                                │   │
│  │  • Code Coverage (Cobertura)                                         │   │
│  │  • Screenshots, Videos, Traces                                       │   │
│  │  • Build Artifacts                                                   │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      │ Push/Pull Images
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                      AZURE CONTAINER REGISTRY (ACR)                          │
│                                                                               │
│  Repository: azure-ml-test-automation                                        │
│  Tags: latest, <build-id>                                                    │
│  Size: ~4.5GB (test stage)                                                   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🐳 Docker Image Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            Dockerfile (Multi-Stage)                          │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Stage 1: base                                                        │   │
│  │ FROM: mcr.microsoft.com/dotnet/sdk:9.0                               │   │
│  │ • .NET SDK 9.0                                                       │   │
│  │ • Node.js 18.x                                                       │   │
│  │ • Python 3.11                                                        │   │
│  │ • Azure CLI                                                          │   │
│  │ • SSH Client                                                         │   │
│  │ Size: ~2.5GB                                                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              ▼                                                │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Stage 2: playwright-deps                                             │   │
│  │ FROM: base                                                           │   │
│  │ • Playwright browser dependencies                                    │   │
│  │ • System libraries (libnss3, libgbm1, etc.)                          │   │
│  │ • Fonts and rendering libraries                                      │   │
│  │ Size: ~3.5GB                                                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              ▼                                                │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Stage 3: build                                                       │   │
│  │ FROM: playwright-deps                                                │   │
│  │ • Copy source code                                                   │   │
│  │ • Restore NuGet packages                                             │   │
│  │ • Build .NET solution                                                │   │
│  │ • Install npm packages                                               │   │
│  │ • Install Playwright browsers                                        │   │
│  │ • Install Python dependencies                                        │   │
│  │ Size: ~4.5GB                                                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              ▼                                                │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Stage 4: test (DEFAULT)                                              │   │
│  │ FROM: build                                                          │   │
│  │ • Test execution environment                                         │   │
│  │ • Environment variables configured                                   │   │
│  │ • Test result directories created                                    │   │
│  │ • Default CMD: run all tests                                         │   │
│  │ Size: ~4.5GB                                                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                              ▼                                                │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Stage 5: runtime (OPTIONAL)                                          │   │
│  │ FROM: mcr.microsoft.com/dotnet/runtime:9.0                           │   │
│  │ • Minimal production image                                           │   │
│  │ • Only runtime dependencies                                          │   │
│  │ • Copy built artifacts                                               │   │
│  │ Size: ~500MB                                                         │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🎭 Test Execution Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          LOCAL DEVELOPMENT                                   │
│                                                                               │
│  Developer Machine                                                           │
│  ├─ Docker Desktop / Docker Engine                                           │
│  ├─ Source Code                                                              │
│  └─ .env file (credentials)                                                  │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Option 1: Helper Script                                             │   │
│  │ $ ./run-tests-container.sh --build --category bdd                   │   │
│  │                                                                       │   │
│  │ ├─ Checks prerequisites                                              │   │
│  │ ├─ Builds Docker image                                               │   │
│  │ ├─ Runs tests in container                                           │   │
│  │ ├─ Mounts TestResults volume                                         │   │
│  │ └─ Shows results summary                                             │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Option 2: Docker Compose                                             │   │
│  │ $ docker-compose up bdd-tests                                        │   │
│  │                                                                       │   │
│  │ ├─ Builds all services                                               │   │
│  │ ├─ Runs specific test service                                        │   │
│  │ ├─ Parallel execution available                                      │   │
│  │ └─ Auto-cleanup on exit                                              │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │ Option 3: Direct Docker                                              │   │
│  │ $ docker run --rm azure-ml-tests:latest                              │   │
│  │                                                                       │   │
│  │ ├─ Full control over container                                       │   │
│  │ ├─ Custom environment variables                                      │   │
│  │ ├─ Volume mounts as needed                                           │   │
│  │ └─ Interactive mode available                                        │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                               │
│                              ▼                                                │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                    Test Results (Local)                              │   │
│  │  ./TestResults/                                                      │   │
│  │  ├── BDD/                                                            │   │
│  │  │   ├── *.trx                                                       │   │
│  │  │   └── coverage.cobertura.xml                                      │   │
│  │  ├── UI/                                                             │   │
│  │  ├── API/                                                            │   │
│  │  └── Coverage/                                                       │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 CI/CD Pipeline Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              TRIGGER                                         │
│  • Git Push (main, develop)                                                  │
│  • Pull Request                                                              │
│  • Schedule (Daily 2 AM UTC)                                                 │
│  • Manual Trigger                                                            │
└────────────────────────────────┬────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                      STAGE 1: BUILD DOCKER IMAGE                             │
│                                                                               │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ 1. Checkout Code                                                      │  │
│  │    └─ Clone repository                                                │  │
│  │                                                                        │  │
│  │ 2. Build Docker Image                                                 │  │
│  │    ├─ docker build -t azure-ml-tests:${BUILD_ID} .                   │  │
│  │    ├─ Multi-stage build (base → playwright-deps → build → test)      │  │
│  │    └─ Tag: latest, ${BUILD_ID}                                        │  │
│  │                                                                        │  │
│  │ 3. Push to Azure Container Registry                                   │  │
│  │    ├─ docker push youracr.azurecr.io/azure-ml-tests:${BUILD_ID}      │  │
│  │    └─ docker push youracr.azurecr.io/azure-ml-tests:latest           │  │
│  │                                                                        │  │
│  │ 4. Security Scan                                                      │  │
│  │    └─ Trivy vulnerability scan                                        │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└────────────────────────────────┬────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                   STAGE 2: CONTAINER TESTS (PARALLEL)                        │
│                                                                               │
│  ┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐      │
│  │   Job: BDD Tests │    │   Job: UI Tests  │    │  Job: API Tests  │      │
│  │                  │    │                  │    │                  │      │
│  │ Container:       │    │ Container:       │    │ Container:       │      │
│  │ testrunner       │    │ testrunner       │    │ testrunner       │      │
│  │                  │    │                  │    │                  │      │
│  │ Steps:           │    │ Steps:           │    │ Steps:           │      │
│  │ 1. Setup .NET    │    │ 1. Setup .NET    │    │ 1. Setup .NET    │      │
│  │ 2. Setup Node.js │    │ 2. Setup Node.js │    │ 2. Restore deps  │      │
│  │ 3. Install       │    │ 3. Install       │    │ 3. Build project │      │
│  │    Playwright    │    │    Playwright    │    │ 4. Run API tests │      │
│  │ 4. Restore deps  │    │ 4. Build project │    │ 5. Publish       │      │
│  │ 5. Build project │    │ 5. Run UI tests  │    │    results       │      │
│  │ 6. Run BDD tests │    │ 6. Publish       │    │                  │      │
│  │ 7. Publish       │    │    results       │    │ Filter:          │      │
│  │    results       │    │                  │    │ Category=API     │      │
│  │                  │    │ Filter:          │    │                  │      │
│  │ Filter:          │    │ Category=UI      │    │ Browser:         │      │
│  │ Category=BDD     │    │                  │    │ N/A              │      │
│  │                  │    │ Browser:         │    │                  │      │
│  │ Browser:         │    │ chromium         │    │ Headless: N/A    │      │
│  │ chromium         │    │                  │    │                  │      │
│  │                  │    │ Headless: true   │    │                  │      │
│  │ Headless: true   │    │                  │    │                  │      │
│  └────────┬─────────┘    └────────┬─────────┘    └────────┬─────────┘      │
│           │                       │                       │                 │
│           └───────────────────────┴───────────────────────┘                 │
│                                   │                                          │
│                                   ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Publish Test Results                               │  │
│  │  • Test Results (TRX format)                                          │  │
│  │  • Code Coverage (Cobertura)                                          │  │
│  │  • Test Artifacts (Screenshots, Videos, Traces)                       │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└────────────────────────────────┬────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                 STAGE 3: DOCKER COMPOSE TESTS (OPTIONAL)                     │
│                                                                               │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ 1. Build Docker Compose Services                                      │  │
│  │    └─ docker-compose build                                            │  │
│  │                                                                        │  │
│  │ 2. Run All Test Services in Parallel                                  │  │
│  │    ├─ docker-compose up bdd-tests                                     │  │
│  │    ├─ docker-compose up ui-tests                                      │  │
│  │    └─ docker-compose up api-tests                                     │  │
│  │                                                                        │  │
│  │ 3. Copy Results from Containers                                       │  │
│  │    ├─ docker cp azure-ml-bdd-tests:/workspace/TestResults/BDD         │  │
│  │    ├─ docker cp azure-ml-ui-tests:/workspace/TestResults/UI           │  │
│  │    └─ docker cp azure-ml-api-tests:/workspace/TestResults/API         │  │
│  │                                                                        │  │
│  │ 4. Publish Merged Results                                             │  │
│  │    └─ Combine all test results and publish                            │  │
│  │                                                                        │  │
│  │ 5. Cleanup                                                            │  │
│  │    └─ docker-compose down                                             │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└────────────────────────────────┬────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        STAGE 4: CLEANUP                                      │
│                                                                               │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ 1. Remove Test Containers                                             │  │
│  │    └─ docker rm -f $(docker ps -aq)                                   │  │
│  │                                                                        │  │
│  │ 2. Clean Docker System                                                │  │
│  │    └─ docker system prune -af --volumes                               │  │
│  │                                                                        │  │
│  │ 3. Archive Artifacts (Optional)                                       │  │
│  │    └─ Keep test results for 30 days                                   │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📦 Docker Compose Services

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          docker-compose.yml                                  │
│                                                                               │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ Service: azure-ml-tests (Base)                                        │  │
│  │ • Base configuration for all test services                            │  │
│  │ • Environment variables                                               │  │
│  │ • Volume mounts                                                       │  │
│  │ • Network configuration                                               │  │
│  │ • Resource limits (4 CPU, 8GB RAM)                                    │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                               │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐          │
│  │ Service:         │  │ Service:         │  │ Service:         │          │
│  │ bdd-tests        │  │ ui-tests         │  │ api-tests        │          │
│  │                  │  │                  │  │                  │          │
│  │ Extends:         │  │ Extends:         │  │ Extends:         │          │
│  │ azure-ml-tests   │  │ azure-ml-tests   │  │ azure-ml-tests   │          │
│  │                  │  │                  │  │                  │          │
│  │ Filter:          │  │ Filter:          │  │ Filter:          │          │
│  │ Category=BDD     │  │ Category=UI      │  │ Category=API     │          │
│  │                  │  │                  │  │                  │          │
│  │ Results:         │  │ Results:         │  │ Results:         │          │
│  │ ./TestResults/   │  │ ./TestResults/   │  │ ./TestResults/   │          │
│  │ BDD              │  │ UI               │  │ API              │          │
│  └──────────────────┘  └──────────────────┘  └──────────────────┘          │
│                                                                               │
│  ┌──────────────────┐  ┌──────────────────┐                                 │
│  │ Service:         │  │ Service:         │                                 │
│  │ electron-tests   │  │ python-          │                                 │
│  │                  │  │ automation       │                                 │
│  │ Extends:         │  │                  │                                 │
│  │ azure-ml-tests   │  │ Extends:         │                                 │
│  │                  │  │ azure-ml-tests   │                                 │
│  │ Command:         │  │                  │                                 │
│  │ npm test         │  │ Command:         │                                 │
│  │                  │  │ python3          │                                 │
│  │ Results:         │  │ automation.py    │                                 │
│  │ ./TestResults/   │  │                  │                                 │
│  │ Electron         │  │ Results:         │                                 │
│  │                  │  │ ./TestResults/   │                                 │
│  │                  │  │ Python           │                                 │
│  └──────────────────┘  └──────────────────┘                                 │
│                                                                               │
│  Network: test-network (bridge)                                              │
│  Volumes: test-results (local)                                               │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔐 Security Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          SECURITY LAYERS                                     │
│                                                                               │
│  Layer 1: Secrets Management                                                 │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ • Azure Key Vault integration                                         │  │
│  │ • Azure DevOps Variable Groups (encrypted)                            │  │
│  │ • .env files (local only, gitignored)                                 │  │
│  │ • No secrets in code or Dockerfile                                    │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                               │
│  Layer 2: Container Security                                                 │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ • Non-root user execution (optional)                                  │  │
│  │ • Read-only file systems where possible                               │  │
│  │ • Resource limits (CPU, memory)                                       │  │
│  │ • Network isolation                                                   │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                               │
│  Layer 3: Image Security                                                     │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ • Vulnerability scanning (Trivy)                                      │  │
│  │ • Base images from Microsoft (trusted)                                │  │
│  │ • Multi-stage builds (minimal attack surface)                         │  │
│  │ • Regular updates and patching                                        │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                               │
│  Layer 4: Network Security                                                   │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ • HTTPS/TLS for all external connections                             │  │
│  │ • SSH keys for Azure ML compute access                                │  │
│  │ • Azure AD authentication                                             │  │
│  │ • Private networks for container communication                        │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                               │
│  Layer 5: Access Control                                                     │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ • Azure RBAC for resource access                                      │  │
│  │ • Service principals with minimal permissions                         │  │
│  │ • ACR authentication for image pulls                                  │  │
│  │ • Pipeline permissions and approvals                                  │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📊 Data Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            DATA FLOW DIAGRAM                                 │
│                                                                               │
│  ┌──────────────┐                                                            │
│  │  Developer   │                                                            │
│  │   Machine    │                                                            │
│  └──────┬───────┘                                                            │
│         │                                                                     │
│         │ 1. Push Code                                                       │
│         ▼                                                                     │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Git Repository (Azure Repos)                       │  │
│  └────────────────────────────────┬─────────────────────────────────────┘  │
│                                   │                                          │
│                                   │ 2. Trigger Pipeline                      │
│                                   ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                      Azure Pipelines Agent                            │  │
│  │  ┌────────────────────────────────────────────────────────────────┐  │  │
│  │  │ 3. Build Docker Image                                           │  │  │
│  │  │    • Source Code → Docker Build Context                         │  │  │
│  │  │    • Dockerfile → Image Layers                                  │  │  │
│  │  │    • Dependencies → Cached Layers                               │  │  │
│  │  └────────────────────────────────────────────────────────────────┘  │  │
│  └────────────────────────────────┬─────────────────────────────────────┘  │
│                                   │                                          │
│                                   │ 4. Push Image                            │
│                                   ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │              Azure Container Registry (ACR)                           │  │
│  │  Repository: azure-ml-test-automation                                 │  │
│  │  Tags: latest, ${BUILD_ID}                                            │  │
│  └────────────────────────────────┬─────────────────────────────────────┘  │
│                                   │                                          │
│                                   │ 5. Pull Image                            │
│                                   ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Test Execution Containers                          │  │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐                      │  │
│  │  │ BDD Tests  │  │ UI Tests   │  │ API Tests  │                      │  │
│  │  └─────┬──────┘  └─────┬──────┘  └─────┬──────┘                      │  │
│  │        │               │               │                               │  │
│  │        │ 6. Execute    │ 6. Execute    │ 6. Execute                   │  │
│  │        │    Tests      │    Tests      │    Tests                     │  │
│  │        │               │               │                               │  │
│  │        │ 7. Connect to Azure ML        │                               │  │
│  │        └───────────────┴───────────────┘                               │  │
│  │                        │                                                │  │
│  └────────────────────────┼────────────────────────────────────────────────┘
│                           │                                                  │
│                           │ 8. Test Azure ML Resources                       │
│                           ▼                                                  │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Azure ML Workspace                                 │  │
│  │  • Compute Instances                                                  │  │
│  │  • SSH Connections                                                    │  │
│  │  • File Synchronization                                               │  │
│  │  • VS Code Remote                                                     │  │
│  └────────────────────────────────────────────────────────────────────────┘
│                           │                                                  │
│                           │ 9. Generate Results                              │
│                           ▼                                                  │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                      Test Results & Artifacts                         │  │
│  │  • TRX Files (Test Results)                                           │  │
│  │  • Cobertura XML (Code Coverage)                                      │  │
│  │  • Screenshots, Videos, Traces                                        │  │
│  │  • Logs and Debug Information                                         │  │
│  └────────────────────────────────┬─────────────────────────────────────┘  │
│                                   │                                          │
│                                   │ 10. Publish                              │
│                                   ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Azure DevOps Artifacts                             │  │
│  │  • Test Results Tab                                                   │  │
│  │  • Code Coverage Tab                                                  │  │
│  │  • Build Artifacts                                                    │  │
│  │  • Pipeline Logs                                                      │  │
│  └────────────────────────────────┬─────────────────────────────────────┘  │
│                                   │                                          │
│                                   │ 11. Notify                               │
│                                   ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Notifications                                      │  │
│  │  • Email                                                              │  │
│  │  • Microsoft Teams                                                    │  │
│  │  • Slack                                                              │  │
│  │  • Azure DevOps Dashboard                                             │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🎯 Component Interaction

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       COMPONENT INTERACTION MAP                              │
│                                                                               │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                        Test Framework                                 │  │
│  │                                                                        │  │
│  │  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐         │  │
│  │  │ C# BDD Tests   │  │ TypeScript     │  │ Python         │         │  │
│  │  │ (SpecFlow)     │  │ Electron Tests │  │ Automation     │         │  │
│  │  │                │  │                │  │                │         │  │
│  │  │ • Gherkin      │  │ • VS Code      │  │ • Azure SDK    │         │  │
│  │  │   Features     │  │   Automation   │  │ • SSH/SFTP     │         │  │
│  │  │ • Step Defs    │  │ • Process Mgmt │  │ • File Sync    │         │  │
│  │  │ • Playwright   │  │ • Cross-       │  │ • CLI Tools    │         │  │
│  │  │   Integration  │  │   Platform     │  │                │         │  │
│  │  └────────┬───────┘  └────────┬───────┘  └────────┬───────┘         │  │
│  │           │                   │                   │                  │  │
│  │           └───────────────────┴───────────────────┘                  │  │
│  │                              │                                        │  │
│  └──────────────────────────────┼────────────────────────────────────────┘
│                                 │                                          │
│                                 ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Container Runtime                                  │  │
│  │                                                                        │  │
│  │  ┌────────────────────────────────────────────────────────────────┐  │  │
│  │  │ Docker Engine                                                   │  │  │
│  │  │  • Container Orchestration                                      │  │  │
│  │  │  • Resource Management                                          │  │  │
│  │  │  • Network Isolation                                            │  │  │
│  │  │  • Volume Management                                            │  │  │
│  │  └────────────────────────────────────────────────────────────────┘  │  │
│  └──────────────────────────────┬─────────────────────────────────────────┘
│                                 │                                          │
│                                 ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Azure Services                                     │  │
│  │                                                                        │  │
│  │  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐    │  │
│  │  │ Azure ML   │  │ Azure      │  │ Azure      │  │ Azure      │    │  │
│  │  │ Workspace  │  │ Container  │  │ DevOps     │  │ Key Vault  │    │  │
│  │  │            │  │ Registry   │  │            │  │            │    │  │
│  │  │ • Compute  │  │ • Image    │  │ • Pipelines│  │ • Secrets  │    │  │
│  │  │   Instances│  │   Storage  │  │ • Repos    │  │ • Keys     │    │  │
│  │  │ • SSH      │  │ • Security │  │ • Artifacts│  │ • Certs    │    │  │
│  │  │ • Storage  │  │ • Scanning │  │ • Reports  │  │            │    │  │
│  │  └────────────┘  └────────────┘  └────────────┘  └────────────┘    │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 📈 Scalability & Performance

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    SCALABILITY ARCHITECTURE                                  │
│                                                                               │
│  Horizontal Scaling (Parallel Execution)                                     │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                                                                        │  │
│  │  Single Agent → Multiple Containers                                   │  │
│  │                                                                        │  │
│  │  ┌─────────────────────────────────────────────────────────────┐     │  │
│  │  │ Azure Pipeline Agent                                         │     │  │
│  │  │                                                               │     │  │
│  │  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐    │     │  │
│  │  │  │Container │  │Container │  │Container │  │Container │    │     │  │
│  │  │  │BDD Tests │  │UI Tests  │  │API Tests │  │Electron  │    │     │  │
│  │  │  └──────────┘  └──────────┘  └──────────┘  └──────────┘    │     │  │
│  │  │                                                               │     │  │
│  │  │  Resources: 4 CPU, 8GB RAM per container                     │     │  │
│  │  └─────────────────────────────────────────────────────────────┘     │  │
│  │                                                                        │  │
│  │  Multiple Agents → More Parallelism                                   │  │
│  │                                                                        │  │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐               │  │
│  │  │ Agent Pool 1 │  │ Agent Pool 2 │  │ Agent Pool 3 │               │  │
│  │  │ (4 agents)   │  │ (4 agents)   │  │ (4 agents)   │               │  │
│  │  └──────────────┘  └──────────────┘  └──────────────┘               │  │
│  │                                                                        │  │
│  │  Total Capacity: 12 agents × 4 containers = 48 parallel tests        │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                               │
│  Vertical Scaling (Resource Allocation)                                      │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                                                                        │  │
│  │  Container Resource Limits:                                           │  │
│  │  ├─ CPU: 2-4 cores per container                                      │  │
│  │  ├─ Memory: 4-8GB per container                                       │  │
│  │  ├─ Disk: 20GB per container                                          │  │
│  │  └─ Network: 1Gbps                                                    │  │
│  │                                                                        │  │
│  │  Agent VM Sizes:                                                      │  │
│  │  ├─ Standard: Standard_D4s_v3 (4 vCPU, 16GB RAM)                      │  │
│  │  ├─ Large: Standard_D8s_v3 (8 vCPU, 32GB RAM)                         │  │
│  │  └─ XLarge: Standard_D16s_v3 (16 vCPU, 64GB RAM)                      │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                               │
│  Performance Optimization:                                                   │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │ • Docker layer caching (BuildKit)                                     │  │
│  │ • Azure Container Registry geo-replication                            │  │
│  │ • Parallel test execution (workers=2-4)                               │  │
│  │ • Headless browser mode                                               │  │
│  │ • Incremental builds                                                  │  │
│  │ • Test result streaming                                               │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔍 Monitoring & Observability

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    MONITORING ARCHITECTURE                                   │
│                                                                               │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Azure Monitor                                      │  │
│  │  • Container Insights                                                 │  │
│  │  • Application Insights                                               │  │
│  │  • Log Analytics                                                      │  │
│  └────────────────────────────────┬─────────────────────────────────────┘  │
│                                   │                                          │
│                                   ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Metrics Collection                                 │  │
│  │                                                                        │  │
│  │  Pipeline Metrics:                                                    │  │
│  │  ├─ Build duration                                                    │  │
│  │  ├─ Test execution time                                               │  │
│  │  ├─ Success/failure rates                                             │  │
│  │  └─ Resource utilization                                              │  │
│  │                                                                        │  │
│  │  Container Metrics:                                                   │  │
│  │  ├─ CPU usage                                                         │  │
│  │  ├─ Memory usage                                                      │  │
│  │  ├─ Network I/O                                                       │  │
│  │  └─ Disk I/O                                                          │  │
│  │                                                                        │  │
│  │  Test Metrics:                                                        │  │
│  │  ├─ Test count (passed/failed/skipped)                                │  │
│  │  ├─ Code coverage percentage                                          │  │
│  │  ├─ Test duration per scenario                                        │  │
│  │  └─ Flaky test detection                                              │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
│                                                                               │
│  ┌──────────────────────────────────────────────────────────────────────┐  │
│  │                    Alerting & Notifications                           │  │
│  │  • Test failures → Teams/Slack                                        │  │
│  │  • Performance degradation → Email                                    │  │
│  │  • Security vulnerabilities → Security team                           │  │
│  │  • Resource exhaustion → DevOps team                                  │  │
│  └──────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

*This architecture supports running Azure ML BDD tests in Linux containers on Azure Pipelines with full scalability, security, and observability.*