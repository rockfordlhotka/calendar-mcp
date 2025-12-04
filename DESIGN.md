# Calendar & Email MCP - Design Specification

## Project Overview

An MCP (Model Context Protocol) server that provides a unified orchestration layer for managing multiple email and calendar accounts across Microsoft 365 (multiple tenants), Outlook.com, and Google Workspace. This MCP server can be consumed by any AI assistant (Claude, ChatGPT, GitHub Copilot, etc.) and internally orchestrates existing Microsoft and Google MCP servers with intelligent routing and cross-platform workflow capabilities.

## Problem Statement

Professionals working with multiple organizations often manage:
- Multiple M365 tenants (different work accounts)
- Personal Outlook.com accounts
- Google Workspace accounts
- Need unified AI-powered management across all accounts

Currently, no AI assistant (Claude, ChatGPT, Copilot) can access all these services simultaneously in a multi-tenant scenario.

## Solution Architecture

### High-Level Architecture

```mermaid
graph TD
    A[AI Assistants<br/>Claude, ChatGPT, Copilot]
    B[Calendar-Email MCP Server<br/>ModelContextProtocol]
    C[Smart Router<br/>Configurable LLM Backend]
    D[Microsoft MCP Client]
    E[Google MCP Client]
    F[microsoft-mcp<br/>elyxlz]
    G[google_workspace_mcp<br/>taylorwilsdon]
    H[OpenTelemetry<br/>Logging, Tracing & Metrics]
    
    A -->|MCP Protocol| B
    B --> C
    C --> D
    C --> E
    D -->|MCP Protocol| F
    E -->|MCP Protocol| G
    B -.-> H
    C -.-> H
    D -.-> H
    E -.-> H
```

### Core Components

#### 1. MCP Server Interface (ModelContextProtocol Package)
- **Exposes MCP tools** to AI assistants for unified email/calendar operations
- **Tools include**:
  - `send_email` - Send email from appropriate account
  - `list_calendars` - List all calendars across accounts
  - `create_event` - Create calendar event in appropriate account
  - `search_emails` - Search emails across all accounts
  - `forward_email` - Forward email between accounts/platforms
  - `sync_calendar_event` - Sync events between calendars
- **Transport**: Supports stdio, SSE, and WebSocket transports
- **Configuration**: Account setup and router configuration via MCP resources

#### 2. Account Registry
- Configuration-based mapping of accounts to contexts
- Supports multiple M365 tenants, Outlook.com, and Google Workspace
- Account metadata (display name, domain patterns, priority)
- Credential management integration

#### 3. Smart Router
- **Configurable AI Backend** - Key Design Principle
- Uses LLM to intelligently route requests to appropriate accounts
- Supports multiple backend options:
  - Local models via Ollama (Phi-3.5-mini, Qwen2-7B, etc.)
  - Cloud APIs (OpenAI, Anthropic, Azure OpenAI)
  - Custom model endpoints
- Classification based on:
  - Email domain patterns
  - Content keywords and context
  - User preferences and history
  - Explicit user directives

#### 4. MCP Client Managers
- **Microsoft MCP Client**: Interfaces with microsoft-mcp server
  - Supports multiple M365 tenant connections
  - Outlook.com account support
  - Email, calendar, contacts, OneDrive operations
  
- **Google MCP Client**: Interfaces with google_workspace_mcp server
  - Gmail operations
  - Google Calendar management
  - Drive access
  - Multi-account OAuth support

#### 5. Workflow Engine
- Cross-platform operations
- Examples:
  - "Forward this M365 email to my Gmail"
  - "Sync this Google Calendar event to my work M365 calendar"
  - "Find all meetings this week across all accounts"
- Orchestrates multiple MCP server calls
- Transaction coordination across platforms

#### 6. OpenTelemetry Integration
- **Structured Logging**: Consistent log formatting across all components
- **Distributed Tracing**: End-to-end request tracking across MCP servers
- **Metrics Collection**: Performance monitoring and usage analytics
- **Exporters**: Support for multiple backends
  - Console (development)
  - OTLP (OpenTelemetry Protocol)
  - Jaeger (distributed tracing)
  - Prometheus (metrics)
  - Azure Monitor / Application Insights
