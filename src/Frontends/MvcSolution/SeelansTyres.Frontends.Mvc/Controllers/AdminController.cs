﻿using Microsoft.AspNetCore.Authorization;      // Authorize
using SeelansTyres.Frontends.Mvc.HttpClients;  // IOrderServiceClient, ITyresServiceClient
using SeelansTyres.Frontends.Mvc.Models;       // MvcTyreModel
using SeelansTyres.Frontends.Mvc.Services;     // IImageService
using SeelansTyres.Frontends.Mvc.ViewModels;   // AdminPortalViewModel
using SeelansTyres.Libraries.Shared.Constants; // LoggerConstants 

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

        logger.LogInformation("Controller => Retrieving all brands, orders and tyres [including unavailable]");
        
        var brands = tyresServiceClient.RetrieveAllBrandsAsync();

        var orders = orderServiceClient.RetrieveAllAsync();

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
            "Building the Admin Portal View Model took {StopwatchElapsedTime}ms to complete",
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
                "{Announcement}: Attempt to add a new tyre was unsuccessful",
                LoggerConstants.FailedAnnouncement);
            
            ModelState.AddModelError(string.Empty, "API is not available");

            return View(model);
        }

        logger.LogInformation(
            "{Announcement}: Attempt to add a new tyre completed successfully",
            LoggerConstants.SucceededAnnouncement);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("~/Admin/UpdateTyre/{TyreId}")]
    public async Task<IActionResult> UpdateTyre(Guid tyreId)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index));
        }
        
        logger.LogInformation(
            "Controller => Administrator is attempting to retrieve tyre {TyreId} for update",
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
            "Controller => Administrator is attempting to update tyre {TyreId}",
            model.Id);

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
                "{Announcement}: Attempt to update tyre {TyreId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, model.Id);

            ModelState.AddModelError(string.Empty, "API is not available");
            return View(model);
        }

        logger.LogInformation(
            "{Announcement}: Attempt to update tyre {TyreId} completed successfully",
            LoggerConstants.SucceededAnnouncement, model.Id);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTyre(Guid tyreId, string imageUrl)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index));
        }
        
        logger.LogInformation(
            "Controller => Administrator is attempting to delete tyre {TyreId}",
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
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Index));
        }
        
        logger.LogInformation(
            "Controller => Administrator is attempting to mark order {OrderId} as delivered",
            orderId);
        
        await orderServiceClient.MarkOrderAsDeliveredAsync(orderId);

        return RedirectToAction(nameof(Index));
    }
}
