using CalendarMcp.Core.Providers;
using CalendarMcp.Core.Services;
using CalendarMcp.Core.Tools;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace CalendarMcp.Core.Configuration;

/// <summary>
/// Extension methods for configuring Calendar MCP services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Calendar MCP core services to the dependency injection container
    /// </summary>
    public static IServiceCollection AddCalendarMcpCore(this IServiceCollection services)
    {
        // Register provider services
        services.AddSingleton<IM365ProviderService, M365ProviderService>();
        services.AddSingleton<IGoogleProviderService, GoogleProviderService>();
        services.AddSingleton<IOutlookComProviderService, OutlookComProviderService>();
        services.AddSingleton<IProviderServiceFactory, ProviderServiceFactory>();
        
        // Register account registry
        services.AddSingleton<IAccountRegistry, AccountRegistry>();
        
        // Register MCP tools
        services.AddSingleton<McpServerTool, ListAccountsTool>();
        services.AddSingleton<McpServerTool, GetEmailsTool>();
        services.AddSingleton<McpServerTool, SearchEmailsTool>();
        services.AddSingleton<McpServerTool, ListCalendarsTool>();
        services.AddSingleton<McpServerTool, GetCalendarEventsTool>();
        services.AddSingleton<McpServerTool, SendEmailTool>();
        services.AddSingleton<McpServerTool, CreateEventTool>();
        
        return services;
    }
}
