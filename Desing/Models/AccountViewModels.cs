using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Desing.Resources;

namespace Desing.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_EmailRequired")]
        [Display(Name = "Display_Email", ResourceType = typeof(Common))]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_CodeRequired")]
        [Display(Name = "Display_Code", ResourceType = typeof(Common))]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Display_RememberBrowser", ResourceType = typeof(Common))]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_EmailRequired")]
        [Display(Name = "Display_Email", ResourceType = typeof(Common))]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {

        public string EmailError { get; set; }
        public string PasswordError { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_EmailRequired")]
        [Display(Name = "Display_Email", ResourceType = typeof(Common))]
        [EmailAddress(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_EmailInvalid")]
        public string Email { get; set; }


        [Required(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_PasswordRequired")]
        [DataType(DataType.Password)]
        [Display(Name = "Display_Password", ResourceType = typeof(Common))]
        public string Password { get; set; }



        [Display(Name = "Display_RememberMe", ResourceType = typeof(Common))]
        public bool RememberMe { get; set; }
        [Display(Name = "Display_UserName", ResourceType = typeof(Common))]
        public string UserName { get; set; }

    }

    public class RegisterViewModel
    {
        [Required(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_EmailRequired")]
        [EmailAddress(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_EmailInvalid")]
        [Display(Name = "Display_Email", ResourceType = typeof(Common))]
        public string Email { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_PasswordRequired")]
        [StringLength(100,
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_PasswordTooShort",
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Display_Password", ResourceType = typeof(Common))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Display_ConfirmPassword", ResourceType = typeof(Common))]
        [Compare("Password",
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_PasswordsDoNotMatch")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_EmailRequired")]
        [EmailAddress(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_EmailInvalid")]
        [Display(Name = "Display_Email", ResourceType = typeof(Common))]
        public string Email { get; set; }

        [Required(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_PasswordRequired")]
        [StringLength(100,
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_PasswordTooShort",
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Display_Password", ResourceType = typeof(Common))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Display_ConfirmPassword", ResourceType = typeof(Common))]
        [Compare("Password",
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_PasswordsDoNotMatch")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_EmailRequired")]
        [EmailAddress(
            ErrorMessageResourceType = typeof(Common),
            ErrorMessageResourceName = "Val_EmailInvalid")]
        [Display(Name = "Display_Email", ResourceType = typeof(Common))]
        public string Email { get; set; }
    }
}
