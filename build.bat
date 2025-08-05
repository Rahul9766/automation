@echo off
echo Building Azure Server Manager...
echo.

REM Clean previous builds
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "obj\Release" rmdir /s /q "obj\Release"

REM Build self-contained application
echo Building self-contained application for Windows x64...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Build failed! Please check the error messages above.
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo.
echo The standalone application is located in:
echo bin\Release\net6.0\win-x64\publish\
echo.
echo To deploy:
echo 1. Copy the entire 'publish' folder to your Azure server
echo 2. Run AzureServerManager.exe on the server
echo 3. Access the web interface at http://localhost:5000
echo.
pause 