using Blink.Deploy.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Blink.Deploy.Commands
{
    public class PrepareCommand : Command<PrepareCommand.Settings>
    {
        private readonly ConfigService _configService;
        private readonly FileService _fileService;
        private readonly LogService _logService;
        private readonly StateService _stateService;

        public PrepareCommand()
        {
            _configService = new ConfigService();
            _logService = new LogService();
            _fileService = new FileService(_logService);
            _stateService = new StateService();
        }

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[app]")]
            [Description("The app name defined in blink.config.json")]
            public string AppName { get; set; } = string.Empty;

            [CommandOption("-s|--source")]
            [Description("Path to the published files that will be prepared")]
            public string Source { get; set; } = string.Empty;
        }

        protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            var source = string.IsNullOrWhiteSpace(settings.Source)
                ? AnsiConsole.Prompt(
                    new TextPrompt<string>("Source path:")
                        .DefaultValue(Directory.GetCurrentDirectory()))
                : settings.Source;

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
            var absoluteSource = Path.GetFullPath(source);
            AnsiConsole.Write(new Rule("[blue]Confirm Prepare[/]"));
            AnsiConsole.MarkupLine($"  App     : [green]{app.Name}[/]");
            AnsiConsole.MarkupLine($"  Source  : [yellow]{absoluteSource}[/]");
            AnsiConsole.MarkupLine($"  Target  : [yellow]{app.Path}[/]");
            AnsiConsole.MarkupLine($"  Service Type : [yellow]{app.ServiceType ?? "None"}[/]");
            AnsiConsole.MarkupLine($"  Service Name : [yellow]{app.ServiceName ?? "None"}[/]");

            var confirm = AnsiConsole.Confirm("Proceed?", false);
            if (!confirm) return 0;

            _logService.Info("prepare", app.Name, "Started");
            _fileService.Backup(app);
            _fileService.Prepare(app, absoluteSource);
            _stateService.SetLastPrepare(app.Name);
            return 0;
        }
    }
}
