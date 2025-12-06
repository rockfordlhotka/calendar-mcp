using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace CalendarMcp.Core.Tools;

/// <summary>
/// MCP tool for creating calendar events
/// </summary>
public class CreateEventTool : McpServerTool
{
    private readonly IAccountRegistry _accountRegistry;
    private readonly IProviderServiceFactory _providerFactory;
    private readonly ILogger<CreateEventTool> _logger;

    public CreateEventTool(
        IAccountRegistry accountRegistry,
        IProviderServiceFactory providerFactory,
        ILogger<CreateEventTool> logger)
    {
        _accountRegistry = accountRegistry;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public override Tool ProtocolTool => new Tool
    {
        Name = "create_event",
        Description = "Create calendar event in specific calendar (requires explicit account selection or smart routing)",
        InputSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
            "type": "object",
            "properties": {
                "subject": {
                    "type": "string",
                    "description": "Event subject/title"
                },
                "start": {
                    "type": "string",
                    "description": "Event start date and time (ISO 8601 format)",
                    "format": "date-time"
                },
                "end": {
                    "type": "string",
                    "description": "Event end date and time (ISO 8601 format)",
                    "format": "date-time"
                },
                "accountId": {
                    "type": "string",
                    "description": "Specific account ID, or omit for smart routing"
                },
                "calendarId": {
                    "type": "string",
                    "description": "Specific calendar ID, or omit for default calendar"
                },
                "location": {
                    "type": "string",
                    "description": "Event location"
                },
                "attendees": {
                    "type": "array",
                    "description": "List of attendee email addresses",
                    "items": {
                        "type": "string"
                    }
                },
                "body": {
                    "type": "string",
                    "description": "Event description/body"
                }
            },
            "required": ["subject", "start", "end"]
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
            if (args?.TryGetValue("subject", out var subjectObj) != true || subjectObj == null)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = new List<ContentBlock>
                    {
                        new TextContentBlock
                        {
                            Type = "text",
                            Text = "Parameter 'subject' is required"
                        }
                    }
                };
            }
            
            if (args.TryGetValue("start", out var startObj) != true || startObj == null)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = new List<ContentBlock>
                    {
                        new TextContentBlock
                        {
                            Type = "text",
                            Text = "Parameter 'start' is required"
                        }
                    }
                };
            }
            
            if (args.TryGetValue("end", out var endObj) != true || endObj == null)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = new List<ContentBlock>
                    {
                        new TextContentBlock
                        {
                            Type = "text",
                            Text = "Parameter 'end' is required"
                        }
                    }
                };
            }
            
            string subject = subjectObj.ToString()!;
            DateTime start = DateTime.Parse(startObj.ToString()!);
            DateTime end = DateTime.Parse(endObj.ToString()!);
            
            // Parse optional parameters
            string? accountId = args.TryGetValue("accountId", out var accountIdObj)
                ? accountIdObj?.ToString()
                : null;
            
            string? calendarId = args.TryGetValue("calendarId", out var calendarIdObj)
                ? calendarIdObj?.ToString()
                : null;
            
            string? location = args.TryGetValue("location", out var locationObj)
                ? locationObj?.ToString()
                : null;
            
            List<string>? attendees = args.TryGetValue("attendees", out var attendeesObj) && attendeesObj != null
                ? JsonSerializer.Deserialize<List<string>>(attendeesObj.ToString()!)
                : null;
            
            string? body = args.TryGetValue("body", out var bodyObj)
                ? bodyObj?.ToString()
                : null;

            // Determine which account to use
            Models.AccountInfo? account = null;
            
            if (!string.IsNullOrEmpty(accountId))
            {
                account = _accountRegistry.GetAccount(accountId);
                if (account == null)
                {
                    return new CallToolResult
                    {
                        IsError = true,
                        Content = new List<ContentBlock>
                        {
                            new TextContentBlock
                            {
                                Type = "text",
                                Text = $"Account '{accountId}' not found"
                            }
                        }
                    };
                }
            }
            else
            {
                // Use first enabled account (could enhance with smarter routing)
                account = _accountRegistry.GetEnabledAccounts().FirstOrDefault();
                
                if (account == null)
                {
                    return new CallToolResult
                    {
                        IsError = true,
                        Content = new List<ContentBlock>
                        {
                            new TextContentBlock
                            {
                                Type = "text",
                                Text = "No enabled account available to create event"
                            }
                        }
                    };
                }
            }

            // Create event
            var provider = _providerFactory.GetProvider(account.Provider);
            var eventId = await provider.CreateEventAsync(
                account.Id, calendarId, subject, start, end, location, attendees, body, cancellationToken);

            var result = new
            {
                success = true,
                eventId = eventId,
                accountUsed = account.Id,
                calendarUsed = calendarId ?? "default"
            };

            _logger.LogInformation("Created event in account {AccountId}", account.Id);
            
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
            _logger.LogError(ex, "Error in create_event tool");
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
