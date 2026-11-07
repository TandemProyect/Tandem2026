# Handoff técnico — `TANDEM_MURO2D` (OpenCADStudio)

## 1) Contexto y objetivo

Objetivo pedido por negocio: **paridad 1:1 con Desing/AutoCAD** para encuentros de muros 2D (L, T, +), incluyendo finalización por Enter/ESC, sin lógica geométrica divergente en el cliente.

Regla de arquitectura vigente: lógica geométrica en `Desing` y cliente CAD como renderer/transporte.

## 2) Estado actual (real)

- El usuario reporta que la paridad funcional **no está conseguida**.
- Los casos de esquina **L y T han presentado regresiones** entre iteraciones.
- En varias pruebas visuales del usuario, la T interior queda sin recorte correcto de cara (o aparecen artefactos de unión).
- Los tests automáticos del plugin Rust pasan, pero **no garantizan paridad visual real** en OpenCAD.

## 3) Qué sí está cableado

- Solver incremental remoto vía `DesignToolsAutocad/PluginWall2dStep`.
- Envío de `ExistingAxisSegments` desde OpenCAD al bridge y al endpoint C#.
- Finalización unificada por notificaciones (`Cancel` / `CommandStateChanged`).
- Rebuild del grupo de entidades en finalización y recorte adicional de cara en T desde cliente Rust.

## 4) Experiencia mala (para NO repetir)

1. **Desviación del flujo común**: se introdujo lógica geométrica local extra en Rust para “arreglar” T.
2. **Parcheo incremental sin baseline congelado**: arreglos de T rompieron L repetidamente.
3. **Validación incompleta**: tests unitarios verdes, pero sin paridad visual obligatoria por escenarios reales.
4. **Complejidad acumulada en finalización**: múltiples estrategias de snap/proyección/recorte local en cliente aumentaron riesgo de regresión.
5. **Tiempo/coste**: muchas iteraciones caras sin cerrar criterio único de aceptación.

## 5) Archivos clave tocados

- Cliente OpenCAD (Rust): `src/lib.rs`
- Bridge C#: `../OpenCadStudioPluging/MvcWall2dStepClient.cs`
- Bridge C#: `../OpenCadStudioPluging/BridgeCommandRunner.cs`
- API Desing: `../Desing/Controllers/DesignToolsAutocadController.cs`
- Solver C#: `../Desing/Services/CadWall2dService.cs`
- Contrato DTO: `../Desing/Models/ZwcadModels.cs`

## 6) Hipótesis técnica principal

La divergencia proviene de combinar:

- solver incremental remoto (C#) **más**
- recortes/ajustes posteriores locales (Rust),

lo cual produce diferencias en orden de operaciones y topología respecto al flujo común esperado.

## 7) Plan recomendado para el siguiente agente (estricto, bajo riesgo)

1. **Congelar baseline**: recuperar estado donde L era estable (antes de recortes locales complejos).
2. **Eliminar geometría local no esencial** en cliente para T/+ (evitar doble-solver).
3. Si falta comportamiento, moverlo al solver común C# (`CadWall2dService`) y exponerlo por contrato.
4. Mantener OpenCAD como renderer: sin heurísticas de topología nuevas en Rust.
5. Aceptar cambios solo si pasan:
   - tests automáticos,
   - paridad visual manual en casos L/T/+ con dataset reproducible.

## 8) Casos manuales mínimos obligatorios (aceptación)

1. **L exterior**: unión limpia, sin picos/triángulos, sin doble línea residual.
2. **T interior**: recorte correcto de cara del muro anfitrión (sin continuidad indebida).
3. **+ interior**: intersección completa coherente en ambas direcciones.
4. **ESC**: finalización limpia sin extremos reservados abiertos.
5. **Enter**: mismo resultado que ESC al cerrar tramo final.

## 9) Comandos de validación usados

```powershell
cd TandemOpenCadStudioPlugin
cargo test
cargo build --release
powershell -NoProfile -ExecutionPolicy Bypass -File install-local.ps1
```

Notas operativas:

- Si falla `install-local.ps1` por lock de DLL/EXE, cerrar procesos `OpenCADStudio.exe` y `OpenCadStudioPluging.exe`.

## 10) Conclusión para traspaso

El problema no es de documentación ni de falta de datos: la causa ha sido ejecución no suficientemente disciplinada respecto al flujo común.
El siguiente agente debería priorizar **paridad funcional** sobre “arreglos locales rápidos” en Rust.

