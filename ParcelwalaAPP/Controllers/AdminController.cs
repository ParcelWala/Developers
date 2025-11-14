using Microsoft.AspNetCore.Mvc;

namespace ParcelwalaAPP.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
