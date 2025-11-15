
using BodegaApi.Models;

namespace BodegaApi.Repositories;

public interface IProductoRepository
{
    Task<List<Producto>> GetAllAsync();
    Task AddAsync(Producto p);
}
