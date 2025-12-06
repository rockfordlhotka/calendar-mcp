using System.ComponentModel;
using System.Text.Json;
using CalendarMcp.Core.Models;
using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace CalendarMcp.Core.Tools;

/// <summary>
/// MCP tool for searching emails
/// </summary>
[McpServerToolType]
public sealed class SearchEmailsTool(
    IAccountRegistry accountRegistry,
    IProviderServiceFactory providerFactory,
    ILogger<SearchEmailsTool> logger)
{
    [McpServerTool, Description("Search emails by sender/subject/criteria for specific account or all accounts")]
    public async Task<string> SearchEmails(
        [Description("Search query string")] string query,
        [Description("Specific account ID, or omit for all accounts")] string? accountId = null,
        [Description("Number of emails to retrieve")] int count = 20,
        [Description("Start date for search (ISO 8601 format)")] DateTime? fromDate = null,
        [Description("End date for search (ISO 8601 format)")] DateTime? toDate = null)
    {
        logger.LogInformation("Searching emails: query={Query}, accountId={AccountId}, count={Count}",
            query, accountId, count);

        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return JsonSerializer.Serialize(new
                {
                    error = "Parameter 'query' is required"
                });
            }

            // Determine which accounts to query
            var accounts = string.IsNullOrEmpty(accountId)
                ? await accountRegistry.GetAllAccountsAsync()
                : new[] { await accountRegistry.GetAccountAsync(accountId) }.Where(a => a != null).Cast<AccountInfo>();

            var validAccounts = accounts.ToList();

            if (validAccounts.Count == 0)
            {
                return JsonSerializer.Serialize(new
                {
                    error = accountId != null ? $"Account '{accountId}' not found" : "No accounts found"
                });
            }

            // Query all accounts in parallel
            var tasks = validAccounts.Select(async account =>
            {
                try
                {
                    var provider = providerFactory.GetProvider(account!.Provider);
                    var emails = await provider.SearchEmailsAsync(
                        account.Id, query, count, fromDate, toDate, CancellationToken.None);
                    return emails;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error searching emails in account {AccountId}", account!.Id);
                    return Enumerable.Empty<EmailMessage>();
                }
            });

            var results = await Task.WhenAll(tasks);
            var allEmails = results.SelectMany(e => e)
                .OrderByDescending(e => e.ReceivedDateTime)
                .ToList();

            var response = new
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
                })
            };

            logger.LogInformation("Found {Count} emails matching '{Query}' from {AccountCount} accounts",
                allEmails.Count, query, validAccounts.Count);

            return JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in search_emails tool");
            return JsonSerializer.Serialize(new
            {
                error = "Failed to search emails",
                message = ex.Message
            });
        }
    }
}
