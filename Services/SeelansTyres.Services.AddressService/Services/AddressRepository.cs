using Microsoft.EntityFrameworkCore;
using SeelansTyres.Services.AddressService.Data.Entities;
using SeelansTyres.Services.AddressService.Data;

namespace SeelansTyres.Services.AddressService.Services;

public class AddressRepository : IAddressRepository
{
    private readonly AddressContext context;

    public AddressRepository(AddressContext context) => 
        this.context = context;

    public async Task<IEnumerable<Address>> RetrieveAllAsync(Guid customerId) =>
        await context.Addresses.Where(address => address.CustomerId == customerId).ToListAsync();

    public async Task<Address?> RetrieveSingleAsync(Guid customerId, Guid addressId) =>
        await context.Addresses.SingleOrDefaultAsync(address => address.Id == addressId && address.CustomerId == customerId);

    public async Task CreateAsync(Guid customerId, Address newAddress)
    {
        newAddress.CustomerId = customerId;

        if (newAddress.PreferredAddress is true)
        {
            await context.Addresses
                .Where(address => address.CustomerId == customerId)
                .ForEachAsync(address => address.PreferredAddress = false);
        }

        await context.Addresses.AddAsync(newAddress);
    }

    public async Task MarkAsPrefferedAsync(Guid customerId, Address addressToMarkAsPreferred)
    {
        await context.Addresses
            .Where(address => address.CustomerId == customerId)
            .ForEachAsync(address => address.PreferredAddress = false);

        addressToMarkAsPreferred.PreferredAddress = true;
    }

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
