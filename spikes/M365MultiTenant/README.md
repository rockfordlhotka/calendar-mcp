# M365 Multi-Tenant Spike

This spike validates the ability to connect to multiple Microsoft 365 tenants simultaneously using the microsoft-mcp server.

## Objectives

1. **Test single tenant connection** - Verify connection to Tenant 1
2. **Test second tenant connection** - Verify connection to Tenant 2
3. **Test simultaneous access** - Verify both tenants can be accessed in parallel

## Prerequisites

### 1. Choose and Install a Microsoft MCP Server

Three real implementations are available:

#### Option A: Softeria's ms-365-mcp-server (TypeScript - **RECOMMENDED**)
Most mature and feature-complete:
```bash
npm install -g @softeria/ms-365-mcp-server
```

#### Option B: hvkshetry's office-365-mcp-server (JavaScript)
Comprehensive with good documentation:
```bash
git clone https://github.com/hvkshetry/office-365-mcp-server
cd office-365-mcp-server
npm install
```

#### Option C: elyxlz's microsoft-mcp (Python)
Minimal but powerful:
```bash
git clone https://github.com/elyxlz/microsoft-mcp.git
cd microsoft-mcp
uv sync
```

### 2. Azure AD App Registrations

Create Azure AD app registrations for each tenant:

#### For Each Tenant:
1. Go to Azure Portal → Azure Active Directory → App registrations
2. Click "New registration"
3. Name: "Calendar MCP Spike - [Tenant Name]"
4. Supported account types: "Accounts in this organizational directory only"
5. Redirect URI: (leave blank for now, will configure based on microsoft-mcp requirements)
6. Click "Register"

#### Configure API Permissions:
Add the following Microsoft Graph permissions:
- `Calendars.Read`
- `Calendars.ReadWrite`
- `Mail.Read`
- `Mail.ReadWrite`
- `User.Read`

Grant admin consent for the permissions.

#### Get Credentials:
- Copy the **Application (client) ID**
- Copy the **Directory (tenant) ID**
- Create a client secret (if required by microsoft-mcp)

### 3. Configure appsettings

Copy `appsettings.Development.json.template` to `appsettings.Development.json`:

```bash
cd spikes/M365MultiTenant
cp appsettings.Development.json.template appsettings.Development.json
```

Edit `appsettings.Development.json` and add your actual tenant IDs and client IDs:

```json
{
  "Accounts": {
    "Tenant1": {
      "TenantId": "your-actual-tenant1-id",
      "ClientId": "your-actual-tenant1-client-id",
      "ClientSecret": "your-client-secret-here",
      "DisplayName": "Work Account 1"
    },
    "Tenant2": {
      "TenantId": "your-actual-tenant2-id",
      "ClientId": "your-actual-tenant2-client-id",
      "ClientSecret": "your-client-secret-here",
      "DisplayName": "Work Account 2"
    }
  },
  "McpServer": {
    "Type": "softeria",
    "Command": "npx",
    "Args": ["-y", "@softeria/ms-365-mcp-server", "--org-mode"]
  }
}
```

## Running the Spike

### Option 1: Using Visual Studio

1. Open `CalendarMcp.sln` in the root directory
2. Set `M365MultiTenant` as the startup project
3. Press F5 to run

### Option 2: Using .NET CLI

```bash
cd spikes/M365MultiTenant
dotnet restore
dotnet build
dotnet run
```

## Expected Output

```
===========================================
Starting M365 Multi-Tenant Spike
===========================================

TEST 1: Testing Tenant 1 connection...
Starting MCP server for account: tenant1-work (Work Account 1)
  Tenant ID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
  Client ID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
  Port: 3000
✓ MCP server started for tenant1-work
  Testing calendar access for tenant1-work...
  ✓ Calendar access test completed for tenant1-work
  Testing email access for tenant1-work...
  ✓ Email access test completed for tenant1-work
Stopping MCP server for tenant1-work
✓ Tenant 1 test completed

TEST 2: Testing Tenant 2 connection...
[Similar output for Tenant 2]

TEST 3: Testing simultaneous multi-tenant access...
[Output showing both servers running in parallel]

===========================================
✓ Spike completed successfully
===========================================
```

## Success Criteria

- ✅ Can launch microsoft-mcp server with Tenant 1 credentials
- ✅ Can launch microsoft-mcp server with Tenant 2 credentials
- ✅ Can run both servers simultaneously on different ports
- ✅ Can make parallel requests to both servers
- ✅ Authentication works for both tenants independently

## Findings to Document

As you run the spike, document:

1. **Authentication Flow**: How does microsoft-mcp handle OAuth? Interactive? Device code? Certificate?
2. **Token Management**: How are tokens stored and refreshed?
3. **Multi-Instance Support**: Can multiple instances run simultaneously?
4. **Performance**: What is the latency for calendar/email queries?
5. **Limitations**: Any rate limits or restrictions discovered?
6. **API Coverage**: Which Microsoft Graph APIs are supported?

## Next Steps

Based on the results:

1. If successful → Proceed with Google Workspace spike
2. If issues found → Evaluate alternative microsoft-mcp servers (Softeria's ms-365-mcp-server)
3. Document architectural decisions for the main calendar-mcp server

## Troubleshooting

### MCP Server not found
**For Softeria:**
```bash
npm install -g @softeria/ms-365-mcp-server
# Or use npx (no install needed)
npx @softeria/ms-365-mcp-server --help
```

**For hvkshetry:**
```bash
git clone https://github.com/hvkshetry/office-365-mcp-server
cd office-365-mcp-server && npm install
```

**For elyxlz (Python):**
```bash
git clone https://github.com/elyxlz/microsoft-mcp.git
cd microsoft-mcp && uv sync
```

### Authentication failures
- Verify tenant IDs and client IDs are correct
- Check Azure AD app registration permissions
- Ensure admin consent has been granted

### Port conflicts
- Change the port numbers in the test methods
- Check if ports 3000-3002 are available

## References

- [Softeria ms-365-mcp-server](https://github.com/Softeria/ms-365-mcp-server) - **Recommended**
- [hvkshetry office-365-mcp-server](https://github.com/hvkshetry/office-365-mcp-server)
- [elyxlz microsoft-mcp (Python)](https://github.com/elyxlz/microsoft-mcp)
- [Microsoft Graph API Documentation](https://docs.microsoft.com/en-us/graph/)
- [Model Context Protocol Specification](https://spec.modelcontextprotocol.io/)
