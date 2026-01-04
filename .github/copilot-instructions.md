# GitHub Copilot Instructions for Calendar-MCP

This repository contains a Model Context Protocol (MCP) server written in C# / .NET 10 that provides unified access to multiple email and calendar accounts across Microsoft 365, Outlook.com, and Google Workspace.

## 📖 Quick Context

**Read these first when starting work:**
- [Project Context & Architecture](instructions/context.md) - High-level overview, problem statement, and architecture
- [.NET Development Guidelines](instructions/dotnet-guidelines.md) - .NET best practices and coding standards
- [Repository Rules](instructions/rules.md) - Documentation and change management rules

**Detailed documentation:** See `/docs` folder for comprehensive technical specifications on authentication, providers, routing, security, telemetry, and more.

## 🎯 Project Overview

This MCP server enables AI assistants (Claude, ChatGPT, GitHub Copilot) to access multiple email and calendar accounts simultaneously. It solves the multi-tenant problem where professionals manage multiple Microsoft 365 tenants, Outlook.com, and Google Workspace accounts.

**Key Architecture Points:**
- **Provider ≠ Account**: One provider service manages multiple accounts with isolated credentials
- **Per-Account Isolation**: Every account has its own authentication context and encrypted token storage
- **Smart Routing**: Configurable AI backend (Ollama local or cloud APIs) for intelligent account selection
- **Security First**: Encrypted tokens, minimal privilege scopes, privacy-first telemetry

## 🏗️ Project Structure

```
/src                    # Main source code
  /CalendarMcp.Core     # Core business logic and services
  /CalendarMcp.StdioServer  # MCP stdio server implementation
/docs                   # Comprehensive technical documentation
/spikes                 # Spike/experimental projects
/.github/instructions   # Detailed Copilot instructions
/changelogs            # Change logs and summaries (use for documenting changes)
```

## 🛠️ Technology Stack

- **Language**: C# / .NET 10
- **MCP Framework**: ModelContextProtocol NuGet package
- **Provider SDKs**: Microsoft.Graph, Google.Apis.Gmail/Calendar
- **Authentication**: OAuth 2.0 with MSAL (Microsoft) and Google OAuth
- **Observability**: OpenTelemetry (logging, tracing, metrics)
- **Console UI**: Spectre.Console for rich console applications

## 📝 Development Guidelines

### .NET Best Practices
- Use **async/await** for all I/O operations with proper CancellationToken support
- Prefer **dependency injection** over singletons for testability
- Use **ILogger<T>** for structured logging throughout
- Leverage **OpenTelemetry** for distributed tracing
- Follow proper service lifetime patterns (Singleton, Scoped, Transient)

### Code Organization
- Keep source files in `/src` folder
- Keep documentation in `/docs` folder
- Store change logs in `/changelogs` folder
- Place experimental code in `/spikes` folder

### Configuration & Security
- Use `IConfiguration` and options pattern (`IOptions<T>`)
- Never hardcode secrets; use dotnet user secrets or environment variables
- Use encrypted token storage with per-account isolation
- Validate configuration at application startup

### Error Handling & Logging
- Catch specific exception types, not general exceptions
- Log with sufficient context for troubleshooting
- Use try-catch with proper cleanup (finally or using statements)
- Follow OpenTelemetry best practices for spans and metrics

## 🎨 Console Applications

When building console applications:
- Use **Spectre.Console** for rich UI components (tables, progress bars, prompts)
- Provide clear input/output handling
- Include helpful error messages and guidance

## 🔄 Development Phases

### Phase 1 - Core Functionality (Current)
- Multi-account authentication and management
- Read-only email queries (unread, search, details)
- Read-only calendar queries (events, availability)
- Unified view aggregation across all accounts

### Phase 2 - Write Operations (Planned)
- Send email with smart account routing
- Create calendar events with smart calendar routing
- Email threading and conversation tracking

### Phase 3 - AI-Assisted Scheduling (Future)
- Intelligent meeting time suggestions
- Automated meeting coordination
- Conflict detection and resolution

## 🚀 Current Status

🚧 **Early Development** - Spike projects completed, documentation organized, ready for main implementation.

## 📚 Additional Resources

- **[DESIGN.md](../DESIGN.md)** - High-level design overview with links to detailed docs
- **[README.md](../README.md)** - Project README with quick start guide
- **[IMPLEMENTATION-STATUS.md](../IMPLEMENTATION-STATUS.md)** - Current implementation status

## 🎯 When Working on This Project

1. **Start with context**: Read [instructions/context.md](instructions/context.md) to understand goals and architecture
2. **Follow .NET standards**: Reference [instructions/dotnet-guidelines.md](instructions/dotnet-guidelines.md) for coding patterns
3. **Consult docs**: Check `/docs` folder for detailed specifications on specific topics
4. **Maintain structure**: Follow the [repository rules](instructions/rules.md) for file organization
5. **Security focus**: Always consider per-account isolation and credential security
