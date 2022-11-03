namespace SeelansTyres.Frontends.Mvc.Services;

public class TyresService : ITyresService
{
	private readonly HttpClient client;
	private readonly ILogger<TyresService> logger;

	public TyresService(
		HttpClient client,
		IHttpContextAccessor httpContextAccessor,
		ILogger<TyresService> logger)
	{
		this.client = client;
		this.logger = logger;

        if (httpContextAccessor.HttpContext!.User.Identity!.IsAuthenticated is true)
        {
            var roleClaim = httpContextAccessor.HttpContext!.User.Claims.SingleOrDefault(claim => claim.Type.EndsWith("role"));

            if (roleClaim is not null && roleClaim.Value is "Administrator")
            {
				logger.LogInformation("Currently logged in user is an administrator. Adding custom X-User-Role header to all requests");
				
				client.DefaultRequestHeaders.Add("X-User-Role", "Administrator");
            }
        }
    }

	public async Task<bool> CreateTyreAsync(TyreModel tyre)
	{
        logger.LogInformation("Service => Attempting to add a new tyre");

        try
		{
			_ = await client.PostAsync("api/tyres", JsonContent.Create(tyre));

            logger.LogInformation(
				"{announcement}: Attempt to add a new tyre completed successfully",
				"SUCCEEDED");

            return true;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(
				ex, 
				"{announcement}: The API is unavailable",
				"FAILED");

			return false;
		}
	}

	public async Task<IEnumerable<BrandModel>> RetrieveAllBrandsAsync()
	{
        logger.LogInformation("Service => Attempting to retrieve all brands");

        try
		{
			var response = await client.GetAsync("api/brands");
            response.EnsureSuccessStatusCode();

			var brands = await response.Content.ReadFromJsonAsync<IEnumerable<BrandModel>>();

            logger.LogInformation(
				"{announcement}: Attempt to retrieve all brands completed successfully with {brandsCount} brands",
				"SUCCEEDED", brands!.Count());

            return brands!;
		}
		catch (HttpRequestException ex)
		{
            logger.LogError(
                ex,
                "{announcement}: The API is unavailable",
                "FAILED");

            return new List<BrandModel>();
		}
	}

	public async Task<IEnumerable<TyreModel>> RetrieveAllTyresAsync(bool availableOnly = true)
	{
        string includingUnavailable = availableOnly is false ? " including unavailable" : "";

        logger.LogInformation(
            "Service => Attempting to retrieve all tyres{includingUnavailable}",
            includingUnavailable);

        try
        {
            var response = await client.GetAsync($"api/tyres?availableOnly={availableOnly}");
            response.EnsureSuccessStatusCode();

            var tyres = await response.Content.ReadFromJsonAsync<IEnumerable<TyreModel>>();

            logger.LogInformation(
				"{announcement}: Attempt to retrieve all tyres{includingUnavailable} completed successfully with {tyresCount} tyre(s)",
				"SUCCEEDED", includingUnavailable, tyres!.Count());

            return tyres!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "{announcement}: The API is unavailable",
                "FAILED");

            return new List<TyreModel>();
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
				"{announcement}: Attempt to retrieve tyre {tyreId} completed successfully",
				"SUCCEEDED", tyreId);

            return tyre!;
		}
		catch (HttpRequestException ex)
		{
            logger.LogError(
                ex,
                "{announcement}: The API is unavailable",
                "FAILED");

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
			_ = await client.PutAsync($"api/tyres/{tyreId}", JsonContent.Create(tyre));

            logger.LogInformation(
                "{announcement}: Attempt to update tyre {tyreId} completed successfully",
                "SUCCEEDED", tyreId);

            return true;
		}
		catch (HttpRequestException ex)
		{
            logger.LogError(
                ex,
                "{announcement}: The API is unavailable",
                "FAILED");

            return false;
		}
	}
}
