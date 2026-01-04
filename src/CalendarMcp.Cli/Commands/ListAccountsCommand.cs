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
                var id = account.TryGetValue("Id", out var idElem) ? idElem.GetString() ?? "" : "";
                var displayName = account.TryGetValue("DisplayName", out var nameElem) ? nameElem.GetString() ?? "" : "";
                var provider = account.TryGetValue("Provider", out var provElem) ? provElem.GetString() ?? "" : "";
                var enabled = account.TryGetValue("Enabled", out var enElem) && enElem.GetBoolean();
                var priority = account.TryGetValue("Priority", out var prioElem) ? prioElem.GetInt32() : 0;
                
                var domains = "";
                if (account.TryGetValue("Domains", out var domainsElem) && domainsElem.ValueKind == JsonValueKind.Array)
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
}
