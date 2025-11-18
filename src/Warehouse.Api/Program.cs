using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Warehouse.Application;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// DB: SQLite local (archivo warehouse.db)
builder.Services.AddDbContext<WarehouseDbContext>(opt =>
{
    opt.UseSqlite("Data Source=warehouse.db");
});

builder.Services.AddScoped<InventoryService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Migrar/crear DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", at = DateTime.UtcNow }));

// Productos
app.MapPost("/products", async (Product p, WarehouseDbContext db) =>
{
    db.Products.Add(p);
    await db.SaveChangesAsync();
    return Results.Created($"/products/{p.Id}", p);
});

app.MapGet("/products/{id:guid}", async (Guid id, WarehouseDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    return p is null ? Results.NotFound() : Results.Ok(p);
});

// Proveedores
app.MapPost("/suppliers", async (Supplier s, WarehouseDbContext db) =>
{
    db.Suppliers.Add(s);
    await db.SaveChangesAsync();
    return Results.Created($"/suppliers/{s.Id}", s);
});

// Bodegas
app.MapPost("/warehouses", async (Warehouse.Domain.Entities.Warehouse w, WarehouseDbContext db) =>
{
    db.Warehouses.Add(w);
    await db.SaveChangesAsync();
    return Results.Created($"/warehouses/{w.Id}", w);
});

// Recepciones (Entradas)
app.MapPost("/inventory/receipts", async (ReceiptDto dto, InventoryService service) =>
{
    var mv = await service.ReceiveAsync(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.UnitCost, dto.Reference ?? "PO");
    return Results.Created($"/inventory/movements/{mv.Id}", mv);
});

// Despachos (Salidas)
app.MapPost("/inventory/dispatches", async (DispatchDto dto, InventoryService service) =>
{
    var mvs = await service.DispatchAsync(dto.ProductId, dto.WarehouseId, dto.Quantity, dto.Reference ?? "SO");
    return Results.Ok(mvs);
});

// Stock
app.MapGet("/inventory/stock", async (Guid productId, Guid warehouseId, InventoryService service) =>
{
    var stock = await service.GetStockAsync(productId, warehouseId);
    return Results.Ok(new { productId, warehouseId, stock });
});

// Kardex
app.MapGet("/inventory/kardex", async (Guid productId, Guid warehouseId, DateTime? from, DateTime? to, InventoryService service) =>
{
    var data = await service.GetKardexAsync(productId, warehouseId, from, to);
    return Results.Ok(data);
});

app.Run();

// DTOs
public record ReceiptDto(Guid ProductId, Guid WarehouseId, decimal Quantity, decimal UnitCost, string? Reference);
public record DispatchDto(Guid ProductId, Guid WarehouseId, decimal Quantity, string? Reference);
