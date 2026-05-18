using Blink.Deploy.Models;
using Blink.Deploy.Services;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Text.Json;

namespace Blink.Deploy.Commands
{
    public class AddApplicationCommand : Command<AddApplicationCommand.Settings>
    {
        public class Settings : CommandSettings { }

        protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "blink.config.json");

            BlinkConfig config;
            if (File.Exists(configPath))
            {
                var existing = File.ReadAllText(configPath);
                config = JsonSerializer.Deserialize<BlinkConfig>(existing, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new BlinkConfig();
            }
            else
            {
                config = new BlinkConfig();
            }

            AnsiConsole.Write(new Rule("[cyan]New App[/]"));

            var app = new AppConfig
            {
                Name = AnsiConsole.Ask<string>("App name:"),
                Path = AnsiConsole.Ask<string>("App path:"),
                ServiceType = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Service type:")
                        .AddChoices("None", "IIS", "WindowsService")),
            };

            if (app.ServiceType == "None")
            {
                app.ServiceType = null;
                app.ServiceName = null;
            }
            else
            {
                app.ServiceName = AnsiConsole.Ask<string>("Service/App pool name:");
            }

            var preserveInput = AnsiConsole.Ask<string>(
                "Preserve files (comma separated):", "appsettings.json,appsettings.*.json,web.config");

            app.PreserveFiles = preserveInput
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();

            var existing_app = config.Apps.FirstOrDefault(a =>
                a.Name.Equals(app.Name, StringComparison.OrdinalIgnoreCase));

            if (existing_app != null)
            {
                var overwrite = AnsiConsole.Confirm($"[yellow]App '{app.Name}' already exists. Overwrite?[/]", false);
                if (!overwrite) return 0;
                config.Apps.Remove(existing_app);
            }

            config.Apps.Add(app);

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(configPath, json);
            AnsiConsole.MarkupLine($"[green]App '{app.Name}' saved to blink.config.json.[/]");

            return 0;
        }
    }
}