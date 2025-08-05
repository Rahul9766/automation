namespace AzureServerManager.Models
{
    public class ServerConfig
    {
        public string Name { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<string> Services { get; set; } = new List<string>();
    }

    public class ServiceStatus
    {
        public string ServerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    public class FileOperationRequest
    {
        public string ServerName { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty; // "forward" or "reverse"
    }

    public class ServiceOperationRequest
    {
        public string ServerName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty; // "start", "stop", "restart"
    }
} 