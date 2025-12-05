# Findings: Outlook.com Personal Account Integration

## Investigation Log

### Session 1: Initial Research (2025-12-04)

**Goal:** Determine the most viable approach for accessing personal MSA outlook.com calendar and mail.

**Key Questions:**
1. Is Outlook REST API v2.0 still functional despite deprecation?
2. What are the exact authentication requirements?
3. What are the API endpoint patterns?
4. How does it compare to Microsoft Graph API?

---

## Outlook REST API v2.0 Status

### Current State (December 2025)

**Official Status:** 
- ❓ Need to verify deprecation timeline
- ❓ Check if still accepting new app registrations
- ❓ Confirm current functionality

**Documentation:**
- Microsoft has moved docs to "previous versions" archive
- Original URL: https://docs.microsoft.com/en-us/previous-versions/office/office-365-api/

### Authentication Requirements

**OAuth 2.0 Flow:**
```
Authorization Endpoint: https://login.microsoftonline.com/common/oauth2/v2.0/authorize
Token Endpoint: https://login.microsoftonline.com/common/oauth2/v2.0/token
```

**Required Scopes:**
- Calendar: `https://outlook.office.com/Calendars.Read`, `https://outlook.office.com/Calendars.ReadWrite`
- Mail: `https://outlook.office.com/Mail.Read`, `https://outlook.office.com/Mail.ReadWrite`

**Tenant ID:** `common` (supports both organizational and personal accounts)

### API Endpoints

**Base URL:** `https://outlook.office.com/api/v2.0`

**Calendar Operations:**
```
GET    /me/calendars                    # List calendars
GET    /me/calendar/events              # List events
POST   /me/calendar/events              # Create event
GET    /me/calendar/events/{id}         # Get event
PATCH  /me/calendar/events/{id}         # Update event
DELETE /me/calendar/events/{id}         # Delete event
```

**Mail Operations:**
```
GET    /me/mailfolders/inbox/messages   # List inbox messages
POST   /me/sendmail                     # Send message
GET    /me/messages/{id}                # Get message
POST   /me/messages                     # Create draft
POST   /me/messages/{id}/reply          # Reply to message
```

---

## Next Investigation Steps

1. ⏳ Set up Azure app registration with Outlook REST API scopes
2. ⏳ Implement authentication proof-of-concept
3. ⏳ Test calendar read operation
4. ⏳ Test calendar write operation
5. ⏳ Compare API responses with Graph API format
6. ⏳ Document any limitations or issues

---

## Comparison with Graph API

### Similarities
- REST/JSON architecture
- OAuth 2.0 authentication
- Similar endpoint patterns (`/me/calendar/events`)
- Bearer token authentication

### Differences
- Different base URL (`outlook.office.com` vs `graph.microsoft.com`)
- Different scope format (`https://outlook.office.com/...` vs `Calendars.ReadWrite`)
- Different API version path (`/api/v2.0` vs `/v1.0`)
- May have different response schemas

---

## Decision Points

### Critical Questions to Answer:
1. ✅ Can we authenticate successfully?
2. ✅ Can we read calendar events?
3. ✅ Can we write calendar events?
4. ✅ Are response formats manageable?
5. ✅ Is the API stable enough for production use?

### Risk Assessment:
- **Deprecation Risk:** Medium - API deprecated but functional
- **Maintenance Risk:** Low - REST API unlikely to break
- **Migration Risk:** Medium - May need to switch to Graph if/when supported
- **Implementation Risk:** Low - Standard REST API patterns

---

## Recommendations

**Status:** TBD after initial testing

**If Outlook REST API works:**
- ✅ Use for personal account support
- ✅ Implement abstraction layer for future migration
- ✅ Document deprecation risk for users
- ✅ Monitor Microsoft announcements

**If Outlook REST API doesn't work:**
- 🔄 Evaluate EWS as fallback
- 🔄 Consider limiting to organizational accounts only
- 🔄 Document limitation clearly

---

## Open Questions

1. What is the actual sunset date for Outlook REST API?
2. Does Microsoft plan to enable Graph API for personal account calendars?
3. Are there rate limits different from Graph API?
4. What is the token cache/refresh behavior?
5. Are there any undocumented limitations?

---

## References

- [Previous Versions: Outlook REST API](https://docs.microsoft.com/en-us/previous-versions/office/office-365-api/)
- [Microsoft Identity Platform OAuth 2.0](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-auth-code-flow)
