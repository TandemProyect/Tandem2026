# Esquinas regulables ATK60 en Desing_2

Guía técnica para continuar implementación sin depender del contexto de chat.

## 1) Objetivo funcional

Implementar esquinas en L con tres modos:

- `Angular`
- `Universal_X` (panel regulable en eje X)
- `Universal_Y` (panel regulable en eje Y)

con estas reglas:

- decisiones por **vértice** (no por muro completo),
- geometría/cotas coherentes con la solución activa,
- encofrado por cara sin solapes ni duplicados.

## 2) Archivos clave

- UI y geometría 3D:
  - [master-article-details-stl-viewer.js](C:/00_Tandem2026/Desing/Scripts/MasterArticles/master-article-details-stl-viewer.js)
- Encofrado ATK60 (lado servidor):
  - [LCornerFormwork.cs](C:/00_Tandem2026/Desing/Repositories/Repository_Desing_2/Atk60/LCornerFormwork.cs)
  - [ExactWidthEndTape.cs](C:/00_Tandem2026/Desing/Repositories/Repository_Desing_2/Atk60/ExactWidthEndTape.cs)
  - [PillarFormwork.cs](C:/00_Tandem2026/Desing/Repositories/Repository_Desing_2/Atk60/PillarFormwork.cs)
  - [FormTie.cs](C:/00_Tandem2026/Desing/Repositories/Repository_Desing_2/Atk60/FormTie.cs)
- Persistencia de atributos:
  - [DesignWallRepository.cs](C:/00_Tandem2026/Desing/Repositories/RepositoryDesing2/DesignWallRepository.cs)
  - [FormworkDtos.cs](C:/00_Tandem2026/Desing/Repositories/RepositoryCommun/FormworkDtos.cs)

## 3) Datos de esquina que mandan

- Tipo global legacy: `_Tape_0`
- Tipo por extremo de eje: `_CornerTypeP1`, `_CornerTypeP2`

La resolución debe priorizar el extremo del vértice actual, con fallback a `_Tape_0`.

## 4) Reglas geométricas acordadas

- `Angular`: necesidad base `espesor opuesto + 0,30` (sin +0,03).
- `Universal_X/Y`: necesidad base `espesor opuesto + 0,30 + 0,03`.
- Universal aplica solución por cara:
  - cara regulable: panel regulable 0,75 + unión de esquina estilo pilar,
  - cara no regulable: panelización sin angular ni unión de esquina.

## 5) Regla de tirantes y unión en regulable

En cara regulable Universal:

- retirar grapa fija de unión de esquina,
- colocar hardware de unión tipo pilar:
  - gancho `1920811`,
  - placa/tuerca `10443020`,
  - en las alturas definidas por las bandas de `PillarFormwork`.

No confundir el gancho de unión `1920811` con otras piezas/caminos de grapa de `LCornerFormwork` (por ejemplo, sus constantes `1850164`).

En esquina interior, el criterio del usuario es que el tirante quede en el **panel interior**, no en el angular. Su presencia en el payload no demuestra que aparezca ni que esté bien colocado en el visor.

La rotación de la placa llegó a confirmarse visualmente por el usuario. La posición final de gancho y placa no está resuelta visualmente; no cambiar sus giros o probar offsets por conjetura.

## 6) Patrón de implementación recomendado

1. Resolver una `CornerPolicy` por vértice:
   - `cornerType`,
   - `regulatedFace` (run regulado),
   - `plainFace`.
2. Ejecutar generación por cara:
   - `regulatedFace`: 0,75 + unión/hardware pilar.
   - `plainFace`: sin angular + sin unión de esquina.
3. Evitar rutas duplicadas de inserción (una sola fuente de verdad por pieza).
4. Mantener `Angular` intacto.

## 7) Errores típicos a evitar

- Aplicar tipo de esquina a todo el muro (rompe esquinas múltiples en el mismo eje).
- No invalidar caché tras cambiar esquina (`wall3d` no regenera).
- Colocar tirante en el lado del angular por signo invertido de desplazamiento.
- Duplicar unión (ruta cover + ruta gap) sin filtro por cara.

## 8) Checklist de validación visual

Caso base: `muros-30009-20261103-1828.tnwall`.

1. Cambiar esquina a Universal vertical.
2. Verificar en cara regulable:
   - panel 0,75,
   - unión/hardware de pilar en agujeros,
   - sin grapa fija de unión de esquina.
