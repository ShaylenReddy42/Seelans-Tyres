using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Mvc.Models;
using SeelansTyres.Mvc.Services;
using System.Diagnostics;

namespace SeelansTyres.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly ITyresService tyresService;

    public HomeController(
        ILogger<HomeController> logger,
        ITyresService tyresService) => 
            (this.logger, this.tyresService) = (logger, tyresService);

    public IActionResult Index()
    {
        List<string> brands = new()
        {
            "bfgoodrich",
            "continental",
            "goodyear",
            "hankook",
            "michelin",
            "pirelli"
        };

        return View(brands);
    }

    public async Task<IActionResult> Shop()
    {
        var tyres = await tyresService.RetrieveAllTyresAsync();

        return View(tyres);
    }

    public IActionResult Services()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}