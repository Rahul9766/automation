using System.Diagnostics;

namespace PowerShellWebApp.Services
{
    public class PowerShellService
    {
        public Task<List<ServiceStatus>> GetServiceStatusAsync()
        {
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

            // Build the PowerShell script
            var scriptBuilder = new System.Text.StringBuilder();
            scriptBuilder.AppendLine("$serversAndServices = @{");
            foreach (var server in serversAndServices)
            {
                var servicesList = string.Join(",", server.Value.Select(s => $"'{s}'"));
                scriptBuilder.AppendLine($"    '{server.Key}' = @({servicesList})");
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
                // Write script to temporary file
                var tempScriptPath = Path.Combine(Path.GetTempPath(), $"temp_{Guid.NewGuid()}.ps1");
                File.WriteAllText(tempScriptPath, scriptBuilder.ToString());

                // Execute the script
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-ExecutionPolicy Bypass -File \"{tempScriptPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var timeout = 10000; // 10 seconds
                if (!process.WaitForExit(timeout))
                {
                    process.Kill();
                    Console.WriteLine("PowerShell script timed out.");
                    return GetFallbackResults(serversAndServices);
                }

                var output = process.StandardOutput.ReadToEnd().Trim();
                var error = process.StandardError.ReadToEnd().Trim();

                Console.WriteLine("PowerShell Output:");
                Console.WriteLine(output);
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("PowerShell Error:");
                    Console.WriteLine(error);
                }

                // Delete the temp script file
                if (File.Exists(tempScriptPath))
                    File.Delete(tempScriptPath);

                if (!string.IsNullOrEmpty(output))
                {
                    try
                    {
                        var jsonResults = System.Text.Json.JsonSerializer.Deserialize<List<ServiceStatus>>(output);
                        if (jsonResults != null)
                        {
                            results.AddRange(jsonResults);
                            return results;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("JSON Parsing Error: " + ex.Message);
                        return GetFallbackResults(serversAndServices);
                    }
                }

                return GetFallbackResults(serversAndServices);
            }
            catch (Exception ex)
            {
                Console.WriteLine("PowerShell execution error: " + ex.Message);
                return GetFallbackResults(serversAndServices);
            }
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
