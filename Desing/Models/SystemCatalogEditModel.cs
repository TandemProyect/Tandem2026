namespace Desing.Models
{
    public sealed class SystemCatalogEditModel
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextDescription { get; set; }
        public string TextPanelColor { get; set; }
        public bool AddIsActive { get; set; }
    }
}
