using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

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
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {httpContextAccessor.HttpContext!.Session.GetString("ApiAuthToken")}");
    }

    public async Task<bool> AddNewAddressAsync(CreateAddressModel address, Guid customerId)
    {
        try
        {
            await client.PostAsync($"api/customers/{customerId}/addresses", JsonContent.Create(address));
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            return false;
        }
    }

    public async Task<IEnumerable<AddressModel>> GetAllAddressesForCustomerAsync(Guid customerId)
    {
        try
        {
            var response = await client.GetAsync($"api/customers/{customerId}/addresses");
            var addresses = await response.Content.ReadFromJsonAsync<IEnumerable<AddressModel>>();

            return addresses!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            return new List<AddressModel>();
        }
    }

    public async Task<bool> MarkAddressAsPreferredAsync(Guid customerId, int addressId)
    {
        try
        {
            await client.PutAsync($"api/customers/{customerId}/addresses/{addressId}?markAsPreferred=true", new StringContent(""));
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            return false;
        }
    }
}
