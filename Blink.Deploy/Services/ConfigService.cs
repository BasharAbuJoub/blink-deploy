using Blink.Deploy.Models;
using System.Text.Json;

namespace Blink.Deploy.Services
{
    public class ConfigService
    {
        private static readonly string ConfigPath = Path.Combine(
            AppContext.BaseDirectory, "blink.config.json");

        public BlinkConfig Load()
        {
            if (!File.Exists(ConfigPath))
                throw new FileNotFoundException($"blink.config.json not found at {ConfigPath}");

            var json = File.ReadAllText(ConfigPath);

            return JsonSerializer.Deserialize<BlinkConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Failed to parse blink.config.json");
        }

        public AppConfig GetApp(string name)
        {
            var config = Load();
            return config.Apps.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                ?? throw new Exception($"App '{name}' not found in blink.config.json");
        }
    }
}
