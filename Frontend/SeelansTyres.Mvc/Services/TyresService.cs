using SeelansTyres.Mvc.Models.External;

namespace SeelansTyres.Mvc.Services;

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
		client.DefaultRequestHeaders.Add("Authorization", $"Bearer {httpContextAccessor.HttpContext!.Session.GetString("ApiAuthToken")}");
	}

	public async Task<bool> CreateTyreAsync(TyreModel tyre)
	{
		try
		{
			_ = await client.PostAsync("api/tyres", JsonContent.Create(tyre));
			return true;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex, "The API is unavailable");
			return false;
		}
	}

	public async Task<IEnumerable<BrandModel>> RetrieveAllBrandsAsync()
	{
		try
		{
			var response = await client.GetAsync("api/brands");
			var brands = await response.Content.ReadFromJsonAsync<IEnumerable<BrandModel>>();

			return brands!;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex, "The API is unavailable");
			return new List<BrandModel>();
		}
	}

	public async Task<IEnumerable<TyreModel>> RetrieveAllTyresAsync(bool availableOnly = true)
	{
        try
        {
            var response = await client.GetAsync($"api/tyres?availableOnly={availableOnly}");
            var tyres = await response.Content.ReadFromJsonAsync<IEnumerable<TyreModel>>();

            return tyres!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The API is unavailable");
            return new List<TyreModel>();
        }
    }

	public async Task<TyreModel?> RetrieveSingleTyreAsync(Guid tyreId)
	{
		try
		{
			var response = await client.GetAsync($"api/tyres/{tyreId}");
			var tyre = await response.Content.ReadFromJsonAsync<TyreModel>();

			return tyre!;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex, "The API is unavailable");
			return null;
		}
	}

	public async Task<bool> UpdateTyreAsync(Guid tyreId, TyreModel tyre)
	{
		try
		{
			_ = await client.PutAsync($"api/tyres/{tyreId}", JsonContent.Create(tyre));
			return true;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex, "The API is unavailable");
			return false;
		}
	}
}
