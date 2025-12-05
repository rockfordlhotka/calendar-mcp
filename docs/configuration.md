# Configuration

## Overview

Calendar-MCP uses JSON configuration files for accounts, routing, and telemetry settings. Configuration supports environment variables and encrypted sections.

## Configuration Files

### Primary Configuration
- **Location**: `appsettings.json` in application directory
- **Format**: JSON
- **Environment Overrides**: `appsettings.Development.json`, `appsettings.Production.json`

### Environment Variables
- Prefix: `CALENDAR_MCP_`
- Example: `CALENDAR_MCP_Router__Backend=ollama`

## Complete Configuration Example

```json
{
  "accounts": [
    {
      "id": "xebia-work",
      "displayName": "Xebia Work Account",
      "provider": "microsoft365",
      "enabled": true,
      "priority": 1,
      "domains": ["xebia.com"],
      "configuration": {
        "tenantId": "12345678-1234-1234-1234-123456789abc",
        "clientId": "87654321-4321-4321-4321-cba987654321",
        "scopes": [
          "Mail.Read",
          "Mail.Send",
          "Calendars.ReadWrite"
        ]
      }
    },
    {
      "id": "marimer-work",
      "displayName": "Marimer Consulting",
      "provider": "microsoft365",
      "enabled": true,
      "priority": 2,
      "domains": ["marimer.com", "lhotka.net"],
      "configuration": {
        "tenantId": "87654321-4321-4321-4321-123456789xyz",
        "clientId": "87654321-4321-4321-4321-cba987654321",
        "scopes": [
          "Mail.Read",
          "Mail.Send",
          "Calendars.ReadWrite"
        ]
      }
    },
    {
      "id": "rocky-gmail",
      "displayName": "Personal Gmail",
      "provider": "google",
      "enabled": true,
      "priority": 3,
      "domains": ["gmail.com"],
      "configuration": {
        "clientId": "123456789-abcdefg.apps.googleusercontent.com",
        "clientSecret": "GOCSPX-...",
        "userEmail": "rocky@gmail.com",
        "scopes": [
          "https://www.googleapis.com/auth/gmail.readonly",
          "https://www.googleapis.com/auth/gmail.send",
          "https://www.googleapis.com/auth/calendar"
        ]
      }
    },
    {
      "id": "rocky-outlook",
      "displayName": "Personal Outlook",
      "provider": "outlook.com",
      "enabled": true,
      "priority": 4,
      "domains": ["outlook.com", "hotmail.com"],
      "configuration": {
        "clientId": "abcdef12-3456-7890-abcd-ef1234567890",
        "scopes": [
          "Mail.Read",
          "Mail.Send",
          "Calendars.ReadWrite"
        ]
      }
    }
  ],
  "router": {
    "backend": "ollama",
    "model": "phi3.5:3.8b",
    "endpoint": "http://localhost:11434",
    "temperature": 0.1,
    "maxTokens": 500,
    "timeoutSeconds": 10,
    "fallbackToDefault": true,
    "defaultAccountId": "xebia-work",
    "caching": {
      "enabled": true,
      "ttlMinutes": 30
    }
  },
  "telemetry": {
    "enabled": true,
    "serviceName": "calendar-mcp",
    "serviceVersion": "1.0.0",
    "console": {
      "enabled": true,
      "logLevel": "Information"
    },
    "otlp": {
      "enabled": false,
      "endpoint": "http://localhost:4317"
    },
    "jaeger": {
      "enabled": false,
      "agentHost": "localhost",
      "agentPort": 6831
    },
    "azureMonitor": {
      "enabled": false,
      "connectionString": "InstrumentationKey=..."
    },
    "sampling": {
      "alwaysSample": false,
      "samplingRate": 0.1
    },
    "redaction": {
      "enabled": true,
      "redactEmailContent": true,
      "redactTokens": true,
      "redactPii": true
    }
  },
  "server": {
    "transport": "stdio",
    "name": "calendar-mcp",
    "version": "1.0.0"
  }
}
```

## Account Configuration

