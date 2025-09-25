@echo off
setlocal enabledelayedexpansion

REM Test Automation Framework Runner Script for Windows
REM Usage: run-tests.bat [options]

REM Default values
set ENVIRONMENT=dev
set BROWSER=chromium
set HEADED=false
set PARALLEL=false
set LANGUAGE=typescript
set TEST_PATTERN=
set WORKERS=1
set ELECTRON_ONLY=false
set INSTALL_DEPS=false
set OPEN_REPORT=false

REM Function to show usage
:show_usage
echo Test Automation Framework Runner
echo.
echo Usage: %~nx0 [OPTIONS]
echo.
echo Options:
echo   --env ENVIRONMENT     Set test environment (dev, qa, prod) [default: dev]
echo   --browser BROWSER     Set browser (chromium, firefox, webkit) [default: chromium]
echo   --headed              Run tests in headed mode [default: headless]
echo   --parallel            Run tests in parallel [default: sequential]
echo   --workers NUMBER      Number of parallel workers [default: 1]
echo   --language LANG       Test language (typescript, csharp, both) [default: typescript]
echo   --test PATTERN        Test pattern to run [default: all tests]
echo   --electron            Run Electron tests only
echo   --install             Install dependencies and browsers
echo   --report              Open test report after execution
echo   --help                Show this help message
echo.
echo Examples:
echo   %~nx0 --env qa --browser firefox --headed
echo   %~nx0 --parallel --workers 4 --language csharp
echo   %~nx0 --electron --headed
echo   %~nx0 --test "login" --env dev
goto :eof

REM Parse command line arguments
:parse_args
if "%~1"=="" goto :end_parse
if "%~1"=="--env" (
    set ENVIRONMENT=%~2
    shift
    shift
    goto :parse_args
)
if "%~1"=="--browser" (
    set BROWSER=%~2
    shift
    shift
    goto :parse_args
)
if "%~1"=="--headed" (
    set HEADED=true
    shift
    goto :parse_args
)
if "%~1"=="--parallel" (
    set PARALLEL=true
    shift
    goto :parse_args
)
if "%~1"=="--workers" (
    set WORKERS=%~2
    set PARALLEL=true
    shift
    shift
    goto :parse_args
)
if "%~1"=="--language" (
    set LANGUAGE=%~2
    shift
    shift
    goto :parse_args
)
if "%~1"=="--test" (
    set TEST_PATTERN=%~2
    shift
    shift
    goto :parse_args
)
if "%~1"=="--electron" (
    set ELECTRON_ONLY=true
    shift
    goto :parse_args
)
if "%~1"=="--install" (
    set INSTALL_DEPS=true
    shift
    goto :parse_args
)
if "%~1"=="--report" (
    set OPEN_REPORT=true
    shift
    goto :parse_args
)
if "%~1"=="--help" (
    call :show_usage
    exit /b 0
)
echo Unknown option: %~1
call :show_usage
exit /b 1

:end_parse

REM Handle install dependencies
if "%INSTALL_DEPS%"=="true" (
    echo [INFO] Installing dependencies...
    
    if exist package.json (
        echo [INFO] Installing Node.js dependencies...
        call npm install
        echo [INFO] Installing Playwright browsers...
        call npx playwright install
        call npx playwright install-deps
    )
    
    if exist CSharpTests\PlaywrightFramework.csproj (
        echo [INFO] Restoring .NET packages...
        cd CSharpTests
        call dotnet restore
        echo [INFO] Installing Playwright for .NET...
        call pwsh bin\Debug\net8.0\playwright.ps1 install
        cd ..
    )
    
    echo [SUCCESS] Dependencies installed successfully!
    exit /b 0
)

REM Handle open report
if "%OPEN_REPORT%"=="true" (
    echo [INFO] Opening test report...
    if exist Reports\html-report\index.html (
        start Reports\html-report\index.html
    ) else (
        echo [WARNING] No HTML report found. Run tests first.
    )
    exit /b 0
)

REM Validate inputs
if not "%ENVIRONMENT%"=="dev" if not "%ENVIRONMENT%"=="qa" if not "%ENVIRONMENT%"=="prod" (
    echo [ERROR] Invalid environment: %ENVIRONMENT%. Must be dev, qa, or prod.
    exit /b 1
)

if not "%BROWSER%"=="chromium" if not "%BROWSER%"=="firefox" if not "%BROWSER%"=="webkit" (
    echo [ERROR] Invalid browser: %BROWSER%. Must be chromium, firefox, or webkit.
    exit /b 1
)

if not "%LANGUAGE%"=="typescript" if not "%LANGUAGE%"=="csharp" if not "%LANGUAGE%"=="both" (
    echo [ERROR] Invalid language: %LANGUAGE%. Must be typescript, csharp, or both.
    exit /b 1
)

REM Create reports directory
if not exist Reports\screenshots mkdir Reports\screenshots
if not exist Reports\logs mkdir Reports\logs

REM Print configuration
echo [INFO] Test Configuration:
echo   Environment: %ENVIRONMENT%
echo   Browser: %BROWSER%
echo   Headed: %HEADED%
echo   Parallel: %PARALLEL%
if "%PARALLEL%"=="true" echo   Workers: %WORKERS%
echo   Language: %LANGUAGE%
if not "%TEST_PATTERN%"=="" echo   Test Pattern: %TEST_PATTERN%
if "%ELECTRON_ONLY%"=="true" echo   Electron Only: true
echo.

REM Set environment variable
set TEST_ENV=%ENVIRONMENT%

REM Run tests based on language selection
if "%LANGUAGE%"=="typescript" goto :run_typescript
if "%LANGUAGE%"=="csharp" goto :run_csharp
if "%LANGUAGE%"=="both" goto :run_both

:run_typescript
echo [INFO] Running TypeScript tests...
set cmd=npx playwright test

if not "%BROWSER%"=="chromium" set cmd=!cmd! --project=%BROWSER%
if "%HEADED%"=="true" set cmd=!cmd! --headed
if "%PARALLEL%"=="true" set cmd=!cmd! --workers=%WORKERS%
if not "%TEST_PATTERN%"=="" set cmd=!cmd! --grep="%TEST_PATTERN%"
if "%ELECTRON_ONLY%"=="true" set cmd=!cmd! --grep="@electron"

echo [INFO] Executing: !cmd!
call !cmd!
goto :end_tests

:run_csharp
echo [INFO] Running C# tests...
cd CSharpTests

set cmd=dotnet test
if not "%TEST_PATTERN%"=="" set cmd=!cmd! --filter "Name~%TEST_PATTERN%"
set cmd=!cmd! --logger:html --results-directory:../Reports/csharp-results

echo [INFO] Executing: !cmd!
call !cmd!
cd ..
goto :end_tests

:run_both
echo [INFO] Running both TypeScript and C# tests...
call :run_typescript
call :run_csharp
goto :end_tests

:end_tests
echo [SUCCESS] Test execution completed!
echo [INFO] Reports available in: Reports/
echo [INFO] To view HTML report, run: %~nx0 --report

REM Parse arguments
call :parse_args %*