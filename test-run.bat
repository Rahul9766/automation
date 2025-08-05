@echo off
echo Testing Azure Server Manager...
echo.

REM Check if .NET is available
dotnet --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo Error: .NET SDK is not installed or not in PATH
    echo Please install .NET 6.0 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

REM Run the application
echo Starting Azure Server Manager...
echo.
echo The application will be available at: http://localhost:5000
echo Press Ctrl+C to stop the application
echo.

dotnet run

pause 