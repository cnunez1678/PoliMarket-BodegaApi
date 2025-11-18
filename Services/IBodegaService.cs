using PoliMarket.Bodega.Api.Models;

namespace PoliMarket.Bodega.Api.Services
{
    public interface IBodegaService
    {
        Task<IEnumerable<Product>> GetProductsAsync();
        Task<decimal> GetStockAsync(int productId);
        Task<KardexResult> GetKardexAsync(int productId);
        Task<KardexEntry> RegisterPurchaseAsync(int productId, decimal quantity, decimal unitCost, int? supplierId, string? reference);
        Task<KardexEntry> RegisterDispatchAsync(int productId, decimal quantity, string? reference);
    }
}
