using System;
using System.Collections.Generic;

namespace AutocadPlugin.Models
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
    }

    public class WallSnapshotDto
    {
        public List<WallLineDto> Lines { get; set; }
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
}
