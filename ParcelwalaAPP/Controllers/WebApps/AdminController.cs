using Microsoft.AspNetCore.Mvc;

namespace ParcelwalaAPP.Controllers.WebApps
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
