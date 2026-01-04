# 2025-01-04: Google/Gmail Support Implementation

## Summary

Added full Google Workspace/Gmail support for authentication (CLI) and tools (StdioServer), including support for custom domains like lhotka.net.

## Changes Made

### Core Library (CalendarMcp.Core)

#### New Files
- `Services/IGoogleAuthenticationService.cs` - Interface for Google OAuth authentication
- `Providers/GoogleAuthenticationService.cs` - Implementation using Google.Apis.Auth for OAuth 2.0 flow with per-account token caching

#### Modified Files
- `Providers/GoogleProviderService.cs` - Fully implemented (was previously a stub) with real Google API integration:
  - Gmail API integration for email operations (get, search, send, details)
  - Google Calendar API integration for calendar operations (list, events, create, update, delete)
  - Per-account credential isolation using FileDataStore
  - Token refresh handling
- `Configuration/ServiceCollectionExtensions.cs` - Added `IGoogleAuthenticationService` registration

### CLI (CalendarMcp.Cli)

#### New Files
- `Commands/AddGoogleAccountCommand.cs` - Interactive command to add Google accounts:
  - Supports Gmail (@gmail.com)
  - Supports Google Workspace
  - Supports custom domains (e.g., lhotka.net)
  - Prompts for Client ID and Client Secret from Google Cloud Console
  - Interactive OAuth authentication flow
  - Saves credentials to per-account directory

#### Modified Files
- `Program.cs` - Added `add-google-account` command and `IGoogleAuthenticationService` registration
- `Commands/TestAccountCommand.cs` - Extended to support testing Google account authentication
- `CalendarMcp.Cli.csproj` - Added `Google.Apis.Auth` package reference
- `README.md` - Comprehensive documentation update including:
  - Google Cloud Console setup instructions
  - New command documentation
  - Configuration examples
  - Troubleshooting guide

## Token Storage

### Microsoft Accounts (M365, Outlook.com)
- Location: `%LOCALAPPDATA%/CalendarMcp/msal_cache_{accountId}.bin`
- Encryption: DPAPI (Windows), Keychain (macOS)

### Google Accounts
- Location: `%LOCALAPPDATA%/CalendarMcp/google/{accountId}/`
- File: `Google.Apis.Auth.OAuth2.Responses.TokenResponse-user`
- Per-account directory isolation

## Configuration Example

```json
{
  "CalendarMcp": {
    "Accounts": [
      {
        "id": "rocky-gmail",
        "displayName": "Personal Gmail",
        "provider": "google",
        "enabled": true,
        "priority": 0,
        "domains": ["gmail.com"],
        "providerConfig": {
          "clientId": "123456789.apps.googleusercontent.com",
          "clientSecret": "GOCSPX-xxxxxxxxxxxx"
        }
      },
      {
        "id": "lhotka-workspace",
        "displayName": "Lhotka.net Workspace",
        "provider": "google",
        "enabled": true,
        "priority": 1,
        "domains": ["lhotka.net"],
        "providerConfig": {
          "clientId": "987654321.apps.googleusercontent.com",
          "clientSecret": "GOCSPX-yyyyyyyyyyyy"
        }
      }
    ]
  }
}
```

## Google Cloud Console Prerequisites

To use Google authentication, users must:

1. Create a project in Google Cloud Console
2. Enable Gmail API and Google Calendar API
3. Create OAuth 2.0 Client ID (Desktop app type)
4. Configure OAuth consent screen
5. Add required scopes:
   - `https://www.googleapis.com/auth/gmail.readonly`
   - `https://www.googleapis.com/auth/gmail.send`
   - `https://www.googleapis.com/auth/gmail.compose`
   - `https://www.googleapis.com/auth/calendar.readonly`
   - `https://www.googleapis.com/auth/calendar.events`

## Testing

```bash
# Add a Google account
dotnet run --project src/CalendarMcp.Cli/CalendarMcp.Cli.csproj -- add-google-account

# Test the account
dotnet run --project src/CalendarMcp.Cli/CalendarMcp.Cli.csproj -- test-account <account-id>

# List all accounts
dotnet run --project src/CalendarMcp.Cli/CalendarMcp.Cli.csproj -- list-accounts
```

## Security Notes

- Client secrets are stored in appsettings.json - consider environment variables for production
- OAuth tokens are stored in per-account directories for isolation
- Token refresh is handled automatically by the Google.Apis library
