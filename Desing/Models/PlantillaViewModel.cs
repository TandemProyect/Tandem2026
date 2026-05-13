using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Desing.Models
{
    /// <summary>
    /// ViewModel para crear y editar plantillas de estilo de usuario
    /// (color principal, logo, marca y colores de texto).
    /// </summary>
    public class PlantillaViewModel
    {
        public long SysObjectID { get; set; }

        [Required(ErrorMessage = "El nombre de la plantilla es obligatorio")]
        [StringLength(150)]
        [DisplayName("Nombre de la plantilla")]
        public string AttName { get; set; }

        [Required(ErrorMessage = "El texto de marca es obligatorio")]
        [StringLength(120)]
        [DisplayName("Nombre mostrado (marca)")]
        public string AttBrandText { get; set; } = "T Desing.net";

        [StringLength(20)]
        [RegularExpression(@"^$|^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
            ErrorMessage = "Color de texto: vacío (usa color primario) o HEX, ej: #4c4c4c")]
        [DisplayName("Color texto marca (resto)")]
        public string AttBrandTextColor { get; set; }

        [Required(ErrorMessage = "El color de acento es obligatorio")]
        [StringLength(20)]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
            ErrorMessage = "El color de acento debe estar en formato HEX, ej: #f29100")]
        [DisplayName("Color primera letra (acento)")]
        public string AttBrandAccentColor { get; set; } = "#f29100";

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

        [StringLength(500)]
        [DisplayName("Favicon (ruta)")]
        public string AttFavicon { get; set; } = "/assets/client/images/Default/Ico/at.ico";

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
