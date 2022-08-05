using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public interface ISeelansTyresRepository
{
    /***** Customer *****/
    Task<bool> CheckIfCustomerExistsAsync(Guid customerId);

    /***** Addresses *****/
    Task<IEnumerable<Address>> GetAddressesForCustomerAsync(Guid customerId);
    Task<Address?> GetAddressForCustomerAsync(Guid customerId, int addressId);
    Task AddNewAddressForCustomerAsync(Guid customerId, Address newAddress);
    Task MarkAsPrefferedAsync(Guid customerId, Address addressToMarkAsPreferred);

    /***** Brands *****/
    Task<IEnumerable<Brand>> GetAllBrandsAsync();

    /***** Tyres *****/
    Task<IEnumerable<Tyre>> GetAllTyresAsync();
    Task<Tyre?> GetTyreByIdAsync(int id);
    Task AddNewTyreAsync(Tyre tyreEntity);

    /***** Orders *****/
    Task AddNewOrderAsync(Order newOrder);
    Task<IEnumerable<Order>> GetAllOrdersAsync(Guid? customerId);
    Task<Order?> GetOrderByIdAsync(int id);

    /***** Cart *****/
    Task AddItemToCartAsync(CartItem cartItem);
    Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
    Task<IEnumerable<CartItem>> GetCartItemsByCartId(string cartId);
    void RemoveItemFromCart(CartItem cartItem);
    void RemoveCartById(IEnumerable<CartItem> cartItems);

    Task<bool> SaveChangesAsync();
}
