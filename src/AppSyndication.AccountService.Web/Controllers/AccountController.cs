using System.Threading.Tasks;
using AppSyndication.AccountService.Web.Extensions;
using AppSyndication.AccountService.Web.Models;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;

namespace AppSyndication.AccountService.Web.Controllers
{
    [Route("[controller]")]
    public class AccountController : Controller
    {
        private readonly ILogger _log;

        public AccountController(ILoggerFactory loggerFactory)
        {
            _log = loggerFactory.CreateLogger<AccountController>();
        }

        [Authorize]
        public IActionResult Index()
        {
            return this.View(new AccountViewModel() { Name = this.User.Name(), Username = this.User.Username() });
        }

        [Authorize]
        [Route("[action]")]
        public IActionResult Profile(string alias)
        {
            return this.View();
        }

        [Route("[action]")]
        public async Task Signout()
        {
            await this.HttpContext.Authentication.SignOutAsync("Cookies");

            await this.HttpContext.Authentication.SignOutAsync("OpenIdConnect");
        }

        [Route("[action]")]
        public IActionResult Error()
        {
            _log.LogError(1, "Error from logging!");

            var error = this.HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (error != null)
            {
                _log.LogError(666, error.ToString());
            }

            return this.View();
        }
    }
}