3. Verificar en cara no regulable:
   - sin angular,
   - sin unión de esquina,
   - panelización estable.
4. Cambiar a Angular:
   - sin regresión de geometría y cotas.

## 9) Trabajo pendiente identificado

- Doble tirante cuando hay remate (fase siguiente).
- Consolidar tests automáticos de casos Universal_X, Universal_Y y Angular por vértice.

## 10) Historial de decisiones y lecciones

### Selección y persistencia de esquina

- El editor se añadió al doble clic sobre una esquina L en modo `wall3d`, con `Angular`, `Universal_X` y `Universal_Y`, vista previa de la selección, guardado y regeneración.
- `_Tape_0` es el valor legacy/fallback. Para muros con varias esquinas, el tipo debe ser local al extremo: `_CornerTypeP1` o `_CornerTypeP2` según el vértice. No propagar una selección local a todo el muro.
- Hubo un defecto de caché: el tipo cambiaba, pero la geometría no, porque el fingerprint del modelo no incluía la selección. Al cambiar una esquina, invalidar la caché y verificar que se regeneran tanto huella como cotas.
- El repositorio Desing_2 persiste `_Tape_0` en `TSql_DesignWall.TextTape_0`. No dar por hecho que cualquier atributo nuevo de `Extra` queda persistido: comprobar expresamente el mapeo DTO → entidad → DTO.

### Reglas geométricas y de encofrado

- Las reglas acordadas durante las iteraciones fueron `E + 300 mm` para Angular y `E + 300 mm + 30 mm` para Universal. En el caso Angular con `E = 220 mm`, se espera una necesidad de `520 mm`; el panel de `600 mm` deja un remate interior de `80 mm`. Confirmar estas magnitudes con el caso real antes de cambiar el resolvedor.
- Universal se decide por vértice y por cara: la cara regulable usa el módulo de `750 mm` y su unión; la cara no regulable no debe recibir una segunda unión ni un angular por efecto colateral.
- En una iteración, suprimir la grapa habitual también suprimía toda la rutina que generaba el tirante. En otra, un fallback de hardware lo insertó en el muro perpendicular. Mantener separadas las responsabilidades de unión y tirante, y filtrar por el `Run`/cara regulable.
- En el caso Angular con `E = 220 mm` se reportó un encofrado roto: paneles montados, con remate/tirante interior ausente. El backend usa `EndIsFree` (derivado de `_Idconnection_*`) para decidir si llama a `AddEnd`; una esquina conectada puede no ser tratada como extremo libre aunque necesite un cierre. Este caso sigue pendiente.
- Existe una desactivación temporal de `_IsFormwork` para el eje regulable Universal hasta una fase posterior. No quitarla sin revisar la petición que la motivó y probar Angular por separado.

### Unión, renderizado y ensayos descartados

- El gancho y la placa pueden existir en el JSON y aun así no verse. Se encontró que `ElementType=Union` seguía una transformación genérica distinta; alinear la unión con la pose estricta `Orientation="pillar"` fue necesario, pero no suficiente para corregir su posición.
- La cara de los herrajes quedó corregida: el usuario confirmó que estaban en el panel regulable. No reinsertar el mismo gancho/placa en el muro perpendicular cuando se suprime su grapa.
- `PillarFormwork.PlaceCornerHardware` es la referencia de bandas, extremos, caras y orientaciones; el legacy útil está en `ModuloWallEsqTLe` y `TapeRP_E50_00`. El legacy expresa sus coordenadas en centímetros (`EndWallX + 1`, `Position_Y + 6` para el gancho; `EndWallX + 12`, `Position_Y + 15` para la placa). No copiar esos valores al marco actual sin mapear origen, eje, cara y unidades.
- Se probaron offsets y traslaciones en más de un eje. Una corrección común preservaba la separación entre piezas pero no resolvía el acoplamiento. Una estimación de `33,91 mm` basada en cajas de los STL fue rechazada: la distancia entre superficies transformadas seguía siendo `44,23 mm`. No reutilizar ese valor ni usar bounds globales como prueba de contacto.
- El usuario pidió detener los ajustes especulativos, conservar/restaurar la versión que funcionaba aunque quedara desplazada y corregir después en AutoCAD. **El estado actual del archivo debe comprobarse antes de continuar:** `LCornerFormwork.PlaceCornerHardwareFromPillarPattern` todavía contiene overrides de posición X/Z (`LegacyCornerHookAlongMm = 10` y `LegacyCornerHookFaceBaseMm = 300 + otherThicknessMm`). No asumir que la restauración solicitada ya se hizo ni añadir más offsets sin decisión explícita.
- La placa se confirmó girada en el visor. No cambiar la rotación global de `PillarFormwork`: cualquier ajuste específico debe seguir limitado a la esquina y respetar la orientación ya aceptada.

