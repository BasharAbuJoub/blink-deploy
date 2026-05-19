using Blink.Deploy.Models;
using Microsoft.Extensions.FileSystemGlobbing;
using Spectre.Console;

namespace Blink.Deploy.Services
{
    public class FileService
    {
        private readonly LogService _logService;

        public FileService(LogService logService)
        {
            _logService = logService;
        }

        public void Prepare(AppConfig app, string sourcePath)
        {
            var nextPath = app.Path + "-next";

            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException($"Source path not found: {sourcePath}");

            if (!Directory.Exists(app.Path))
                throw new DirectoryNotFoundException($"App path not found: {app.Path}");

            _logService.Info("prepare", app.Name, $"Copying current to next: {nextPath}");
            AnsiConsole.MarkupLine("[cyan]Copying current to next...[/]");
            CopyDirectory(app.Path, nextPath, overwrite: true);

            _logService.Info("prepare", app.Name, $"Copying source files from: {sourcePath}");
            AnsiConsole.MarkupLine("[cyan]Copying new files to next...[/]");
            CopyDirectory(sourcePath, nextPath, overwrite: true, preserve: app.PreserveFiles);

            _logService.Info("prepare", app.Name, "Prepare complete.");
            AnsiConsole.MarkupLine("[green1]Prepare complete.[/]");
        }

        public void Swap(AppConfig app)
        {
            var nextPath = app.Path + "-next";
            var prevPath = app.Path + "-prev";

            if (!Directory.Exists(nextPath))
            {
                _logService.Error("swap", app.Name, $"Next folder not found: {nextPath}");
                throw new DirectoryNotFoundException($"Next folder not found: {nextPath}. Run prepare first.");
            }

            if (Directory.Exists(prevPath))
            {
                _logService.Info("swap", app.Name, $"Deleting old prev: {prevPath}");
                AnsiConsole.MarkupLine("[cyan]Deleting old prev...[/]");
                Directory.Delete(prevPath, recursive: true);
            }

            _logService.Info("swap", app.Name, $"Renaming current to prev: {prevPath}");
            AnsiConsole.MarkupLine("[cyan]Renaming current to prev...[/]");
            Directory.Move(app.Path, prevPath);

            _logService.Info("swap", app.Name, $"Renaming next to current: {app.Path}");
            AnsiConsole.MarkupLine("[cyan]Renaming next to current...[/]");
            Directory.Move(nextPath, app.Path);

            _logService.Info("swap", app.Name, "Swap complete.");
            AnsiConsole.MarkupLine("[green1]Swap complete.[/]");
        }

        public void Rollback(AppConfig app)
        {
            var nextPath = app.Path + "-next";
            var prevPath = app.Path + "-prev";

            if (!Directory.Exists(prevPath))
                throw new DirectoryNotFoundException($"Prev folder not found: {prevPath}. Nothing to rollback to.");

            if (Directory.Exists(nextPath))
            {
                _logService.Info("rollback", app.Name, $"Deleting next: {nextPath}");
                AnsiConsole.MarkupLine("[cyan]Deleting next...[/]");
                Directory.Delete(nextPath, recursive: true);
            }

            _logService.Info("rollback", app.Name, $"Renaming current to next: {nextPath}");
            AnsiConsole.MarkupLine("[cyan]Renaming current to next...[/]");
            Directory.Move(app.Path, nextPath);

            _logService.Info("rollback", app.Name, $"Renaming prev to current: {app.Path}");
            AnsiConsole.MarkupLine("[cyan]Renaming prev to current...[/]");
            Directory.Move(prevPath, app.Path);

            _logService.Info("rollback", app.Name, "Rollback complete.");
            AnsiConsole.MarkupLine("[green1]Rollback complete.[/]");
        }

        public void Backup(AppConfig app)
        {
            if (!Directory.Exists(app.Path))
            {
                _logService.Info("backup", app.Name, $"App folder not found, skipping backup.");
                AnsiConsole.MarkupLine("[yellow]App folder not found, skipping backup.[/]");
                return;
            }

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupRoot = Path.Combine(Path.GetDirectoryName(app.Path)!, "Backup");
            Directory.CreateDirectory(backupRoot);
            var backupPath = Path.Combine(backupRoot, $"{app.Name}_{timestamp}.zip");

            _logService.Info("backup", app.Name, $"Taking backup to {backupPath}");
            AnsiConsole.MarkupLine("[cyan]Taking backup...[/]");
            System.IO.Compression.ZipFile.CreateFromDirectory(app.Path, backupPath);
            _logService.Info("backup", app.Name, "Backup complete.");
            AnsiConsole.MarkupLine($"[green1]Backup saved to: {backupPath}[/]");
        }

        private void CopyDirectory(string source, string dest, bool overwrite, List<string>? preserve = null)
        {
            Directory.CreateDirectory(dest);

            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(source, file);

                if (preserve != null && IsPreserved(relativePath, preserve))
                {
                    AnsiConsole.MarkupLine($"[yellow]Preserving: {relativePath}[/]");
                    continue;
                }

                var destFile = Path.Combine(dest, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                File.Copy(file, destFile, overwrite);
            }
        }

        private bool IsPreserved(string relativePath, List<string> patterns)
        {
            var fileName = Path.GetFileName(relativePath);
            return patterns.Any(pattern =>
            {
                // Check if pattern matches a directory prefix
                if (relativePath.StartsWith(pattern.TrimEnd('\\', '/'),
                    StringComparison.OrdinalIgnoreCase))
                    return true;

                // Check file name glob pattern
                var matcher = new Matcher();
                matcher.AddInclude(pattern);
                return matcher.Match(fileName).HasMatches;
            });
        }
    }
}
