using Desing.Helpers;
using Desing.Models;
using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Desing
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_BeginRequest()
        {
            LanguageUiHelper.ApplyCultureEarly(Context.Request);
        }

        protected void Application_Start()
        {
            // Identity uses claims; antiforgery must key off NameIdentifier, not Identity.Name.
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            // Never auto-create databases (shared SQL / remote hosting has no CREATE DATABASE on master).
            Database.SetInitializer<ApplicationDbContext>(null);
            Database.SetInitializer<DAL.ConexionData>(null);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var decimalBinder = new Desing.ModelBinders.CultureFallbackDecimalModelBinder();
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(decimal), decimalBinder);
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(decimal?), decimalBinder);
        }
    }
}
