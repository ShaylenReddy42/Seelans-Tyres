using SeelansTyres.Frontends.Mvc.Services; // ICartService

namespace SeelansTyres.Frontends.Mvc.Components;

/// <summary>
/// Provides the number of items in the cart for the user
/// </summary>
public class ShoppingCartSummary(
    ILogger<ShoppingCartSummary> logger,
    ICartService cartService) : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        logger.LogDebug("ViewComponent => Retrieving cart to extract the number of items in it");

        int cartItemCount;

        // This exception is allowed to bubble up, happens when redis is used for caching
        try
        {
            // awaiting RetrieveAsync breaks the code
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
