using Microsoft.AspNetCore.Http;

namespace Erp.Platform.Tenancy;

/// <summary>
/// Reads "X-Tenant-Id" header from the request and populates the scoped ITenantProvider.
/// For production, replace with a JWT-claim / host-based resolver.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader)
            && Guid.TryParse(tenantHeader, out var tenantId))
        {
            tenantProvider.CurrentTenantId = tenantId;
        }

        await _next(context);
    }
}
