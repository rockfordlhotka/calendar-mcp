using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text.Json;

namespace CalendarMcp.Core.Tools;

/// <summary>
/// MCP tool for sending emails
/// </summary>
public class SendEmailTool : McpServerTool
{
    private readonly IAccountRegistry _accountRegistry;
    private readonly IProviderServiceFactory _providerFactory;
    private readonly ILogger<SendEmailTool> _logger;

    public SendEmailTool(
        IAccountRegistry accountRegistry,
        IProviderServiceFactory providerFactory,
        ILogger<SendEmailTool> logger)
    {
        _accountRegistry = accountRegistry;
        _providerFactory = providerFactory;
        _logger = logger;
    }

    public override Tool ProtocolTool => new Tool
    {
        Name = "send_email",
        Description = "Send email from specific account (requires explicit account selection or smart routing)",
        InputSchema = JsonSerializer.Deserialize<JsonElement>("""
        {
            "type": "object",
            "properties": {
                "to": {
                    "type": "string",
                    "description": "Recipient email address"
                },
                "subject": {
                    "type": "string",
                    "description": "Email subject"
                },
                "body": {
                    "type": "string",
                    "description": "Email body content"
                },
                "accountId": {
                    "type": "string",
                    "description": "Specific account ID, or omit for smart routing"
                },
                "bodyFormat": {
                    "type": "string",
                    "description": "Body format: 'html' or 'text'",
                    "default": "html"
                },
                "cc": {
                    "type": "array",
                    "description": "CC recipients",
                    "items": {
                        "type": "string"
                    }
                }
            },
            "required": ["to", "subject", "body"]
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
            if (args?.TryGetValue("to", out var toObj) != true || toObj == null)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = new List<ContentBlock>
                    {
                        new TextContentBlock
                        {
                            Type = "text",
                            Text = "Parameter 'to' is required"
                        }
                    }
                };
            }
            
            if (args.TryGetValue("subject", out var subjectObj) != true || subjectObj == null)
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
            
            if (args.TryGetValue("body", out var bodyObj) != true || bodyObj == null)
            {
                return new CallToolResult
                {
                    IsError = true,
                    Content = new List<ContentBlock>
                    {
                        new TextContentBlock
                        {
                            Type = "text",
                            Text = "Parameter 'body' is required"
                        }
                    }
                };
            }
            
            string to = toObj.ToString()!;
            string subject = subjectObj.ToString()!;
            string body = bodyObj.ToString()!;
            
            // Parse optional parameters
            string? accountId = args.TryGetValue("accountId", out var accountIdObj)
                ? accountIdObj?.ToString()
                : null;
            
            string bodyFormat = args.TryGetValue("bodyFormat", out var bodyFormatObj) && bodyFormatObj != null
                ? bodyFormatObj.ToString()!
                : "html";
            
            List<string>? cc = args.TryGetValue("cc", out var ccObj) && ccObj != null
                ? JsonSerializer.Deserialize<List<string>>(ccObj.ToString()!)
                : null;

            // Determine which account to use
            Models.AccountInfo? account = null;
            
            if (!string.IsNullOrEmpty(accountId))
            {
                // Explicit account specified
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
                // Smart routing: extract domain from recipient
                var recipientDomain = to.Split('@').LastOrDefault();
                if (!string.IsNullOrEmpty(recipientDomain))
                {
                    var matchingAccounts = _accountRegistry.GetAccountsByDomain(recipientDomain).ToList();
                    if (matchingAccounts.Count == 1)
                    {
                        account = matchingAccounts[0];
                        _logger.LogInformation("Smart routing selected account {AccountId} based on domain {Domain}",
                            account.Id, recipientDomain);
                    }
                    else if (matchingAccounts.Count > 1)
                    {
                        // Multiple matches, use priority
                        account = matchingAccounts.OrderByDescending(a => a.Priority).First();
                        _logger.LogInformation("Smart routing selected account {AccountId} (priority) from {Count} matches",
                            account.Id, matchingAccounts.Count);
                    }
                }
                
                // If still no account, use default (first enabled)
                account ??= _accountRegistry.GetEnabledAccounts().FirstOrDefault();
                
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
                                Text = "No enabled account available to send email"
                            }
                        }
                    };
                }
            }

            // Send email
            var provider = _providerFactory.GetProvider(account.Provider);
            var messageId = await provider.SendEmailAsync(
                account.Id, to, subject, body, bodyFormat, cc, cancellationToken);

            var result = new
            {
                success = true,
                messageId = messageId,
                accountUsed = account.Id
            };

            _logger.LogInformation("Sent email from account {AccountId} to {To}", account.Id, to);
            
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
            _logger.LogError(ex, "Error in send_email tool");
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
