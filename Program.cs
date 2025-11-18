using Microsoft.EntityFrameworkCore;
using PoliMarket.Bodega.Api.Data;
using PoliMarket.Bodega.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=bodega.db"));

builder.Services.AddScoped<IBodegaService, BodegaService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Ensure database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
