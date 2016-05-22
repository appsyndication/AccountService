using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Mvc;

namespace AppSyndication.AccountService.Web.Models
{
    public class ChangeEmailUsingResetKey
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

        [HiddenInput]
        public string Email { get; set; }

        public string Username { get; set; }
    }
}