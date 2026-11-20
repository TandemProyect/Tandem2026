using DAL;
using System.Collections.Generic;
using System.Linq;

namespace Desing.Models
{
    /// <summary>Datos compartidos entre Detalles y Editar para la tarjeta del visor STL.</summary>
    public class MasterArticleStlPreviewSectionModel
    {
        public IReadOnlyList<MasterArticleAttachmentSlot> AttachmentSlots { get; set; }
        /// <summary>Color 1 (<c>TextColor1</c>) en hex para el material del STL en el visor Three.js.</summary>
        public string TextColor1Hex { get; set; }
        /// <summary>Color 2 (<c>TextColor2</c>) en hex para la segunda malla (<c>{{base}}2.stl</c>) en el visor.</summary>
        public string TextColor2Hex { get; set; }
        /// <summary>False en Editar: oculta la tabla de adjuntos y auto-carga el STL en el visor.</summary>
        public bool ShowAttachmentGrid { get; set; } = true;
        public bool HasStlPreview => AttachmentSlots != null && AttachmentSlots.Any(s => s.StlPreviewExists);
    }

    /// <summary>Panel STL de Editar: mismas URLs y colores que el recuento Desing_2.</summary>
    public class MasterArticleEditCountStlModel
    {
        public string StlUrl { get; set; }
        public string StlUrl2 { get; set; }
        public string Color1 { get; set; }
        public string Color2 { get; set; }
        public string Label { get; set; }
        /// <summary>True si el artículo es panel ATK-60: el visor usa el color de paneles del sistema.</summary>
        public bool IsPanel { get; set; }
        /// <summary>IdObject de sistema → TextPanelColor, para cambiar el visor al elegir Sistema.</summary>
        public System.Collections.Generic.Dictionary<string, string> SystemPanelColors { get; set; }
    }

    public class MasterArticleDetailsViewModel
    {
        public Tsql_Master_Articles Article { get; set; }
        public string CompanyTextLabel { get; set; }
        public string SystemTextLabel { get; set; }
        public IReadOnlyList<MasterArticleAttachmentSlot> AttachmentSlots { get; set; }
        /// <summary>Hex normalizado en servidor para <see cref="MasterArticleStlPreviewSectionModel.TextColor1Hex"/> en la vista Detalles.</summary>
        public string StlPreviewTextColor1Hex { get; set; }
        /// <summary>Hex normalizado para <see cref="MasterArticleStlPreviewSectionModel.TextColor2Hex"/> en Detalles (TextColor2, longitud máx. 10 al guardar).</summary>
        public string StlPreviewTextColor2Hex { get; set; }
        /// <summary>Mismo panel STL que Editar (recuento Desing_2).</summary>
        public MasterArticleEditCountStlModel CountStl { get; set; }
    }

    public class MasterArticleAttachmentSlot
    {
        /// <summary>Clave de propiedad (p. ej. LinkBlockDwg3D).</summary>
        public string SlotKey { get; set; }
        public string Label { get; set; }
        public string VirtualPath { get; set; }
        /// <summary>none | dwg | stl | dxf (según extensión del archivo).</summary>
        public string ViewerKind { get; set; }
        /// <summary>Ruta virtual del DXF gemelo (mismo nombre base que el .dwg, extensión .dxf).</summary>
        public string SiblingDxfVirtualPath { get; set; }
        /// <summary>True si existe el fichero .dxf gemelo en disco junto al .dwg.</summary>
        public bool SiblingDxfExists { get; set; }
        /// <summary>Ruta virtual del STL para el visor (gemelo del .dwg o el adjunto .stl del slot).</summary>
        public string StlPreviewVirtualPath { get; set; }
        /// <summary>True si el STL del visor existe en disco.</summary>
        public bool StlPreviewExists { get; set; }
    }
}
