# BodegaApi - .NET 8 Minimal API con EF Core (SQLite)

## Requisitos
- .NET 8 SDK instalado
- (Opcional) ngrok instalado y en PATH para exponer la API públicamente

## Ejecutar localmente
1. Abrir terminal en `src/BodegaApi`
2. Restaurar paquetes:
   dotnet restore
3. Ejecutar:
   dotnet run
   La API escuchará en http://localhost:5000

## Exponer con ngrok
En otra terminal:
   ngrok http 5000 --host-header=localhost

## Endpoints
- GET  /api/bodega/productos
- GET  /api/bodega/productos/{id}
- POST /api/bodega/productos
- PUT  /api/bodega/productos/{id}
- DELETE /api/bodega/productos/{id}
- POST /api/bodega/solicitar-reabastecimiento
- GET  /api/bodega/proveedores
- POST /api/bodega/proveedores
