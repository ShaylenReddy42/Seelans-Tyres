using Microsoft.EntityFrameworkCore;
using SeelansTyres.Services.TyresService.Data;
using SeelansTyres.Services.TyresService.Data.Entities;

namespace SeelansTyres.Services.TyresService.Services;

public class TyresRepository : ITyresRepository
{
    private readonly TyresContext context;

    public TyresRepository(TyresContext context) => 
        this.context = context;

    public async Task<IEnumerable<Brand>> RetrieveAllBrandsAsync() =>
        await context.Brands.ToListAsync();

    public async Task<IEnumerable<Tyre>> RetrieveAllTyresAsync(bool availableOnly) =>
        availableOnly switch
        {
            true  => await context.Tyres.Include(tyre => tyre.Brand).Where(tyre => tyre.Available).OrderBy(tyre => tyre.Name).ToListAsync(),
            false => await context.Tyres.Include(tyre => tyre.Brand).OrderBy(tyre => tyre.Name).ToListAsync()
        };

    public async Task<Tyre?> RetrieveSingleTyreAsync(Guid tyreId) =>
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
