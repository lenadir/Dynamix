namespace Erp.Platform.Tenancy;

/// <summary>
/// Provides the current tenant id resolved from the incoming request.
/// Injected as Scoped so it is the same instance throughout a single request.
/// </summary>
public interface ITenantProvider
{
    Guid CurrentTenantId { get; set; }
}

public class TenantProvider : ITenantProvider
{
    public Guid CurrentTenantId { get; set; }
}
