# Multi-Tenant Token Caching Issue

## Problem Discovered

When using the **same client ID** for multiple tenants, Softeria's ms-365-mcp-server has a token caching conflict:

### Symptom:
After authenticating to Marimer tenant successfully, attempting to authenticate to Xebia tenant fails with:

```
Selected user account does not exist in tenant 'Marimer LLC' and cannot access 
the application '<marimer-client-id>' in that tenant.
```

### Root Cause:
The token cache keys on **client ID only**, not **client ID + tenant ID**. When you try to authenticate to Xebia:
1. Server looks for cached token by client ID
2. Finds Marimer token (same client ID)
3. Tries to use Marimer token for Xebia tenant
4. Fails with tenant mismatch error

## Workarounds

### Workaround 1: Use Separate Client IDs Per Tenant ✅ **RECOMMENDED**

Create a separate app registration for each tenant scenario:

#### Create Xebia-Specific App:
```bash
az ad app create \
  --display-name "Calendar-MCP-Xebia" \
  --sign-in-audience "AzureADMultipleOrgs" \
  --is-fallback-public-client true \
  --public-client-redirect-uris "http://localhost"
```

Get the app ID and add permissions:
```bash
XEBIA_APP_ID="<new-app-id>"

az ad app permission add --id $XEBIA_APP_ID \
  --api 00000003-0000-0000-c000-000000000000 \
  --api-permissions \
    e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope \
    570282fd-fa5c-430d-a7fd-fc8dc98a9dca=Scope \
    7b9103a5-4610-446b-9670-80643382c1fa=Scope \
    1ec239c2-d7c9-4623-a91a-a9775856bb36=Scope \
    465a38f9-76ea-45b9-9f34-9e8b0d4b0b42=Scope

az ad app permission admin-consent --id $XEBIA_APP_ID
```

#### Update Configuration:
```json
{
  "Accounts": {
    "Tenant1": {
      "TenantId": "<xebia-tenant-id>",
      "ClientId": "<xebia-app-id>",
      "DisplayName": "Xebia Work"
    },
    "Tenant2": {
      "TenantId": "<marimer-tenant-id>",
      "ClientId": "<marimer-app-id>",
      "DisplayName": "Marimer LLC"
    }
  }
}
```

**Pros:**
✅ Clean token separation
✅ No cache conflicts
✅ Each tenant has its own app
✅ Easier to manage permissions per tenant

**Cons:**
❌ Requires creating multiple app registrations
❌ More apps to manage

### Workaround 2: Clear Cache Between Tenant Authentications

For testing/spike purposes, manually clear cache:

```bash
# Authenticate to Marimer
export MS365_MCP_CLIENT_ID="<your-client-id>"
export MS365_MCP_TENANT_ID="<marimer-tenant-id>"
npx @softeria/ms-365-mcp-server --login

# Clear cache before switching tenants
rm ~/.ms-365-mcp-tokens.json

# Authenticate to Xebia
export MS365_MCP_TENANT_ID="<xebia-tenant-id>"
npx @softeria/ms-365-mcp-server --login
```

**Pros:**
✅ Uses single app registration
✅ Simple for testing

**Cons:**
❌ Cannot run simultaneously
❌ Tokens keep overwriting each other
❌ Not viable for production

### Workaround 3: Use Different Token Cache Locations

Set a custom token cache location per tenant:

```bash
# Marimer
export MS365_MCP_TOKEN_CACHE="$HOME/.ms365-tokens-marimer.json"
export MS365_MCP_TENANT_ID="<marimer-tenant-id>"
npx @softeria/ms-365-mcp-server --login

# Xebia  
export MS365_MCP_TOKEN_CACHE="$HOME/.ms365-tokens-xebia.json"
export MS365_MCP_TENANT_ID="<xebia-tenant-id>"
npx @softeria/ms-365-mcp-server --login
```

**Note:** This requires checking if Softeria supports custom token cache paths via environment variable.

### Workaround 4: Use BYOT (Bring Your Own Token)

Manage tokens externally and pass them in:

