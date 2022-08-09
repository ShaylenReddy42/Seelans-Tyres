﻿using Microsoft.AspNetCore.Authorization;
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

        var jsonContent = JsonContent.Create(createTyreModel);

        try
        {
            _ = await client.PostAsync("api/tyres", jsonContent);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            ModelState.AddModelError(string.Empty, "API is not available");
            return View(model);
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> UpdateTyre(int tyreId)
    {
        var response = await client.GetAsync($"api/tyres/{tyreId}");

        var model = await response.Content.ReadFromJsonAsync<TyreModel>();
        
        var mvcTyreModel = new MvcTyreModel
        {
            Id = model!.Id,
            Name = model.Name,
            Width = model.Width,
            Ratio = model.Ratio,
            Diameter = model.Diameter,
            VehicleType = model.VehicleType,
            Price = model.Price,
            Available = model.Available,
            OriginalImageUrl = model.ImageUrl,
            BrandId = model.Brand!.Id
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

        var jsonContent = JsonContent.Create(createTyreModel);

        try
        {
            _ = await client.PutAsync($"api/tyres/{model.Id}", jsonContent);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            ModelState.AddModelError(string.Empty, "API is not available");
            return View(model);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> MarkOrderAsDelivered(int orderId)
    {
        try
        {
            await client.PutAsync($"api/orders/{orderId}?delivered=true", new StringContent(""));
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
        }

        return RedirectToAction("Index");
    }
}
