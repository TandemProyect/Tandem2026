-- Paleta por ancho de los paneles ATK-60 en el maestro (TextColor1).
-- 0,30 magenta, 0,45 azul, 0,60 rojo, 0,75 verde, 0,90 negro.
-- Idempotente. No toca esquinas, grapas, rigidizadores, puntales ni Color 2.
SET NOCOUNT ON;

UPDATE a
   SET a.TextColor1 = CASE
        WHEN ABS(a.NumberWidth - 0.30) < 0.02 THEN N'#FF00FF'
        WHEN ABS(a.NumberWidth - 0.45) < 0.02 THEN N'#0000FF'
        WHEN ABS(a.NumberWidth - 0.60) < 0.02 THEN N'#FF0000'
        WHEN ABS(a.NumberWidth - 0.75) < 0.02 THEN N'#00FF00'
        WHEN ABS(a.NumberWidth - 0.90) < 0.02 THEN N'#000000'
        WHEN a.TextLabel LIKE N'%x 0,30%' OR a.TextLabel LIKE N'%x 0.30%' THEN N'#FF00FF'
        WHEN a.TextLabel LIKE N'%x 0,45%' OR a.TextLabel LIKE N'%x 0.45%' THEN N'#0000FF'
        WHEN a.TextLabel LIKE N'%x 0,60%' OR a.TextLabel LIKE N'%x 0.60%' THEN N'#FF0000'
        WHEN a.TextLabel LIKE N'%x 0,75%' OR a.TextLabel LIKE N'%x 0.75%' THEN N'#00FF00'
        WHEN a.TextLabel LIKE N'%x 0,90%' OR a.TextLabel LIKE N'%x 0.90%' THEN N'#000000'
        ELSE a.TextColor1
       END
  FROM dbo.Tsql_Master_Articles a
  JOIN dbo.TSql_System s ON s.IdObject = a.LinkSystem
 WHERE a.AddIsActive = 1
   AND REPLACE(REPLACE(UPPER(LTRIM(RTRIM(s.TextLabel))), N'-', N''), N' ', N'') = N'ATK60'
   AND (a.TextLabel LIKE N'Atk-60 %' OR a.TextLabel LIKE N'Panel Universal%')
   AND a.TextLabel NOT LIKE N'%Esquina%'
   AND ISNULL(a.TextColor1, N'') <> CASE
        WHEN ABS(a.NumberWidth - 0.30) < 0.02 THEN N'#FF00FF'
        WHEN ABS(a.NumberWidth - 0.45) < 0.02 THEN N'#0000FF'
        WHEN ABS(a.NumberWidth - 0.60) < 0.02 THEN N'#FF0000'
        WHEN ABS(a.NumberWidth - 0.75) < 0.02 THEN N'#00FF00'
        WHEN ABS(a.NumberWidth - 0.90) < 0.02 THEN N'#000000'
        WHEN a.TextLabel LIKE N'%x 0,30%' OR a.TextLabel LIKE N'%x 0.30%' THEN N'#FF00FF'
        WHEN a.TextLabel LIKE N'%x 0,45%' OR a.TextLabel LIKE N'%x 0.45%' THEN N'#0000FF'
        WHEN a.TextLabel LIKE N'%x 0,60%' OR a.TextLabel LIKE N'%x 0.60%' THEN N'#FF0000'
        WHEN a.TextLabel LIKE N'%x 0,75%' OR a.TextLabel LIKE N'%x 0.75%' THEN N'#00FF00'
        WHEN a.TextLabel LIKE N'%x 0,90%' OR a.TextLabel LIKE N'%x 0.90%' THEN N'#000000'
        ELSE ISNULL(a.TextColor1, N'')
       END;
