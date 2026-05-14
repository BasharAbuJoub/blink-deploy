using System.Text.Json;

namespace Blink.Deploy.Services
{
    public class AppState
    {
        public string? LastPrepare { get; set; }

        public string? LastSwap { get; set; }
        
        public string? LastRollback { get; set; }
    }

    public class BlinkState
    {
        public Dictionary<string, AppState> Apps { get; set; } = new();
    }

    public class StateService
    {
        private static readonly string StatePath = Path.Combine(
            AppContext.BaseDirectory, "blink.state.json");

        private BlinkState Load()
        {
            if (!File.Exists(StatePath))
                return new BlinkState();

            var json = File.ReadAllText(StatePath);
            return JsonSerializer.Deserialize<BlinkState>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new BlinkState();
        }

        private void Save(BlinkState state)
        {
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            File.WriteAllText(StatePath, json);
        }

        public void SetLastPrepare(string appName)
        {
            var state = Load();
            
            if (!state.Apps.ContainsKey(appName))
                state.Apps[appName] = new AppState();
            
            state.Apps[appName].LastPrepare = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            Save(state);
        }

        public void SetLastSwap(string appName)
        {
            var state = Load();

            if (!state.Apps.ContainsKey(appName))
                state.Apps[appName] = new AppState();
            
            state.Apps[appName].LastSwap = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            Save(state);
        }

        public void SetLastRollback(string appName)
        {
            var state = Load();

            if (!state.Apps.ContainsKey(appName))
                state.Apps[appName] = new AppState();
            
            state.Apps[appName].LastRollback = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            Save(state);
        }

        public AppState? GetState(string appName)
        {
            var state = Load();

            return state.Apps.TryGetValue(appName, out var appState) ? appState : null;
        }
    }
}