using System.ServiceProcess;

namespace PowerShellWebApp.Services;

public class PowerShellService
{
    public Task<List<ServiceStatus>> GetServiceStatusAsync()
    {
        var results = new List<ServiceStatus>();
        
        // Sample data - you can modify this or load from configuration
        var serversAndServices = new Dictionary<string, string[]>
        {
            { "DESKTOP-CH5B0I4", new[] { "AsusAppService" } }
        };

        // Use built-in .NET ServiceController for fast, reliable service status
        results = GetServicesUsingDotNet(serversAndServices);

        return Task.FromResult(results);
    }

    private List<ServiceStatus> GetServicesUsingDotNet(Dictionary<string, string[]> serversAndServices)
    {
        var results = new List<ServiceStatus>();

        foreach (var server in serversAndServices.Keys)
        {
            foreach (var service in serversAndServices[server])
            {
                try
                {
                    using var serviceController = new ServiceController(service);
                    results.Add(new ServiceStatus
                    {
                        Server = server,
                        Service = service,
                        Status = serviceController.Status.ToString()
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