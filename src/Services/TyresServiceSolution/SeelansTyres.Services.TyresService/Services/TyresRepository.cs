﻿using Microsoft.EntityFrameworkCore;                    // ToListAsync(), SingleAsync(), Include(), ExecuteDeleteAsync()
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
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve all brands was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve all brands completed successfully with {brandsCount} brands",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, brands.Count());

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
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to add a new tyre was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to add a new tyre completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds);
    }

    public async Task<IEnumerable<Tyre>> RetrieveAllTyresAsync(bool availableOnly)
    {
        string includingUnavailable = !availableOnly ? " including unavailable" : "";

        logger.LogInformation(
            "Repository => Attempting to retrieve all tyres{includingUnavailable}",
            includingUnavailable);
        
        IEnumerable<Tyre> tyres = Enumerable.Empty<Tyre>();

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
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve all tyres{includingUnavailable} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, includingUnavailable);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve all tyres{includingUnavailable} completed successfully with {tyresCount} tyre(s)",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, includingUnavailable, tyres.Count());

        return tyres;
    }

    public async Task<Tyre?> RetrieveSingleTyreAsync(Guid tyreId)
    {
        logger.LogInformation(
            "Repository => Attempting to retrieve tyre {tyreId}",
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
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve tyre {tyreId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, tyreId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve tyre {tyreId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, tyreId);

        return tyre;
    }

    public async Task DeleteTyreAsync(Guid tyreId)
    {
        logger.LogInformation(
            "Repository => Attempting to delete tyre {tyreId}",
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
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to delete tyre {tyreId} was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds, tyreId);

            throw ex.GetBaseException();
        }
        stopwatch.Stop();

        logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to delete tyre {tyreId} completed successfully",
            "SUCCEEDED", stopwatch.ElapsedMilliseconds, tyreId);
    }

    public async Task<bool> SaveChangesAsync() =>
        await context.SaveChangesAsync() >= 0;
}
