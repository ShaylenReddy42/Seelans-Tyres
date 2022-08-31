using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;
using SeelansTyres.WebApi.Services;

namespace SeelansTyres.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly ILogger<CartController> logger;
    private readonly ICartRepository cartRepository;
    private readonly IMapper mapper;

    public CartController(
        ILogger<CartController> logger,
        ICartRepository cartRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.cartRepository = cartRepository;
        this.mapper = mapper;
    }

    [HttpGet("{cartId}")]
    public async Task<ActionResult<IEnumerable<CartItemModel>>> RetrieveCart(string cartId)
    {
        var cartItems = await cartRepository.RetrieveCartAsync(cartId);

        return Ok(mapper.Map<IEnumerable<CartItem>, IEnumerable<CartItemModel>>(cartItems));
    }

    [HttpPost]
    public async Task<ActionResult> CreateItem(CreateCartItemModel newItem)
    {
        var cartItem = mapper.Map<CreateCartItemModel, CartItem>(newItem);

        await cartRepository.CreateItemAsync(cartItem);

        await cartRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{cartId}/items/{itemId}")]
    public async Task<IActionResult> DeleteItem(int itemId)
    {
        var cartItem = await cartRepository.RetrieveSingleItemAsync(itemId);

        if (cartItem is null)
        {
            return NotFound();
        }

        cartRepository.DeleteItem(cartItem);

        await cartRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{cartId}")]
    public async Task<ActionResult> DeleteCart(string cartId)
    {
        var cartItems = await cartRepository.RetrieveCartAsync(cartId);

        if (cartItems.Count() is 0)
        {
            return NotFound();
        }

        cartRepository.DeleteCart(cartItems);

        await cartRepository.SaveChangesAsync();

        return NoContent();
    }
}
