using Microsoft.AspNetCore.Mvc;

namespace HermesBankingApp.Controllers
{
    public class CreditCardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
