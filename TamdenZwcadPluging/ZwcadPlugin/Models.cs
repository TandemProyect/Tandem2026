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
}