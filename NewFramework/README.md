# Azure ML BDD Test Automation Framework

A comprehensive Behavior-Driven Development (BDD) test automation framework for Azure ML workspace management, supporting C#, TypeScript, and Electron-based VS Code automation.

## 🏗️ Project Structure

```
NewFramework/
├── CSharpTests/                       # C# Playwright BDD Tests
│   ├── Features/                      # Gherkin feature files
│   ├── StepDefinitions/              # C# step implementations
│   ├── Utils/                        # C# utility classes
│   ├── Hooks/                        # Test hooks and setup
│   └── PlaywrightFramework.csproj    # C# project file
├── ElectronTests/                     # VS Code Electron Automation
│   ├── tests/                        # Electron test files
│   ├── utils/                        # VS Code automation utilities
│   ├── config/                       # VS Code configuration
│   ├── package.json                  # Node.js dependencies
│   ├── tsconfig.json                 # TypeScript configuration
│   └── playwright.config.ts          # Playwright configuration
├── Config/                           # Shared configuration files
├── TestData/                         # Test data files
├── Documentation/                    # Project documentation
├── Scripts/                          # Setup and utility scripts
└── Reports/                          # Test execution reports
```

## 🚀 Getting Started

### Prerequisites

- **Visual Studio Code** (for Electron testing)
- **.NET 9.0 SDK** (for C# tests)
- **Node.js** (v16 or higher)
- **npm** (v8 or higher)
- **Playwright** (installed automatically)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd AzureML-BDD-Framework
   ```

2. **Install dependencies**
   ```bash
   npm run setup
   ```

3. **Open in Visual Studio**
   - Open `AzureML-BDD-Framework.sln` in Visual Studio
   - Both C# and TypeScript projects will be visible in Solution Explorer

## 🧪 Running Tests

### C# Playwright Tests

**From Command Line:**
```bash
# Run all C# tests
cd CSharpTests
dotnet test

# Run specific feature
dotnet test --filter "TestCategory=AzureML"
```

### VS Code Electron Tests

**Quick Start:**
```bash
# Setup and run (first time)
cd ElectronTests
./setup.sh  # macOS/Linux
# or
.\setup.ps1  # Windows

# Run all tests
npm test

# Run with visible VS Code
npm run test:headed

# Run specific tests
npm test -- --grep "basic functionality"
```

**Using Scripts:**
```bash
# From project root
./Scripts/run-electron-tests.sh --headed
# or
.\Scripts\run-electron-tests.ps1 -Headed
```

## 📁 Visual Studio Integration

### Solution Structure in VS

When you open the solution in Visual Studio, you'll see:

```
📁 AzureML-BDD-Framework
├── 📁 AzureML.BDD.CSharp
│   ├── 📁 Features
│   │   ├── 📄 AzureMLWorkspace.feature
│   │   └── 📄 AzureAISearch.feature
│   ├── 📁 StepDefinitions
│   ├── 📁 Utils
│   └── 📁 Hooks
├── 📁 AzureML.BDD.TypeScript
│   ├── 📁 Features
│   │   ├── 📄 AzureMLWorkspace.feature
│   │   └── 📄 GoogleSearch.feature
│   ├── 📁 StepDefinitions
│   └── 📁 Utils
├── 📁 Solution Items
│   ├── 📄 README.md
│   ├── 📄 package.json
│   └── 📄 tsconfig.json
├── 📁 Shared
│   └── 📄 appsettings.json
├── 📁 Documentation
└── 📁 Scripts
```

### Features

- **IntelliSense Support**: Full IntelliSense for both C# and TypeScript
- **Feature File Syntax Highlighting**: Gherkin syntax highlighting for .feature files
- **Integrated Debugging**: Debug both C# and TypeScript tests from VS
- **Test Explorer Integration**: C# tests appear in Test Explorer
- **Project Dependencies**: Proper project references and shared resources

## 🎯 Test Scenarios

### C# Playwright Tests (Azure ML Focus)

- ✅ **Workspace Access**: Navigate and authenticate to Azure ML workspace
- ✅ **Compute Instance Management**: Start, stop, and monitor compute instances
- ✅ **Multiple Instance Handling**: Manage multiple compute instances simultaneously
- ✅ **PIM Role Management**: Handle Privileged Identity Management roles
- ✅ **API Integration**: Test Azure ML REST APIs

### VS Code Electron Tests (Desktop Automation)

- ✅ **VS Code Launch**: Cross-platform VS Code automation
- ✅ **File Operations**: Create, edit, save files and folders
- ✅ **Editor Interactions**: Text editing, syntax highlighting, IntelliSense
- ✅ **Command Palette**: Execute VS Code commands programmatically
- ✅ **Terminal Integration**: Run commands in integrated terminal
- ✅ **Extension Management**: Install, configure, and test extensions
- ✅ **Settings Configuration**: Modify user and workspace settings
- ✅ **Azure ML Integration**: Test Azure ML extension workflows
- ✅ **Jupyter Notebooks**: Create and execute notebook cells
- ✅ **Python Development**: Python script creation and execution

## 🔧 Configuration

### Shared Configuration

- **Config/appsettings.json**: Shared configuration for both projects
- **TestData/**: Common test data files
- **Reports/**: Unified reporting location

### Project-Specific Configuration

**C# Project:**
- `reqnroll.json`: Reqnroll (BDD) configuration
- `AzureML.BDD.CSharp.csproj`: NuGet packages and build settings

**TypeScript Project:**
- `package.json`: npm dependencies and scripts
- `tsconfig.json`: TypeScript compiler options
- `cucumber.js`: Cucumber configuration

## 📊 Reporting

Test reports are generated in the `Reports/` directory:

- **HTML Reports**: `cucumber-report.html`
- **JSON Reports**: `cucumber-report.json`
- **JUnit Reports**: `junit-results.xml`
- **Screenshots**: `screenshots/` (on test failures)
- **Logs**: `logs/test-execution.log`

## 🛠️ Development

### Adding New Features

1. **Create Feature File**: Add `.feature` file in respective `Features/` directory
2. **Implement Step Definitions**: Add step implementations in `StepDefinitions/`
3. **Update Project Files**: Ensure feature files are included in project configuration

### Extending Utilities

- **C# Utils**: Extend classes in `src/AzureML.BDD.CSharp/Utils/`
- **TypeScript Utils**: Extend classes in `src/AzureML.BDD.TypeScript/Utils/`

## 🤝 Contributing

1. Follow BDD best practices
2. Maintain consistency between C# and TypeScript implementations
3. Update documentation for new features
4. Ensure all tests pass before committing

## 📝 License

MIT License - see LICENSE file for details.