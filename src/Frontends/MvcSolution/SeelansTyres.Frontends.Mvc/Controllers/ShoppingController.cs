﻿using Microsoft.AspNetCore.Authorization;      // Authorize
using RazorLight;                              // RazorLightEngineBuilder
using SeelansTyres.Frontends.Mvc.Channels;     // SendReceiptChannel
using SeelansTyres.Frontends.Mvc.HttpClients;  // IAddressServiceClient, ICustomerServiceClient, IOrderServiceClient
using SeelansTyres.Frontends.Mvc.Models;       // CartItemModel
using SeelansTyres.Frontends.Mvc.Services;     // ICartService
using SeelansTyres.Frontends.Mvc.ViewModels;   // CartViewModel
using SeelansTyres.Libraries.Shared.Constants; // LoggerConstants

namespace SeelansTyres.Frontends.Mvc.Controllers;

public class ShoppingController(
    ILogger<ShoppingController> logger,
    IAddressServiceClient addressServiceClient,
    ICartService cartService,
    ICustomerServiceClient customerServiceClient,
    IOrderServiceClient orderServiceClient,
    SendReceiptChannel sendReceiptChannel) : Controller
{
    private readonly Stopwatch stopwatch = new();

    public async Task<IActionResult> Cart()
    {
        stopwatch.Start();

        List<CartItemModel>? cartItems = [];

        try
        {
            cartItems = await cartService.RetrieveAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Cache is unavailable");
        }

        var numberOfAddresses = 0;

        if (User.Identity!.IsAuthenticated)
        {
            var customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

            numberOfAddresses = (await addressServiceClient.RetrieveAllAsync(customerId)).Count();
        }

        var cartViewModel = new CartViewModel
        {
            CartItems = cartItems,
            NumberOfAddresses = numberOfAddresses
        };

        stopwatch.Stop();

        logger.LogInformation(
            "Building the Cart View Model took {StopwatchElapsedTime}ms to complete",
            stopwatch.ElapsedMilliseconds);
        
        return View(cartViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddTyreToCart(int quantity, Guid tyreId, string tyreName, decimal tyrePrice)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(HomeController.Shop), "Home");
        }
        
        logger.LogInformation(
            "Controller => Adding tyre {TyreId} to cart with quantity {Quantity}",
            tyreId, quantity);
        
        var cartItem = new CartItemModel
        {
            TyreId = tyreId,
            TyreName = tyreName,
            TyrePrice = tyrePrice,
            Quantity = quantity
        };

        await cartService.CreateItemAsync(cartItem);
        
        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    public async Task<IActionResult> RemoveTyreFromCart(Guid tyreId)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction(nameof(Cart));
        }
        
        logger.LogInformation(
            "Controller => Removing tyre {TyreId} from cart",
            tyreId);

        await cartService.DeleteItemAsync(tyreId);

        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        stopwatch.Start();
        
        var cartItems = await cartService.RetrieveAsync();

        var customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        logger.LogInformation(
            "Controller => Attempting to place an order for customer {CustomerId}",
            customerId);

        var customer = customerServiceClient.RetrieveSingleAsync(customerId);
        var addresses = addressServiceClient.RetrieveAllAsync(customerId);

        await Task.WhenAll(
            Task.Run(() => customer),
            Task.Run(() => addresses));

        var preferredAddress = addresses.Result.Single(address => address.PreferredAddress);

        var order = new OrderModel
        {
            Id = 0,
            OrderPlaced = DateTime.Now,
            CustomerId = customerId.ToString(),
            FirstName = customer.Result.FirstName,
            LastName = customer.Result.LastName,
            Email = customer.Result.Email,
            PhoneNumber = customer.Result.PhoneNumber,
            AddressId = preferredAddress.Id,
            AddressLine1 = preferredAddress.AddressLine1,
            AddressLine2 = preferredAddress.AddressLine2,
            City = preferredAddress.City,
            PostalCode = preferredAddress.PostalCode,
            TotalPrice = cartItems!.Sum(item => item.TotalItemPrice)
        };

        foreach (var item in cartItems!)
        {
            order.OrderItems.Add(new()
            {
                Id = Guid.Empty,
                TyreId = item.TyreId,
                TyreName = item.TyreName,
                TyrePrice = item.TyrePrice,
                Quantity = item.Quantity
            });
        }

        var placedOrder = await orderServiceClient.CreateAsync(order);

        if (placedOrder is not null)
        {
            await cartService.DeleteAsync();

            logger.LogInformation(
                "{Announcement}: Attempt to place an order for customer {CustomerId} completed successfully",
                LoggerConstants.SucceededAnnouncement, customerId);

            logger.LogDebug("Controller => Writing the order to the channel for sending to the customer");

            await sendReceiptChannel
                .WriteToChannelAsync(
                    placedOrder,
                    Activity.Current!.TraceId.ToString(),
                    Activity.Current!.SpanId.ToString());

            stopwatch.Stop();

            logger.LogInformation(
                "Placing an order for customer {CustomerId} took {StopwatchElapsedTime}ms to complete",
                customerId, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            stopwatch.Stop();
            
            logger.LogInformation(
                "{Announcement}: Attempt to place an order for customer {CustomerId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, customerId);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    public async Task<string> ViewReceipt(int orderId)
    {
        if (!ModelState.IsValid) 
        {
            return string.Empty;
        }
        
        logger.LogInformation(
            "Controller => Attempting to retrieve order {OrderId}",
            orderId);
        
        var order = await orderServiceClient.RetrieveSingleAsync(orderId);

        var engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(Assembly.GetExecutingAssembly(), "SeelansTyres.Frontends.Mvc.Templates")
            .UseMemoryCachingProvider()
            .Build();

        if (order is null)
        {
            logger.LogWarning(
                "{Announcement}: Order {OrderId} does not exist!",
                "NULL", orderId);
            
            return string.Empty;
        }

        logger.LogInformation(
            "{Announcement}: Attempt to retrieve order {OrderId} completed successfully",
            LoggerConstants.SucceededAnnouncement, orderId);

        return await engine.CompileRenderAsync("Receipt", order);
    }
}
