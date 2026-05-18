using Blink.Deploy.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Blink.Deploy.Commands
{
    public class RestartCommand : Command<RestartCommand.Settings>
    {
        private readonly ConfigService _configService;
        private readonly ServiceManager _serviceManager;

        public RestartCommand()
        {
            _configService = new ConfigService();
            _serviceManager = new ServiceManager();
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
            _serviceManager.Restart(app);

            return 0;
        }
    }
}