### Microsoft 365 Accounts

**Shared App Registration Pattern** (one ClientId for multiple tenants):
```json
{
  "id": "tenant1-work",
  "provider": "microsoft365",
  "configuration": {
    "tenantId": "tenant1-id",
    "clientId": "shared-multi-tenant-client-id",
    "scopes": ["Mail.Read", "Mail.Send", "Calendars.ReadWrite"]
  }
}
```

**Per-Tenant App Registration Pattern** (different ClientId per tenant):
```json
{
  "id": "tenant1-work",
  "provider": "microsoft365",
  "configuration": {
    "tenantId": "tenant1-id",
    "clientId": "tenant1-specific-client-id",
    "scopes": ["Mail.Read", "Mail.Send", "Calendars.ReadWrite"]
  }
}
```

**Required Fields**:
- `id`: Unique account identifier (used for token cache naming)
- `displayName`: Human-readable name
- `provider`: "microsoft365"
- `tenantId`: Azure AD tenant ID
- `clientId`: App registration client ID (can be shared or unique)
- `scopes`: Required Microsoft Graph permissions

**Optional Fields**:
- `enabled` (default: true): Enable/disable account
- `priority` (default: 999): Priority for ambiguous routing decisions
- `domains`: Email domains for smart routing (e.g., ["company.com"])

### Google Workspace / Gmail Accounts

**Shared OAuth Client Pattern** (one ClientId for multiple accounts):
```json
{
  "id": "personal-gmail",
  "provider": "google",
  "configuration": {
    "clientId": "shared-oauth-client-id.apps.googleusercontent.com",
    "clientSecret": "GOCSPX-shared-secret",
    "userEmail": "user1@gmail.com",
    "scopes": [
      "https://www.googleapis.com/auth/gmail.readonly",
      "https://www.googleapis.com/auth/gmail.send",
      "https://www.googleapis.com/auth/calendar"
    ]
  }
}
```

**Per-Organization OAuth Client Pattern** (different ClientId per org):
```json
{
  "id": "workspace-org",
  "provider": "google",
  "configuration": {
    "clientId": "org-specific-id.apps.googleusercontent.com",
    "clientSecret": "GOCSPX-org-specific-secret",
    "userEmail": "user@organization.com",
    "scopes": [
      "https://www.googleapis.com/auth/gmail.readonly",
      "https://www.googleapis.com/auth/gmail.send",
      "https://www.googleapis.com/auth/calendar"
    ]
  }
}
```

**Required Fields**:
- `id`: Unique account identifier
- `displayName`: Human-readable name
- `provider`: "google"
- `clientId`: OAuth 2.0 client ID (can be shared or unique)
- `clientSecret`: OAuth 2.0 client secret
- `userEmail`: Google account email address
- `scopes`: Required Google API permissions

### Outlook.com Accounts

```json
{
  "id": "personal-outlook",
  "provider": "outlook.com",
  "configuration": {
    "clientId": "personal-msa-app-client-id",
    "scopes": [
      "Mail.Read",
      "Mail.Send",
      "Calendars.ReadWrite"
    ]
  }
}
```

**Required Fields**:
- `id`: Unique account identifier
- `displayName`: Human-readable name
- `provider`: "outlook.com"
- `clientId`: App registration client ID (typically shared for personal accounts)
- `scopes`: Required Microsoft Graph permissions

**Note**: Outlook.com uses 'common' tenant automatically (no tenantId needed).

## Router Configuration

### Ollama (Local)

```json
{
  "router": {
    "backend": "ollama",
    "model": "phi3.5:3.8b",
    "endpoint": "http://localhost:11434",
    "temperature": 0.1,
    "maxTokens": 500,
    "timeoutSeconds": 10,
    "fallbackToDefault": true,
    "defaultAccountId": "xebia-work"
  }
}
```

### OpenAI

```json
{
  "router": {
    "backend": "openai",
    "model": "gpt-4o-mini",
    "apiKey": "sk-...",
    "temperature": 0.1,
    "maxTokens": 500,
    "timeoutSeconds": 10
  }
}
```

