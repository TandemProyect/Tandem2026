using DAL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Desing.Helpers
{
    /// <summary>Códigos de <c>TSql_DevelopIssue</c> (tipo, urgencia, estado); el texto visible sale de <c>Resources.DevelopIssue</c>.</summary>
    public static class DevelopIssueCodes
    {
        public static readonly string[] Types = { "US", "BUG" };
        public static readonly string[] Urgencies = { "LOW", "MEDIUM", "URGENT" };
        public static readonly string[] States = { "OPEN", "IN_PROGRESS", "CLOSED" };
        public static readonly string[] Sizes = { "SMALL", "MEDIUM", "LARGE" };
        public static readonly string[] Sources = { "AI", "AGENT" };

        public static string TypeText(string code) => Text("Type_", code);
        public static string UrgencyText(string code) => Text("Urgency_", code);
        public static string StateText(string code) => Text("State_", code);
        public static string SizeText(string code) => Text("Size_", code);
        public static string SourceText(string code) => Text("Source_", code);

        public static string SizeBadge(string code)
        {
            switch (code)
            {
                case "LARGE": return "bg-label-danger";
                case "MEDIUM": return "bg-label-warning";
                default: return "bg-label-success";
            }
        }

        public static string TypeBadge(string code) => code == "BUG" ? "bg-label-danger" : "bg-label-primary";

        public static string UrgencyBadge(string code)
        {
            switch (code)
            {
                case "URGENT": return "bg-label-danger";
                case "MEDIUM": return "bg-label-warning";
                default: return "bg-label-secondary";
            }
        }

        public static string StateBadge(string code)
        {
            switch (code)
            {
                case "CLOSED": return "bg-label-success";
                case "IN_PROGRESS": return "bg-label-warning";
                default: return "bg-label-info";
            }
        }

        private static string Text(string prefix, string code)
        {
            if (string.IsNullOrEmpty(code)) return "";
            return Resources.DevelopIssue.ResourceManager.GetString(prefix + code, Resources.DevelopIssue.Culture) ?? code;
        }
    }

    /// <summary>Fila de <c>TSql_DevelopIssue</c> (US o bug) y modelo de los formularios Create/Edit.</summary>
    public class DevelopIssueRow
    {
        public long IdObject { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.DevelopIssue), ErrorMessageResourceName = "Err_NameRequired")]
        [StringLength(500)]
        public string TextLabel { get; set; }

        public string TextType { get; set; }
        public string TextUrgency { get; set; }
        public string TextState { get; set; }
        public DateTime DateIssue { get; set; }
        public int NumberDetails { get; set; }

        public string LinkMadeBy { get; set; }
        public string LinModifiedBy { get; set; }
        public DateTime AddDateMade { get; set; }
        public DateTime? AddLastDateChange { get; set; }
        public long Ntimeschanged { get; set; }
    }

    /// <summary>Fila de <c>TSql_DevelopIssueDetail</c> (observación y/o captura de pantalla).</summary>
    public class DevelopIssueDetailRow
    {
        public long IdObject { get; set; }
        public long LinkDevelopIssue { get; set; }
        public int NumberOrder { get; set; }
        public string TextLabel { get; set; }
        public string TextObservation { get; set; }
        public string TextImagePath { get; set; }
        /// <summary>Adjunto que no es imagen (.tnwall); <c>TextLabel</c> guarda su nombre original.</summary>
        public string TextFilePath { get; set; }
        public string LinkMadeBy { get; set; }
        public string TextMadeBy { get; set; }
        public DateTime AddDateMade { get; set; }
    }

    /// <summary>Fila de <c>TSql_DevelopIssueAssessment</c> (valoración de la IA de la intranet o del agente).</summary>
    public class DevelopIssueAssessmentRow
    {
        public long IdObject { get; set; }
        public long LinkDevelopIssue { get; set; }
        public string TextSource { get; set; }
        public string TextSize { get; set; }
        public decimal? NumberHours { get; set; }
        public string TextArea { get; set; }
        public string TextAnalysis { get; set; }
        public string TextModel { get; set; }
        public DateTime AddDateMade { get; set; }
    }

    /// <summary>
    /// SQL de <c>TSql_DevelopIssue</c> y <c>TSql_DevelopIssueDetail</c> (aún no están en <c>Model.edmx</c>).
    /// Borrado siempre lógico; auditoría estándar de 9 columnas.
    /// </summary>
    public static class DevelopIssueRepository
    {
        public static bool TablesExist(ConexionData db)
        {
            return db.Database.SqlQuery<int>(@"
SELECT CASE WHEN OBJECT_ID(N'dbo.TSql_DevelopIssue', N'U') IS NULL
              OR OBJECT_ID(N'dbo.TSql_DevelopIssueDetail', N'U') IS NULL THEN 0 ELSE 1 END").First() == 1;
        }

        public static List<DevelopIssueRow> ListIssues(ConexionData db)
        {
            if (!TablesExist(db)) return new List<DevelopIssueRow>();

            return db.Database.SqlQuery<DevelopIssueRow>(@"
SELECT i.IdObject, i.TextLabel, i.TextType, i.TextUrgency, i.TextState, i.DateIssue,
       (SELECT COUNT(*) FROM dbo.TSql_DevelopIssueDetail d
         WHERE d.LinkDevelopIssue = i.IdObject AND d.Is_Delete = 0) AS NumberDetails,
       i.LinkMadeBy, i.LinModifiedBy, i.AddDateMade, i.AddLastDateChange, i.Ntimeschanged
  FROM dbo.TSql_DevelopIssue i
 WHERE i.Is_Delete = 0
 ORDER BY i.DateIssue DESC, i.IdObject DESC").ToList();
        }

        public static DevelopIssueRow GetIssue(ConexionData db, long id)
        {
            return db.Database.SqlQuery<DevelopIssueRow>(@"
SELECT i.IdObject, i.TextLabel, i.TextType, i.TextUrgency, i.TextState, i.DateIssue,
       (SELECT COUNT(*) FROM dbo.TSql_DevelopIssueDetail d
         WHERE d.LinkDevelopIssue = i.IdObject AND d.Is_Delete = 0) AS NumberDetails,
       i.LinkMadeBy, i.LinModifiedBy, i.AddDateMade, i.AddLastDateChange, i.Ntimeschanged
  FROM dbo.TSql_DevelopIssue i
 WHERE i.IdObject = @p0 AND i.Is_Delete = 0", id).FirstOrDefault();
        }

        public static long InsertIssue(ConexionData db, DevelopIssueRow m, string userId)
        {
            return db.Database.SqlQuery<long>(@"
INSERT INTO dbo.TSql_DevelopIssue
    (TextLabel, TextType, TextUrgency, TextState, DateIssue,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
OUTPUT INSERTED.IdObject
VALUES (@p0, @p1, @p2, @p3, @p4, 0, 1, @p5, NULL, GETDATE(), NULL, 0)",
                Cut(m.TextLabel, 500), m.TextType, m.TextUrgency, m.TextState, m.DateIssue,
                userId ?? "system").Single();
        }

        public static int UpdateIssue(ConexionData db, DevelopIssueRow m, string userId)
        {
            return db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_DevelopIssue
   SET TextLabel = @p1, TextType = @p2, TextUrgency = @p3, TextState = @p4, DateIssue = @p5,
       LinModifiedBy = @p6, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0 AND Is_Delete = 0",
                m.IdObject, Cut(m.TextLabel, 500), m.TextType, m.TextUrgency, m.TextState, m.DateIssue,
                userId ?? "system");
        }

        public static int SoftDeleteIssue(ConexionData db, long id, string userId)
        {
            return db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_DevelopIssue
   SET Is_Delete = 1, LinModifiedBy = @p1, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0 AND Is_Delete = 0", id, userId ?? "system");
        }

        public static List<DevelopIssueDetailRow> ListDetails(ConexionData db, long issueId)
        {
            return db.Database.SqlQuery<DevelopIssueDetailRow>(@"
SELECT d.IdObject, d.LinkDevelopIssue, d.NumberOrder, d.TextLabel, d.TextObservation, d.TextImagePath, d.TextFilePath,
       d.LinkMadeBy, COALESCE(u.UserName, d.LinkMadeBy) AS TextMadeBy, d.AddDateMade
  FROM dbo.TSql_DevelopIssueDetail d
  LEFT JOIN dbo.AspNetUsers u ON u.Id = d.LinkMadeBy
 WHERE d.LinkDevelopIssue = @p0 AND d.Is_Delete = 0
 ORDER BY d.NumberOrder, d.IdObject", issueId).ToList();
        }

        public static long InsertDetail(ConexionData db, long issueId, string label, string observation, string imagePath, string filePath, string userId)
        {
            return db.Database.SqlQuery<long>(@"
DECLARE @nextOrder INT = (
    SELECT ISNULL(MAX(NumberOrder), 0) + 1
      FROM dbo.TSql_DevelopIssueDetail
     WHERE LinkDevelopIssue = @p0 AND Is_Delete = 0
);
INSERT INTO dbo.TSql_DevelopIssueDetail
    (TextLabel, LinkDevelopIssue, NumberOrder, TextObservation, TextImagePath, TextFilePath,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
OUTPUT INSERTED.IdObject
VALUES (@p1, @p0, @nextOrder, @p2, @p3, @p5, 0, 1, @p4, NULL, GETDATE(), NULL, 0)",
                issueId,
                Cut(label ?? "", 500),
                (object)Null(observation) ?? DBNull.Value,
                (object)Null(Cut(imagePath, 1000)) ?? DBNull.Value,
                userId ?? "system",
                (object)Null(Cut(filePath, 1000)) ?? DBNull.Value).Single();
        }

        public static int UpdateDetailImage(ConexionData db, long issueId, long detailId, string imagePath, string userId)
        {
            return db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_DevelopIssueDetail
   SET TextImagePath = @p3, LinModifiedBy = @p2, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0 AND LinkDevelopIssue = @p1 AND Is_Delete = 0", detailId, issueId, userId ?? "system", imagePath);
        }

        public static int SoftDeleteDetail(ConexionData db, long issueId, long detailId, string userId)
        {
            return db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_DevelopIssueDetail
   SET Is_Delete = 1, LinModifiedBy = @p2, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0 AND LinkDevelopIssue = @p1 AND Is_Delete = 0", detailId, issueId, userId ?? "system");
        }

        public static bool AssessmentTableExists(ConexionData db)
        {
            return db.Database.SqlQuery<int>(
                "SELECT CASE WHEN OBJECT_ID(N'dbo.TSql_DevelopIssueAssessment', N'U') IS NULL THEN 0 ELSE 1 END").First() == 1;
        }

        /// <summary>Valoraciones de la US o bug, la más reciente primero.</summary>
        public static List<DevelopIssueAssessmentRow> ListAssessments(ConexionData db, long issueId)
        {
            if (!AssessmentTableExists(db)) return new List<DevelopIssueAssessmentRow>();

            return db.Database.SqlQuery<DevelopIssueAssessmentRow>(@"
SELECT a.IdObject, a.LinkDevelopIssue, a.TextSource, a.[TextSize], a.NumberHours, a.TextArea, a.TextAnalysis, a.TextModel, a.AddDateMade
  FROM dbo.TSql_DevelopIssueAssessment a
 WHERE a.LinkDevelopIssue = @p0 AND a.Is_Delete = 0
 ORDER BY a.IdObject DESC", issueId).ToList();
        }

        public static long InsertAssessment(ConexionData db, DevelopIssueAssessmentRow a, string label, string userId)
        {
            return db.Database.SqlQuery<long>(@"
INSERT INTO dbo.TSql_DevelopIssueAssessment
    (TextLabel, LinkDevelopIssue, TextSource, [TextSize], NumberHours, TextArea, TextAnalysis, TextModel,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
OUTPUT INSERTED.IdObject
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, 0, 1, @p8, NULL, GETDATE(), NULL, 0)",
                Cut(label ?? "", 500), a.LinkDevelopIssue, a.TextSource, a.TextSize,
                (object)a.NumberHours ?? DBNull.Value,
                (object)Null(Cut(a.TextArea, 500)) ?? DBNull.Value,
                (object)Null(a.TextAnalysis) ?? DBNull.Value,
                (object)Null(Cut(a.TextModel, 100)) ?? DBNull.Value,
                userId ?? "system").Single();
        }

        public static int UpdateState(ConexionData db, long id, string state, string userId)
        {
            return db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_DevelopIssue
   SET TextState = @p1, LinModifiedBy = @p2, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0 AND Is_Delete = 0", id, state, userId ?? "system");
        }

        private static string Null(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string Cut(string value, int max)
        {
            if (value == null) return null;
            value = value.Trim();
            return value.Length <= max ? value : value.Substring(0, max);
        }
    }
}
