namespace Desing.Models

{

    /// <summary>

    /// Página del visor STL (<see cref="Controllers.Desing_2Controller"/>), pensada para abrirse al editar un diseño ligado a una oferta.

    /// </summary>

    public class Desing2ViewerPageModel

    {

        /// <summary>Hex #rgb / #rrggbb para tinte principal del STL (atributo <c>data-ma-text-color1</c>).</summary>

        public string TextColor1Hex { get; set; }



        /// <summary>Hex para segunda malla opcional <c>*2.stl</c> (<c>data-ma-text-color2</c>).</summary>

        public string TextColor2Hex { get; set; }



        /// <summary>URL ya pasada por <c>Url.Content</c>; solo rutas de aplicación.</summary>

        public string InitialStlUrl { get; set; }



        /// <summary>Texto para estado «Viendo: …» tras cargar.</summary>

        public string InitialStlLabel { get; set; }



        /// <summary>Si es true y hay <see cref="InitialStlUrl"/>, el script dispara la carga inicial (p. ej. <c>autoLoad=1</c> desde la oferta).</summary>

        public bool AutoLoadInitialStl { get; set; }



        /// <summary>Texto inicial del pie del visor (#master-article-details-stl-viewer-status).</summary>

        public string InitialStatusFooter { get; set; }



        public long? OfferId { get; set; }



        public long? DesignId { get; set; }



        /// <summary>Logo de la plantilla activa (misma resolución que el menú lateral).</summary>

        public string BrandLogoUrl { get; set; }



        /// <summary>Línea única: código obra — número oferta — nombre oferta — diseño (# / etiqueta).</summary>

        public string ContextSubtitleLine { get; set; }



        /// <summary>

        /// Unidad en que vienen los vértices del STL (<c>mm</c>, <c>cm</c>, <c>m</c>). El visor escala el modelo a metros.

        /// </summary>

        public string StlSourceUnits { get; set; }

    }

}

