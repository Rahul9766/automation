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
            { "localhost", new[] { "Spooler", "Themes", "AsusAppService" } }
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
                    // Use PowerShell to get service status
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-ExecutionPolicy Bypass -Command \"Get-Service -Name '{service}' -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Status\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = new Process { StartInfo = startInfo };
                    process.Start();
                    
                    // Add timeout to prevent hanging
                    var timeout = 3000; // 3 second timeout
                    if (!process.WaitForExit(timeout))
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
                    var error = process.StandardError.ReadToEnd().Trim();
                    var status = "NotFound";
                    
                    // Debug logging
                    Console.WriteLine($"Server: {server}, Service: {service}, Output: '{output}', Error: '{error}'");
                    
                    if (!string.IsNullOrEmpty(output))
                    {
                        // The output should be the direct status value
                        status = output;
                    }
                    else if (!string.IsNullOrEmpty(error))
                    {
                        // If there's an error, log it
                        Console.WriteLine($"PowerShell error for {service} on {server}: {error}");
                        status = "Error";
                    }

                    results.Add(new ServiceStatus
                    {
                        Server = server,
                        Service = service,
                        Status = status
                    });
                }
                catch (Exception ex)
                {
                    // Log the error for debugging
                    Console.WriteLine($"Error checking service {service} on {server}: {ex.Message}");
                    results.Add(new ServiceStatus
                    {
                        Server = server,
                        Service = service,
                        Status = "Error"
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