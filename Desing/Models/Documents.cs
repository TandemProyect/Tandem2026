namespace Desing.Models
{
    public class DocumentViewModel
    {
        public string Nobra { get; set; }
        public string nOferta { get; set; }
        public string NombreDocumento { get; set; }
        public string NOMBRE_ARCHIVO { get; set; }
        public string RUTA_ARCHIVO { get; set; }
        public string TAMANO { get; set; }
        public string NombreOferta { get; set; }
        public int SysObjectID { get;  set; }
    }



    public class Documents
    {
        public string Filename { get; set; }
        public string ContentId { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Disposition { get; set; }
    }

    public enum Disposition
    {
        inline,
        attachment
    }
}