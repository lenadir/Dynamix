using Erp.Platform;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Erp.Modules.Finance;

public class FinanceModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        // Module-specific registrations can go here.
    }

    public void Configure(WebApplication app)
    {
        // Module-specific middleware or endpoints.
    }
}
