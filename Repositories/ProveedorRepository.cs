
using BodegaApi.Data;
using BodegaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BodegaApi.Repositories;

public class ProveedorRepository : IProveedorRepository
{
    private readonly BodegaContext _ctx;

    public ProveedorRepository(BodegaContext ctx) => _ctx = ctx;

    public async Task<List<Proveedor>> GetAllAsync() => await _ctx.Proveedores.ToListAsync();
}
