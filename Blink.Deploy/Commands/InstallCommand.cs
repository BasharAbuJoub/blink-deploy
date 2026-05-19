using Spectre.Console;
using Spectre.Console.Cli;

namespace Blink.Deploy.Commands
{
    public class InstallCommand : Command<InstallCommand.Settings>
    {
        public class Settings : CommandSettings { }

        protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            var exeDir = AppContext.BaseDirectory;
            var currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) ?? string.Empty;

            if (currentPath.Contains(exeDir))
            {
                AnsiConsole.MarkupLine("[yellow]blink is already in system PATH.[/]");
                return 0;
            }

            try
            {
                Environment.SetEnvironmentVariable("PATH",
                    currentPath + ";" + exeDir,
                    EnvironmentVariableTarget.Machine);

                AnsiConsole.MarkupLine("[green1]blink added to system PATH.[/]");
                AnsiConsole.MarkupLine("[yellow]Restart your terminal for changes to take effect.[/]");
            }
            catch (UnauthorizedAccessException)
            {
                AnsiConsole.MarkupLine("[red]Access denied. Run blink as Administrator.[/]");
                return 1;
            }

            return 0;
        }
    }
}