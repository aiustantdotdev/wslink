# wslink

**Windows Subsystem Link port bridge.** Zero-config, single-file executable. No dependencies. No installation.

## Problem

WSL2 has its own virtual network. Services running on `localhost` inside WSL are not accessible from Windows apps. You need `netsh interface portproxy` + admin privileges + WSL IP detection. Every reboot changes the IP. It's manual and fragile.

## Solution

`wslink.exe` is a native Windows (C#) executable that:

1. Detects running WSL distros
2. Gets their IP addresses
3. Sets up `netsh interface portproxy` forwarding rules
4. Adds Windows Firewall rules (optional)
5. Cleans up everything on exit

## Installation

Install `wslink` with a single command in **PowerShell (Windows)**:

```powershell
irm https://raw.githubusercontent.com/memlinkdotdev/wslink/main/install.ps1 | iex
```

## Usage

```cmd
# Forward WSL port 4444 → Windows localhost:4444
wslink forward 4444

# Forward with specific distro
wslink forward 4444 --distro Ubuntu

# Remove a forwarding rule
wslink remove 4444

# List active rules
wslink list

# Install as background service
wslink service install

# Remove service
wslink service remove
```

## Requirements

| Requirement | Details                                           |
| ----------- | ------------------------------------------------- |
| OS          | Windows 10/11 with WSL2                           |
| Runtime     | Self-contained. No runtime needed.                |
| Elevation   | Admin rights for `netsh` + firewall (prompts UAC) |

## Integration

If you use `memlink` (Universal Memory for AI Agents), you can use `wslink` alongside it to allow your Windows host agents to connect to the memory server running inside WSL:

1. Start your `memlink` server inside WSL:
   ```bash
   memlink serve --daemon
   ```
2. Bridge the port to Windows using `wslink` in Windows PowerShell:
   ```powershell
   wslink forward 4444
   ```

## Architecture

```
wslink.exe (C#, Native AOT)
  ├── WSL detection (wsl.exe --list --running)
  ├── IP resolution (wsl.exe -d <distro> -- hostname -I)
  ├── netsh portproxy management
  ├── Windows Firewall management
  └── Cleanup on exit / service mode

No dependencies. Single file. ~1MB.
```
