namespace Blink.Deploy.Models
{
    public class AppConfig
    {
        private string _path = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Path
        {
            get => _path;
            set => _path = value.TrimEnd('\\', '/');
        }

        public string? ServiceType { get; set; }
        public string? ServiceName { get; set; }
        public List<string> PreserveFiles { get; set; } = new();
    }

    public class BlinkConfig
    {
        public List<AppConfig> Apps { get; set; } = new();
    }
}