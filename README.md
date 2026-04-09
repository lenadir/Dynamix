# Dynamix ERP – Modular-Monolith Starter

A production-minded modular-monolith ERP scaffold inspired by Microsoft Dynamics 365, built with **C# / .NET 8**, **Domain-Driven Design**, and **Clean Architecture**.

---

## Architecture Overview

```
┌──────────────────────────────────────────────────────────┐
│                     Erp.Api (Host)                       │
│         ASP.NET Core · Swagger · Health Checks           │
├────────────┬────────────┬────────────────────────────────┤
│ Sales      │ Finance    │  ← Bounded-context modules     │
│ Module     │ Module     │    (IModule plug-in pattern)   │
├────────────┴────────────┤                                │
│    Erp.Platform          │  ← Core abstractions, Tenancy │
├──────────────────────────┤                                │
│    Erp.Infrastructure    │  ← EF Core, DbContext         │
├──────────────────────────┤                                │
│    Erp.Shared            │  ← DTOs, Validation, Events   │
└──────────────────────────┘
```

### Key Patterns
- **Multi-tenancy** – Shared-schema with `TenantId` on every aggregate and EF Core global query filters.
- **CQRS** – MediatR commands & queries per module.
- **Cross-module messaging** – MassTransit + RabbitMQ (e.g., `SalesOrderPlaced` → Finance `JournalEntry`).
- **Validation** – FluentValidation on commands.
- **Logging** – Serilog with console sink.

---

## Projects

| Project | Description |
|---|---|
| `src/Erp.Platform` | Core abstractions: `IModule`, `Tenant`, `ITenantProvider`, middleware |
| `src/Erp.Api` | ASP.NET Core Web API host, Swagger, health checks |
| `src/Erp.Web` | Blazor Server UI (admin, sales pages) |
| `src/Erp.Modules.Sales` | Sales bounded context: `SalesOrder`, commands, queries |
| `src/Erp.Modules.Finance` | Finance bounded context: `JournalEntry`, GL accounts, consumer |
| `src/Erp.Infrastructure` | EF Core `ErpDbContext`, multi-tenant query filters, seed data |
| `src/Erp.Shared` | DTOs, integration events, FluentValidation validators |
| `tests/Erp.UnitTests` | xUnit tests for domain logic and MediatR handlers |

---

## Quick Start with Docker Compose

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop) installed
- (Optional) [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) for local development

### Run Everything

```bash
docker-compose up --build
```

This starts:
| Service | URL |
|---|---|
| **Erp.Api** (Swagger) | http://localhost:5000/swagger |
| **Erp.Web** (Blazor) | http://localhost:5002 |
| **SQL Server** | `localhost:1433` |
| **RabbitMQ Management** | http://localhost:15672 (guest/guest) |

### Demo Tenant
The seed data creates a demo tenant:
- **Tenant ID**: `11111111-1111-1111-1111-111111111111`
- **Company**: Demo Corporation Ltd.

---

## Sample API Requests (curl)

### Create a Sales Order
```bash
curl -X POST http://localhost:5000/api/sales/orders \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111" \
  -d '{
    "customerId": "22222222-2222-2222-2222-222222222222",
    "lines": [
      { "productId": "33333333-3333-3333-3333-333333333333", "quantity": 5, "unitPrice": 99.95 }
    ]
  }'
```

### Get an Order by ID
```bash
curl http://localhost:5000/api/sales/orders/{ORDER_ID} \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111"
```

### View Journal Entries (auto-created by Finance consumer)
```bash
curl http://localhost:5000/api/finance/journal-entries \
  -H "X-Tenant-Id: 11111111-1111-1111-1111-111111111111"
```

### Health Check
```bash
curl http://localhost:5000/health
```

---

## Local Development (without Docker)

```bash
# 1. Restore packages
dotnet restore Dynamix.sln

# 2. Start SQL Server and RabbitMQ (via Docker)
docker run -d --name mssql -e "SA_PASSWORD=Your_password123" -e "ACCEPT_EULA=Y" -p 1433:1433 mcr.microsoft.com/mssql/server:2019-latest
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# 3. Run the API (auto-creates the database in Development mode)
dotnet run --project src/Erp.Api

# 4. (Optional) Run the Blazor UI
dotnet run --project src/Erp.Web

# 5. Run tests
dotnet test
```

### EF Core Migrations
The API runs `EnsureCreated()` in Development mode. For proper migrations:

```bash
# Install the EF tool (once)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate \
  --project src/Erp.Infrastructure \
  --startup-project src/Erp.Api

# Apply migrations
dotnet ef database update \
  --project src/Erp.Infrastructure \
  --startup-project src/Erp.Api
```

---

## Evolving to DB-per-Tenant

The current approach uses **shared-schema multi-tenancy** with `TenantId` columns and EF Core global query filters. To evolve to **DB-per-tenant**:

1. Create a `TenantConnectionResolver` that maps `TenantId` → connection string.
2. Override `DbContext` configuration to use the resolved connection string.
3. Remove the global query filters.
4. Each tenant gets its own database, which provides stronger data isolation.

---

## CI/CD

GitHub Actions CI is configured in `.github/workflows/ci.yml`:
- Triggers on push/PR to `main`
- Restores, builds, tests, and publishes artifacts

---

## Next Steps

- [ ] Add authentication (ASP.NET Core Identity / OpenID Connect)
- [ ] Add authorization policies per module
- [ ] Implement EF Core migrations instead of `EnsureCreated()`
- [ ] Add more modules: Inventory, Purchasing, HR
- [ ] Add Testcontainers for integration tests
- [ ] Add API versioning
- [ ] Add rate limiting and CORS policies
- [ ] Add distributed caching (Redis)
- [ ] Add Azure / AWS deployment manifests
- [ ] Add background job scheduling (Hangfire / Quartz)

---

## License

MIT – See [LICENSE](LICENSE) for details.
