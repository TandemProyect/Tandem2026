# Cómo trabajar a partir de ahora (documentación y agentes)

Guía para Juan Andrés. Desde el 14/11/2026 la documentación está por áreas.

## 1. Qué decirle a un agente nuevo

Una frase:

> Lee `AGENTS.md` y trabaja en **<área>**: <lo que quieres>.

Ejemplos:

- «Lee AGENTS.md y trabaja en **dibujo 2D**: el recortar no corta bien en arcos.»
- «Lee AGENTS.md y trabaja en **encofrado ATK-60**: la T de 0,15 sale desplazada.»
- «Lee AGENTS.md y trabaja en **plugin ZWCAD**: el icono no aparece.»

No le digas «lee toda la documentación». Con `AGENTS.md` (unos 60 renglones) sabe las reglas y qué `INDEX.md` abrir. El `INDEX.md` le dice qué ficha abrir para cada tarea.

En Cursor, además, las reglas de `.cursor/rules/area-*.mdc` se cargan solas cuando el agente abre archivos de esa carpeta. Si trabajas en `TamdenZwcadPluging/`, no le llega nada del visor ni de OpenCAD.

Si la tarea cruza áreas, nómbralas: «trabaja en **encofrado ATK-60** y **plugin AutoCAD**». El agente leerá los dos INDEX y `Docs/codigo-comun.md`.

## 2. Cómo añadir un tema nuevo

1. Decide el área. Si no encaja en ninguna, crea `Docs/areas/<area-nueva>/INDEX.md` copiando la forma de otro INDEX, y añade una fila en la tabla de `AGENTS.md`.
2. Escribe la ficha en `Docs/areas/<area>/<tema>.md`. Un tema por ficha.
3. Añade una línea en la tabla «Si vas a…» del `INDEX.md` del área.
4. Si el tema usa código común, enlázalo en la sección «Código común» del INDEX y, si es código nuevo compartido, añádelo a `Docs/codigo-comun.md`.
5. Opcional: si el área tiene carpetas de código nuevas, añádelas a los `globs` de su `.cursor/rules/area-<area>.mdc`.

Los handovers de sesión van a `Docs/areas/<area>/historial/AAAA-MM-DD-<tema>.md`. Cuando algo del handover queda como regla, pásalo a la ficha y deja el handover en historial.

## 3. Cómo mantener los documentos pequeños

- `AGENTS.md`: 60 renglones como máximo. Solo reglas que valen para todo el repo.
- `INDEX.md` de área: 60 renglones como máximo. Es un mapa: qué leer para qué. No explica.
- Ficha de tema: 300 renglones o unos 8 000 tokens como máximo. Si crece, pártela por subtemas.
- Lo que ya pasó (sesiones, decisiones antiguas) va a `historial/`. La ficha guarda solo cómo está hoy.
- No copies texto de un área a otra: enlaza.
- Nada de «leer siempre al iniciar» fuera de `AGENTS.md`.
- Al acabar una tarea, pide al agente: «actualiza la ficha del área si algo cambió y no la hagas crecer sin necesidad».

## 4. Mapa rápido

```text
AGENTS.md                      entrada única (reglas + tabla de áreas)
Docs/codigo-comun.md           código que usan varias áreas y qué probar
Docs/COMO-TRABAJAR.md          esta guía
Docs/areas/<area>/INDEX.md     mapa del área
Docs/areas/<area>/*.md         fichas de tema
Docs/areas/<area>/historial/   handovers y sesiones pasadas
Docs/areas/general/            arquitectura, convenciones, Git (todo el repo)
Docs/historial/MAPA-RUTAS-2026-11.md   dónde está ahora cada documento movido
Docs/historial/                concepto inicial (abril 2026)
.cursor/rules/area-*.mdc       reglas que Cursor carga por carpeta
```
