namespace PoliMarket.Bodega.Api.Models
{
    // FIFO stock layer used by Kardex valuation
    public class StockLayer
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }
}
