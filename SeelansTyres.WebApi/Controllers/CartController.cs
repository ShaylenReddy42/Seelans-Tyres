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
    private readonly ISeelansTyresRepository repository;
    private readonly IMapper mapper;

    public CartController(
        ILogger<CartController> logger,
        ISeelansTyresRepository repository,
        IMapper mapper)
    {
        this.logger = logger;
        this.repository = repository;
        this.mapper = mapper;
    }

    [HttpGet("{cartId}")]
    public async Task<ActionResult<IEnumerable<CartItemModel>>> GetAllCartItemsByCartId(string cartId)
    {
        var cartItems = await repository.GetCartItemsByCartId(cartId);

        return Ok(mapper.Map<IEnumerable<CartItem>, IEnumerable<CartItemModel>>(cartItems));
    }

    [HttpPost]
    public async Task<ActionResult> AddCartItem(CreateCartItemModel newItem)
    {
        var cartItem = mapper.Map<CreateCartItemModel, CartItem>(newItem);

        await repository.AddItemToCartAsync(cartItem);

        await repository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{cartId}/items/{itemId}")]
    public async Task<IActionResult> RemoveItemFromCart(int itemId)
    {
        var cartItem = await repository.GetCartItemByIdAsync(itemId);

        if (cartItem is null)
        {
            return NotFound();
        }

        repository.RemoveItemFromCart(cartItem);

        await repository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{cartId}")]
    public async Task<ActionResult> RemoveCart(string cartId)
    {
        var cartItems = await repository.GetCartItemsByCartId(cartId);

        if (cartItems.Count() is 0)
        {
            return NotFound();
        }

        repository.RemoveCartById(cartItems);

        await repository.SaveChangesAsync();

        return NoContent();
    }
}
