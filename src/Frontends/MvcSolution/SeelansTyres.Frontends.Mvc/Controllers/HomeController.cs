using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Frontends.Mvc.Models;
using SeelansTyres.Frontends.Mvc.Services;
using System.Diagnostics;

namespace SeelansTyres.Frontends.Mvc.Controllers;

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
        logger.LogInformation("Controller => Retrieving all tyres");
        
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
        var errorViewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };
        
        return View(errorViewModel);
    }
}