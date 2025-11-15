
using BodegaApi.Models;

namespace BodegaApi.Repositories;

public interface IProveedorRepository
{
    Task<List<Proveedor>> GetAllAsync();
}
