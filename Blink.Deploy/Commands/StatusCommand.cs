using Blink.Deploy.Models;
using Blink.Deploy.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Blink.Deploy.Commands
{
    public class StatusCommand : Command<StatusCommand.Settings>
    {
        private readonly ConfigService _configService;
        private readonly StateService _stateService;

        public StatusCommand()
        {
            _configService = new ConfigService();
            _stateService = new StateService();
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[app]")]
            [Description("The app name defined in blink.config.json")]
            public string AppName { get; set; } = string.Empty;

            [CommandOption("-a|--all")]
            [Description("Show status for all configured apps")]
            public bool All { get; set; }
        }

        protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            var config = _configService.Load();

            if (settings.All)
            {
                WriteAllAppsStatus(config.Apps);
                return 0;
            }

            var appName = settings.AppName;
            if (string.IsNullOrWhiteSpace(appName))
            {
                appName = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an app:")
                        .AddChoices(config.Apps.Select(a => a.Name)));
            }

            var app = _configService.GetApp(appName);
            WriteSingleAppStatus(app);

            return 0;
        }

        private void WriteSingleAppStatus(AppConfig app)
        {
            var appState = _stateService.GetState(app.Name);
            var nextPath = app.Path + "-next";
            var prevPath = app.Path + "-prev";

            var backupCount = GetBackupCount(app);

            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Property")
                .AddColumn("Value");

            table.AddRow("App", $"[green1]{app.Name}[/]");
            table.AddRow("Current", Exists(app.Path));
            table.AddRow("Next", Exists(nextPath));
            table.AddRow("Prev", Exists(prevPath));
            table.AddRow("Backups", $"[cyan]{backupCount}[/]");

            if (!string.IsNullOrWhiteSpace(app.ServiceType))
            {
                table.AddRow("Service Type", $"[yellow]{app.ServiceType}[/]");
                table.AddRow("Service Name", $"[yellow]{app.ServiceName}[/]");
                table.AddRow("Service Status", GetServiceStatus(app));
            }

            table.AddRow("Last Prepare", appState?.LastPrepare ?? "[grey]Never[/]");
            table.AddRow("Last Swap", appState?.LastSwap ?? "[grey]Never[/]");
            table.AddRow("Last Rollback", appState?.LastRollback ?? "[grey]Never[/]");

            AnsiConsole.Write(table);
        }

        private void WriteAllAppsStatus(IEnumerable<AppConfig> apps)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("App")
                .AddColumn("Service Status")
                .AddColumn("Last Prepare")
                .AddColumn("Last Swap")
                .AddColumn("Last Rollback")
                .AddColumn("Service Type")
                .AddColumn("Service Name")
                .AddColumn("Backups");

            foreach (var app in apps)
            {
                var appState = _stateService.GetState(app.Name);

                table.AddRow(
                    $"[green1]{app.Name}[/]",
                    string.IsNullOrWhiteSpace(app.ServiceType) ? "[grey]N/A[/]" : GetServiceStatus(app),
                    appState?.LastPrepare ?? "[grey]Never[/]",
                    appState?.LastSwap ?? "[grey]Never[/]",
                    appState?.LastRollback ?? "[grey]Never[/]",
                    $"[yellow]{app.ServiceType ?? "None"}[/]",
                    $"[yellow]{app.ServiceName ?? "None"}[/]",
                    $"[cyan]{GetBackupCount(app)}[/]");
            }

            AnsiConsole.Write(table);
        }

        private string Exists(string path) =>
            Directory.Exists(path) ? "[green1]Exists[/]" : "[red]Not found[/]";

        private int GetBackupCount(AppConfig app)
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(app.Path)!, "Backup");

            return Directory.Exists(backupPath)
                ? Directory.GetFiles(backupPath, $"{app.Name}_*.zip").Length
                : 0;
        }

        private string GetServiceStatus(AppConfig app)
        {
            try
            {
                if (app.ServiceType!.Equals("IIS", StringComparison.OrdinalIgnoreCase))
                {
                    var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = @"C:\Windows\System32\inetsrv\appcmd.exe",
                        Arguments = $"list apppool /apppool.name:\"{app.ServiceName}\" /state:*",
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    });
                    var output = process?.StandardOutput.ReadToEnd() ?? string.Empty;
                    process?.WaitForExit();
                    return output.Contains("Started")
                        ? "[green1]Running[/]"
                        : "[red]Stopped[/]";
                }
                else
                {
                    using var service = new System.ServiceProcess.ServiceController(app.ServiceName!);
                    return service.Status == System.ServiceProcess.ServiceControllerStatus.Running
                        ? "[green1]Running[/]"
                        : "[red]Stopped[/]";
                }
            }
            catch
            {
                return "[yellow]Unknown[/]";
            }
        }
    }
}