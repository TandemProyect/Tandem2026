# Handover Tecnico - Imagen y Tipificacion de Muros (ZWCAD)

## Objetivo funcional consolidado

Partiendo de imagen o de lineas/polilineas CAD:

1. Detectar esquinas L de forma estable.
2. Generar muros rectos por tipo de conectividad.
3. Mantener la regla de cotas: linea exterior como referencia y espesor hacia adentro.
4. Evitar errores repetidos de endpoints, solapes y residuos decimales.

> Regla de negocio confirmada con usuario:
> - El perimetro detectado representa cota exterior (ejemplo `10000 x 5000`).
> - El espesor `E/e` se aplica hacia adentro.
> - Para muros conectados, los extremos deben nacer/morir en puntos de esquina.

---

## Archivos clave que concentran la logica

### Servidor

- `Desing/Services/LCornerDetector.cs` (nucleo de deteccion/tipificacion)
- `Desing/Controllers/DesignToolsAutocadController.cs` (normalizacion, flujo CAD e imagen)
- `Desing/Services/ImageAnalysisService.cs` (extraccion E/H desde imagen)

### Cliente

- `TamdenZwcadPluging/ZwcadPlugin/Commands.cs`
- `TamdenZwcadPluging/ZwcadPlugin/MVCApiService.cs`

---

## Flujo correcto (orden obligatorio)

1. Normalizar entrada (snap de coordenadas).
2. Detectar esquinas L.
3. Construir puntos de referencia de esquina.
4. Generar muros rectos por tipificacion:
   - Tipo 1 primero (conectado-conectado, corner-first).
   - Tipo 2 y 3 (un extremo conectado, otro libre).
   - Tipo 4 (ambos libres, aislados).
5. Emitir polilineas y puntos al plugin.
6. Registrar diagnostico para trazabilidad.

No volver al enfoque anterior de "solo endpoints exactos" para tipo 1.

---

## Tipificacion de muros (definicion oficial)

- `Tipo1_AmbosExtremosConectados`: nace y muere en esquina.
- `Tipo2_InicioConectado_FinLibre`: inicio en esquina, final libre.
- `Tipo3_InicioLibre_FinConectado`: inicio libre, final en esquina.
- `Tipo4_AmbosExtremosLibres`: sin conexion en ambos extremos.

Implementacion actual:

- Tipo 1: `GenerarMurosTipo1DesdeEsquinas(...)`
- Tipo 2/3 (+ tipo 1 fallback por lineas): `GenerarMurosConUnExtremoLibreDesdeLineas(...)`
- Tipo 4: `GenerarMurosLibresAislados(...)`

---

## Cambios criticos que resolvieron el bug recurrente

## 1) Tipo 1 ya no depende de endpoints exactos

Problema historico:

- se perdian muros tipo 1 porque la esquina podia caer sobre el segmento, pero no en `Inicio/Fin`.

Correccion aplicada:

- `PuntoEsEndpointDeLinea(...)` fue reemplazado por chequeo de punto sobre segmento con tolerancia (`PuntoSobreSegmentoConTolerancia(...)`).
- estaciones de tipo 1 se alimentan con:
  - vertices de esquinas detectadas
  - puntos de referencia (`PuntosADibujar`)

Resultado:

- se recuperan muros tipo 1 faltantes en casos con polilinea expandida.

## 2) Corner-first real para muros conectados

- Tipo 1 se genera primero usando estaciones comunes entre caras paralelas.
- En tipos 1/2/3, los extremos conectados se anclan/snapean a puntos de esquina (`TrySnapExtremoConectadoConPuntos`).

## 3) Tipificacion explicita y separacion por metodo

- Se elimino ambiguedad entre muros conectados y muros aislados.
- Tipo 4 se descarta del metodo de conectados y se delega al aislado para evitar duplicados.

## 4) Diagnostico persistente para no repetir errores

Se agrego salida de diagnostico con trazabilidad de cada par de lineas:

- Archivo: `C:\temp\diagnostico_muros_rectos.json`
- Incluye:
  - lineas de entrada
  - esquinas detectadas
  - puntos/polilineas de salida
  - `DebugMurosRectos` por metodo/tipo
  - estado `Generado`/`Descartado`
  - motivo de descarte (`no paralelas`, `sin estaciones comunes`, `largo minimo`, `duplicado`, etc.)

Tambien se mantiene:

- `C:\temp\conexiones.json` (resumen amplio del detector)

---

## Flujo imagen (E/H) y normalizacion

En `ImageAnalysisService.cs`:

- prompt actualizado para extraer `espesorMuro` (`E/e`) y `alturaMuro` (`H/h`).
- parse robusto aceptando coma o punto decimal.

En `DesignToolsAutocadController.cs`:

- validacion:
  - sin lineas: error guiado
  - sin espesor: error bloqueante
  - sin altura: default 2.70m con aviso
- normalizacion de entrada con snap para reducir ruido numerico.
- conversion de lineas a caras interior/exterior para reutilizar detector comun.

---

## Errores historicos y como evitarlos

1. **Muro creado pero fuera de esquina**
   - Causa: geometria no recortada a solape o sin anclaje a puntos de esquina.
   - Prevencion: recorte por overlap + snap de extremos conectados.

2. **Muros tipo 1 faltantes**
   - Causa: chequeo por endpoint exacto.
   - Prevencion: usar punto sobre segmento con tolerancia.

3. **Deriva decimal en cotas**
   - Causa: residuos de intersecciones y offsets.
   - Prevencion: snap entero temprano y despues de ajustes geometricos.

4. **Duplicados de muro**
   - Causa: deteccion en metodos superpuestos.
   - Prevencion: claves canonicas por par de lineas + delegacion tipo 4.

---

## Checklist minimo para siguiente agente

Antes de tocar logica:

1. Ejecutar caso base y revisar `diagnostico_muros_rectos.json`.
2. Confirmar conteo por tipo (1/2/3/4) y motivos de descarte.
3. Verificar que todo tipo 1 generado nace/muere en punto de esquina.
4. Revisar que no se reintrodujo chequeo endpoint-only.
5. Validar cotas finales sin residuos decimales.

Si un muro no aparece:

- buscar en `DebugMurosRectos` el par de lineas y leer `Motivo`.
- ajustar solo la regla puntual; no reescribir flujo completo.

---

## Criterio de aceptacion operativo

Caso nominal rectangular con `E=0,30` y `H=2,70`:

1. Exterior conserva cota principal.
2. Interior respeta `-2E` por dimension global.
3. Esquinas cerradas, sin extensiones fuera de esquina.
4. Conteo de muros consistente con tipificacion.
5. Diagnostico JSON explica cada muro generado o descartado.

---

## Nota de seguridad para scripts

No subir scripts con tokens/PAT embebidos.

- Si se necesita automatizacion DevOps, usar variables de entorno o secreto seguro.
- Cualquier token expuesto debe rotarse antes de commit.

