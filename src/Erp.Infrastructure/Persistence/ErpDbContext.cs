using Erp.Modules.Finance.Domain;
using Erp.Modules.Sales.Domain;
using Erp.Platform.Entities;
using Erp.Platform.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace Erp.Infrastructure.Persistence;

/// <summary>
/// Shared-schema multi-tenant DbContext.  A global query filter on every
/// tenant-scoped entity ensures tenants never see each other's data.
/// To evolve to DB-per-tenant, swap the connection string based on the
/// resolved tenant instead of using query filters.
/// </summary>
public class ErpDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public ErpDbContext(DbContextOptions<ErpDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    // ── DbSets ───────────────────────────────────────────────────────────────
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<GeneralLedgerAccount> GeneralLedgerAccounts => Set<GeneralLedgerAccount>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Tenant ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).HasMaxLength(200);
        });

        // ── Contact ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Contact>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasQueryFilter(c => c.TenantId == _tenantProvider.CurrentTenantId);
        });

        // ── SalesOrder ───────────────────────────────────────────────────────
        modelBuilder.Entity<SalesOrder>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.OrderNumber).HasMaxLength(50);
            e.Property(o => o.Total).HasPrecision(18, 4);
            e.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.SalesOrderId);
            e.Navigation(o => o.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
            e.HasQueryFilter(o => o.TenantId == _tenantProvider.CurrentTenantId);
        });

        modelBuilder.Entity<SalesOrderLine>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.UnitPrice).HasPrecision(18, 4);
            e.Property(l => l.LineTotal).HasPrecision(18, 4);
            e.Property(l => l.Quantity).HasPrecision(18, 4);
        });

        // ── GeneralLedgerAccount ─────────────────────────────────────────────
        modelBuilder.Entity<GeneralLedgerAccount>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.AccountNumber).HasMaxLength(20);
            e.HasQueryFilter(a => a.TenantId == _tenantProvider.CurrentTenantId);
        });

        // ── JournalEntry ─────────────────────────────────────────────────────
        modelBuilder.Entity<JournalEntry>(e =>
        {
            e.HasKey(j => j.Id);
            e.HasMany(j => j.Lines).WithOne().HasForeignKey(l => l.JournalEntryId);
            e.Navigation(j => j.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
            e.HasIndex(j => j.CorrelationId).IsUnique().HasFilter("[CorrelationId] IS NOT NULL");
            e.HasQueryFilter(j => j.TenantId == _tenantProvider.CurrentTenantId);
        });

        modelBuilder.Entity<JournalEntryLine>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Debit).HasPrecision(18, 4);
            e.Property(l => l.Credit).HasPrecision(18, 4);
        });

        // ── Seed demo tenant & chart of accounts ─────────────────────────────
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var demoTenantId = new Guid("11111111-1111-1111-1111-111111111111");

        modelBuilder.Entity<Tenant>().HasData(new Tenant
        {
            Id = demoTenantId,
            Name = "Demo Corp",
            CompanyName = "Demo Corporation Ltd.",
            DefaultCurrency = "USD"
        });

        modelBuilder.Entity<GeneralLedgerAccount>().HasData(
            new GeneralLedgerAccount
            {
                Id = new Guid("00000000-0000-0000-0000-000000001100"),
                TenantId = demoTenantId,
                AccountNumber = "1100",
                Name = "Accounts Receivable",
                Type = "Asset"
            },
            new GeneralLedgerAccount
            {
                Id = new Guid("00000000-0000-0000-0000-000000004000"),
                TenantId = demoTenantId,
                AccountNumber = "4000",
                Name = "Sales Revenue",
                Type = "Revenue"
            });

        modelBuilder.Entity<Contact>().HasData(new Contact
        {
            Id = new Guid("22222222-2222-2222-2222-222222222222"),
            TenantId = demoTenantId,
            Name = "Acme Inc.",
            Email = "orders@acme.example",
            Phone = "+1-555-0100"
        });
    }
}
