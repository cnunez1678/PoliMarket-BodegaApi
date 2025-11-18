using System;

namespace Dominio.Bodega.Entidades;

public enum MetodoCosto
{
    PromedioMovil = 0,
    FIFO = 1
}

public class Producto
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string UnidadMedida { get; set; } = "UN";
    public bool Activo { get; set; } = true;
    public MetodoCosto MetodoCosto { get; set; } = MetodoCosto.PromedioMovil;
    public decimal PuntoReorden { get; set; } = 0m;
}

public class Proveedor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string IdentificacionTributaria { get; set; } = string.Empty;
}

public class Bodega
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
}

public enum TipoMovimiento
{
    Entrada = 1,
    Salida = 2,
    AjusteEntrada = 3,
    AjusteSalida = 4
}

public class LoteInventario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductoId { get; set; }
    public Guid BodegaId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public DateTime FechaRecepcion { get; set; } = DateTime.UtcNow;
}

public class MovimientoInventario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductoId { get; set; }
    public Guid BodegaId { get; set; }
    public TipoMovimiento TipoMovimiento { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal CostoTotal { get; set; }

    public decimal SaldoCantidad { get; set; }
    public decimal SaldoCostoUnitario { get; set; }
    public decimal SaldoCostoTotal { get; set; }

    public string? Referencia { get; set; }
}
