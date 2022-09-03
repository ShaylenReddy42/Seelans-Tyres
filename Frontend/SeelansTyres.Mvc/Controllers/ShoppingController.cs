using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RazorLight;
using SeelansTyres.Mvc.Data.Entities;
using SeelansTyres.Mvc.Models;
using SeelansTyres.Mvc.Models.External;
using SeelansTyres.Mvc.Services;
using SeelansTyres.Mvc.ViewModels;
using System.Reflection;

namespace SeelansTyres.Mvc.Controllers;

public class ShoppingController : Controller
{
    private readonly ILogger<ShoppingController> logger;
    private readonly IAddressService addressService;
    private readonly ICartService cartService;
    private readonly IOrderService orderService;
    private readonly IEmailService emailService;
    private readonly UserManager<Customer> userManager;

    public ShoppingController(
        ILogger<ShoppingController> logger,
        IAddressService addressService,
        ICartService cartService,
        IOrderService orderService,
        IEmailService emailService,
        UserManager<Customer> userManager)
    {
        this.logger = logger;
        this.addressService = addressService;
        this.cartService = cartService;
        this.orderService = orderService;
        this.emailService = emailService;
        this.userManager = userManager;
    }
    
    public async Task<IActionResult> Cart()
    {
        var cartItems = cartService.Retrieve();

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
    public IActionResult AddTyreToCart(int quantity, Guid tyreId, string tyreName, decimal tyrePrice)
    {
        var cartItem = new CachedCartItemModel
        {
            TyreId = tyreId,
            TyreName = tyreName,
            TyrePrice = tyrePrice,
            Quantity = quantity
        };

        cartService.CreateItem(cartItem);
        
        return RedirectToAction("Cart");
    }

    [HttpPost]
    public IActionResult RemoveTyreFromCart(Guid tyreId)
    {
        cartService.DeleteItem(tyreId);

        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        var cartItems = cartService.Retrieve();

        var customer = await userManager.GetUserAsync(User);

        var addresses = await addressService.RetrieveAllAsync(customer.Id);

        var preferredAddress = addresses!.Single(address => address.PreferredAddress is true);

        var order = new OrderModel
        {
            Id = 0,
            OrderPlaced = DateTime.Now,
            CustomerId = customer.Id.ToString(),
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            AddressId = preferredAddress.Id,
            AddressLine1 = preferredAddress.AddressLine1,
            AddressLine2 = preferredAddress.AddressLine2,
            City = preferredAddress.City,
            PostalCode = preferredAddress.PostalCode,
            TotalPrice = cartItems!.Sum(item => item.TotalItemPrice)
        };

        foreach (var item in cartItems!)
        {
            order.OrderItems.Add(new OrderItemModel
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
            cartService.Delete();

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
