namespace CalendarMcp.Core.Models;

/// <summary>
/// Represents a configured calendar/email account
/// </summary>
public class AccountInfo
{
    /// <summary>
    /// Unique identifier for this account (e.g., "xebia-work", "rocky-gmail")
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Display name for user reference (e.g., "Xebia Work Account")
    /// </summary>
    public required string DisplayName { get; init; }
    
    /// <summary>
    /// Provider type: "microsoft365", "google", "outlook.com"
    /// </summary>
    public required string Provider { get; init; }
    
    /// <summary>
    /// Email domains associated with this account for smart routing (e.g., ["xebia.com"])
    /// </summary>
    public List<string> Domains { get; init; } = new();
    
    /// <summary>
    /// Whether this account is enabled for queries
    /// </summary>
    public bool Enabled { get; init; } = true;
    
    /// <summary>
    /// Priority for ambiguous routing decisions (higher = preferred)
    /// </summary>
    public int Priority { get; init; } = 0;
    
    /// <summary>
    /// Provider-specific configuration (tenant ID, client ID, etc.)
    /// </summary>
    public Dictionary<string, string> ProviderConfig { get; init; } = new();
}
