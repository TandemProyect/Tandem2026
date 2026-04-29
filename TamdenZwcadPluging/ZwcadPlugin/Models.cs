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

    public class DisenoResumenDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaModificacion { get; set; }
        public string Usuario { get; set; }
    }

    /// <summary>
    /// DTO para enviar información de una línea desde ZWCAD al servidor MVC
    /// </summary>
    public class LineaDTO
    {
        public string Tipo { get; set; } // "Line" o "Polyline"
        public double InicioX { get; set; }
        public double InicioY { get; set; }
        public double InicioZ { get; set; }
        public double FinX { get; set; }
        public double FinY { get; set; }
        public double FinZ { get; set; }
        public string Layer { get; set; }
        public string Color { get; set; }
        public double Longitud { get; set; }
        public List<PuntoDTO> Vertices { get; set; } // Para polilíneas
    }

    /// <summary>
    /// DTO para representar un punto 3D
    /// </summary>
    public class PuntoDTO
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        /// <summary>
        /// Tipo de punto: "Interior" (vértice de esquina) o "Exterior" (vértice opuesto)
        /// </summary>
        public string TipoPunto { get; set; }

        /// <summary>
        /// ColorIndex ZWCAD calculado por el servidor según TipoPunto
        /// </summary>
        public int ColorIndex { get; set; }
    }

    /// <summary>
    /// DTO para enviar colección de líneas y polilíneas al servidor MVC
    /// </summary>
    public class SeleccionLineasDTO
    {
        public List<LineaDTO> Lineas { get; set; }
        public int TotalSeleccionados { get; set; }
        public int TotalLineas { get; set; }
        public int TotalPolilineas { get; set; }
        public DateTime FechaSeleccion { get; set; }
        public string Usuario { get; set; }
    }

    /// <summary>
    /// DTO para representar una esquina tipo L detectada
    /// </summary>
    public class EsquinaLDTO
    {
        /// <summary>
        /// Punto del vértice donde se forma la esquina L
        /// </summary>
        public PuntoDTO Vertice { get; set; }

        /// <summary>
        /// Índice de la primera línea que forma la esquina
        /// </summary>
        public int IndiceLinea1 { get; set; }

        /// <summary>
        /// Índice de la segunda línea que forma la esquina
        /// </summary>
        public int IndiceLinea2 { get; set; }

        /// <summary>
        /// Ángulo calculado entre las dos líneas (debería ser cercano a 90°)
        /// </summary>
        public double Angulo { get; set; }

        /// <summary>
        /// Orientación de la esquina (0-7, según las 8 orientaciones posibles)
        /// </summary>
        public int Orientacion { get; set; }
    }

    /// <summary>
    /// Polilínea a dibujar en ZWCAD (ObjetoDB2d)
    /// </summary>
    public class PolilineaDTO
    {
        public List<PuntoDTO> Vertices { get; set; }
        public bool Cerrada { get; set; }
        public string Capa { get; set; }
        public int ColorIndex { get; set; }
        public double AlturaExtrusion { get; set; }  // 0 = sin extrusión
    }

    /// <summary>
    /// Respuesta del servidor con esquinas L detectadas
    /// </summary>
    public class DeteccionEsquinasLDTO
    {
        /// <summary>
        /// Lista de esquinas L detectadas
        /// </summary>
        public List<EsquinaLDTO> Esquinas { get; set; }

        /// <summary>
        /// Total de esquinas detectadas
        /// </summary>
        public int TotalEsquinasDetectadas { get; set; }

        /// <summary>
        /// Puntos a dibujar en ZWCAD para visualización
        /// </summary>
        public List<PuntoDTO> PuntosADibujar { get; set; }

        /// <summary>
        /// Polilíneas a dibujar en capa ObjetoDB2d
        /// </summary>
        public List<PolilineaDTO> PolilineasADibujar { get; set; }

        /// <summary>
        /// Mensaje descriptivo del resultado
        /// </summary>
        public string Mensaje { get; set; }
    }
}