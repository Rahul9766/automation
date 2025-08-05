# Deployment Checklist

## Pre-Deployment Checklist

### ✅ Development Environment
- [ ] .NET 8.0 SDK installed
- [ ] Project builds successfully (`dotnet build`)
- [ ] Application runs locally (`dotnet run`)
- [ ] Web interface accessible at http://localhost:5000

### ✅ Server Requirements
- [ ] Windows Server with .NET 8.0 Runtime (or self-contained deployment)
- [ ] PowerShell execution policy allows remote execution
- [ ] Network connectivity to target servers
- [ ] Firewall allows PowerShell remoting (ports 5985/5986)
- [ ] Appropriate credentials for remote server access

### ✅ Target Server Configuration
- [ ] PowerShell remoting enabled on target servers
- [ ] WinRM service running on target servers
- [ ] Network access between management server and target servers
- [ ] Service names verified (use exact Windows service names)

## Build and Deploy

### ✅ Build Process
1. [ ] Run build script: `build.bat` or `build.ps1`
2. [ ] Verify build output in `bin\Release\net6.0\win-x64\publish\`
3. [ ] Test standalone executable locally

### ✅ Deployment Steps
1. [ ] Copy entire `publish` folder to Azure server
2. [ ] Ensure all files are present in deployment folder
3. [ ] Run `AzureServerManager.exe` on the server
4. [ ] Verify application starts without errors
5. [ ] Access web interface at configured port

## Configuration

### ✅ Server Configuration
- [ ] Update server IP addresses in `Services/ServerService.cs`
- [ ] Update server IP addresses in `Services/FileOperationService.cs`
- [ ] Configure correct usernames and passwords
- [ ] Verify service names match target servers
- [ ] Test connectivity to each target server

### ✅ Application Settings
- [ ] Configure port in `appsettings.json` if needed
- [ ] Set appropriate logging levels
- [ ] Verify folder paths for ThinClient operations

## Testing

### ✅ Functionality Testing
- [ ] Service status monitoring (auto-refresh every 2 seconds)
- [ ] Individual service control (Start, Stop, Restart)
- [ ] Bulk indexing operations (Start All, Stop All)
- [ ] ThinClient file operations (Forward, Reverse)
- [ ] Error handling and status messages

### ✅ Security Testing
- [ ] Verify no sensitive data exposed in UI
- [ ] Test with incorrect credentials
- [ ] Verify proper session cleanup
- [ ] Check network security requirements

## Post-Deployment

### ✅ Monitoring
- [ ] Monitor application logs for errors
- [ ] Verify real-time status updates
- [ ] Test all operations with actual servers
- [ ] Document any issues or customizations

### ✅ Documentation
- [ ] Update server configurations as needed
- [ ] Document any custom settings
- [ ] Create user access documentation
- [ ] Set up monitoring/alerting if required

## Troubleshooting

### Common Issues
- [ ] PowerShell execution policy: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope LocalMachine`
- [ ] Network connectivity: Test with `Test-NetConnection -ComputerName <server> -Port 5985`
- [ ] Authentication: Verify credentials and permissions
- [ ] Service names: Use exact Windows service names

### Logs and Debugging
- [ ] Check console output for detailed error messages
- [ ] Review application logs for PowerShell execution results
- [ ] Test individual PowerShell commands manually
- [ ] Verify target server configurations

## Security Considerations

### ✅ Security Checklist
- [ ] Credentials stored securely (consider encryption)
- [ ] Application runs on secure network
- [ ] Access limited to authorized personnel
- [ ] Regular security updates applied
- [ ] Audit logging enabled if required

## Maintenance

### ✅ Regular Maintenance
- [ ] Monitor application performance
- [ ] Update server configurations as needed
- [ ] Review and update credentials
- [ ] Backup configuration files
- [ ] Test after server updates or changes

---

**Note**: This checklist should be completed before and after each deployment to ensure successful operation of the Azure Server Manager application. 