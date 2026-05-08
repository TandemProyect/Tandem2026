using System;
using System.Collections.Generic;

namespace Desing.Models
{
    /// <summary>
    /// ColorIndex ZWCAD para cada tipo de punto de esquina L
    /// </summary>
     public enum TipoPunto
    {
        PtEInterior    = 5,  // Blue
        PtEExteriro    = 1,  // Red
        PtEInt300H     = 3,  // Green
        PtEInt300V     = 2,  // Yellow
        PtEExt300H     = 7,  // White
        PtEExt300V     = 4,  // Cyan
        PtEExtPanelH   = 6,  // Magenta
        PtEExtPanelV   = 9   // Gray
    }

    /// <summary>
    /// DTO para recibir información de una línea desde ZWCAD
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

        /// <summary>
        /// US-688 T1 — forma geométrica del marcador: "Circulo" (default) o "Cuadrado".
        /// El cliente (ZWCAD, three.js, etc.) elige la primitiva a dibujar.
        /// </summary>
        public string Forma { get; set; } = "Circulo";

        /// <summary>
        /// US-688 T1 — tamaño del marcador en mm (radio para Circulo, semi-lado para Cuadrado).
        /// Si es 0, el cliente usa su default.
        /// </summary>
        public double Tamano { get; set; } = 0;
    }

    /// <summary>
    /// DTO para recibir colección de líneas y polilíneas desde ZWCAD
    /// </summary>
    public class SeleccionLineasDTO
    {
        public List<LineaDTO> Lineas { get; set; }
        public int TotalSeleccionados { get; set; }
        public int TotalLineas { get; set; }
        public int TotalPolilineas { get; set; }
        public DateTime FechaSeleccion { get; set; }
        public string Usuario { get; set; }

        /// <summary>
        /// US-697 — Altura del muro en mm para extruir en capa ModelDesing.
        /// Default 2700 (2.70 m). El cliente puede sobrescribirlo desde el formulario.
        /// </summary>
        public double AlturaMuroMm { get; set; } = 2700;
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
        /// US-697 — Total de muros rectos detectados (B/C entre esquinas, A/D con extremo libre, E/F aislados).
        /// </summary>
        public int TotalMurosRectos { get; set; }

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

    /// <summary>
    /// Respuesta genérica de la API
    /// </summary>
    /// <typeparam name="T">Tipo de datos en la respuesta</typeparam>
    public class ApiResponse<T>
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public T Datos { get; set; }
    }

    /// <summary>
    /// Solicitud de autorización del plugin por equipo.
    /// </summary>
    public class PluginAuthRequestDTO
    {
        public string DeviceId { get; set; }
        public string MachineName { get; set; }
        public string UsuarioWindows { get; set; }
        public string AspNetUserId { get; set; }
        public string PluginVersion { get; set; }
    }

    /// <summary>
    /// Resultado de autorización del plugin.
    /// </summary>
    public class PluginAuthResultDTO
    {
        public bool Permitido { get; set; }
        public string Estado { get; set; }
        public string Motivo { get; set; }
        public string DeviceId { get; set; }
    }
}
