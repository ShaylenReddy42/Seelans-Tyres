using Microsoft.AspNetCore.Mvc;

namespace SeelansTyres.Mvc.Controllers;

public class ShoppingController : Controller
{
    public IActionResult Cart()
    {
        return View();
    }
}
