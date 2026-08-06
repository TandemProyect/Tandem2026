using Desing.Models.TandemXr;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace Desing.Helpers
{
    /// <summary>
    /// Acceso SQL a <c>dbo.TSql_XrDevice</c> hasta incorporar la tabla al EDMX.
    /// </summary>
    public static class XrDeviceQueries
    {
        public static bool TableExists(Database database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            return database.SqlQuery<int?>(
                @"SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                  WHERE TABLE_SCHEMA = N'dbo' AND TABLE_NAME = N'TSql_XrDevice'").FirstOrDefault() == 1;
        }

        public static List<XrDeviceEntity> ListActive(Database database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            return database.SqlQuery<XrDeviceEntity>(
                @"SELECT IdObject, TextLabel, TextDeviceType, TextPairingCode, TextNotes,
                         Is_Paired, DateLastSeen, Is_Delete, Is_Active,
                         LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
                  FROM dbo.TSql_XrDevice WITH (NOLOCK)
                  WHERE Is_Delete = 0
                  ORDER BY TextLabel").ToList();
        }

        public static List<XrDeviceEntity> ListSelectable(Database database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            return database.SqlQuery<XrDeviceEntity>(
                @"SELECT IdObject, TextLabel, TextDeviceType, TextPairingCode, TextNotes,
                         Is_Paired, DateLastSeen, Is_Delete, Is_Active,
                         LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
                  FROM dbo.TSql_XrDevice WITH (NOLOCK)
                  WHERE Is_Delete = 0 AND Is_Active = 1
                  ORDER BY TextLabel").ToList();
        }

        public static XrDeviceEntity GetById(Database database, long id)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            return database.SqlQuery<XrDeviceEntity>(
                @"SELECT IdObject, TextLabel, TextDeviceType, TextPairingCode, TextNotes,
                         Is_Paired, DateLastSeen, Is_Delete, Is_Active,
                         LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
                  FROM dbo.TSql_XrDevice WITH (NOLOCK)
                  WHERE IdObject = @Id AND Is_Delete = 0",
                new SqlParameter("@Id", id)).FirstOrDefault();
        }

        public static XrDeviceEntity GetByPairingCode(Database database, string pairingCode)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (string.IsNullOrWhiteSpace(pairingCode)) return null;

            return database.SqlQuery<XrDeviceEntity>(
                @"SELECT IdObject, TextLabel, TextDeviceType, TextPairingCode, TextNotes,
                         Is_Paired, DateLastSeen, Is_Delete, Is_Active,
                         LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
                  FROM dbo.TSql_XrDevice WITH (NOLOCK)
                  WHERE TextPairingCode = @Code AND Is_Delete = 0 AND Is_Active = 1",
                new SqlParameter("@Code", pairingCode.Trim())).FirstOrDefault();
        }

        public static bool LabelExists(Database database, string label, long? excludeId)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            var labelLower = (label ?? "").Trim().ToLowerInvariant();
            if (labelLower.Length == 0) return false;

            var sql = @"SELECT COUNT(1) FROM dbo.TSql_XrDevice WITH (NOLOCK)
                        WHERE Is_Delete = 0 AND LOWER(TextLabel) = @Label";
            if (excludeId.HasValue)
            {
                sql += " AND IdObject <> @ExcludeId";
            }

            var p = new List<SqlParameter>
            {
                new SqlParameter("@Label", labelLower)
            };
            if (excludeId.HasValue)
            {
                p.Add(new SqlParameter("@ExcludeId", excludeId.Value));
            }

            return database.SqlQuery<int>(sql, p.ToArray()).FirstOrDefault() > 0;
        }

        public static bool PairingCodeExists(Database database, string code, long? excludeId)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            var c = (code ?? "").Trim();
            if (c.Length == 0) return false;

            var sql = @"SELECT COUNT(1) FROM dbo.TSql_XrDevice WITH (NOLOCK)
                        WHERE Is_Delete = 0 AND TextPairingCode = @Code";
            if (excludeId.HasValue)
            {
                sql += " AND IdObject <> @ExcludeId";
            }

            var p = new List<SqlParameter> { new SqlParameter("@Code", c) };
            if (excludeId.HasValue)
            {
                p.Add(new SqlParameter("@ExcludeId", excludeId.Value));
            }

            return database.SqlQuery<int>(sql, p.ToArray()).FirstOrDefault() > 0;
        }

        public static long Insert(Database database, XrDeviceEntity entity)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var id = database.SqlQuery<long>(
                @"INSERT INTO dbo.TSql_XrDevice
                    (TextLabel, TextDeviceType, TextPairingCode, TextNotes, Is_Paired, DateLastSeen,
                     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
                  OUTPUT INSERTED.IdObject
                  VALUES
                    (@TextLabel, @TextDeviceType, @TextPairingCode, @TextNotes, @Is_Paired, @DateLastSeen,
                     0, @Is_Active, @LinkMadeBy, NULL, @AddDateMade, NULL, 0)",
                new SqlParameter("@TextLabel", (object)entity.TextLabel ?? DBNull.Value),
                new SqlParameter("@TextDeviceType", (object)entity.TextDeviceType ?? DBNull.Value),
                new SqlParameter("@TextPairingCode", (object)entity.TextPairingCode ?? DBNull.Value),
                new SqlParameter("@TextNotes", (object)entity.TextNotes ?? DBNull.Value),
                new SqlParameter("@Is_Paired", entity.Is_Paired),
                new SqlParameter("@DateLastSeen", (object)entity.DateLastSeen ?? DBNull.Value),
                new SqlParameter("@Is_Active", entity.Is_Active),
                new SqlParameter("@LinkMadeBy", (object)entity.LinkMadeBy ?? DBNull.Value),
                new SqlParameter("@AddDateMade", entity.AddDateMade)).FirstOrDefault();

            return id;
        }

        public static void Update(Database database, XrDeviceEntity entity)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            database.ExecuteSqlCommand(
                @"UPDATE dbo.TSql_XrDevice SET
                    TextLabel = @TextLabel,
                    TextDeviceType = @TextDeviceType,
                    TextPairingCode = @TextPairingCode,
                    TextNotes = @TextNotes,
                    Is_Active = @Is_Active,
                    LinModifiedBy = @LinModifiedBy,
                    AddLastDateChange = @AddLastDateChange,
                    Ntimeschanged = @Ntimeschanged
                  WHERE IdObject = @Id AND Is_Delete = 0",
                new SqlParameter("@TextLabel", (object)entity.TextLabel ?? DBNull.Value),
                new SqlParameter("@TextDeviceType", (object)entity.TextDeviceType ?? DBNull.Value),
                new SqlParameter("@TextPairingCode", (object)entity.TextPairingCode ?? DBNull.Value),
                new SqlParameter("@TextNotes", (object)entity.TextNotes ?? DBNull.Value),
                new SqlParameter("@Is_Active", entity.Is_Active),
                new SqlParameter("@LinModifiedBy", (object)entity.LinModifiedBy ?? DBNull.Value),
                new SqlParameter("@AddLastDateChange", (object)entity.AddLastDateChange ?? DBNull.Value),
                new SqlParameter("@Ntimeschanged", entity.Ntimeschanged),
                new SqlParameter("@Id", entity.IdObject));
        }

        public static void SoftDelete(Database database, XrDeviceEntity entity)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            database.ExecuteSqlCommand(
                @"UPDATE dbo.TSql_XrDevice SET
                    Is_Delete = 1,
                    LinModifiedBy = @LinModifiedBy,
                    AddLastDateChange = @AddLastDateChange,
                    Ntimeschanged = @Ntimeschanged
                  WHERE IdObject = @Id AND Is_Delete = 0",
                new SqlParameter("@LinModifiedBy", (object)entity.LinModifiedBy ?? DBNull.Value),
                new SqlParameter("@AddLastDateChange", (object)entity.AddLastDateChange ?? DBNull.Value),
                new SqlParameter("@Ntimeschanged", entity.Ntimeschanged),
                new SqlParameter("@Id", entity.IdObject));
        }

        public static void TouchSeen(Database database, long id, string actorUserId)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            database.ExecuteSqlCommand(
                @"UPDATE dbo.TSql_XrDevice SET
                    Is_Paired = 1,
                    DateLastSeen = GETDATE(),
                    LinModifiedBy = @By,
                    AddLastDateChange = GETDATE(),
                    Ntimeschanged = Ntimeschanged + 1
                  WHERE IdObject = @Id AND Is_Delete = 0",
                new SqlParameter("@By", (object)actorUserId ?? (object)DBNull.Value),
                new SqlParameter("@Id", id));
        }

        public static string NewPairingCode()
        {
            // 8 chars A-Z0-9 sin confusos (0/O, 1/I)
            const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var bytes = Guid.NewGuid().ToByteArray();
            var chars = new char[8];
            for (var i = 0; i < 8; i++)
            {
                chars[i] = alphabet[bytes[i] % alphabet.Length];
            }
            return new string(chars);
        }
    }
}
