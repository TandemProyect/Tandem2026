# Muros en archivo cifrado

29 de octubre de 2026. Sirve para revisar un diseño cuando algo falla: la persona exporta y se puede leer el archivo.

Botones en la barra superior del visor, a la derecha: bajar (exportar) y subir (importar). El fichero se llama `muros-<diseño>-fecha.tnwall`.

Dentro, una vez descifrado, solo va esto:

- Cada línea de muro: extremos, largo, ancho, alto, giros, punto medio, tapes, solapes, conexiones, ménsulas, rigidizadores, puntales y cota.
- Los puntos de referencia de las esquinas L (`cornerReferencePoints`), con identificador y posición XYZ en mm en la intersección superior de las caras exteriores.
- La vista: cámaras, encuadre y entorno.
- Los muros especiales puestos a mano (AutoCAD u otra aplicación): la línea con `IsSpecial` y las piezas colocadas (código, inserción y giro). El encofrado que calcula el visor no entra; se vuelve a generar al encofrar.

Importar sustituye los muros del diseño abierto. Si el archivo es de otro diseño, no reutiliza los id internos. Los archivos antiguos sin `cornerReferencePoints` siguen siendo válidos; el visor reconstruye los puntos desde los muros importados. Los puntos también se guardan en `dbo.TSql_Design_V2.AttCornerReferencePointsJson`.

## Bases de datos por entorno

Las conexiones de desarrollo y producción apuntan a servidores distintos, aunque usan el mismo nombre de catálogo:

- **Desarrollo local** (`Web.config`, perfil Debug): `.\SQLEXPRESS`, catálogo `db_a197cd_desingproducction`, autenticación integrada.
- **Producción/publicación** (`Web.Release.config`): `SQL5113.site4now.net`, catálogo `db_a197cd_desingproducction`, autenticación SQL.

Las credenciales de producción deben permanecer en la configuración protegida y no copiarse a esta documentación. La migración [2026-11-05_alter_TSql_Design_V2_CornerReferencePointsJson.sql](../../Scripts/TemporalScript/2026-11-05_alter_TSql_Design_V2_CornerReferencePointsJson.sql) se aplicó y verificó en ambas instancias; al desplegar, verificar cada servidor por separado aunque el catálogo tenga el mismo nombre.

## Cómo leerlo

```text
node Desing/IA/tools/decrypt-wall-export.mjs ruta\al\archivo.tnwall
```

Escribe al lado un `.json`. La clave es `Tandem2026.WallExchange.v1`, AES-256-GCM. El archivo empieza por `TNWALL1`, luego el iv de 12 bytes y el cifrado con la etiqueta al final, en base64.

Código: `Desing/Scripts/Desing2/desing2-wall-exchange.js` y `ImportWallExchange` en `Desing_2Controller`.
