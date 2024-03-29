﻿using Microsoft.AspNetCore.Authorization;     // Authorize
using SeelansTyres.Frontends.Mvc.HttpClients; // IOrderServiceClient, ITyresServiceClient
using SeelansTyres.Frontends.Mvc.Models;      // MvcTyreModel
using SeelansTyres.Frontends.Mvc.Services;    // IImageService
using SeelansTyres.Frontends.Mvc.ViewModels;  // AdminPortalViewModel

namespace SeelansTyres.Frontends.Mvc.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController(
    ILogger<AdminController> logger,
    IImageService imageService,
    IOrderServiceClient orderServiceClient,
    ITyresServiceClient tyresServiceClient) : Controller
{
    private readonly Stopwatch stopwatch = new();

    public async Task<IActionResult> Index()
    {
        stopwatch.Start();

        logger.LogInformation("Controller => Retrieving all brands");
        
        var brands = tyresServiceClient.RetrieveAllBrandsAsync();

        logger.LogInformation("Controller => Retrieving all orders");

        var orders = orderServiceClient.RetrieveAllAsync();

        logger.LogInformation("Controller => Retrieving all tyres including unavailable");

        var tyres = tyresServiceClient.RetrieveAllTyresAsync(false);

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
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        logger.LogInformation("Controller => Administrator is attempting to add a new tyre");

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

        var requestSucceeded = await tyresServiceClient.CreateTyreAsync(createTyreModel);

        if (!requestSucceeded)
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

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Admin/UpdateTyre/{tyreId}")]
    public async Task<IActionResult> UpdateTyre(Guid tyreId)
    {
        logger.LogInformation(
            "Controller => Administrator is attempting to retrieve tyre {tyreId} for update",
            tyreId);

        var tyre = await tyresServiceClient.RetrieveSingleTyreAsync(tyreId);
        
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
        if (!ModelState.IsValid)
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

        var requestSucceeded = await tyresServiceClient.UpdateTyreAsync(model.Id, updateTyreModel);

        if (!requestSucceeded)
        {
            logger.LogError(
                "{announcement}: Attempt to update tyre {tyreId} was unsuccessful",
                "FAILED", model.Id);

            ModelState.AddModelError(string.Empty, "API is not available");
            return View(model);
        }

        logger.LogInformation(
            "{announcement}: Attempt to update tyre {tyreId} completed successfully",
            "SUCCEEDED", model.Id);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTyre(Guid tyreId, string imageUrl)
    {
        logger.LogInformation(
            "Controller => Administrator is attempting to delete tyre {tyreId}",
            tyreId);

        var succeeded = await tyresServiceClient.DeleteTyreAsync(tyreId);

        if (succeeded)
        {
            await imageService.DeleteAsync(imageUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarkOrderAsDelivered(int orderId)
    {
        logger.LogInformation(
            "Controller => Administrator is attempting to mark order {orderId} as delivered",
            orderId);
        
        _ = await orderServiceClient.MarkOrderAsDeliveredAsync(orderId);

        return RedirectToAction(nameof(Index));
    }
}
