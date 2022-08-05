using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Components;

public class ShoppingCartSummary : ViewComponent
{
    private readonly HttpClient client;

    public ShoppingCartSummary(IHttpClientFactory httpClientFactory)
    {
        client = httpClientFactory.CreateClient("SeelansTyresAPI");
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
        var response = await client.GetAsync($"api/cart/{HttpContext.Session.GetString("CartId")}");

        var cartItems = await response.Content.ReadFromJsonAsync<IEnumerable<CartItemModel>>();

        return cartItems!.Count();
    }
}
