using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace CalendarMcp.Core.Tools;

/// <summary>
/// MCP tool for getting calendar events
/// </summary>
public class GetCalendarEventsTool : McpServerTool
{
    private readonly IAccountRegistry _accountRegistry;
    private readonly IProviderServiceFactory _providerFactory;
    private readonly ILogger<GetCalendarEventsTool> _logger;

    public GetCalendarEventsTool(
        IAccountRegistry accountRegistry,
        IProviderServiceFactory providerFactory,
        ILogger<GetCalendarEventsTool> logger)
    {
        _accountRegistry = accountRegistry;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public override Tool ProtocolTool => new Tool
    {
        Name = "get_calendar_events",
        Description = "Get events (past/present/future) for specific account or all accounts",
        InputSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
            "type": "object",
            "properties": {
                "startDate": {
                    "type": "string",
                    "description": "Start date for event range (ISO 8601 format)",
                    "format": "date-time"
                },
                "endDate": {
                    "type": "string",
                    "description": "End date for event range (ISO 8601 format)",
                    "format": "date-time"
                },
                "accountId": {
                    "type": "string",
                    "description": "Specific account ID, or omit for all accounts"
                },
                "calendarId": {
                    "type": "string",
                    "description": "Specific calendar ID, or omit for all calendars"
                },
                "count": {
                    "type": "integer",
                    "description": "Maximum number of events to retrieve",
                    "default": 50
                }
            },
            "required": ["startDate", "endDate"]
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
            
            // Parse required parameters
            if (args?.TryGetValue("startDate", out var startDateObj) != true || startDateObj == null)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = new List<ContentBlock>
                    {
                        new TextContentBlock
                        {
                            Type = "text",
                            Text = "Parameter 'startDate' is required"
                        }
                    }
                };
            }
            
            if (args.TryGetValue("endDate", out var endDateObj) != true || endDateObj == null)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = new List<ContentBlock>
                    {
                        new TextContentBlock
                        {
                            Type = "text",
                            Text = "Parameter 'endDate' is required"
                        }
                    }
                };
            }
            
            DateTime startDate = DateTime.Parse(startDateObj.ToString()!);
            DateTime endDate = DateTime.Parse(endDateObj.ToString()!);
            
            // Parse optional parameters
            string? accountId = args.TryGetValue("accountId", out var accountIdObj)
                ? accountIdObj?.ToString()
                : null;
            
            string? calendarId = args.TryGetValue("calendarId", out var calendarIdObj)
                ? calendarIdObj?.ToString()
                : null;
            
            int count = args.TryGetValue("count", out var countObj) && countObj != null
                ? Convert.ToInt32(countObj)
                : 50;

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
                    var events = await provider.GetCalendarEventsAsync(
                        account.Id, calendarId, startDate, endDate, count, cancellationToken);
                    return events;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting calendar events from account {AccountId}", account.Id);
                    return Enumerable.Empty<Models.CalendarEvent>();
                }
            });

            var results = await Task.WhenAll(tasks);
            var allEvents = results.SelectMany(e => e)
                .OrderBy(e => e.Start)
                .ToList();

            var result = new
            {
                events = allEvents.Select(e => new
                {
                    id = e.Id,
                    accountId = e.AccountId,
                    calendarId = e.CalendarId,
                    subject = e.Subject,
                    start = e.Start,
                    end = e.End,
                    location = e.Location,
                    attendees = e.Attendees,
                    isAllDay = e.IsAllDay,
                    organizer = e.Organizer
                }).ToList()
            };

            _logger.LogInformation("Retrieved {Count} events from {AccountCount} accounts between {Start} and {End}",
                result.events.Count, accounts.Count, startDate, endDate);
            
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
            _logger.LogError(ex, "Error in get_calendar_events tool");
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
