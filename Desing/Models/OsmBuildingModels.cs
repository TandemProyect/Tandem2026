using System.Collections.Generic;

namespace Desing.Models
{
    /// <summary>
    /// Resultado de geocodificación (Nominatim / OpenStreetMap).
    /// </summary>
    public class OsmGeocodeResultDTO
    {
        public string DisplayName { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double? South { get; set; }
        public double? West { get; set; }
        public double? North { get; set; }
        public double? East { get; set; }
    }

    /// <summary>
    /// Vértice geográfico WGS84.
    /// </summary>
    public class OsmLatLngDTO
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    /// <summary>
    /// Huella de edificio OSM (way con tag building=*).
    /// HeightM/Levels alimentan la extrusión 3D del modal (cuando OSM los trae).
    /// </summary>
    public class OsmBuildingFootprintDTO
    {
        public long OsmId { get; set; }
        public string TextLabel { get; set; }
        public string BuildingType { get; set; }
        public List<OsmLatLngDTO> Ring { get; set; }
        /// <summary>Altura en metros si OSM aporta height / building:levels.</summary>
        public double? HeightM { get; set; }
        /// <summary>Número de plantas (building:levels), si existe.</summary>
        public int? Levels { get; set; }
    }

    /// <summary>
    /// Solicitud para listar edificios en un bounding box.
    /// </summary>
    public class OsmBuildingsBboxRequestDTO
    {
        public double South { get; set; }
        public double West { get; set; }
        public double North { get; set; }
        public double East { get; set; }
    }

    /// <summary>
    /// Solicitud para convertir una huella seleccionada a líneas de planta (mm).
    /// </summary>
    public class OsmBuildingImportRequestDTO
    {
        public long? OsmId { get; set; }
        public string TextLabel { get; set; }
        public List<OsmLatLngDTO> Ring { get; set; }
        public double? HeightM { get; set; }
    }
}
