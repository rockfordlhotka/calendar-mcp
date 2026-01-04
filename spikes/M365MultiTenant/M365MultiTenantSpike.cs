using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CalendarMcp.Spikes.M365MultiTenant;

public class M365MultiTenantSpike
{
    private readonly ILogger<M365MultiTenantSpike> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    
    public M365MultiTenantSpike(
        ILogger<M365MultiTenantSpike> logger, 
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }
    
    public async Task RunAsync()
    {
        _logger.LogInformation("===========================================");
        _logger.LogInformation("Starting M365 Multi-Tenant Spike");
        _logger.LogInformation("===========================================");
        _logger.LogInformation("");
        
        // Validate configuration
        if (!ValidateConfiguration())
        {
            _logger.LogError("Configuration validation failed. Please check appsettings.json");
            return;
        }
        
        try
        {
            // Test 1: Launch microsoft-mcp server for first tenant
            _logger.LogInformation("TEST 1: Testing Tenant 1 connection...");
            await TestTenant1Connection();
            _logger.LogInformation("");
            
            // Test 2: Launch microsoft-mcp server for second tenant
            _logger.LogInformation("TEST 2: Testing Tenant 2 connection...");
            await TestTenant2Connection();
            _logger.LogInformation("");
            
            // Test 3: Test simultaneous access
            _logger.LogInformation("TEST 3: Testing simultaneous multi-tenant access...");
            await TestSimultaneousAccess();
            _logger.LogInformation("");
            
            _logger.LogInformation("===========================================");
            _logger.LogInformation("✓ Spike completed successfully");
            _logger.LogInformation("===========================================");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Spike execution failed");
            throw;
        }
    }
    
    private bool ValidateConfiguration()
    {
        var tenant1 = _configuration.GetSection("Accounts:Tenant1");
        var tenant2 = _configuration.GetSection("Accounts:Tenant2");
        
        var tenant1Valid = !string.IsNullOrEmpty(tenant1["TenantId"]) && 
                          !string.IsNullOrEmpty(tenant1["ClientId"]);
        var tenant2Valid = !string.IsNullOrEmpty(tenant2["TenantId"]) && 
                          !string.IsNullOrEmpty(tenant2["ClientId"]);
        
        if (!tenant1Valid)
        {
            _logger.LogError("Tenant1 configuration is incomplete");
        }
        
        if (!tenant2Valid)
        {
            _logger.LogError("Tenant2 configuration is incomplete");
        }
        
        return tenant1Valid && tenant2Valid;
    }
    
    private async Task TestTenant1Connection()
    {
        var mcpServer = CreateMcpServerLauncher(
            accountId: "tenant1-work",
            tenantSection: "Accounts:Tenant1"
        );
        
        await mcpServer.StartAsync();
        
        // Test basic operations
        await mcpServer.TestGetCalendars();
        await mcpServer.TestGetEmails();
        
        await mcpServer.StopAsync();
        
        _logger.LogInformation("✓ Tenant 1 test completed");
    }
    
    private async Task TestTenant2Connection()
    {
        var mcpServer = CreateMcpServerLauncher(
            accountId: "tenant2-work",
            tenantSection: "Accounts:Tenant2"
        );
        
        await mcpServer.StartAsync();
        
        // Test basic operations
        await mcpServer.TestGetCalendars();
        await mcpServer.TestGetEmails();
        
        await mcpServer.StopAsync();
        
        _logger.LogInformation("✓ Tenant 2 test completed");
    }
    
    private async Task TestSimultaneousAccess()
    {
        var tenant1Server = CreateMcpServerLauncher(
            accountId: "tenant1-work",
            tenantSection: "Accounts:Tenant1",
            port: 3001
        );
        
        var tenant2Server = CreateMcpServerLauncher(
            accountId: "tenant2-work",
            tenantSection: "Accounts:Tenant2",
            port: 3002
        );
        
        // Start both servers
        await tenant1Server.StartAsync();
        await tenant2Server.StartAsync();
        
        _logger.LogInformation("Both servers started, testing parallel access...");
        
        // Test parallel access
        var task1 = tenant1Server.TestGetCalendars();
        var task2 = tenant2Server.TestGetCalendars();
        
        await Task.WhenAll(task1, task2);
        
        _logger.LogInformation("Parallel calendar access completed");
        
        // Test parallel email access
        var emailTask1 = tenant1Server.TestGetEmails();
        var emailTask2 = tenant2Server.TestGetEmails();
        
        await Task.WhenAll(emailTask1, emailTask2);
        
        _logger.LogInformation("Parallel email access completed");
        
        // Clean up
        await tenant1Server.StopAsync();
        await tenant2Server.StopAsync();
        
        _logger.LogInformation("✓ Simultaneous access test completed");
    }
    
    private McpServerLauncher CreateMcpServerLauncher(
        string accountId, 
        string tenantSection, 
        int port = 3000)
    {
        var section = _configuration.GetSection(tenantSection);
        
        return new McpServerLauncher(_configuration, _serviceProvider)
        {
            AccountId = accountId,
            TenantId = section["TenantId"] ?? throw new InvalidOperationException($"{tenantSection}:TenantId not found"),
            ClientId = section["ClientId"] ?? throw new InvalidOperationException($"{tenantSection}:ClientId not found"),
            DisplayName = section["DisplayName"] ?? accountId,
            Port = port
        };
    }
}
