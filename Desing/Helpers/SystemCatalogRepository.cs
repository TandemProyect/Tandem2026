using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DAL;
using Desing.Repositories.RepositoryCommun;

namespace Desing.Helpers
{
    public sealed class SystemCatalogRow
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextDescription { get; set; }
        public string TextPanelColor { get; set; }
        public bool AddIsActive { get; set; }
        public long LinkCompany { get; set; }
    }

    /// <summary>
    /// Sistemas de encofrado. El color de paneles se lee por SQL: la columna no está en el EDMX.
    /// </summary>
    public static class SystemCatalogRepository
    {
        public static IList<SystemCatalogRow> List(ConexionData db)
        {
            return db.Database.SqlQuery<SystemCatalogRow>(@"
SELECT IdObject, TextLabel, TextDescription, TextPanelColor, AddIsActive, LinkCompany
  FROM dbo.TSql_System
 ORDER BY TextLabel, IdObject").ToList();
        }

        public static SystemCatalogRow Get(ConexionData db, long id)
        {
            return db.Database.SqlQuery<SystemCatalogRow>(@"
SELECT IdObject, TextLabel, TextDescription, TextPanelColor, AddIsActive, LinkCompany
  FROM dbo.TSql_System
 WHERE IdObject = @id",
                new SqlParameter("@id", id)).FirstOrDefault();
        }

        /// <summary>Texto legible. El HTML de catálogo no se muestra como etiquetas.</summary>
        public static string ToPlainText(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var text = Regex.Replace(value, "<[^>]+>", " ");
            text = WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, "\\s+", " ").Trim();
            if (maxLength > 0 && text.Length > maxLength)
            {
                text = text.Substring(0, maxLength).Trim() + "...";
            }

            return text;
        }

        public static string Save(ConexionData db, long id, string label, string description, string color, bool isActive, string userId)
        {
            var name = (label ?? string.Empty).Trim();
            if (name.Length == 0 || name.Length > 500)
            {
                return "El nombre es obligatorio y no puede pasar de 500 caracteres.";
            }

            var hex = FormworkPanelColorRepository.NormalizeColor(color);
            if (hex == null)
            {
                return "El color de paneles debe ser un hex válido, por ejemplo #FF0000.";
            }

            var duplicate = db.Database.SqlQuery<int>(@"
SELECT COUNT(1) FROM dbo.TSql_System
 WHERE IdObject <> @id
   AND REPLACE(REPLACE(UPPER(LTRIM(RTRIM(TextLabel))), N'-', N''), N' ', N'') = @key",
                new SqlParameter("@id", id),
                new SqlParameter("@key", FormworkPanelColorRepository.NormalizeSystem(name))).FirstOrDefault();
            if (duplicate > 0)
            {
                return "Ya existe un sistema con ese nombre.";
            }

            var user = string.IsNullOrWhiteSpace(userId) ? "system" : userId;
            var desc = string.IsNullOrWhiteSpace(description) ? string.Empty : description.Trim();
            if (desc.Length > 4000)
            {
                return "La descripción no puede pasar de 4000 caracteres.";
            }

            if (id <= 0)
            {
                db.Database.ExecuteSqlCommand(@"
INSERT INTO dbo.TSql_System
    (TextLabel, LinkMadeBy, AddDateMade, LinkChangeBy, AddLastDateChange, Ntimeschanged, AddIsActive, TextDescription, LinkCompany, TextPanelColor)
SELECT @label, @user, GETDATE(), @user, GETDATE(), 0, @active, @desc,
       COALESCE((SELECT TOP (1) LinkCompany FROM dbo.TSql_System ORDER BY IdObject), 1),
       @color",
                    new SqlParameter("@label", name),
                    new SqlParameter("@user", user),
                    new SqlParameter("@active", isActive),
                    new SqlParameter("@desc", desc),
                    new SqlParameter("@color", hex));
                return null;
            }

            var updated = db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_System
   SET TextLabel = @label,
       TextDescription = @desc,
       TextPanelColor = @color,
       AddIsActive = @active,
       LinkChangeBy = @user,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @id",
                new SqlParameter("@label", name),
                new SqlParameter("@desc", desc),
                new SqlParameter("@color", hex),
                new SqlParameter("@active", isActive),
                new SqlParameter("@user", user),
                new SqlParameter("@id", id));
            return updated == 0 ? "El sistema ya no existe." : null;
        }

        /// <summary>Color guardado del sistema, o null si no hay hex válido.</summary>
        public static string ReadPanelColor(ConexionData db, string systemLabel)
        {
            if (db == null)
            {
                return null;
            }

            var key = FormworkPanelColorRepository.NormalizeSystem(string.IsNullOrWhiteSpace(systemLabel) ? "Atk60" : systemLabel);
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var color = db.Database.SqlQuery<string>(@"
SELECT TOP (1) TextPanelColor
  FROM dbo.TSql_System
 WHERE AddIsActive = 1
   AND REPLACE(REPLACE(UPPER(LTRIM(RTRIM(TextLabel))), N'-', N''), N' ', N'') = @key
 ORDER BY IdObject",
                new SqlParameter("@key", key)).FirstOrDefault();
            return FormworkPanelColorRepository.NormalizeColor(color);
        }

        /// <summary>Pinta los paneles verticales y devuelve el hex aplicado. Suplementos no cambian.</summary>
        public static string StampPanelColor(ConexionData db, string systemLabel, Atk60ThreeJsPaintPayload payload)
        {
            var hex = ReadPanelColor(db, systemLabel);
            if (hex == null || payload == null || payload.Elements == null || payload.Elements.Count == 0)
            {
                return hex;
            }

            for (var i = 0; i < payload.Elements.Count; i++)
            {
                var item = payload.Elements[i];
                if (item == null
                    || !string.Equals(item.ElementType, "Panel", System.StringComparison.OrdinalIgnoreCase)
                    || !string.Equals(item.Orientation, "Vertical", System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                item.SystemPanelColor = hex;
            }

            return hex;
        }
    }
}
