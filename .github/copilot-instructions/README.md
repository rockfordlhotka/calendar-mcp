# Copilot Instructions

This directory contains instructions and context files to guide AI assistants (GitHub Copilot, Claude, ChatGPT) working on the calendar-mcp project.

## Files

### [context.md](context.md)
High-level project overview and architecture summary for the calendar-mcp project. Read this first to understand:
- What problem the project solves
- Core capabilities and development phases
- Architecture principles and technical stack
- Smart routing and authentication approaches
- Project status and documentation structure

### [dotnet-guidelines.md](dotnet-guidelines.md)
.NET development best practices and coding standards to follow throughout the project:
- Async programming patterns (async/await, Task, CancellationToken)
- Dependency injection over singletons
- Logging and telemetry with ILogger and OpenTelemetry

## Usage

When working on this project:

1. **Start with [context.md](context.md)** to understand the project's goals, architecture, and current status
2. **Reference [dotnet-guidelines.md](dotnet-guidelines.md)** when writing C# code to ensure consistent patterns
3. **Consult the main `/docs` folder** for detailed technical specifications on specific topics (authentication, providers, routing, etc.)

These instructions help maintain consistency and quality across all AI-assisted development work.
