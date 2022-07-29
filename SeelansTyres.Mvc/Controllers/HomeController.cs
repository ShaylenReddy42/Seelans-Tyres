using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Models;
using System.Diagnostics;

namespace SeelansTyres.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory httpClientFactory;

    public HomeController(
        ILogger<HomeController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        this.httpClientFactory = httpClientFactory;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Home Page";

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
        ViewData["Title"] = "Shop";

        var client = httpClientFactory.CreateClient("SeelansTyresAPI");

        var response = await client.GetAsync("api/tyres");
        response.EnsureSuccessStatusCode();

        var tyres = await response.Content.ReadFromJsonAsync<IEnumerable<TyreModel>>();
        
        return View(tyres);
    }

    public IActionResult Services()
    {
        ViewData["Title"] = "Services";
        
        return View();
    }

    public IActionResult About()
    {
        ViewData["Title"] = "About";
        
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}