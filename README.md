# Bodega API (.NET 8) con lógica de Kardex (Promedio móvil y FIFO)

Este proyecto implementa un microservicio **Bodega** para *PoliMarket* con lógica de inventarios y **Kardex** (entradas/salidas) usando .NET 8, Minimal APIs y EF Core (SQLite). Incluye pruebas unitarias y endpoints para productos, proveedores, bodegas, recepciones, despachos, stock y reporte de Kardex.

## Requisitos
- .NET SDK 8.0+

## Ejecutar
```bash
# Restaurar y compilar
cd src/Warehouse.Api
dotnet run
```
Abra Swagger en: http://localhost:5000/swagger

## Endpoints principales
- `POST /products` Crea producto (incluye método de costo: MovingAverage o FIFO)
- `POST /warehouses` Crea bodega
- `POST /suppliers` Crea proveedor
- `POST /inventory/receipts` Registra entrada: { productId, warehouseId, quantity, unitCost, reference }
- `POST /inventory/dispatches` Registra salida: { productId, warehouseId, quantity, reference }
- `GET /inventory/stock?productId=&warehouseId=` Stock actual
- `GET /inventory/kardex?productId=&warehouseId=&from=&to=` Reporte Kardex

## Pruebas
```bash
cd tests/Warehouse.Tests
dotnet test
```

## Notas de dominio
- Kardex guarda saldos acumulados por movimiento.
- **FIFO** consume lotes empezando por los más antiguos (control por `InventoryLot`).
- **Promedio móvil** valora salidas al promedio vigente y reduce stock distribuyendo contra lotes.

## Estructura
```
BodegaApi.sln
src/
  Warehouse.Api/            # Minimal API + DTOs + Swagger
  Warehouse.Application/    # Servicios de dominio (InventoryService)
  Warehouse.Domain/         # Entidades y enums
  Warehouse.Infrastructure/ # EF Core (DbContext, SQLite)
tests/
  Warehouse.Tests/          # xUnit con pruebas de Kardex
```
