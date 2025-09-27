@echo off
REM Cross-Platform Jupyter Notebook Test Runner Script for Windows

echo 🚀 Cross-Platform Jupyter Notebook Test Runner
echo ==============================================

REM Default values
set DEFAULT_NOTEBOOK_FOLDER=C:\Users\%USERNAME%\Documents\Notebooks
set DEFAULT_NOTEBOOK_FILE=azure_ml_project.ipynb

REM Check if custom folder is provided as argument
if "%1"=="" (
    set NOTEBOOK_FOLDER=%DEFAULT_NOTEBOOK_FOLDER%
    echo 📁 Using default notebook folder: %DEFAULT_NOTEBOOK_FOLDER%
) else (
    set NOTEBOOK_FOLDER=%1
    echo 📁 Using custom notebook folder: %1
)

REM Check if custom file is provided as second argument
if "%2"=="" (
    set NOTEBOOK_FILE=%DEFAULT_NOTEBOOK_FILE%
    echo 📓 Using default notebook file: %DEFAULT_NOTEBOOK_FILE%
) else (
    set NOTEBOOK_FILE=%2
    echo 📓 Using custom notebook file: %2
)

echo 🖥️  Detected platform: Windows

REM Check if VS Code is available
code.cmd --version >nul 2>&1
if %errorlevel%==0 (
    echo ✅ VS Code CLI is available
    for /f "tokens=1" %%i in ('code.cmd --version') do (
        echo %%i
        goto :version_done
    )
    :version_done
) else (
    echo ❌ VS Code CLI not found. Please install VS Code and ensure it's in PATH.
    pause
    exit /b 1
)

REM Check if the notebook folder exists
if exist "%NOTEBOOK_FOLDER%" (
    echo ✅ Notebook folder exists: %NOTEBOOK_FOLDER%
    
    REM Count .ipynb files (simplified for Windows)
    echo 📚 Checking for notebook files...
    dir "%NOTEBOOK_FOLDER%\*.ipynb" >nul 2>&1
    if %errorlevel%==0 (
        echo 📋 Notebook files found:
        dir /b "%NOTEBOOK_FOLDER%\*.ipynb"
    ) else (
        echo ⚠️  No .ipynb files found in folder
    )
) else (
    echo ⚠️  Notebook folder does not exist: %NOTEBOOK_FOLDER%
    echo The test will create a fallback notebook for testing.
)

echo.
echo 🧪 Running Playwright tests...
echo ==============================================

REM Run the specific test for existing notebooks
npx playwright test tests/cross-platform-jupyter.test.ts -g "existing notebook" --headed

echo.
echo ✅ Test execution completed!
echo ==============================================
pause