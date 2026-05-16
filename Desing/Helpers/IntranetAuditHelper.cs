using DAL;
using Desing.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Principal;

namespace Desing.Helpers
{
    /// <summary>
    /// Helper de auditoría para entidades intranet (`TSql_*`).
    ///
    /// Aplica las 9 columnas estándar definidas en la regla
    /// `.cursor/rules/sql-tsql-table-conventions.mdc`:
    ///   IdObject, TextLabel,
    ///   Is_Delete, Is_Active,
    ///   LinkMadeBy, LinModifiedBy,
    ///   AddDateMade, AddLastDateChange, Ntimeschanged.
    ///
    /// Los métodos `SetAuditOn*` son genéricos vía reflection: sirven para
    /// cualquier entidad EF que exponga las columnas con esos nombres,
    /// sin requerir interface ni cambios en las clases auto-generadas.
    /// </summary>
    public static class IntranetAuditHelper
    {
        /* ===================================================================
           Construcción del modelo de visualización (Details / Audit section).
           =================================================================== */
        public static IntranetAuditDisplayModel BuildDisplay(
            ConexionData db,
            string linkMadeBy,
            string linModifiedBy,
            string addChangeBy,
            DateTime addDateMade,
            DateTime? addLastDateChange,
            long ntimeschanged)
        {
            var userIds = new[] { linkMadeBy, linModifiedBy, addChangeBy }
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var users = userIds.Count == 0
                ? new Dictionary<string, AspNetUsers>()
                : db.AspNetUsers.Where(u => userIds.Contains(u.Id)).ToDictionary(u => u.Id);

            var employeeLinks = userIds.Count == 0
                ? new Dictionary<string, long>()
                : db.TSql_Employee
                    .Where(e => userIds.Contains(e.LinAspNetUsert) && !e.AttIsDeleted)
                    .GroupBy(e => e.LinAspNetUsert)
                    .ToDictionary(g => g.Key, g => g.FirstOrDefault().SysObjectID);

            return new IntranetAuditDisplayModel
            {
                LinkMadeBy = ResolveUser(linkMadeBy, users, employeeLinks),
                LinModifiedBy = ResolveUser(linModifiedBy, users, employeeLinks),
                AddChangeBy = ResolveUser(addChangeBy, users, employeeLinks),
                AddDateMade = addDateMade,
                // Si nunca se ha modificado, mostramos la fecha de alta como
                // valor por defecto para no romper la vista existente
                // (`_IntranetAuditSection.cshtml` espera un DateTime no-nullable).
                AddLastDateChange = addLastDateChange ?? addDateMade,
                Ntimeschanged = ntimeschanged
            };
        }

        /// <summary>
        /// Sobrecarga sin `addChangeBy` (columna deprecada por la regla).
        /// </summary>
        public static IntranetAuditDisplayModel BuildDisplay(
            ConexionData db,
            string linkMadeBy,
            string linModifiedBy,
            DateTime addDateMade,
            DateTime? addLastDateChange,
            long ntimeschanged)
        {
            return BuildDisplay(db, linkMadeBy, linModifiedBy, null, addDateMade, addLastDateChange, ntimeschanged);
        }

        private static IntranetAuditUserLink ResolveUser(
            string userId,
            Dictionary<string, AspNetUsers> users,
            Dictionary<string, long> employeeLinks)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new IntranetAuditUserLink { UserId = "", DisplayName = "—" };
            }

            AspNetUsers user;
            users.TryGetValue(userId, out user);
            var display = user != null
                ? (!string.IsNullOrWhiteSpace(user.UserName) ? user.UserName : user.Email)
                : userId;

            long employeeId;
            employeeLinks.TryGetValue(userId, out employeeId);

