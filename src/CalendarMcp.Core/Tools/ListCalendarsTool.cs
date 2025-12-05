using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace CalendarMcp.Core.Tools;

/// <summary>
/// MCP tool for listing calendars
/// </summary>
public class ListCalendarsTool : McpServerTool
{
    private readonly IAccountRegistry _accountRegistry;
    private readonly IProviderServiceFactory _providerFactory;
    private readonly ILogger<ListCalendarsTool> _logger;

    public ListCalendarsTool(
        IAccountRegistry accountRegistry,
        IProviderServiceFactory providerFactory,
        ILogger<ListCalendarsTool> logger)
    {
        _accountRegistry = accountRegistry;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public override Tool ProtocolTool => new Tool
    {
        Name = "list_calendars",
        Description = "List all calendars from specific account or all accounts",
        InputSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
            "type": "object",
            "properties": {
                "accountId": {
                    "type": "string",
                    "description": "Specific account ID, or omit for all accounts"
                }
            }
        }
        """)
    };

    public override IReadOnlyList<object> Metadata => Array.Empty<object>();

    public override async ValueTask<CallToolResult> InvokeAsync(
        RequestContext<CallToolRequestParams> request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var args = request.Params?.Arguments;
            
            // Parse optional parameters
            string? accountId = args?.TryGetValue("accountId", out var accountIdObj) == true
                ? accountIdObj?.ToString()
                : null;

            // Determine which accounts to query
            var accounts = string.IsNullOrEmpty(accountId)
                ? _accountRegistry.GetEnabledAccounts().ToList()
                : new[] { _accountRegistry.GetAccount(accountId) }.Where(a => a != null).ToList()!;

            if (accounts.Count == 0)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = new List<ContentBlock>
                    {
                        new TextContentBlock
                        {
                            Type = "text",
                            Text = accountId != null
                                ? $"Account '{accountId}' not found"
                                : "No enabled accounts found"
                        }
                    }
                };
            }

            // Query all accounts in parallel
            var tasks = accounts.Select(async account =>
            {
                try
                {
                    var provider = _providerFactory.GetProvider(account.Provider);
                    var calendars = await provider.ListCalendarsAsync(account.Id, cancellationToken);
                    return calendars;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error listing calendars from account {AccountId}", account.Id);
                    return Enumerable.Empty<Models.CalendarInfo>();
                }
            });

            var results = await Task.WhenAll(tasks);
            var allCalendars = results.SelectMany(c => c).ToList();

            var result = new
            {
                calendars = allCalendars.Select(c => new
                {
                    id = c.Id,
                    accountId = c.AccountId,
                    name = c.Name,
                    owner = c.Owner,
                    canEdit = c.CanEdit,
                    isDefault = c.IsDefault
                }).ToList()
            };

            _logger.LogInformation("Retrieved {Count} calendars from {AccountCount} accounts",
                result.calendars.Count, accounts.Count);
            
            return new CallToolResult
            {
                Content = new List<ContentBlock>
                {
                    new TextContentBlock
                    {
                        Type = "text",
                        Text = JsonSerializer.Serialize(result)
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in list_calendars tool");
            return new CallToolResult
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
            };
        }
    }
}
