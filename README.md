# ⚡ blink-deploy

> Deploy your .NET apps on-premise — in the blink of an eye.

`blink` is a lightweight, self-contained Windows CLI tool for deploying .NET applications on air-gapped, on-premise servers. No internet required, no runtime needed — just a single `blink.exe`.

---

## ✨ Features

- 🗂 Manage multiple apps from a single config file
- 📦 Prepare deployments without touching the running service
- 🔁 Swap new version into production with minimal downtime
- ⏪ Rollback to previous version instantly
- 💾 Automatic timestamped backups before every deployment
- 🔒 Preserves production config files (`appsettings.*.json`, `web.config`)
- 🖥 Supports both **IIS App Pools** and **Windows Services**
- 📋 Audit log of every operation
- 🚀 Single self-contained `blink.exe` — no .NET runtime required on server

---

## 📦 Installation

1. Download `blink.exe` from [Releases](https://github.com/BasharAbuJaab/blink-deploy/releases)
2. Place it in a folder on your server (e.g. `C:\tools\blink\`)
3. Run as Administrator:

```
blink install
```

This adds `blink` to your system PATH so you can run it from anywhere.

---

## ⚙️ Configuration

Run the interactive setup to add your first app:

```
blink add
```

This creates/updates `blink.config.json` next to `blink.exe`:

```json
{
  "apps": [
    {
      "name": "myapp",
      "path": "C:\\inetpub\\myapp",
      "serviceType": "IIS",
      "serviceName": "myapp-pool",
      "preserveFiles": ["appsettings.json", "appsettings.*.json", "web.config"]
    }
  ]
}
```

| Field           | Description                                                    |
| --------------- | -------------------------------------------------------------- |
| `name`          | App identifier used in commands                                |
| `path`          | Path to the currently running app folder                       |
| `serviceType`   | `IIS`, `WindowsService`, or `None`                             |
| `serviceName`   | App pool name (IIS) or service name (Windows Service)          |
| `preserveFiles` | Files to never overwrite from source (glob patterns supported) |

---

## 🚀 Usage

### Typical deployment flow

```bash
# 1. RDP into server, copy published files to a folder
# 2. Prepare the next version (backs up current, copies new files)
blink prepare myapp --source "C:\deploys\myapp"

# 3. Verify everything looks good in the -next folder
# 4. Swap into production (stops service, renames folders, starts service)
blink swap myapp

# 5. If something goes wrong, rollback instantly
blink rollback myapp
```

---

## 📖 Commands

| Command                | Alias | Description                                          |
| ---------------------- | ----- | ---------------------------------------------------- |
| `blink install`        |       | Add blink to system PATH (run once as Admin)         |
| `blink add`            | `a`   | Add or update an app in `blink.config.json`          |
| `blink prepare [app]`  | `p`   | Backup current, copy new files to `-next` folder     |
| `blink swap [app]`     | `s`   | Stop service, swap `-next` to current, start service |
| `blink rollback [app]` | `r`   | Swap `-prev` back to current                         |
| `blink status [app]`   |       | Show folder and service status                       |

> All commands prompt for app selection if `[app]` is omitted.

---

## 📁 Folder Structure

After a full deployment cycle, your server will look like this:

```
C:\inetpub\
├── myapp\              ← currently running
├── myapp-next\         ← staged (ready to swap)
├── myapp-prev\         ← previous version (for rollback)
└── Backup\
    ├── myapp_2024-01-15_10-30-00.zip
    └── myapp_2024-01-16_09-00-00.zip
```

---

## 📋 Audit Log

Every operation is logged to `blink.audit.log` next to `blink.exe`:

```
[2024-01-15 10:30:00] [PREPARE] myapp - Started
[2024-01-15 10:30:01] [BACKUP] myapp - Taking backup to C:\inetpub\Backup\myapp_2024-01-15_10-30-00.zip
[2024-01-15 10:30:05] [PREPARE] myapp - Prepare complete.
[2024-01-15 10:31:00] [SWAP] myapp - Started
[2024-01-15 10:31:02] [SWAP] myapp - Swap complete.
```

---

## 🏗 Building from Source

Requirements: .NET 10 SDK, Windows x64

```bash
git clone https://github.com/BasharAbuJaab/blink-deploy.git
cd blink-deploy
dotnet publish -c Release
```

Output: `bin\Release\net10.0-windows\win-x64\publish\blink.exe`

---

## 📄 License

MIT
