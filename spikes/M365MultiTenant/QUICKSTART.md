# Quick Start: Testing Softeria's ms-365-mcp-server

This guide walks you through testing the Softeria MCP server to validate multi-tenant M365 integration.

## Step 1: Install the Server

```bash
npm install -g @softeria/ms-365-mcp-server
```

Or test without installing:
```bash
npx @softeria/ms-365-mcp-server --help
```

## Step 2: Azure App Registration

You need an Azure AD app registration. Choose one of:

### Option A: Use Built-in App (Quickest)
Softeria includes a default app registration. Just run and authenticate:
```bash
npx @softeria/ms-365-mcp-server --org-mode
```

### Option B: Create Your Own Multi-Tenant App

1. Go to [Azure Portal](https://portal.azure.com/)
2. Navigate to **Azure Active Directory** → **App registrations** → **New registration**
3. **Name**: `Calendar-MCP-Test`
4. **Supported account types**: **Accounts in any organizational directory (Any Azure AD directory - Multitenant)**
5. **Redirect URI**: 
   - Type: **Mobile and desktop applications**
   - URI: `http://localhost` (or leave blank for device code flow)
6. Click **Register**

#### Configure Permissions:
1. Go to **API permissions** → **Add a permission** → **Microsoft Graph** → **Delegated permissions**
2. Add these permissions:
   - `Mail.Read`
   - `Mail.ReadWrite`
   - `Calendars.Read`
   - `Calendars.ReadWrite`
   - `User.Read`
   - `Files.Read`
   - `Files.ReadWrite`
   - For org mode, also add:
     - `Chat.Read`
     - `Chat.ReadWrite`
     - `Team.ReadBasic.All`
     - `ChannelMessage.Read.All`
3. Click **Grant admin consent** if available (or user consent during first login)

#### Get Credentials:
- Copy the **Application (client) ID**
- Copy the **Directory (tenant) ID** (or use `common` for multi-tenant)

## Step 3: Test Single Tenant

### Test with Built-in App (Easiest):
```bash
# Start server
npx @softeria/ms-365-mcp-server --org-mode

# The server will display MCP tools available
# Exit with Ctrl+C
```

### Test with Your App:
```bash
# Set your client ID
export MS365_MCP_CLIENT_ID="your-client-id-here"
export MS365_MCP_TENANT_ID="common"  # or specific tenant ID

# Start server in HTTP mode
npx @softeria/ms-365-mcp-server --org-mode --http 3000
```

### Test with MCP Inspector:
```bash
# Test the server interactively
npx @modelcontextprotocol/inspector npx -y @softeria/ms-365-mcp-server --org-mode
```

This opens a web interface where you can:
1. See all available tools
2. Call the `login` tool
3. Complete authentication
4. Test `list-calendars`, `list-mail-messages`, etc.

## Step 4: Test Multi-Tenant (Two Terminals)

### Terminal 1 - Xebia Tenant:
```bash
# Set credentials for Xebia
export MS365_MCP_CLIENT_ID="xebia-client-id"
export MS365_MCP_TENANT_ID="xebia-tenant-id"  # or "common"

# Start server on port 3001
npx @softeria/ms-365-mcp-server --org-mode --http 3001
```

### Terminal 2 - Marimer Tenant:
```bash
# Set credentials for Marimer
export MS365_MCP_CLIENT_ID="marimer-client-id"
export MS365_MCP_TENANT_ID="marimer-tenant-id"  # or "common"

# Start server on port 3002
npx @softeria/ms-365-mcp-server --org-mode --http 3002
```

### Terminal 3 - Test Both:
```bash
# Test Xebia instance
curl http://localhost:3001/mcp

# Test Marimer instance
curl http://localhost:3002/mcp
```

## Step 5: Test with Claude Desktop

Add to `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS) or `%APPDATA%\Claude\claude_desktop_config.json` (Windows):

```json
{
  "mcpServers": {
    "ms365-xebia": {
      "command": "npx",
      "args": ["-y", "@softeria/ms-365-mcp-server", "--org-mode"],
      "env": {
        "MS365_MCP_CLIENT_ID": "xebia-client-id",
        "MS365_MCP_TENANT_ID": "xebia-tenant-id"
      }
    },
    "ms365-marimer": {
      "command": "npx",
      "args": ["-y", "@softeria/ms-365-mcp-server", "--org-mode"],
      "env": {
        "MS365_MCP_CLIENT_ID": "marimer-client-id",
        "MS365_MCP_TENANT_ID": "marimer-tenant-id"
      }
    }
  }
}
```

Restart Claude Desktop and try:
- "List my calendars from Xebia"
- "List my calendars from Marimer"
- "Show me all my emails from both accounts"

## Step 6: Authentication Flow

The first time you use the server, it will:

1. **Prompt for authentication**:
   ```
   To sign in, use a web browser to open the page https://microsoft.com/devicelogin
   and enter the code ABC123XYZ to authenticate.
   ```

2. **Open browser** and visit the URL
3. **Enter the code** shown
4. **Sign in** with your Microsoft account
5. **Consent** to the requested permissions

Tokens are cached in your OS credential store (macOS Keychain, Windows Credential Manager, or fallback to `~/.ms-365-mcp-tokens.json`).

## Step 7: Test MCP Tools

Once authenticated, test these tools via MCP Inspector or Claude:

### Calendar Tools:
- `list-calendars` - See all calendars
- `list-calendar-events` - See events in calendar
- `create-calendar-event` - Create a test event

### Email Tools:
- `list-mail-folders` - See folder structure
- `list-mail-messages` - See inbox messages
- `get-mail-message` - Get specific email with body

### Teams Tools (org-mode only):
- `list-joined-teams` - See your teams
- `list-team-channels` - See channels in a team
- `list-chats` - See your chats

## Step 8: Verify Multi-Tenant Success

You'll know multi-tenant works when:
- ✅ Both server instances start without errors
- ✅ Each instance authenticates to its own tenant
- ✅ Tokens are cached separately
- ✅ You can query calendars/emails from both tenants
- ✅ Results are independent and correct

## Troubleshooting

### "Need admin approval" error:
```bash
# Use tenant ID instead of "common"
export MS365_MCP_TENANT_ID="your-specific-tenant-id"
```

### "Invalid client" error:
- Verify CLIENT_ID is correct
- Check app registration still exists
- Ensure app supports device code flow (public client)

### Port already in use:
```bash
# Find and kill process
# Windows:
netstat -ano | findstr :3001
taskkill /PID <PID> /F

# macOS/Linux:
lsof -i :3001
kill -9 <PID>
```

### Tokens not refreshing:
```bash
# Clear token cache and re-authenticate
rm ~/.ms-365-mcp-tokens.json
# Or on Windows:
# Delete cached credentials in Credential Manager
```

## Next Steps

Once you've validated:
1. ✅ Can connect to multiple tenants
2. ✅ Can query calendars from each
3. ✅ Can query emails from each
4. ✅ Authentication works reliably
5. ✅ Tokens refresh automatically

Then proceed to:
- **Google Workspace spike** (similar process with google-workspace-mcp)
- **Integrate with .NET spike** (McpServerLauncher.cs)
- **Build orchestration layer** (Account Registry + Smart Router)

## Resources

- [Softeria ms-365-mcp-server GitHub](https://github.com/Softeria/ms-365-mcp-server)
- [MCP Specification](https://spec.modelcontextprotocol.io/)
- [Microsoft Graph API Docs](https://docs.microsoft.com/en-us/graph/)
- [Azure App Registration Guide](https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)
