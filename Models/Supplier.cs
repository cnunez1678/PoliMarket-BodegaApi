using System.ComponentModel.DataAnnotations;

namespace PoliMarket.Bodega.Api.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = null!;
        public string? ContactEmail { get; set; }
    }
}
