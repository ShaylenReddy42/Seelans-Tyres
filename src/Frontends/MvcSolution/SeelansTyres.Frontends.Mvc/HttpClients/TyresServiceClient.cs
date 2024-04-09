namespace SeelansTyres.Frontends.Mvc.HttpClients;

public class TyresServiceClient(
    HttpClient client,
    ILogger<TyresServiceClient> logger) : ITyresServiceClient
{
    public async Task<IEnumerable<BrandModel>> RetrieveAllBrandsAsync()
    {
        logger.LogInformation("Service => Attempting to retrieve all brands");

        try
        {
            var response = await client.GetAsync("api/brands");
            response.EnsureSuccessStatusCode();

            var brands = await response.Content.ReadFromJsonAsync<IEnumerable<BrandModel>>();

            logger.LogInformation(
                "{Announcement}: Attempt to retrieve all brands completed successfully with {brandsCount} brands",
                "SUCCEEDED", brands!.Count());

            return brands!;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to retrieve all brands was unsuccessful",
                "FAILED");

            return [];
        }
    }

    public async Task<bool> CreateTyreAsync(TyreModel tyre)
    {
        logger.LogInformation("Service => Attempting to add a new tyre");

        try
        {
            var response = await client.PostAsync("api/tyres", JsonContent.Create(tyre));
            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "{Announcement}: Attempt to add a new tyre completed successfully",
                "SUCCEEDED");

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to add a new tyre was unsuccessful",
                "FAILED");

            return false;
        }
    }

    public async Task<IEnumerable<TyreModel>> RetrieveAllTyresAsync(bool availableOnly = true)
    {
        string includingUnavailable = !availableOnly ? " including unavailable" : "";

        logger.LogInformation(
            "Service => Attempting to retrieve all tyres{includingUnavailable}",
            includingUnavailable);

        try
        {
            var response = await client.GetAsync($"api/tyres?availableOnly={availableOnly}");
            response.EnsureSuccessStatusCode();

            var tyres = await response.Content.ReadFromJsonAsync<IEnumerable<TyreModel>>();

            logger.LogInformation(
                "{Announcement}: Attempt to retrieve all tyres{includingUnavailable} completed successfully with {tyresCount} tyre(s)",
                "SUCCEEDED", includingUnavailable, tyres?.Count() ?? 0);

            return tyres ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to retrieve all tyres{includingUnavailable} was unsuccessful",
                "FAILED", includingUnavailable);

            return [];
        }
    }

    public async Task<TyreModel?> RetrieveSingleTyreAsync(Guid tyreId)
    {
        logger.LogInformation(
            "Service => Attempting to retrieve tyre {tyreId}",
            tyreId);

        try
        {
            var response = await client.GetAsync($"api/tyres/{tyreId}");
            response.EnsureSuccessStatusCode();

            var tyre = await response.Content.ReadFromJsonAsync<TyreModel>();

            logger.LogInformation(
                "{Announcement}: Attempt to retrieve tyre {tyreId} completed successfully",
                "SUCCEEDED", tyreId);

            return tyre;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to retrieve tyre {tyreId} was unsuccessful",
                "FAILED", tyreId);

            return null;
        }
    }

    public async Task<bool> UpdateTyreAsync(Guid tyreId, TyreModel tyre)
    {
        logger.LogInformation(
            "Service => Attempting to update tyre {tyreId}",
            tyreId);

        try
        {
            var response = await client.PutAsync($"api/tyres/{tyreId}", JsonContent.Create(tyre));
            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "{Announcement}: Attempt to update tyre {tyreId} completed successfully",
                "SUCCEEDED", tyreId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to update tyre {tyreId} was unsuccessful",
                "FAILED");

            return false;
        }
    }

    public async Task<bool> DeleteTyreAsync(Guid tyreId)
    {
        logger.LogInformation(
            "Service => Attempting to delete tyre {tyreId}",
            tyreId);

        try
        {
            var response = await client.DeleteAsync($"api/tyres/{tyreId}");
            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "{Announcement}: Attempt to delete tyre {tyreId} completed successfully",
                "SUCCEEDED", tyreId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to delete tyre {tyreId} was unsuccessful",
                "FAILED", tyreId);

            return false;
        }
    }
}
