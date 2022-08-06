using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Services;
using SeelansTyres.Mvc.ViewModels;
using System.Net;

namespace SeelansTyres.Mvc.Controllers;

public class ShoppingController : Controller
{
    private HttpClient client;
    private readonly ILogger<ShoppingController> logger;
    private readonly UserManager<Customer> userManager;
    private readonly IEmailService emailService;

    public ShoppingController(
        ILogger<ShoppingController> logger,
        IHttpClientFactory httpClientFactory,
        UserManager<Customer> userManager,
        IEmailService emailService)
    {
        client = httpClientFactory.CreateClient("SeelansTyresAPI");
        this.logger = logger;
        this.userManager = userManager;
        this.emailService = emailService;
    }
    
    public async Task<IActionResult> Cart()
    {
        var response = await client.GetAsync($"api/cart/{HttpContext.Session.GetString("CartId")}");

        var cartItems = await response.Content.ReadFromJsonAsync<IEnumerable<CartItemModel>>();

        var numberOfAddresses = 0;

        if (User.Identity!.IsAuthenticated)
        {
            var customerId = (await userManager.GetUserAsync(User)).Id;
            response = await client.GetAsync($"api/customers/{customerId}/addresses");

            numberOfAddresses = (await response.Content.ReadFromJsonAsync<IEnumerable<AddressModel>>())!.Count();
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

        var jsonContent = JsonContent.Create(cartItem);

        await client.PostAsync("api/cart", jsonContent);
        
        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveTyreFromCart(int itemId)
    {
        await client.DeleteAsync($"api/cart/{HttpContext.Session.GetString("CartId")}/items/{itemId}");

        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> Checkout()
    {
        var cartId = HttpContext.Session!.GetString("CartId");

        var response = await client.GetAsync($"api/cart/{cartId}");

        var cartItems = await response.Content.ReadFromJsonAsync<IEnumerable<CartItemModel>>();

        var customerId = (await userManager.GetUserAsync(User)).Id;

        response = await client.GetAsync($"api/customers/{customerId}/addresses");

        var addresses = await response.Content.ReadFromJsonAsync<IEnumerable<AddressModel>>();

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

        var jsonContent = JsonContent.Create(order);

        response = await client.PostAsync("api/orders", jsonContent);

        if (response.StatusCode is HttpStatusCode.Created)
        {
            await client.DeleteAsync($"api/cart/{HttpContext.Session.GetString("CartId")}");
            var orderId = (await response.Content.ReadFromJsonAsync<OrderModel>())!.Id;

            response = await client.GetAsync($"api/orders/{orderId}");

            var orderModel = await response.Content.ReadFromJsonAsync<OrderModel>();

            await emailService.SendReceiptAsync(orderModel!);
        }

        return RedirectToAction("Index", "Home");
    }
}
