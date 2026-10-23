using DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Desing.Repositories.RepositoryDesing2
{
    public sealed class Desing2DesignWorkerDto
    {
        public double XMm { get; set; }
        public double YMm { get; set; }
        public double ZMm { get; set; }
        public double RotY { get; set; }
        public double Scale { get; set; }
        public string TextCode { get; set; }
    }

    /// <summary>
    /// Trabajadores de un diseño V2. SQL directo: la tabla no está en el EDMX.
    /// </summary>
    public sealed class DesignWorkerRepository
    {
        private readonly ConexionData _db;

        public DesignWorkerRepository(ConexionData db)
        {
            _db = db;
        }

        public List<Desing2DesignWorkerDto> ListActive(long designId)
        {
            if (designId <= 0 || !TableExists())
            {
                return new List<Desing2DesignWorkerDto>();
            }

            var codeSql = HasTextCode() ? "TextCode" : "CAST(NULL AS NVARCHAR(50)) AS TextCode";
            return _db.Database.SqlQuery<Desing2DesignWorkerDto>(
                "SELECT NumberXMm AS XMm, NumberYMm AS YMm, NumberZMm AS ZMm, NumberRotY AS RotY, " +
                "ISNULL(NumberScale, 0) AS Scale, " + codeSql + " " +
                "FROM dbo.TSql_DesignWorker WHERE LinkDesign_V2 = @p0 AND Is_Delete = 0 AND Is_Active = 1 " +
                "ORDER BY IdObject",
                designId).ToList();
        }

        public int Replace(long designId, IList<Desing2DesignWorkerDto> workers, string userId)
        {
            if (designId <= 0)
            {
                return 0;
            }

            if (!TableExists())
            {
                throw new InvalidOperationException("Falta la tabla dbo.TSql_DesignWorker.");
            }

            var who = string.IsNullOrWhiteSpace(userId) ? "system" : userId;
            if (who.Length > 128) who = who.Substring(0, 128);
            var now = DateTime.Now;

            _db.Database.ExecuteSqlCommand(
                "UPDATE dbo.TSql_DesignWorker SET Is_Delete = 1, LinModifiedBy = @p0, AddLastDateChange = @p1, Ntimeschanged = Ntimeschanged + 1 " +
                "WHERE LinkDesign_V2 = @p2 AND Is_Delete = 0",
                who,
                now,
                designId);

            var count = 0;
            if (workers == null)
            {
                return count;
            }

            for (var i = 0; i < workers.Count; i++)
            {
                var row = workers[i];
                if (row == null || double.IsNaN(row.XMm) || double.IsNaN(row.ZMm) || double.IsInfinity(row.XMm) || double.IsInfinity(row.ZMm))
                {
                    continue;
                }

                var label = "Trabajador " + (count + 1);
                var scale = row.Scale;
                if (double.IsNaN(scale) || double.IsInfinity(scale) || scale < 0.05d) scale = 0d;
                var code = string.IsNullOrWhiteSpace(row.TextCode) ? "Worker" : row.TextCode.Trim();
                if (code.Length > 50) code = code.Substring(0, 50);
                if (HasTextCode())
                {
                    _db.Database.ExecuteSqlCommand(
                        "INSERT INTO dbo.TSql_DesignWorker " +
                        "(TextLabel, LinkDesign_V2, NumberXMm, NumberYMm, NumberZMm, NumberRotY, NumberScale, TextCode, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged) " +
                        "VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, 0, 1, @p8, NULL, @p9, NULL, 0)",
                        label,
                        designId,
                        row.XMm,
                        row.YMm,
                        row.ZMm,
                        row.RotY,
                        scale,
                        code,
                        who,
                        now);
                }
                else
                {
                    _db.Database.ExecuteSqlCommand(
                        "INSERT INTO dbo.TSql_DesignWorker " +
                        "(TextLabel, LinkDesign_V2, NumberXMm, NumberYMm, NumberZMm, NumberRotY, NumberScale, Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged) " +
                        "VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, 0, 1, @p7, NULL, @p8, NULL, 0)",
                        label,
                        designId,
                        row.XMm,
                        row.YMm,
                        row.ZMm,
                        row.RotY,
                        scale,
                        who,
                        now);
                }
                count++;
            }

            return count;
        }

        private bool TableExists()
        {
            var found = _db.Database.SqlQuery<int>(
                "SELECT CASE WHEN OBJECT_ID(N'dbo.TSql_DesignWorker', N'U') IS NULL THEN 0 ELSE 1 END").FirstOrDefault();
            return found == 1;
        }

        private bool HasTextCode()
        {
            var found = _db.Database.SqlQuery<int>(
                "SELECT CASE WHEN COL_LENGTH(N'dbo.TSql_DesignWorker', N'TextCode') IS NULL THEN 0 ELSE 1 END").FirstOrDefault();
            return found == 1;
        }
    }
}
