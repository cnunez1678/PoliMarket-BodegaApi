
using Microsoft.EntityFrameworkCore;
using BodegaApi.Data;
using BodegaApi.Repositories;
using BodegaApi.Services;
using BodegaApi.Models;
using BodegaApi.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");
builder.Services.AddDbContext<BodegaContext>(opt => opt.UseInMemoryDatabase("BodegaDB"));

builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProveedorRepository, ProveedorRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/productos", async (IProductoRepository repo) => await repo.GetAllAsync());
app.MapPost("/api/productos", async (ProductoCreateDto dto, IProductoService service) => Results.Ok(await service.CreateAsync(dto)));

app.Run();
