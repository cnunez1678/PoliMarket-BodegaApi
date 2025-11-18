using System.ComponentModel.DataAnnotations;

namespace PoliMarket.Bodega.Api.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required] public string Name { get; set; } = null!;
        [Required] public string Code { get; set; } = null!;
    }
}
