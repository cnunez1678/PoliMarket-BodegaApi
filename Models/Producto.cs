
namespace BodegaApi.Models;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int Cantidad { get; set; }
    public int ProveedorId { get; set; }
}
