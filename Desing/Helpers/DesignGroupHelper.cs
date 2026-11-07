using System;
using System.Data.Entity;

namespace Desing.Helpers
{
    /// <summary>
    /// Grupo inicial de un diseño: número 1, nombre «por decidir».
    /// Un número borrado no se reutiliza; en un diseño nuevo el 1 está libre.
    /// </summary>
    public static class DesignGroupHelper
    {
        public const string DefaultName = "por decidir";

        public static void EnsureFirstGroup(Database database, long designId, string userId, DateTime now)
        {
            if (database == null || designId <= 0)
            {
                return;
            }

            EnsureGroupTable(database);

            var uid = string.IsNullOrWhiteSpace(userId) ? "system" : userId.Trim();
            if (uid.Length > 128)
            {
                uid = uid.Substring(0, 128);
            }

            database.ExecuteSqlCommand(
                @"IF NOT EXISTS (
                      SELECT 1 FROM dbo.TSql_DesgloseGrupo
                      WHERE LinkDesign_V2 = @p0 AND Is_Delete = 0)
                  INSERT INTO dbo.TSql_DesgloseGrupo
                      (TextLabel, LinkDesign_V2, NumberGroup, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
                  VALUES (@p1, @p0, 1, 0, 1, @p2, NULL, @p3, NULL, 0)",
                designId,
                DefaultName,
                uid,
                now);
        }

        private static void EnsureGroupTable(Database database)
        {
            database.ExecuteSqlCommand(
                @"IF OBJECT_ID(N'dbo.TSql_DesgloseGrupo', N'U') IS NULL
                  BEGIN
                    CREATE TABLE [dbo].[TSql_DesgloseGrupo]
                    (
                      [IdObject] BIGINT IDENTITY(1,1) NOT NULL
                        CONSTRAINT [PK_TSql_DesgloseGrupo] PRIMARY KEY CLUSTERED,
                      [TextLabel] NVARCHAR(500) NOT NULL,
                      [LinkDesign_V2] BIGINT NOT NULL,
                      [NumberGroup] BIGINT NOT NULL,
                      [Is_Delete] BIT NOT NULL CONSTRAINT [DF_TSql_DesgloseGrupo_Is_Delete] DEFAULT (0),
                      [Is_Active] BIT NOT NULL CONSTRAINT [DF_TSql_DesgloseGrupo_Is_Active] DEFAULT (1),
                      [LinkMadeBy] NVARCHAR(128) NOT NULL,
                      [LinModifiedBy] NVARCHAR(128) NULL,
                      [AddDateMade] DATETIME NOT NULL CONSTRAINT [DF_TSql_DesgloseGrupo_AddDateMade] DEFAULT (GETDATE()),
                      [AddLastDateChange] DATETIME NULL,
                      [Ntimeschanged] BIGINT NOT NULL CONSTRAINT [DF_TSql_DesgloseGrupo_Ntimeschanged] DEFAULT (0)
                    );
                  END");

            database.ExecuteSqlCommand(
                @"IF OBJECT_ID(N'dbo.TSql_Design_V2', N'U') IS NOT NULL
                    AND NOT EXISTS (
                      SELECT 1
                      FROM sys.foreign_keys
                      WHERE name = N'FK_TSql_DesgloseGrupo_TSql_Design_V2'
                        AND parent_object_id = OBJECT_ID(N'dbo.TSql_DesgloseGrupo')
                    )
                  BEGIN
                    ALTER TABLE [dbo].[TSql_DesgloseGrupo] WITH CHECK
                    ADD CONSTRAINT [FK_TSql_DesgloseGrupo_TSql_Design_V2]
                      FOREIGN KEY ([LinkDesign_V2]) REFERENCES [dbo].[TSql_Design_V2] ([SysObjectID]);
                  END");

            database.ExecuteSqlCommand(
                @"IF NOT EXISTS (
                      SELECT 1 FROM sys.indexes
                      WHERE name = N'UX_TSql_DesgloseGrupo_Design_NumberGroup'
                        AND object_id = OBJECT_ID(N'dbo.TSql_DesgloseGrupo')
                    )
                  CREATE UNIQUE NONCLUSTERED INDEX [UX_TSql_DesgloseGrupo_Design_NumberGroup]
                  ON [dbo].[TSql_DesgloseGrupo] ([LinkDesign_V2], [NumberGroup])
                  WHERE [Is_Delete] = 0;");

            database.ExecuteSqlCommand(
                @"IF NOT EXISTS (
                      SELECT 1 FROM sys.indexes
                      WHERE name = N'IX_TSql_DesgloseGrupo_TextLabel_Active'
                        AND object_id = OBJECT_ID(N'dbo.TSql_DesgloseGrupo')
                    )
                  CREATE NONCLUSTERED INDEX [IX_TSql_DesgloseGrupo_TextLabel_Active]
                  ON [dbo].[TSql_DesgloseGrupo] ([TextLabel])
                  INCLUDE ([Is_Active])
                  WHERE [Is_Delete] = 0;");

            database.ExecuteSqlCommand(
                @"IF NOT EXISTS (
                      SELECT 1 FROM sys.indexes
                      WHERE name = N'IX_TSql_DesgloseGrupo_LinkDesign_Active'
                        AND object_id = OBJECT_ID(N'dbo.TSql_DesgloseGrupo')
                    )
                  CREATE NONCLUSTERED INDEX [IX_TSql_DesgloseGrupo_LinkDesign_Active]
                  ON [dbo].[TSql_DesgloseGrupo] ([LinkDesign_V2])
                  INCLUDE ([NumberGroup], [Is_Active])
                  WHERE [Is_Delete] = 0;");
        }
    }
}
