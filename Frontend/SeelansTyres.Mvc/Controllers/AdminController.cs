using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Models;
using SeelansTyres.Mvc.Services;

namespace SeelansTyres.Mvc.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> logger;
    private readonly IWebHostEnvironment environment;
    private readonly IOrderService orderService;
    private readonly ITyresService tyresService;

    public AdminController(
        ILogger<AdminController> logger,
        IWebHostEnvironment environment,
        IOrderService orderService,
        ITyresService tyresService)
    {
        this.logger = logger;
        this.environment = environment;
        this.orderService = orderService;
        this.tyresService = tyresService;
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
    public async Task<IActionResult> AddTyre(MvcTyreModel model)
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

        var requestSucceeded = await tyresService.CreateTyreAsync(createTyreModel);

        if (requestSucceeded is false)
        {
            ModelState.AddModelError(string.Empty, "API is not available");
            return View(model);
        }

        return RedirectToAction("Index");
    }

    [HttpGet("Admin/UpdateTyre/{tyreId}")]
    public async Task<IActionResult> UpdateTyre(int tyreId)
    {
        var tyre = await tyresService.RetrieveSingleTyreAsync(tyreId);
        
        var mvcTyreModel = new MvcTyreModel
        {
            Id = tyre!.Id,
            Name = tyre.Name,
            Width = tyre.Width,
            Ratio = tyre.Ratio,
            Diameter = tyre.Diameter,
            VehicleType = tyre.VehicleType,
            Price = tyre.Price,
            Available = tyre.Available,
            OriginalImageUrl = tyre.ImageUrl,
            BrandId = tyre.Brand!.Id
        };
        
        return View(mvcTyreModel);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTyre(MvcTyreModel model)
    {
        if (ModelState.IsValid is false)
        {
            return View(model);
        }

        var filePath = model.OriginalImageUrl;

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
            ImageUrl = filePath!,
            BrandId = model.BrandId
        };

        var requestSucceeded = await tyresService.UpdateTyreAsync(model.Id, createTyreModel);

        if (requestSucceeded is false)
        {
            ModelState.AddModelError(string.Empty, "API is not available");
            return View(model);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> MarkOrderAsDelivered(int orderId)
    {
        _ = await orderService.MarkOrderAsDeliveredAsync(orderId);

        return RedirectToAction("Index");
    }
}
