using System;

namespace Desing.Models.TandemXr
{
    /// <summary>
    /// POCO de <c>dbo.TSql_XrDevice</c> (acceso SQL hasta Update Model from Database).
    /// </summary>
    public class XrDeviceEntity
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextDeviceType { get; set; }
        public string TextPairingCode { get; set; }
        public string TextNotes { get; set; }
        public bool Is_Paired { get; set; }
        public DateTime? DateLastSeen { get; set; }
        public bool Is_Delete { get; set; }
        public bool Is_Active { get; set; }
        public string LinkMadeBy { get; set; }
        public string LinModifiedBy { get; set; }
        public DateTime AddDateMade { get; set; }
        public DateTime? AddLastDateChange { get; set; }
        public long Ntimeschanged { get; set; }
    }
}
