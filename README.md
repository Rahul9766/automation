# PowerShell Web Application

A simple .NET 8 web application that executes PowerShell scripts and displays the results in a browser.

## Features

- Executes the `Check-ServiceStatus.ps1` PowerShell script
- Displays service status for configured servers
- Auto-refreshes every 10 seconds
- Clean, responsive web interface

## Prerequisites

- .NET 8 SDK
- Windows PowerShell (for executing PowerShell scripts)

## How to Run

1. Open a terminal in the project directory
2. Run the following commands:

```bash
dotnet restore
dotnet run
```

3. Open your browser and navigate to `https://localhost:5001` or `http://localhost:5000`

## Configuration

The application is configured to check services on the local machine by default. You can modify the services and servers in the `PowerShellService.cs` file:

```csharp
var serversAndServices = new Dictionary<string, string[]>
{
    { Environment.MachineName, new[] { "spooler", "wuauserv", "bits", "wsearch" } }
};
```

## Project Structure

- `Program.cs` - Application entry point and configuration
- `Services/PowerShellService.cs` - Service to execute PowerShell scripts
- `Controllers/HomeController.cs` - Web controller for handling requests
- `Views/Home/Index.cshtml` - Web interface
- `Check-ServiceStatus.ps1` - PowerShell script for checking service status

## Notes

- The application requires PowerShell execution policy to be set to allow script execution
- Make sure the `Check-ServiceStatus.ps1` file is in the same directory as the application
- The web interface will automatically refresh every 10 seconds to show the latest service status 