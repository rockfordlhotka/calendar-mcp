using CalendarMcp.Core.Configuration;
using CalendarMcp.Core.Models;
using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CalendarMcp.Core.Providers;

/// <summary>
/// In-memory account registry loaded from configuration
/// </summary>
public class AccountRegistry : IAccountRegistry
{
    private readonly Dictionary<string, AccountInfo> _accounts;
    private readonly ILogger<AccountRegistry> _logger;

    public AccountRegistry(IOptions<CalendarMcpConfiguration> configuration, ILogger<AccountRegistry> logger)
    {
        _logger = logger;
        _accounts = new Dictionary<string, AccountInfo>(StringComparer.OrdinalIgnoreCase);

        // Load accounts from configuration
        var config = configuration.Value;
        if (config.Accounts != null && config.Accounts.Count > 0)
        {
            _logger.LogInformation("Loading {Count} account(s) from configuration...", config.Accounts.Count);
            
            foreach (var account in config.Accounts)
            {
                _accounts[account.Id] = account;
                
                var domains = account.Domains.Count > 0 
                    ? string.Join(", ", account.Domains) 
                    : "(none)";
                var status = account.Enabled ? "enabled" : "disabled";
                
                _logger.LogInformation(
                    "  Account: {AccountId} | {DisplayName} | Provider: {Provider} | Domains: {Domains} | Status: {Status} | Priority: {Priority}",
                    account.Id,
                    account.DisplayName,
                    account.Provider,
                    domains,
                    status,
                    account.Priority);
            }
            
            var enabledCount = _accounts.Values.Count(a => a.Enabled);
            _logger.LogInformation("Account registry initialized: {EnabledCount} enabled, {DisabledCount} disabled",
                enabledCount, _accounts.Count - enabledCount);
        }
        else
        {
            _logger.LogWarning("No accounts found in configuration. Add accounts using the CLI: calendar-mcp-cli add-m365-account");
        }
    }

    public Task<IEnumerable<AccountInfo>> GetAllAccountsAsync()
    {
        return Task.FromResult<IEnumerable<AccountInfo>>(_accounts.Values);
    }

    public Task<AccountInfo?> GetAccountAsync(string accountId)
    {
        var account = _accounts.TryGetValue(accountId, out var acc) ? acc : null;
        return Task.FromResult(account);
    }

    public IEnumerable<AccountInfo> GetEnabledAccounts()
    {
        return _accounts.Values.Where(a => a.Enabled);
    }

    public IEnumerable<AccountInfo> GetAccountsByProvider(string provider)
    {
        return _accounts.Values.Where(a => 
            string.Equals(a.Provider, provider, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<AccountInfo> GetAccountsByDomain(string domain)
    {
        return _accounts.Values.Where(a => 
            a.Domains.Any(d => string.Equals(d, domain, StringComparison.OrdinalIgnoreCase)));
    }
}
