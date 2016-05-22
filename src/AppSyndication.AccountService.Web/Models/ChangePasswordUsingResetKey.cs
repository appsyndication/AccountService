using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Mvc;

namespace AppSyndication.AccountService.Web.Models
{
    public class ChangePasswordUsingResetKey
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Reset confirmation must match password.")]
        public string ConfirmPassword { get; set; }

        [HiddenInput]
        public string Key { get; set; }
    }
}