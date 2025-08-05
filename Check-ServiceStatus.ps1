using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Management.Automation; // Only needed if you later decide to use PowerShell SDK

namespace PowerShellWebApp.Services
{
    public class PowerShellService
    {
        public Task<List<ServiceStatus>> GetServiceStatusAsync()
        {
            // Define servers and services to monitor
            var serversAndServices = new Dictionary<string, string[]>
            {
                { "DESKTOP-CH5B0I4", new[] { "AsusAppService" } },
                { "server1", new[] { "service1", "Themes" } }
            };

            var results = GetServicesUsingPowerShell(serversAndServices);
            return Task.FromResult(results);
        }

        private List<ServiceStatus> GetServicesUsingPowerShell(Dictionary<string, string[]> serversAndServices)
        {
            var results = new List<ServiceStatus>();

            // PowerShell script content (parameterized script)
            string scriptContent = @"
param (
    [Parameter(Mandatory=$true)]
    [hashtable]$ServersAndServices
)

$results = @()

foreach ($server in $ServersAndServices.Keys) {
    foreach ($service in $ServersAndServices[$server]) {
        try {
            $svc = Get-Service -ComputerName $server -Name $service -ErrorAction Stop
            $results += [PSCustomObject]@{
                Server = $server
                Service = $service
                Status = $svc.Status
            }
        }
        catch {
            $results += [PSCustomObject]@{
                Server = $server
                Service = $service
                Status = 'NotFound'
            }
        }
    }
}

$results | ConvertTo-Json -Depth 2
";

            // Save the script to a temp .ps1 file
            var scriptPath = Path.Combine(Path.GetTempPath(), $"ServiceCheck_{Guid.NewGuid()}.ps1");
            File.WriteAllText(scriptPath, scriptContent);

            // Build PowerShell argument as Hashtable string
            var hashtableBuilder = new StringBuilder();
            hashtableBuilder.Append("@{");
            foreach (var kvp in serversAndServices)
            {
                var services = string.Join(",", kvp.Value.Select(s => $"'{s}'"));
                hashtableBuilder.Append($"'{kvp.Key}'=@({services});");
            }
            hashtableBuilder.Append("}");

            // Build full process start info
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -ServersAndServices {hashtableBuilder}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using var process = new Process { StartInfo = psi };
                process.Start();

                var output = process.StandardOutput.ReadToEnd().Trim();
                var error = process.StandardError.ReadToEnd().Trim();
                process.WaitForExit();

                Console.WriteLine("PowerShell Output:");
                Console.WriteLine(output);
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.WriteLine("PowerShell Error:");
                    Console.WriteLine(error);
                }

                // Delete the temporary script
                if (File.Exists(scriptPath))
                    File.Delete(scriptPath);

                if (!string.IsNullOrWhiteSpace(output))
                {
                    var jsonResults = JsonSerializer.Deserialize<List<ServiceStatus>>(output);
                    if (jsonResults != null)
                    {
                        results.AddRange(jsonResults);
                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing PowerShell: {ex.Message}");
            }

            return GetFallbackResults(serversAndServices);
        }

        private List<ServiceStatus> GetFallbackResults(Dictionary<string, string[]> serversAndServices)
        {
            var results = new List<ServiceStatus>();

            foreach (var server in serversAndServices)
            {
                foreach (var service in server.Value)
                {
                    results.Add(new ServiceStatus
                    {
                        Server = server.Key,
                        Service = service,
                        Status = "Error"
                    });
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
}
