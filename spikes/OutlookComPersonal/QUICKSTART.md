# Quickstart: Outlook.com Personal Account Spike

## Prerequisites

- .NET 9 SDK
- Personal Microsoft account (@outlook.com, @hotmail.com, or @live.com)
- **Pre-configured Azure app registration**: `Calendar-MCP-Personal` (already set up in your Entra ID tenant)

## Existing App Registration

This spike uses an **existing app registration** that is already configured:

- **App Name:** Calendar-MCP-Personal
- **Client ID:** (configured in `appsettings.Development.json`)
- **Supported Accounts:** Personal and organizational Microsoft accounts
- **API:** Microsoft Graph (modern, recommended)
- **Permissions:** 
  - ✅ `Calendars.Read`
  - ✅ `Calendars.ReadWrite`
  - ✅ `Mail.Read`
  - ✅ `Mail.ReadWrite`
  - ✅ `Mail.Send`

**You can skip app registration setup and go directly to step 4!**

## Optional: Manual App Registration Setup

If you need to create a new app registration instead of using the existing one:

<details>
<summary>Click to expand manual setup instructions</summary>

### 1. Create Azure App Registration

1. Go to [Azure Portal](https://portal.azure.com) → Azure Active Directory → App registrations
2. Click "New registration"
3. Configure:
   - **Name:** "Outlook Personal Account Spike"
   - **Supported account types:** "Accounts in any organizational directory and personal Microsoft accounts"
   - **Redirect URI:** 
     - Platform: "Mobile and desktop applications"
     - URI: `http://localhost:5000`
4. Click "Register"
5. Note the **Application (client) ID**

### 2. Configure API Permissions

1. In your app registration, go to "API permissions"
2. Click "Add a permission"
3. Select "Microsoft Graph"
4. Select **Delegated permissions**:
   - ✅ `Calendars.ReadWrite`
   - ✅ `Calendars.Read`
   - ✅ `Mail.ReadWrite`
   - ✅ `Mail.Read`
   - ✅ `Mail.Send`
5. Click "Add permissions"

**Note:** Do NOT request admin consent - personal accounts don't need it.

### 3. Configure Authentication

1. In your app registration, go to "Authentication"
2. Under "Advanced settings", enable:
   - ✅ "Allow public client flows" = Yes
3. Under "Supported account types", ensure it's set to:
   - ✅ "Accounts in any organizational directory and personal Microsoft accounts"
4. Save changes

</details>

## Setup Steps

### 4. Set Up Local Configuration

The configuration is already set up in `appsettings.json` with the correct Client ID and Microsoft Graph settings. No additional configuration is needed!

If you want to customize settings, you can create an override file:

```bash
cd spikes/OutlookComPersonal
cp appsettings.Development.json.template appsettings.Development.json
```

The default configuration structure:
```json
{
  "MicrosoftGraph": {
    "ClientId": "<your-client-id-here>",
    "TenantId": "common",
    "RedirectUri": "http://localhost:5000",
    "Scopes": [
      "https://graph.microsoft.com/Calendars.ReadWrite",
      "https://graph.microsoft.com/Mail.ReadWrite",
      "https://graph.microsoft.com/Mail.Send",
      "offline_access"
    ],
    "ApiBaseUrl": "https://graph.microsoft.com/v1.0"
  }
}
```

### 5. Run the Spike

```bash
dotnet restore
dotnet build
dotnet run
```

### 6. Authenticate

1. A browser window will open
2. Sign in with your **personal Microsoft account** (@outlook.com, etc.)
3. Consent to the requested permissions
4. Return to the console to see results

## Expected Output

```
=== Outlook.com Personal Account Spike ===

Configuration loaded:
  Client ID: <your-client-id>
  Tenant: common
  Base URL: https://graph.microsoft.com/v1.0
  Scopes: https://graph.microsoft.com/Calendars.ReadWrite, ...

Step 1: Authenticating...
✅ Authentication successful!
   Token preview: eyJ0eXAiOiJKV1QiLCJ...

Step 2: Testing calendar access...
✅ Found 1 calendar(s)
   - Calendar (ID: AAMkAGI...)

Step 3: Listing calendar events...
✅ Found 3 event(s)
   - Team Meeting
     Start: 2025-12-05T10:00:00
     End: 2025-12-05T11:00:00
   ...

Step 4: Testing mail access...
✅ Found 5 message(s)
   - From: someone@example.com
     Subject: Hello World
     Received: 2025-12-04T08:30:00Z
   ...

=== Spike Completed Successfully ===
✅ Authentication: WORKING
✅ Calendar Read: WORKING
✅ Mail Read: WORKING
```

## Troubleshooting

### Error: "AADSTS700016: Application not found"
- Check that Client ID is correct
- Verify app registration exists in Azure Portal

### Error: "AADSTS650053: The application is disabled"
- Ensure "Allow public client flows" is enabled in Authentication settings

### Error: "401 Unauthorized"
- Check that API permissions were added correctly
- Verify scopes in appsettings.json use: `https://graph.microsoft.com/...`
- Clear token cache (see Clean Up section below) and try again

### Error: "AADSTS65001: User or administrator has not consented"
- This is expected on first run - consent during the browser authentication
- Make sure you're signing in with a personal Microsoft account (@outlook.com, @hotmail.com, @live.com)

### Error: "404 Not Found" or "Resource not found"
- Verify base URL is `https://graph.microsoft.com/v1.0` (not Outlook REST API)
- Check that the endpoint paths are correct for Microsoft Graph

### No browser opens for authentication
- Check that redirect URI is `http://localhost:5000`
- Verify firewall allows localhost connections
- Try running as administrator

## Next Steps

Once the spike runs successfully:

1. Test calendar write operations (create/update/delete events)
2. Test mail send operation
3. Document API response schemas
4. Explore Microsoft Graph API capabilities beyond basic read/write
5. Update [FINDINGS.md](FINDINGS.md) with results

## Key Differences: Microsoft Graph vs Outlook REST API

This spike uses **Microsoft Graph API** (the modern approach) instead of the deprecated Outlook REST API:

| Feature | Microsoft Graph | Outlook REST API (Deprecated) |
|---------|----------------|------------------------------|
| Base URL | `https://graph.microsoft.com/v1.0` | `https://outlook.office.com/api/v2.0` |
| Scopes | `https://graph.microsoft.com/...` | `https://outlook.office.com/...` |
| Status | ✅ Actively developed | ⚠️ Deprecated |
| Personal Accounts | ✅ Fully supported | ⚠️ Limited support |
| Property Names | camelCase (`id`, `subject`) | PascalCase (`Id`, `Subject`) |

**Why Microsoft Graph?**
- Modern, unified API for all Microsoft 365 services
- Better personal account support
- Active development and new features
- Consistent with other Microsoft SDKs

## Clean Up

To clear cached authentication tokens:
- Windows: Delete folder `%LOCALAPPDATA%\.IdentityService`
- macOS/Linux: Delete folder `~/.IdentityService`
