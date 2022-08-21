using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.Entities;
using SeelansTyres.WebApi.Data;

namespace SeelansTyres.WebApi.Services;

public class TyresRepository : ITyresRepository
{
    private readonly SeelansTyresContext context;

    public TyresRepository(SeelansTyresContext context) => 
        this.context = context;

    public async Task<IEnumerable<Brand>> GetAllBrandsAsync() =>
        await context.Brands.ToListAsync();

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

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
