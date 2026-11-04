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
- colocar hardware tipo pilar:
  - gancho `1850164`,
  - placa `10443020`,
  - por alturas de agujero según banda.

En esquina interior:

- el tirante debe quedar en el **panel interior**, no en el angular.

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
