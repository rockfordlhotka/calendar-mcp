# Configuration Path Update

**Date**: January 3, 2026

## Summary

Updated the MCP server and CLI tools to use a consistent configuration location in the user's local application data directory. This ensures the server works correctly when launched from any working directory (e.g., when launched by Claude Desktop).

## Problem

When Claude Desktop launches the MCP server, the working directory is Claude Desktop's directory, not the server's directory. The default `Host.CreateDefaultBuilder` looks for `appsettings.json` in the current working directory, which caused the server to fail to find its configuration.

## Solution

All user-specific data is now stored in a consistent location:

### Windows
```
%LOCALAPPDATA%\CalendarMcp\
├── appsettings.json          # Main configuration file
├── msal_cache_*.bin          # M365/Outlook.com token caches
└── logs\                     # Server logs
```

### Linux/macOS
```
~/.local/share/CalendarMcp/
├── appsettings.json
└── logs/
```

This location was chosen because:
1. It's the same location already used for MSAL token caches
2. It's a standard location for user-specific application data
3. It's accessible regardless of working directory

## Configuration Loading Priority

1. **Environment Variable**: `CALENDAR_MCP_CONFIG` - override the config directory or file path
2. **User Data Directory**: `%LOCALAPPDATA%\CalendarMcp\appsettings.json` (default)
3. **Application Directory**: Fallback for development scenarios

## Changes Made

### New Files
- `src/CalendarMcp.Core/Configuration/ConfigurationPaths.cs` - Shared helper for consistent path resolution

### Modified Files
- `src/CalendarMcp.StdioServer/Program.cs` - Uses ConfigurationPaths for config loading
- `src/CalendarMcp.StdioServer/README.md` - Updated documentation
- `src/CalendarMcp.Cli/Commands/AddM365AccountCommand.cs` - Uses ConfigurationPaths, auto-creates config
- `src/CalendarMcp.Cli/Commands/ListAccountsCommand.cs` - Uses ConfigurationPaths
- `src/CalendarMcp.Cli/Commands/TestAccountCommand.cs` - Uses ConfigurationPaths
- `docs/configuration.md` - Updated with new location documentation

## Usage

### CLI Tool
The CLI tool now automatically creates the configuration file if it doesn't exist:

```bash
# Adds account and creates %LOCALAPPDATA%\CalendarMcp\appsettings.json if needed
calendar-mcp-cli add-m365-account

# Override location if needed
calendar-mcp-cli add-m365-account --config C:\MyConfig\appsettings.json
```

### Environment Variable Override
```bash
# Point to custom directory
set CALENDAR_MCP_CONFIG=C:\MyConfig\CalendarMcp

# Point to specific file
set CALENDAR_MCP_CONFIG=C:\MyConfig\custom-config.json
```

## Migration

If you have an existing `appsettings.json` in your application directory, either:

1. Copy it to `%LOCALAPPDATA%\CalendarMcp\appsettings.json`
2. Set `CALENDAR_MCP_CONFIG` to point to the existing location

The server will log which configuration file it's loading at startup.
