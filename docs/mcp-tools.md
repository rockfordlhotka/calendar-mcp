# MCP Tools

## Overview

Calendar-MCP exposes tools through the Model Context Protocol (MCP) that AI assistants can use to query and manage emails and calendars across multiple accounts.

## Core Tools

### Account Management

#### `list_accounts`
Get list of all configured accounts across all providers.

**Parameters**: None

**Returns**:
```json
{
  "accounts": [
    {
      "id": "xebia-work",
      "displayName": "Xebia Work Account",
      "provider": "microsoft365",
      "domains": ["xebia.com"],
      "enabled": true
    }
  ]
}
```

### Email Operations

#### `get_emails`
Get emails (unread/read, filtered by count) for specific account or all accounts.

**Parameters**:
- `accountId` (optional): Specific account ID, or omit for all accounts
- `count` (default: 20): Number of emails to retrieve
- `unreadOnly` (default: false): Only return unread emails

**Returns**:
```json
{
  "emails": [
    {
      "id": "msg-123",
      "accountId": "xebia-work",
      "subject": "Project Update",
      "from": "colleague@xebia.com",
      "receivedDateTime": "2025-12-04T10:30:00Z",
      "isRead": false,
      "hasAttachments": true
    }
  ]
}
```

#### `search_emails`
Search emails by sender/subject/criteria for specific account or all accounts.

**Parameters**:
- `accountId` (optional): Specific account ID, or omit for all accounts
- `query`: Search query string
- `count` (default: 20): Max results
- `fromDate` (optional): Start date for search range
- `toDate` (optional): End date for search range

**Returns**: Same format as `get_emails`

#### `get_email_details`
Get full email content including body and attachments.

**Parameters**:
- `accountId`: Specific account ID (required)
- `emailId`: Email message ID (required)

**Returns**:
```json
{
  "id": "msg-123",
  "accountId": "xebia-work",
  "subject": "Project Update",
  "from": "colleague@xebia.com",
  "to": ["me@xebia.com"],
  "cc": [],
  "body": "Full email body content...",
  "bodyFormat": "html",
  "receivedDateTime": "2025-12-04T10:30:00Z",
  "attachments": [
    {
      "name": "report.pdf",
      "size": 524288,
      "contentType": "application/pdf"
    }
  ]
}
```

#### `send_email`
Send email from specific account (requires explicit account selection or smart routing).

**Parameters**:
- `accountId` (optional): Specific account, or let router decide
- `to`: Recipient email address (can be array)
- `subject`: Email subject
- `body`: Email body content
- `bodyFormat` (default: "html"): "html" or "text"
- `cc` (optional): CC recipients
- `attachments` (optional): Attachments to include

**Returns**:
```json
{
  "success": true,
  "messageId": "sent-msg-456",
  "accountUsed": "xebia-work"
}
```

### Calendar Operations

#### `list_calendars`
List all calendars from specific account or all accounts.

**Parameters**:
- `accountId` (optional): Specific account ID, or omit for all accounts

**Returns**:
```json
{
  "calendars": [
    {
      "id": "cal-123",
      "accountId": "xebia-work",
      "name": "Calendar",
      "owner": "me@xebia.com",
      "canEdit": true,
      "isDefault": true
    }
  ]
}
```

#### `get_calendar_events`
Get events (past/present/future) for specific account or all accounts.

**Parameters**:
- `accountId` (optional): Specific account ID, or omit for all accounts
- `calendarId` (optional): Specific calendar ID
- `startDate`: Start of date range (ISO 8601)
- `endDate`: End of date range (ISO 8601)
- `count` (default: 50): Max events to return

**Returns**:
```json
{
  "events": [
    {
      "id": "evt-123",
      "accountId": "xebia-work",
      "calendarId": "cal-123",
      "subject": "Team Meeting",
      "start": "2025-12-05T14:00:00Z",
      "end": "2025-12-05T15:00:00Z",
      "location": "Conference Room A",
      "attendees": ["colleague@xebia.com"],
      "isAllDay": false,
      "organizer": "me@xebia.com"
    }
  ]
}
```

#### `find_available_times`
Find free time slots across specified or all calendars.

**Parameters**:
- `accountIds` (optional): Array of account IDs, or omit for all accounts
- `duration`: Duration in minutes (e.g., 60 for 1 hour)
- `startDate`: Start of search range
- `endDate`: End of search range
- `workingHoursOnly` (default: true): Only suggest during working hours

**Returns**:
```json
{
  "availableSlots": [
    {
      "start": "2025-12-05T10:00:00Z",
      "end": "2025-12-05T11:00:00Z",
      "allAccountsFree": true,
      "busyAccounts": []
    }
  ]
}
```

#### `create_event`
Create calendar event in specific calendar (requires explicit account selection or smart routing).

**Parameters**:
- `accountId` (optional): Specific account, or let router decide
- `calendarId` (optional): Specific calendar within account
- `subject`: Event subject
- `start`: Start date/time (ISO 8601)
- `end`: End date/time (ISO 8601)
- `location` (optional): Event location
- `attendees` (optional): Array of attendee emails
- `body` (optional): Event description

