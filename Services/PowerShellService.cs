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

        // Build the PowerShell script based on the reference script
        var scriptBuilder = new System.Text.StringBuilder();
        scriptBuilder.AppendLine("$serversAndServices = @{");
        foreach (var server in serversAndServices.Keys)
        {
            var servicesArray = string.Join(",", serversAndServices[server].Select(s => $"'{s}'"));
            scriptBuilder.AppendLine($"    '{server}' = @({servicesArray})");
        }
        scriptBuilder.AppendLine("}");
        scriptBuilder.AppendLine("$results = @()");
        scriptBuilder.AppendLine("foreach ($server in $serversAndServices.Keys) {");
        scriptBuilder.AppendLine("    foreach ($service in $serversAndServices[$server]) {");
        scriptBuilder.AppendLine("        try {");
        scriptBuilder.AppendLine("            $svc = Get-Service -ComputerName $server -Name $service -ErrorAction Stop");
        scriptBuilder.AppendLine("            $results += [PSCustomObject]@{");
        scriptBuilder.AppendLine("                Server = $server");
        scriptBuilder.AppendLine("                Service = $service");
        scriptBuilder.AppendLine("                Status = $svc.Status.ToString()");
        scriptBuilder.AppendLine("            }");
        scriptBuilder.AppendLine("        }");
        scriptBuilder.AppendLine("        catch {");
        scriptBuilder.AppendLine("            $results += [PSCustomObject]@{");
        scriptBuilder.AppendLine("                Server = $server");
        scriptBuilder.AppendLine("                Service = $service");
        scriptBuilder.AppendLine("                Status = 'NotFound'");
        scriptBuilder.AppendLine("            }");
        scriptBuilder.AppendLine("        }");
        scriptBuilder.AppendLine("    }");
        scriptBuilder.AppendLine("}");
        scriptBuilder.AppendLine("$results | ConvertTo-Json -Depth 2");

        try
        {
            // Execute the PowerShell script
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-ExecutionPolicy Bypass -Command \"{scriptBuilder}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            
            // Add timeout to prevent hanging
            var timeout = 10000; // 10 second timeout for multiple servers
            if (!process.WaitForExit(timeout))
            {
                try { process.Kill(); } catch { }
                Console.WriteLine("PowerShell script timed out");
                return GetFallbackResults(serversAndServices);
            }
            
            var output = process.StandardOutput.ReadToEnd().Trim();
            var error = process.StandardError.ReadToEnd().Trim();
            
            // Debug logging
            Console.WriteLine($"PowerShell Output: {output}");
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"PowerShell Error: {error}");
            }
            
            if (!string.IsNullOrEmpty(output))
            {
                // Parse JSON output
                try
                {
                    var jsonResults = System.Text.Json.JsonSerializer.Deserialize<ServiceStatus[]>(output);
                    if (jsonResults != null)
                    {
                        results.AddRange(jsonResults);
                        return results;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"JSON parsing error: {ex.Message}");
                }
            }
            
            // Fallback if JSON parsing fails
            return GetFallbackResults(serversAndServices);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error executing PowerShell script: {ex.Message}");
            return GetFallbackResults(serversAndServices);
        }
    }

    private List<ServiceStatus> GetFallbackResults(Dictionary<string, string[]> serversAndServices)
    {
        var results = new List<ServiceStatus>();
        
        foreach (var server in serversAndServices.Keys)
        {
            foreach (var service in serversAndServices[server])
            {
                results.Add(new ServiceStatus
                {
                    Server = server,
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