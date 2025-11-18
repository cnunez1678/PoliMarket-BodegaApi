using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Aplicacion.Bodega;
using Dominio.Bodega.Entidades;
using Infraestructura.Bodega;
using Xunit;

public class PruebasKardex
{
    private ContextoBodega CrearDb()
    {
        var options = new DbContextOptionsBuilder<ContextoBodega>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ContextoBodega(options);
    }

    [Fact]
    public async Task PromedioMovil_EntradaSalida_ComputaPromedio()
    {
        using var db = CrearDb();
        var servicio = new ServicioInventario(db);
        var producto = new Producto { Nombre = "Lápiz", Sku = "P001", MetodoCosto = MetodoCosto.PromedioMovil };
        var bodega = new Bodega { Nombre = "Central" };
        db.Productos.Add(producto); db.Bodegas.Add(bodega); await db.SaveChangesAsync();

        await servicio.RecibirAsync(producto.Id, bodega.Id, 10, 100, "PO-1");
        await servicio.RecibirAsync(producto.Id, bodega.Id, 10, 200, "PO-2");
        var salidas = await servicio.DespacharAsync(producto.Id, bodega.Id, 5, "SO-1");

        Assert.Single(salidas);
        Assert.Equal(150m, salidas[0].CostoUnitario);
        var kardex = await servicio.ObtenerKardexAsync(producto.Id, bodega.Id);
        Assert.Equal(3, kardex.Count);
        Assert.Equal(15m, kardex[^1].SaldoCantidad);
        Assert.Equal(150m, kardex[^1].SaldoCostoUnitario);
    }

    [Fact]
    public async Task FIFO_EntradaSalida_ConsumeLotes()
    {
        using var db = CrearDb();
        var servicio = new ServicioInventario(db);
        var producto = new Producto { Nombre = "Cuaderno", Sku = "P002", MetodoCosto = MetodoCosto.FIFO };
        var bodega = new Bodega { Nombre = "Central" };
        db.Productos.Add(producto); db.Bodegas.Add(bodega); await db.SaveChangesAsync();

        await servicio.RecibirAsync(producto.Id, bodega.Id, 10, 100, "PO-1");
        await servicio.RecibirAsync(producto.Id, bodega.Id, 10, 200, "PO-2");
        var salidas = await servicio.DespacharAsync(producto.Id, bodega.Id, 15, "SO-1");

        Assert.Equal(2, salidas.Count);
        Assert.Equal(100m, salidas[0].CostoUnitario);
        Assert.Equal(200m, salidas[1].CostoUnitario);
    }
}
