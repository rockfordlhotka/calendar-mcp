using Spectre.Console;
using Spectre.Console.Cli;
using CalendarMcp.Core.Configuration;
using System.Text.Json;
using System.ComponentModel;

namespace CalendarMcp.Cli.Commands;

/// <summary>
/// Command to list configured accounts
/// </summary>
public class ListAccountsCommand : AsyncCommand<ListAccountsCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Path to appsettings.json (default: %LOCALAPPDATA%/CalendarMcp/appsettings.json)")]
        [CommandOption("--config")]
        public string? ConfigPath { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.Write(new FigletText("Calendar MCP")
            .Centered()
            .Color(Color.Blue));

        AnsiConsole.MarkupLine("[bold]Configured Accounts[/]");
        AnsiConsole.WriteLine();

        // Determine config file path - use shared ConfigurationPaths by default
        var configPath = settings.ConfigPath ?? ConfigurationPaths.GetConfigFilePath();

        if (!File.Exists(configPath))
        {
            AnsiConsole.MarkupLine($"[red]Error: Configuration file not found at {configPath}[/]");
            AnsiConsole.MarkupLine($"[yellow]Default location: {ConfigurationPaths.GetConfigFilePath()}[/]");
            AnsiConsole.MarkupLine("[dim]Run 'add-m365-account' to create the configuration and add an account.[/]");
            return 1;
        }

        AnsiConsole.MarkupLine($"[dim]Using configuration: {configPath}[/]");
        AnsiConsole.WriteLine();

        try
        {
            // Load configuration
            var jsonString = await File.ReadAllTextAsync(configPath);
            var jsonDoc = JsonDocument.Parse(jsonString);

            // Look for CalendarMcp.Accounts (PascalCase) in the config
            if (!jsonDoc.RootElement.TryGetProperty("CalendarMcp", out var calendarMcpElement) ||
                !calendarMcpElement.TryGetProperty("Accounts", out var accountsElement))
            {
                AnsiConsole.MarkupLine("[yellow]No accounts configured.[/]");
                return 0;
            }

            var accounts = accountsElement.Deserialize<List<Dictionary<string, JsonElement>>>();
            
            if (accounts == null || accounts.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No accounts configured.[/]");
                return 0;
            }

            // Create table
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.AddColumn("[bold]ID[/]");
            table.AddColumn("[bold]Display Name[/]");
            table.AddColumn("[bold]Provider[/]");
            table.AddColumn("[bold]Enabled[/]");
            table.AddColumn("[bold]Priority[/]");
            table.AddColumn("[bold]Domains[/]");

            foreach (var account in accounts)
            {
                // Support both PascalCase and camelCase property names for backwards compatibility
                var id = GetStringValue(account, "Id", "id");
                var displayName = GetStringValue(account, "DisplayName", "displayName");
                var provider = GetStringValue(account, "Provider", "provider");
                var enabled = GetBoolValue(account, "Enabled", "enabled");
                var priority = GetIntValue(account, "Priority", "priority");
                
                var domains = "";
                if (TryGetElement(account, out var domainsElem, "Domains", "domains") && 
                    domainsElem.ValueKind == JsonValueKind.Array)
                {
                    var domainList = domainsElem.Deserialize<List<string>>() ?? new List<string>();
                    domains = string.Join(", ", domainList);
                }

                var enabledStr = enabled ? "[green]✓[/]" : "[red]✗[/]";

                table.AddRow(
                    id,
                    displayName,
                    provider,
                    enabledStr,
                    priority.ToString(),
                    domains
                );
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[dim]Total accounts: {accounts.Count}[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            return 1;
        }
    }

    /// <summary>
    /// Try to get a JsonElement by checking multiple property names (for case-insensitive lookup)
    /// </summary>
    private static bool TryGetElement(Dictionary<string, JsonElement> dict, out JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (dict.TryGetValue(name, out element))
                return true;
        }
        element = default;
        return false;
    }

    /// <summary>
    /// Get a string value, checking multiple property names
    /// </summary>
    private static string GetStringValue(Dictionary<string, JsonElement> dict, params string[] names)
    {
        if (TryGetElement(dict, out var elem, names) && elem.ValueKind == JsonValueKind.String)
            return elem.GetString() ?? "";
        return "";
    }

    /// <summary>
    /// Get a bool value, checking multiple property names
    /// </summary>
    private static bool GetBoolValue(Dictionary<string, JsonElement> dict, params string[] names)
    {
        if (TryGetElement(dict, out var elem, names))
        {
            if (elem.ValueKind == JsonValueKind.True) return true;
            if (elem.ValueKind == JsonValueKind.False) return false;
        }
        return false;
    }

    /// <summary>
    /// Get an int value, checking multiple property names
    /// </summary>
    private static int GetIntValue(Dictionary<string, JsonElement> dict, params string[] names)
    {
        if (TryGetElement(dict, out var elem, names) && elem.ValueKind == JsonValueKind.Number)
            return elem.GetInt32();
        return 0;
    }
}
