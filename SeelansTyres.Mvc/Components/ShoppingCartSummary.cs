using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Services;

namespace SeelansTyres.Mvc.Components;

public class ShoppingCartSummary : ViewComponent
{
    private readonly HttpClient client;
    private readonly ILogger<ShoppingCartSummary> logger;
    private readonly ICartService cartService;

    public ShoppingCartSummary(
        ILogger<ShoppingCartSummary> logger,
        IHttpClientFactory httpClientFactory,
        ICartService cartService)
    {
        client = httpClientFactory.CreateClient("SeelansTyresAPI");
        this.logger = logger;
        this.cartService = cartService;
    }

    public IViewComponentResult Invoke()
    {
        var cartItemCount = GetCartItemsCountAsync();
        cartItemCount.Wait();
        
        ViewData["CartItemsCount"] = cartItemCount.Result;

        return View();
    }

    private async Task<int> GetCartItemsCountAsync()
    {
        var cartItems = await cartService.GetAllCartItemsAsync();

        return cartItems!.Count();
    }
}
