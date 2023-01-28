using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Frontends.Mvc.Services;

namespace SeelansTyres.Frontends.Mvc.Components;

public class ShoppingCartSummary : ViewComponent
{
    private readonly ILogger<ShoppingCartSummary> logger;
    private readonly ICartService cartService;

    public ShoppingCartSummary(
        ILogger<ShoppingCartSummary> logger,
        ICartService cartService)
    {
        this.logger = logger;
        this.cartService = cartService;
    }

    public IViewComponentResult Invoke()
    {
        logger.LogInformation("ViewComponent => Retrieving cart to extract the number of items in it");

        int cartItemCount;

        try
        {
            cartItemCount = cartService.RetrieveAsync().Result.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Cache is unavailable");

            cartItemCount = 0;
        }
        
        ViewData["CartItemsCount"] = cartItemCount;

        return View();
    }
}
