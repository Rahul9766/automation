param (
    [Parameter(Mandatory=$true)]
    [hashtable]$ServersAndServices
)

# Result array
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
                Status = "NotFound"
            }
        }
    }
}

# Output result as table
$results | Format-Table -AutoSize

# Optional: Export to JSON (for web app integration later)
# $results | ConvertTo-Json -Depth 2
