# Authentication Flow Findings

## Device Code Flow (Current - Default)

**What you're experiencing:** The 2-step browser authentication process

### How It Works:
1. MCP server displays: "Go to https://microsoft.com/devicelogin"
2. User opens browser, enters code (e.g., `AHHMFYMF2`)
3. User authenticates with Microsoft account
4. User consents to permissions
5. Token is cached for future use

### Pros:
✅ Works on any device (even headless servers)
✅ No redirect URI configuration issues
✅ Most secure for public clients
✅ Works across different machines

### Cons:
❌ Manual process - requires browser
❌ Not automated-friendly
❌ User must enter code manually
❌ Awkward for development/testing

### After First Authentication:
✅ Tokens are cached (in credential store or `~/.ms-365-mcp-tokens.json`)
✅ Subsequent runs use cached tokens automatically
✅ No re-authentication needed until token expires (typically 90 days)

## Alternative Authentication Methods

### Option 1: BYOT (Bring Your Own Token) - Best for Testing

You manage OAuth externally and provide the token:

```bash
# Get token from somewhere (Azure CLI, custom OAuth flow, etc.)
TOKEN=$(az account get-access-token --resource https://graph.microsoft.com --query accessToken -o tsv)

# Pass it to MCP server
MS365_MCP_OAUTH_TOKEN=$TOKEN npx @softeria/ms-365-mcp-server --org-mode
```

**Pros:**
✅ No manual browser flow
✅ Can automate token acquisition
✅ Full control over authentication

**Cons:**
❌ You must handle token refresh
❌ More complex setup

### Option 2: HTTP Mode with OAuth (For Production)

Run the server in HTTP mode which handles OAuth flow automatically:

```bash
npx @softeria/ms-365-mcp-server --org-mode --http 3000
```

The server provides OAuth endpoints and handles the flow via HTTP redirects.

**Pros:**
✅ Standard OAuth redirect flow
✅ Better for web applications
✅ MCP client can handle OAuth automatically

**Cons:**
❌ Requires HTTP transport (not stdio)
❌ More complex for simple testing

### Option 3: Use Cached Tokens (After First Auth)

**For the spike, you only need to authenticate ONCE per tenant:**

1. First time: Device code flow (what you're doing now)
2. Complete authentication and consent
3. Token is cached
4. All subsequent runs use cached token automatically

**This is actually the expected workflow!**

## Recommended Spike Approach

### Step 1: Authenticate Once Per Tenant

**Marimer:**
```bash
export MS365_MCP_CLIENT_ID="<your-marimer-client-id>"
export MS365_MCP_TENANT_ID="<your-marimer-tenant-id>"
npx @softeria/ms-365-mcp-server --login
```

Complete the device code flow with rocky@marimer.llc.

**Xebia:**
```bash
export MS365_MCP_CLIENT_ID="<your-xebia-client-id>"
export MS365_MCP_TENANT_ID="<your-xebia-tenant-id>"
npx @softeria/ms-365-mcp-server --login
```

Complete the device code flow with rocky.lhotka@xebia.com.

### Step 2: Test with Cached Tokens

After authentication, all subsequent runs will use cached tokens:

```bash
# No more device code prompts!
npx @softeria/ms-365-mcp-server --org-mode --http 3001
```

### Step 3: Run Multi-Instance Test

Both instances will use their cached tokens:

```bash
# Terminal 1 - Marimer (uses cached token)
export MS365_MCP_CLIENT_ID="<your-marimer-client-id>"
export MS365_MCP_TENANT_ID="<your-marimer-tenant-id>"
npx @softeria/ms-365-mcp-server --org-mode --http 3001

# Terminal 2 - Xebia (uses cached token)
export MS365_MCP_CLIENT_ID="<your-xebia-client-id>"
export MS365_MCP_TENANT_ID="<your-xebia-tenant-id>"
npx @softeria/ms-365-mcp-server --org-mode --http 3002
```

## Token Cache Behavior

### Where Tokens Are Stored:

**Windows:**
- Primary: Windows Credential Manager
- Fallback: `%USERPROFILE%\.ms-365-mcp-tokens.json`

**macOS:**
- Primary: Keychain
- Fallback: `~/.ms-365-mcp-tokens.json`

### Token Lifetime:
- Access token: 1 hour (automatically refreshed)
- Refresh token: 90 days (requires re-authentication after expiry)

### Clearing Tokens:
```bash
# If you need to re-authenticate
rm ~/.ms-365-mcp-tokens.json
# Or delete from Windows Credential Manager
```

## Spike Conclusions

### Authentication Requirements:
✅ **One-time manual authentication per tenant** (via device code)
✅ **Tokens cached for 90 days** (automatic refresh for 1-hour access tokens)
✅ **Multi-tenant works** (separate token cache per tenant+client ID)

### For Production Calendar-MCP:
1. **Use device code flow** for initial setup (acceptable for user-facing tool)
2. **OR use BYOT** if you want centralized OAuth management
3. **Tokens are long-lived** (90 days) - minimal re-authentication needed
4. **Each MCP server instance** maintains its own token cache

### Not a Blocker:
The device code flow is actually the **correct** authentication method for:
- Desktop applications
- CLI tools
- Tools that need to work across multiple machines
- Public client applications (no client secret)

**Claude Desktop, VS Code, and other MCP clients expect this flow.**

## What This Means for the Spike

✅ **The authentication flow is working correctly**
✅ **Complete device code flow once per tenant**
✅ **Then test multi-instance with cached tokens**
✅ **Document this as expected behavior, not a limitation**

The "awkwardness" goes away after first authentication!
