# Azure ML BDD Test Automation Framework

A comprehensive Behavior-Driven Development (BDD) test automation framework for Azure ML workspace management, supporting both C# and TypeScript implementations.

## 🏗️ Project Structure

```
AzureML-BDD-Framework/
├── AzureML-BDD-Framework.sln          # Visual Studio Solution File
├── src/
│   ├── AzureML.BDD.CSharp/            # C# BDD Project
│   │   ├── Features/                   # Gherkin feature files
│   │   ├── StepDefinitions/           # C# step implementations
│   │   ├── Utils/                     # C# utility classes
│   │   ├── Hooks/                     # Test hooks and setup
│   │   └── AzureML.BDD.CSharp.csproj # C# project file
│   └── AzureML.BDD.TypeScript/        # TypeScript BDD Project
│       ├── Features/                   # Gherkin feature files
│       ├── StepDefinitions/           # TypeScript step implementations
│       ├── Utils/                     # TypeScript utility classes
│       ├── package.json               # TypeScript dependencies
│       ├── tsconfig.json              # TypeScript configuration
│       └── AzureML.BDD.TypeScript.esproj # TypeScript project file
├── Config/                            # Shared configuration files
├── TestData/                          # Test data files
├── Documentation/                     # Project documentation
├── Scripts/                           # Setup and utility scripts
├── Reports/                           # Test execution reports
└── package.json                       # Root package.json for solution-level commands
```

## 🚀 Getting Started

### Prerequisites

- **Visual Studio 2022** (with .NET 9.0 support)
- **Node.js** (v18 or higher)
- **npm** (v9 or higher)
- **.NET 9.0 SDK**

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

### C# BDD Tests

**From Visual Studio:**
- Right-click on `AzureML.BDD.CSharp` project → Run Tests
- Use Test Explorer to run specific scenarios

**From Command Line:**
```bash
# Run all C# BDD tests
npm run test:csharp

# Or directly from C# project
cd src/AzureML.BDD.CSharp
dotnet test
```

### TypeScript BDD Tests

**From Visual Studio:**
- Right-click on `AzureML.BDD.TypeScript` project → Debug → Start New Instance

**From Command Line:**
```bash
# Run all TypeScript BDD tests
npm run test:typescript

# Run with browser visible
npm run test:typescript:headed

# Run specific scenarios
npm run test:azureml     # Azure ML scenarios only
npm run test:google      # Google search scenarios only

# Dry run (validate step definitions)
npm run bdd:dry-run
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

## 🎯 BDD Scenarios

### Azure ML Workspace Management

Both C# and TypeScript implementations support:

- ✅ **Workspace Access**: Navigate and authenticate to Azure ML workspace
- ✅ **Compute Instance Management**: Start, stop, and monitor compute instances
- ✅ **Multiple Instance Handling**: Manage multiple compute instances simultaneously
- ✅ **VS Code Integration**: Launch and interact with VS Code Desktop
- ✅ **PIM Role Management**: Handle Privileged Identity Management roles

### Google Search (TypeScript Only)

- ✅ **Basic Search**: Search for products and verify results
- ✅ **Result Navigation**: Click on search results and verify navigation

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