            return new IntranetAuditUserLink
            {
                UserId = userId,
                DisplayName = display ?? userId,
                EmployeeId = employeeId > 0 ? (long?)employeeId : null
            };
        }

        /* ===================================================================
           Resolución del usuario actual (AspNet Id) — preferente sobre Name.
           =================================================================== */

        /// <summary>
        /// Devuelve el AspNet user Id (cadena GUID) del usuario actual.
        /// Fallbacks: `User.Identity.Name`, y finalmente `"system"`.
        /// </summary>
        public static string ResolveCurrentUserId(IPrincipal user)
        {
            try
            {
                if (user != null && user.Identity != null && user.Identity.IsAuthenticated)
                {
                    var id = user.Identity.GetUserId();
                    if (!string.IsNullOrWhiteSpace(id))
                    {
                        return id;
                    }
                    if (!string.IsNullOrWhiteSpace(user.Identity.Name))
                    {
                        return user.Identity.Name;
                    }
                }
            }
            catch
            {
                /* fallback abajo */
            }
            return "system";
        }

        /* ===================================================================
           SetAuditOnCreate / SetAuditOnUpdate / SetAuditOnDelete
           Genéricos vía reflection: sirven para TSql_Client_V2, TSql_Jobside,
           TSql_DocumentType y cualquier otra tabla que respete la regla.
           =================================================================== */

        /// <summary>
        /// INSERT: rellena `LinkMadeBy`, `AddDateMade` y, si existen, `AddChangeBy`,
        /// `Is_Delete = false`, `Is_Active = true`. Deja `LinModifiedBy = NULL`,
        /// `Ntimeschanged = 0`. Para `AddLastDateChange` deja `null` si la columna
        /// es nullable, o `AddDateMade` como fallback si la columna es
        /// `DateTime` NOT NULL (esquemas legacy todavía no migrados).
        /// </summary>
        public static void SetAuditOnCreate(object entity, IPrincipal user)
        {
            SetAuditOnCreate(entity, ResolveCurrentUserId(user));
        }

        public static void SetAuditOnCreate(object entity, string userId)
        {
            if (entity == null) return;
            var type = entity.GetType();
            var now = DateTime.Now;

            SetIfExists(type, entity, "LinkMadeBy", userId);
            // Compatibilidad con tablas legacy que aún tienen `AddChangeBy`
            // NOT NULL (TSql_DocumentType, TSql_Extension, TSql_DocumentTypeExtension,
            // TSql_Client_V2, TSql_Jobside, etc.). Si la columna no existe o ya se
            // eliminó, `SetIfExists` ignora la asignación.
            SetIfExists(type, entity, "AddChangeBy", userId);
            SetIfExists(type, entity, "AddDateMade", now);
            SetIfExists(type, entity, "LinModifiedBy", null);
            // Si `AddLastDateChange` es `DateTime?` (regla nueva) → null.
            // Si todavía es `DateTime` NOT NULL en la BD (legacy) → `now` para
            // evitar `Cannot insert NULL` y valores fuera de rango (DateTime.MinValue).
            SetIfExistsDate(type, entity, "AddLastDateChange", null, now);
            SetIfExists(type, entity, "Ntimeschanged", 0L);
            SetIfExists(type, entity, "Is_Delete", false);
            SetIfExists(type, entity, "Is_Active", true);
        }

        /// <summary>
        /// UPDATE: actualiza `LinModifiedBy`, `AddLastDateChange`, `AddChangeBy`
        /// (si existe en la entidad) e incrementa `Ntimeschanged`. NO toca
        /// `LinkMadeBy` ni `AddDateMade` (inmutables).
        /// </summary>
        public static void SetAuditOnUpdate(object entity, IPrincipal user)
        {
            SetAuditOnUpdate(entity, ResolveCurrentUserId(user));
        }

        public static void SetAuditOnUpdate(object entity, string userId)
        {
            if (entity == null) return;
            var type = entity.GetType();
            var now = DateTime.Now;

            SetIfExists(type, entity, "LinModifiedBy", userId);
            // Tablas legacy con `AddChangeBy` NOT NULL: si existe, se sobreescribe
            // con el usuario actual; si la columna ya no está, no pasa nada.
            SetIfExists(type, entity, "AddChangeBy", userId);
            SetIfExists(type, entity, "AddLastDateChange", now);
            IncrementIfExists(type, entity, "Ntimeschanged");
        }

        /// <summary>
        /// DELETE lógico: pone `Is_Delete = true` y aplica los campos de
        /// modificación (`LinModifiedBy`, `AddChangeBy`, `AddLastDateChange`,
        /// `Ntimeschanged`).
        /// </summary>
        public static void SetAuditOnDelete(object entity, IPrincipal user)
        {
            SetAuditOnDelete(entity, ResolveCurrentUserId(user));
        }

        public static void SetAuditOnDelete(object entity, string userId)
        {
            if (entity == null) return;
            var type = entity.GetType();

            SetIfExists(type, entity, "Is_Delete", true);
            SetAuditOnUpdate(entity, userId);
        }

        /* ===================================================================
           Reflection helpers.
           =================================================================== */

        /// <summary>
        /// Asignador específico para columnas de fecha con dos esquemas posibles:
        ///   - `DateTime?` (regla actual)  → admite null.
        ///   - `DateTime`  (legacy NOT NULL) → no admite null; se asigna el
        ///     `fallbackIfNotNullable` para evitar `Cannot insert NULL` y para
        ///     no dejar `DateTime.MinValue` (fuera del rango de SQL Server `datetime`).
        /// Si la propiedad no existe, no hace nada.
        /// </summary>
        private static void SetIfExistsDate(
            Type type,
            object entity,
            string propertyName,
            DateTime? value,
            DateTime? fallbackIfNotNullable)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite) return;

            var underlying = Nullable.GetUnderlyingType(prop.PropertyType);
            var isNullable = underlying != null;

            try
            {
                if (value.HasValue)
                {
                    prop.SetValue(entity, value.Value);
                    return;
                }

                if (isNullable)
                {
                    prop.SetValue(entity, null);
                }
                else if (fallbackIfNotNullable.HasValue)
                {
                    prop.SetValue(entity, fallbackIfNotNullable.Value);
                }
                // Si no hay fallback y la columna es NOT NULL, dejamos el valor
                // que ya tuviera (default(DateTime)); el SaveChanges fallará y
                // ese fallo expone un bug del controlador, no del helper.
            }
            catch
            {
                /* si falla la conversión, no rompemos el flujo del controlador */
            }
        }

        private static void SetIfExists(Type type, object entity, string propertyName, object value)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite) return;

            try
            {
                if (value == null)
                {
                    // Asignación a null sólo permitida en tipos por referencia o Nullable<T>.
                    var t = prop.PropertyType;
                    if (!t.IsValueType || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        prop.SetValue(entity, null);
                    }
                    return;
                }

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                var converted = targetType.IsInstanceOfType(value)
                    ? value
                    : Convert.ChangeType(value, targetType);
                prop.SetValue(entity, converted);
            }
            catch
            {
                /* si la conversión falla, dejamos la propiedad como está */
            }
        }

        private static void IncrementIfExists(Type type, object entity, string propertyName)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null || !prop.CanWrite) return;

            try
            {
                var current = prop.GetValue(entity);
                long currentLong = 0L;
                if (current != null)
                {
                    currentLong = Convert.ToInt64(current);
                }

                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                object next = Convert.ChangeType(currentLong + 1L, targetType);
                prop.SetValue(entity, next);
            }
            catch
            {
                /* idem: no hacer nada si la propiedad no es numérica */
            }
        }
    }
}
