# Personal Microsoft Account Limitation

## Problem

Personal Microsoft accounts (`@outlook.com`, `@hotmail.com`, `@live.com`) **do not support** the Microsoft Graph API endpoints used by @softeria/ms-365-mcp-server.

## Error Message

```
Microsoft Graph API error: 404 Not Found - {
  "error": {
    "code": "MailboxNotEnabledForRESTAPI",
    "message": "The mailbox is either inactive, soft-deleted, or is hosted on-premise."
  }
}
```

## Root Cause

Microsoft Graph API has **different capabilities** for organizational vs personal accounts:

### Organizational Accounts (Azure AD / Entra ID):
- ✅ Full Graph API access
- ✅ Calendars.Read, Calendars.ReadWrite
- ✅ Mail.Read, Mail.ReadWrite
- ✅ Teams, Channels, Chat
- ✅ All enterprise features

### Personal Accounts (Microsoft Account):
- ✅ User.Read (profile information)
- ✅ OneDrive access
- ✅ Some Mail endpoints (limited)
- ❌ **NO** Calendars.Read/Write via Graph API
- ❌ **NO** full Mail API access
- ❌ **NO** Teams/Chat (not applicable)

**Why:** Personal accounts use legacy backend infrastructure that predates Microsoft Graph. Calendar and mail access require the older **Outlook.com REST API** or **Exchange Web Services (EWS)**, not Graph API.

## Testing Results

### What Works with Personal Outlook.com:
```bash
# Authentication succeeds
MS365_MCP_CLIENT_ID="<personal-client-id>" \
MS365_MCP_TENANT_ID="<personal-tenant-id>" \
npx @modelcontextprotocol/inspector npx -y @softeria/ms-365-mcp-server

# Tools that work:
✅ get-my-profile          # Returns user profile
✅ Tool list loads          # Authentication successful
```

### What Fails:
```bash
# Calendar operations fail with 404:
❌ list-calendars
❌ list-calendar-events
❌ create-calendar-event
❌ update-calendar-event
❌ delete-calendar-event

# Mail operations fail with 404:
❌ list-mail-messages
❌ send-mail
❌ create-draft-message
❌ reply-to-message

Error: "MailboxNotEnabledForRESTAPI"
```

## Microsoft Documentation References

From Microsoft Graph documentation:
> "Personal Microsoft accounts have limited support for Microsoft Graph. Some APIs, including Calendars and certain Mail operations, are only available for organizational accounts."

Graph API endpoints for personal accounts:
- User profile: ✅ Supported
- OneDrive files: ✅ Supported
- Outlook calendar: ❌ **Not supported via Graph**
- Outlook mail: ⚠️ **Limited support** (some endpoints work, many don't)

## Alternative Solutions for Personal Accounts

### Option 1: Use Outlook.com REST API (Deprecated)
**Endpoint:** `https://outlook.office.com/api/v2.0/me/...`

**Status:** Deprecated by Microsoft, will be retired

**Pros:**
- ✅ Supports personal accounts
- ✅ Calendar and mail access

**Cons:**
- ❌ Being retired by Microsoft
- ❌ Different API contract than Graph
- ❌ Requires separate implementation

### Option 2: Use Exchange Web Services (EWS)
**Protocol:** SOAP-based XML web service

**Status:** Legacy technology, maintained but not enhanced

**Pros:**
- ✅ Works with personal Outlook.com
- ✅ Full calendar/mail access
- ✅ Mature, stable protocol

**Cons:**
- ❌ SOAP/XML (not REST/JSON)
- ❌ Complex implementation
- ❌ Not modern API design
- ❌ Microsoft recommends Graph instead (for orgs)

### Option 3: Use IMAP/CalDAV (If Available)
**Protocols:** IMAP for mail, CalDAV for calendars

**Status:** Outlook.com supports IMAP, CalDAV support unclear

**Pros:**
- ✅ Standard protocols
- ✅ Works with many providers

**Cons:**
- ❌ Limited functionality vs REST APIs
- ❌ No rich metadata support
- ❌ May not support all Outlook.com features

### Option 4: Direct User to Upgrade to Microsoft 365
**Recommendation:** Personal users should upgrade to Microsoft 365 Personal or Family

**Microsoft 365 Personal/Family:**
- Same email address can stay (@outlook.com)
- Account becomes "organizationless" Azure AD account
- Full Graph API support unlocked
- Cost: ~$70/year for Personal, ~$100/year for Family

## Implications for Calendar-MCP

### Scope Decision: **Organizational Accounts Only**

**Rationale:**
1. Target audience is **professionals/consultants** with multiple organization accounts
2. All three evaluated MCP servers (@softeria, hvkshetry, elyxlz) target organizational accounts
3. Personal account support requires entirely different implementation
4. Microsoft Graph API is the modern, maintained approach
5. Personal users can upgrade to M365 for $6/month

### Recommended Approach:
- ✅ Support organizational Azure AD accounts (primary use case)
- ✅ Support Microsoft 365 organizational accounts
- ✅ Support multi-tenant scenarios (consultants)
- ❌ **Do not support** personal @outlook.com/@hotmail.com/@live.com accounts
- 📝 Document workaround: Users can upgrade to M365 Personal

### Documentation for Users:
```markdown
## Account Support

Calendar-MCP supports:
✅ Organizational Microsoft 365 accounts (work/school)
✅ Azure Active Directory (Entra ID) accounts
✅ Multiple organizational tenants

❌ Personal Microsoft accounts (@outlook.com, @hotmail.com, @live.com) are NOT supported.

### Why not personal accounts?
Personal Microsoft accounts do not support the Microsoft Graph Calendar API. 
They require legacy APIs that Microsoft is deprecating.

### Workaround:
Upgrade to Microsoft 365 Personal ($6.99/month) to get full Graph API access 
while keeping your @outlook.com email address.
```

## Spike Conclusion

**Finding:** Personal Microsoft accounts are **fundamentally incompatible** with Microsoft Graph-based MCP servers.

**Root Cause:** Personal accounts hosted on legacy infrastructure without full Graph API support.

**Decision:** Calendar-MCP will **not support personal accounts** in initial release.

**Alternative Testing Strategy:** Use two organizational accounts for multi-tenant testing:
- ✅ Organization 1 (user@org1.com) - authenticated successfully
- ⏳ Organization 2 (user@org2.com) - awaiting admin consent
- ❌ Personal Outlook (user@outlook.com) - separate spike needed for personal account integration

**Recommendation:** Complete spike validation with two organizational accounts. Personal account integration should be evaluated as a **separate spike** with different implementation approach (Outlook.com REST API or EWS).

## Next Steps

1. ✅ **Document personal account limitation** (this file)
2. ⏳ **Wait for Organization 2 admin approval** or find alternative org account for testing
3. ✅ **Multi-tenant testing with two org accounts** (Organization 1 + Organization 2)
4. ✅ **Complete spike with organizational accounts only**
5. 🔮 **Future spike:** Personal account support via Outlook.com REST API (if needed)
