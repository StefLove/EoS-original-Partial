using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EoS.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
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

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel //: IValidatableObject
    {
        //[Required] not when Admin account
        [Display(Name = "Role")]
        [EnumDataType(typeof(RegisterRole))]
        public RegisterRole? Role { get; set; } //if not set, it is an Admin account !!

        [Required(ErrorMessage = "*")]
        [Display(Name = "First Name")]
        public string UserFirstName { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Last Name")]
        public string UserLastName { get; set; }

        [Required(ErrorMessage = "*")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Organisation")]
        public string Organisation { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Country")]
        public int CountryId { get; set; }
        public SelectList Countries { get; set; }

        [Display(Name = "Account Expiry Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Account Lockout End Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? LockoutEndDate { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        //public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        //{
        //    //throw new NotImplementedException();
        //    if (Role == RegisterRole.IdeaCarrier && string.IsNullOrEmpty(UserFirstName))
        //    {
        //        yield return new ValidationResult("First Name is required!", new List<string> { "UserFirstName" });
        //    }
        //}
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class DisplayUserViewModel
    {
        public string Id;

        [Display(Name = "Role")]
        public string Role { get; set; }

        [Display(Name = "Start Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Last Login Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? LastLoginDate { get; set; }

        [Display(Name = "Country")]
        public string CountryName { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "User Full Name")]
        public string UserFullName { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        //[Phone]
        //[Display(Name = "PhoneNumber")]
        //public string PhoneNumber { get; set; } //<------------

        //[Display(Name = "Phone Number Confirmed")]
        //public bool PhoneNumberConfirmed { get; set; } //<-----------

        //[Display(Name = "Organisation")]
        //public string Organisation { get; set; } //<---------------

        [Display(Name = "Account Expiry Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Lockout End Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? LockoutEndDate { get; set; }

        [Display(Name = "Lockout Enabled")]
        public bool LockoutEnabled { get; set; }

        [Display(Name = "Access Failed Count")]
        public int AccessFailedCount { get; set; }

        [Display(Name = "Two Factor Enabled")]
        public bool TwoFactorEnabled { get; set; }

        [Display(Name = "#Blogs")]
        public int NumberOfBlogs { get; set; }

        [Display(Name = "#Blog Comments")]
        public int NumberOfBlogComments { get; set; }

        //[Display(Name = "Blogs")]
        //public ICollection<Admin.Blog> Blogs { get; set; }
        //[Display(Name = "Blog Comments")]
        //public ICollection<Shared.BlogComment> BlogComments { get; set; }
    }

    public class EditUserViewModel
    {
        [Key]
        public string Id { get; set; }

        //[Display(Name = "Type")]
        //[EnumDataType(typeof(RegisterRole))]
        //public Role Role { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; }

        [Display(Name = "First Name")]
        public string UserFirstName { get; set; }

        [Display(Name = "Last Name")]
        public string UserLastName { get; set; }

        //[Display(Name = "User Name")]
        //[System.ComponentModel.DataAnnotations.Compare("Email", ErrorMessage = "The user name and email must be the same.")]
        //public string UserName { get; set; }

        [EmailAddress]
        [Display(Name = "Email (= User Name)")]
        public string Email { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        //[Phone]
        //[Display(Name = "Phone")]
        //public string PhoneNumber { get; set; }

        //[Display(Name = "Phone Number Confirmed")]
        //public bool PhoneNumberConfirmed { get; set; }

        //[Display(Name = "Organisation")]
        //public string Organisation { get; set; }

        [Display(Name = "Country")]
        public int CountryId { get; set; }
        public SelectList Countries { get; set; }

        [Display(Name = "Account Expiry Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "Lockout End Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? LockoutEndDate { get; set; }

        [Display(Name = "Lockout Enabled")]
        public bool LockoutEnabled { get; set; }

        //[Display(Name = "Last Login Date")]
        //[DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        //public DateTime LastLoginDate { get; set; }

        [Display(Name = "Two Factor Enabled")]
        public bool TwoFactorEnabled { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
