using Microsoft.AspNetCore.Mvc;

namespace DailyExpenseTracker.Controllers
{
    public class testController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
