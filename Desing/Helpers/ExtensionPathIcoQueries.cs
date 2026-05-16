using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;

namespace Desing.Helpers
{
    /// <summary>
    /// Lectura / escritura de dbo.TSql_Extension.Path_Ico vía SQL mientras EF EDMX
    /// pueda estar sin esa propiedad hasta "Update Model from Database".
    /// </summary>
    public static class ExtensionPathIcoQueries
    {
        private class ExtensionIdPathRow
        {
            public long IdObject { get; set; }

            public string Path_Ico { get; set; }
        }

        public static string GetPathIco(Database database, long idObject)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            var value = database
                .SqlQuery<string>(
                    "SELECT Path_Ico FROM dbo.TSql_Extension WITH (NOLOCK) WHERE IdObject = {0} AND Is_Delete = 0",
                    idObject)
                .FirstOrDefault();
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        /// <summary>IDs como claves; solo entradas con ruta no vacía.</summary>
        public static Dictionary<long, string> LoadPathIcoMap(Database database, IEnumerable<long> idObjects)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            var idList = idObjects == null ? new List<long>() : idObjects.Where(id => id > 0).Distinct().ToList();
            if (idList.Count == 0)
            {
                return new Dictionary<long, string>();
            }

            var csv = string.Join(",", idList.ConvertAll(id => id.ToString(CultureInfo.InvariantCulture)));
            var sql =
                @"SELECT IdObject, Path_Ico
                  FROM dbo.TSql_Extension WITH (NOLOCK)
                  WHERE Is_Delete = 0
                    AND Path_Ico IS NOT NULL
                    AND LEN(LTRIM(RTRIM(Path_Ico))) > 0
                    AND IdObject IN (" + csv + ")";

            var rows = database.SqlQuery<ExtensionIdPathRow>(sql).ToList();

            var map = new Dictionary<long, string>();
            foreach (var r in rows)
            {
                if (!string.IsNullOrWhiteSpace(r.Path_Ico))
                {
                    map[r.IdObject] = r.Path_Ico.Trim();
                }
            }

            return map;
        }

        public static void SetPathIco(Database database, long idObject, string pathOrNull)
        {
            if (database == null)
            {
                throw new ArgumentNullException(nameof(database));
            }

            if (string.IsNullOrWhiteSpace(pathOrNull))
            {
                database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_Extension SET Path_Ico = NULL WHERE IdObject = {0} AND Is_Delete = 0",
                    idObject);
            }
            else
            {
                database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_Extension SET Path_Ico = {0} WHERE IdObject = {1} AND Is_Delete = 0",
                    pathOrNull.Trim(),
                    idObject);
            }
        }
    }
}
