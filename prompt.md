Scaffold a production-minded modular-monolith ERP starter just like Microsoft Dynamics 365 using C# and .NET 8 (net8.0). Create a new Git repository structure and implement a minimal working scaffold that demonstrates the architecture, patterns, and integrations we will build out. Follow Domain-Driven Design and Clean Architecture principles. Provide code, configuration, and readme. Do not add any proprietary or external closed-source license text; keep MIT-friendly examples.

High-level requirements:
- Solution name: Dynamix
- Projects:
  - src/Erp.Platform (core abstractions, IModule, Tenant, common services)
  - src/Erp.Api (ASP.NET Core Web API, Program.cs, OpenAPI/Swagger)
  - src/Erp.Web (Blazor Server UI, minimal pages for admin, sales)
  - src/Erp.Modules.Sales (bounded context: Contacts, SalesOrder aggregate)
  - src/Erp.Modules.Finance (bounded context: Chart of Accounts, JournalEntry)
  - src/Erp.Infrastructure (EF Core DbContext, migrations, repository implementations)
  - src/Erp.Shared (DTOs, mapping profiles, validation)
  - tests/Erp.UnitTests (xUnit, sample unit tests)
- Use EF Core (SqlServer provider) with a single shared-schema multi-tenant approach: include TenantId on aggregate roots and configure a global query filter in DbContext.
- Use MediatR for commands/queries; use FluentValidation for command validation; use Mapster or AutoMapper for mapping.
- Add MassTransit wiring for RabbitMQ with a placeholder event and a simple consumer used for illustrating cross-module communication (e.g., SalesOrderPlaced -> Finance creates JournalEntry).
- Add Dockerfile(s) and a docker-compose.yml that brings up: SQL Server (mcr.microsoft.com/mssql/server), RabbitMQ, and the API service.
- Add GitHub Actions CI workflow that builds, runs tests, and creates artifacts.

Scaffold and implement:
1) Repository and solution:
   - Create a .sln named Dynamix.sln and add the projects above with appropriate SDK/project types.
   - Create a README.md at repo root with purpose, how to run locally with Docker Compose, and next steps.

2) Erp.Platform:
   - Add IModule interface:
     public interface IModule { void ConfigureServices(IServiceCollection services, IConfiguration config); void Configure(WebApplication app); }
   - Add Tenant entity (Id, Name, CompanyName, DefaultCurrency).
   - Add a simple TenantResolutionMiddleware and ITenantProvider to read TenantId from header "X-Tenant-Id" for local development.

3) Erp.Infrastructure:
   - Add ErpDbContext : DbContext with DbSet<Tenant>, DbSet<Contact>, DbSet<SalesOrder>, DbSet<SalesOrderLine>, DbSet<GeneralLedgerAccount>, DbSet<JournalEntry>.
   - Configure global query filter for multi-tenancy: modelBuilder.Entity<...>().HasQueryFilter(e => e.TenantId == _tenantProvider.CurrentTenantId) where tenant provider injected.
   - Configure EF Core migrations scaffolding and a sample initial migration class (or instructions to run `dotnet ef migrations add InitialCreate`).
   - Seed demo tenant + sample data in OnModelCreating or a Startup seed routine.

4) Erp.Modules.Sales:
   - Implement domain aggregate: SalesOrder (Id, TenantId, OrderNumber, CustomerId, Date, Total, List<SalesOrderLine>).
   - SalesOrderLine (ProductId, Quantity, UnitPrice, LineTotal).
   - Implement Commands and Queries using MediatR:
     - CreateSalesOrderCommand + handler (validates with FluentValidation).
     - GetSalesOrderByIdQuery + handler.
   - Raise a domain event or publish a MassTransit event SalesOrderPlaced with the order id and total, published after the order is persisted.

5) Erp.Modules.Finance:
   - Implement simple ChartOfAccounts / GeneralLedgerAccount entity.
   - Implement JournalEntry (Id, TenantId, Date, Lines where each line has AccountId, Debit, Credit, Description).
   - Add a simple MassTransit consumer that listens for SalesOrderPlaced events and creates a JournalEntry (simulate posting AR and Revenue lines). Demonstrate idempotency by using the SalesOrderId in a correlation/unique constraint check.

6) Erp.Api:
   - Create Program.cs that:
     - Sets up configuration, logging (Serilog), OpenAPI/Swagger.
     - Registers Platform modules by scanning for types implementing IModule and calling ConfigureServices.
     - Registers DbContext with connection string coming from configuration or environment (support SQL Server connection via Docker Compose).
     - Adds MediatR, MassTransit (RabbitMQ), Mapster/AutoMapper, FluentValidation.
     - Adds controllers and minimal APIs for:
       POST /api/sales/orders -> Create order (accepts DTO), returns 201 with Location.
       GET /api/sales/orders/{id}
       GET /api/finance/journal-entries
     - Adds health checks and swagger UI.

7) Erp.Web (Blazor Server):
   - Minimal Blazor Server project that authenticates with a simple cookie scheme or no-auth for local dev.
   - Add pages: /admin (tenant info), /sales (list orders), /sales/create (form to create an order calling the API).

8) Tests:
   - Add xUnit project with a sample unit test for SalesOrder creation business rule and a test for the MediatR handler (use in-memory EF Core provider or testcontainers style guidance).

9) Docker and Local Dev:
   - Dockerfile for Erp.Api and Erp.Web.
   - docker-compose.yml that creates:
     - mssql: image mcr.microsoft.com/mssql/server:2019-latest environment SA_PASSWORD=Your_password123 ACCEPT_EULA=Y and ports mapped
     - rabbitmq: image rabbitmq:3-management with ports 5672 and 15672
     - erp.api service: built from Dockerfile, depends_on mssql and rabbitmq
   - README instructions with environment variables to set, `docker-compose up --build`, and how to run EF Migrations from inside the container or locally.

10) CI:
    - .github/workflows/ci.yml:
      - On push/pull_request to main
      - Steps: checkout, setup-dotnet (net8.0), restore, build, test, publish artifact

11) Extras / Developer ergonomics:
    - Add appsettings.Development.json with connection strings for Docker Compose.
    - Add Serilog console sink configuration.
    - Add sample requests (curl) to README to demonstrate creating a tenant, creating an order, and viewing journal entries.

Developer constraints and expectations to follow:
- Keep code minimal but idiomatic and compilable; show full files for key files (Program.cs, IModule.cs, Tenant.cs, ErpDbContext.cs, a sample aggregate and handler, sample controller, Dockerfile, docker-compose.yml, GitHub Actions file, README).
- Use comments to explain non-obvious decisions in files.
- Use multi-tenant shared-schema approach by default, but note in README how to evolve to DB-per-tenant.
- Make the scaffold production-aware (appsettings, environment variables, secrets via env) but simplified for ease of local start.
- Provide a final repo tree output (text) showing files created.

Output expected from you (Copilot): create the repository file tree, and populate the key files listed above with compile-able C# code and configs. If anything cannot be fully implemented within a short scaffold (migrations, secrets), provide clear, actionable instructions and commands to run locally.

End: after scaffold generation, print concise next steps and commands for the developer to run (e.g., dotnet restore, dotnet ef database update, docker-compose up --build, example curl commands).

Do not scaffold any external proprietary tools or closed-source connectors. Keep code examples MIT-friendly and short where possible but runnable. Thank you.