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

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() >= 0;
    }
}
