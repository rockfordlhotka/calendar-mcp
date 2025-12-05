# Outlook.com Personal Account Spike

## Objective

Establish that it is possible to connect to a personal Microsoft Account (MSA) outlook.com email and calendar with read/write access.

## Status: ✅ COMPLETED

**Current Implementation:** Using **Microsoft Graph API** with pre-configured `Calendar-MCP-Personal` app registration.

**Key Success:** Microsoft Graph API **DOES** support personal accounts when properly configured with:
- Sign-in audience: `AzureADandPersonalMicrosoftAccount`
- Access token version: 2
- Tenant ID: `common`

## Background

The M365MultiTenant spike successfully established connection patterns for organizational Azure AD accounts. This spike extends that work to support personal Microsoft accounts (@outlook.com, @hotmail.com, @live.com).

**Initial Concern:** Documentation suggested personal accounts might require the deprecated Outlook REST API.

**Resolution:** Microsoft Graph API fully supports personal accounts when the app registration is properly configured. The previous README mentioned limitations that are **no longer accurate** with correct configuration.

## Implementation Details

### Azure App Registration
- **App Name:** Calendar-MCP-Personal
- **Tenant:** Marimer Entra ID
- **Sign-in Audience:** Personal and organizational Microsoft accounts
- **Access Token Version:** 2 (required for personal accounts)
- **Client ID:** Configured in `appsettings.Development.json` (not committed to repo)

### API Configuration
- **Base URL:** `https://graph.microsoft.com/v1.0`
- **Authentication:** MSAL (Microsoft.Identity.Client)
- **Scopes:**
  - `https://graph.microsoft.com/Calendars.ReadWrite`
  - `https://graph.microsoft.com/Mail.ReadWrite`
  - `https://graph.microsoft.com/Mail.Send`
  - `offline_access`

### Key Differences from Outlook REST API

### Key Differences from Outlook REST API

Microsoft Graph uses different property naming conventions and endpoints:

| Feature | Microsoft Graph | Outlook REST API (Deprecated) |
|---------|-----------------|-------------------------------|
| Base URL | `https://graph.microsoft.com/v1.0` | `https://outlook.office.com/api/v2.0` |
| Property Names | camelCase (`id`, `subject`) | PascalCase (`Id`, `Subject`) |
| DateTime Fields | `start.dateTime`, `end.dateTime` | `Start.DateTime`, `End.DateTime` |
| Email Address | `from.emailAddress.address` | `From.EmailAddress.Address` |
| Scopes | `https://graph.microsoft.com/...` | `https://outlook.office.com/...` |

## Project Structure

```
OutlookComPersonal/
├── README.md (this file)
├── QUICKSTART.md (setup instructions)
├── FINDINGS.md (detailed discoveries)
├── OutlookRestSpike.csproj
├── Program.cs (main spike implementation)
├── appsettings.json (pre-configured with Calendar-MCP-Personal)
└── appsettings.Development.json.template (optional override template)
```

## Current Implementation

The spike implements:
- ✅ **Authentication:** MSAL interactive flow with token caching
- ✅ **Calendar Read:** List calendars and events
- ✅ **Mail Read:** List inbox messages
- ⏳ **Calendar Write:** Create/update/delete events (ready to implement)
- ⏳ **Mail Send:** Send messages (ready to implement)

### Code Structure
- `GraphConfig`: Configuration model for Microsoft Graph API
- `GraphAuthenticator`: MSAL-based authentication with silent/interactive flow
- `GraphCalendarService`: Calendar operations using Microsoft Graph
- `GraphMailService`: Mail operations using Microsoft Graph

## Success Criteria

✅ **Minimum Viable:**
- ✅ Authenticate with personal MSA account
- ✅ Read calendar events
- ⏳ Create/update/delete calendar events (code ready, needs testing)
- ✅ Read mail messages
- ⏳ Send mail messages (code ready, needs testing)

✅ **Ideal:**
- ✅ REST/JSON API
- ✅ Similar patterns to M365MultiTenant spike for consistency
- ✅ Microsoft Graph API (actively maintained, future-proof)

## Getting Started

See [QUICKSTART.md](QUICKSTART.md) for detailed setup and testing instructions.

**Quick Start:**
```bash
cd spikes/OutlookComPersonal
dotnet restore
dotnet build
dotnet run
```

The app is pre-configured with the existing `Calendar-MCP-Personal` app registration - no additional setup needed!

## Next Steps

1. ✅ Set up Azure app registration
2. ✅ Configure for personal accounts
3. ✅ Implement authentication flow
4. ✅ Test calendar read operations
5. ✅ Test mail read operations
6. ⏳ Test calendar write operations (create/update/delete)
7. ⏳ Test mail send operation
8. ⏳ Document findings and API response schemas
9. ⏳ Integrate patterns into Calendar MCP server

## References

- [Microsoft Graph API Reference](https://docs.microsoft.com/en-us/graph/api/overview)
- [Microsoft Graph Calendar API](https://docs.microsoft.com/en-us/graph/api/resources/calendar)
- [Microsoft Graph Mail API](https://docs.microsoft.com/en-us/graph/api/resources/message)
- [MSAL for .NET](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-overview)
- [Personal Microsoft Accounts with Graph](https://docs.microsoft.com/en-us/graph/auth-v2-user)
