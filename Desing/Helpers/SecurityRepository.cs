using DAL;
using Desing.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Desing.Helpers
{
    /// <summary>
    /// Acceso SQL a <c>TSql_Roles_V2</c>, <c>TSql_Functions</c>, <c>TSql_RoleFunction</c> y
    /// <c>TSql_UserRole</c> respetando la auditoría estándar y el borrado lógico.
    /// Cada cambio invalida la caché de <see cref="FunctionAccessService"/>.
    /// </summary>
    public static class SecurityRepository
    {
        private const string Audit = "Is_Delete = 1, LinModifiedBy = @p1, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1";

        /* ------------------------------ Roles ------------------------------ */

        private const string RoleSelect = @"
SELECT r.IdObject, r.TextLabel, r.TextCode, r.TextDescription, r.Is_System, r.Is_AllFunctions, r.Is_Active,
       r.LinkMadeBy, r.LinModifiedBy, r.AddDateMade, r.AddLastDateChange, r.Ntimeschanged,
       (SELECT COUNT(1) FROM dbo.TSql_RoleFunction rf JOIN dbo.TSql_Functions f ON f.IdObject = rf.LinkFunctions
         WHERE rf.LinkRoles_V2 = r.IdObject AND rf.Is_Delete = 0 AND f.Is_Delete = 0) AS NFunctions,
       (SELECT COUNT(1) FROM dbo.TSql_UserRole ur
         WHERE ur.LinkRoles_V2 = r.IdObject AND ur.Is_Delete = 0) AS NUsers
  FROM dbo.TSql_Roles_V2 r
 WHERE r.Is_Delete = 0";

        public static List<SecurityRoleRow> ListRoles(ConexionData db)
        {
            return db.Database.SqlQuery<SecurityRoleRow>(RoleSelect + " ORDER BY r.TextLabel").ToList();
        }

        public static SecurityRoleRow GetRole(ConexionData db, long id)
        {
            return db.Database.SqlQuery<SecurityRoleRow>(RoleSelect + " AND r.IdObject = @p0", id).FirstOrDefault();
        }

        public static bool RoleLabelExists(ConexionData db, string label, long? excludeId)
        {
            return db.Database.SqlQuery<int>(
                "SELECT COUNT(1) FROM dbo.TSql_Roles_V2 WHERE Is_Delete = 0 AND LOWER(TextLabel) = LOWER(@p0) AND IdObject <> @p1",
                label, excludeId ?? 0).First() > 0;
        }

        public static bool RoleCodeExists(ConexionData db, string code, long? excludeId)
        {
            return db.Database.SqlQuery<int>(
                "SELECT COUNT(1) FROM dbo.TSql_Roles_V2 WHERE Is_Delete = 0 AND TextCode IS NOT NULL AND LOWER(TextCode) = LOWER(@p0) AND IdObject <> @p1",
                code, excludeId ?? 0).First() > 0;
        }

        public static long InsertRole(ConexionData db, SecurityRoleRow role, string userId)
        {
            var id = db.Database.SqlQuery<long>(
                @"INSERT INTO dbo.TSql_Roles_V2
                    (TextLabel, TextCode, TextDescription, Is_System, Is_AllFunctions,
                     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
                  OUTPUT INSERTED.IdObject
                  VALUES (@p0, @p1, @p2, 0, @p3, 0, @p4, @p5, NULL, GETDATE(), NULL, 0)",
                role.TextLabel, (object)role.TextCode ?? DBNull.Value, (object)role.TextDescription ?? DBNull.Value,
                role.Is_AllFunctions, role.Is_Active, userId).First();
            FunctionAccessService.InvalidateAll();
            return id;
        }

        public static void UpdateRole(ConexionData db, SecurityRoleRow role, string userId)
        {
            db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.TSql_Roles_V2
                     SET TextLabel = @p1, TextCode = @p2, TextDescription = @p3,
                         Is_AllFunctions = CASE WHEN Is_System = 1 THEN 1 ELSE @p4 END,
                         Is_Active = CASE WHEN Is_System = 1 THEN 1 ELSE @p5 END,
                         LinModifiedBy = @p6, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
                   WHERE IdObject = @p0 AND Is_Delete = 0",
                role.IdObject, role.TextLabel, (object)role.TextCode ?? DBNull.Value, (object)role.TextDescription ?? DBNull.Value,
                role.Is_AllFunctions, role.Is_Active, userId);
            FunctionAccessService.InvalidateAll();
        }

        /// <summary>Borrado lógico del rol y de sus enlaces. Los roles de sistema no se borran.</summary>
        public static bool SoftDeleteRole(ConexionData db, long id, string userId)
        {
            var n = db.Database.ExecuteSqlCommand(
                "UPDATE dbo.TSql_Roles_V2 SET " + Audit + " WHERE IdObject = @p0 AND Is_Delete = 0 AND Is_System = 0",
                id, userId);
            if (n > 0)
            {
                db.Database.ExecuteSqlCommand("UPDATE dbo.TSql_RoleFunction SET " + Audit + " WHERE LinkRoles_V2 = @p0 AND Is_Delete = 0", id, userId);
                db.Database.ExecuteSqlCommand("UPDATE dbo.TSql_UserRole SET " + Audit + " WHERE LinkRoles_V2 = @p0 AND Is_Delete = 0", id, userId);
            }
            FunctionAccessService.InvalidateAll();
            return n > 0;
        }

        public static List<SecurityRoleOption> ListRoleOptions(ConexionData db)
        {
            return db.Database.SqlQuery<SecurityRoleOption>(
                "SELECT IdObject, TextLabel, TextDescription FROM dbo.TSql_Roles_V2 WHERE Is_Delete = 0 AND Is_Active = 1 ORDER BY TextLabel").ToList();
        }

        /* ---------------------------- Funciones ---------------------------- */

        private const string FunctionSelect = @"
SELECT f.IdObject, f.TextLabel, f.TextCode, f.TextModule, f.TextDescription, f.Is_Active,
       f.LinkMadeBy, f.LinModifiedBy, f.AddDateMade, f.AddLastDateChange, f.Ntimeschanged,
       (SELECT COUNT(1) FROM dbo.TSql_RoleFunction rf JOIN dbo.TSql_Roles_V2 r ON r.IdObject = rf.LinkRoles_V2
         WHERE rf.LinkFunctions = f.IdObject AND rf.Is_Delete = 0 AND r.Is_Delete = 0) AS NRoles
  FROM dbo.TSql_Functions f
 WHERE f.Is_Delete = 0";

        public static List<SecurityFunctionRow> ListFunctions(ConexionData db)
        {
            return db.Database.SqlQuery<SecurityFunctionRow>(FunctionSelect + " ORDER BY f.TextModule, f.TextLabel").ToList();
        }

        public static SecurityFunctionRow GetFunction(ConexionData db, long id)
        {
            return db.Database.SqlQuery<SecurityFunctionRow>(FunctionSelect + " AND f.IdObject = @p0", id).FirstOrDefault();
        }

        public static bool FunctionCodeExists(ConexionData db, string code, long? excludeId)
        {
            return db.Database.SqlQuery<int>(
                "SELECT COUNT(1) FROM dbo.TSql_Functions WHERE Is_Delete = 0 AND LOWER(TextCode) = LOWER(@p0) AND IdObject <> @p1",
                code, excludeId ?? 0).First() > 0;
        }

        public static long InsertFunction(ConexionData db, SecurityFunctionRow f, string userId)
        {
            var id = db.Database.SqlQuery<long>(
                @"INSERT INTO dbo.TSql_Functions
                    (TextLabel, TextCode, TextModule, TextDescription,
                     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
                  OUTPUT INSERTED.IdObject
                  VALUES (@p0, @p1, @p2, @p3, 0, @p4, @p5, NULL, GETDATE(), NULL, 0)",
                f.TextLabel, f.TextCode, (object)f.TextModule ?? DBNull.Value, (object)f.TextDescription ?? DBNull.Value,
                f.Is_Active, userId).First();
            FunctionAccessService.InvalidateAll();
            return id;
        }

        public static void UpdateFunction(ConexionData db, SecurityFunctionRow f, string userId)
        {
            db.Database.ExecuteSqlCommand(
                @"UPDATE dbo.TSql_Functions
                     SET TextLabel = @p1, TextCode = @p2, TextModule = @p3, TextDescription = @p4, Is_Active = @p5,
                         LinModifiedBy = @p6, AddLastDateChange = GETDATE(), Ntimeschanged = Ntimeschanged + 1
                   WHERE IdObject = @p0 AND Is_Delete = 0",
                f.IdObject, f.TextLabel, f.TextCode, (object)f.TextModule ?? DBNull.Value, (object)f.TextDescription ?? DBNull.Value,
                f.Is_Active, userId);
            FunctionAccessService.InvalidateAll();
        }

        public static bool SoftDeleteFunction(ConexionData db, long id, string userId)
        {
            var n = db.Database.ExecuteSqlCommand(
                "UPDATE dbo.TSql_Functions SET " + Audit + " WHERE IdObject = @p0 AND Is_Delete = 0", id, userId);
            if (n > 0)
            {
                db.Database.ExecuteSqlCommand("UPDATE dbo.TSql_RoleFunction SET " + Audit + " WHERE LinkFunctions = @p0 AND Is_Delete = 0", id, userId);
            }
            FunctionAccessService.InvalidateAll();
            return n > 0;
        }

        /* ------------------------------ Enlaces ----------------------------- */

        public static List<long> GetRoleFunctionIds(ConexionData db, long roleId)
        {
            return db.Database.SqlQuery<long>(
                "SELECT LinkFunctions FROM dbo.TSql_RoleFunction WHERE LinkRoles_V2 = @p0 AND Is_Delete = 0", roleId).ToList();
        }

        public static List<long> GetFunctionRoleIds(ConexionData db, long functionId)
        {
            return db.Database.SqlQuery<long>(
                "SELECT LinkRoles_V2 FROM dbo.TSql_RoleFunction WHERE LinkFunctions = @p0 AND Is_Delete = 0", functionId).ToList();
        }

        public static List<string> GetRoleUserIds(ConexionData db, long roleId)
        {
            return db.Database.SqlQuery<string>(
                "SELECT LinkAspNetUsers FROM dbo.TSql_UserRole WHERE LinkRoles_V2 = @p0 AND Is_Delete = 0", roleId).ToList();
        }

        public static List<long> GetUserRoleIds(ConexionData db, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId) || !FunctionAccessService.TablesExist(db))
            {
                return new List<long>();
            }

            return db.Database.SqlQuery<long>(
                "SELECT LinkRoles_V2 FROM dbo.TSql_UserRole WHERE LinkAspNetUsers = @p0 AND Is_Delete = 0", userId).ToList();
        }

        public static void SyncRoleFunctions(ConexionData db, long roleId, IEnumerable<long> functionIds, string userId)
        {
            var wanted = new HashSet<long>(functionIds ?? Enumerable.Empty<long>());
            var current = new HashSet<long>(GetRoleFunctionIds(db, roleId));
            foreach (var id in current.Where(c => !wanted.Contains(c)))
            {
                db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_RoleFunction SET " + Audit.Replace("@p1", "@p2") + " WHERE LinkRoles_V2 = @p0 AND LinkFunctions = @p1 AND Is_Delete = 0",
                    roleId, id, userId);
            }
            foreach (var id in wanted.Where(w => !current.Contains(w)))
            {
                InsertRoleFunction(db, roleId, id, userId);
            }
            FunctionAccessService.InvalidateAll();
        }

        public static void SyncFunctionRoles(ConexionData db, long functionId, IEnumerable<long> roleIds, string userId)
        {
            var wanted = new HashSet<long>(roleIds ?? Enumerable.Empty<long>());
            var current = new HashSet<long>(GetFunctionRoleIds(db, functionId));
            foreach (var id in current.Where(c => !wanted.Contains(c)))
            {
                db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_RoleFunction SET " + Audit.Replace("@p1", "@p2") + " WHERE LinkFunctions = @p0 AND LinkRoles_V2 = @p1 AND Is_Delete = 0",
                    functionId, id, userId);
            }
            foreach (var id in wanted.Where(w => !current.Contains(w)))
            {
                InsertRoleFunction(db, id, functionId, userId);
            }
            FunctionAccessService.InvalidateAll();
        }

        public static void SyncUserRoles(ConexionData db, string userId, IEnumerable<long> roleIds, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(userId) || !FunctionAccessService.TablesExist(db))
            {
                return;
            }

            var wanted = new HashSet<long>(roleIds ?? Enumerable.Empty<long>());
            var current = new HashSet<long>(GetUserRoleIds(db, userId));
            foreach (var id in current.Where(c => !wanted.Contains(c)))
            {
                db.Database.ExecuteSqlCommand(
                    "UPDATE dbo.TSql_UserRole SET " + Audit.Replace("@p1", "@p2") + " WHERE LinkAspNetUsers = @p0 AND LinkRoles_V2 = @p1 AND Is_Delete = 0",
                    userId, id, currentUserId);
            }
            foreach (var id in wanted.Where(w => !current.Contains(w)))
            {
                InsertUserRole(db, userId, id, currentUserId);
            }
            FunctionAccessService.InvalidateAll();
        }

        /// <summary>Asigna un rol activo al usuario; si ya lo tiene no hace nada.</summary>
        public static bool AddUserRole(ConexionData db, string userId, long roleId, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(userId) || GetUserRoleIds(db, userId).Contains(roleId))
            {
                return false;
            }

            InsertUserRole(db, userId, roleId, currentUserId);
            FunctionAccessService.InvalidateAll();
            return true;
        }

        /// <summary>Quita el rol al usuario (borrado lógico de la fila de <c>TSql_UserRole</c>).</summary>
        public static bool RemoveUserRole(ConexionData db, string userId, long roleId, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            var n = db.Database.ExecuteSqlCommand(
                "UPDATE dbo.TSql_UserRole SET " + Audit.Replace("@p1", "@p2") + " WHERE LinkAspNetUsers = @p0 AND LinkRoles_V2 = @p1 AND Is_Delete = 0",
                userId, roleId, currentUserId);
            FunctionAccessService.InvalidateAll();
            return n > 0;
        }

        private static void InsertRoleFunction(ConexionData db, long roleId, long functionId, string userId)
        {
            db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.TSql_RoleFunction
                    (TextLabel, LinkRoles_V2, LinkFunctions,
                     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
                  SELECT LEFT(r.TextLabel + N' · ' + f.TextLabel, 500), r.IdObject, f.IdObject,
                         0, 1, @p2, NULL, GETDATE(), NULL, 0
                    FROM dbo.TSql_Roles_V2 r CROSS JOIN dbo.TSql_Functions f
                   WHERE r.IdObject = @p0 AND r.Is_Delete = 0 AND f.IdObject = @p1 AND f.Is_Delete = 0",
                roleId, functionId, userId);
        }

        private static void InsertUserRole(ConexionData db, string userId, long roleId, string currentUserId)
        {
            db.Database.ExecuteSqlCommand(
                @"INSERT INTO dbo.TSql_UserRole
                    (TextLabel, LinkAspNetUsers, LinkRoles_V2,
                     Is_Delete, Is_Active, LinkMadeBy, LinModifiedBy, AddDateMade, AddLastDateChange, Ntimeschanged)
                  SELECT LEFT(ISNULL(u.UserName, u.Id) + N' · ' + r.TextLabel, 500), u.Id, r.IdObject,
                         0, 1, @p2, NULL, GETDATE(), NULL, 0
                    FROM dbo.AspNetUsers u CROSS JOIN dbo.TSql_Roles_V2 r
                   WHERE u.Id = @p0 AND r.IdObject = @p1 AND r.Is_Delete = 0",
                userId, roleId, currentUserId);
        }

        /* ------------------------------ Usuarios ---------------------------- */

        public static List<SecurityUserOption> ListUsers(ConexionData db)
        {
            return db.Database.SqlQuery<SecurityUserOption>(
                @"SELECT u.Id, u.UserName,
                         LTRIM(RTRIM(ISNULL(e.AttName, N'') + N' ' + ISNULL(e.AttSurname, N''))) AS FullName
                    FROM dbo.AspNetUsers u
                    OUTER APPLY (SELECT TOP (1) AttName, AttSurname FROM dbo.TSql_Employee
                                  WHERE LinAspNetUsert = u.Id AND AttIsDeleted = 0 ORDER BY SysObjectID) e
                   ORDER BY u.UserName").ToList();
        }
    }
}
