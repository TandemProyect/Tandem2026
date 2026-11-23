# US y Bugs: valoración y arreglo por un agente

Vale para cualquier agente de programación (Devin, Cursor, VS Code/Copilot…). Se activa cuando el usuario dice «valora la id N» o «arregla el bug o la US id N».

Registro: `TSql_DevelopIssue` (US o bug), `TSql_DevelopIssueDetail` (observaciones y capturas) y `TSql_DevelopIssueAssessment` (valoraciones). Pantalla: `/DevelopIssue`. Código: `DevelopIssueController`, `DevelopIssueRepository`, `DevelopIssueAssessmentService`.

## Dos valoraciones

| Origen (`TextSource`) | Quién | Qué ve |
|---|---|---|
| `AI` | Botón «Valorar con IA» de la ficha (OpenAI `gpt-4o`, clave `OPENAI_APIKEY`) | Título, observaciones y hasta 6 capturas. No ve el código: es orientativa. |
| `AGENT` | Cualquier agente de programación, cuando se le pide | Lo mismo más el código del repositorio. |

Tamaño (`TextSize`): `SMALL` hasta 2 h, `MEDIUM` de 2 a 8 h, `LARGE` más de 8 h o con decisiones de diseño.

## Acceso a las fichas: dos vías

Usa la que tengas disponible. Las fichas reales están en producción (`db_a197cd_desingproducction`), que es también la base del entorno develop.

### Vía A — SQL directo (agente en el equipo del desarrollador: Cursor, VS Code)

Conexión: la cadena `IdentityConnection` de `Desing/Web.config` (no mostrar ni copiar la contraseña). Con `sqlcmd` usar `-I` (los índices filtrados exigen `QUOTED_IDENTIFIER ON`).

```sql
-- Leer la ficha N
SELECT IdObject, TextLabel, TextType, TextUrgency, TextState, DateIssue
FROM dbo.TSql_DevelopIssue WHERE IdObject = @N AND Is_Delete = 0;
SELECT IdObject, TextObservation, TextImagePath, AddDateMade
FROM dbo.TSql_DevelopIssueDetail WHERE LinkDevelopIssue = @N AND Is_Delete = 0 ORDER BY NumberOrder, IdObject;
SELECT TextSource, [TextSize], NumberHours, TextArea, TextAnalysis, AddDateMade
FROM dbo.TSql_DevelopIssueAssessment WHERE LinkDevelopIssue = @N AND Is_Delete = 0 ORDER BY IdObject DESC;

-- Anotar valoración (TextModel = nombre del agente: devin, cursor, copilot…)
INSERT INTO dbo.TSql_DevelopIssueAssessment
  (TextLabel, LinkDevelopIssue, TextSource, [TextSize], NumberHours, TextArea, TextAnalysis, TextModel, LinkMadeBy)
VALUES (N'AGENT SMALL', @N, N'AGENT', N'SMALL', 1.5, N'zona afectada', N'análisis', N'cursor', N'agent');

-- Anotar observación
INSERT INTO dbo.TSql_DevelopIssueDetail (TextLabel, LinkDevelopIssue, NumberOrder, TextObservation, LinkMadeBy)
SELECT N'Nota del agente', @N, ISNULL(MAX(NumberOrder), 0) + 1, N'qué se cambió y cómo probarlo', N'agent'
FROM dbo.TSql_DevelopIssueDetail WHERE LinkDevelopIssue = @N;

-- Cambiar estado (auditoría obligatoria)
UPDATE dbo.TSql_DevelopIssue
SET TextState = N'IN_PROGRESS', LinModifiedBy = N'agent', AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
WHERE IdObject = @N AND Is_Delete = 0;
```

Capturas: `TextImagePath` es una ruta relativa al sitio (`/Files/DevelopIssue/aaaa-mm/…`). Descargar de `https://www.tdesing.net` + ruta; las creadas en local están en `Desing/Files/DevelopIssue/`. Nunca `DELETE`.

### Vía B — HTTP con token (agente remoto sin acceso a la base: Devin)

Cabecera `X-Develop-Agent-Token` con el valor de `DEVELOP_AGENT_TOKEN` (variable de entorno o `appSettings` del servidor, mínimo 32 caracteres). Sin token configurado, las tres acciones responden 404. El token nunca va al repositorio.

| Acción | Uso |
|---|---|
| `GET /DevelopIssue/AgentList` | US y bugs sin cerrar, con su última valoración. |
| `GET /DevelopIssue/AgentIssue?id=N` | Ficha: datos, observaciones, `imageUrl` de cada captura y valoraciones. |
| `POST /DevelopIssue/AgentReport` | Campos opcionales: `size`+`hours`+`area`+`analysis` (valoración), `observation` (nota en el detalle), `state`. |

```bash
BASE=https://www.tdesing.net
curl -s -H "X-Develop-Agent-Token: $DEVELOP_AGENT_TOKEN" "$BASE/DevelopIssue/AgentIssue?id=34"
curl -s -H "X-Develop-Agent-Token: $DEVELOP_AGENT_TOKEN" -X POST "$BASE/DevelopIssue/AgentReport" \
  --data-urlencode id=34 --data-urlencode size=SMALL --data-urlencode hours=1.5 \
  --data-urlencode "area=Views/DevelopIssue/Index.cshtml" --data-urlencode "analysis=…"
```

## Procedimiento: «valora la id N» / «arregla el bug o la US id N»

1. Leer la ficha (vía A o B) y abrir todas las capturas.
2. Localizar en el repositorio los archivos afectados (por áreas, según `AGENTS.md`).
3. Valorar y anotar la valoración en la ficha (tamaño, horas, zona, análisis).
4. Si solo se pidió valorar, parar aquí y resumir al usuario.
5. Si se pidió arreglar:
   - `LARGE`: no arreglar. Explicar la valoración y preguntar.
   - `SMALL` o `MEDIUM`: pasar a `IN_PROGRESS`, hacer el cambio en la rama `develo` siguiendo las reglas del área, compilar y probar.
   - Anotar como observación qué se cambió y cómo probarlo.
6. No pasar a `CLOSED`: lo cierra el usuario después de probarlo. No hacer commit, push ni publicar sin que lo pida.
7. Si la ficha no basta para entender el problema, anotar la duda como observación y preguntar al usuario.
