using System.Management.Automation;
using AzureServerManager.Models;

namespace AzureServerManager.Services
{
    public class FileOperationService
    {
        private readonly List<ServerConfig> _thinClientServers;

        public FileOperationService()
        {
            _thinClientServers = new List<ServerConfig>
            {
                new ServerConfig 
                { 
                    Name = "ThinClient1", 
                    IpAddress = "192.168.1.30", 
                    Username = "admin", 
                    Password = "password123",
                    Services = new List<string> { "Spooler" }
                },
                new ServerConfig 
                { 
                    Name = "ThinClient2", 
                    IpAddress = "192.168.1.31", 
                    Username = "admin", 
                    Password = "password123",
                    Services = new List<string> { "Spooler" }
                }
            };
        }

        public List<ServerConfig> GetThinClientServers() => _thinClientServers;

        public async Task<string> PerformFileOperation(FileOperationRequest request)
        {
            var server = _thinClientServers.FirstOrDefault(s => s.Name == request.ServerName);
            if (server == null)
                return "Server not found";

            try
            {
                var script = $@"
                    $cred = New-Object System.Management.Automation.PSCredential('{server.Username}', (ConvertTo-SecureString '{server.Password}' -AsPlainText -Force))
                    $session = New-PSSession -ComputerName '{server.IpAddress}' -Credential $cred
                    Invoke-Command -Session $session -ScriptBlock {{ 
                        $folderPath = '{request.FolderPath}'
                        $defaultPath = Join-Path $folderPath 'default.html'
                        $backupPath = Join-Path $folderPath 'backup.html'
                        $tempPath = Join-Path $folderPath 'temp.html'
                        
                        if ('{request.Operation}' -eq 'forward') {{
                            if (Test-Path $defaultPath) {{
                                Rename-Item -Path $defaultPath -NewName 'temp.html'
                            }}
                            if (Test-Path $backupPath) {{
                                Rename-Item -Path $backupPath -NewName 'default.html'
                            }}
                        }} else {{
                            if (Test-Path $defaultPath) {{
                                Rename-Item -Path $defaultPath -NewName 'backup.html'
                            }}
                            if (Test-Path $tempPath) {{
                                Rename-Item -Path $tempPath -NewName 'default.html'
                            }}
                        }}
                    }}
                    Remove-PSSession $session
                ";

                return await ExecutePowerShellScript(script);
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> PerformFileOperationOnAllServers(string operation, string folderPath)
        {
            var results = new List<string>();

            foreach (var server in _thinClientServers)
            {
                var request = new FileOperationRequest
                {
                    ServerName = server.Name,
                    FolderPath = folderPath,
                    Operation = operation
                };

                var result = await PerformFileOperation(request);
                results.Add($"{server.Name}: {result}");
            }

            return string.Join("\n", results);
        }

        private async Task<string> ExecutePowerShellScript(string script)
        {
            using var ps = PowerShell.Create();
            ps.AddScript(script);
            var results = await ps.InvokeAsync();
            return string.Join("\n", results.Select(r => r.ToString()));
        }
    }
} 