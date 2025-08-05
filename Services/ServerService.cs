using System.Management.Automation;
using AzureServerManager.Models;
using Microsoft.AspNetCore.SignalR;
using AzureServerManager.Hubs;

namespace AzureServerManager.Services
{
    public class ServerService
    {
        private readonly IHubContext<ServiceHub> _hubContext;
        private readonly List<ServerConfig> _servers;
        private readonly List<ServerConfig> _indexingServers;
        private readonly List<ServerConfig> _thinClientServers;
        private Timer? _statusTimer;

        public ServerService(IHubContext<ServiceHub> hubContext)
        {
            _hubContext = hubContext;
            
            // Initialize server configurations
            _servers = new List<ServerConfig>
            {
                new ServerConfig 
                { 
                    Name = "Server1", 
                    IpAddress = "192.168.1.10", 
                    Username = "admin", 
                    Password = "password123",
                    Services = new List<string> { "Spooler", "Themes", "AudioSrv", "BITS" }
                },
                new ServerConfig 
                { 
                    Name = "Server2", 
                    IpAddress = "192.168.1.11", 
                    Username = "admin", 
                    Password = "password123",
                    Services = new List<string> { "Spooler", "Themes", "AudioSrv", "BITS" }
                }
            };

            _indexingServers = new List<ServerConfig>
            {
                new ServerConfig 
                { 
                    Name = "IndexServer1", 
                    IpAddress = "192.168.1.20", 
                    Username = "admin", 
                    Password = "password123",
                    Services = new List<string> { "WSearch", "Spooler", "Themes" }
                },
                new ServerConfig 
                { 
                    Name = "IndexServer2", 
                    IpAddress = "192.168.1.21", 
                    Username = "admin", 
                    Password = "password123",
                    Services = new List<string> { "WSearch", "Spooler", "Themes" }
                }
            };

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

            StartStatusMonitoring();
        }

        public List<ServerConfig> GetServers() => _servers;
        public List<ServerConfig> GetIndexingServers() => _indexingServers;
        public List<ServerConfig> GetThinClientServers() => _thinClientServers;

        private void StartStatusMonitoring()
        {
            _statusTimer = new Timer(async _ => await UpdateServiceStatuses(), null, TimeSpan.Zero, TimeSpan.FromSeconds(2));
        }

        private async Task UpdateServiceStatuses()
        {
            var allStatuses = new List<ServiceStatus>();

            // Get statuses for main servers
            foreach (var server in _servers)
            {
                foreach (var service in server.Services)
                {
                    var status = await GetServiceStatus(server, service);
                    allStatuses.Add(status);
                }
            }

            // Get statuses for indexing servers
            foreach (var server in _indexingServers)
            {
                foreach (var service in server.Services)
                {
                    var status = await GetServiceStatus(server, service);
                    allStatuses.Add(status);
                }
            }

            await _hubContext.Clients.All.SendAsync("ServiceStatusUpdate", allStatuses);
        }

        private async Task<ServiceStatus> GetServiceStatus(ServerConfig server, string serviceName)
        {
            try
            {
                var script = $@"
                    $cred = New-Object System.Management.Automation.PSCredential('{server.Username}', (ConvertTo-SecureString '{server.Password}' -AsPlainText -Force))
                    $session = New-PSSession -ComputerName '{server.IpAddress}' -Credential $cred
                    $service = Invoke-Command -Session $session -ScriptBlock {{ Get-Service -Name '{serviceName}' }}
                    Remove-PSSession $session
                    $service.Status
                ";

                var result = await ExecutePowerShellScript(script);
                return new ServiceStatus
                {
                    ServerName = server.Name,
                    ServiceName = serviceName,
                    Status = result.Trim(),
                    LastUpdated = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                return new ServiceStatus
                {
                    ServerName = server.Name,
                    ServiceName = serviceName,
                    Status = "Error: " + ex.Message,
                    LastUpdated = DateTime.Now
                };
            }
        }

        public async Task<string> ControlService(ServiceOperationRequest request)
        {
            var server = _servers.FirstOrDefault(s => s.Name == request.ServerName) ??
                        _indexingServers.FirstOrDefault(s => s.Name == request.ServerName);

            if (server == null)
                return "Server not found";

            try
            {
                var script = $@"
                    $cred = New-Object System.Management.Automation.PSCredential('{server.Username}', (ConvertTo-SecureString '{server.Password}' -AsPlainText -Force))
                    $session = New-PSSession -ComputerName '{server.IpAddress}' -Credential $cred
                    Invoke-Command -Session $session -ScriptBlock {{ 
                        $service = Get-Service -Name '{request.ServiceName}'
                        switch ('{request.Operation}') {{
                            'start' {{ $service.Start() }}
                            'stop' {{ $service.Stop() }}
                            'restart' {{ $service.Stop(); Start-Sleep -Seconds 2; $service.Start() }}
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

        public async Task<string> ControlAllIndexingServices(string operation)
        {
            var results = new List<string>();

            foreach (var server in _indexingServers)
            {
                foreach (var service in server.Services)
                {
                    var request = new ServiceOperationRequest
                    {
                        ServerName = server.Name,
                        ServiceName = service,
                        Operation = operation
                    };

                    var result = await ControlService(request);
                    results.Add($"{server.Name}.{service}: {result}");
                }
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