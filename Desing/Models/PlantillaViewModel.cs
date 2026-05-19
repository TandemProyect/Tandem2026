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

        [Required(
            ErrorMessageResourceType = typeof(Desing.Resources.Plantilla),
            ErrorMessageResourceName = "Val_NameRequired")]
        [StringLength(150)]
        public string AttName { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(Desing.Resources.Plantilla),
            ErrorMessageResourceName = "Val_BrandTextRequired")]
        [StringLength(120)]
        public string AttBrandText { get; set; } = "T Desing.net";

        [StringLength(20)]
        [RegularExpression(@"^$|^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
            ErrorMessageResourceType = typeof(Desing.Resources.Plantilla),
            ErrorMessageResourceName = "Val_BrandTextColorHexFormat")]
        public string AttBrandTextColor { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(Desing.Resources.Plantilla),
            ErrorMessageResourceName = "Val_AccentColorRequired")]
        [StringLength(20)]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
            ErrorMessageResourceType = typeof(Desing.Resources.Plantilla),
            ErrorMessageResourceName = "Val_AccentColorHexFormat")]
        public string AttBrandAccentColor { get; set; } = "#f29100";

        [Required(
            ErrorMessageResourceType = typeof(Desing.Resources.Plantilla),
            ErrorMessageResourceName = "Val_MainColorRequired")]
        [StringLength(20)]
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
            ErrorMessageResourceType = typeof(Desing.Resources.Plantilla),
            ErrorMessageResourceName = "Val_MainColorHexFormat")]
        public string AttColor { get; set; } = "#349d7d";

        [Required(
            ErrorMessageResourceType = typeof(Desing.Resources.Plantilla),
            ErrorMessageResourceName = "Val_LogoPathRequired")]
        [StringLength(500)]
        public string AttLogo { get; set; } = "/Content/images/Login/at.png";

        [StringLength(500)]
        public string AttFavicon { get; set; } = "/assets/client/images/Default/Ico/at.ico";

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
        public string AttBrandText { get; set; }
        public string AttColor { get; set; }
        public string AttLogo { get; set; }
        public bool AttIsDefault { get; set; }
        public DateTime AttCreated { get; set; }
        public string AttCreatedFormatted { get; set; }
    }
}
