using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.Entities;
using SeelansTyres.WebApi.Data;

namespace SeelansTyres.WebApi.Services;

public class CartRepository : ICartRepository
{
    private readonly SeelansTyresContext context;

    public CartRepository(SeelansTyresContext context) => 
        this.context = context;

    public async Task AddItemToCartAsync(CartItem newCartItem)
    {
        var cartItem = await context
            .CartItems
            .FirstOrDefaultAsync(item => item.TyreId == newCartItem.TyreId && item.CartId == newCartItem.CartId);

        if (cartItem is null)
        {
            newCartItem.Tyre = await context.Tyres.SingleAsync(tyre => tyre.Id == newCartItem.TyreId);

            await context.CartItems.AddAsync(newCartItem);
        }
        else
        {
            cartItem.Quantity += newCartItem.Quantity;
        }
    }

    public async Task<IEnumerable<CartItem>> GetCartItemsByCartId(string cartId) =>
        await context.CartItems
        .Include(item => item.Tyre)
            .ThenInclude(tyre => tyre!.Brand)
        .Where(item => item.CartId == cartId)
        .ToListAsync();

    public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId) =>
        await context.CartItems
        .SingleOrDefaultAsync(item => item.Id == cartItemId);

    public void RemoveItemFromCart(CartItem cartItem) =>
        context.CartItems.Remove(cartItem);

    public void RemoveCartById(IEnumerable<CartItem> cartItems) =>
        context.CartItems.RemoveRange(cartItems);

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