- **Key Telemetry**:
  - Router decision latency and accuracy
  - MCP server response times
  - Authentication success/failure rates
  - API call volumes per account
  - Error rates and exception tracking

## Existing MCP Servers to Leverage

### Microsoft Ecosystem
1. **microsoft-mcp** (elyxlz)
   - Most comprehensive M365 integration
   - Multi-account support
   - Email, calendar, OneDrive, contacts

2. **ms-365-mcp-server** (Softeria)
   - Full M365 suite support
   - Organization mode for work accounts

3. **office-365-mcp-server** (hvkshetry)
   - 24 consolidated tools
   - Headless operation
   - Automatic token refresh

### Google Ecosystem
1. **google_workspace_mcp** (taylorwilsdon)
   - Most feature-complete
   - Natural language control
   - Multi-user OAuth 2.1
   - Gmail, Calendar, Drive, Docs, Sheets

2. **google-workspace-mcp** (aaronsb)
   - Authenticated access to Gmail, Calendar, Drive, Contacts
   - Multi-account support

## Configuration Design

### Model Configuration
The router's AI backend should be fully configurable to allow users to choose based on their preferences and resources:

```json
{
  "router": {
    "backend": "ollama|openai|anthropic|azure|custom",
    "model": {
      "ollama": {
        "model": "phi3.5:3.8b",
        "endpoint": "http://localhost:11434"
      },
      "openai": {
        "model": "gpt-4o-mini",
        "apiKey": "${OPENAI_API_KEY}"
      },
      "anthropic": {
        "model": "claude-sonnet-4-20250514",
        "apiKey": "${ANTHROPIC_API_KEY}"
      },
      "azure": {
        "model": "gpt-4o-mini",
        "endpoint": "${AZURE_ENDPOINT}",
        "apiKey": "${AZURE_API_KEY}"
      },
      "custom": {
        "endpoint": "https://custom-endpoint.com/v1/chat",
        "model": "custom-model-name",
        "apiKey": "${CUSTOM_API_KEY}"
      }
    },
    "temperature": 0.3,
    "maxTokens": 500
  }
}
```

### OpenTelemetry Configuration

```json
{
  "telemetry": {
    "serviceName": "calendar-email-mcp",
    "serviceVersion": "1.0.0",
    "logging": {
      "level": "Information",
      "console": true,
      "structured": true
    },
    "tracing": {
      "enabled": true,
      "samplingRatio": 1.0,
      "exporters": ["console", "otlp"]
    },
    "metrics": {
      "enabled": true,
      "intervalSeconds": 60,
      "exporters": ["console", "prometheus"]
    },
    "exporters": {
      "otlp": {
        "endpoint": "http://localhost:4317",
        "protocol": "grpc"
      },
      "jaeger": {
        "endpoint": "http://localhost:14268/api/traces"
      },
      "prometheus": {
        "port": 9090,
        "endpoint": "/metrics"
      },
      "azureMonitor": {
        "connectionString": "${APPLICATIONINSIGHTS_CONNECTION_STRING}"
      }
    }
  }
}
```

### Account Configuration

```json
{
  "accounts": [
    {
      "id": "xebia-work",
      "type": "microsoft365",
      "displayName": "Xebia Work",
      "tenantId": "...",
      "priority": 1,
      "domains": ["xebia.com"],
      "keywords": ["xebia", "work project"],
      "isDefault": false
    },
    {
      "id": "marimer-work",
      "type": "microsoft365",
      "displayName": "Marimer M365",
      "tenantId": "...",
      "priority": 2,
      "domains": ["marimer.com"],
      "keywords": ["marimer", "client"],
      "isDefault": false
    },
    {
      "id": "marimer-gsuite",
      "type": "google",
      "displayName": "Marimer G-Suite",
      "priority": 3,
      "domains": ["marimer.com"],
      "keywords": ["marimer", "personal"],
      "isDefault": false
    },
    {
      "id": "personal-outlook",
      "type": "outlook",
      "displayName": "Personal Outlook",
      "priority": 4,
      "domains": ["outlook.com", "hotmail.com"],
      "keywords": ["personal", "family"],
      "isDefault": true
    }
  ]
}
```

