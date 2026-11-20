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

**No** uses el historial de git como relato. **No** reescribas un informe cerrado; resúmelo si piden dos semanas.

Horas: Europe/Madrid. Tres relojes (persona, Cursor, Copilot). No inventar. No sumar persona + agente.

## Convención US / BUG

Cada tema:

- Tipo: <span style="color:#1d4ed8"><strong>US</strong></span> o <span style="color:#b91c1c"><strong>BUG</strong></span> (HTML, no solo markdown).
- Estado, detalle técnico, imágenes si existen (`Desing/Report/2026/img/`), horas del detalle.

Bugs de un frente van como sublista del **US** padre.

## Qué hacer (pasos)

1. Leer `Desing/Report/LEEME.md` y abrir el informe **abierto**.
2. Recoger trabajo desde la última actualización (chats, no git).
3. Si piden dos semanas: añadir un apartado «Semana anterior» con tabla US/BUG del MD cerrado (sin copiar páginas).
4. Actualizar el `.md` abierto. No cerrar el periodo antes del día de corte.
5. Generar HTML A4 (estilo `informe-quincenal-01.html`: Segoe UI, US azul `#1d4ed8`, BUG rojo `#b91c1c`, sin membrete).
6. PDF con Chrome headless:

```powershell
& "C:\Program Files\Google\Chrome\Application\chrome.exe" `
  --headless=new --disable-gpu --no-first-run --no-pdf-header-footer `
  --print-to-pdf="C:\Users\jag\00_Tandem2026\Desing\Report\2026\Informe Semanal DDMMAAAA-DDMMAAAA.pdf" `
  "file:///C:/Users/jag/.cursor/projects/c-00-Tandem2026/agent-tools/informe.html"
```

7. Borrar el HTML de `Report/` si se creó ahí. No commit, push ni stash.

## Contenido del PDF

- Portada: periodo, fichero, fecha, fuentes.
- Temas numerados US/BUG.
- Tabla resumen (n.º, tema, tipo, estado).
- Tabla de horas por frente.

## No hacer

- No abrir informe nuevo antes del día de corte.
- No reescribir el 05/11–19/11 (cerrado).
- No citar git como fuente.
- No dejar HTML en `Desing/Report/`.
