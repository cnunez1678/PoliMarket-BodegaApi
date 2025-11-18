using Microsoft.EntityFrameworkCore;
using PoliMarket.Bodega.Api.Data;
using PoliMarket.Bodega.Api.Models;

namespace PoliMarket.Bodega.Api.Services
{
    // Kardex FIFO implementation:
    // - Purchases create a KardexEntry (positive quantity) and a StockLayer
    // - Dispatches create a KardexEntry (negative quantity) and consume StockLayers FIFO,
    //   calculating the cost of goods removed.
    public class BodegaService : IBodegaService
    {
        private readonly WarehouseDbContext _db;
        private readonly object _lock = new();

        public BodegaService(WarehouseDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Product>> GetProductsAsync()
        {
            return await _db.Products.AsNoTracking().ToListAsync();
        }

        public async Task<decimal> GetStockAsync(int productId)
        {
            var sumLayers = await _db.StockLayers.Where(s => s.ProductId == productId).SumAsync(s => (decimal?)s.Quantity) ?? 0m;
            return sumLayers;
        }

        public async Task<KardexResult> GetKardexAsync(int productId)
        {
            var entries = await _db.KardexEntries.Where(k => k.ProductId == productId).OrderBy(k => k.Timestamp).ToListAsync();
            decimal runningQty = 0;
            decimal runningVal = 0;
            var rows = new List<KardexRow>();
            foreach (var e in entries)
            {
                runningQty += e.Quantity;
                runningVal += e.Quantity * e.UnitCost;
                rows.Add(new KardexRow
                {
                    Timestamp = e.Timestamp,
                    Type = e.Type,
                    Quantity = e.Quantity,
                    UnitCost = e.UnitCost,
                    TotalCost = e.Quantity * e.UnitCost,
                    RunningQuantity = runningQty,
                    RunningValuation = runningVal
                });
            }
            return new KardexResult { ProductId = productId, Rows = rows };
        }

        public async Task<KardexEntry> RegisterPurchaseAsync(int productId, decimal quantity, decimal unitCost, int? supplierId, string? reference)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive for purchases.");
            if (unitCost < 0) throw new ArgumentException("Unit cost cannot be negative.");

            lock (_lock)
            {
                // create stock layer and kardex entry
                var layer = new StockLayer
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitCost = unitCost,
                    ReceivedAt = DateTime.UtcNow
                };
                _db.StockLayers.Add(layer);

                var entry = new KardexEntry
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitCost = unitCost,
                    Timestamp = DateTime.UtcNow,
                    Reference = reference ?? $"PUR-{Guid.NewGuid()}",
                    Type = "PURCHASE"
                };
                _db.KardexEntries.Add(entry);
                _db.SaveChanges();
                return entry;
            }
        }

        public async Task<KardexEntry> RegisterDispatchAsync(int productId, decimal quantity, string? reference)
        {
            if (quantity <= 0) throw new ArgumentException("Quantity must be positive for dispatches.");

            lock (_lock)
            {
                // Ensure enough stock
                var totalStock = _db.StockLayers.Where(s => s.ProductId == productId).Sum(s => s.Quantity);
                if (totalStock < quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock. Available: {totalStock}, requested: {quantity}");
                }

                decimal remaining = quantity;
                decimal weightedCost = 0m;

                // consume FIFO layers
                var layers = _db.StockLayers.Where(s => s.ProductId == productId).OrderBy(s => s.ReceivedAt).ToList();
                foreach (var layer in layers)
                {
                    if (remaining <= 0) break;
                    var take = Math.Min(layer.Quantity, remaining);
                    weightedCost += take * layer.UnitCost;
                    layer.Quantity -= take;
                    remaining -= take;
                    if (layer.Quantity == 0)
                    {
                        _db.StockLayers.Remove(layer);
                    }
                    else
                    {
                        _db.StockLayers.Update(layer);
                    }
                }

                var avgUnitCost = quantity > 0 ? weightedCost / quantity : 0m;

                var entry = new KardexEntry
                {
                    ProductId = productId,
                    Quantity = -quantity, // negative for dispatch
                    UnitCost = avgUnitCost,
                    Timestamp = DateTime.UtcNow,
                    Reference = reference ?? $"DSP-{Guid.NewGuid()}",
                    Type = "DISPATCH"
                };
                _db.KardexEntries.Add(entry);
                _db.SaveChanges();
                return entry;
            }
        }
    }

    // Supporting DTOs for service responses
    public class KardexResult
    {
        public int ProductId { get; set; }
        public List<KardexRow> Rows { get; set; } = new();
    }

    public class KardexRow
    {
        public DateTime Timestamp { get; set; }
        public string? Type { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public decimal RunningQuantity { get; set; }
        public decimal RunningValuation { get; set; }
    }
}