**Returns**:
```json
{
  "success": true,
  "eventId": "evt-456",
  "accountUsed": "xebia-work",
  "calendarUsed": "cal-123"
}
```

#### `update_event`
Update existing calendar event.

**Parameters**:
- `accountId`: Specific account ID (required)
- `calendarId`: Specific calendar ID (required)
- `eventId`: Event ID to update (required)
- `subject` (optional): New subject
- `start` (optional): New start time
- `end` (optional): New end time
- `location` (optional): New location
- `attendees` (optional): New attendee list

**Returns**:
```json
{
  "success": true,
  "eventId": "evt-456"
}
```

#### `delete_event`
Delete calendar event.

**Parameters**:
- `accountId`: Specific account ID (required)
- `calendarId`: Specific calendar ID (required)
- `eventId`: Event ID to delete (required)

**Returns**:
```json
{
  "success": true
}
```

## Multi-Account Aggregation

### Read Operations

For read operations (get_emails, get_calendar_events, etc.), when `accountId` is omitted, the workflow engine:

1. **Parallel Execution**: Queries all accounts simultaneously using `Task.WhenAll`
2. **Result Merging**: Combines results from all accounts
3. **Deduplication**: Removes duplicates based on message/event IDs
4. **Sorting**: Orders by relevance (date, unread status, etc.)
5. **Metadata**: Includes `accountId` in each result for traceability

**Example**:
```
User: "Show me my unread emails"
→ MCP tool: get_emails(unreadOnly=true)
→ Router: No accountId, execute on all accounts
→ Parallel queries: [xebia-work, marimer-work, rocky-gmail]
→ Results merged: 45 unread emails across 3 accounts
→ Sorted by date descending
→ Return to AI assistant
```

### Write Operations

For write operations (send_email, create_event, etc.), exactly ONE account must be selected:

1. **Router Decision**: Smart router determines best account
2. **Ambiguity Handling**: If unclear, ask user to specify
3. **Execution**: Perform operation on selected account only
4. **Confirmation**: Return which account was used

**Example**:
```
User: "Send email to john@acme.com about project update"
→ MCP tool: send_email(to="john@acme.com", ...)
→ Router: Extract domain "acme.com" → matches "acme-work" account
→ Execute: Send via acme-work account
→ Return: { success: true, accountUsed: "acme-work" }
```

## Workflow Examples

### Example 1: Email Summary Across All Accounts
```
AI Assistant receives: "Summarize my unread emails from the last 24 hours"

Workflow:
1. Call get_emails(unreadOnly=true, accountId=null)
2. Filter results to last 24 hours
3. Group by account
4. Generate summary:
   "You have 15 unread emails:
   - Xebia Work: 8 emails (3 urgent)
   - Marimer Work: 5 emails (1 from client)
   - Personal Gmail: 2 emails (1 newsletter)"
```

### Example 2: Find Meeting Time Across All Calendars
```
AI Assistant receives: "Find a 1-hour slot next week where I'm free across all calendars"

Workflow:
1. Calculate date range (next week)
2. Call find_available_times(duration=60, startDate, endDate)
3. Analyze all accounts' calendars in parallel
4. Return slots where ALL accounts are free
5. Present options to user
```

### Example 3: Smart Email Sending
```
AI Assistant receives: "Send email to sarah@xebia.com saying I'll be 10 minutes late"

Workflow:
1. Call send_email(to="sarah@xebia.com", body="...")
2. Router extracts domain "xebia.com"
3. Router finds account with matching domain: "xebia-work"
4. Send via xebia-work account
5. Confirm: "Sent from your Xebia work account"
```

## Error Handling

All tools return consistent error responses:

```json
{
  "success": false,
  "error": {
    "code": "ACCOUNT_NOT_FOUND",
    "message": "Account 'invalid-account' not found",
    "details": {
      "availableAccounts": ["xebia-work", "rocky-gmail"]
    }
  }
}
```

Common error codes:
- `ACCOUNT_NOT_FOUND`: Specified account doesn't exist
- `AUTH_FAILED`: Authentication failed for account
- `PERMISSION_DENIED`: Insufficient permissions
- `RATE_LIMIT`: API rate limit exceeded
- `NETWORK_ERROR`: Network connectivity issue
- `INVALID_PARAMETER`: Invalid parameter value

## Transport

Tools are exposed via MCP protocol with multiple transport options:

- **stdio** (primary): Standard input/output for local AI assistants
- **SSE**: Server-Sent Events for web-based clients
- **WebSocket**: Bidirectional communication for interactive clients

Configuration in Claude Desktop:
```json
{
  "mcpServers": {
    "calendar-mcp": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/CalendarMcp.Server.dll"],
      "env": {
        "CALENDAR_MCP_CONFIG": "path/to/appsettings.json"
      }
    }
  }
}
```
