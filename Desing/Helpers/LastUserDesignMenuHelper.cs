using DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Desing.Helpers
{
    /// <summary>Diseño del usuario, para el apartado del menú lateral.</summary>
    public sealed class LastUserDesignMenuItem
    {
        public long DesignId { get; set; }
        public string AttLabel { get; set; }
        public string AttThumbnail { get; set; }
        public string AttPhoto { get; set; }
        public DateTime AttChange { get; set; }
        public long OfferId { get; set; }
        public string OfferNumber { get; set; }
    }

    public static class LastUserDesignMenuHelper
    {
        /// <summary>Diseños creados por el usuario, del más reciente al más antiguo.</summary>
        public static IList<LastUserDesignMenuItem> List(ConexionData db, string userId)
        {
            if (db == null || string.IsNullOrWhiteSpace(userId))
            {
                return new List<LastUserDesignMenuItem>();
            }

            try
            {
                return Query(db, userId, true);
            }
            catch
            {
                try
                {
                    return Query(db, userId, false);
                }
                catch
                {
                    return new List<LastUserDesignMenuItem>();
                }
            }
        }

        private static List<LastUserDesignMenuItem> Query(ConexionData db, string userId, bool withPhoto)
        {
            var photo = withPhoto
                ? "d.AttPhoto AS AttPhoto"
                : "CAST(NULL AS NVARCHAR(500)) AS AttPhoto";
            return db.Database.SqlQuery<LastUserDesignMenuItem>(
                @"SELECT
                    d.SysObjectID AS DesignId,
                    d.AttLabel AS AttLabel,
                    d.AttThumbnail AS AttThumbnail,
                    " + photo + @",
                    d.AttChange AS AttChange,
                    d.LinkOffers AS OfferId,
                    ISNULL(o.AddOfferNumber, N'') AS OfferNumber
                  FROM dbo.TSql_Design_V2 d
                  LEFT JOIN dbo.TSql_Offers o
                    ON o.IdObject = d.LinkOffers AND o.Is_Delete = 0
                  WHERE d.AttIsDeleted = 0 AND d.LinCreatedBy = {0}
                  ORDER BY d.AttCreated DESC, d.SysObjectID DESC",
                userId).ToList();
        }
    }
}
