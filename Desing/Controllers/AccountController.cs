using DAL;
using Desing.Helpers;
using Desing.Models;
using Desing.Resources;
using Desing.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
namespace Desing.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            ViewBag.ReturnUrl = returnUrl;

            var isPluginLogin = IsPluginCadReturnUrl(returnUrl);
            var passwordResetSent = TempData["PasswordResetSent"] as bool? == true;
            var passwordChanged = TempData["PasswordChanged"] as bool? == true;
            if (passwordResetSent)
                ViewBag.PasswordResetSent = true;
            if (passwordChanged)
                ViewBag.PasswordChanged = true;

            if ((isPluginLogin || passwordResetSent || passwordChanged) && User.Identity.IsAuthenticated)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            }
            else if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            if (isPluginLogin)
                ApplyPluginCadLoginChrome();

            if (string.Equals(Request["pluginDeviceBlocked"], "1", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = Common.PluginCad_DeviceBlocked;
            }
            else if (string.Equals(Request["pluginUserInactive"], "1", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = Common.PluginCad_UserInactive;
            }
            else if (!string.IsNullOrWhiteSpace(returnUrl) && !isPluginLogin)
            {
                ViewBag.ErrorMessage = Common.Account_Err_LoginRequired;
            }
            return View();
        }
        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            var isPluginLogin = IsPluginCadReturnUrl(returnUrl);
            if (isPluginLogin)
                ApplyPluginCadLoginChrome();

            if (model == null || !ModelState.IsValid)
            {
                return View(model);
            }

            ApplicationUser user = null;
            try
            {
                user = await UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                    user = await UserManager.FindByNameAsync(model.Email);

                if (user == null)
                {
                    ViewBag.ErrorMessage = Common.Account_Err_UnknownEmail;
                    return View(model);
                }

                if (!await UserManager.IsEmailConfirmedAsync(user.Id))
                {
                    if (isPluginLogin)
                    {
                        ViewBag.ErrorMessage = Common.PluginCad_UserInactive;
                        ViewBag.PlantillaLogo = PluginCadDeviceHelper.DefaultLogoVirtualPath;
                        return View(model);
                    }

                    try
                    {
                        await SendEmailConfirmationTokenAsync(user.Id, user.UserName, user.Email, Common.Account_EmailConfirmation_Subject);
                    }
                    catch
                    {
                    }

                    ViewBag.ErrorMessage = Common.Account_Err_EmailNotConfirmed;
                    return View(model);
                }

                if (!await UserManager.CheckPasswordAsync(user, model.Password))
                {
                    ViewBag.ErrorMessage = Common.Account_Err_InvalidPassword;
                    return View(model);
                }

                var result = await SignInManager.PasswordSignInAsync(user.UserName, model.Password, model.RememberMe, shouldLockout: false);
                switch (result)
                {
                    case SignInStatus.LockedOut:
                        return View("Lockout");
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    case SignInStatus.Failure:
                        ViewBag.ErrorMessage = Common.Account_Err_InvalidPassword;
                        return View(model);
                }
            }
            catch
            {
                ViewBag.ErrorMessage = Common.Account_Err_LoginFailed;
                return View(model);
            }

            if (user != null && isPluginLogin)
            {
                var snap = PluginCadDeviceHelper.TryReadCookie(Request);
                var userId = user.Id;
                HostingEnvironment.QueueBackgroundWorkItem(_ =>
                {
                    try
                    {
                        using (var ctx = new ConexionData())
                        {
                            PluginCadDeviceHelper.RegisterAfterLogin(ctx, snap, userId);
                        }
                    }
                    catch
                    {
                    }
                });
            }
            else if (user != null)
            {
                try { WritePlantillaChromeFromUser(user.Id); }
                catch { }
            }

            return RedirectToLocal(returnUrl);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", Common.Account_Err_InvalidCode);
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View("Register");
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    ////sendgrid.com/
                    //await SendEmailConfirmationTokenAsync(user.Id, user.UserName, user.Email, "Por favor, valida tu cuenta vscad");
                    //ViewBag.Message = "Chek yor mail";
                    //return RedirectToAction("SendCode", new { ReturnUrl = "registerLink", RememberMe = false });

                    ////await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    //// For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    //// Send an email with this link
                    //// string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    //// var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    //// await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    Session["userSystem"] = user.Id;
                    Session["userVscad"] = model.Email;
                    TempData["passVscad"] = model.Password;

                    return RedirectToAction("Create_Employee", "Employee");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
                user = await UserManager.FindByNameAsync(model.Email);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                ViewBag.ErrorMessage = Common.Account_Err_UnknownEmail;
                return View(model);
            }

            try
            {
                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { code, email = user.Email },
                    protocol: Request.Url.Scheme);
                var body = string.Format(Common.Account_ResetEmail_Html, callbackUrl);
                await TandemMailClient.SendHtmlAsync(user.Email, Common.Account_ResetEmail_Subject, body);
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                TempData["PasswordResetSent"] = true;
                return RedirectToAction("Login", "Account");
            }
            catch
            {
                ViewBag.ErrorMessage = Common.Account_Err_MailSendFailed;
                return View(model);
            }
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            TempData["PasswordResetSent"] = true;
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code, string email)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }

            return View(new ResetPasswordViewModel
            {
                Code = code,
                Email = email ?? ""
            });
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
                user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                ViewBag.ErrorMessage = Common.Account_Err_UnknownEmail;
                return View(model);
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                TempData["PasswordChanged"] = true;
                return RedirectToAction("Login", "Account");
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            TempData["PasswordChanged"] = true;
            return RedirectToAction("Login", "Account");
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }


        [AllowAnonymous]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }
        public ActionResult user()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private void ApplyPluginCadLoginChrome()
        {
            ViewBag.PluginCadLogin = true;
            ViewBag.SkipRemoteFonts = true;
            var companyLogo = PluginCadDeviceHelper.TryReadLogoCookie(Request);
            ViewBag.PlantillaLogo = string.IsNullOrWhiteSpace(companyLogo)
                ? PluginCadDeviceHelper.DefaultLogoVirtualPath
                : companyLogo;
        }

        private static bool IsPluginCadReturnUrl(string returnUrl)
        {
            return !string.IsNullOrWhiteSpace(returnUrl)
                && returnUrl.IndexOf("/DesignToolsAutocad/", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion

        #region Helper2021
        public async Task SendEmailConfirmationTokenAsync(string userID,
            string userName,
            string userEmail,
            string subject)
        {
            string code = await UserManager.GenerateEmailConfirmationTokenAsync(userID);
            var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = userID, code = code }, protocol: Request.Url.Scheme);
            EmailService emailService = new EmailService();
            var data = new { userName = userName, callbackUrl = callbackUrl };
            var basePathTemplate = Server.MapPath("~/Resources/Template/ConfirmEmail.html");
            var content = emailService.GetHtml(basePathTemplate, data);

            List<Documents> documents = new List<Documents>();
            documents.Add(new Documents
            {
                ContentId = "logo",
                Disposition = Disposition.inline.ToString(),
                Type = "image/png",
                Filename = "logo",
                Path = Server.MapPath("~/Resources/Template/Images/logo.png"),
            });
            await emailService.SendNotification(documents, userEmail, subject, content);
        }

        #endregion

    }
}