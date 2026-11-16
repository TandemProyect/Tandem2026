using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;

namespace Desing.Helpers
{
    /// <summary>
    /// Control de acceso por funciones (<c>TSql_Functions</c>) asignadas a roles
    /// (<c>TSql_Roles_V2</c> / <c>TSql_RoleFunction</c>) que tiene cada usuario (<c>TSql_UserRole</c>).
    ///
    /// Regla: un comando (clave <c>Controlador</c>, <c>Controlador.Accion</c>, línea de menú o
    /// comando JS) está protegido solo si existe una función ACTIVA con ese <c>TextCode</c>.
    /// Si no existe, lo puede usar cualquier usuario autenticado. Un rol con
    /// <c>Is_AllFunctions = 1</c> concede todas las funciones.
    ///
    /// Acceso por SQL directo: las tablas aún no están en el EDMX.
    /// </summary>
    public static class FunctionAccessService
    {
        private const string ProtectedCacheKey = "TandemFunctions_Protected_";
        private const string UserCacheKeyPrefix = "TandemFunctions_User_";
        private static readonly TimeSpan ProtectedCacheDuration = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(5);
        private static readonly HashSet<string> Empty = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static int _version;
        private static HashSet<string> _lastKnownProtected;

        private sealed class UserAccess
        {
            public bool All { get; set; }
            public HashSet<string> Granted { get; set; }
        }

        /// <summary>Borra la caché de todos los usuarios (llamar tras cualquier cambio de roles/funciones).</summary>
        public static void InvalidateAll()
        {
            Interlocked.Increment(ref _version);
        }

        public static bool TablesExist(ConexionData db)
        {
            try
            {
                return db.Database.SqlQuery<int>(
                    @"SELECT CASE WHEN OBJECT_ID(N'dbo.TSql_Functions', N'U') IS NOT NULL
                                   AND OBJECT_ID(N'dbo.TSql_Roles_V2', N'U') IS NOT NULL
                                   AND OBJECT_ID(N'dbo.TSql_RoleFunction', N'U') IS NOT NULL
                                   AND OBJECT_ID(N'dbo.TSql_UserRole', N'U') IS NOT NULL
                              THEN 1 ELSE 0 END").FirstOrDefault() == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Códigos protegidos (funciones activas y no borradas).</summary>
        public static HashSet<string> GetProtectedCodes()
        {
            var key = ProtectedCacheKey + Volatile.Read(ref _version);
            var cached = HttpRuntime.Cache[key] as HashSet<string>;
            if (cached != null)
            {
                return cached;
            }

            HashSet<string> codes;
            try
            {
                using (var db = new ConexionData())
                {
                    if (!TablesExist(db))
                    {
                        codes = Empty;
                    }
                    else
                    {
                        codes = new HashSet<string>(
                            db.Database.SqlQuery<string>(
                                @"SELECT LTRIM(RTRIM(TextCode)) FROM dbo.TSql_Functions
                                   WHERE Is_Delete = 0 AND Is_Active = 1 AND TextCode IS NOT NULL").ToList(),
                            StringComparer.OrdinalIgnoreCase);
                    }
                }
                _lastKnownProtected = codes;
            }
            catch
            {
                // Fallo puntual de BD: mantener la última lista conocida para no abrir lo protegido.
                return _lastKnownProtected ?? Empty;
            }

            HttpRuntime.Cache.Insert(key, codes, null, DateTime.UtcNow.Add(ProtectedCacheDuration), Cache.NoSlidingExpiration);
            return codes;
        }

        private static UserAccess GetUserAccess(string userId)
        {
            var key = UserCacheKeyPrefix + Volatile.Read(ref _version) + "_" + userId;
            var cached = HttpRuntime.Cache[key] as UserAccess;
            if (cached != null)
            {
                return cached;
            }

            var access = new UserAccess { All = false, Granted = Empty };
            try
            {
                using (var db = new ConexionData())
                {
                    if (TablesExist(db))
                    {
                        access.All = db.Database.SqlQuery<int>(
                            @"SELECT COUNT(1) FROM dbo.TSql_UserRole ur
                               JOIN dbo.TSql_Roles_V2 r ON r.IdObject = ur.LinkRoles_V2
                              WHERE ur.LinkAspNetUsers = @p0 AND ur.Is_Delete = 0 AND ur.Is_Active = 1
                                AND r.Is_Delete = 0 AND r.Is_Active = 1 AND r.Is_AllFunctions = 1",
                            userId).FirstOrDefault() > 0;

                        access.Granted = new HashSet<string>(
                            db.Database.SqlQuery<string>(
                                @"SELECT DISTINCT LTRIM(RTRIM(f.TextCode))
                                    FROM dbo.TSql_UserRole ur
                                    JOIN dbo.TSql_Roles_V2 r ON r.IdObject = ur.LinkRoles_V2
                                    JOIN dbo.TSql_RoleFunction rf ON rf.LinkRoles_V2 = r.IdObject
                                    JOIN dbo.TSql_Functions f ON f.IdObject = rf.LinkFunctions
                                   WHERE ur.LinkAspNetUsers = @p0 AND ur.Is_Delete = 0 AND ur.Is_Active = 1
                                     AND r.Is_Delete = 0 AND r.Is_Active = 1
                                     AND rf.Is_Delete = 0 AND rf.Is_Active = 1
                                     AND f.Is_Delete = 0",
                                userId).ToList(),
                            StringComparer.OrdinalIgnoreCase);
                    }
                }
            }
            catch
            {
                // Sin caché: se reintenta en la siguiente petición; mientras, sin funciones concedidas.
                return access;
            }

            HttpRuntime.Cache.Insert(key, access, null, DateTime.UtcNow.Add(UserCacheDuration), Cache.NoSlidingExpiration);
            return access;
        }

        /// <summary>¿Puede el usuario usar el comando <paramref name="code"/>?</summary>
        public static bool Can(string userId, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return true;
            }

            var protectedCodes = GetProtectedCodes();
            if (protectedCodes.Count == 0 || !protectedCodes.Contains(code.Trim()))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return false;
            }

            var access = GetUserAccess(userId);
            return access.All || access.Granted.Contains(code.Trim());
        }

        /// <summary>True si el usuario puede usar todos los códigos indicados.</summary>
        public static bool CanAll(string userId, IEnumerable<string> codes)
        {
            return codes == null || codes.All(c => Can(userId, c));
        }

        /// <summary>Códigos protegidos que el usuario NO tiene (para ocultar en cliente).</summary>
        public static IList<string> GetDeniedCodes(string userId)
        {
            var protectedCodes = GetProtectedCodes();
            if (protectedCodes.Count == 0)
            {
                return new List<string>();
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                return protectedCodes.OrderBy(c => c).ToList();
            }

            var access = GetUserAccess(userId);
            if (access.All)
            {
                return new List<string>();
            }

            return protectedCodes.Where(c => !access.Granted.Contains(c)).OrderBy(c => c).ToList();
        }
    }
}
