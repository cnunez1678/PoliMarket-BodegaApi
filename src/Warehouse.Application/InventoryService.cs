using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Application;

public class InventoryService
{
    private readonly WarehouseDbContext _db;

    public InventoryService(WarehouseDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Recepción de compra o entrada manual. Crea lote y movimiento de entrada.
    /// </summary>
    public async Task<InventoryMovement> ReceiveAsync(Guid productId, Guid warehouseId, decimal quantity, decimal unitCost, string reference)
    {
        if (quantity <= 0) throw new ArgumentException("quantity must be > 0");
        if (unitCost < 0) throw new ArgumentException("unitCost must be >= 0");

        var product = await _db.Products.FindAsync(productId) ?? throw new InvalidOperationException("Producto no existe");
        var wh = await _db.Warehouses.FindAsync(warehouseId) ?? throw new InvalidOperationException("Bodega no existe");

        // Crear lote
        var lot = new InventoryLot
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = quantity,
            UnitCost = unitCost,
            ReceivedAt = DateTime.UtcNow
        };
        _db.InventoryLots.Add(lot);

        // Calcular saldo anterior
        var last = await _db.InventoryMovements
            .Where(m => m.ProductId == productId && m.WarehouseId == warehouseId)
            .OrderByDescending(m => m.PerformedAt).ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        var prevQty = last?.BalanceQuantity ?? 0m;
        var prevTotal = last?.BalanceTotalCost ?? 0m;

        // Nuevo saldo
        var newQty = prevQty + quantity;
        var newTotal = prevTotal + (quantity * unitCost);
        var newAvg = newQty == 0 ? 0 : decimal.Round(newTotal / newQty, 4);

        var movement = new InventoryMovement
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = MovementType.In,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = quantity * unitCost,
            BalanceQuantity = newQty,
            BalanceTotalCost = newTotal,
            BalanceUnitCost = newAvg,
            Reference = reference,
            PerformedAt = DateTime.UtcNow
        };

        _db.InventoryMovements.Add(movement);
        await _db.SaveChangesAsync();
        return movement;
    }

    /// <summary>
    /// Despacho de venta/salida. Calcula costo aplicado según método del producto.
    /// FIFO consume de los lotes más antiguos. Promedio móvil usa el promedio vigente.
    /// </summary>
    public async Task<List<InventoryMovement>> DispatchAsync(Guid productId, Guid warehouseId, decimal quantity, string reference)
    {
        if (quantity <= 0) throw new ArgumentException("quantity must be > 0");

        var product = await _db.Products.FindAsync(productId) ?? throw new InvalidOperationException("Producto no existe");
        var wh = await _db.Warehouses.FindAsync(warehouseId) ?? throw new InvalidOperationException("Bodega no existe");

        var available = await GetStockAsync(productId, warehouseId);
        if (available < quantity) throw new InvalidOperationException($"Stock insuficiente. Disponible: {available}");

        var created = new List<InventoryMovement>();

        if (product.CostMethod == CostMethod.FIFO)
        {
            // Consumir lotes por orden de recepción (más antiguos primero)
            var lots = await _db.InventoryLots
                .Where(l => l.ProductId == productId && l.WarehouseId == warehouseId && l.Quantity > 0)
                .OrderBy(l => l.ReceivedAt)
                .ToListAsync();

            var remaining = quantity;
            foreach (var lot in lots)
            {
                if (remaining <= 0) break;
                var take = Math.Min(remaining, lot.Quantity);
                lot.Quantity -= take;
                remaining -= take;

                var movement = await CreateOutMovement(productId, warehouseId, take, lot.UnitCost, reference);
                created.Add(movement);
            }

            if (remaining > 0) throw new InvalidOperationException("No se pudo consumir todos los lotes (inconsistencia)");
        }
        else // MovingAverage
        {
            // Costo promedio vigente
            var last = await _db.InventoryMovements
                .Where(m => m.ProductId == productId && m.WarehouseId == warehouseId)
                .OrderByDescending(m => m.PerformedAt).ThenByDescending(m => m.Id)
                .FirstOrDefaultAsync();
            var avg = last?.BalanceUnitCost ?? 0m;

            // Reducir contra lotes sin importar costo (solo para disponibilidad)
            var lots = await _db.InventoryLots
                .Where(l => l.ProductId == productId && l.WarehouseId == warehouseId && l.Quantity > 0)
                .OrderBy(l => l.ReceivedAt)
                .ToListAsync();
            var remaining = quantity;
            foreach (var lot in lots)
            {
                if (remaining <= 0) break;
                var take = Math.Min(remaining, lot.Quantity);
                lot.Quantity -= take;
                remaining -= take;
            }
            if (remaining > 0) throw new InvalidOperationException("No se pudo reducir stock (inconsistencia)");

            var movement = await CreateOutMovement(productId, warehouseId, quantity, avg, reference);
            created.Add(movement);
        }

        await _db.SaveChangesAsync();
        return created;
    }

    private async Task<InventoryMovement> CreateOutMovement(Guid productId, Guid warehouseId, decimal qty, decimal unitCost, string reference)
    {
        // Obtener saldo anterior
        var last = await _db.InventoryMovements
            .Where(m => m.ProductId == productId && m.WarehouseId == warehouseId)
            .OrderByDescending(m => m.PerformedAt).ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        var prevQty = last?.BalanceQuantity ?? 0m;
        var prevTotal = last?.BalanceTotalCost ?? 0m;

        var totalCost = decimal.Round(qty * unitCost, 4);
        var newQty = prevQty - qty;
        var newTotal = prevTotal - totalCost;
        if (newQty < 0 || newTotal < 0) throw new InvalidOperationException("Saldo negativo no permitido");
        var newAvg = newQty == 0 ? 0 : decimal.Round(newTotal / newQty, 4);

        var movement = new InventoryMovement
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = MovementType.Out,
            Quantity = qty,
            UnitCost = unitCost,
            TotalCost = totalCost,
            BalanceQuantity = newQty,
            BalanceTotalCost = newTotal,
            BalanceUnitCost = newAvg,
            Reference = reference,
            PerformedAt = DateTime.UtcNow
        };
        _db.InventoryMovements.Add(movement);
        return movement;
    }

    public async Task<decimal> GetStockAsync(Guid productId, Guid warehouseId)
    {
        var lots = await _db.InventoryLots
            .Where(l => l.ProductId == productId && l.WarehouseId == warehouseId)
            .ToListAsync();
        return lots.Sum(x => x.Quantity);
    }

    public async Task<IReadOnlyList<InventoryMovement>> GetKardexAsync(Guid productId, Guid warehouseId, DateTime? from = null, DateTime? to = null)
    {
        var query = _db.InventoryMovements
            .Where(m => m.ProductId == productId && m.WarehouseId == warehouseId)
            .OrderBy(m => m.PerformedAt).ThenBy(m => m.Id)
            .AsQueryable();
        if (from.HasValue) query = query.Where(m => m.PerformedAt >= from.Value);
        if (to.HasValue) query = query.Where(m => m.PerformedAt <= to.Value);
        return await query.ToListAsync();
    }
}
