# API de Bodega (.NET 8) — Español — Kardex (Promedio móvil y FIFO)

## Ejecución
```bash
cd src/Api.Bodega
dotnet run
```
Abrir Swagger: `http://localhost:50xx/swagger`

## Endpoints
- `POST /productos`
- `POST /bodegas`
- `POST /proveedores`
- `POST /inventario/entradas`
- `POST /inventario/salidas`
- `GET /inventario/stock?productoId=&bodegaId=`
- `GET /inventario/kardex?productoId=&bodegaId=&desde=&hasta=`
