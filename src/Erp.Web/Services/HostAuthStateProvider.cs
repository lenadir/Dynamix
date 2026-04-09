using Microsoft.AspNetCore.Components.Authorization;

namespace Erp.Web.Services;

/// <summary>
/// Provides Blazor components with the current user's authentication state
/// by reading from the HttpContext established during the initial connection.
/// </summary>
public class HostAuthStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HostAuthStateProvider(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _httpContextAccessor.HttpContext?.User
            ?? new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
        return Task.FromResult(new AuthenticationState(user));
    }
}
