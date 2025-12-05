using Microsoft.Identity.Client;
using System.Text.Json;

namespace OutlookRestSpike;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Outlook.com Personal Account Spike ===");
        Console.WriteLine();

        // Load configuration
        var config = LoadConfiguration();
        if (config == null)
        {
            Console.WriteLine("ERROR: Failed to load configuration. Please set up appsettings.Development.json");
            return;
        }

        Console.WriteLine("Configuration loaded:");
        Console.WriteLine($"  Client ID: {config.ClientId}");
        Console.WriteLine($"  Tenant: {config.TenantId}");
        Console.WriteLine($"  Base URL: {config.ApiBaseUrl}");
        Console.WriteLine($"  Scopes: {string.Join(", ", config.Scopes)}");
        Console.WriteLine();

        // Initialize authenticator
        var authenticator = new GraphAuthenticator(config);

        try
        {
            // Step 1: Authenticate
            Console.WriteLine("Step 1: Authenticating...");
            var accessToken = await authenticator.AuthenticateAsync();
            Console.WriteLine($"✅ Authentication successful!");
            Console.WriteLine($"   Token preview: {accessToken[..20]}...");
            Console.WriteLine();

            // Step 2: Test Calendar Access
            Console.WriteLine("Step 2: Testing calendar access...");
            var calendarService = new GraphCalendarService(config, accessToken);
            
            var calendars = await calendarService.ListCalendarsAsync();
            Console.WriteLine($"✅ Found {calendars.Count} calendar(s)");
            foreach (var calendar in calendars)
            {
                Console.WriteLine($"   - {calendar.Name} (ID: {calendar.Id})");
            }
            Console.WriteLine();

            // Step 3: List Calendar Events
            Console.WriteLine("Step 3: Listing calendar events...");
            var events = await calendarService.ListEventsAsync();
            Console.WriteLine($"✅ Found {events.Count} event(s)");
            foreach (var evt in events.Take(5))
            {
                Console.WriteLine($"   - {evt.Subject}");
                Console.WriteLine($"     Start: {evt.Start}");
                Console.WriteLine($"     End: {evt.End}");
            }
            if (events.Count > 5)
            {
                Console.WriteLine($"   ... and {events.Count - 5} more");
            }
            Console.WriteLine();

            // Step 4: Test Mail Access
            Console.WriteLine("Step 4: Testing mail access...");
            var mailService = new GraphMailService(config, accessToken);
            
            var messages = await mailService.ListMessagesAsync(maxResults: 5);
            Console.WriteLine($"✅ Found {messages.Count} message(s)");
            foreach (var message in messages)
            {
                Console.WriteLine($"   - From: {message.From}");
                Console.WriteLine($"     Subject: {message.Subject}");
                Console.WriteLine($"     Received: {message.ReceivedDateTime}");
            }
            Console.WriteLine();

            Console.WriteLine("=== Spike Completed Successfully ===");
            Console.WriteLine("✅ Authentication: WORKING");
            Console.WriteLine("✅ Calendar Read: WORKING");
            Console.WriteLine("✅ Mail Read: WORKING");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("  - Test calendar write operations (create/update/delete)");
            Console.WriteLine("  - Test mail send operation");
            Console.WriteLine("  - Document API response schemas");
            Console.WriteLine("  - Compare with Graph API patterns");
        }
        catch (MsalException ex)
        {
            Console.WriteLine($"❌ Authentication error: {ex.Message}");
            Console.WriteLine($"   Error code: {ex.ErrorCode}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"❌ HTTP error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Unexpected error: {ex.Message}");
            Console.WriteLine($"   Type: {ex.GetType().Name}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
        }
    }

    static GraphConfig? LoadConfiguration()
    {
        try
        {
            // Try to load appsettings.Development.json first, then fall back to appsettings.json
            var devPath = "appsettings.Development.json";
            var defaultPath = "appsettings.json";
            
            var path = File.Exists(devPath) ? devPath : defaultPath;
            var json = File.ReadAllText(path);
            var doc = JsonDocument.Parse(json);
            
            var configSection = doc.RootElement.GetProperty("MicrosoftGraph");
            
            return new GraphConfig
            {
                ClientId = configSection.GetProperty("ClientId").GetString() ?? "",
                TenantId = configSection.GetProperty("TenantId").GetString() ?? "common",
                RedirectUri = configSection.GetProperty("RedirectUri").GetString() ?? "",
                ApiBaseUrl = configSection.GetProperty("ApiBaseUrl").GetString() ?? "",
                Scopes = configSection.GetProperty("Scopes")
                    .EnumerateArray()
                    .Select(s => s.GetString() ?? "")
                    .ToArray()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
            return null;
        }
    }
}

class GraphConfig
{
    public string ClientId { get; set; } = "";
    public string TenantId { get; set; } = "common";
    public string RedirectUri { get; set; } = "";
    public string ApiBaseUrl { get; set; } = "";
    public string[] Scopes { get; set; } = Array.Empty<string>();
}

class GraphAuthenticator
{
    private readonly GraphConfig _config;
    private readonly IPublicClientApplication _app;

    public GraphAuthenticator(GraphConfig config)
    {
        _config = config;
        _app = PublicClientApplicationBuilder
            .Create(config.ClientId)
            .WithAuthority($"https://login.microsoftonline.com/{config.TenantId}")
            .WithRedirectUri(config.RedirectUri)
            .Build();
    }

    public async Task<string> AuthenticateAsync()
    {
        try
        {
            // Try to get token silently from cache first
            var accounts = await _app.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    var result = await _app.AcquireTokenSilent(_config.Scopes, accounts.First())
                        .ExecuteAsync();
                    return result.AccessToken;
                }
                catch (MsalUiRequiredException)
                {
                    // Silent acquisition failed, fall through to interactive
                }
            }

            // Perform interactive authentication
            var authResult = await _app.AcquireTokenInteractive(_config.Scopes)
                .WithPrompt(Prompt.SelectAccount)
                .ExecuteAsync();

            return authResult.AccessToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Authentication failed: {ex.Message}");
            throw;
        }
    }
}

