using Blink.Deploy.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Blink.Deploy.Commands
{
    public class CleanCommand : Command<CleanCommand.Settings>
    {
        private readonly ConfigService _configService;
        private readonly LogService _logService;

        public CleanCommand()
        {
            _configService = new ConfigService();
            _logService = new LogService();
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[app]")]
            [Description("The app name defined in blink.config.json")]
            public string AppName { get; set; } = string.Empty;
        }

        protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            var config = _configService.Load();

            var appName = settings.AppName;
            if (string.IsNullOrWhiteSpace(appName))
            {
                appName = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select an app:")
                        .AddChoices(config.Apps.Select(a => a.Name)));
            }

            var app = _configService.GetApp(appName);
            var prevPath = app.Path + "-prev";

            if (!Directory.Exists(prevPath))
            {
                AnsiConsole.MarkupLine("[yellow]No prev folder found, nothing to clean.[/]");
                return 0;
            }

            var confirm = AnsiConsole.Confirm($"Delete [yellow]{prevPath}[/]?", false);
            if (!confirm) return 0;

            _logService.Info("clean", app.Name, $"Deleting prev: {prevPath}");
            Directory.Delete(prevPath, recursive: true);
            _logService.Info("clean", app.Name, "Clean complete.");
            AnsiConsole.MarkupLine("[green1]Prev folder deleted.[/]");

            return 0;
        }
    }
}