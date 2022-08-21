using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.Entities;
using SeelansTyres.WebApi.Data;

namespace SeelansTyres.WebApi.Services;

public class AddressRepository : IAddressRepository
{
    private readonly SeelansTyresContext context;
    private readonly UserManager<Customer> userManager;

    public AddressRepository(
        SeelansTyresContext context,
        UserManager<Customer> userManager) => 
            (this.context, this.userManager) = (context, userManager);

    public async Task<bool> CheckIfCustomerExistsAsync(Guid customerId)
    {
        var customer = await userManager.FindByIdAsync(customerId.ToString());

        return customer is not null;
    }

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

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
