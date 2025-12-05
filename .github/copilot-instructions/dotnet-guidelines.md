# .NET Development Guidelines

## Async Programming

- Prefer `async Task` over `void` for methods that perform any asynchronous operations
- Only use `async void` for event handlers where required by the event signature
- Always await async operations rather than using `.Result` or `.Wait()` to avoid deadlocks
- Always use async/await for IO operations (file, network, database)
- Use cancellation tokens for async methods where possible to support graceful cancellation

## Dependency Injection

- Prefer dependency injection for managing service lifetimes and dependencies
- Prefer dependency injection over singleton patterns for better testability and maintainability
- Register services in the DI container rather than using `new` keyword for service instantiation
- Use constructor injection as the primary pattern for receiving dependencies
- Follow proper service lifetime patterns (Singleton, Scoped, Transient)

## Logging and Telemetry

- Use `ILogger<T>` for structured logging throughout the application
- Leverage OpenTelemetry for distributed tracing and observability
- Include relevant context in log messages using structured logging parameters
- Use appropriate log levels (Trace, Debug, Information, Warning, Error, Critical)
- Instrument critical paths with OpenTelemetry spans for performance monitoring
