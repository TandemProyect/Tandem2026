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
    /// Huella de edificio (OSM way o Catastro INSPIRE BU).
    /// HeightM/Levels alimentan la extrusión 3D del modal.
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
        /// <summary>Referencia catastral (14 chars parcela) si proviene de Catastro o se enriqueció.</summary>
        public string CadastralRef { get; set; }
        /// <summary>Dirección catastral (calle, número, municipio) si está disponible.</summary>
        public string CadastralAddress { get; set; }
        /// <summary>Origen de la huella: Osm | Catastro.</summary>
        public string Source { get; set; }
    }

    /// <summary>
    /// Consulta de referencia catastral por coordenadas WGS84.
    /// </summary>
    public class CatastroRcLookupRequestDTO
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    /// <summary>
    /// Resultado de Consulta_RCCOOR (Catastro).
    /// </summary>
    public class CatastroRcLookupResultDTO
    {
        public string CadastralRef { get; set; }
        public string Address { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
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

    /// <summary>
    /// Solicitud de localización de IFC para un edificio (Catastro / OSM).
    /// Hoy: ejemplo fijo. Futuro: catálogo real por RC / coordenadas.
    /// </summary>
    public class IfcLocateRequestDTO
    {
        public string CadastralRef { get; set; }
        public string TextLabel { get; set; }
        public string Address { get; set; }
        public long? OsmId { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
    }

    /// <summary>
    /// IFC localizado (ruta descargable). No importa geometría aquí:
    /// la app IFC externa consumirá FileUrl / FileName.
    /// </summary>
    public class IfcLocateResultDTO
    {
        public bool Found { get; set; }
        public bool IsSample { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public string RelativePath { get; set; }
        public string CadastralRef { get; set; }
        public string BuildingLabel { get; set; }
        public string Message { get; set; }
        public long? FileSizeBytes { get; set; }
    }
}
