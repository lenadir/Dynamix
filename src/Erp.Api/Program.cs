using Erp.Infrastructure.Persistence;
using Erp.Modules.Finance.Consumers;
using Erp.Platform;
using Erp.Platform.Tenancy;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;

// ── Bootstrap Serilog early so we capture startup errors ─────────────────
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .WriteTo.Console());

    // ── Swagger / OpenAPI ────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Dynamix ERP API", Version = "v1" });
    });

    // ── Multi-tenancy ────────────────────────────────────────────────────
    builder.Services.AddScoped<ITenantProvider, TenantProvider>();

    // ── EF Core (SQL Server, shared schema) ──────────────────────────────
    builder.Services.AddDbContext<ErpDbContext>(opts =>
        opts.UseSqlServer(
            builder.Configuration.GetConnectionString("ErpDatabase"),
            sql => sql.MigrationsAssembly("Erp.Infrastructure")));

    // Register DbContext as the base DbContext type so modules can depend on it
    builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<ErpDbContext>());

    // ── MediatR (scan all module assemblies) ─────────────────────────────
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
        typeof(Erp.Modules.Sales.SalesModule).Assembly,
        typeof(Erp.Modules.Finance.FinanceModule).Assembly));

    // ── FluentValidation ─────────────────────────────────────────────────
    builder.Services.AddValidatorsFromAssemblyContaining<Erp.Shared.Validation.CreateSalesOrderDtoValidator>();

    // ── MassTransit + RabbitMQ ───────────────────────────────────────────
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<SalesOrderPlacedConsumer>();

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost", "/", h =>
            {
                h.Username(builder.Configuration.GetValue<string>("RabbitMQ:Username") ?? "guest");
                h.Password(builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? "guest");
            });

            cfg.ConfigureEndpoints(ctx);
        });
    });

    // ── Controllers ──────────────────────────────────────────────────────
    builder.Services.AddControllers();

    // ── Health checks ────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddSqlServer(builder.Configuration.GetConnectionString("ErpDatabase") ?? "")
        .AddRabbitMQ(rabbitConnectionString:
            $"amqp://{builder.Configuration.GetValue<string>("RabbitMQ:Username") ?? "guest"}:{builder.Configuration.GetValue<string>("RabbitMQ:Password") ?? "guest"}@{builder.Configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost"}/");

    // ── Discover & register IModule implementations (modular monolith) ──
    var moduleTypes = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(a => a.GetTypes())
        .Where(t => typeof(IModule).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });

    var modules = moduleTypes.Select(t => (IModule)Activator.CreateInstance(t)!).ToList();
    foreach (var module in modules)
        module.ConfigureServices(builder.Services, builder.Configuration);

    // ── Build ────────────────────────────────────────────────────────────
    var app = builder.Build();

    // ── Middleware pipeline ───────────────────────────────────────────────
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dynamix ERP API v1"));
    }

    app.UseMiddleware<TenantResolutionMiddleware>();
    app.UseSerilogRequestLogging();
    app.MapControllers();
    app.MapHealthChecks("/health");

    // Let each module configure its own middleware/endpoints
    foreach (var module in modules)
        module.Configure(app);

    // ── Auto-migrate on startup (dev convenience; disable in production) ─
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
        db.Database.EnsureCreated();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
