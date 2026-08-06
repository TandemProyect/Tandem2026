using System;

namespace Desing.Models.TandemXr
{
    /// <summary>
    /// POCO de <c>dbo.TSql_XrPushJob</c> (acceso SQL hasta Update Model from Database).
    /// </summary>
    public class XrPushJobEntity
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public long LinkXrDevice { get; set; }
        public long LinkDesign { get; set; }
        public long? LinkOffer { get; set; }
        public string TextStatus { get; set; }
        public DateTime? DateDelivered { get; set; }
        public bool Is_Delete { get; set; }
        public bool Is_Active { get; set; }
        public string LinkMadeBy { get; set; }
        public string LinModifiedBy { get; set; }
        public DateTime AddDateMade { get; set; }
        public DateTime? AddLastDateChange { get; set; }
        public long Ntimeschanged { get; set; }
    }

    public static class XrPushJobStatus
    {
        public const string Pending = "Pending";
        public const string Delivered = "Delivered";
        public const string Cancelled = "Cancelled";
        public const string Failed = "Failed";
    }

    public static class XrDeviceTypes
    {
        public const string Quest = "Quest";
        public const string Tablet = "Tablet";
    }
}
