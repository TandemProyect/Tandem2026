-- Paneles universales ATK-60 0,75: el visor recuenta el GLB (27754219 / 24754224 / 12754120).
-- El maestro comercial ya existe (27104219 / 24104224 / 12104120 + Atenko 3120xx0075).
-- No se duplican artículos. El recuento mapea GLB → TextCode en Atk60MaterialCountCodeMap.
-- Idempotente: solo rellena TextLabel / AddAtenkoCode si están vacíos.
SET NOCOUNT ON;

UPDATE a
   SET a.TextLabel = N'Panel Universal 2,70 x 0,75'
  FROM dbo.Tsql_Master_Articles a
 WHERE a.AddIsActive = 1
   AND a.TextCode = N'27104219'
   AND LTRIM(RTRIM(ISNULL(a.TextLabel, N''))) = N'';

UPDATE a
   SET a.AddAtenkoCode = N'3120270075'
  FROM dbo.Tsql_Master_Articles a
 WHERE a.AddIsActive = 1
   AND a.TextCode = N'27104219'
   AND LTRIM(RTRIM(ISNULL(a.AddAtenkoCode, N''))) = N'';

UPDATE a
   SET a.TextLabel = N'Panel Universal 2,40 x 0,75'
  FROM dbo.Tsql_Master_Articles a
 WHERE a.AddIsActive = 1
   AND a.TextCode = N'24104224'
   AND LTRIM(RTRIM(ISNULL(a.TextLabel, N''))) = N'';

UPDATE a
   SET a.AddAtenkoCode = N'3120240075'
  FROM dbo.Tsql_Master_Articles a
 WHERE a.AddIsActive = 1
   AND a.TextCode = N'24104224'
   AND LTRIM(RTRIM(ISNULL(a.AddAtenkoCode, N''))) = N'';

UPDATE a
   SET a.TextLabel = N'Panel Universal 1,20 x 0,75'
  FROM dbo.Tsql_Master_Articles a
 WHERE a.AddIsActive = 1
   AND a.TextCode = N'12104120'
   AND LTRIM(RTRIM(ISNULL(a.TextLabel, N''))) = N'';

UPDATE a
   SET a.AddAtenkoCode = N'3120120075'
  FROM dbo.Tsql_Master_Articles a
 WHERE a.AddIsActive = 1
   AND a.TextCode = N'12104120'
   AND LTRIM(RTRIM(ISNULL(a.AddAtenkoCode, N''))) = N'';

SELECT TextCode, AddAtenkoCode, TextLabel
  FROM dbo.Tsql_Master_Articles
 WHERE AddIsActive = 1
   AND TextCode IN (N'27104219', N'24104224', N'12104120')
 ORDER BY TextCode;

PRINT N'OK — universales 0,75 comprobados (sin duplicar 27754219).';
