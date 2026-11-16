using System;
using System.Collections.Generic;

namespace Desing.Models
{
    /// <summary>Fila de <c>TSql_Roles_V2</c> (SQL directo; la tabla aún no está en el EDMX).</summary>
    public class SecurityRoleRow
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextCode { get; set; }
        public string TextDescription { get; set; }
        public bool Is_System { get; set; }
        public bool Is_AllFunctions { get; set; }
        public bool Is_Active { get; set; }
        public string LinkMadeBy { get; set; }
        public string LinModifiedBy { get; set; }
        public DateTime AddDateMade { get; set; }
        public DateTime? AddLastDateChange { get; set; }
        public long Ntimeschanged { get; set; }
        public int NFunctions { get; set; }
        public int NUsers { get; set; }
    }

    /// <summary>Fila de <c>TSql_Functions</c>.</summary>
    public class SecurityFunctionRow
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextCode { get; set; }
        public string TextModule { get; set; }
        public string TextDescription { get; set; }
        public bool Is_Active { get; set; }
        public string LinkMadeBy { get; set; }
        public string LinModifiedBy { get; set; }
        public DateTime AddDateMade { get; set; }
        public DateTime? AddLastDateChange { get; set; }
        public long Ntimeschanged { get; set; }
        public int NRoles { get; set; }
    }

    /// <summary>Usuario para las listas de selección (AspNetUsers + empleado si existe).</summary>
    public class SecurityUserOption
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
    }

    /// <summary>Opción de rol para la ficha de empleado.</summary>
    public class SecurityRoleOption
    {
        public long IdObject { get; set; }
        public string TextLabel { get; set; }
        public string TextDescription { get; set; }
    }

    public class SecurityRoleFormViewModel
    {
        public SecurityRoleRow Role { get; set; }
        public IList<SecurityFunctionRow> AvailableFunctions { get; set; }
        public IList<long> SelectedFunctionIds { get; set; }
        public IList<SecurityUserOption> AvailableUsers { get; set; }
        public IList<string> SelectedUserIds { get; set; }
    }

    public class SecurityFunctionFormViewModel
    {
        public SecurityFunctionRow Function { get; set; }
        public IList<SecurityRoleOption> AvailableRoles { get; set; }
        public IList<long> SelectedRoleIds { get; set; }
        public IList<FunctionCatalogOption> Catalog { get; set; }
    }

    public class FunctionCatalogOption
    {
        public string Code { get; set; }
        public string Module { get; set; }
    }
}
