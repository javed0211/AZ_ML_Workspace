# VS Code Desktop Testing with TypeScript Playwright

This directory contains the TypeScript Playwright automation for testing VS Code Desktop integration with Azure ML Workspace.

## Architecture

The solution uses a hybrid approach:
- **C# (Reqnroll BDD)**: Main test framework, reporting, and Azure SDK integrations
- **TypeScript Playwright**: VS Code Desktop automation (Electron app testing)
- **Node.js Process**: Bridge between C# and TypeScript

## Files Structure

```
VSCodeDesktop/
├── package.json              # Node.js dependencies
├── tsconfig.json             # TypeScript configuration
├── src/
│   └── vscode-automation.ts  # Main TypeScript automation script
├── dist/                     # Compiled JavaScript (auto-generated)
└── README.md                 # This file
```

## Setup

1. **Install Node.js dependencies:**
   ```bash
   cd VSCodeDesktop
   npm install
   ```

2. **Install Playwright browsers:**
   ```bash
   npx playwright install
   ```

3. **Compile TypeScript:**
   ```bash
   npm run build
   ```

## Usage from C#

The `VSCodeDesktopHelper` class in C# provides the interface to the TypeScript automation:

```csharp
// Inject the helper
var vsCodeHelper = serviceProvider.GetRequiredService<VSCodeDesktopHelper>();

// Create ability
var vsCodeAbility = UseVSCodeDesktop.With(vsCodeHelper);
actor.Can(vsCodeAbility);

// Launch VS Code
await actor.AttemptsTo(StartVSCodeDesktop.Now());

// Check interactivity
var isInteractive = await actor.AsksFor(VSCodeInteractivity.IsWorking());
```

## Available Actions

### Launch VS Code
```csharp
await vsCodeAbility.LaunchAsync();
await vsCodeAbility.LaunchAsync("/path/to/workspace");
```

### Check Interactivity
```csharp
var result = await vsCodeAbility.CheckInteractivityAsync();
```

### Application Links
```csharp
var result = await vsCodeAbility.CheckApplicationLinksAsync();
```

### Connect to Remote Compute
```csharp
var result = await vsCodeAbility.ConnectToComputeAsync("compute-name");
```

### Take Screenshot
```csharp
var result = await vsCodeAbility.TakeScreenshotAsync("screenshot.png");
```

### Close VS Code
```csharp
await vsCodeAbility.CloseAsync();
```

## TypeScript Actions

The TypeScript script supports these actions:

- `launch`: Launch VS Code Desktop
- `checkInteractivity`: Test if VS Code responds to keyboard input
- `openWorkspace`: Open a workspace folder
- `connectToCompute`: Connect to remote compute instance
- `checkApplicationLinks`: Check available application integrations
- `takeScreenshot`: Capture VS Code window
- `close`: Close VS Code

## Environment Variables

Set these environment variables for VS Code path detection:

```bash
export VSCODE_PATH="/Applications/Visual Studio Code.app/Contents/MacOS/Electron"
```

## Troubleshooting

### VS Code Not Found
- Ensure VS Code is installed
- Set `VSCODE_PATH` environment variable
- Check the paths in `findVSCodeExecutable()` method

### TypeScript Compilation Issues
```bash
cd VSCodeDesktop
npm run build
```

### Playwright Issues
```bash
cd VSCodeDesktop
npx playwright install
```

### Node.js Process Issues
- Check Node.js is installed and accessible
- Verify the compiled JavaScript exists in `dist/` folder
- Check console logs for detailed error messages

## Example BDD Scenario

```gherkin
Scenario: Azure ML Workspace with VS Code Desktop Integration
    When I go to workspace "ml-workspace"
    And If login required I login as user "Javed Khan"
    And I select Workspace "CTO-workspace"
    And I choose compute option
    And I open compute "com-jk"
    And If compute is not running, I start compute
    Then I check if application link are enabled
    When I start VS code Desktop
    Then I check if I am able to interact with VS code
```

## Development

To modify the TypeScript automation:

1. Edit `src/vscode-automation.ts`
2. Run `npm run build` to compile
3. Test with C# integration

The C# helper automatically compiles TypeScript if source is newer than compiled version.