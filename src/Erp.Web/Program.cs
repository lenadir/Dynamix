using System.Security.Claims;
using Erp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    builder.Services.AddRazorPages();
    builder.Services.AddServerSideBlazor();

    // ── Authentication ──────────────────────────────────────────────────
    builder.Services.AddSingleton<InMemoryUserStore>();
    builder.Services.AddAuthentication("DynamixCookie")
        .AddCookie("DynamixCookie", opts =>
        {
            opts.LoginPath = "/account/login";
            opts.ExpireTimeSpan = TimeSpan.FromHours(24);
            opts.SlidingExpiration = true;
        });
    builder.Services.AddAuthorization();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<AuthenticationStateProvider, HostAuthStateProvider>();

    // HttpClient for calling the Erp.Api
    builder.Services.AddHttpClient("ErpApi", client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5000");
        // Default demo tenant header
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "11111111-1111-1111-1111-111111111111");
    });

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // Logout endpoint
    app.MapGet("/account/logout", async (HttpContext ctx) =>
    {
        await ctx.SignOutAsync("DynamixCookie");
        return Results.Redirect("/");
    });

    // Trial registration endpoint (called via JS fetch from Blazor trial forms)
    app.MapPost("/account/trial-register", async (HttpContext ctx, InMemoryUserStore users) =>
    {
        var body = await ctx.Request.ReadFromJsonAsync<TrialRegisterRequest>();
        if (body is null || string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password))
            return Results.BadRequest();

        users.Register(body.Email, body.FirstName ?? "", body.LastName ?? "", body.Password);
        var user = users.Validate(body.Email, body.Password);
        if (user is null)
            return Results.Conflict();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new(ClaimTypes.GivenName, user.FirstName),
        };
        await ctx.SignInAsync("DynamixCookie",
            new ClaimsPrincipal(new ClaimsIdentity(claims, "DynamixCookie")));
        return Results.Ok();
    });

    app.MapRazorPages();
    app.MapBlazorHub();
    app.MapFallbackToPage("/_Host");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Blazor host terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}

record TrialRegisterRequest(string Email, string Password, string? FirstName, string? LastName);
