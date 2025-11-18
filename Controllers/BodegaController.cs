using Microsoft.AspNetCore.Mvc;
using PoliMarket.Bodega.Api.DTOs;
using PoliMarket.Bodega.Api.Services;

namespace PoliMarket.Bodega.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BodegaController : ControllerBase
    {
        private readonly IBodegaService _svc;
        public BodegaController(IBodegaService svc) => _svc = svc;

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts() => Ok(await _svc.GetProductsAsync());

        [HttpGet("stock/{productId:int}")]
        public async Task<IActionResult> GetStock(int productId) => Ok(new { ProductId = productId, Stock = await _svc.GetStockAsync(productId) });

        [HttpPost("purchase")]
        public async Task<IActionResult> Purchase([FromBody] StockTransactionDto dto)
        {
            var entry = await _svc.RegisterPurchaseAsync(dto.ProductId, dto.Quantity, dto.UnitCost, dto.SupplierId, dto.Reference);
            return CreatedAtAction(nameof(GetKardex), new { productId = dto.ProductId }, entry);
        }

        [HttpPost("dispatch")]
        public async Task<IActionResult> Dispatch([FromBody] StockTransactionDto dto)
        {
            var entry = await _svc.RegisterDispatchAsync(dto.ProductId, dto.Quantity, dto.Reference);
            return CreatedAtAction(nameof(GetKardex), new { productId = dto.ProductId }, entry);
        }

        [HttpGet("kardex/{productId:int}")]
        public async Task<IActionResult> GetKardex(int productId) => Ok(await _svc.GetKardexAsync(productId));
    }
}
