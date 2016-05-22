using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;

namespace AppSyndication.AccountService.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return this.View();
        }

        [Authorize]
        public IActionResult About()
        {
            this.ViewData["Message"] = "Your application description page.";

            return this.View();
        }
    }
}
