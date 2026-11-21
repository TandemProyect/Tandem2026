# Generar Informe Develop

Solo cómo hacer el informe US/BUG y el PDF. Nada más.

## Cuándo

Juan pide **informe**, **informe US y Bug**, **informe semanal** o el comando **Generar Informe Develop**.

Cadencia **semanal**. Si pide **dos últimas semanas**: resumen del semanal cerrado + detalle del abierto.

## Ficheros

| Qué | Dónde |
|-----|--------|
| Norma | `Desing/Report/LEEME.md` |
| Abierto ahora (16–29 nov 2026) | `Desing/Report/2026/Informe Semanal 16112026-29112026.md` y `.pdf` |
| Semanal anterior (cerrado 13/11) | `Desing/Report/2026/Informe Semanal 05112026-19112026.md` |
| HTML intermedio | No dejarlo en `Report/` |

Nombre: `Informe Semanal DDMMAAAA-DDMMAAAA.md` junto al PDF.

## Fuentes (obligatorio)

1. Chats de Cursor y de Visual Studio Code (Copilot).
2. `LEEME.md` y el informe **abierto**.
3. Documentación de área solo si el chat la citó.
4. Imágenes: solo las que el agente **recibió** (p. ej. `vscode-chat-images`). Si no están, no inventar.

**No** uses el historial de git como relato. **No** reescribas un informe cerrado; resúmelo si piden dos semanas.

Horas: Europe/Madrid. Reloj Copilot del tramo. No inventar. No sumar persona + agente.

## Estructura fija (desde 20/11/2026)

Informe **interno** (Juan Andrés y quien fiscaliza). Tres partes, en este orden:

### Parte 1. Apartados y subapartados

Solo titulares. Cada **subapartado**:

- Icono y categoría: 🟦 <span style="color:#1d4ed8"><strong>US</strong></span> o 🟥 <span style="color:#b91c1c"><strong>BUG</strong></span>
- Esfuerzo total (reloj Copilot del tramo; hijos: «incluido» si van en el padre)
- Estado

### Parte 2. Detalle

Por cada apartado de la parte 1: qué se hizo y qué se solucionó. Pegar las imágenes recibidas. Bugs del frente van aquí, no se reabre un apartado nuevo.

### Parte 3. Resumen

Solo apartados (número, nombre, tipo, estado). **Sin** subapartados ni detalle.

## Qué hacer (pasos)

1. Leer `Desing/Report/LEEME.md` y abrir el informe **abierto**.
2. Recoger trabajo desde la última actualización (chats, no git).
3. Actualizar el `.md` en las tres partes. No cerrar el periodo antes del día de corte.
4. Generar HTML A4 (Segoe UI, US azul `#1d4ed8`, BUG rojo `#b91c1c`, sin membrete).
5. PDF con Chrome headless:

```powershell
& "C:\Program Files\Google\Chrome\Application\chrome.exe" `
  --headless=new --disable-gpu --no-first-run --no-pdf-header-footer `
  --print-to-pdf="C:\Users\jag\00_Tandem2026\Desing\Report\2026\Informe Semanal DDMMAAAA-DDMMAAAA.pdf" `
  "file:///C:/Users/jag/.cursor/projects/c-00-Tandem2026/agent-tools/informe.html"
```

6. Borrar el HTML de `Report/` si se creó ahí. No commit, push ni stash.

## Catálogo Agentes (BD)

Fila `TSql_DevelodAgentDocument` con `TextCode = informe.generar-develop`. Script: `Desing/Scripts/TemporalScript/2026-11-20_seed_TSql_DevelodAgentDocument_informe_tres_partes.sql`. Catálogo: develop y producción.

## No hacer

- No abrir informe nuevo antes del día de corte.
- No reescribir el 05/11–19/11 (cerrado).
- No citar git como fuente.
- No dejar HTML en `Desing/Report/`.
- No mezclar las tres partes.
