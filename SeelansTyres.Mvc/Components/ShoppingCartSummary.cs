using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Components;

public class ShoppingCartSummary : ViewComponent
{
    private readonly HttpClient client;
    private readonly ILogger<ShoppingCartSummary> logger;

    public ShoppingCartSummary(
        ILogger<ShoppingCartSummary> logger,
        IHttpClientFactory httpClientFactory)
    {
        client = httpClientFactory.CreateClient("SeelansTyresAPI");
        this.logger = logger;
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

        return cartItems!.Count();
    }
}
