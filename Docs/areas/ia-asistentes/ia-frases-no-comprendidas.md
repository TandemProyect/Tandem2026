# Frases que la IA no comprende (§16.15)

Desde el 16/11/2026 (pt119u), imports `?v=20261116a`.

Cuando un asistente contesta «No he entendido…», la frase se guarda en el servidor para revisarla después. El usuario no ve nada y, si falla, no sale ningún error.

## Dónde salta

| Asistente | Momento | `asistente` | `contexto` |
|-----------|---------|-------------|------------|
| IA del diseño (`design-ai-assistant.js`, `maStlDesignAiHandleMessage`) | Último recurso: «No he entendido «…»» + ayuda | `design` | `modo` (`lines` / `wall3d` / `formwork`), `muroSeleccionado` (número que se ve en Cotas), `respuesta` |
| IA del muro (`wall-ai-assistant.js`, `maStlWallAiHandleMessage`) | Último recurso: `T.help` («No he entendido «…»…») | `wall` | `tipoMuro`, `muro`, `fase: "chat"`, `respuesta` |
| IA del muro, cambio guiado de paneles | `swapNotUnderstood` | `wall` | igual, `fase: "cambio de paneles"` |

No se guardan: órdenes entendidas, «¿Querías decir…?», «no disponible» (recortar, empalme) ni las órdenes pendientes de IA (contestan «aún no lo hago por voz»).

El envío es `maStlAiLogUnknownPhrase` de `ai-command-catalog.js`: `fetch` POST con `keepalive`, sin esperar. La URL sale de `data-ma-stl-ai-commands-url` (`…/Desing_2/AiCommands` → `…/Desing_2/SaveAiUnknownPhrase`). La paleta de los plugins CAD no tiene esa URL: allí no se guarda nada.

## Archivo

`Desing/App_Data/IA/frases-no-comprendidas.json` (en el servidor, `~/App_Data/IA/…`). Lo crea el primer «no entendido». Está en `.gitignore`. El FTP de despliegue no lo borra (solo toca lo que sube), y `App_Data` no se sirve por web.

UTF-8 sin BOM, indentado, escrito con candado (`lock`) y archivo temporal. Cada entrada:

```json
{
  "id": 1,
  "fecha": "2026-11-16T09:10:00+01:00",
  "ultimaFecha": "2026-11-16T09:12:30+01:00",
  "frase": "pon un tejado verde",
  "asistente": "design",
  "idioma": "es",
  "contexto": { "modo": "lines", "muroSeleccionado": "7", "respuesta": "No he entendido «pon un tejado verde»." },
  "usuario": "jag@…",
  "veces": 2,
  "estado": "pendiente",
  "comandoAsignado": null
}
```

- Horas de Madrid con su desfase.
- Misma frase normalizada (sin mayúsculas, tildes ni signos) del mismo asistente: no se repite; suma `veces` y cambia `ultimaFecha` y `contexto`.
- Si una entrada `resuelta` vuelve a fallar, pasa otra vez a `pendiente` (conserva `comandoAsignado`).
- `frase` hasta 500 caracteres; `contexto` hasta 12 claves de 1000 caracteres.

## Endpoints (`Desing_2Controller.AiUnknownPhrases.cs`, con sesión iniciada)

| Método | URL | Cuerpo / parámetros | Respuesta |
|--------|-----|---------------------|-----------|
| POST | `Desing_2/SaveAiUnknownPhrase` | `{ frase, asistente, idioma?, contexto? }` | `{ ok, id, veces, nueva }` |
| GET | `Desing_2/AiUnknownPhrases?estado=pendiente` | `estado` vacío = todas | `{ ok, archivo, total, pendientes, items[] }` (más vistas primero) |
| POST | `Desing_2/ResolveAiUnknownPhrase` | `{ id, comandoAsignado?, estado?: "resuelta" \| "descartada" \| "pendiente", nota? }` | `{ ok, item }` (añade `fechaResuelta`, `resueltaPor`) |

Nunca lanzan: si algo falla, `{ ok: false, message }`.

## Revisión con Juan Andrés (en el chat)

1. Él dice «abre el .json». El agente lee el archivo (en local `Desing\App_Data\IA\frases-no-comprendidas.json`; en el servidor, `GET Desing_2/AiUnknownPhrases?estado=pendiente` con su sesión).
2. Se analizan una a una las `pendiente`: qué quería decir y a qué comando va (`TSql_AiCommand.TextCode`), o si se descarta.
3. Las comprendidas se añaden como frases (`TSql_AiCommandPhrase`) en las **dos** BD (develop y producción, idénticas) con un script idempotente en `Desing/Scripts/TemporalScript/` (patrón RESEED), y, si hace falta, al vocabulario JS (`MA_STL_DESIGN_AI_WORDS` en `design-ai-assistant.js` o las listas de `ai-command-catalog.js`). Se sube `?v=`.
4. Se marcan con `POST Desing_2/ResolveAiUnknownPhrase` `{ id, comandoAsignado: "WALL_IDS" }`, o editando el JSON a mano (`estado: "resuelta"`, `comandoAsignado`).

La IA del visor **no** tiene orden «abre el .json»: la revisión se hace en el chat.

## Pruebas

- C#: arnés `net8` con el controlador real (`/workspace/ia/p32/ia/csh2`): 21 OK (alta, duplicados, otro asistente, vacía, JSON roto, recorte, 40 hilos a la vez, lista, resolver, descartar, reabrir, sin `.tmp`).
- JS: `unit/test-unknown.mjs`, 16 OK (URL, cuerpo, solo en «no entendido», red caída sin error, paleta sin URL). Suites: unit 170, catálogo 89, p31 33, p30 29, p29 28, p23 15, p21 87, p20 56.
