using System;
using System.Collections.Generic;

namespace Warehouse.Domain.Entities;

public enum CostMethod
{
    MovingAverage = 0,
    FIFO = 1
}

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "UN";
    public bool IsActive { get; set; } = true;
    public CostMethod CostMethod { get; set; } = CostMethod.MovingAverage;
    public decimal ReorderPoint { get; set; } = 0m;
}

public class Supplier
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
}

public class Warehouse
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
}

public enum MovementType
{
    In = 1,
    Out = 2,
    AdjustmentIn = 3,
    AdjustmentOut = 4
}

/// <summary>
/// Lotes para control FIFO / vencimientos.
/// </summary>
public class InventoryLot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Movimiento de inventario (Kardex). Incluye saldos acumulados.
/// </summary>
public class InventoryMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }
    public MovementType MovementType { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    // Cantidades y costos del movimiento
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; } // Costo unitario aplicado al movimiento (p. ej. promedio o FIFO)
    public decimal TotalCost { get; set; }

    // Saldos resultantes
    public decimal BalanceQuantity { get; set; }
    public decimal BalanceUnitCost { get; set; }
    public decimal BalanceTotalCost { get; set; }

    public string? Reference { get; set; } // PO-123, SO-456, AJ-001, etc.
}
