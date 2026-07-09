using System.Collections.Generic;

namespace Desing.Models.TandemXr
{
    /// <summary>
    /// Contrato JSON entre Desing (servidor) y TandemXR-Unity (Quest + tablet).
    /// Misma idea que ListRenderElement / visor web, en formato API.
    /// </summary>
    public class TandemXrDesignManifestDto
    {
        public long DesignId { get; set; }
        public long? OfferId { get; set; }
        public string TextLabel { get; set; }
        /// <summary>Origen absoluto para resolver rutas STL relativas (ej. https://host/).</summary>
        public string ServerBaseUrl { get; set; }
        public string ThumbnailStlUrl { get; set; }
        public List<TandemXrInstanceDto> Instances { get; set; } = new List<TandemXrInstanceDto>();
        public string Message { get; set; }
    }

    public class TandemXrInstanceDto
    {
        public string IdElement { get; set; }
        public string CodeName { get; set; }
        /// <summary>Ruta virtual o URL del STL de la pieza.</summary>
        public string StlUrl { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double RotateX { get; set; }
        public double RotateY { get; set; }
        public double RotateZ { get; set; }
        public bool Mirror { get; set; }
    }
}
