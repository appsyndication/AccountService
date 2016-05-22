using System.ComponentModel.DataAnnotations;

namespace AppSyndication.AccountService.Web.Models
{
    public class RequestPasswordUpdate
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}