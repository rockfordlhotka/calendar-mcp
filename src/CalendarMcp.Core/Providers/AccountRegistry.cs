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
        if (config.Accounts != null)
        {
            foreach (var account in config.Accounts)
            {
                _accounts[account.Id] = account;
                _logger.LogInformation("Loaded account {AccountId} ({Provider})", account.Id, account.Provider);
            }
        }

        _logger.LogInformation("Account registry initialized with {Count} accounts", _accounts.Count);
    }

    public IEnumerable<AccountInfo> GetAllAccounts()
    {
        return _accounts.Values;
    }

    public IEnumerable<AccountInfo> GetEnabledAccounts()
    {
        return _accounts.Values.Where(a => a.Enabled);
    }

    public AccountInfo? GetAccount(string accountId)
    {
        return _accounts.TryGetValue(accountId, out var account) ? account : null;
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
