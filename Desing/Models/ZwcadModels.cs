using System;
using System.Collections.Generic;

namespace Desing.Models
{
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
}
