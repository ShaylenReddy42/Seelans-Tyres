using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Models;
using System.Diagnostics;

namespace SeelansTyres.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly HttpClient client;

    public HomeController(
        ILogger<HomeController> logger,
        IHttpClientFactory httpClientFactory)
    {
        this.logger = logger;
        client = httpClientFactory.CreateClient("SeelansTyresAPI");
    }

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
        IEnumerable<TyreModel>? tyres = new List<TyreModel>();

        try
        {
            var response = await client.GetAsync("api/tyres");

            tyres = await response.Content.ReadFromJsonAsync<IEnumerable<TyreModel>>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
        }

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