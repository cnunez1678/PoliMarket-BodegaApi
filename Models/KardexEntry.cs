using System.ComponentModel.DataAnnotations;

namespace PoliMarket.Bodega.Api.Models
{
    // Representa un registro en el Kardex (entrada o salida)
    public class KardexEntry
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        // Positive quantity => ingress (compra), negative => egress (salida)
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; } // cost per unit at moment of transaction
        public decimal TotalCost => Quantity * UnitCost;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Reference { get; set; }
        public string Type { get; set; } = "UNKNOWN"; // PURCHASE, DISPATCH, ADJUSTMENT...
    }
}
