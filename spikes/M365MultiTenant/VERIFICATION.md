# Multi-Tenant Authentication Verification

## What You Just Did

You ran the MCP server for both tenants without visible errors - that's a **good sign**! ✅

## What "No Error" Means

When the MCP server starts without errors, it typically means:
1. ✅ The server initialized successfully
2. ✅ It's ready to authenticate OR already has cached tokens
3. ✅ No configuration issues (client ID, tenant ID are valid)

## Next: Verify It Actually Works

### Quick Test: Use MCP Inspector

The MCP Inspector provides a web UI to test MCP servers:

```bash
# Test Tenant1
export MS365_MCP_CLIENT_ID="<your-tenant1-client-id>"
export MS365_MCP_TENANT_ID="<your-tenant1-tenant-id>"
npx @modelcontextprotocol/inspector npx -y @softeria/ms-365-mcp-server --org-mode
```

This will:
1. Open a web browser with the MCP Inspector UI
2. Show all available tools
3. Let you call `login` tool if needed
4. Let you test `list-calendars`, `list-mail-messages`, etc.

### Better Test: Run Both in HTTP Mode

Running in HTTP mode makes it easier to see what's happening:

**Terminal 1 - Tenant1:**
```bash
export MS365_MCP_CLIENT_ID="<your-tenant1-client-id>"
export MS365_MCP_TENANT_ID="<your-tenant1-tenant-id>"
npx @softeria/ms-365-mcp-server --org-mode --http 3001
```

**Terminal 2 - Tenant2:**
```bash
export MS365_MCP_CLIENT_ID="<your-tenant2-client-id>"
export MS365_MCP_TENANT_ID="<your-tenant2-tenant-id>"
npx @softeria/ms-365-mcp-server --org-mode --http 3002
```

Look for output like:
```
Server started on port 3001
Authentication: Device code flow
[INFO] Token cache found - using cached credentials
[INFO] Authenticated as: user@example.com
```

### Check Authentication Status

You can check if you're authenticated by looking for:

**Windows:** Check Windows Credential Manager
- Press Win+R, type `control /name Microsoft.CredentialManager`
- Look for entries related to "ms-365-mcp" or "Microsoft Graph"

**Or check token file:**
```bash
# May be in one of these locations
cat ~/.ms-365-mcp-tokens.json
cat %USERPROFILE%\.ms-365-mcp-tokens.json
```

## What Success Looks Like

### ✅ Tenant1 (Admin approved app):
- Server starts immediately
- No authentication prompts (tokens cached)
- Can call tools like `list-calendars` successfully

### ✅ Tenant2 (Depends on user consent policy):

**Scenario A: User consent allowed**
- Server starts
- First time: Device code prompt → authenticate → consent → done
- Subsequent times: Uses cached tokens

**Scenario B: Admin approval required**
- Server starts
- First tool call shows error: "AADSTS65001: User or administrator has not consented"
- You need to send consent request to Tenant2 IT admin

## Spike Success Criteria

The spike is successful if you can:
1. ✅ Start MCP server for Tenant1
2. ✅ Start MCP server for Tenant2 (even if needs IT approval)
3. ✅ Call `list-calendars` on Tenant1 and get your calendars
4. ✅ Run both servers simultaneously on different ports
5. Document the authentication flow and any limitations

## Run the .NET Spike

Once you've verified authentication works, test the C# spike:

```bash
cd spikes/M365MultiTenant
dotnet run
```

This will test the orchestration layer managing multiple MCP server instances.

## Troubleshooting

**If you see "Need admin approval" for Tenant2:**
1. This is expected if Tenant2 restricts user consent
2. The app still works - you just need IT approval
3. Document this as a limitation for the spike
4. For the spike purposes, having Tenant1 working proves the concept

**If servers exit immediately:**
- Check the logs for error messages
- Verify Client ID and Tenant ID are correct
- Try with `--verbose` flag for more details

## Document Your Findings

Create a findings document with:
- ✅ What worked (Tenant1 authentication)
- ⚠️  What needs approval (Tenant2 if applicable)
- ✅ Multi-instance capability (can run both simultaneously)
- 📊 Performance observations (startup time, token caching)
- 🔧 Integration notes (how to call from C#)

Ready to proceed with the full testing?
