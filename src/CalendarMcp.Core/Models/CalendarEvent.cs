namespace CalendarMcp.Core.Models;

/// <summary>
/// Unified calendar event representation across all providers
/// </summary>
public class CalendarEvent
{
    /// <summary>
    /// Provider-specific event ID
    /// </summary>
    public required string Id { get; init; }
    
    /// <summary>
    /// Account ID this event belongs to
    /// </summary>
    public required string AccountId { get; init; }
    
    /// <summary>
    /// Calendar ID within the account
    /// </summary>
    public required string CalendarId { get; init; }
    
    /// <summary>
    /// Event subject/title
    /// </summary>
    public string Subject { get; init; } = string.Empty;
    
    /// <summary>
    /// Event start date/time
    /// </summary>
    public DateTime Start { get; init; }
    
    /// <summary>
    /// Event end date/time
    /// </summary>
    public DateTime End { get; init; }
    
    /// <summary>
    /// Event location
    /// </summary>
    public string Location { get; init; } = string.Empty;
    
    /// <summary>
    /// Event description/body
    /// </summary>
    public string Body { get; init; } = string.Empty;
    
    /// <summary>
    /// Event organizer email
    /// </summary>
    public string Organizer { get; init; } = string.Empty;
    
    /// <summary>
    /// Attendee email addresses
    /// </summary>
    public List<string> Attendees { get; init; } = new();
    
    /// <summary>
    /// Whether this is an all-day event
    /// </summary>
    public bool IsAllDay { get; init; }
    
    /// <summary>
    /// Response status (if attendee): "accepted", "tentative", "declined", "notResponded"
    /// </summary>
    public string ResponseStatus { get; init; } = "notResponded";
}
