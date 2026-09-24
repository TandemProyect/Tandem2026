# BricscadPlugin — Plugin Tandem 2026 para BricsCAD V26

Copia del plugin AutoCAD (paletas MVC, muro 2D, generar 3D, popup de espera).
La geometría sale de `LCornerDetector` vía `DesignToolsAutocad/ProcesarLineasZwcad`.

## Compilar

```
dotnet build TandemBricscadPlugin\BricscadPlugin\BricscadPlugin.csproj -c Debug
```

API: `C:\Program Files\Bricsys\BricsCAD V26 en_US\` (o `BRICSCAD_API_ROOT`).

## Cargar

En BricsCAD V26: `NETLOAD` → `TandemBricscadPlugin\BricscadPlugin\bin\Debug\BricscadPlugin.dll`

Pestaña **Tandem 2026** o comando `TANDEM`. Desing debe estar en IIS Express (`https://localhost:44384/`).

## Comandos

| Comando | Uso |
|---------|-----|
| `TANDEM` | Mostrar/ocultar paletas MVC |
| `TANDEM_MURO2D` | Dibujar muro 2D (eje + caras) |
| `TANDEM_MURO3D` / `GENERAR3D` | Levantar sólidos con LCornerDetector |
| `TANDEM_PROBAR_CONEXION` | Ping al MVC |
