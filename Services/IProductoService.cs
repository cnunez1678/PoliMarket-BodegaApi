
using BodegaApi.Models;
using BodegaApi.Dtos;

namespace BodegaApi.Services;

public interface IProductoService
{
    Task<Producto> CreateAsync(ProductoCreateDto dto);
}
