
using BodegaApi.Models;
using BodegaApi.Repositories;
using BodegaApi.Dtos;

namespace BodegaApi.Services;

public class ProductoService : IProductoService
{
    private readonly IProductoRepository _repo;

    public ProductoService(IProductoRepository repo) => _repo = repo;

    public async Task<Producto> CreateAsync(ProductoCreateDto dto)
    {
        var p = new Producto
        {
            Nombre = dto.Nombre,
            Cantidad = dto.Cantidad
        };

        await _repo.AddAsync(p);
        return p;
    }
}
