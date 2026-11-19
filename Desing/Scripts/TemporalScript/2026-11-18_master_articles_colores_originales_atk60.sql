-- Colores originales del render ATK-60 en el maestro de artículos.
-- Paneles: amarillo de estructura. Rigidizadores: azul. Grapas y tuercas: metal.
-- Idempotente. No toca Color 2 (fenólico) ni artículos de otros sistemas.
SET NOCOUNT ON;

UPDATE dbo.Tsql_Master_Articles
   SET TextColor1 = N'#EFB608'
 WHERE LinkSystem = 1
   AND AddIsActive = 1
   AND TextLabel NOT LIKE N'%Esquina%'
   AND (TextLabel LIKE N'Atk-60 %' OR TextLabel LIKE N'Panel Universal %')
   AND ISNULL(TextColor1, N'') <> N'#EFB608';

UPDATE dbo.Tsql_Master_Articles
   SET TextColor1 = N'#2A7AB8'
 WHERE LinkSystem = 1
   AND TextCode IN (N'1850162', N'1850163', N'1850164', N'10004225', N'1920811')
   AND ISNULL(TextColor1, N'') <> N'#2A7AB8';

UPDATE dbo.Tsql_Master_Articles
   SET TextColor1 = N'#B8BCC4'
 WHERE LinkSystem = 1
   AND TextCode IN (N'10000221', N'10004220', N'10443020', N'7238001')
   AND ISNULL(TextColor1, N'') <> N'#B8BCC4';

-- Puntal exterior #908F8E. El tubo interior (#DCDBDA) no es un artículo distinto.
UPDATE dbo.Tsql_Master_Articles
   SET TextColor1 = N'#908F8E'
 WHERE LinkSystem = 1
   AND AddIsActive = 1
   AND TextLabel LIKE N'%untal%'
   AND ISNULL(TextColor1, N'') <> N'#908F8E';
