using Microsoft.EntityFrameworkCore;
using SeelansTyres.WebApi.Data;
using SeelansTyres.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace SeelansTyres.WebApi.Services;

public class SeelansTyresRepository : ISeelansTyresRepository
{
    private readonly SeelansTyresContext context;
    private readonly UserManager<Customer> userManager;

    public SeelansTyresRepository(
        SeelansTyresContext context,
        UserManager<Customer> userManager)
    {
        this.context = context;
        this.userManager = userManager;
    }

    #region Customer

    public async Task<bool> CheckIfCustomerExistsAsync(Guid customerId)
    {
        var customer = await userManager.FindByIdAsync(customerId.ToString());

        return customer is not null;
    }

    #endregion Customer

    #region Addresses

    public async Task<IEnumerable<Address>> GetAddressesForCustomerAsync(Guid customerId) => 
        await context.Addresses.Where(address => address.Customer!.Id == customerId).ToListAsync();

    public async Task<Address?> GetAddressForCustomerAsync(Guid customerId, int addressId) => 
        await context.Addresses.FirstOrDefaultAsync(address => address.Id == addressId && address.Customer!.Id == customerId);

    public async Task AddNewAddressForCustomerAsync(Guid customerId, Address newAddress)
    {
        newAddress.CustomerId = customerId;

        if (newAddress.PreferredAddress is true)
        {
            await context.Addresses
                .Where(address => address.Customer!.Id == customerId)
                .ForEachAsync(address => address.PreferredAddress = false);
        }
        
        await context.Addresses.AddAsync(newAddress);
    }

    public async Task MarkAsPrefferedAsync(Guid customerId, Address addressToMarkAsPreferred)
    {
        await context.Addresses
            .Where(address => address.Customer!.Id == customerId)
            .ForEachAsync(address => address.PreferredAddress = false);

        addressToMarkAsPreferred.PreferredAddress = true;
    }

    #endregion Addresses

    #region Brands

    public async Task<IEnumerable<Brand>> GetAllBrandsAsync() =>
        await context.Brands.ToListAsync();

    #endregion Brands

    #region Tyres

    public async Task<IEnumerable<Tyre>> GetAllTyresAsync() => 
        await context.Tyres.Include(tyre => tyre.Brand).ToListAsync();

    public async Task<Tyre?> GetTyreByIdAsync(int tyreId) => 
        await context.Tyres.Include(tyre => tyre.Brand).FirstOrDefaultAsync(tyre => tyre.Id == tyreId);

    public async Task AddNewTyreAsync(Tyre tyreEntity)
    {
        var brand = await context.Brands.FirstOrDefaultAsync(brand => brand.Id == tyreEntity.BrandId);

        tyreEntity.Brand = brand;

        await context.Tyres.AddAsync(tyreEntity);
    }

    #endregion Tyres

    #region Orders

    public async Task AddNewOrderAsync(Order newOrder)
    {
        newOrder.Customer = await userManager.FindByIdAsync(newOrder.CustomerId.ToString());
        newOrder.Address = await context.Addresses.SingleAsync(address => address.Id == newOrder.AddressId);

        newOrder.OrderItems
            .Select(async item => item.Tyre = await context.Tyres.SingleAsync(tyre => tyre.Id == item.TyreId));

        await context.Orders.AddAsync(newOrder);
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync(Guid? customerId, bool notDeliveredOnly)
    {
        var collection = notDeliveredOnly switch
        {
            true  => context.Orders
                .Include(order => order.Customer)
                .Include(order => order.Address)
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Tyre)
                        .ThenInclude(tyre => tyre!.Brand)
                .Where(order => order.Delivered == false),
            false => context.Orders
                .Include(order => order.Customer)
                .Include(order => order.Address)
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Tyre)
                        .ThenInclude(tyre => tyre!.Brand)
        };
        
        var orders = customerId switch
        {
            null => await collection
                .ToListAsync(),
            _    => await collection
                .Where(order => order.CustomerId == customerId)
                .ToListAsync()
        };

        return orders;
    }

    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        return await context.Orders
            .Include(order => order.Customer)
            .Include(order => order.Address)
            .Include(order => order.OrderItems)
                .ThenInclude(item => item.Tyre)
                    .ThenInclude(tyre => tyre!.Brand)
            .SingleOrDefaultAsync(order => order.Id == id);
    }

    #endregion Orders

    #region Cart

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

    #endregion Cart

    public async Task<bool> SaveChangesAsync() => 
        await context.SaveChangesAsync() >= 0;
}
