using SeelansTyres.Frontends.Mvc.Models.External;

namespace SeelansTyres.Frontends.Mvc.Services;

public class AddressService : IAddressService
{
    private readonly HttpClient client;
    private readonly ILogger<AddressService> logger;

    public AddressService(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AddressService> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task<bool> CreateAsync(AddressModel address, Guid customerId)
    {
        try
        {
            await client.PostAsync($"api/customers/{customerId}/addresses", JsonContent.Create(address));
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The API is unavailable");
            return false;
        }
    }

    public async Task<IEnumerable<AddressModel>> RetrieveAllAsync(Guid customerId)
    {
        try
        {
            var response = await client.GetAsync($"api/customers/{customerId}/addresses");
            var addresses = await response.Content.ReadFromJsonAsync<IEnumerable<AddressModel>>();

            return addresses!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The API is unavailable");
            return new List<AddressModel>();
        }
    }

    public async Task<bool> MarkAddressAsPreferredAsync(Guid customerId, Guid addressId)
    {
        try
        {
            await client.PutAsync($"api/customers/{customerId}/addresses/{addressId}?markAsPreferred=true", new StringContent(""));
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The API is unavailable");
            return false;
        }
    }
}
