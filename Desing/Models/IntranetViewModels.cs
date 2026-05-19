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
        public string TextTaxId { get; set; }
        public string TextEmail { get; set; }
        public string TextPhone { get; set; }
        public string Path_Ico { get; set; }
        public string Path_Logo { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
    }

    public class JobsideListItem
    {
        public long IdObject { get; set; }
        public string AddNJobside { get; set; }
        public string AddNJobsideClient { get; set; }
        public string TextLabel { get; set; }
        public string ClientName { get; set; }
        public string Loc_Formatted_Address { get; set; }
        public bool Is_Active { get; set; }
        public bool Is_Delete { get; set; }
    }

    /// <summary>
    /// Proyección para el listado DataTables de documentos ligados a una obra
    /// (<see cref="DAL.TSql_Document.LinkJobside"/>).
    /// </summary>
    public class JobsideDocumentListItem
    {
        public long IdObject { get; set; }
        public string AddDescription { get; set; }
        public string AddPath { get; set; }
        public string DocumentTypeName { get; set; }
        public DateTime AddDateMade { get; set; }
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

    /// <summary>Panel lateral de sedes (<see cref="DAL.TSql_Branch"/>) al crear/editar empresa.</summary>
    public class CompanyBranchesPanelModel
    {
        public CompanyBranchesPanelModel()
        {
            BranchRows = new List<CompanyBranchListRow>();
        }

        public long? CompanyId { get; set; }
        public IList<CompanyBranchListRow> BranchRows { get; set; }
    }

    /// <summary>Fila de sede para DataTables en ficha empresa (<see cref="TSql_CompanyController.ListCompanyBranches"/>).</summary>
    public class CompanyBranchDataTablesItem
    {
        public long SysObjectID { get; set; }
        public string AttLabel { get; set; }
        public string AttDescription { get; set; }
        public string AddLetter { get; set; }
        public string Attcolor { get; set; }
    }

    /// <summary>Fila de sede en el panel lateral de empresa.</summary>
    public class CompanyBranchListRow
    {
        public long SysObjectID { get; set; }
        public string AttLabel { get; set; }
        public string AttDescription { get; set; }
        public long LinBusiness { get; set; }
        public string BusinessName { get; set; }
        public string AddLetter { get; set; }
        /// <summary>Color de acento (#RRGGBB) para el badge de <see cref="AddLetter"/>.</summary>
        public string Attcolor { get; set; }

        public string Loc_Place_Id { get; set; }
        public string Loc_Formatted_Address { get; set; }
        public decimal? Loc_Lat { get; set; }
        public decimal? Loc_Lng { get; set; }
        public string Loc_Street_Number { get; set; }
        public string Loc_Route { get; set; }
        public string Loc_Subpremise { get; set; }
        public string Loc_Locality { get; set; }
        public string Loc_Admin_Area_1 { get; set; }
        public string Loc_Admin_Area_2 { get; set; }
        public string Loc_Postal_Code { get; set; }
        public string Loc_Country_Code { get; set; }
        public string Loc_Country_Name { get; set; }
        public string Loc_Address_Components_Json { get; set; }

        /// <summary>JSON compacto (atributo HTML) para rellenar el modal de sede en panel empresa.</summary>
        public string LocJsonDom { get; set; }
    }

    /// <summary>Opción de idioma UI en crear/editar empresa (bandera desde país vinculado al idioma).</summary>
    public class CompanyUiLanguageOption
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextCode { get; set; }
        /// <summary>Ruta virtual tipo ~/Content/... o null.</summary>
        public string FlagVirtualPath { get; set; }
    }

    /// <summary>Valor inicial país en crear/editar idioma (sin depender del filtro solo activos del listado).</summary>
    public class LanguageCountryFormBootstrap
    {
        public long? Id { get; set; }
        public string Label { get; set; }
        public string Iso2 { get; set; }
        public string Iso3 { get; set; }
        /// <summary>Ruta aplicación resuelta vía Url.Content (~ → /app/…) o null.</summary>
        public string FlagUrl { get; set; }
    }
}
