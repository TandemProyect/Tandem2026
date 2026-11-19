using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DAL;
using Desing.Repositories.RepositoryCommun;

namespace Desing.Helpers
{
    public sealed class FormworkPanelColorRow
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public long LinkSystem { get; set; }
        public string SystemLabel { get; set; }
        public int NumberWidthMm { get; set; }
        public string TextColor { get; set; }
        public bool Is_Active { get; set; }
    }

    public sealed class FormworkSystemOption
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
    }

    public sealed class ArticleColorRow
    {
        public string Code { get; set; }
        public string Color { get; set; }
    }

    public sealed class FormworkPanelColorViewerRow
    {
        public int WidthMm { get; set; }
        public string Color { get; set; }
    }

    /// <summary>
    /// Catálogo de color por sistema y ancho. SQL directo: la tabla aún no está en el EDMX.
    /// </summary>
    public static class FormworkPanelColorRepository
    {
        private static readonly Regex ColorHex = new Regex("^#[0-9A-Fa-f]{6}$", RegexOptions.Compiled);

        public static IList<FormworkPanelColorRow> List(ConexionData db)
        {
            return db.Database.SqlQuery<FormworkPanelColorRow>(@"
SELECT c.IdObject, c.TextLabel, c.LinkSystem, s.TextLabel AS SystemLabel,
       c.NumberWidthMm, c.TextColor, c.Is_Active
  FROM dbo.TSql_FormworkPanelColor c
  JOIN dbo.TSql_System s ON s.IdObject = c.LinkSystem
 WHERE c.Is_Delete = 0
 ORDER BY s.TextLabel, c.NumberWidthMm").ToList();
        }

        public static FormworkPanelColorRow Get(ConexionData db, long id)
        {
            return db.Database.SqlQuery<FormworkPanelColorRow>(@"
SELECT c.IdObject, c.TextLabel, c.LinkSystem, s.TextLabel AS SystemLabel,
       c.NumberWidthMm, c.TextColor, c.Is_Active
  FROM dbo.TSql_FormworkPanelColor c
  JOIN dbo.TSql_System s ON s.IdObject = c.LinkSystem
 WHERE c.Is_Delete = 0 AND c.IdObject = @id",
                new SqlParameter("@id", id)).FirstOrDefault();
        }

        public static IList<FormworkSystemOption> ListSystems(ConexionData db)
        {
            return db.Database.SqlQuery<FormworkSystemOption>(@"
SELECT IdObject, TextLabel
  FROM dbo.TSql_System
 WHERE AddIsActive = 1
 ORDER BY TextLabel").ToList();
        }

        public static IList<FormworkPanelColorViewerRow> ListForViewer(ConexionData db, string systemLabel)
        {
            var key = NormalizeSystem(systemLabel);
            if (string.IsNullOrEmpty(key))
            {
                return new List<FormworkPanelColorViewerRow>();
            }

            return db.Database.SqlQuery<FormworkPanelColorViewerRow>(@"
SELECT c.NumberWidthMm AS WidthMm, c.TextColor AS Color
  FROM dbo.TSql_FormworkPanelColor c
  JOIN dbo.TSql_System s ON s.IdObject = c.LinkSystem
 WHERE c.Is_Delete = 0 AND c.Is_Active = 1 AND s.AddIsActive = 1
   AND REPLACE(REPLACE(UPPER(LTRIM(RTRIM(s.TextLabel))), N'-', N''), N' ', N'') = @key
 ORDER BY c.NumberWidthMm",
                new SqlParameter("@key", key)).ToList();
        }

        public static string Save(
            ConexionData db,
            long id,
            long linkSystem,
            string newSystemLabel,
            string widthText,
            string color,
            bool isActive,
            string userId)
        {
            int widthMm;
            string widthError;
            if (!TryParseWidthMm(widthText, out widthMm, out widthError))
            {
                return widthError;
            }

            var hex = NormalizeColor(color);
            if (hex == null)
            {
                return "El color debe ser #RRGGBB.";
            }

            if (!string.IsNullOrWhiteSpace(newSystemLabel))
            {
                linkSystem = EnsureSystem(db, newSystemLabel.Trim(), userId);
            }

            if (linkSystem <= 0)
            {
                return "Elige un sistema o escribe uno nuevo.";
            }

            var system = ListSystems(db).FirstOrDefault(s => s.IdObject == linkSystem);
            if (system == null)
            {
                return "El sistema no existe o está inactivo.";
            }

            var label = system.TextLabel + " · " + (widthMm / 1000m).ToString("0.00", CultureInfo.GetCultureInfo("es-ES")) + " m";
            if (label.Length > 500)
            {
                label = label.Substring(0, 500);
            }

            var duplicate = db.Database.SqlQuery<int>(@"
SELECT COUNT(1) FROM dbo.TSql_FormworkPanelColor
 WHERE Is_Delete = 0 AND LinkSystem = @system AND NumberWidthMm = @width AND IdObject <> @id",
                new SqlParameter("@system", linkSystem),
                new SqlParameter("@width", widthMm),
                new SqlParameter("@id", id)).FirstOrDefault();
            if (duplicate > 0)
            {
                return "Ese sistema ya tiene un color para ese ancho.";
            }

            if (id <= 0)
            {
                db.Database.ExecuteSqlCommand(@"
INSERT INTO dbo.TSql_FormworkPanelColor
    (TextLabel, LinkSystem, NumberWidthMm, TextColor,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
VALUES
    (@label, @system, @width, @color, 0, @active, @user, NULL, GETDATE(), NULL, 0)",
                    new SqlParameter("@label", label),
                    new SqlParameter("@system", linkSystem),
                    new SqlParameter("@width", widthMm),
                    new SqlParameter("@color", hex),
                    new SqlParameter("@active", isActive),
                    new SqlParameter("@user", userId ?? "system"));
            }
            else
            {
                var updated = db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_FormworkPanelColor
   SET TextLabel = @label,
       LinkSystem = @system,
       NumberWidthMm = @width,
       TextColor = @color,
       Is_Active = @active,
       LinModifiedBy = @user,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @id AND Is_Delete = 0",
                    new SqlParameter("@label", label),
                    new SqlParameter("@system", linkSystem),
                    new SqlParameter("@width", widthMm),
                    new SqlParameter("@color", hex),
                    new SqlParameter("@active", isActive),
                    new SqlParameter("@user", userId ?? "system"),
                    new SqlParameter("@id", id));
                if (updated == 0)
                {
                    return "El color ya no existe.";
                }
            }

            return null;
        }

        public static void SoftDelete(ConexionData db, long id, string userId)
        {
            db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_FormworkPanelColor
   SET Is_Delete = 1,
       LinModifiedBy = @user,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @id AND Is_Delete = 0",
                new SqlParameter("@user", userId ?? "system"),
                new SqlParameter("@id", id));
        }

        public static bool TryParseWidthMm(string text, out int widthMm, out string error)
        {
            widthMm = 0;
            error = null;
            var raw = (text ?? string.Empty).Trim().Replace(',', '.');
            decimal meters;
            if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out meters)
                || meters <= 0m
                || meters > 20m)
            {
                error = "El ancho debe estar en metros, entre 0,01 y 20 (por ejemplo 0,90).";
                return false;
            }

            widthMm = (int)Math.Round(meters * 1000m, MidpointRounding.AwayFromZero);
            if (widthMm <= 0)
            {
                error = "El ancho debe estar en metros, entre 0,01 y 20 (por ejemplo 0,90).";
                return false;
            }

            return true;
        }

        public static string NormalizeColor(string color)
        {
            var text = (color ?? string.Empty).Trim();
            if (text.Length == 6 && ColorHex.IsMatch("#" + text))
            {
                text = "#" + text;
            }

            return ColorHex.IsMatch(text) ? text.ToUpperInvariant() : null;
        }

        public static void StampArticleColors(ConexionData db, string systemLabel, Atk60ThreeJsPaintPayload payload)
        {
            if (db == null || payload == null || payload.Elements == null || payload.Elements.Count == 0)
            {
                return;
            }

            var key = NormalizeSystem(string.IsNullOrWhiteSpace(systemLabel) ? "Atk60" : systemLabel);
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            var rows = db.Database.SqlQuery<ArticleColorRow>(@"
SELECT a.TextCode AS Code, a.TextColor1 AS Color
  FROM dbo.Tsql_Master_Articles a
  JOIN dbo.TSql_System s ON s.IdObject = a.LinkSystem
 WHERE a.AddIsActive = 1
   AND REPLACE(REPLACE(UPPER(LTRIM(RTRIM(s.TextLabel))), N'-', N''), N' ', N'') = @key",
                new SqlParameter("@key", key)).ToList();

            var byCode = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < rows.Count; i++)
            {
                var code = (rows[i].Code ?? string.Empty).Trim();
                if (code.Length == 0 || byCode.ContainsKey(code))
                {
                    continue;
                }

                byCode[code] = rows[i].Color;
            }

            for (var i = 0; i < payload.Elements.Count; i++)
            {
                var item = payload.Elements[i];
                if (item == null || string.IsNullOrWhiteSpace(item.ElementCode))
                {
                    continue;
                }

                string color;
                var elementCode = item.ElementCode.Trim();
                if (!byCode.TryGetValue(elementCode, out color))
                {
                    byCode.TryGetValue(ArticleCodeKey(elementCode), out color);
                }

                if (!string.IsNullOrWhiteSpace(color))
                {
                    item.ArticleColor = color;
                }
            }
        }

        /// <summary>PANEL_27904209 y 27904209_FRAME cruzan con el TextCode del maestro.</summary>
        public static string ArticleCodeKey(string elementCode)
        {
            var code = (elementCode ?? string.Empty).Trim();
            if (code.StartsWith("PANEL_", StringComparison.OrdinalIgnoreCase))
            {
                code = code.Substring("PANEL_".Length);
            }

            var cut = code.IndexOf('_');
            if (cut > 0)
            {
                code = code.Substring(0, cut);
            }

            return code.Trim();
        }

        public static string NormalizeSystem(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return string.Empty;
            }

            return label.Trim().Replace("-", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
        }

        private static long EnsureSystem(ConexionData db, string label, string userId)
        {
            var existing = db.Database.SqlQuery<long>(@"
SELECT TOP (1) IdObject FROM dbo.TSql_System
 WHERE AddIsActive = 1 AND TextLabel = @label
 ORDER BY IdObject",
                new SqlParameter("@label", label)).FirstOrDefault();
            if (existing > 0)
            {
                return existing;
            }

            db.Database.ExecuteSqlCommand(@"
INSERT INTO dbo.TSql_System
    (TextLabel, LinkMadeBy, AddDateMade, LinkChangeBy, AddLastDateChange, Ntimeschanged, AddIsActive, TextDescription, LinkCompany)
SELECT @label, @user, GETDATE(), @user, GETDATE(), 0, 1, N'',
                   COALESCE((SELECT TOP (1) LinkCompany FROM dbo.TSql_System ORDER BY IdObject), 1)
             WHERE NOT EXISTS (SELECT 1 FROM dbo.TSql_System WHERE TextLabel = @label)",
                new SqlParameter("@label", label),
                            new SqlParameter("@user", userId ?? "system"));
                        return db.Database.SqlQuery<long>(@"
            SELECT TOP (1) IdObject FROM dbo.TSql_System WHERE TextLabel = @label ORDER BY IdObject DESC",
                            new SqlParameter("@label", label)).FirstOrDefault();
        }
    }
}
