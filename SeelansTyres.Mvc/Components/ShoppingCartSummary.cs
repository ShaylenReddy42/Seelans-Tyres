using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Mvc.Services;

namespace SeelansTyres.Mvc.Components;

public class ShoppingCartSummary : ViewComponent
{
    private readonly ILogger<ShoppingCartSummary> logger;
    private readonly ICartService cartService;

    public ShoppingCartSummary(
        ILogger<ShoppingCartSummary> logger,
        ICartService cartService) =>
            (this.logger, this.cartService) = (logger, cartService);

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
