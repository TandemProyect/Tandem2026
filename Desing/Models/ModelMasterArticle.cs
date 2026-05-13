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

        public string LinkBlockDwgPlant3D { get; set; }
        public string LinkBlockDwgVerticalElevation3D { get; set; }
        public string LinkBlockDwgHorizontalElevation3D { get; set; }
        public string LinkBlockDwgPlantMckUp { get; set; }
        public string LinkBlockDwgVerticalElevationMockUp { get; set; }
        public string LinkBlockDwgHorizontalElevationMockUp { get; set; }
        public string LinkBlockDwgPlantStl { get; set; }
        public string LinkBlockDwgVerticalElevationStl { get; set; }
        public string LinkBlockDwgHorizontalElevationStl { get; set; }
        public bool IInsertinMaterArticles { get; set; }
    }

    /// <summary>Datos para la parcial de adjunto DWG en Detalles (evita Razor anidado en el foreach).</summary>
    public class MasterArticleDwgSlotModel
    {
        public string Href { get; set; }
        public string VirtualPath { get; set; }
        public string SlotKey { get; set; }
        public long ArticleId { get; set; }
        /// <summary>True si existe el .dxf gemelo (mismo nombre base que el .dwg) en la misma carpeta.</summary>
        public bool LoadDxfBrowserPreview { get; set; }
        /// <summary>URL para abrir/descargar el .dxf gemelo (vacío si no existe).</summary>
        public string SiblingDxfHref { get; set; }
        /// <summary>Nombre del fichero .dxf esperado (solo nombre).</summary>
        public string SiblingDxfFileName { get; set; }
    }

}
