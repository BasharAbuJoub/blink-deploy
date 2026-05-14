using Blink.Deploy.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.AddCommand<AddApplicationCommand>("add")
        .WithAlias("a")
        .WithDescription("Adds a new app to blink.config.json.");

    config.AddCommand<PrepareCommand>("prepare")
        .WithAlias("p")
        .WithDescription("Prepares the next version for deployment.");

    config.AddCommand<SwapCommand>("swap")
        .WithAlias("s")
        .WithDescription("Swaps the next version into production.");

    config.AddCommand<RollbackCommand>("rollback")
        .WithAlias("r")
        .WithDescription("Rolls back to the previous version.");

    config.AddCommand<StatusCommand>("status")
        .WithDescription("Shows the current status of an app.");

    config.AddCommand<InstallCommand>("install")
        .WithDescription("Adds blink to system PATH.");
});

return app.Run(args);