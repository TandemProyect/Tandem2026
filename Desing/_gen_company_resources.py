"""One-off helper: generates Company.resx, Company.en.resx, Company.Designer.cs."""
from pathlib import Path


def _xml_esc(s):
    return (
        s.replace("&", "&amp;")
        .replace("<", "&lt;")
        .replace(">", "&gt;")
        .replace('"', "&quot;")
    )


def wrap_res_body(entries_es, entries_en):
    header = '''<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
'''

    tail = '</root>'
    chunks_es = []
    for name in sorted(entries_es.keys()):
        val_es = _xml_esc(entries_es[name])
        chunks_es.append(f'  <data name="{name}" xml:space="preserve">\n    <value>{val_es}</value>\n  </data>\n')

    chunks_en = []
    for name in sorted(entries_es.keys()):
        val_en = _xml_esc(entries_en.get(name, entries_es[name]))
        chunks_en.append(f'  <data name="{name}" xml:space="preserve">\n    <value>{val_en}</value>\n  </data>\n')

    return header + "".join(chunks_es) + tail, header + "".join(chunks_en) + tail


def main():
    es = dict(
Common_Home="Inicio",
Index_Breadcrumb="Empresas",
Index_CreateCompany="Crear nueva empresa",

Col_Name="Nombre",
Col_AddLeter="Signadura",
Col_Logo="Logo",
Col_Description="Descripción",
Col_Address="Dirección",
Col_Address2="Dirección 2",
Col_PostalCode="Código postal",
Col_City="Ciudad",
Col_Country="País",
Col_State="Estado",

Dt_Rows_All="Todas",
Dt_Rows_N="filas",
Dt_MenuAria="Opciones del listado",
Dt_Section_Records="Registros",
Dt_Section_Export="Exportar",
Dt_Section_ColumnsVisible="Columnas visibles",

State_Active="Activa",
State_Inactive="Desactivada",

Js_ConfirmDeleteCompany="¿Eliminar esta empresa?",
Js_ConfirmToggleCompany="¿Desactivar o activar esta empresa?",

ToastTitle_CreateCompany="Crear empresa",
ToastMessage_CompanySaved="Empresa creada correctamente.",
ToastTitle_EditCompany="Editar empresa",
ToastMessage_CompanyUpdated="Empresa actualizada correctamente.",

Val_CompanyNameRequired="El nombre de la empresa es obligatorio.",
Val_DuplicateNameCreate="Ya existe una empresa con ese nombre.",
Val_DuplicateNameEdit="Ya existe otra empresa con ese nombre.",
Val_UiLanguageRequired="Seleccione el idioma de trabajo para todos los empleados de esta empresa.",

List_LinkOpenTooltip="Abrir empresa",
List_LinkEditTooltip="Editar empresa",
List_LinkDeleteTooltip="Eliminar empresa",
List_LinkToggleTooltip="Activar o desactivar empresa",

Err_CompanyNotFound="Empresa no encontrada.",
Err_CannotDeleteRelated="No se puede eliminar. La empresa tiene datos relacionados.",
Msg_CompanyDeleted="Empresa eliminada correctamente.",
Msg_CompanyPaused="Empresa desactivada.",
Msg_CompanyResumed="Empresa activada.",

Branch_Err_InvalidCompany="Empresa no válida.",
Branch_Err_NameRequired="El nombre de la sede es obligatorio.",
Branch_Err_NoBusinessDetailed="No hay negocio para esta empresa en base de datos. Cree al menos un TSql_Business con LinCompany igual a esta empresa antes de añadir sedes.",
Branch_Msg_Created="Sede creada correctamente.",

Branch_Err_InvalidData="Datos no válidos.",
Branch_Err_NotFound="Sede no encontrada.",
Branch_Msg_Updated="Sede actualizada correctamente.",
Branch_Msg_Deleted="Sede eliminada correctamente.",

Plantilla_InheritGlobalDefault="— Heredar plantilla global por defecto —",

Google_NotConfiguredStrong="Google Maps no configurado.",
Google_NotConfiguredBody="Añada GoogleMaps:ApiKey en Web.config para autocompletado y mapa. Puede rellenar la dirección manualmente.",

Lbl_NameRequired="Nombre *",
Lbl_AddLeter="Signadura",
Lbl_Country="País",
Dd_SelectPlaceholder="— Seleccionar —",

Lbl_VisualTemplate="Plantilla visual (colores, marca)",
Help_VisualTemplateEmployees="Todos los empleados de esta empresa verán esta plantilla. Vacío = plantilla global por defecto.",

Lbl_WorkUiLanguageRequired="Idioma de trabajo (empleados) *",

Lbl_Description="Descripción",

Lbl_AddressGooglePlaces="Dirección (Google Places)",
Lbl_LegacyBlockHint="Campos de texto legacy (opcional)",
Lbl_AddressLegacy="Dirección",
Lbl_AddressLegacy2="Dirección 2",
Lbl_PostalLegacy="Código postal",
Lbl_CityLegacy="Ciudad",
Lbl_LogoPath="Logo (texto/ruta)",

Btn_Edit="Editar",
Btn_Cancel="Cancelar",
Btn_SaveCompany="Guardar empresa",
Btn_SaveChanges="Guardar cambios",

Page_CreateTitle="Crear empresa",
Page_EditTitle="Editar empresa",
Details_Field_Name="Nombre",
Details_Field_AddLeter="Signadura",
Details_Field_Description="Descripción",
Details_Field_Address="Dirección",
Details_Field_Address2="Dirección 2",
Details_Field_Postal="Código postal",
Details_Field_City="Ciudad",
Details_Field_Country="País",

Details_Field_WorkLanguage="Idioma de trabajo (empleados)",
Details_NoValue="-",
Details_WorkLanguageHint="La bandera corresponde al país asociado al idioma en Configuración → Idiomas. Los empleados de esta empresa usan este idioma en la aplicación.",

BranchPanel_Title="Sedes (Branch)",
BranchPanel_New="Nueva sede",
BranchPanel_CreateSectionTitle="Crear sede",

Branch_Lbl_NameRequiredModal="Nombre *",
Branch_Lbl_Description="Descripción",
Branch_Lbl_LetterShort="Letra (máx. 2)",
Branch_Btn_SaveInline="Guardar sede",
Branch_Msg_SaveToManageBranches="Guarde la empresa para poder crear y gestionar sedes.",
Branch_Msg_NoBusinessBlock="No hay ningún negocio (TSql_Business) para esta empresa en base de datos. Debe existir al menos uno para poder crear sedes; el sistema asignará automáticamente ese negocio a cada nueva sede.",
Branch_ModalTitle_Edit="Editar sede",
Aria_CloseModal="Cerrar",
Branch_ModalBtn_CancelModal="Cancelar",
Branch_NoRows="No hay sedes registradas.",
Branch_RowTooltip_Edit="Editar fila",
Branch_RowTooltip_Delete="Eliminar fila",

LanguageSelect_FlagHint="La bandera viene del país asociado al idioma en Configuración → Idiomas (campo país). Ej.: español → España.",

JsBranch_RefreshFail="No se pudo actualizar la lista de sedes.",
JsBranch_NewNeedsBusiness="No puede crear sedes todavía: necesita al menos un negocio (TSql_Business) para esta empresa en base de datos.",
JsBranch_NoBusinessInline="No hay negocio asociado: no se puede crear la sede.",
JsBranch_CreateFail="Error al crear la sede.",
JsBranch_NetworkError="Error de red.",
JsBranch_UseInlineFirst="Use el formulario «Crear sede» de la tarjeta para dar de alta una nueva sede.",
JsBranch_SaveFail="Error al guardar.",
JsBranch_DeleteConfirm="¿Eliminar esta sede?",
JsBranch_DeleteFail="No se pudo eliminar.",
    )

    en = dict(
Common_Home="Home",
Index_Breadcrumb="Companies",
Index_CreateCompany="New company",

Col_Name="Name",
Col_AddLeter="Abbrev.",
Col_Logo="Logo",
Col_Description="Description",
Col_Address="Address",
Col_Address2="Address line 2",
Col_PostalCode="Postal code",
Col_City="City",
Col_Country="Country",
Col_State="Status",

Dt_Rows_All="All",
Dt_Rows_N="rows",
Dt_MenuAria="List options",
Dt_Section_Records="Records",
Dt_Section_Export="Export",
Dt_Section_ColumnsVisible="Visible columns",

State_Active="Active",
State_Inactive="Inactive",

Js_ConfirmDeleteCompany="Delete this company?",
Js_ConfirmToggleCompany="Deactivate or activate this company?",

ToastTitle_CreateCompany="Create company",
ToastMessage_CompanySaved="Company created successfully.",
ToastTitle_EditCompany="Edit company",
ToastMessage_CompanyUpdated="Company updated successfully.",

Val_CompanyNameRequired="Company name is required.",
Val_DuplicateNameCreate="A company with this name already exists.",
Val_DuplicateNameEdit="Another company already uses this name.",
Val_UiLanguageRequired="Choose the working language for all employees of this company.",

List_LinkOpenTooltip="Open company",
List_LinkEditTooltip="Edit company",
List_LinkDeleteTooltip="Delete company",
List_LinkToggleTooltip="Activate or deactivate company",

Err_CompanyNotFound="Company not found.",
Err_CannotDeleteRelated="Cannot delete. The company still has related data.",
Msg_CompanyDeleted="Company deleted successfully.",
Msg_CompanyPaused="Company deactivated.",
Msg_CompanyResumed="Company activated.",

Branch_Err_InvalidCompany="Invalid company.",
Branch_Err_NameRequired="Branch name is required.",
Branch_Err_NoBusinessDetailed="There is no business linked to this company. Create at least one TSql_Business with LinCompany pointing to this company before adding branches.",
Branch_Msg_Created="Branch created successfully.",

Branch_Err_InvalidData="Invalid data.",
Branch_Err_NotFound="Branch not found.",
Branch_Msg_Updated="Branch updated successfully.",
Branch_Msg_Deleted="Branch deleted successfully.",

Plantilla_InheritGlobalDefault="— Use default global layout —",

Google_NotConfiguredStrong="Google Maps is not configured.",
Google_NotConfiguredBody="Add GoogleMaps:ApiKey in Web.config for autocomplete and maps. You can still enter the address manually.",

Lbl_NameRequired="Name *",
Lbl_AddLeter="Abbrev.",
Lbl_Country="Country",
Dd_SelectPlaceholder="— Select —",

Lbl_VisualTemplate="Visual template (colors, branding)",
Help_VisualTemplateEmployees="Employees of this company will see this template. Empty means the global default template.",

Lbl_WorkUiLanguageRequired="Employees' working UI language *",

Lbl_Description="Description",

Lbl_AddressGooglePlaces="Address (Google Places)",
Lbl_LegacyBlockHint="Legacy text fields (optional)",
Lbl_AddressLegacy="Address",
Lbl_AddressLegacy2="Address line 2",
Lbl_PostalLegacy="Postal code",
Lbl_CityLegacy="City",
Lbl_LogoPath="Logo (text/path)",

Btn_Edit="Edit",
Btn_Cancel="Cancel",
Btn_SaveCompany="Save company",
Btn_SaveChanges="Save changes",

Page_CreateTitle="Create company",
Page_EditTitle="Edit company",
Details_Field_Name="Name",
Details_Field_AddLeter="Abbrev.",
Details_Field_Description="Description",
Details_Field_Address="Address",
Details_Field_Address2="Address line 2",
Details_Field_Postal="Postal code",
Details_Field_City="City",
Details_Field_Country="Country",

Details_Field_WorkLanguage="Employees' working UI language",
Details_NoValue="-",
Details_WorkLanguageHint="The flag comes from the country linked to the language under Settings → Languages. Employees at this company use this language across the application.",

BranchPanel_Title="Branches",
BranchPanel_New="New branch",
BranchPanel_CreateSectionTitle="Create branch",

Branch_Lbl_NameRequiredModal="Name *",
Branch_Lbl_Description="Description",
Branch_Lbl_LetterShort="Letter (max 2)",
Branch_Btn_SaveInline="Save branch",
Branch_Msg_SaveToManageBranches="Save the company before you can add or manage branches.",
Branch_Msg_NoBusinessBlock="There is no TSql_Business row for this company. At least one is required before creating branches; new branches reuse that business automatically.",
Branch_ModalTitle_Edit="Edit branch",
Aria_CloseModal="Close",
Branch_ModalBtn_CancelModal="Cancel",
Branch_NoRows="No branches yet.",
Branch_RowTooltip_Edit="Edit",
Branch_RowTooltip_Delete="Delete",

LanguageSelect_FlagHint="The flag comes from the country linked to each language under Settings → Languages.",

JsBranch_RefreshFail="Could not refresh the branch list.",
JsBranch_NewNeedsBusiness="You cannot add branches yet: at least one TSql_Business is required for this company.",
JsBranch_NoBusinessInline="No linked business — cannot create the branch.",
JsBranch_CreateFail="Could not create the branch.",
JsBranch_NetworkError="Network error.",
JsBranch_UseInlineFirst="Use the «Create branch» form on this card to add a new branch.",
JsBranch_SaveFail="Could not save.",
JsBranch_DeleteConfirm="Delete this branch?",
JsBranch_DeleteFail="Could not delete.",
)

    Path("Resources").mkdir(exist_ok=True)
    bodies = wrap_res_body(es, en)
    Path("Resources/Company.resx").write_bytes(bodies[0].encode("utf-8"))
    Path("Resources/Company.en.resx").write_bytes(bodies[1].encode("utf-8"))

    props_body = "".join(f'''
        public static string {k} =>
            ResourceManager.GetString(nameof({k}), resourceCulture);''' for k in sorted(es.keys()))

    designer = rf'''//------------------------------------------------------------------------------
// Auto-generated — Desing.Resources.Company satellite strings ({len(es)} entries).
//------------------------------------------------------------------------------
namespace Desing.Resources
{{
    using System;
    using global::System.Globalization;
    using global::System.Resources;

    /// <summary>Empresa (TSql_Company): localized strings (.resx satellite; UI culture from LanguageUiHelper).</summary>
    public class Company
    {{
        private static ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        public static ResourceManager ResourceManager =>
            resourceMan ??
            (resourceMan = new ResourceManager("Desing.Resources.Company", typeof(Company).Assembly));

        public static CultureInfo Culture
        {{
            get => resourceCulture;
            set => resourceCulture = value;
        }}
        {props_body}
    }}
}}
'''

    Path("Resources/Company.Designer.cs").write_text(designer, encoding="utf-8")
    print(f"Written {len(es)} keys")


if __name__ == "__main__":
    main()
