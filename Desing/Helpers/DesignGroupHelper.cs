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
    }
}
