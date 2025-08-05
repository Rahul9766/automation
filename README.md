# Azure Server Manager

A standalone web application for Azure Windows Server service control and file operations.

## 🚀 Features

### Service Management
- Display list of servers with their services
- Real-time service status monitoring (auto-refresh every 2 seconds)
- Individual service control (Start, Stop, Restart)
- Visual status indicators with color coding

### Indexing Section
- Bulk operations for indexing servers
- Start/Stop all services across all indexing servers
- Separate server list for indexing operations

### ThinClient Down Section
- File rename operations on remote servers
- Forward operation: `default.html` → `temp.html`, `backup.html` → `default.html`
- Reverse operation: `default.html` → `backup.html`, `temp.html` → `default.html`
- Configurable folder paths

## 📋 Requirements

- Windows Server with .NET 8.0 Runtime (or self-contained deployment)
- PowerShell execution policy allowing remote execution
- Network access to target servers
- Appropriate credentials for remote server access

## 🛠️ Deployment

### Option 1: Self-Contained Deployment (Recommended)

1. **Build the application:**
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

2. **Copy the published files to your Azure server:**
   - Copy the entire `bin/Release/net8.0/win-x64/publish/` folder to your server
   - The application will be completely standalone with no external dependencies

3. **Run the application:**
   ```bash
   AzureServerManager.exe
   ```

4. **Access the web interface:**
   - Open a web browser and navigate to `http://localhost:5000`
   - The application will be available on the configured port

### Option 2: Framework-Dependent Deployment

1. **Ensure .NET 8.0 Runtime is installed on the server**

2. **Build and deploy:**
   ```bash
   dotnet publish -c Release
   ```

3. **Copy files and run:**
   ```bash
   dotnet AzureServerManager.dll
   ```

## ⚙️ Configuration

### Server Configuration

Edit the server configurations in the following files:

- **Main Servers**: `Services/ServerService.cs` - `_servers` list
- **Indexing Servers**: `Services/ServerService.cs` - `_indexingServers` list  
- **ThinClient Servers**: `Services/FileOperationService.cs` - `_thinClientServers` list

Example server configuration:
```csharp
new ServerConfig 
{ 
    Name = "Server1", 
    IpAddress = "192.168.1.10", 
    Username = "admin", 
    Password = "password123",
    Services = new List<string> { "Spooler", "Themes", "AudioSrv", "BITS" }
}
```

### Port Configuration

Edit `appsettings.json` to change the default port:
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  }
}
```

## 🔧 Features

### Real-Time Updates
- Service status updates every 2 seconds via SignalR
- Live status indicators with color coding
- Automatic reconnection handling

### Security
- PowerShell remoting with secure credentials
- No sensitive data exposed in the UI
- Proper session cleanup after operations

### User Interface
- Modern, responsive design
- Tabbed interface for different operations
- Status notifications with auto-dismiss
- Mobile-friendly layout

## 📁 Project Structure

```
AzureServerManager/
├── Controllers/
│   └── ServiceController.cs          # API endpoints
├── Models/
│   └── ServerConfig.cs              # Data models
├── Services/
│   ├── ServerService.cs             # Service management logic
│   └── FileOperationService.cs      # File operations logic
├── Hubs/
│   └── ServiceHub.cs                # SignalR hub for real-time updates
├── wwwroot/
│   ├── index.html                   # Main web interface
│   ├── styles.css                   # Styling
│   └── script.js                    # Frontend logic
├── Program.cs                       # Application entry point
├── appsettings.json                 # Configuration
└── AzureServerManager.csproj        # Project file
```

## 🔍 Troubleshooting

### Common Issues

1. **PowerShell Execution Policy**
   - Ensure PowerShell execution policy allows remote execution
   - Run: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine`

2. **Network Connectivity**
   - Verify network access to target servers
   - Check firewall settings for PowerShell remoting (port 5985/5986)

3. **Authentication**
   - Ensure credentials have appropriate permissions on target servers
   - Verify username/password combinations

4. **Service Names**
   - Use exact Windows service names
   - Common services: "Spooler", "Themes", "AudioSrv", "BITS", "WSearch"

### Logs
- Check console output for detailed error messages
- Application logs will show PowerShell execution results

## 🚨 Security Notes

- **Credentials**: Store credentials securely, consider using encrypted configuration
- **Network**: Ensure the application runs on a secure network
- **Access**: Limit access to authorized personnel only
- **Updates**: Regularly update the application and dependencies

## 📞 Support

For issues or questions:
1. Check the troubleshooting section above
2. Review console logs for error details
3. Verify server configurations and network connectivity

## 📄 License

This project is provided as-is for internal use. Ensure compliance with your organization's security policies and Azure usage guidelines. 