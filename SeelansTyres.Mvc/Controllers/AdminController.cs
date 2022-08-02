using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IWebHostEnvironment environment;
    private readonly HttpClient client;

    public AdminController(
        ILogger<AdminController> logger,
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment environment)
    {
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
        this.environment = environment;
        client = httpClientFactory.CreateClient("SeelansTyresAPI");
    }
    
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult AddTyre()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> AddTyre(AddNewTyreModel model)
    {
        if (ModelState.IsValid is false)
        {
            return View(model);
        }

        // integrate with azure storage later on.
        // upload the file to azure storage, get the url
        // and set the ImageUrl

        var filePath = "/images/no-image.png";

        if (model.Image is not null)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
            
            filePath = Path.Combine(
                environment.WebRootPath,
                "images",
                fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.Image.CopyToAsync(fileStream);
            }

            filePath = $"/images/{fileName}";
        }

        var createTyreModel = new CreateTyreModel
        {
            Name = model.Name,
            Width = model.Width,
            Ratio = model.Ratio,
            Diameter = model.Diameter,
            VehicleType = model.VehicleType,
            Price = model.Price,
            Available = model.Available,
            ImageUrl = filePath,
            BrandId = model.BrandId
        };

        var jsonContent = JsonContent.Create(createTyreModel);

        var response = await client.PostAsync("api/tyres", jsonContent);

        return RedirectToAction("Index");
    }
}