## 11) Punto exterior e intercambio de referencias

- El punto de referencia es la intersección superior de las caras exteriores de la L; no los extremos inferiores ni la esquina interior. El usuario confirmó que el punto ya se veía.
- El `.tnwall` incluye `cornerReferencePoints`; los archivos antiguos sin ese campo siguen siendo válidos y deben reconstruir los puntos desde los muros importados. El centro visual del marcador puede tener un desplazamiento de render, pero el dato persistido debe ser la coordenada geométrica de la superficie.
- Los puntos también se guardan en `dbo.TSql_Design_V2.AttCornerReferencePointsJson`. La migración idempotente es `Desing/Scripts/TemporalScript/2026-11-05_alter_TSql_Design_V2_CornerReferencePointsJson.sql`.
- La migración quedó aplicada y verificada en desarrollo local (`.\SQLEXPRESS`) y producción (`SQL5113.site4now.net`). Ambos usan el catálogo `db_a197cd_desingproducction`, pero son servidores distintos; verificar cada instancia por separado.
- La importación autorizada del archivo de 30009 no llegó a ejecutarse porque el visor local permaneció en Login. No afirmar que ese diseño fue actualizado ni repetir la importación sin sesión autenticada.

## 12) Estado verificable a 5 nov 2026

| Aspecto | Estado |
|---|---|
| Selector Angular / Universal X / Universal Y, persistencia de tipo y regeneración | Implementado; los cambios por extremo y la invalidez de caché son importantes para evitar que parezca que “no cambia nada”. |
| Cara del hardware de unión | El usuario confirmó que es la cara regulable. |
| Giro de la placa | Confirmado visualmente por el usuario. |
| Posición final de gancho y placa | Pendiente; no hacer más ajustes de coordenadas por intuición. |
| Tirante interior / doble tirante con remate | Pendiente de comprobación visual y de resolver la interacción con extremos conectados. |
| Punto exterior superior | Confirmado visible por el usuario; exportación/importación `.tnwall` y persistencia SQL implementadas. |
| Migración SQL | Aplicada y verificada en las instancias local de desarrollo y producción. |
| Importación del diseño 30009 | No ejecutada; faltó autenticación del visor. |

Las compilaciones de MSBuild de Visual Studio 18 y pruebas focalizadas pasaron en iteraciones anteriores, pero no prueban la posición visual actual. No declarar resuelto el hardware o el tirante hasta comprobar el visor con una generación reciente. La página puede servir datos antiguos si la caché de `wall3d` no se invalida.

## 13) Protocolo para el siguiente agente

1. Leer primero este estado y el apartado de esquina del [informe semanal activo](../../../Desing/Report/2026/Informe%20Semanal%2005112026-19112026.md); no empezar otra vez por tantear offsets.
2. No modificar la geometría de gancho/placa hasta reconciliar el pedido de restaurar la versión estable con los overrides X/Z que siguen en `LCornerFormwork.cs`.
3. Si el usuario vuelve a autorizar ajuste de posición, medir puntos/superficies de contacto en el marco transformado del STL y confirmar eje/sentido con una captura del visor. Mantener las coordenadas restantes y la rotación confirmada.
4. Separar visualmente y en código cuatro resultados: panel regulable, unión gancho/placa, tirante interior y Angular. Un JSON correcto no sustituye la validación visual.
5. Probar como mínimo Universal_X, Universal_Y, Angular, ambos extremos del eje, dos esquinas en un muro, espesor `220 mm`, archivo `.tnwall` antiguo y exportar/importar con referencias.

## 14) Comandos y rutas de validación

Build de Desing en el entorno que tiene los targets WebApplication de Visual Studio:

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" `
  "C:\00_Tandem2026\Desing\Design.csproj" /t:Build /p:Configuration=Debug /m /nologo
```

`dotnet build` no sustituye ese comando en este proyecto .NET Framework: en este entorno falló por falta de `Microsoft.WebApplication.targets`. Para el PDF, seguir `Desing/Report/LEEME.md`; generar desde el Markdown vigente, conservar las imágenes enlazadas y no registrar horas no verificadas.
