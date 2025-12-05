using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace CalendarMcp.Core.Tools;

/// <summary>
/// MCP tool for getting emails
/// </summary>
public class GetEmailsTool : McpServerTool
{
    private readonly IAccountRegistry _accountRegistry;
    private readonly IProviderServiceFactory _providerFactory;
    private readonly ILogger<GetEmailsTool> _logger;

    public GetEmailsTool(
        IAccountRegistry accountRegistry,
        IProviderServiceFactory providerFactory,
        ILogger<GetEmailsTool> logger)
    {
        _accountRegistry = accountRegistry;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public override Tool ProtocolTool => new Tool
    {
        Name = "get_emails",
        Description = "Get emails (unread/read, filtered by count) for specific account or all accounts",
        InputSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
            "type": "object",
            "properties": {
                "accountId": {
                    "type": "string",
                    "description": "Specific account ID, or omit for all accounts"
                },
                "count": {
                    "type": "integer",
                    "description": "Number of emails to retrieve",
                    "default": 20
                },
                "unreadOnly": {
                    "type": "boolean",
                    "description": "Only return unread emails",
                    "default": false
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
            
            // Parse parameters
            string? accountId = args?.TryGetValue("accountId", out var accountIdObj) == true
                ? accountIdObj?.ToString()
                : null;
            
            int count = args?.TryGetValue("count", out var countObj) == true && countObj != null
                ? Convert.ToInt32(countObj)
                : 20;
            
            bool unreadOnly = args?.TryGetValue("unreadOnly", out var unreadObj) == true && unreadObj != null
                ? Convert.ToBoolean(unreadObj)
                : false;

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
                    var emails = await provider.GetEmailsAsync(account.Id, count, unreadOnly, cancellationToken);
                    return emails;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting emails from account {AccountId}", account.Id);
                    return Enumerable.Empty<Models.EmailMessage>();
                }
            });

            var results = await Task.WhenAll(tasks);
            var allEmails = results.SelectMany(e => e)
                .OrderByDescending(e => e.ReceivedDateTime)
                .ToList();

            var result = new
            {
                emails = allEmails.Select(e => new
                {
                    id = e.Id,
                    accountId = e.AccountId,
                    subject = e.Subject,
                    from = e.From,
                    receivedDateTime = e.ReceivedDateTime,
                    isRead = e.IsRead,
                    hasAttachments = e.HasAttachments
                }).ToList()
            };

            _logger.LogInformation("Retrieved {Count} emails from {AccountCount} accounts",
                result.emails.Count, accounts.Count);
            
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
            _logger.LogError(ex, "Error in get_emails tool");
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
