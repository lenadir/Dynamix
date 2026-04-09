namespace Erp.Platform;

/// <summary>
/// Contract every bounded-context module implements so the host can
/// discover and wire it up at startup (modular-monolith plug-in pattern).
/// </summary>
public interface IModule
{
    void ConfigureServices(IServiceCollection services, IConfiguration config);
    void Configure(WebApplication app);
}