## Recommended Local Models

For users choosing local Ollama models, recommended options:

### Phi-3.5-mini (3.8B) - Recommended Default
- Excellent classification accuracy
- 128K context length
- ~2.4GB quantized
- Fast inference (12+ tokens/sec on modest hardware)
- Strong reasoning capabilities

### Qwen2-7B
- Excellent structured data understanding
- Strong logic-based routing
- Good for complex decision-making

### Qwen2-1.5B
- Ultra-lightweight
- Blazing fast routing
- Good for simpler classification tasks
- Minimal resource usage

## Key Features

### Phase 1 - Core Functionality
1. Multi-account authentication and management
2. Configurable AI-powered smart routing
3. Basic email operations across all accounts
4. Basic calendar operations across all accounts
5. Unified account view
6. OpenTelemetry instrumentation for observability

### Phase 2 - Advanced Workflows
1. Cross-platform operations (forward, sync, copy)
2. Intelligent scheduling across calendars
3. Email threading and conversation management
4. Contact synchronization
5. Search across all accounts

### Phase 3 - Intelligence Layer
1. Learning from user routing preferences
2. Automatic categorization and filing
3. Smart suggestions for meeting times
4. Conflict detection across calendars
5. Priority-based inbox management

## Technical Stack

- **Language**: C# / .NET 10
- **MCP Server Framework**: ModelContextProtocol NuGet package
- **MCP Client Integration**: Consumes existing Microsoft and Google MCP servers
- **AI Routing**: Configurable (Ollama, OpenAI, Anthropic, Azure, Custom)
- **Configuration**: JSON-based with environment variable support
- **Authentication**: OAuth 2.0 (Microsoft MSAL, Google OAuth)
- **Observability**: OpenTelemetry for logging, tracing, and metrics
  - OpenTelemetry .NET SDK
  - OTLP exporters
  - Instrumentation libraries for HTTP, gRPC, and custom spans

## Security Considerations

1. **Credential Storage**: Use system credential managers (Windows Credential Manager, macOS Keychain)
2. **Token Refresh**: Automatic refresh token management
3. **Multi-Tenant Isolation**: Separate token stores per tenant
4. **API Key Protection**: Never log or expose API keys
5. **Secure Configuration**: Support for encrypted configuration sections
6. **Telemetry Data Privacy**: 
   - Redact sensitive information in logs and traces (email content, tokens, PII)
   - Configurable data retention policies
   - Support for local-only telemetry export
   - Compliance with GDPR and other privacy regulations

## Open Source Strategy

### License
MIT or Apache 2.0 - permissive to encourage adoption

### Target Audience
- Consultants managing multiple client accounts
- Contractors with multiple work engagements
- Professionals with separate work/personal accounts
- Anyone in multi-tenant scenarios

### Value Proposition
- No existing solution handles multi-tenant M365 + Google Workspace
- Configurable AI backend allows users to choose privacy/cost tradeoff
- Leverages proven MCP implementations
- Open source enables community contributions and customization

## Next Steps

1. Set up project structure and solution with ModelContextProtocol package
2. Define MCP tools/resources schema for the server interface
3. Implement account registry and configuration system
4. Create MCP client wrappers for Microsoft and Google servers
5. Implement configurable smart router with multiple backend support
6. Implement MCP tool handlers that orchestrate the backend MCP clients
7. Create sample workflows and usage examples
8. Test integration with Claude Desktop, VS Code, and other MCP clients
9. Documentation and setup guides
10. Community feedback and iteration

## Success Metrics

- Successfully routes 95%+ of requests to correct account
- Sub-second routing decisions
- Works seamlessly with 3+ accounts
- Comprehensive telemetry coverage (>90% of operations traced)
- Positive community feedback and adoption
- Contributions from other developers

## Future Enhancements

- Mobile app integration
- Slack/Teams notification integration
- Advanced analytics and reporting
- Calendar optimization suggestions
- Email template management
- Meeting scheduling assistant
- Integration with additional platforms (iCloud, Exchange on-premises)
