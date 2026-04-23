using System;

namespace Desing.Models
{
    public class ListMasterArticle
    {
        public long IdObject { get; set; }
        public string CompanyTextLabel { get; set; }
        public string System_TextLabel { get; set; }
        public string TextCode { get; set; }
        public string TextLabel { get; set; }
        public double? NumberHigh { get; set; }
        public double? NumberWidth { get; set; }
        public double? NumberLong { get; set; }
        public double? NumberWeight { get; set; }
        public double? NumberMts2 { get; set; }
        public double? NumberMts3 { get; set; }
        public string TextBlockNumber { get; set; }
        public string TextStlNumber { get; set; }
        public string TextColor1 { get; set; }
        public string TextColor2 { get; set; }

        public bool AddIsActive { get; set; }
        public DateTime AddChangeBy { get; set; }
    }

}