```bash
# Get Marimer token
MARIMER_TOKEN=$(az account get-access-token \
  --tenant <marimer-tenant-id> \
  --resource https://graph.microsoft.com \
  --query accessToken -o tsv)

# Get Xebia token
XEBIA_TOKEN=$(az account get-access-token \
  --tenant <xebia-tenant-id> \
  --resource https://graph.microsoft.com \
  --query accessToken -o tsv)

# Use tokens
MS365_MCP_OAUTH_TOKEN=$MARIMER_TOKEN npx @softeria/ms-365-mcp-server --org-mode --http 3001
MS365_MCP_OAUTH_TOKEN=$XEBIA_TOKEN npx @softeria/ms-365-mcp-server --org-mode --http 3002
```

**Pros:**
✅ Complete control over tokens
✅ Can run simultaneously
✅ No cache conflicts

**Cons:**
❌ Must handle token refresh
❌ Requires Azure CLI or custom OAuth flow
❌ More complex

## Recommended Solution for Calendar-MCP

**Use separate Client IDs per tenant:**

### Architecture Decision:
```
Calendar-MCP (Orchestration Layer)
├── Account Registry
│   ├── Xebia Account
│   │   ├── Client ID: <xebia-app-id>
│   │   ├── Tenant ID: <xebia-tenant-id>
│   │   └── MCP Server Instance (Port 3001)
│   └── Marimer Account  
│       ├── Client ID: <marimer-app-id>
│       ├── Tenant ID: <marimer-tenant-id>
│       └── MCP Server Instance (Port 3002)
```

### Benefits:
1. ✅ Clean token isolation
2. ✅ Per-tenant permission management
3. ✅ Can run multiple instances simultaneously
4. ✅ Each tenant can have different permission scopes
5. ✅ Easier to debug authentication issues

### Trade-off:
- Requires creating N app registrations for N tenants
- Acceptable for professional use case (consultants/contractors)
- Each tenant administrator can control their own app registration

## Implementation for Spike

For the spike, let's create separate app IDs:

1. **Marimer app** (already exists): `<marimer-app-id>`
2. **Xebia app** (create new): Use Azure CLI to create

This proves:
✅ Multi-tenant authentication works
✅ Multiple MCP instances can run simultaneously
✅ Token caching works (with separate client IDs)
✅ Validates the architecture approach

## Next Steps

1. Create Xebia-specific app registration
2. Authenticate both tenants with their respective client IDs
3. Run both MCP servers simultaneously (different ports)
4. Test querying both from C# orchestration layer
5. Document findings in spike report

## Spike Conclusion

**Finding:** Softeria's ms-365-mcp-server requires **separate client IDs for simultaneous multi-tenant operation**.

**Root Cause:** Token cache keys on client ID only (`${clientId}`), not client ID + tenant ID (`${clientId}_${tenantId}`). This creates authentication conflicts when reusing the same app registration across multiple tenants.

**Real-World Evidence from Testing:**

1. **First collision (Marimer + Personal Outlook):**
   - Used client ID `<marimer-client-id>` for both tenants
   - Authenticated Marimer tenant successfully
   - Attempted to authenticate Personal Outlook tenant
   - **Result:** Required logout/re-authentication, invalidated Marimer session
   - Cannot maintain simultaneous sessions with shared client ID

2. **Workaround validation:**
   - Created three separate app registrations:
     - `Calendar-MCP-Xebia` (<xebia-client-id>)
     - `Calendar-MCP-Marimer` (<marimer-client-id>)
     - `Calendar-MCP-Personal` (<personal-client-id>)
   - Each tenant configured with unique client ID
   - **Result:** Token isolation confirmed, no authentication conflicts

**Impact on Design:** This is actually a **good constraint** because it:
- Ensures proper token isolation per tenant
- Allows per-tenant permission management
- Prevents accidental cross-tenant data access
- Aligns with security best practices
- Forces explicit per-tenant configuration

**Recommendation:** Calendar-MCP should require users to create one app registration per tenant they want to integrate. This is reasonable for the target audience (professionals managing multiple organization accounts).

**Trade-offs Accepted:**
- ✅ Operational complexity: N app registrations for N tenants
- ✅ Permission management: Each app requires separate admin consent
- ✅ Configuration: Must track client ID per tenant
- ❌ Single-app approach: Not viable with current MCP server implementation

**Future Consideration:** File issue with @softeria/ms-365-mcp-server to update cache key strategy from `${clientId}` to `${clientId}_${tenantId}` for better multi-tenant support with shared app registrations.
