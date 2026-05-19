namespace Desing.Models
{
    /// <summary>
    /// Fila para selector UI de idioma (SqlQuery; TextFlagRaw viene del país vinculado).
    /// </summary>
    public class LanguageNavItem
    {
        public long IdObject { get; set; }
        public string TextCode { get; set; }
        public string TextLabel { get; set; }
        public bool IsDefault { get; set; }
        public string TextFlagRaw { get; set; }
    }
}
