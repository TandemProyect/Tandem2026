using System;

namespace Desing.Models
{
    public class ListDesing
    {
        public long IdObject { get; set; }
        public string AttLabel { get; set; }
        public string AttDescription { get; set; }
        public string LinCreatedBy { get; set; }
        public string LinModifiedBy { get; set; }
        public bool AttIsDeleted { get; set; }
        public DateTime AddChangeBy { get; set; }
        public DateTime AttCreated { get; set; }
        public string AttThumbnail { get; set; }
        public string AttNameEmployee { get; set; }
        public string AttSurnameEmployee { get; set; }

        public string DateCreate { get; set; }
        public string AttLabelEmployee { get; set; }
        public string AddLetercompany { get; set; }
        public string Attcompany { get; set; }
        public string AttLabelBranch { get; set; }
        public string AttPhotoMenu { get; set; }
        public DateTime AttChange { get; set; }
        public string DateChange { get; internal set; }
    }
    public class ListTemp
    {
        public Array TextCode { get; set; }
    }


    public class TemporalList
    {
        public string TextCode { get; set; }
    }

    public class SedMailDesingModel
    {
        public long TotalElement { get; set; }
        public double TotalWeight { get; set; }
        public long IdDesing { get; set; }
        public string AddNameDesing { get; set; }
        public string UserName { get; set; }
        public string Type { get; set; }
    }
    public class ListMaterial
    {
        public long Quantity { get; set; }
        public string TextCode { get; set; }
        public string TextLabel { get; set; }
        public double NumberWeight { get; set; }
        public double NumberMts2 { get; set; }
        public double TotalWeight { get; set; }
        public double TotalNumberMts2 { get; set; }
        public string AddGrup { get; set; }
    }

    public class ListMaterialGrup
    {
        public long Quantity { get; set; }
        public string AddGrup { get; set; }
        public double TotalW { get; set; }
        public double TotalM2 { get; set; }

    }


}