**Environment Variable**: `CALENDAR_MCP_Router__ApiKey=sk-...`

### Anthropic

```json
{
  "router": {
    "backend": "anthropic",
    "model": "claude-3-haiku-20240307",
    "apiKey": "sk-ant-...",
    "temperature": 0.1,
    "maxTokens": 500,
    "timeoutSeconds": 10
  }
}
```

### Azure OpenAI

```json
{
  "router": {
    "backend": "azure-openai",
    "endpoint": "https://your-resource.openai.azure.com/",
    "deploymentName": "gpt-4o-mini",
    "apiKey": "...",
    "apiVersion": "2024-02-15-preview",
    "temperature": 0.1,
    "maxTokens": 500
  }
}
```

### Custom Endpoint

```json
{
  "router": {
    "backend": "custom",
    "endpoint": "https://your-inference-server.com/v1/chat/completions",
    "apiKey": "...",
    "model": "your-model-name",
    "temperature": 0.1,
    "maxTokens": 500
  }
}
```

## OpenTelemetry Configuration

### Console Only (Development)

```json
{
  "telemetry": {
    "enabled": true,
    "serviceName": "calendar-mcp",
    "console": {
      "enabled": true,
      "logLevel": "Debug"
    }
  }
}
```

### OTLP (Production)

```json
{
  "telemetry": {
    "enabled": true,
    "serviceName": "calendar-mcp",
    "serviceVersion": "1.0.0",
    "otlp": {
      "enabled": true,
      "endpoint": "http://collector:4317",
      "protocol": "grpc"
    },
    "sampling": {
      "samplingRate": 0.1
    },
    "redaction": {
      "enabled": true,
      "redactEmailContent": true,
      "redactTokens": true
    }
  }
}
```

### Jaeger (Distributed Tracing)

```json
{
  "telemetry": {
    "enabled": true,
    "jaeger": {
      "enabled": true,
      "agentHost": "localhost",
      "agentPort": 6831
    }
  }
}
```

### Azure Monitor

```json
{
  "telemetry": {
    "enabled": true,
    "azureMonitor": {
      "enabled": true,
      "connectionString": "InstrumentationKey=...;IngestionEndpoint=..."
    }
  }
}
```

### Multiple Exporters

```json
{
  "telemetry": {
    "enabled": true,
    "console": { "enabled": true },
    "otlp": { "enabled": true, "endpoint": "http://localhost:4317" },
    "jaeger": { "enabled": true, "agentHost": "localhost", "agentPort": 6831 }
  }
}
```

## Security Considerations

### Sensitive Data Protection

**DO NOT store in appsettings.json**:
- API keys
- Client secrets
- Access tokens
- Refresh tokens

**Use environment variables instead**:
```bash
export CALENDAR_MCP_Router__ApiKey="sk-..."
export CALENDAR_MCP_Accounts__0__Configuration__ClientSecret="GOCSPX-..."
```

**Or use encrypted configuration sections** (future enhancement):
```json
{
  "router": {
    "apiKey": "encrypted:AQAAANCMnd8BFd..."
  }
}
```

### Token Storage

**Never store tokens in configuration files!**

Tokens are stored separately:
- **Microsoft**: `%LOCALAPPDATA%/CalendarMcp/msal_cache_{accountId}.bin` (encrypted)
- **Google**: `~/.credentials/calendar-mcp/{accountId}/` (JSON files)

See [Authentication](authentication.md#per-account-token-storage) for details.

## Configuration Validation

On startup, Calendar-MCP validates:

1. **Required fields**: All required account fields present
2. **Unique IDs**: No duplicate account IDs
3. **Valid providers**: Provider must be "microsoft365", "google", or "outlook.com"
4. **Router backend**: Supported backend type
5. **Telemetry settings**: Valid exporter configurations

Validation errors prevent server startup with clear error messages.

## Dynamic Configuration Updates

**Not supported in v1.0**. Configuration changes require server restart.

**Future enhancement**: Watch for configuration file changes and reload accounts dynamically.
