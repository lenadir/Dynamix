using Erp.Platform;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Erp.Modules.Sales;

/// <summary>
/// Registers Sales module services. Discovered at startup via IModule scanning.
/// </summary>
public class SalesModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        // Module-specific registrations can go here.
        // MediatR handlers are auto-registered in the host via assembly scanning.
    }

    public void Configure(WebApplication app)
    {
        // Module-specific middleware or endpoints can go here.
    }
}
