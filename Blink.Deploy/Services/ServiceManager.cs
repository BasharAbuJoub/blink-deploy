using System.ServiceProcess;
using Blink.Deploy.Models;
using Spectre.Console;

namespace Blink.Deploy.Services;

public class ServiceManager
{
    public void Restart(AppConfig app)
    {
        if (string.IsNullOrWhiteSpace(app.ServiceType))
        {
            AnsiConsole.MarkupLine("[yellow]No service configured, skipping restart.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[cyan]Restarting service...[/]");
        Stop(app);
        Start(app);
        AnsiConsole.MarkupLine("[green1]Service restarted.[/]");
    }

    public void Stop(AppConfig app)
    {
        if (string.IsNullOrWhiteSpace(app.ServiceType))
        {
            AnsiConsole.MarkupLine("[yellow]No service configured, skipping stop.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[cyan]Stopping service...[/]");

        if (app.ServiceType.Equals("IIS", StringComparison.OrdinalIgnoreCase))
            StopIIS(app.ServiceName);
        else
            StopWindowsService(app.ServiceName);

        AnsiConsole.MarkupLine("[green1]Service stopped.[/]");
    }

    public void Start(AppConfig app)
    {
        if (string.IsNullOrWhiteSpace(app.ServiceType))
        {
            AnsiConsole.MarkupLine("[yellow]No service configured, skipping start.[/]");
            return;
        }

        AnsiConsole.MarkupLine("[cyan]Starting service...[/]");

        if (app.ServiceType.Equals("IIS", StringComparison.OrdinalIgnoreCase))
            StartIIS(app.ServiceName);
        else
            StartWindowsService(app.ServiceName);

        AnsiConsole.MarkupLine("[green1]Service started.[/]");
    }

    private void StopIIS(string appPoolName)
    {
        var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = @"C:\Windows\System32\inetsrv\appcmd.exe",
            Arguments = $"stop apppool /apppool.name:\"{appPoolName}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
        process?.WaitForExit();
    }

    private void StartIIS(string appPoolName)
    {
        var process = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = @"C:\Windows\System32\inetsrv\appcmd.exe",
            Arguments = $"start apppool /apppool.name:\"{appPoolName}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
        process?.WaitForExit();
    }

    private void StopWindowsService(string serviceName)
    {
        using var service = new ServiceController(serviceName);
        if (service.Status == ServiceControllerStatus.Running)
        {
            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
        }
    }

    private void StartWindowsService(string serviceName)
    {
        using var service = new ServiceController(serviceName);
        if (service.Status == ServiceControllerStatus.Stopped)
        {
            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
        }
    }
}