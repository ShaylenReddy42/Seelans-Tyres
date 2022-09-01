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
        var cartItemCount = cartService.Retrieve().Count;
        
        ViewData["CartItemsCount"] = cartItemCount;

        return View();
    }
}
