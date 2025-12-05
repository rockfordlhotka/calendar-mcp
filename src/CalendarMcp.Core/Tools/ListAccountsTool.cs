using CalendarMcp.Core.Models;
using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace CalendarMcp.Core.Tools;

/// <summary>
/// MCP tool for listing all configured accounts
/// </summary>
public class ListAccountsTool : McpServerTool
{
    private readonly IAccountRegistry _accountRegistry;
    private readonly ILogger<ListAccountsTool> _logger;

    public ListAccountsTool(IAccountRegistry accountRegistry, ILogger<ListAccountsTool> logger)
    {
        _accountRegistry = accountRegistry;
        _logger = logger;
    }

    public override Tool ProtocolTool => new Tool
    {
        Name = "list_accounts",
        Description = "Get list of all configured accounts across all providers",
        InputSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
            "type": "object",
            "properties": {}
        }
        """)
    };

    public override IReadOnlyList<object> Metadata => Array.Empty<object>();

    public override ValueTask<CallToolResult> InvokeAsync(
        RequestContext<CallToolRequestParams> request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = _accountRegistry.GetAllAccounts();
            
            var result = new
            {
                accounts = accounts.Select(a => new
                {
                    id = a.Id,
                    displayName = a.DisplayName,
                    provider = a.Provider,
                    domains = a.Domains,
                    enabled = a.Enabled
                }).ToList()
            };

            _logger.LogInformation("Listed {Count} accounts", result.accounts.Count);
            
            return new ValueTask<CallToolResult>(new CallToolResult
            {
                Content = new List<ContentBlock>
                {
                    new TextContentBlock
                    {
                        Type = "text",
                        Text = JsonSerializer.Serialize(result)
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing accounts");
            return new ValueTask<CallToolResult>(new CallToolResult
            {
                IsError = true,
                Content = new List<ContentBlock>
                {
                    new TextContentBlock
                    {
                        Type = "text",
                        Text = $"Error: {ex.Message}"
                    }
                }
            });
        }
    }
}
