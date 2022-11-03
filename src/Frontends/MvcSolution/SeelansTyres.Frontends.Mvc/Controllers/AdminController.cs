using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Frontends.Mvc.Models;
using SeelansTyres.Frontends.Mvc.Services;
using SeelansTyres.Frontends.Mvc.ViewModels;
using System.Diagnostics;

namespace SeelansTyres.Frontends.Mvc.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> logger;
    private readonly IImageService imageService;
    private readonly IOrderService orderService;
    private readonly ITyresService tyresService;
    private readonly Stopwatch stopwatch = new();

    public AdminController(
        ILogger<AdminController> logger,
        IImageService imageService,
        IOrderService orderService,
        ITyresService tyresService)
    {
        this.logger = logger;
        this.imageService = imageService;
        this.orderService = orderService;
        this.tyresService = tyresService;
    }
    
    public async Task<IActionResult> Index()
    {
        stopwatch.Start();

        logger.LogInformation("Controller => Retrieving all brands");
        
        var brands = tyresService.RetrieveAllBrandsAsync();

        logger.LogInformation("Controller => Retrieving all orders");

        var orders = orderService.RetrieveAllAsync();

        logger.LogInformation("Controller => Retrieving all tyres including unavailable");

        var tyres = tyresService.RetrieveAllTyresAsync(false);

        await Task.WhenAll(brands, orders, tyres);

        var adminPortalViewModel = new AdminPortalViewModel
        {
            Brands = brands.Result,
            Orders = orders.Result,
            Tyres = tyres.Result
        };

        stopwatch.Stop();

        logger.LogInformation(
            "Building the Admin Portal View Model took {stopwatchElapsedTime}ms to complete",
            stopwatch.ElapsedMilliseconds);
        
        return View(adminPortalViewModel);
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

        logger.LogInformation("Controller => Administrator is attempting to add a new tyre");

        // integrate with azure storage later on.
        // upload the file to azure storage, get the url
        // and set the ImageUrl

        logger.LogInformation("Attempting to upload image");

        var imageUrl = await imageService.UploadAsync(model.Image, "/images/no-image.png");

        var createTyreModel = new TyreModel
        {
            Id = Guid.Empty,
            Name = model.Name,
            Width = model.Width,
            Ratio = model.Ratio,
            Diameter = model.Diameter,
            VehicleType = model.VehicleType,
            Price = model.Price,
            Available = model.Available,
            ImageUrl = imageUrl,
            BrandId = model.BrandId
        };

        var requestSucceeded = await tyresService.CreateTyreAsync(createTyreModel);

        if (requestSucceeded is false)
        {
            logger.LogError(
                "{announcement}: Attempt to add a new tyre was unsuccessful",
                "FAILED");
            
            ModelState.AddModelError(string.Empty, "API is not available");

            return View(model);
        }

        logger.LogInformation(
            "{announcement}: Attempt to add a new tyre completed successfully",
            "SUCCEEDED");

        return RedirectToAction("Index");
    }

    [HttpGet("Admin/UpdateTyre/{tyreId}")]
    public async Task<IActionResult> UpdateTyre(Guid tyreId)
    {
        logger.LogInformation(
            "Controller => Administrator is attempting to retrieve tyre {tyreId} for update",
            tyreId);

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

        logger.LogInformation(
            "Controller => Administrator is attempting to update tyre {tyreId}",
            model.Id);

        logger.LogInformation("Attempting to upload image");

        var imageUrl = await imageService.UploadAsync(model.Image, model.OriginalImageUrl!);

        var updateTyreModel = new TyreModel
        {
            Id = model.Id,
            Name = model.Name,
            Width = model.Width,
            Ratio = model.Ratio,
            Diameter = model.Diameter,
            VehicleType = model.VehicleType,
            Price = model.Price,
            Available = model.Available,
            ImageUrl = imageUrl,
            BrandId = model.BrandId
        };

        var requestSucceeded = await tyresService.UpdateTyreAsync(model.Id, updateTyreModel);

        if (requestSucceeded is false)
        {
            logger.LogError(
                "{announcement}: Attempt to update tyre {tyreId} was unsuccessful",
                "FAILED", model.Id);

            ModelState.AddModelError(string.Empty, "API is not available");
            return View(model);
        }

        logger.LogError(
            "{announcement}: Attempt to update tyre {tyreId} completed successfully",
            "SUCCEEDED", model.Id);

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> MarkOrderAsDelivered(int orderId)
    {
        logger.LogInformation(
            "Controller => Administrator is attempting to mark order {orderId} as delivered",
            orderId);
        
        _ = await orderService.MarkOrderAsDeliveredAsync(orderId);

        return RedirectToAction("Index");
    }
}
