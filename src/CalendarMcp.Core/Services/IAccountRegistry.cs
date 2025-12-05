using CalendarMcp.Core.Models;

namespace CalendarMcp.Core.Services;

/// <summary>
/// Manages account configuration and registry
/// </summary>
public interface IAccountRegistry
{
    /// <summary>
    /// Gets all configured accounts
    /// </summary>
    IEnumerable<AccountInfo> GetAllAccounts();
    
    /// <summary>
    /// Gets enabled accounts only
    /// </summary>
    IEnumerable<AccountInfo> GetEnabledAccounts();
    
    /// <summary>
    /// Gets account by ID
    /// </summary>
    AccountInfo? GetAccount(string accountId);
    
    /// <summary>
    /// Gets accounts by provider type
    /// </summary>
    IEnumerable<AccountInfo> GetAccountsByProvider(string provider);
    
    /// <summary>
    /// Gets accounts that match the given email domain
    /// </summary>
    IEnumerable<AccountInfo> GetAccountsByDomain(string domain);
}
