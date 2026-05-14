namespace Blink.Deploy.Models
{
    public class AppConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty; // IIS or WindowsService
        public string ServiceName { get; set; } = string.Empty;
        public List<string> PreserveFiles { get; set; } = new();
    }

    public class BlinkConfig
    {
        public List<AppConfig> Apps { get; set; } = new();
    }
}
