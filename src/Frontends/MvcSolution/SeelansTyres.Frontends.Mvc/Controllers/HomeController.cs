using SeelansTyres.Frontends.Mvc.HttpClients; // ITyresServiceClient
using SeelansTyres.Frontends.Mvc.Models;      // ErrorViewModel

namespace SeelansTyres.Frontends.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly ITyresServiceClient tyresServiceClient;

    public HomeController(
        ILogger<HomeController> logger,
        ITyresServiceClient tyresServiceClient)
    {
        this.logger = logger;
        this.tyresServiceClient = tyresServiceClient;
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
        logger.LogInformation("Controller => Retrieving all tyres");
        
        var tyres = await tyresServiceClient.RetrieveAllTyresAsync();

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