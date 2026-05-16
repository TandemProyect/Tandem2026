using System.ComponentModel.DataAnnotations.Schema;

namespace DAL
{
    /// <summary>
    /// Propiedad MVC (no EF) para conservar la ruta del icono en formularios.
    /// Cuando ejecutéis "Update Model from Database", si Path_Ico queda ya mapeada
    /// en la entidad generada, podéis usar esa propiedad mapeada y retirar esta.
    /// </summary>
    public partial class TSql_Extension
    {
        [NotMapped]
        public string IcoPath { get; set; }
    }
}
