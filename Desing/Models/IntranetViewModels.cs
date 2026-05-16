using System;
using System.Collections.Generic;
using DAL;

namespace Desing.Models
{
    public class IntranetAuditUserLink
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public long? EmployeeId { get; set; }
    }

    public class IntranetAuditDisplayModel
    {
        public IntranetAuditUserLink LinkMadeBy { get; set; }
        public IntranetAuditUserLink LinModifiedBy { get; set; }
        public IntranetAuditUserLink AddChangeBy { get; set; }
        public DateTime AddDateMade { get; set; }
        public DateTime AddLastDateChange { get; set; }
        public long Ntimeschanged { get; set; }
    }

    public class ClientV2ListItem
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextCode { get; set; }
        public string Path_Ico { get; set; }
        public string Path_Logo { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
    }

    public class JobsideListItem
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string ClientName { get; set; }
        public string Loc_Formatted_Address { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
    }

    public class DocumentTypeListItem
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextCode { get; set; }
        public string TextDescription { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
    }

    /// <summary>
    /// ViewModel para el formulario Create/Edit de TSql_DocumentType.
    /// Encapsula la entidad EF junto con la lista de extensiones disponibles
    /// (catálogo completo) y las extensiones marcadas (puente N:N
    /// TSql_DocumentTypeExtension).
    /// </summary>
    public class DocumentTypeFormViewModel
    {
        public DocumentTypeFormViewModel()
        {
            ExtensionesDisponibles = new List<TSql_Extension>();
            IdExtensionesSeleccionadas = new List<long>();
            ExtensionPathIcoById = new Dictionary<long, string>();
        }

        public TSql_DocumentType DocumentType { get; set; }

        /// <summary>Catálogo completo de extensiones activas (Is_Active=1, Is_Delete=0).</summary>
        public IList<TSql_Extension> ExtensionesDisponibles { get; set; }

        /// <summary>IdObject de las extensiones marcadas en el formulario.</summary>
        public IList<long> IdExtensionesSeleccionadas { get; set; }

        /// <summary>Rutas (~ o /…) de icono por IdObject de extensión; para vistas de selección.</summary>
        public Dictionary<long, string> ExtensionPathIcoById { get; set; }
    }

    /// <summary>Entrada hija de un grupo del sidebar (_SidebarMenuItemGroup).</summary>
    public class SidebarMenuChildItem
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Text { get; set; }
        /// <summary>Controladores que marcan el hijo como activo (coma separada). Por defecto usa Controller.</summary>
        public string ActiveControllers { get; set; }
    }

    /// <summary>Grupo expandible del sidebar (clases propias js-menu-group / js-menu-group-toggle).</summary>
    public class SidebarMenuGroupModel
    {
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string IconClass { get; set; }
        public bool IsOpen { get; set; }
        public IList<SidebarMenuChildItem> Children { get; set; }
    }
}
