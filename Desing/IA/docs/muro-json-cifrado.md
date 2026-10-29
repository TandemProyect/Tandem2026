# Muros en archivo cifrado

29 de octubre de 2026. Sirve para revisar un diseño cuando algo falla: la persona exporta y se puede leer el archivo.

Botones en la barra superior del visor, a la derecha: bajar (exportar) y subir (importar). El fichero se llama `muros-<diseño>-fecha.tnwall`.

Dentro, una vez descifrado, solo va esto:

- Cada línea de muro: extremos, largo, ancho, alto, giros, punto medio, tapes, solapes, conexiones, ménsulas, rigidizadores, puntales y cota.
- La vista: cámaras, encuadre y entorno.
- Los muros especiales puestos a mano (AutoCAD u otra aplicación): la línea con `IsSpecial` y las piezas colocadas (código, inserción y giro). El encofrado que calcula el visor no entra; se vuelve a generar al encofrar.

Importar sustituye los muros del diseño abierto. Si el archivo es de otro diseño, no reutiliza los id internos.

## Cómo leerlo

```text
node Desing/IA/tools/decrypt-wall-export.mjs ruta\al\archivo.tnwall
```

Escribe al lado un `.json`. La clave es `Tandem2026.WallExchange.v1`, AES-256-GCM. El archivo empieza por `TNWALL1`, luego el iv de 12 bytes y el cifrado con la etiqueta al final, en base64.

Código: `Desing/Scripts/Desing2/desing2-wall-exchange.js` y `ImportWallExchange` en `Desing_2Controller`.
