using System;
using System.Collections.Generic;

namespace ZwcadPlugin.Models
{
    public class BloqueDTO
    {
        public string Nombre { get; set; }
        public double PuntoInsertX { get; set; }
        public double PuntoInsertY { get; set; }
        public double PuntoInsertZ { get; set; }
        public double Escala { get; set; }
        public double Rotacion { get; set; }
        public Dictionary<string, string> Atributos { get; set; }
        public string RutaArchivo { get; set; }
    }

    public class DisenoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string Usuario { get; set; }
        public List<EntidadDTO> Entidades { get; set; }
        public List<BloqueDTO> Bloques { get; set; }
        public List<LayerDTO> Layers { get; set; }
    }

    public class EntidadDTO
    {
        public string Tipo { get; set; }
        public string Layer { get; set; }
        public string Color { get; set; }
        public Dictionary<string, object> Propiedades { get; set; }
    }

    public class LayerDTO
    {
        public string Nombre { get; set; }
        public string Color { get; set; }
        public bool Visible { get; set; }
        public bool Bloqueado { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public T Datos { get; set; }
    }

    public class XyzMmDto
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public double? Z { get; set; }
    }

    public class WallLineDto
    {
        public long? Id { get; set; }
        public XyzMmDto P1Mm { get; set; }
        public XyzMmDto P2Mm { get; set; }
        public string WallRole { get; set; }
        public long? WallGroupId { get; set; }
        public string TextSystem { get; set; }

        [Newtonsoft.Json.JsonProperty("_Datalong")]
        public double? DataLong { get; set; }

        [Newtonsoft.Json.JsonProperty("_DataWith")]
        public double? DataWith { get; set; }

        [Newtonsoft.Json.JsonProperty("_DataHeight")]
        public double? DataHeight { get; set; }

        public long? WallDbId { get; set; }
        public bool? IsSpecial { get; set; }
    }

    public class WallArticleDto
    {
        public long IdObject { get; set; }
        public long WallDbId { get; set; }
        public long? MasterArticleId { get; set; }
        public string TextCode { get; set; }
        public string TextView { get; set; }
        public long NumberSequence { get; set; }
        public XyzMmDto InsertMm { get; set; }
        public double RotationX { get; set; }
        public double RotationY { get; set; }
        public double RotationZ { get; set; }
        public string TextHandleCad { get; set; }
    }

    public class PluginSaveWallArticleRequest
    {
        public long DesignId { get; set; }
        public string DeviceId { get; set; }
        public long? WallDbId { get; set; }
        public long? MasterArticleId { get; set; }
        public string CodeName { get; set; }
        public string View { get; set; }
        public XyzMmDto P1Mm { get; set; }
        public XyzMmDto P2Mm { get; set; }
        public double? DataWith { get; set; }
        public double? DataLong { get; set; }
        public double? DataHeight { get; set; }
        public string TextSystem { get; set; }
        public XyzMmDto InsertMm { get; set; }
        public double RotationX { get; set; }
        public double RotationY { get; set; }
        public double RotationZ { get; set; }
        public string HandleCad { get; set; }
    }

    public class PluginSaveWallArticleResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public long WallId { get; set; }
        public long ArticleId { get; set; }
        public long Sequence { get; set; }
        public long DesignId { get; set; }
    }

    public class PluginSaveWallsRequest
    {
        public long DesignId { get; set; }
        public string DeviceId { get; set; }
        public List<WallLineDto> Lines { get; set; }
        public List<PluginSaveWallArticleRequest> Articles { get; set; }
    }

    public class PluginSaveWallsResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public int Count { get; set; }
        public int ArticleCount { get; set; }
        public long DesignId { get; set; }
    }

    public class WallSnapshotDto
    {
        public List<WallLineDto> Lines { get; set; }
        public List<WallArticleDto> Articles { get; set; }
    }

    public class PluginAuthRequestDTO
    {
        public string DeviceId { get; set; }
        public string MachineName { get; set; }
        public string UsuarioWindows { get; set; }
        public string AspNetUserId { get; set; }
        public string PluginVersion { get; set; }
    }

    public class PluginAuthResultDTO
    {
        public bool Permitido { get; set; }
        public string Estado { get; set; }
        public string Motivo { get; set; }
        public string DeviceId { get; set; }
    }

    public class DisenoResumenDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string Usuario { get; set; }
    }

    public class LineaDTO
    {
        public string Tipo { get; set; }
        public double InicioX { get; set; }
        public double InicioY { get; set; }
        public double InicioZ { get; set; }
        public double FinX { get; set; }
        public double FinY { get; set; }
        public double FinZ { get; set; }
        public string Layer { get; set; }
        public string Color { get; set; }
        public double Longitud { get; set; }
        public List<PuntoDTO> Vertices { get; set; }
    }

    public class PuntoDTO
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public string TipoPunto { get; set; }
        public int ColorIndex { get; set; }
        public string Forma { get; set; } = "Circulo";
        public double Tamano { get; set; } = 0;
    }

    public class SeleccionLineasDTO
    {
        public List<LineaDTO> Lineas { get; set; }
        public int TotalSeleccionados { get; set; }
        public int TotalLineas { get; set; }
        public int TotalPolilineas { get; set; }
        public DateTime FechaSeleccion { get; set; }
        public string Usuario { get; set; }
        public double AlturaMuroMm { get; set; } = 2700;
    }

    public class EsquinaLDTO
    {
        public PuntoDTO Vertice { get; set; }
        public int IndiceLinea1 { get; set; }
        public int IndiceLinea2 { get; set; }
        public double Angulo { get; set; }
        public int Orientacion { get; set; }
    }

    public class PolilineaDTO
    {
        public List<PuntoDTO> Vertices { get; set; }
        public bool Cerrada { get; set; }
        public string Capa { get; set; }
        public int ColorIndex { get; set; }
        public double AlturaExtrusion { get; set; }
    }

    public class DeteccionEsquinasLDTO
    {
        public List<EsquinaLDTO> Esquinas { get; set; }
        public int TotalEsquinasDetectadas { get; set; }
        public int TotalMurosRectos { get; set; }
        public List<PuntoDTO> PuntosADibujar { get; set; }
        public List<PolilineaDTO> PolilineasADibujar { get; set; }
        public string Mensaje { get; set; }
    }

    public class Atk60FormworkResponse
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public int WallsCount { get; set; }
        public int ElementsForThreeJsCount { get; set; }
        public Atk60FormworkPaint ElementsForThreeJs { get; set; }
    }

    public class Atk60FormworkPaint
    {
        public List<Atk60FormworkElement> Elements { get; set; }
    }

    public class Atk60FormworkElement
    {
        public string ElementCode { get; set; }
        public string ImportPath { get; set; }
        public string Orientation { get; set; }
        public string IdWall { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double RotY { get; set; }
        public double PieceWidthMm { get; set; }
        public bool IsMirrored { get; set; }
    }
}