class GraphCalendarService
{
    private readonly GraphConfig _config;
    private readonly string _accessToken;
    private readonly HttpClient _httpClient;

    public GraphCalendarService(GraphConfig config, string accessToken)
    {
        _config = config;
        _accessToken = accessToken;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<List<Calendar>> ListCalendarsAsync()
    {
        var response = await _httpClient.GetAsync($"{_config.ApiBaseUrl}/me/calendars");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var calendars = new List<Calendar>();
        
        foreach (var item in doc.RootElement.GetProperty("value").EnumerateArray())
        {
            calendars.Add(new Calendar
            {
                Id = item.GetProperty("id").GetString() ?? "",
                Name = item.GetProperty("name").GetString() ?? ""
            });
        }
        
        return calendars;
    }

    public async Task<List<CalendarEvent>> ListEventsAsync()
    {
        var response = await _httpClient.GetAsync($"{_config.ApiBaseUrl}/me/calendar/events");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var events = new List<CalendarEvent>();
        
        foreach (var item in doc.RootElement.GetProperty("value").EnumerateArray())
        {
            events.Add(new CalendarEvent
            {
                Id = item.GetProperty("id").GetString() ?? "",
                Subject = item.GetProperty("subject").GetString() ?? "",
                Start = item.GetProperty("start").GetProperty("dateTime").GetString() ?? "",
                End = item.GetProperty("end").GetProperty("dateTime").GetString() ?? ""
            });
        }
        
        return events;
    }
}

class GraphMailService
{
    private readonly GraphConfig _config;
    private readonly string _accessToken;
    private readonly HttpClient _httpClient;

    public GraphMailService(GraphConfig config, string accessToken)
    {
        _config = config;
        _accessToken = accessToken;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<List<MailMessage>> ListMessagesAsync(int maxResults = 10)
    {
        var response = await _httpClient.GetAsync(
            $"{_config.ApiBaseUrl}/me/mailfolders/inbox/messages?$top={maxResults}");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var messages = new List<MailMessage>();
        
        foreach (var item in doc.RootElement.GetProperty("value").EnumerateArray())
        {
            var from = item.GetProperty("from").GetProperty("emailAddress");
            messages.Add(new MailMessage
            {
                Id = item.GetProperty("id").GetString() ?? "",
                Subject = item.GetProperty("subject").GetString() ?? "",
                From = from.GetProperty("address").GetString() ?? "",
                ReceivedDateTime = item.GetProperty("receivedDateTime").GetString() ?? ""
            });
        }
        
        return messages;
    }
}

// Model classes
class Calendar
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}

class CalendarEvent
{
    public string Id { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Start { get; set; } = "";
    public string End { get; set; } = "";
}

class MailMessage
{
    public string Id { get; set; } = "";
    public string Subject { get; set; } = "";
    public string From { get; set; } = "";
    public string ReceivedDateTime { get; set; } = "";
}
