using System.ComponentModel;
using System.Text.Json;
using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace CalendarMcp.Core.Tools;

/// <summary>
/// MCP tool for listing all configured accounts
/// </summary>
[McpServerToolType]
public sealed class ListAccountsTool(
    IAccountRegistry accountRegistry,
    ILogger<ListAccountsTool> logger)
{
    [McpServerTool, Description("Get list of all configured accounts across all providers")]
    public async Task<string> ListAccounts()
    {
        logger.LogInformation("Listing all accounts");

        try
        {
            var accounts = await accountRegistry.GetAllAccountsAsync();

            var response = new
            {
                accounts = accounts.Select(a => new
                {
                    accountId = a.Id,
                    provider = a.Provider,
                    displayName = a.DisplayName,
                    domains = a.Domains
                })
            };

            return JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing accounts");
            return JsonSerializer.Serialize(new
            {
                error = "Failed to list accounts",
                message = ex.Message
            });
        }
    }
}
