namespace Blink.Deploy.Services
{
    public class LogService
    {
        private static readonly string LogPath = Path.Combine(
            AppContext.BaseDirectory, "blink.audit.log");

        public void Log(string command, string app, string message)
        {
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{command.ToUpper()}] {app} - {message}";
            File.AppendAllText(LogPath, entry + Environment.NewLine);
        }

        public void Info(string command, string app, string message) =>
            Log(command, app, message);

        public void Error(string command, string app, string message) =>
            Log(command, app, $"ERROR: {message}");
    }
}