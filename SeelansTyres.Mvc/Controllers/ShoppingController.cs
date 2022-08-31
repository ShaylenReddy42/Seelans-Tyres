using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RazorLight;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Services;
using SeelansTyres.Mvc.ViewModels;
using System.Reflection;

namespace SeelansTyres.Mvc.Controllers;

public class ShoppingController : Controller
{
    private readonly ILogger<ShoppingController> logger;
    private readonly UserManager<Customer> userManager;
    private readonly IAddressService addressService;
    private readonly ICartService cartService;
    private readonly IOrderService orderService;
    private readonly IEmailService emailService;

    public ShoppingController(
        ILogger<ShoppingController> logger,
        UserManager<Customer> userManager,
        IAddressService addressService,
        ICartService cartService,
        IOrderService orderService,
        IEmailService emailService)
    {
        this.logger = logger;
        this.userManager = userManager;
        this.addressService = addressService;
        this.cartService = cartService;
        this.orderService = orderService;
        this.emailService = emailService;
    }
    
    public async Task<IActionResult> Cart()
    {
        var cartItems = await cartService.RetrieveCartAsync();

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
        
        return View(cartViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> AddTyreToCart(int quantity, int tyreId)
    {
        var cartItem = new CreateCartItemModel
        {
            TyreId = tyreId,
            Quantity = quantity,
            CartId = HttpContext.Session.GetString("CartId")!
        };

        _ = await cartService.CreateItemAsync(cartItem);
        
        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveTyreFromCart(int itemId)
    {
        _ = await cartService.DeleteItemAsync(itemId);

        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        var cartItems = await cartService.RetrieveCartAsync();

        var customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        var addresses = await addressService.RetrieveAllAsync(customerId);

        var preferredAddressId = addresses!.Single(address => address.PreferredAddress is true).Id;

        var order = new CreateOrderModel()
        {
            CustomerId = customerId,
            AddressId = preferredAddressId,
            TotalPrice = cartItems!.Sum(item => item.TotalItemPrice)
        };

        foreach (var item in cartItems!)
        {
            order.OrderItems.Add(new CreateOrderItemModel
            {
                TyreId = item.Tyre!.Id,
                Quantity = item.Quantity
            });
        }

        var placedOrder = await orderService.CreateAsync(order);

        if (placedOrder is not null)
        {
            _ = await cartService.DeleteCartAsync();

            await emailService.SendReceiptAsync(placedOrder);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    public async Task<string> ViewReceipt(int orderId)
    {
        var order = await orderService.RetrieveSingleAsync(orderId);

        var engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(Assembly.GetExecutingAssembly(), "SeelansTyres.Mvc.Templates")
            .UseMemoryCachingProvider()
            .Build();

        return order is not null ? await engine.CompileRenderAsync("Receipt", order) : "";
    }
}
