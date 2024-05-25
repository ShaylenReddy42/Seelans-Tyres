using Microsoft.EntityFrameworkCore;                    // ToListAsync(), SingleAsync(), Include(), ExecuteDeleteAsync()
using SeelansTyres.Libraries.Shared.Constants;          // LoggerConstants
using SeelansTyres.Services.TyresService.Data;          // TyresDbContext
using SeelansTyres.Services.TyresService.Data.Entities; // Brand, Tyre
using System.Diagnostics;                               // Stopwatch

namespace SeelansTyres.Services.TyresService.Services;

public class TyresRepository(
    TyresDbContext context,
    ILogger<TyresRepository> logger) : ITyresRepository
{
    private readonly Stopwatch stopwatch = new();

    public async Task<IEnumerable<Brand>> RetrieveAllBrandsAsync()
    {
        logger.LogInformation("Repository => Attempting to retrieve all brands");
        
        IEnumerable<Brand> brands;

        stopwatch.Start();
        try
        {
            brands = await context.Brands.ToListAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve all brands was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve all brands completed successfully with {BrandsCount} brands",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, brands.Count());

        return brands;
    }

    public async Task CreateTyreAsync(Tyre tyre)
    {
        logger.LogInformation("Repository => Attempting to add a new tyre");

        stopwatch.Start();
        try
        {
            var brand = await context.Brands.SingleAsync(brand => brand.Id == tyre.BrandId);

            tyre.Brand = brand;

            await context.Tyres.AddAsync(tyre);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to add a new tyre was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to add a new tyre completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds);
    }

    public async Task<IEnumerable<Tyre>> RetrieveAllTyresAsync(bool availableOnly)
    {
        string includingUnavailable = !availableOnly ? " including unavailable" : "";

        logger.LogInformation(
            "Repository => Attempting to retrieve all tyres{IncludingUnavailable}",
            includingUnavailable);
        
        IEnumerable<Tyre> tyres = [];

        stopwatch.Start();
        try
        {
            tyres = availableOnly switch
            {
                true => await context.Tyres.Include(tyre => tyre.Brand).Where(tyre => tyre.Available).OrderBy(tyre => tyre.Name).ToListAsync(),
                _    => await context.Tyres.Include(tyre => tyre.Brand).OrderBy(tyre => tyre.Name).ToListAsync()
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve all tyres{IncludingUnavailable} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, includingUnavailable);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve all tyres{IncludingUnavailable} completed successfully with {TyresCount} tyre(s)",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, includingUnavailable, tyres.Count());

        return tyres;
    }

    public async Task<Tyre?> RetrieveSingleTyreAsync(Guid tyreId)
    {
        logger.LogInformation(
            "Repository => Attempting to retrieve tyre {TyreId}",
            tyreId);

        Tyre? tyre = null;

        stopwatch.Start();
        try
        {
            tyre = await context.Tyres.Include(tyre => tyre.Brand).SingleOrDefaultAsync(tyre => tyre.Id == tyreId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve tyre {TyreId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, tyreId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to retrieve tyre {TyreId} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, tyreId);

        return tyre;
    }

    public async Task DeleteTyreAsync(Guid tyreId)
    {
        logger.LogInformation(
            "Repository => Attempting to delete tyre {TyreId}",
            tyreId);

        stopwatch.Start();
        try
        {
            await context.Tyres
                .Where(tyre => tyre.Id == tyreId)
                .ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            logger.LogError(
                ex,
                "{Announcement} ({StopwatchElapsedTime}ms): Attempt to delete tyre {TyreId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, stopwatch.ElapsedMilliseconds, tyreId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{Announcement} ({StopwatchElapsedTime}ms): Attempt to delete tyre {TyreId} completed successfully",
            LoggerConstants.SucceededAnnouncement, stopwatch.ElapsedMilliseconds, tyreId);
    }

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
