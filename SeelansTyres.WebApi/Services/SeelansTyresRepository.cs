using Microsoft.EntityFrameworkCore;
using SeelansTyres.WebApi.Data;
using SeelansTyres.Data.Entities;

namespace SeelansTyres.WebApi.Services;

public class SeelansTyresRepository : ISeelansTyresRepository
{
    private readonly SeelansTyresContext context;

    public SeelansTyresRepository(
        SeelansTyresContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<Address>?> GetAddressesForCustomerAsync(Guid customerId)
    {
        var customer = await context.Customers.FirstOrDefaultAsync(customer => customer.Id == customerId);

        if (customer is not null)
        {
            return customer.Addresses.ToList();
        }

        return null;
    }

    public async Task<IEnumerable<Brand>> GetAllBrandsAsync() =>
        await context.Brands.ToListAsync();

    public async Task<IEnumerable<Tyre>> GetAllTyresAsync() => 
        await context.Tyres.Include(tyre => tyre.Brand).ToListAsync();

    public async Task<Tyre?> GetTyreById(int tyreId) => 
        await context.Tyres.Include(tyre => tyre.Brand).FirstOrDefaultAsync(tyre => tyre.Id == tyreId);

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}
