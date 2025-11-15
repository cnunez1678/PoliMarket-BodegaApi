
using BodegaApi.Data;
using BodegaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BodegaApi.Repositories;

public class ProductoRepository : IProductoRepository
{
    private readonly BodegaContext _ctx;

    public ProductoRepository(BodegaContext ctx) => _ctx = ctx;

    public async Task<List<Producto>> GetAllAsync() => await _ctx.Productos.ToListAsync();

    public async Task AddAsync(Producto p)
    {
        _ctx.Productos.Add(p);
        await _ctx.SaveChangesAsync();
    }
}
