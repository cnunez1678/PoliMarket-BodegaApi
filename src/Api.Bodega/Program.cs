using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Aplicacion.Bodega;
using Dominio.Bodega.Entidades;
using Infraestructura.Bodega;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContextoBodega>(opt =>
{
    opt.UseSqlite("Data Source=bodega.db");
});

builder.Services.AddScoped<ServicioInventario>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ContextoBodega>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/salud", () => Results.Ok(new { estado = "ok", fecha = DateTime.UtcNow }));

// Productos
app.MapPost("/productos", async (Producto p, ContextoBodega db) =>
{
    db.Productos.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/productos/{p.Id}", p);
});
app.MapGet("/productos/{id:guid}", async (Guid id, ContextoBodega db) =>
{
    var p = await db.Productos.FindAsync(id);
    return p is null ? Results.NotFound() : Results.Ok(p);
});

// Proveedores
app.MapPost("/proveedores", async (Proveedor s, ContextoBodega db) =>
{
    db.Proveedores.Add(s);
    await db.SaveChangesAsync();
    return Results.Created($"/proveedores/{s.Id}", s);
});

// Bodegas
app.MapPost("/bodegas", async (Dominio.Bodega.Entidades.Bodega b, ContextoBodega db) =>
{
    db.Bodegas.Add(b);
    await db.SaveChangesAsync();
    return Results.Created($"/bodegas/{b.Id}", b);
});

// Entradas
app.MapPost("/inventario/entradas", async (RecepcionDto dto, ServicioInventario srv) =>
{
    var mv = await srv.RecibirAsync(dto.ProductoId, dto.BodegaId, dto.Cantidad, dto.CostoUnitario, dto.Referencia ?? "PO");
    return Results.Created($"/inventario/movimientos/{mv.Id}", mv);
});

// Salidas
app.MapPost("/inventario/salidas", async (DespachoDto dto, ServicioInventario srv) =>
{
    var mvs = await srv.DespacharAsync(dto.ProductoId, dto.BodegaId, dto.Cantidad, dto.Referencia ?? "SO");
    return Results.Ok(mvs);
});

// Stock
app.MapGet("/inventario/stock", async (Guid productoId, Guid bodegaId, ServicioInventario srv) =>
{
    var stock = await srv.ObtenerStockAsync(productoId, bodegaId);
    return Results.Ok(new { productoId, bodegaId, stock });
});

// Kardex
app.MapGet("/inventario/kardex", async (Guid productoId, Guid bodegaId, DateTime? desde, DateTime? hasta, ServicioInventario srv) =>
{
    var data = await srv.ObtenerKardexAsync(productoId, bodegaId, desde, hasta);
    return Results.Ok(data);
});

app.Run();

public record RecepcionDto(Guid ProductoId, Guid BodegaId, decimal Cantidad, decimal CostoUnitario, string? Referencia);
public record DespachoDto(Guid ProductoId, Guid BodegaId, decimal Cantidad, string? Referencia);
