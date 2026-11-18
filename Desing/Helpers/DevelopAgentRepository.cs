using DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Desing.Helpers
{
    /// <summary>Fila de <c>TSql_DevelodAgentDocument</c> (documentos y comandos para agentes).</summary>
    public class DevelopAgentDocumentRow
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextCode { get; set; }
        public string TextCommand { get; set; }
        public string TextPath { get; set; }
        public string TextDescription { get; set; }
        public int NumberOrder { get; set; }
    }

    /// <summary>
    /// Lectura SQL de <c>TSql_DevelodAgentDocument</c> (aún no está en <c>Model.edmx</c>).
    /// Solo lectura: los datos se mantienen con la semilla <c>2026-11-17_seed_TSql_DevelodAgentDocument.sql</c>.
    /// </summary>
    public static class DevelopAgentRepository
    {
        public static bool TableExists(ConexionData db)
        {
            return db.Database.SqlQuery<int>(
                "SELECT CASE WHEN OBJECT_ID(N'dbo.TSql_DevelodAgentDocument', N'U') IS NULL THEN 0 ELSE 1 END").First() == 1;
        }

        public static List<DevelopAgentDocumentRow> ListDocuments(ConexionData db)
        {
            if (!TableExists(db))
            {
                return new List<DevelopAgentDocumentRow>();
            }

            return db.Database.SqlQuery<DevelopAgentDocumentRow>(@"
SELECT d.IdObject, d.TextLabel, d.TextCode, d.TextCommand, d.TextPath, d.TextDescription, d.NumberOrder
  FROM dbo.TSql_DevelodAgentDocument d
 WHERE d.Is_Delete = 0 AND d.Is_Active = 1
 ORDER BY d.NumberOrder, d.TextLabel").ToList();
        }
    }
}
