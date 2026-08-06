using Desing.Models.TandemXr;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace Desing.Helpers
{
    /// <summary>
    /// Acceso SQL a <c>dbo.TSql_XrPushJob</c> hasta incorporar la tabla al EDMX.
    /// </summary>
    public static class XrPushJobQueries
    {
        public static bool TableExists(Database database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            return database.SqlQuery<int?>(
                @"SELECT 1 FROM INFORMATION_SCHEMA.TABLES
                  WHERE TABLE_SCHEMA = N'dbo' AND TABLE_NAME = N'TSql_XrPushJob'").FirstOrDefault() == 1;
        }

        public static long Insert(Database database, XrPushJobEntity entity)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            return database.SqlQuery<long>(
                @"INSERT INTO dbo.TSql_XrPushJob
                    (TextLabel, LinkXrDevice, LinkDesign, LinkOffer, TextStatus, DateDelivered,
                     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
                  OUTPUT INSERTED.IdObject
                  VALUES
                    (@TextLabel, @LinkXrDevice, @LinkDesign, @LinkOffer, @TextStatus, NULL,
                     0, 1, @LinkMadeBy, NULL, @AddDateMade, NULL, 0)",
                new SqlParameter("@TextLabel", (object)entity.TextLabel ?? DBNull.Value),
                new SqlParameter("@LinkXrDevice", entity.LinkXrDevice),
                new SqlParameter("@LinkDesign", entity.LinkDesign),
                new SqlParameter("@LinkOffer", (object)entity.LinkOffer ?? DBNull.Value),
                new SqlParameter("@TextStatus", (object)entity.TextStatus ?? XrPushJobStatus.Pending),
                new SqlParameter("@LinkMadeBy", (object)entity.LinkMadeBy ?? DBNull.Value),
                new SqlParameter("@AddDateMade", entity.AddDateMade)).FirstOrDefault();
        }

        public static XrPushJobEntity GetOldestPendingForDevice(Database database, long deviceId)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            return database.SqlQuery<XrPushJobEntity>(
                @"SELECT TOP (1)
                    IdObject, TextLabel, LinkXrDevice, LinkDesign, LinkOffer, TextStatus, DateDelivered,
                    Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
                  FROM dbo.TSql_XrPushJob WITH (NOLOCK)
                  WHERE Is_Delete = 0
                    AND LinkXrDevice = @DeviceId
                    AND TextStatus = @Status
                  ORDER BY AddDateMade ASC, IdObject ASC",
                new SqlParameter("@DeviceId", deviceId),
                new SqlParameter("@Status", XrPushJobStatus.Pending)).FirstOrDefault();
        }

        public static XrPushJobEntity GetById(Database database, long id)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            return database.SqlQuery<XrPushJobEntity>(
                @"SELECT IdObject, TextLabel, LinkXrDevice, LinkDesign, LinkOffer, TextStatus, DateDelivered,
                         Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged
                  FROM dbo.TSql_XrPushJob WITH (NOLOCK)
                  WHERE IdObject = @Id AND Is_Delete = 0",
                new SqlParameter("@Id", id)).FirstOrDefault();
        }

        public static void MarkDelivered(Database database, long jobId, string actorUserId)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            database.ExecuteSqlCommand(
                @"UPDATE dbo.TSql_XrPushJob SET
                    TextStatus = @Status,
                    DateDelivered = GETDATE(),
                    LinModifiedBy = @By,
                    AddLastDateChange = GETDATE(),
                    Ntimeschanged = Ntimeschanged + 1
                  WHERE IdObject = @Id AND Is_Delete = 0 AND TextStatus = @Pending",
                new SqlParameter("@Status", XrPushJobStatus.Delivered),
                new SqlParameter("@By", (object)actorUserId ?? (object)DBNull.Value),
                new SqlParameter("@Id", jobId),
                new SqlParameter("@Pending", XrPushJobStatus.Pending));
        }

        public static int CountPendingForDevice(Database database, long deviceId)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            return database.SqlQuery<int>(
                @"SELECT COUNT(1) FROM dbo.TSql_XrPushJob WITH (NOLOCK)
                  WHERE Is_Delete = 0 AND LinkXrDevice = @DeviceId AND TextStatus = @Status",
                new SqlParameter("@DeviceId", deviceId),
                new SqlParameter("@Status", XrPushJobStatus.Pending)).FirstOrDefault();
        }
    }
}
