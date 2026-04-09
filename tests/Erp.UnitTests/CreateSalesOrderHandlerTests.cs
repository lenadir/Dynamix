using Erp.Infrastructure.Persistence;
using Erp.Modules.Sales.Commands;
using Erp.Modules.Sales.Domain;
using Erp.Platform.Tenancy;
using Erp.Shared.DTOs;
using Erp.Shared.Validation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Erp.UnitTests;

public class CreateSalesOrderHandlerTests
{
    private readonly Guid _tenantId = new("11111111-1111-1111-1111-111111111111");

    private (ErpDbContext db, CreateSalesOrderHandler handler) CreateHandler()
    {
        var tenantProvider = new TenantProvider { CurrentTenantId = _tenantId };

        var options = new DbContextOptionsBuilder<ErpDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new ErpDbContext(options, tenantProvider);

        var bus = new Mock<IPublishEndpoint>();
        var validator = new CreateSalesOrderDtoValidator();
        var handler = new CreateSalesOrderHandler(db, tenantProvider, bus.Object, validator);

        return (db, handler);
    }

    [Fact]
    public async Task Handle_ValidOrder_ShouldPersistAndReturnDto()
    {
        var (db, handler) = CreateHandler();
        var dto = new CreateSalesOrderDto(
            Guid.NewGuid(),
            new List<CreateSalesOrderLineDto>
            {
                new(Guid.NewGuid(), 5, 10.00m)
            });

        var result = await handler.Handle(new CreateSalesOrderCommand(dto), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(50.00m, result.Total);
        Assert.Single(result.Lines);

        // Verify persisted
        var persisted = await db.Set<SalesOrder>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(o => o.Id == result.Id);
        Assert.NotNull(persisted);

        db.Dispose();
    }

    [Fact]
    public async Task Handle_EmptyLines_ShouldThrowValidation()
    {
        var (db, handler) = CreateHandler();
        var dto = new CreateSalesOrderDto(Guid.NewGuid(), new List<CreateSalesOrderLineDto>());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => handler.Handle(new CreateSalesOrderCommand(dto), CancellationToken.None));

        db.Dispose();
    }
}
