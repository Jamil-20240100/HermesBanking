using Microsoft.AspNetCore.Mvc;

namespace HermesBankingApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
