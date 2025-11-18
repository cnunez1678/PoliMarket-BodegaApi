using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse.Application;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;
using Xunit;

public class KardexTests
{
    private WarehouseDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<WarehouseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WarehouseDbContext(options);
    }

    [Fact]
    public async Task MovingAverage_EntradaSalida_ComputaPromedio()
    {
        using var db = CreateDb();
        var service = new InventoryService(db);
        var product = new Product { Name = "Lapiz", Sku = "P001", CostMethod = CostMethod.MovingAverage };
        var wh = new Warehouse { Name = "Bod Central" };
        db.Products.Add(product); db.Warehouses.Add(wh); await db.SaveChangesAsync();

        await service.ReceiveAsync(product.Id, wh.Id, 10, 100, "PO-1"); // saldo: 10 u, $100
        await service.ReceiveAsync(product.Id, wh.Id, 10, 200, "PO-2"); // saldo: 20 u, $150 avg
        var outs = await service.DispatchAsync(product.Id, wh.Id, 5, "SO-1"); // costo 150

        Assert.Single(outs);
        Assert.Equal(150m, outs[0].UnitCost);
        var kardex = await service.GetKardexAsync(product.Id, wh.Id);
        Assert.Equal(3, kardex.Count);
        Assert.Equal(15m, kardex[^1].BalanceQuantity);
        Assert.Equal(150m, kardex[^1].BalanceUnitCost);
    }

    [Fact]
    public async Task FIFO_EntradaSalida_ConsumeLotes()
    {
        using var db = CreateDb();
        var service = new InventoryService(db);
        var product = new Product { Name = "Cuaderno", Sku = "P002", CostMethod = CostMethod.FIFO };
        var wh = new Warehouse { Name = "Bod Central" };
        db.Products.Add(product); db.Warehouses.Add(wh); await db.SaveChangesAsync();

        await service.ReceiveAsync(product.Id, wh.Id, 10, 100, "PO-1");
        await service.ReceiveAsync(product.Id, wh.Id, 10, 200, "PO-2");
        var outs = await service.DispatchAsync(product.Id, wh.Id, 15, "SO-1");

        Assert.Equal(2, outs.Count);
        Assert.Equal(100m, outs[0].UnitCost); // 10 a 100
        Assert.Equal(200m, outs[1].UnitCost); // 5 a 200
    }
}
