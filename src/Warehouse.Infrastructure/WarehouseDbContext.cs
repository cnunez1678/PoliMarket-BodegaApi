using Microsoft.EntityFrameworkCore;
using Warehouse.Domain.Entities;

namespace Warehouse.Infrastructure;

public class WarehouseDbContext : DbContext
{
    public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Warehouse.Domain.Entities.Warehouse> Warehouses => Set<Warehouse.Domain.Entities.Warehouse>();
    public DbSet<InventoryLot> InventoryLots => Set<InventoryLot>();
    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<InventoryLot>().HasIndex(l => new { l.ProductId, l.WarehouseId, l.ReceivedAt });
        modelBuilder.Entity<InventoryMovement>().HasIndex(m => new { m.ProductId, m.WarehouseId, m.PerformedAt });
    }
}
