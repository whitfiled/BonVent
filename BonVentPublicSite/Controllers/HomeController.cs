using Microsoft.AspNetCore.Mvc;

namespace BonVentPublicSite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
