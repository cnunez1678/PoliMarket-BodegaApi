
using Microsoft.EntityFrameworkCore;
using BodegaApi.Models;

namespace BodegaApi.Data;

public class BodegaContext : DbContext
{
    public BodegaContext(DbContextOptions<BodegaContext> options) : base(options) {}

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
}
