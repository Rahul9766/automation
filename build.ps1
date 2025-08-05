# Azure Server Manager Build Script
Write-Host "Building Azure Server Manager..." -ForegroundColor Green
Write-Host ""

# Clean previous builds
if (Test-Path "bin\Release") {
    Remove-Item "bin\Release" -Recurse -Force
}
if (Test-Path "obj\Release") {
    Remove-Item "obj\Release" -Recurse -Force
}

# Build self-contained application
Write-Host "Building self-contained application for Windows x64..." -ForegroundColor Yellow
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Build failed! Please check the error messages above." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

Write-Host ""
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "The standalone application is located in:" -ForegroundColor Cyan
Write-Host "bin\Release\net6.0\win-x64\publish\" -ForegroundColor White
Write-Host ""
Write-Host "To deploy:" -ForegroundColor Cyan
Write-Host "1. Copy the entire 'publish' folder to your Azure server" -ForegroundColor White
Write-Host "2. Run AzureServerManager.exe on the server" -ForegroundColor White
Write-Host "3. Access the web interface at http://localhost:5000" -ForegroundColor White
Write-Host ""
Read-Host "Press Enter to exit" 