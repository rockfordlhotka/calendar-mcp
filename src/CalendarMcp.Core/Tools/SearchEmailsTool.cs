using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace CalendarMcp.Core.Tools;

/// <summary>
/// MCP tool for searching emails
/// </summary>
public class SearchEmailsTool : McpServerTool
{
    private readonly IAccountRegistry _accountRegistry;
    private readonly IProviderServiceFactory _providerFactory;
    private readonly ILogger<SearchEmailsTool> _logger;

    public SearchEmailsTool(
        IAccountRegistry accountRegistry,
        IProviderServiceFactory providerFactory,
        ILogger<SearchEmailsTool> logger)
    {
        _accountRegistry = accountRegistry;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public override Tool ProtocolTool => new Tool
    {
        Name = "search_emails",
        Description = "Search emails by sender/subject/criteria for specific account or all accounts",
        InputSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
            "type": "object",
            "properties": {
                "query": {
                    "type": "string",
                    "description": "Search query string"
                },
                "accountId": {
                    "type": "string",
                    "description": "Specific account ID, or omit for all accounts"
                },
                "count": {
                    "type": "integer",
                    "description": "Number of emails to retrieve",
                    "default": 20
                },
                "fromDate": {
                    "type": "string",
                    "description": "Start date for search (ISO 8601 format)",
                    "format": "date-time"
                },
                "toDate": {
                    "type": "string",
                    "description": "End date for search (ISO 8601 format)",
                    "format": "date-time"
                }
            },
            "required": ["query"]
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
            if (args?.TryGetValue("query", out var queryObj) != true || queryObj == null)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = new List<ContentBlock>
                    {
                        new TextContentBlock
                        {
                            Type = "text",
                            Text = "Parameter 'query' is required"
                        }
                    }
                };
            }
            
            string query = queryObj.ToString()!;
            
            // Parse optional parameters
            string? accountId = args.TryGetValue("accountId", out var accountIdObj)
                ? accountIdObj?.ToString()
                : null;
            
            int count = args.TryGetValue("count", out var countObj) && countObj != null
                ? Convert.ToInt32(countObj)
                : 20;
            
            DateTime? fromDate = args.TryGetValue("fromDate", out var fromDateObj) && fromDateObj != null
                ? DateTime.Parse(fromDateObj.ToString()!)
                : null;
            
            DateTime? toDate = args.TryGetValue("toDate", out var toDateObj) && toDateObj != null
                ? DateTime.Parse(toDateObj.ToString()!)
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
                    var emails = await provider.SearchEmailsAsync(
                        account.Id, query, count, fromDate, toDate, cancellationToken);
                    return emails;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching emails in account {AccountId}", account.Id);
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

            _logger.LogInformation("Found {Count} emails matching '{Query}' from {AccountCount} accounts",
                result.emails.Count, query, accounts.Count);
            
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
            _logger.LogError(ex, "Error in search_emails tool");
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
