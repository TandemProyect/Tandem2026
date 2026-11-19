using DAL;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Desing.Helpers
{
    public class ClientOfferLookup
    {
        public long Id { get; set; }
        public string TextLabel { get; set; }
        public string TextExtra { get; set; }
    }

    public class ClientOfferBrandRow
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextCode { get; set; }
        public long LinkCompany { get; set; }
        public long? LinkSystem { get; set; }
        public long? LinkPlantilla { get; set; }
        public string TextLogo { get; set; }
        public string TextCompanyName { get; set; }
        public string TextSubjectTemplate { get; set; }
        public string TextHeaderTemplate { get; set; }
        public string TextBodyTemplate { get; set; }
        public string TextLayoutJson { get; set; }
        public string KeepLogoIdsCsv { get; set; }
        [AllowHtml]
        public string TextBlocksJson { get; set; }
        public bool Is_Active { get; set; }
        public string CompanyLabel { get; set; }
        public string SystemLabel { get; set; }
        public List<ClientOfferBrandLogoRow> Logos { get; set; }
        public List<ClientOfferBrandTextRow> TextBlocks { get; set; }
    }

    public class ClientOfferBrandLogoRow
    {
        public long IdObject { get; set; }
        public long LinkClientOfferBrand { get; set; }
        public int NumberOrder { get; set; }
        public string TextImagePath { get; set; }
        public bool Is_Active { get; set; }
    }

    public class ClientOfferBrandTextRow
    {
        public long IdObject { get; set; }
        public long LinkClientOfferBrand { get; set; }
        public int NumberOrder { get; set; }
        public string TextLabel { get; set; }
        public string TextBody { get; set; }
        public bool Is_Active { get; set; }
    }

    public class ClientOfferReusableTextRow
    {
        public long IdObject { get; set; }
        public long LinkClientOfferBrand { get; set; }
        public string BrandLabel { get; set; }
        public string TextLabel { get; set; }
        public string TextBody { get; set; }
    }

    public class ClientOfferRow
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public long? LinkDesign { get; set; }
        public long? LinkOffers { get; set; }
        public long? LinkJobside { get; set; }
        public long? LinkClient_V2 { get; set; }
        public long LinkCompany { get; set; }
        public long? LinkPlantilla { get; set; }
        public long LinkSystem { get; set; }
        public long? LinkClientOfferBrand { get; set; }
        public long? LinkClientOfferContact { get; set; }
        public string TextSubject { get; set; }
        public string TextClientAddress { get; set; }
        public string TextContactName { get; set; }
        public string TextContactEmail { get; set; }
        public string TextMaterial { get; set; }
        public string TextLogo { get; set; }
        public string TextRecuento { get; set; }
        public string TextStatus { get; set; }
        public string TextPdfPath { get; set; }
        public DateTime? AddPdfDate { get; set; }
        public bool Is_Active { get; set; }
        public string CompanyLabel { get; set; }
        public string SystemLabel { get; set; }
        public string ClientLabel { get; set; }
        public string HeaderBody { get; set; }
        public string TextBody { get; set; }
        public List<ClientOfferCaptureRow> Captures { get; set; }
        public List<string> TemplateLogos { get; set; }
    }

    public class ClientOfferAiMessageRow
    {
        public long IdObject { get; set; }
        public long LinkClientOffer { get; set; }
        public string TextRole { get; set; }
        public string TextMessage { get; set; }
        public string AddPayloadJson { get; set; }
        public DateTime AddDateMade { get; set; }
    }

    public class ClientOfferContactRow
    {
        public long IdObject { get; set; }
        public long LinkCompany { get; set; }
        public long? LinkClient_V2 { get; set; }
        public string TextLabel { get; set; }
        public string TextEmail { get; set; }
        public string TextPhone { get; set; }
        public bool Is_Default { get; set; }
        public bool Is_Active { get; set; }
        public string StateCode { get; set; }
        public DateTime? AddStatusDate { get; set; }
    }

    public class ClientOfferContactStateTypeRow
    {
        public long IdObject { get; set; }
        public string TextCode { get; set; }
        public string TextLabel { get; set; }
        public bool Is_Active { get; set; }
    }

    public class ClientOfferCaptureRow
    {
        public long IdObject { get; set; }
        public long LinkClientOffer { get; set; }
        public int NumberOrder { get; set; }
        public string TextImagePath { get; set; }
        public string TextLabel { get; set; }
        public DateTime AddDateMade { get; set; }
    }

    public class ClientOfferContext
    {
        public string Empresa { get; set; }
        public string DireccionEmpresa { get; set; }
        public string Cliente { get; set; }
        public string ClienteCifNif { get; set; }
        public string DireccionCliente { get; set; }
        public string Diseno { get; set; }
        public string Oferta { get; set; }
        public string Obra { get; set; }
        public string DireccionObra { get; set; }
        public string Plantilla { get; set; }
        public string Sistema { get; set; }
        public string Logo { get; set; }
        public long? LinkOffers { get; set; }
        public long? LinkJobside { get; set; }
        public long? LinkClient_V2 { get; set; }
        public long? LinkPlantilla { get; set; }
    }

    /// <summary>
    /// SQL de oferta al cliente. Las tablas aún no están en Model.edmx.
    /// </summary>
    public static class ClientOfferRepository
    {
        public static bool TablesExist(ConexionData db)
        {
            return db.Database.SqlQuery<int>(@"
SELECT CASE WHEN OBJECT_ID(N'dbo.TSql_ClientOffer', N'U') IS NOT NULL
             AND OBJECT_ID(N'dbo.TSql_ClientOfferBrand', N'U') IS NOT NULL
             AND OBJECT_ID(N'dbo.TSql_ClientOfferBrandLogo', N'U') IS NOT NULL
             AND OBJECT_ID(N'dbo.TSql_ClientOfferBrandText', N'U') IS NOT NULL
             AND OBJECT_ID(N'dbo.TSql_ClientOfferSection', N'U') IS NOT NULL
             AND OBJECT_ID(N'dbo.TSql_ClientOfferAiMessage', N'U') IS NOT NULL
             AND OBJECT_ID(N'dbo.TSql_ClientOfferContact', N'U') IS NOT NULL
             AND OBJECT_ID(N'dbo.TSql_ClientOfferCapture', N'U') IS NOT NULL
             AND OBJECT_ID(N'dbo.TSql_ClientOfferContactStateType', N'U') IS NOT NULL
             AND OBJECT_ID(N'dbo.TSql_ClientOfferContactState', N'U') IS NOT NULL
            THEN 1 ELSE 0 END").First() == 1;
        }

        public static List<ClientOfferLookup> ListCompanies(ConexionData db)
        {
            return db.Database.SqlQuery<ClientOfferLookup>(@"
SELECT c.SysObjectID AS Id, c.TextLabel
  FROM dbo.TSql_Company c
 WHERE c.BitIsDeleted = 0
 ORDER BY c.TextLabel").ToList();
        }

        public static List<ClientOfferLookup> ListSystems(ConexionData db)
        {
            return db.Database.SqlQuery<ClientOfferLookup>(@"
SELECT s.IdObject AS Id, s.TextLabel
  FROM dbo.TSql_System s
 WHERE s.AddIsActive = 1
 ORDER BY s.TextLabel").ToList();
        }

        public static long? FindSystemIdByLabel(ConexionData db, string label)
        {
            var value = (label ?? string.Empty).Trim();
            if (value.Length == 0)
            {
                return null;
            }

            return db.Database.SqlQuery<long?>(@"
SELECT TOP (1) s.IdObject
  FROM dbo.TSql_System s
 WHERE s.AddIsActive = 1 AND LOWER(LTRIM(RTRIM(s.TextLabel))) = LOWER(@p0)",
                value).FirstOrDefault();
        }

        public static List<ClientOfferLookup> ListPlantillas(ConexionData db)
        {
            return db.Database.SqlQuery<ClientOfferLookup>(@"
SELECT p.SysObjectID AS Id, p.AttName AS TextLabel
  FROM dbo.TSql_Plantilla p
 WHERE p.AttIsDeleted = 0
 ORDER BY p.AttName").ToList();
        }

        public static List<ClientOfferLookup> ListDesigns(ConexionData db)
        {
            return db.Database.SqlQuery<ClientOfferLookup>(@"
SELECT TOP (200) d.SysObjectID AS Id, d.AttLabel AS TextLabel
  FROM dbo.TSql_Design_V2 d
 WHERE d.AttIsDeleted = 0
 ORDER BY d.AttCreated DESC").ToList();
        }

        public static List<ClientOfferLookup> ListOffers(ConexionData db)
        {
            return db.Database.SqlQuery<ClientOfferLookup>(@"
SELECT o.IdObject AS Id, o.TextLabel
  FROM dbo.TSql_Offers o
 WHERE o.Is_Delete = 0
 ORDER BY o.TextLabel").ToList();
        }

        public static List<ClientOfferLookup> ListJobsides(ConexionData db)
        {
            return db.Database.SqlQuery<ClientOfferLookup>(@"
SELECT j.IdObject AS Id, j.TextLabel
  FROM dbo.TSql_Jobside j
 WHERE j.Is_Delete = 0
 ORDER BY j.TextLabel").ToList();
        }

        public static List<ClientOfferLookup> ListClients(ConexionData db)
        {
            return db.Database.SqlQuery<ClientOfferLookup>(@"
SELECT c.IdObject AS Id, c.TextLabel
  FROM dbo.TSql_Client_V2 c
 WHERE c.Is_Delete = 0
 ORDER BY c.TextLabel").ToList();
        }

        public static List<ClientOfferLookup> ListOfferContacts(ConexionData db, long companyId, long? clientId = null)
        {
            if (companyId <= 0 || !clientId.HasValue || clientId.Value <= 0)
            {
                return new List<ClientOfferLookup>();
            }

            return db.Database.SqlQuery<ClientOfferLookup>(@"
SELECT c.IdObject AS Id,
       c.TextLabel AS TextLabel,
       c.TextEmail AS TextExtra
  FROM dbo.TSql_ClientOfferContact c
 OUTER APPLY (
       SELECT TOP (1) stt.TextCode
         FROM dbo.TSql_ClientOfferContactState st
         JOIN dbo.TSql_ClientOfferContactStateType stt ON stt.IdObject = st.LinkClientOfferContactStateType
        WHERE st.LinkClientOfferContact = c.IdObject
          AND st.Is_Delete = 0
        ORDER BY st.AddStatusDate DESC, st.IdObject DESC
 ) cs
 WHERE c.Is_Delete = 0
   AND c.Is_Active = 1
   AND c.LinkCompany = @p0
   AND c.LinkClient_V2 = @p1
   AND ISNULL(cs.TextCode, N'ALTA') NOT IN (N'BAJA', N'ENFERMO')
 ORDER BY CASE WHEN c.Is_Default = 1 THEN 0 ELSE 1 END, c.TextLabel", companyId, clientId.Value).ToList();
        }

        public static List<ClientOfferContactStateTypeRow> ListContactStateTypes(ConexionData db, bool onlyActive = false)
        {
            var sql = @"
SELECT IdObject, TextCode, TextLabel, Is_Active
  FROM dbo.TSql_ClientOfferContactStateType
 WHERE Is_Delete = 0";
            if (onlyActive)
            {
                sql += " AND Is_Active = 1";
            }

            sql += " ORDER BY TextLabel, IdObject";
            return db.Database.SqlQuery<ClientOfferContactStateTypeRow>(sql).ToList();
        }

        public static ClientOfferContactStateTypeRow GetContactStateTypeByCode(ConexionData db, string code, bool onlyActive = false)
        {
            var normalized = (code ?? string.Empty).Trim().ToUpperInvariant();
            if (normalized.Length == 0)
            {
                return null;
            }

            var sql = @"
SELECT TOP (1) IdObject, TextCode, TextLabel, Is_Active
  FROM dbo.TSql_ClientOfferContactStateType
 WHERE Is_Delete = 0
   AND UPPER(TextCode) = @p0";
            if (onlyActive)
            {
                sql += " AND Is_Active = 1";
            }

            sql += " ORDER BY IdObject DESC";
            return db.Database.SqlQuery<ClientOfferContactStateTypeRow>(sql, normalized).FirstOrDefault();
        }

        public static long InsertContactStateType(ConexionData db, string code, string label, bool isActive, string userId)
        {
            return db.Database.SqlQuery<long>(@"
INSERT INTO dbo.TSql_ClientOfferContactStateType
    (TextCode, TextLabel, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
OUTPUT INSERTED.IdObject
VALUES (@p0, @p1, 0, @p2, @p3, NULL, GETDATE(), NULL, 0)",
                Cut((code ?? string.Empty).Trim().ToUpperInvariant(), 50),
                Cut((label ?? string.Empty).Trim(), 100),
                isActive,
                userId ?? "system").Single();
        }

        public static void SoftDeleteContactStateType(ConexionData db, long id, string userId)
        {
            db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOfferContactStateType
   SET Is_Delete = 1,
       LinModifiedBy = @p1,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0
   AND Is_Delete = 0", id, userId ?? "system");
        }

        public static ClientOfferContactRow GetOfferContact(ConexionData db, long id)
        {
            return db.Database.SqlQuery<ClientOfferContactRow>(@"
SELECT c.IdObject, c.LinkCompany, c.LinkClient_V2, c.TextLabel, c.TextEmail, c.TextPhone, c.Is_Default, c.Is_Active,
       ISNULL(cs.TextCode, N'ALTA') AS StateCode, cs.AddStatusDate
  FROM dbo.TSql_ClientOfferContact c
 OUTER APPLY (
       SELECT TOP (1) stt.TextCode, st.AddStatusDate
         FROM dbo.TSql_ClientOfferContactState st
         JOIN dbo.TSql_ClientOfferContactStateType stt ON stt.IdObject = st.LinkClientOfferContactStateType
        WHERE st.LinkClientOfferContact = c.IdObject
          AND st.Is_Delete = 0
        ORDER BY st.AddStatusDate DESC, st.IdObject DESC
 ) cs
 WHERE c.IdObject = @p0 AND c.Is_Delete = 0", id).FirstOrDefault();
        }

        public static ClientOfferContactRow GetDefaultOfferContact(ConexionData db, long companyId, long clientId)
        {
            return db.Database.SqlQuery<ClientOfferContactRow>(@"
SELECT TOP (1) c.IdObject, c.LinkCompany, c.LinkClient_V2, c.TextLabel, c.TextEmail, c.TextPhone, c.Is_Default, c.Is_Active,
       ISNULL(cs.TextCode, N'ALTA') AS StateCode, cs.AddStatusDate
  FROM dbo.TSql_ClientOfferContact c
 OUTER APPLY (
       SELECT TOP (1) stt.TextCode, st.AddStatusDate
         FROM dbo.TSql_ClientOfferContactState st
         JOIN dbo.TSql_ClientOfferContactStateType stt ON stt.IdObject = st.LinkClientOfferContactStateType
        WHERE st.LinkClientOfferContact = c.IdObject
          AND st.Is_Delete = 0
        ORDER BY st.AddStatusDate DESC, st.IdObject DESC
 ) cs
 WHERE c.Is_Delete = 0
   AND c.Is_Active = 1
   AND c.LinkCompany = @p0
   AND c.LinkClient_V2 = @p1
   AND ISNULL(cs.TextCode, N'ALTA') NOT IN (N'BAJA', N'ENFERMO')
 ORDER BY CASE WHEN c.Is_Default = 1 THEN 0 ELSE 1 END, c.IdObject DESC", companyId, clientId).FirstOrDefault();
        }

        public static long? UserCompanyId(ConexionData db, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            return db.Database.SqlQuery<long?>(@"
SELECT TOP (1) e.LinCompany
  FROM dbo.TSql_Employee e
 WHERE e.LinAspNetUsert = @p0 AND e.AttIsDeleted = 0 AND e.LinCompany IS NOT NULL", userId).FirstOrDefault();
        }

        public static ClientOfferContext LoadContext(ConexionData db, ClientOfferRow row)
        {
            var ctx = new ClientOfferContext();
            var company = db.Database.SqlQuery<CompanySnap>(@"
SELECT c.TextLabel, c.TextLogo, c.Loc_Formatted_Address AS Address,
       CAST(c.TextAddress_1 AS NVARCHAR(500)) AS TextAddress_1,
       CAST(c.TextAddress_2 AS NVARCHAR(500)) AS TextAddress_2,
       CAST(c.TextPostal_Code AS NVARCHAR(100)) AS TextPostal_Code,
       CAST(c.TextTown_1 AS NVARCHAR(500)) AS TextTown_1,
       CAST(c.TextTown_2 AS NVARCHAR(500)) AS TextTown_2,
       c.LinPlantilla, p.AttName AS PlantillaName, p.AttLogo AS PlantillaLogo
  FROM dbo.TSql_Company c
  LEFT JOIN dbo.TSql_Plantilla p ON p.SysObjectID = c.LinPlantilla AND p.AttIsDeleted = 0
 WHERE c.SysObjectID = @p0 AND c.BitIsDeleted = 0", row.LinkCompany).FirstOrDefault();
            if (company != null)
            {
                ctx.Empresa = company.TextLabel;
                ctx.DireccionEmpresa = BuildAddress(
                    company.Address,
                    company.TextAddress_1,
                    company.TextAddress_2,
                    company.TextPostal_Code,
                    company.TextTown_1,
                    company.TextTown_2);
                ctx.Logo = company.TextLogo;
                if (!row.LinkPlantilla.HasValue || row.LinkPlantilla.Value <= 0)
                {
                    row.LinkPlantilla = company.LinPlantilla;
                }
            }

            if (row.LinkSystem > 0)
            {
                ctx.Sistema = db.Database.SqlQuery<string>(
                    "SELECT TextLabel FROM dbo.TSql_System WHERE IdObject = @p0", row.LinkSystem).FirstOrDefault();
            }

            if (row.LinkDesign.HasValue && row.LinkDesign.Value > 0)
            {
                var design = db.Database.SqlQuery<DesignSnap>(@"
SELECT d.AttLabel, CASE WHEN d.LinkOffers > 0 THEN d.LinkOffers ELSE NULL END AS LinkOffers
  FROM dbo.TSql_Design_V2 d
 WHERE d.SysObjectID = @p0 AND d.AttIsDeleted = 0", row.LinkDesign.Value).FirstOrDefault();
                if (design != null)
                {
                    ctx.Diseno = design.AttLabel;
                    if ((!row.LinkOffers.HasValue || row.LinkOffers.Value <= 0) && design.LinkOffers.HasValue)
                    {
                        row.LinkOffers = design.LinkOffers;
                    }
                }
            }

            if (row.LinkOffers.HasValue && row.LinkOffers.Value > 0)
            {
                var offer = db.Database.SqlQuery<OfferSnap>(@"
SELECT o.TextLabel, o.AddOfferNumber, o.LinkJobside, o.LinkClient_V2
  FROM dbo.TSql_Offers o
 WHERE o.IdObject = @p0 AND o.Is_Delete = 0", row.LinkOffers.Value).FirstOrDefault();
                if (offer != null)
                {
                    ctx.Oferta = string.IsNullOrWhiteSpace(offer.AddOfferNumber)
                        ? offer.TextLabel
                        : offer.AddOfferNumber + " " + offer.TextLabel;
                    if (!row.LinkJobside.HasValue || row.LinkJobside.Value <= 0)
                    {
                        row.LinkJobside = offer.LinkJobside;
                    }
                    if (!row.LinkClient_V2.HasValue || row.LinkClient_V2.Value <= 0)
                    {
                        row.LinkClient_V2 = offer.LinkClient_V2;
                    }
                }
            }

            if (row.LinkJobside.HasValue && row.LinkJobside.Value > 0)
            {
                var jobside = db.Database.SqlQuery<JobsideSnap>(@"
SELECT TextLabel, Loc_Formatted_Address AS Address
  FROM dbo.TSql_Jobside
 WHERE IdObject = @p0 AND Is_Delete = 0", row.LinkJobside.Value).FirstOrDefault();
                if (jobside != null)
                {
                    ctx.Obra = jobside.TextLabel;
                    ctx.DireccionObra = jobside.Address;
                }
            }

            if (row.LinkClient_V2.HasValue && row.LinkClient_V2.Value > 0)
            {
                var client = db.Database.SqlQuery<ClientSnap>(@"
SELECT TextLabel, CAST(TextTaxId AS NVARCHAR(150)) AS TextTaxId,
       Loc_Formatted_Address AS Address,
       CAST(Loc_Street_Number AS NVARCHAR(100)) AS Loc_Street_Number,
       CAST(Loc_Route AS NVARCHAR(500)) AS Loc_Route,
       CAST(Loc_Subpremise AS NVARCHAR(500)) AS Loc_Subpremise,
       CAST(Loc_Locality AS NVARCHAR(500)) AS Loc_Locality,
       CAST(Loc_Postal_Code AS NVARCHAR(100)) AS Loc_Postal_Code,
       CAST(Loc_Admin_Area_1 AS NVARCHAR(500)) AS Loc_Admin_Area_1
  FROM dbo.TSql_Client_V2
 WHERE IdObject = @p0 AND Is_Delete = 0", row.LinkClient_V2.Value).FirstOrDefault();
                if (client != null)
                {
                    ctx.Cliente = client.TextLabel;
                    ctx.ClienteCifNif = client.TextTaxId;
                    ctx.DireccionCliente = BuildAddress(
                        client.Address,
                        client.Loc_Route,
                        client.Loc_Street_Number,
                        client.Loc_Subpremise,
                        client.Loc_Postal_Code,
                        client.Loc_Locality,
                        client.Loc_Admin_Area_1);
                }
            }

            if (row.LinkPlantilla.HasValue && row.LinkPlantilla.Value > 0)
            {
                var plantilla = db.Database.SqlQuery<PlantillaSnap>(@"
SELECT AttName, AttLogo FROM dbo.TSql_Plantilla WHERE SysObjectID = @p0 AND AttIsDeleted = 0",
                    row.LinkPlantilla.Value).FirstOrDefault();
                if (plantilla != null)
                {
                    ctx.Plantilla = plantilla.AttName;
                    if (string.IsNullOrEmpty(ctx.Logo))
                    {
                        ctx.Logo = plantilla.AttLogo;
                    }
                }
            }
            else if (company != null)
            {
                ctx.Plantilla = company.PlantillaName;
                if (string.IsNullOrEmpty(ctx.Logo))
                {
                    ctx.Logo = company.PlantillaLogo;
                }
            }

            ctx.LinkOffers = Positive(row.LinkOffers);
            ctx.LinkJobside = Positive(row.LinkJobside);
            ctx.LinkClient_V2 = Positive(row.LinkClient_V2);
            ctx.LinkPlantilla = Positive(row.LinkPlantilla);
            return ctx;
        }

        public static ClientOfferBrandRow FindBrand(ConexionData db, long companyId, long systemId)
        {
            var row = db.Database.SqlQuery<ClientOfferBrandRow>(@"
SELECT TOP (1) b.IdObject, b.TextLabel, b.TextCode, b.LinkCompany, b.LinkSystem, b.LinkPlantilla,
       b.TextLogo, b.TextCompanyName, b.TextSubjectTemplate, b.TextHeaderTemplate, b.TextBodyTemplate, b.TextLayoutJson, b.Is_Active,
       c.TextLabel AS CompanyLabel, s.TextLabel AS SystemLabel
  FROM dbo.TSql_ClientOfferBrand b
  LEFT JOIN dbo.TSql_Company c ON c.SysObjectID = b.LinkCompany
  LEFT JOIN dbo.TSql_System s ON s.IdObject = b.LinkSystem
 WHERE b.Is_Delete = 0 AND b.Is_Active = 1 AND b.LinkCompany = @p0
   AND (b.LinkSystem = @p1 OR b.LinkSystem IS NULL)
 ORDER BY CASE WHEN b.LinkSystem = @p1 THEN 0 ELSE 1 END, b.IdObject DESC", companyId, systemId).FirstOrDefault();
            return AttachBrandAssets(db, row);
        }

        public static List<ClientOfferLookup> ListSelectableBrands(ConexionData db, long companyId, long? includeBrandId = null)
        {
            if (companyId <= 0)
            {
                return new List<ClientOfferLookup>();
            }

            return db.Database.SqlQuery<ClientOfferLookup>(@"
SELECT b.IdObject AS Id,
       CASE
           WHEN s.TextLabel IS NULL OR LTRIM(RTRIM(s.TextLabel)) = N'' THEN b.TextLabel
           ELSE b.TextLabel + N' [' + s.TextLabel + N']'
       END AS TextLabel
  FROM dbo.TSql_ClientOfferBrand b
  LEFT JOIN dbo.TSql_System s ON s.IdObject = b.LinkSystem
 WHERE b.Is_Delete = 0
   AND b.LinkCompany = @p0
   AND (b.Is_Active = 1 OR b.IdObject = @p1)
 ORDER BY b.TextLabel, s.TextLabel", companyId, DbLong(includeBrandId)).ToList();
        }

        public static Dictionary<string, string> Tokens(ClientOfferContext ctx, string subject, string recuento)
        {
            return new Dictionary<string, string>
            {
                { "Empresa", ctx == null ? "" : ctx.Empresa ?? "" },
                { "DireccionEmpresa", ctx == null ? "" : ctx.DireccionEmpresa ?? "" },
                { "Cliente", ctx == null ? "" : ctx.Cliente ?? "" },
                { "ClienteCifNif", ctx == null || string.IsNullOrWhiteSpace(ctx.ClienteCifNif) ? "[CIF del cliente]" : ctx.ClienteCifNif },
                { "DireccionCliente", ctx == null ? "" : ctx.DireccionCliente ?? "" },
                { "DireccionObra", ctx == null ? "" : ctx.DireccionObra ?? "" },
                { "NombreContacto", "" },
                { "EmailContacto", "" },
                { "Asunto", subject ?? "" },
                { "Diseno", ctx == null ? "" : ctx.Diseno ?? "" },
                { "Oferta", ctx == null ? "" : ctx.Oferta ?? "" },
                { "Obra", ctx == null ? "" : ctx.Obra ?? "" },
                { "Plantilla", ctx == null ? "" : ctx.Plantilla ?? "" },
                { "Sistema", ctx == null ? "" : ctx.Sistema ?? "" },
                { "Recuento", recuento ?? "" },
                { "Material", "" }
            };
        }

        public static List<ClientOfferBrandRow> ListBrands(ConexionData db)
        {
            return db.Database.SqlQuery<ClientOfferBrandRow>(@"
SELECT b.IdObject, b.TextLabel, b.TextCode, b.LinkCompany, b.LinkSystem, b.LinkPlantilla,
       b.TextLogo, b.TextCompanyName, b.TextSubjectTemplate, b.TextHeaderTemplate, b.TextBodyTemplate, b.TextLayoutJson, b.Is_Active,
       c.TextLabel AS CompanyLabel, ISNULL(s.TextLabel, N'') AS SystemLabel
  FROM dbo.TSql_ClientOfferBrand b
  LEFT JOIN dbo.TSql_Company c ON c.SysObjectID = b.LinkCompany
  LEFT JOIN dbo.TSql_System s ON s.IdObject = b.LinkSystem
 WHERE b.Is_Delete = 0
 ORDER BY b.TextLabel").ToList();
        }

        public static ClientOfferBrandRow GetBrand(ConexionData db, long id)
        {
            var row = db.Database.SqlQuery<ClientOfferBrandRow>(@"
SELECT b.IdObject, b.TextLabel, b.TextCode, b.LinkCompany, b.LinkSystem, b.LinkPlantilla,
       b.TextLogo, b.TextCompanyName, b.TextSubjectTemplate, b.TextHeaderTemplate, b.TextBodyTemplate, b.TextLayoutJson, b.Is_Active,
       c.TextLabel AS CompanyLabel, s.TextLabel AS SystemLabel
  FROM dbo.TSql_ClientOfferBrand b
  LEFT JOIN dbo.TSql_Company c ON c.SysObjectID = b.LinkCompany
  LEFT JOIN dbo.TSql_System s ON s.IdObject = b.LinkSystem
 WHERE b.IdObject = @p0 AND b.Is_Delete = 0", id).FirstOrDefault();
            return AttachBrandAssets(db, row);
        }

        public static long InsertBrand(ConexionData db, ClientOfferBrandRow row, string userId)
        {
            return db.Database.SqlQuery<long>(@"
INSERT INTO dbo.TSql_ClientOfferBrand
    (TextLabel, TextCode, LinkCompany, LinkSystem, LinkPlantilla, TextLogo, TextCompanyName,
     TextSubjectTemplate, TextHeaderTemplate, TextBodyTemplate, TextLayoutJson,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
OUTPUT INSERTED.IdObject
VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, 0, @p11, @p12, NULL, GETDATE(), NULL, 0)",
                Cut(row.TextLabel, 500),
                Db(Cut(row.TextCode, 100)),
                row.LinkCompany,
                DbLong(row.LinkSystem),
                DbLong(row.LinkPlantilla),
                Db(Cut(row.TextLogo, 500)),
                Db(Cut(row.TextCompanyName, 500)),
                Db(Cut(row.TextSubjectTemplate, 500)),
                Db(row.TextHeaderTemplate),
                Db(row.TextBodyTemplate),
                Db(row.TextLayoutJson),
                row.Is_Active,
                userId ?? "system").Single();
        }

        public static void UpdateBrand(ConexionData db, ClientOfferBrandRow row, string userId)
        {
            db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOfferBrand
   SET TextLabel = @p0, TextCode = @p1, LinkCompany = @p2, LinkSystem = @p3, LinkPlantilla = @p4,
       TextLogo = @p5, TextCompanyName = @p6, TextSubjectTemplate = @p7, TextHeaderTemplate = @p8,
       TextBodyTemplate = @p9, TextLayoutJson = @p10, Is_Active = @p11,
       LinModifiedBy = @p12, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p13 AND Is_Delete = 0",
                Cut(row.TextLabel, 500),
                Db(Cut(row.TextCode, 100)),
                row.LinkCompany,
                DbLong(row.LinkSystem),
                DbLong(row.LinkPlantilla),
                Db(Cut(row.TextLogo, 500)),
                Db(Cut(row.TextCompanyName, 500)),
                Db(Cut(row.TextSubjectTemplate, 500)),
                Db(row.TextHeaderTemplate),
                Db(row.TextBodyTemplate),
                Db(row.TextLayoutJson),
                row.Is_Active,
                userId ?? "system",
                row.IdObject);
        }

        public static void SoftDeleteBrand(ConexionData db, long id, string userId)
        {
            db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOfferBrand
   SET Is_Delete = 1, LinModifiedBy = @p1, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0 AND Is_Delete = 0", id, userId ?? "system");
        }

        public static void SyncBrandLogos(ConexionData db, long brandId, IList<long> keepLogoIdsInOrder, IList<string> newLogoPaths, string userId)
        {
            var by = userId ?? "system";
            var keepIds = (keepLogoIdsInOrder ?? new List<long>()).Where(p => p > 0).Distinct().ToList();
            var safeNewPaths = (newLogoPaths ?? new List<string>())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => Cut(p.Trim(), 1000))
                .ToList();

            var currentRows = ListBrandLogos(db, brandId);
            foreach (var row in currentRows)
            {
                if (!keepIds.Contains(row.IdObject))
                {
                    db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOfferBrandLogo
   SET Is_Delete = 1,
       LinModifiedBy = @p2,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0
   AND LinkClientOfferBrand = @p1
   AND Is_Delete = 0", row.IdObject, brandId, by);
                }
            }

            var order = 1;
            foreach (var keepId in keepIds)
            {
                db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOfferBrandLogo
   SET NumberOrder = @p2,
       LinModifiedBy = @p3,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0
   AND LinkClientOfferBrand = @p1
   AND Is_Delete = 0", keepId, brandId, order, by);
                order++;
            }

            foreach (var path in safeNewPaths)
            {
                db.Database.ExecuteSqlCommand(@"
INSERT INTO dbo.TSql_ClientOfferBrandLogo
    (TextLabel, LinkClientOfferBrand, NumberOrder, TextImagePath,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
VALUES (@p0, @p1, @p2, @p3, 0, 1, @p4, NULL, GETDATE(), NULL, 0)",
                    Cut("Logo " + order, 500),
                    brandId,
                    order,
                    path,
                    by);
                order++;
            }
        }

        public static void SyncBrandTexts(ConexionData db, long brandId, IList<ClientOfferBrandTextRow> textRows, string userId)
        {
            var by = userId ?? "system";
            var safeRows = (textRows ?? new List<ClientOfferBrandTextRow>())
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.TextBody))
                .ToList();
            var keepIds = safeRows.Where(r => r.IdObject > 0).Select(r => r.IdObject).Distinct().ToList();

            var currentRows = ListBrandTexts(db, brandId);
            foreach (var current in currentRows)
            {
                if (!keepIds.Contains(current.IdObject))
                {
                    db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOfferBrandText
   SET Is_Delete = 1,
       LinModifiedBy = @p2,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0
   AND LinkClientOfferBrand = @p1
   AND Is_Delete = 0", current.IdObject, brandId, by);
                }
            }

            var order = 1;
            foreach (var row in safeRows)
            {
                var label = Cut((row.TextLabel ?? string.Empty).Trim(), 500);
                if (label.Length == 0)
                {
                    label = Cut("Texto " + order, 500);
                }

                if (row.IdObject > 0)
                {
                    db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOfferBrandText
   SET TextLabel = @p2,
       NumberOrder = @p3,
       TextBody = @p4,
       Is_Active = 1,
       LinModifiedBy = @p5,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0
   AND LinkClientOfferBrand = @p1
   AND Is_Delete = 0",
                        row.IdObject,
                        brandId,
                        label,
                        order,
                        Db(row.TextBody),
                        by);
                }
                else
                {
                    db.Database.ExecuteSqlCommand(@"
INSERT INTO dbo.TSql_ClientOfferBrandText
    (TextLabel, LinkClientOfferBrand, NumberOrder, TextBody,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
VALUES (@p0, @p1, @p2, @p3, 0, 1, @p4, NULL, GETDATE(), NULL, 0)",
                        label,
                        brandId,
                        order,
                        Db(row.TextBody),
                        by);
                }

                order++;
            }
        }

        public static List<ClientOfferBrandLogoRow> ListBrandLogos(ConexionData db, long brandId)
        {
            return db.Database.SqlQuery<ClientOfferBrandLogoRow>(@"
SELECT IdObject, LinkClientOfferBrand, NumberOrder, TextImagePath, Is_Active
  FROM dbo.TSql_ClientOfferBrandLogo
 WHERE LinkClientOfferBrand = @p0
   AND Is_Delete = 0
 ORDER BY NumberOrder, IdObject", brandId).ToList();
        }

        public static List<ClientOfferBrandTextRow> ListBrandTexts(ConexionData db, long brandId)
        {
            return db.Database.SqlQuery<ClientOfferBrandTextRow>(@"
SELECT IdObject, LinkClientOfferBrand, NumberOrder, TextLabel, TextBody, Is_Active
  FROM dbo.TSql_ClientOfferBrandText
 WHERE LinkClientOfferBrand = @p0
   AND Is_Delete = 0
 ORDER BY NumberOrder, IdObject", brandId).ToList();
        }

        public static List<ClientOfferReusableTextRow> ListReusableBrandTexts(ConexionData db, long companyId, long? excludeBrandId = null)
        {
            if (companyId <= 0)
            {
                return new List<ClientOfferReusableTextRow>();
            }

            return db.Database.SqlQuery<ClientOfferReusableTextRow>(@"
SELECT t.IdObject,
       t.LinkClientOfferBrand,
       b.TextLabel AS BrandLabel,
       t.TextLabel,
       t.TextBody
  FROM dbo.TSql_ClientOfferBrandText t
  INNER JOIN dbo.TSql_ClientOfferBrand b ON b.IdObject = t.LinkClientOfferBrand
 WHERE t.Is_Delete = 0
   AND b.Is_Delete = 0
   AND b.LinkCompany = @p0
   AND (@p1 IS NULL OR b.IdObject <> @p1)
   AND ISNULL(LTRIM(RTRIM(t.TextBody)), N'') <> N''
 ORDER BY b.TextLabel, t.TextLabel, t.IdObject",
                companyId,
                DbLong(excludeBrandId)).ToList();
        }

        public static List<ClientOfferRow> ListDocuments(ConexionData db)
        {
            return db.Database.SqlQuery<ClientOfferRow>(@"
SELECT o.IdObject, o.TextLabel, o.LinkDesign, o.LinkOffers, o.LinkJobside, o.LinkClient_V2,
       o.LinkCompany, o.LinkPlantilla, o.LinkSystem, o.LinkClientOfferBrand, o.LinkClientOfferContact,
       o.TextSubject, o.TextClientAddress, o.TextContactName, o.TextContactEmail, o.TextMaterial, o.TextLogo, o.TextRecuento, o.TextStatus, o.TextPdfPath, o.AddPdfDate, o.Is_Active,
       c.TextLabel AS CompanyLabel, s.TextLabel AS SystemLabel, cl.TextLabel AS ClientLabel,
       CAST(NULL AS NVARCHAR(MAX)) AS HeaderBody, CAST(NULL AS NVARCHAR(MAX)) AS TextBody
  FROM dbo.TSql_ClientOffer o
  LEFT JOIN dbo.TSql_Company c ON c.SysObjectID = o.LinkCompany
  LEFT JOIN dbo.TSql_System s ON s.IdObject = o.LinkSystem
  LEFT JOIN dbo.TSql_Client_V2 cl ON cl.IdObject = o.LinkClient_V2
 WHERE o.Is_Delete = 0").ToList();
        }

        public static ClientOfferRow GetDocument(ConexionData db, long id)
        {
            var row = db.Database.SqlQuery<ClientOfferRow>(@"
SELECT o.IdObject, o.TextLabel, o.LinkDesign, o.LinkOffers, o.LinkJobside, o.LinkClient_V2,
       o.LinkCompany, o.LinkPlantilla, o.LinkSystem, o.LinkClientOfferBrand, o.LinkClientOfferContact,
       o.TextSubject, o.TextClientAddress, o.TextContactName, o.TextContactEmail, o.TextMaterial, o.TextLogo, o.TextRecuento, o.TextStatus, o.TextPdfPath, o.AddPdfDate, o.Is_Active,
       c.TextLabel AS CompanyLabel, s.TextLabel AS SystemLabel, cl.TextLabel AS ClientLabel,
       CAST(NULL AS NVARCHAR(MAX)) AS HeaderBody, CAST(NULL AS NVARCHAR(MAX)) AS TextBody
  FROM dbo.TSql_ClientOffer o
  LEFT JOIN dbo.TSql_Company c ON c.SysObjectID = o.LinkCompany
  LEFT JOIN dbo.TSql_System s ON s.IdObject = o.LinkSystem
  LEFT JOIN dbo.TSql_Client_V2 cl ON cl.IdObject = o.LinkClient_V2
 WHERE o.IdObject = @p0 AND o.Is_Delete = 0", id).FirstOrDefault();
            if (row == null)
            {
                return null;
            }

            var sections = db.Database.SqlQuery<SectionSnap>(@"
SELECT TextCode, TextBody
  FROM dbo.TSql_ClientOfferSection
 WHERE LinkClientOffer = @p0 AND Is_Delete = 0", id).ToList();
            row.HeaderBody = sections.Where(s => s.TextCode == "ENCABEZADO").Select(s => s.TextBody).FirstOrDefault();
            row.TextBody = sections.Where(s => s.TextCode == "TEXTO").Select(s => s.TextBody).FirstOrDefault();
            row.Captures = ListCaptures(db, id);
            if (row.LinkClientOfferBrand.HasValue && row.LinkClientOfferBrand.Value > 0)
            {
                var brand = GetBrand(db, row.LinkClientOfferBrand.Value);
                row.TemplateLogos = ResolvePlacedBrandLogos(brand);
            }

            return row;
        }

        public static long InsertDocument(ConexionData db, ClientOfferRow row, string header, string body, string userId)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                var id = db.Database.SqlQuery<long>(@"
INSERT INTO dbo.TSql_ClientOffer
    (TextLabel, TextCode, LinkDesign, LinkOffers, LinkJobside, LinkClient_V2, LinkCompany, LinkPlantilla,
     LinkSystem, LinkClientOfferBrand, LinkClientOfferContact, TextSubject, TextClientAddress, TextContactName, TextContactEmail, TextMaterial, TextLogo, TextRecuento, TextStatus,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
OUTPUT INSERTED.IdObject
VALUES (@p0, NULL, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16, N'BORRADOR',
        0, 1, @p17, NULL, GETDATE(), NULL, 0)",
                    Cut(row.TextSubject, 500),
                    DbLong(row.LinkDesign),
                    DbLong(row.LinkOffers),
                    DbLong(row.LinkJobside),
                    DbLong(row.LinkClient_V2),
                    row.LinkCompany,
                    DbLong(row.LinkPlantilla),
                    row.LinkSystem,
                    DbLong(row.LinkClientOfferBrand),
                    DbLong(row.LinkClientOfferContact),
                    Cut(row.TextSubject, 500),
                    Db(Cut(row.TextClientAddress, 1000)),
                    Db(Cut(row.TextContactName, 500)),
                    Db(Cut(row.TextContactEmail, 500)),
                    Db(row.TextMaterial),
                    Db(Cut(row.TextLogo, 500)),
                    Db(row.TextRecuento),
                    userId ?? "system").Single();

                InsertSection(db, id, "ENCABEZADO", "Encabezado", 10, header, userId);
                InsertSection(db, id, "TEXTO", "Texto", 20, body, userId);
                tx.Commit();
                return id;
            }
        }

        public static void UpdateDocument(ConexionData db, ClientOfferRow row, string userId)
        {
            using (var tx = db.Database.BeginTransaction())
            {
                db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOffer
   SET TextLabel = @p0, LinkDesign = @p1, LinkOffers = @p2, LinkJobside = @p3, LinkClient_V2 = @p4,
       LinkCompany = @p5, LinkPlantilla = @p6, LinkSystem = @p7, LinkClientOfferBrand = @p8, LinkClientOfferContact = @p9,
       TextSubject = @p10, TextClientAddress = @p11, TextContactName = @p12, TextContactEmail = @p13, TextMaterial = @p14, TextLogo = @p15, TextRecuento = @p16, Is_Active = @p17,
       LinModifiedBy = @p18, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p19 AND Is_Delete = 0",
                    Cut(row.TextSubject, 500),
                    DbLong(row.LinkDesign),
                    DbLong(row.LinkOffers),
                    DbLong(row.LinkJobside),
                    DbLong(row.LinkClient_V2),
                    row.LinkCompany,
                    DbLong(row.LinkPlantilla),
                    row.LinkSystem,
                    DbLong(row.LinkClientOfferBrand),
                    DbLong(row.LinkClientOfferContact),
                    Cut(row.TextSubject, 500),
                    Db(Cut(row.TextClientAddress, 1000)),
                    Db(Cut(row.TextContactName, 500)),
                    Db(Cut(row.TextContactEmail, 500)),
                    Db(row.TextMaterial),
                    Db(Cut(row.TextLogo, 500)),
                    Db(row.TextRecuento),
                    row.Is_Active,
                    userId ?? "system",
                    row.IdObject);
                UpsertSection(db, row.IdObject, "ENCABEZADO", "Encabezado", 10, row.HeaderBody, userId);
                UpsertSection(db, row.IdObject, "TEXTO", "Texto", 20, row.TextBody, userId);
                tx.Commit();
            }
        }

        public static void SoftDeleteDocument(ConexionData db, long id, string userId)
        {
            var by = userId ?? "system";
            db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOfferSection
   SET Is_Delete = 1, LinModifiedBy = @p1, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE LinkClientOffer = @p0 AND Is_Delete = 0;
UPDATE dbo.TSql_ClientOffer
   SET Is_Delete = 1, LinModifiedBy = @p1, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0 AND Is_Delete = 0;", id, by);
        }

        public static void UpdateDocumentPdfPath(ConexionData db, long id, string pdfPath, string userId)
        {
            db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOffer
   SET TextPdfPath = @p1, AddPdfDate = GETDATE(),
       LinModifiedBy = @p2, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0 AND Is_Delete = 0",
                id,
                Db(Cut(pdfPath, 500)),
                userId ?? "system");
        }

        public static List<ClientOfferCaptureRow> ListCaptures(ConexionData db, long offerId)
        {
            return db.Database.SqlQuery<ClientOfferCaptureRow>(@"
SELECT IdObject, LinkClientOffer, NumberOrder, TextImagePath, TextLabel, AddDateMade
  FROM dbo.TSql_ClientOfferCapture
 WHERE LinkClientOffer = @p0
   AND Is_Delete = 0
 ORDER BY NumberOrder, IdObject", offerId).ToList();
        }

        public static long InsertCapture(ConexionData db, long offerId, string imagePath, string label, string userId)
        {
            return db.Database.SqlQuery<long>(@"
DECLARE @nextOrder INT = (
    SELECT ISNULL(MAX(NumberOrder), 0) + 1
      FROM dbo.TSql_ClientOfferCapture
     WHERE LinkClientOffer = @p0
       AND Is_Delete = 0
);
INSERT INTO dbo.TSql_ClientOfferCapture
    (TextLabel, LinkClientOffer, NumberOrder, TextImagePath,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
OUTPUT INSERTED.IdObject
VALUES (@p1, @p0, @nextOrder, @p2, 0, 1, @p3, NULL, GETDATE(), NULL, 0)",
                offerId,
                Db(Cut(label, 500)),
                Cut(imagePath, 1000),
                userId ?? "system").Single();
        }

        public static void SoftDeleteCapture(ConexionData db, long offerId, long captureId, string userId)
        {
            db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOfferCapture
   SET Is_Delete = 1,
       LinModifiedBy = @p2,
       AddLastDateChange = GETDATE(),
       Ntimeschanged = Ntimeschanged + 1
 WHERE IdObject = @p0
   AND LinkClientOffer = @p1
   AND Is_Delete = 0", captureId, offerId, userId ?? "system");
        }

        public static List<ClientOfferAiMessageRow> ListAssistantMessages(ConexionData db, long offerId)
        {
            return db.Database.SqlQuery<ClientOfferAiMessageRow>(@"
SELECT IdObject, LinkClientOffer, TextRole, TextMessage, AddPayloadJson, AddDateMade
  FROM dbo.TSql_ClientOfferAiMessage
 WHERE LinkClientOffer = @p0 AND Is_Delete = 0 AND Is_Active = 1
 ORDER BY IdObject", offerId).ToList();
        }

        public static void InsertAssistantMessage(ConexionData db, long offerId, string role, string message, string payloadJson, string userId)
        {
            db.Database.ExecuteSqlCommand(@"
INSERT INTO dbo.TSql_ClientOfferAiMessage
    (TextLabel, LinkClientOffer, TextRole, TextMessage, AddPayloadJson,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
VALUES (@p0, @p1, @p2, @p3, @p4, 0, 1, @p5, NULL, GETDATE(), NULL, 0)",
                Cut((role ?? "ia").ToUpperInvariant() + " " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), 500),
                offerId,
                Cut(role, 50),
                Db(message),
                Db(payloadJson),
                userId ?? "system");
        }

        private static void InsertSection(ConexionData db, long offerId, string code, string title, int order, string body, string userId)
        {
            db.Database.ExecuteSqlCommand(@"
INSERT INTO dbo.TSql_ClientOfferSection
    (TextLabel, LinkClientOffer, TextCode, NumberOrder, TextBody,
     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
VALUES (@p0, @p1, @p2, @p3, @p4, 0, 1, @p5, NULL, GETDATE(), NULL, 0)",
                title, offerId, code, order, Db(body), userId ?? "system");
        }

        private static void UpsertSection(ConexionData db, long offerId, string code, string title, int order, string body, string userId)
        {
            var n = db.Database.ExecuteSqlCommand(@"
UPDATE dbo.TSql_ClientOfferSection
   SET TextLabel = @p2, NumberOrder = @p3, TextBody = @p4,
       LinModifiedBy = @p5, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
 WHERE LinkClientOffer = @p0 AND TextCode = @p1 AND Is_Delete = 0",
                offerId, code, title, order, Db(body), userId ?? "system");
            if (n == 0)
            {
                InsertSection(db, offerId, code, title, order, body, userId);
            }
        }

        private static ClientOfferBrandRow AttachBrandAssets(ConexionData db, ClientOfferBrandRow row)
        {
            if (row == null || row.IdObject <= 0)
            {
                return row;
            }

            row.Logos = ListBrandLogos(db, row.IdObject);
            row.TextBlocks = ListBrandTexts(db, row.IdObject);
            return row;
        }

        private static List<string> ResolvePlacedBrandLogos(ClientOfferBrandRow brand)
        {
            var result = new List<string>();
            if (brand == null || brand.Logos == null || brand.Logos.Count == 0)
            {
                return result;
            }

            var logos = brand.Logos
                .OrderBy(p => p.NumberOrder)
                .ThenBy(p => p.IdObject)
                .Select(p => p.TextImagePath ?? string.Empty)
                .ToList();

            var indexes = ParseLayoutLogoAssetIndexes(brand.TextLayoutJson);
            foreach (var idx in indexes)
            {
                var pos = idx - 1;
                if (pos < 0 || pos >= logos.Count)
                {
                    continue;
                }

                var path = logos[pos];
                if (!string.IsNullOrWhiteSpace(path))
                {
                    result.Add(path);
                }
            }

            return result;
        }

        private static List<int> ParseLayoutLogoAssetIndexes(string rawJson)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return result;
            }

            try
            {
                var token = JToken.Parse(rawJson);
                var rows = token["logoPlacements"] as JArray;
                if (rows == null)
                {
                    return result;
                }

                foreach (var row in rows)
                {
                    var idx = (int?)row["assetIndex"] ?? 0;
                    if (idx <= 0)
                    {
                        idx = (int?)row["order"] ?? 0;
                    }

                    if (idx > 0)
                    {
                        result.Add(idx);
                    }
                }
            }
            catch
            {
                // Layout inválido: no rompe la carga de oferta.
            }

            return result;
        }

        private static long? Positive(long? value)
        {
            return value.HasValue && value.Value > 0 ? value : null;
        }

        private static object DbLong(long? value)
        {
            return value.HasValue && value.Value > 0 ? (object)value.Value : DBNull.Value;
        }

        private static object Db(string value)
        {
            return string.IsNullOrEmpty(value) ? (object)DBNull.Value : value;
        }

        private static string Cut(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
            {
                return value;
            }

            return value.Substring(0, max);
        }

        private static string BuildAddress(params string[] parts)
        {
            if (parts == null || parts.Length == 0)
            {
                return string.Empty;
            }

            var formatted = parts[0];
            if (!string.IsNullOrWhiteSpace(formatted))
            {
                return formatted.Trim();
            }

            var values = parts
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct()
                .ToList();

            if (values.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(", ", values);
        }

        private class CompanySnap
        {
            public string TextLabel { get; set; }
            public string TextLogo { get; set; }
            public string Address { get; set; }
            public string TextAddress_1 { get; set; }
            public string TextAddress_2 { get; set; }
            public string TextPostal_Code { get; set; }
            public string TextTown_1 { get; set; }
            public string TextTown_2 { get; set; }
            public long? LinPlantilla { get; set; }
            public string PlantillaName { get; set; }
            public string PlantillaLogo { get; set; }
        }

        private class DesignSnap
        {
            public string AttLabel { get; set; }
            public long? LinkOffers { get; set; }
        }

        private class OfferSnap
        {
            public string TextLabel { get; set; }
            public string AddOfferNumber { get; set; }
            public long LinkJobside { get; set; }
            public long LinkClient_V2 { get; set; }
        }

        private class JobsideSnap
        {
            public string TextLabel { get; set; }
            public string Address { get; set; }
        }

        private class ClientSnap
        {
            public string TextLabel { get; set; }
            public string TextTaxId { get; set; }
            public string Address { get; set; }
            public string Loc_Street_Number { get; set; }
            public string Loc_Route { get; set; }
            public string Loc_Subpremise { get; set; }
            public string Loc_Locality { get; set; }
            public string Loc_Postal_Code { get; set; }
            public string Loc_Admin_Area_1 { get; set; }
        }

        private class PlantillaSnap
        {
            public string AttName { get; set; }
            public string AttLogo { get; set; }
        }

        private class SectionSnap
        {
            public string TextCode { get; set; }
            public string TextBody { get; set; }
        }
    }
}
