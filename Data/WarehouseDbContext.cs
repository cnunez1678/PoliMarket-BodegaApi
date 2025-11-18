using Microsoft.EntityFrameworkCore;
using PoliMarket.Bodega.Api.Models;

namespace PoliMarket.Bodega.Api.Data
{
    public class WarehouseDbContext : DbContext
    {
        public WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<KardexEntry> KardexEntries => Set<KardexEntry>();
        public DbSet<StockLayer> StockLayers => Set<StockLayer>();
    }

    public static class DbSeeder
    {
        public static void Seed(WarehouseDbContext db)
        {
            if (!db.Products.Any())
            {
                var p1 = new Product { Name = "Arroz 1kg", Code = "ARZ-001" };
                var p2 = new Product { Name = "Aceite 900ml", Code = "ACE-900" };
                db.Products.AddRange(p1, p2);
            }
            if (!db.Suppliers.Any())
            {
                db.Suppliers.AddRange(
                    new Supplier { Name = "Proveedor A", ContactEmail = "provA@polimarket.com" },
                    new Supplier { Name = "Proveedor B", ContactEmail = "provB@polimarket.com" }
                );
            }
            db.SaveChanges();
        }
    }
}
