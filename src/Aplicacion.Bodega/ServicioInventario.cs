using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dominio.Bodega.Entidades;
using Infraestructura.Bodega;
using Microsoft.EntityFrameworkCore;

namespace Aplicacion.Bodega;

public class ServicioInventario
{
    private readonly ContextoBodega _db;
    public ServicioInventario(ContextoBodega db) { _db = db; }

    public async Task<MovimientoInventario> RecibirAsync(Guid productoId, Guid bodegaId, decimal cantidad, decimal costoUnitario, string referencia)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser > 0");
        if (costoUnitario < 0) throw new ArgumentException("El costo unitario debe ser >= 0");

        _ = await _db.Productos.FindAsync(productoId) ?? throw new InvalidOperationException("Producto no existe");
        _ = await _db.Bodegas.FindAsync(bodegaId) ?? throw new InvalidOperationException("Bodega no existe");

        var lote = new LoteInventario
        {
            ProductoId = productoId,
            BodegaId = bodegaId,
            Cantidad = cantidad,
            CostoUnitario = costoUnitario,
            FechaRecepcion = DateTime.UtcNow
        };
        _db.LotesInventario.Add(lote);

        var ultimo = await _db.MovimientosInventario
            .Where(m => m.ProductoId == productoId && m.BodegaId == bodegaId)
            .OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        var saldoCant = ultimo?.SaldoCantidad ?? 0m;
        var saldoTotal = ultimo?.SaldoCostoTotal ?? 0m;

        var nuevoSaldoCant = saldoCant + cantidad;
        var nuevoSaldoTotal = saldoTotal + (cantidad * costoUnitario);
        var nuevoProm = nuevoSaldoCant == 0 ? 0 : decimal.Round(nuevoSaldoTotal / nuevoSaldoCant, 4);

        var mov = new MovimientoInventario
        {
            ProductoId = productoId,
            BodegaId = bodegaId,
            TipoMovimiento = TipoMovimiento.Entrada,
            Cantidad = cantidad,
            CostoUnitario = costoUnitario,
            CostoTotal = cantidad * costoUnitario,
            SaldoCantidad = nuevoSaldoCant,
            SaldoCostoTotal = nuevoSaldoTotal,
            SaldoCostoUnitario = nuevoProm,
            Referencia = referencia,
            Fecha = DateTime.UtcNow
        };
        _db.MovimientosInventario.Add(mov);
        await _db.SaveChangesAsync();
        return mov;
    }

    public async Task<List<MovimientoInventario>> DespacharAsync(Guid productoId, Guid bodegaId, decimal cantidad, string referencia)
    {
        if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser > 0");

        var producto = await _db.Productos.FindAsync(productoId) ?? throw new InvalidOperationException("Producto no existe");
        _ = await _db.Bodegas.FindAsync(bodegaId) ?? throw new InvalidOperationException("Bodega no existe");

        var disponible = await ObtenerStockAsync(productoId, bodegaId);
        if (disponible < cantidad) throw new InvalidOperationException($"Stock insuficiente. Disponible: {disponible}");

        var creados = new List<MovimientoInventario>();

        if (producto.MetodoCosto == MetodoCosto.FIFO)
        {
            var lotes = await _db.LotesInventario
                .Where(l => l.ProductoId == productoId && l.BodegaId == bodegaId && l.Cantidad > 0)
                .OrderBy(l => l.FechaRecepcion)
                .ToListAsync();
            var restante = cantidad;
            foreach (var lote in lotes)
            {
                if (restante <= 0) break;
                var tomar = Math.Min(restante, lote.Cantidad);
                lote.Cantidad -= tomar;
                restante -= tomar;
                var mov = await CrearMovimientoSalida(productoId, bodegaId, tomar, lote.CostoUnitario, referencia);
                creados.Add(mov);
            }
            if (restante > 0) throw new InvalidOperationException("No se pudo consumir todos los lotes");
        }
        else // PromedioMovil
        {
            var ultimo = await _db.MovimientosInventario
                .Where(m => m.ProductoId == productoId && m.BodegaId == bodegaId)
                .OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id)
                .FirstOrDefaultAsync();
            var promedio = ultimo?.SaldoCostoUnitario ?? 0m;

            var lotes = await _db.LotesInventario
                .Where(l => l.ProductoId == productoId && l.BodegaId == bodegaId && l.Cantidad > 0)
                .OrderBy(l => l.FechaRecepcion)
                .ToListAsync();
            var restante = cantidad;
            foreach (var lote in lotes)
            {
                if (restante <= 0) break;
                var tomar = Math.Min(restante, lote.Cantidad);
                lote.Cantidad -= tomar;
                restante -= tomar;
            }
            if (restante > 0) throw new InvalidOperationException("No se pudo reducir stock");

            var mov = await CrearMovimientoSalida(productoId, bodegaId, cantidad, promedio, referencia);
            creados.Add(mov);
        }

        await _db.SaveChangesAsync();
        return creados;
    }

    private async Task<MovimientoInventario> CrearMovimientoSalida(Guid productoId, Guid bodegaId, decimal cantidad, decimal costoUnitario, string referencia)
    {
        var ultimo = await _db.MovimientosInventario
            .Where(m => m.ProductoId == productoId && m.BodegaId == bodegaId)
            .OrderByDescending(m => m.Fecha).ThenByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        var saldoCant = ultimo?.SaldoCantidad ?? 0m;
        var saldoTotal = ultimo?.SaldoCostoTotal ?? 0m;

        var costoTotal = decimal.Round(cantidad * costoUnitario, 4);
        var nuevoSaldoCant = saldoCant - cantidad;
        var nuevoSaldoTotal = saldoTotal - costoTotal;
        if (nuevoSaldoCant < 0 || nuevoSaldoTotal < 0) throw new InvalidOperationException("Saldo negativo no permitido");
        var nuevoProm = nuevoSaldoCant == 0 ? 0 : decimal.Round(nuevoSaldoTotal / nuevoSaldoCant, 4);

        var mov = new MovimientoInventario
        {
            ProductoId = productoId,
            BodegaId = bodegaId,
            TipoMovimiento = TipoMovimiento.Salida,
            Cantidad = cantidad,
            CostoUnitario = costoUnitario,
            CostoTotal = costoTotal,
            SaldoCantidad = nuevoSaldoCant,
            SaldoCostoTotal = nuevoSaldoTotal,
            SaldoCostoUnitario = nuevoProm,
            Referencia = referencia,
            Fecha = DateTime.UtcNow
        };
        _db.MovimientosInventario.Add(mov);
        return mov;
    }

    public async Task<decimal> ObtenerStockAsync(Guid productoId, Guid bodegaId)
    {
        var lotes = await _db.LotesInventario
            .Where(l => l.ProductoId == productoId && l.BodegaId == bodegaId)
            .ToListAsync();
        return lotes.Sum(x => x.Cantidad);
    }

    public async Task<IReadOnlyList<MovimientoInventario>> ObtenerKardexAsync(Guid productoId, Guid bodegaId, DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _db.MovimientosInventario
            .Where(m => m.ProductoId == productoId && m.BodegaId == bodegaId)
            .OrderBy(m => m.Fecha).ThenBy(m => m.Id)
            .AsQueryable();
        if (desde.HasValue) query = query.Where(m => m.Fecha >= desde.Value);
        if (hasta.HasValue) query = query.Where(m => m.Fecha <= hasta.Value);
        return await query.ToListAsync();
    }
}
