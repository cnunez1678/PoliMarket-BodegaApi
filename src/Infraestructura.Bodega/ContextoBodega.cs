using Microsoft.EntityFrameworkCore;
using Dominio.Bodega.Entidades;

namespace Infraestructura.Bodega;

public class ContextoBodega : DbContext
{
    public ContextoBodega(DbContextOptions<ContextoBodega> options) : base(options) {}

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Dominio.Bodega.Entidades.Bodega> Bodegas => Set<Dominio.Bodega.Entidades.Bodega>();
    public DbSet<LoteInventario> LotesInventario => Set<LoteInventario>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Producto>().HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<LoteInventario>().HasIndex(l => new { l.ProductoId, l.BodegaId, l.FechaRecepcion });
        modelBuilder.Entity<MovimientoInventario>().HasIndex(m => new { m.ProductoId, m.BodegaId, m.Fecha });
    }
}
