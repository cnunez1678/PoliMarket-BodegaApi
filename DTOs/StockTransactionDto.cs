namespace PoliMarket.Bodega.Api.DTOs
{
    public class StockTransactionDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; } // only for purchases
        public int? SupplierId { get; set; } // optional for purchases
        public string? Reference { get; set; }
    }
}
