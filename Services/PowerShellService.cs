using System.Diagnostics;

namespace PowerShellWebApp.Services;

public class PowerShellService
{
    public Task<List<ServiceStatus>> GetServiceStatusAsync()
    {
        var results = new List<ServiceStatus>();
        
        // Sample data - you can modify this or load from configuration
        var serversAndServices = new Dictionary<string, string[]>
        {
            { "DESKTOP-CH5B0I4", new[] { "AsusAppService" } },
            { "localhost", new[] { "Spooler", "Themes" } }
        };

        // Use PowerShell command for service status
        results = GetServicesUsingPowerShell(serversAndServices);

        return Task.FromResult(results);
    }

    private List<ServiceStatus> GetServicesUsingPowerShell(Dictionary<string, string[]> serversAndServices)
    {
        var results = new List<ServiceStatus>();

        foreach (var server in serversAndServices.Keys)
        {
            foreach (var service in serversAndServices[server])
            {
                try
                {
                    // Use PowerShell to get service status on remote server
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-ExecutionPolicy Bypass -Command \"Get-Service -ComputerName '{server}' -Name '{service}' -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Status\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = new Process { StartInfo = startInfo };
                    process.Start();
                    
                    // Add timeout to prevent hanging
                    if (!process.WaitForExit(5000)) // 5 second timeout
                    {
                        try { process.Kill(); } catch { }
                        results.Add(new ServiceStatus
                        {
                            Server = server,
                            Service = service,
                            Status = "Timeout"
                        });
                        continue;
                    }
                    
                    var output = process.StandardOutput.ReadToEnd().Trim();
                    var status = "NotFound";
                    
                    if (!string.IsNullOrEmpty(output))
                    {
                        // The output should be the direct status value
                        status = output;
                    }

                    results.Add(new ServiceStatus
                    {
                        Server = server,
                        Service = service,
                        Status = status
                    });
                }
                catch (Exception)
                {
                    results.Add(new ServiceStatus
                    {
                        Server = server,
                        Service = service,
                        Status = "NotFound"
                    });
                }
            }
        }

        return results;
    }
}

public class ServiceStatus
{
    public string Server { get; set; } = "";
    public string Service { get; set; } = "";
    public string Status { get; set; } = "";
} 