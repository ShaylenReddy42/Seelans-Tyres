using SeelansTyres.Data.Models;

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

	public async Task<bool> AddNewTyreAsync(CreateTyreModel tyre)
	{
		try
		{
			_ = await client.PostAsync("api/tyres", JsonContent.Create(tyre));
			return true;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex.Message);
			return false;
		}
	}

	public async Task<IEnumerable<BrandModel>> GetAllBrandsAsync()
	{
		try
		{
			var response = await client.GetAsync("api/brands");
			var brands = await response.Content.ReadFromJsonAsync<IEnumerable<BrandModel>>();

			return brands!;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex.Message);
			return new List<BrandModel>();
		}
	}

	public async Task<IEnumerable<TyreModel>> GetAllTyresAsync()
	{
        try
        {
            var response = await client.GetAsync("api/tyres");
            var tyres = await response.Content.ReadFromJsonAsync<IEnumerable<TyreModel>>();

            return tyres!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            return new List<TyreModel>();
        }
    }

	public async Task<TyreModel?> GetTyreByIdAsync(int tyreId)
	{
		try
		{
			var response = await client.GetAsync($"api/tyres/{tyreId}");
			var tyre = await response.Content.ReadFromJsonAsync<TyreModel>();

			return tyre!;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex.Message);
			return null;
		}
	}

	public async Task<bool> UpdateTyreAsync(int tyreId, CreateTyreModel tyre)
	{
		try
		{
			_ = await client.PutAsync($"api/tyres/{tyreId}", JsonContent.Create(tyre));
			return true;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex.Message);
			return false;
		}
	}
}
