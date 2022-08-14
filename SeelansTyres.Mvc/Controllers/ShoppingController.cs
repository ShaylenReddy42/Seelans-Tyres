using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RazorLight;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Services;
using SeelansTyres.Mvc.ViewModels;
using System.Net;
using System.Reflection;

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
        HttpResponseMessage response = null!;
        IEnumerable<CartItemModel>? cartItems = new List<CartItemModel>();

        try
        {
            response = await client.GetAsync($"api/cart/{HttpContext.Session.GetString("CartId")}");
            cartItems = await response.Content.ReadFromJsonAsync<IEnumerable<CartItemModel>>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
        }

        var numberOfAddresses = 0;

        if (User.Identity!.IsAuthenticated)
        {
            var customerId = (await userManager.GetUserAsync(User)).Id;

            try
            {
                response = await client.GetAsync($"api/customers/{customerId}/addresses");
                numberOfAddresses = (await response.Content.ReadFromJsonAsync<IEnumerable<AddressModel>>())!.Count();
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex.Message);
            }
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

        try
        {
            _ = await client.PostAsync("api/cart", jsonContent);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
        }
        
        return RedirectToAction("Cart");
    }

    [HttpPost]
    public async Task<IActionResult> RemoveTyreFromCart(int itemId)
    {
        try
        {
            _ = await client.DeleteAsync($"api/cart/{HttpContext.Session.GetString("CartId")}/items/{itemId}");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
        }

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

        var request = new HttpRequestMessage(HttpMethod.Post, "api/orders");
        request.Headers.Add("Authorization", $"Bearer {HttpContext.Session.GetString("ApiAuthToken")}");
        request.Content = jsonContent;

        response = await client.SendAsync(request);

        if (response.StatusCode is HttpStatusCode.Created)
        {
            await client.DeleteAsync($"api/cart/{HttpContext.Session.GetString("CartId")}");
            var orderId = (await response.Content.ReadFromJsonAsync<OrderModel>())!.Id;

            request = new HttpRequestMessage(HttpMethod.Get, $"api/orders/{orderId}");
            request.Headers.Add("Authorization", $"Bearer {HttpContext.Session.GetString("ApiAuthToken")}");

            response = await client.SendAsync(request);

            var orderModel = await response.Content.ReadFromJsonAsync<OrderModel>();

            await emailService.SendReceiptAsync(orderModel!);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    public async Task<string> ViewReceipt(int orderId)
    {
        OrderModel? order = null!;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"api/orders/{orderId}");
            request.Headers.Add("Authorization", $"Bearer {HttpContext.Session.GetString("ApiAuthToken")}");

            var response = await client.SendAsync(request);

            order = await response.Content.ReadFromJsonAsync<OrderModel>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
        }

        var engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(Assembly.GetExecutingAssembly(), "SeelansTyres.Mvc.Templates")
            .UseMemoryCachingProvider()
            .Build();

        return order is not null ? await engine.CompileRenderAsync("Receipt", order) : "";
    }
}
