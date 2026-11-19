-- Color de panel ATK60 en el maestro (TextColor1). No crea columnas: ya existen.
-- Solo rellena el marcador 0 o vacío. Un color ya guardado no se toca.
SET NOCOUNT ON;

UPDATE a
   SET a.TextColor1 = CASE
        WHEN ABS(a.NumberWidth - 0.30) < 0.02 THEN N'#FF00FF'
        WHEN ABS(a.NumberWidth - 0.45) < 0.02 THEN N'#0000FF'
        WHEN ABS(a.NumberWidth - 0.60) < 0.02 THEN N'#FF0000'
        WHEN ABS(a.NumberWidth - 0.75) < 0.02 THEN N'#00FF00'
        WHEN ABS(a.NumberWidth - 0.90) < 0.02 THEN N'#000000'
        WHEN a.TextLabel LIKE N'%0,75%' OR a.TextLabel LIKE N'%0.75%' THEN N'#00FF00'
        ELSE a.TextColor1
       END
  FROM dbo.Tsql_Master_Articles a
  JOIN dbo.TSql_System s ON s.IdObject = a.LinkSystem
 WHERE a.AddIsActive = 1
   AND REPLACE(REPLACE(UPPER(LTRIM(RTRIM(s.TextLabel))), N'-', N''), N' ', N'') = N'ATK60'
   AND (a.TextLabel LIKE N'Atk-60 %' OR a.TextLabel LIKE N'Panel Universal%')
   AND a.TextLabel NOT LIKE N'%Esquina%'
   AND (a.TextColor1 IS NULL OR LTRIM(RTRIM(a.TextColor1)) IN (N'', N'0'));
