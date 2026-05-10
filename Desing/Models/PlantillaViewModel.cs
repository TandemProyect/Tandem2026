using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Desing.Models
{
    /// <summary>
    /// ViewModel para crear y editar plantillas de estilo de usuario
    /// (color principal y logo del layout).
    /// </summary>
    public class PlantillaViewModel
    {
        public long SysObjectID { get; set; }

        [Required(ErrorMessage = "El nombre de la plantilla es obligatorio")]
        [StringLength(150)]
        [DisplayName("Nombre de la plantilla")]
        public string AttName { get; set; }

        [Required(ErrorMessage = "El color es obligatorio")]
        [StringLength(20)]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
            ErrorMessage = "El color debe estar en formato HEX, ej: #349d7d")]
        [DisplayName("Color principal")]
        public string AttColor { get; set; } = "#349d7d";

        [Required(ErrorMessage = "La ruta del logo es obligatoria")]
        [StringLength(500)]
        [DisplayName("Logo (ruta)")]
        public string AttLogo { get; set; } = "/Content/images/Login/at.png";

        [DisplayName("Plantilla por defecto")]
        public bool AttIsDefault { get; set; }

        public bool IsEdit { get; set; }
    }

    /// <summary>
    /// Fila listada en el grid de plantillas.
    /// </summary>
    public class PlantillaListItem
    {
        public long SysObjectID { get; set; }
        public string AttName { get; set; }
        public string AttColor { get; set; }
        public string AttLogo { get; set; }
        public bool AttIsDefault { get; set; }
        public DateTime AttCreated { get; set; }
        public string AttCreatedFormatted { get; set; }
    }
}
