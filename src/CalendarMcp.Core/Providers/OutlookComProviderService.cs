using CalendarMcp.Core.Models;
using CalendarMcp.Core.Services;
using Microsoft.Extensions.Logging;

namespace CalendarMcp.Core.Providers;

/// <summary>
/// Stub implementation of Outlook.com provider service for personal Microsoft accounts
/// TODO: Implement using Microsoft Graph SDK with MSAL (common tenant)
/// </summary>
public class OutlookComProviderService : IOutlookComProviderService
{
    private readonly ILogger<OutlookComProviderService> _logger;

    public OutlookComProviderService(ILogger<OutlookComProviderService> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<EmailMessage>> GetEmailsAsync(
        string accountId, 
        int count = 20, 
        bool unreadOnly = false, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("OutlookComProviderService.GetEmailsAsync not yet implemented for account {AccountId}", accountId);
        return Task.FromResult(Enumerable.Empty<EmailMessage>());
    }

    public Task<IEnumerable<EmailMessage>> SearchEmailsAsync(
        string accountId, 
        string query, 
        int count = 20, 
        DateTime? fromDate = null, 
        DateTime? toDate = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("OutlookComProviderService.SearchEmailsAsync not yet implemented for account {AccountId}", accountId);
        return Task.FromResult(Enumerable.Empty<EmailMessage>());
    }

    public Task<EmailMessage?> GetEmailDetailsAsync(
        string accountId, 
        string emailId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("OutlookComProviderService.GetEmailDetailsAsync not yet implemented for account {AccountId}", accountId);
        return Task.FromResult<EmailMessage?>(null);
    }

    public Task<string> SendEmailAsync(
        string accountId, 
        string to, 
        string subject, 
        string body, 
        string bodyFormat = "html", 
        List<string>? cc = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("OutlookComProviderService.SendEmailAsync not yet implemented for account {AccountId}", accountId);
        return Task.FromResult("stub-message-id");
    }

    public Task<IEnumerable<CalendarInfo>> ListCalendarsAsync(
        string accountId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("OutlookComProviderService.ListCalendarsAsync not yet implemented for account {AccountId}", accountId);
        return Task.FromResult(Enumerable.Empty<CalendarInfo>());
    }

    public Task<IEnumerable<CalendarEvent>> GetCalendarEventsAsync(
        string accountId, 
        string? calendarId = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        int count = 50, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("OutlookComProviderService.GetCalendarEventsAsync not yet implemented for account {AccountId}", accountId);
        return Task.FromResult(Enumerable.Empty<CalendarEvent>());
    }

    public Task<string> CreateEventAsync(
        string accountId, 
        string? calendarId, 
        string subject, 
        DateTime start, 
        DateTime end, 
        string? location = null, 
        List<string>? attendees = null, 
        string? body = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("OutlookComProviderService.CreateEventAsync not yet implemented for account {AccountId}", accountId);
        return Task.FromResult("stub-event-id");
    }

    public Task UpdateEventAsync(
        string accountId, 
        string calendarId, 
        string eventId, 
        string? subject = null, 
        DateTime? start = null, 
        DateTime? end = null, 
        string? location = null, 
        List<string>? attendees = null, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("OutlookComProviderService.UpdateEventAsync not yet implemented for account {AccountId}", accountId);
        return Task.CompletedTask;
    }

    public Task DeleteEventAsync(
        string accountId, 
        string calendarId, 
        string eventId, 
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("OutlookComProviderService.DeleteEventAsync not yet implemented for account {AccountId}", accountId);
        return Task.CompletedTask;
    }
}
