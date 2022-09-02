using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.Entities;
using SeelansTyres.WebApi.Data;

namespace SeelansTyres.WebApi.Services;

public class TyresRepository : ITyresRepository
{
    private readonly SeelansTyresContext context;

    public TyresRepository(SeelansTyresContext context) => 
        this.context = context;

    public async Task<IEnumerable<Brand>> RetrieveAllBrandsAsync() =>
        await context.Brands.ToListAsync();

    public async Task<IEnumerable<Tyre>> RetrieveAllTyresAsync(bool availableOnly) =>
        availableOnly switch
        {
            true  => await context.Tyres.Include(tyre => tyre.Brand).Where(tyre => tyre.Available).ToListAsync(),
            false => await context.Tyres.Include(tyre => tyre.Brand).ToListAsync()
        };

    public async Task<Tyre?> RetrieveSingleTyreAsync(int tyreId) =>
        await context.Tyres.Include(tyre => tyre.Brand).SingleOrDefaultAsync(tyre => tyre.Id == tyreId);

    public async Task CreateTyreAsync(Tyre tyreEntity)
    {
        var brand = await context.Brands.SingleAsync(brand => brand.Id == tyreEntity.BrandId);

        tyreEntity.Brand = brand;

        await context.Tyres.AddAsync(tyreEntity);
    }

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
