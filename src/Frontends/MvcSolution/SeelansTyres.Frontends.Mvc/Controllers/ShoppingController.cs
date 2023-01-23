using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RazorLight;
using SeelansTyres.Frontends.Mvc.Channels;
using SeelansTyres.Frontends.Mvc.Models;
using SeelansTyres.Frontends.Mvc.Services;
using SeelansTyres.Frontends.Mvc.ViewModels;
using System.Diagnostics;
using System.Reflection;

namespace SeelansTyres.Frontends.Mvc.Controllers;

public class ShoppingController : Controller
{
    private readonly ILogger<ShoppingController> logger;
    private readonly IAddressService addressService;
    private readonly ICartService cartService;
    private readonly ICustomerService customerService;
    private readonly IOrderService orderService;
    private readonly SendReceiptChannel sendReceiptChannel;
    private readonly Stopwatch stopwatch = new();

    public ShoppingController(
        ILogger<ShoppingController> logger,
        IAddressService addressService,
        ICartService cartService,
        ICustomerService customerService,
        IOrderService orderService,
        SendReceiptChannel sendReceiptChannel)
    {
        this.logger = logger;
        this.addressService = addressService;
        this.cartService = cartService;
        this.customerService = customerService;
        this.orderService = orderService;
        this.sendReceiptChannel = sendReceiptChannel;
    }
    
    public async Task<IActionResult> Cart()
    {
        stopwatch.Start();
        
        var cartItems = await cartService.RetrieveAsync();

        var numberOfAddresses = 0;

        if (User.Identity!.IsAuthenticated)
        {
            var customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

            numberOfAddresses = (await addressService.RetrieveAllAsync(customerId)).Count();
        }

        var cartViewModel = new CartViewModel
        {
            CartItems = cartItems,
            NumberOfAddresses = numberOfAddresses
        };

        stopwatch.Stop();

        logger.LogInformation(
            "Building the Cart View Model took {stopwatchElapsedTime}ms to complete",
            stopwatch.ElapsedMilliseconds);
        
        return View(cartViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddTyreToCart(int quantity, Guid tyreId, string tyreName, decimal tyrePrice)
    {
        logger.LogInformation(
            "Controller => Adding tyre {tyreId} to cart with quantity {quantity}",
            tyreId, quantity);
        
        var cartItem = new CartItemModel
        {
            TyreId = tyreId,
            TyreName = tyreName,
            TyrePrice = tyrePrice,
            Quantity = quantity
        };

        await cartService.CreateItemAsync(cartItem);
        
        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveTyreFromCart(Guid tyreId)
    {
        logger.LogInformation(
            "Controller => Removing tyre {tyreId} from cart",
            tyreId);

        await cartService.DeleteItemAsync(tyreId);

        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        stopwatch.Start();
        
        var cartItems = await cartService.RetrieveAsync();

        var customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        logger.LogInformation(
            "Controller => Attempting to place an order for customer {customerId}",
            customerId);

        var customer = customerService.RetrieveSingleAsync(customerId);
        var addresses = addressService.RetrieveAllAsync(customerId);

        await Task.WhenAll(
            Task.Run(() => customer),
            Task.Run(() => addresses));

        var preferredAddress = addresses.Result.Single(address => address.PreferredAddress is true);

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

        var placedOrder = await orderService.CreateAsync(order);

        if (placedOrder is not null)
        {
            await cartService.DeleteAsync();

            logger.LogInformation(
                "{announcement}: Attempt to place an order for customer {customerId} completed successfully",
                "SUCCEEDED", customerId);

            logger.LogInformation("Controller => Writing the order to the channel for sending to the customer");

            await sendReceiptChannel
                .WriteToChannelAsync(
                    placedOrder,
                    Activity.Current!.TraceId.ToString(),
                    Activity.Current!.SpanId.ToString());

            stopwatch.Stop();

            logger.LogInformation(
                "Placing an order for customer {customerId} took {stopwatchElapsedTime}ms to complete",
                customerId, stopwatch.ElapsedMilliseconds);
        }
        else
        {
            stopwatch.Stop();
            
            logger.LogInformation(
                "{announcement}: Attempt to place an order for customer {customerId} was unsuccessful",
                "FAILED", customerId);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    public async Task<string> ViewReceipt(int orderId)
    {
        logger.LogInformation(
            "Controller => Attempting to retrieve order {orderId}",
            orderId);
        
        var order = await orderService.RetrieveSingleAsync(orderId);

        var engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(Assembly.GetExecutingAssembly(), "SeelansTyres.Frontends.Mvc.Templates")
            .UseMemoryCachingProvider()
            .Build();

        if (order is null)
        {
            logger.LogWarning(
                "{announcement}: Order {orderId} does not exist!",
                "NULL", orderId);
            
            return string.Empty;
        }

        logger.LogInformation(
            "{SUCCEEDED}: Attempt to retrieve order {orderId} completed successfully",
            orderId);

        return await engine.CompileRenderAsync("Receipt", order);
    }
}
