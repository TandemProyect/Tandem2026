using Desing.Helpers;
using Desing.Models;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
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
            var sw = Stopwatch.StartNew();
            LanguageUiHelper.ApplyCultureEarly(Context.Request);
            sw.Stop();
            TraceStartupTiming("BeginRequest " + (Context.Request != null ? Context.Request.RawUrl : ""), sw.ElapsedMilliseconds);
        }

        protected void Application_Start()
        {
            var sw = Stopwatch.StartNew();
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
            sw.Stop();
            TraceStartupTiming("Application_Start", sw.ElapsedMilliseconds);
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            var anti = ex as HttpAntiForgeryException
                ?? ex?.InnerException as HttpAntiForgeryException
                ?? ex?.GetBaseException() as HttpAntiForgeryException;
            if (anti == null)
                return;

            Server.ClearError();
            try
            {
                Context.GetOwinContext().Authentication.SignOut(
                    DefaultAuthenticationTypes.ApplicationCookie);
            }
            catch
            {
            }

            var returnUrl = Context.Request != null ? Context.Request["ReturnUrl"] : null;
            var login = VirtualPathUtility.ToAbsolute("~/Account/Login");
            if (!string.IsNullOrWhiteSpace(returnUrl) && returnUrl.StartsWith("/", StringComparison.Ordinal))
                login += "?returnUrl=" + Uri.EscapeDataString(returnUrl);
            Response.Redirect(login, endResponse: true);
        }

        private static void TraceStartupTiming(string label, long elapsedMs)
        {
            if (!string.Equals(ConfigurationManager.AppSettings["TandemStartupTiming"], "true", StringComparison.OrdinalIgnoreCase))
                return;

            Debug.WriteLine("[TandemStartupTiming] " + label + " = " + elapsedMs + " ms");
        }
    }
}
