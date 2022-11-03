namespace SeelansTyres.Frontends.Mvc.Services;

public class AddressService : IAddressService
{
    private readonly HttpClient client;
    private readonly ILogger<AddressService> logger;

    public AddressService(
        HttpClient client,
        ILogger<AddressService> logger)
    {
        this.client = client;
        this.logger = logger;
    }

    public async Task<bool> CreateAsync(AddressModel address, Guid customerId)
    {
        logger.LogInformation(
            "Service => Attempting to add a new address for customer {customerId}",
            customerId);
        
        try
        {
            await client.PostAsync($"api/customers/{customerId}/addresses", JsonContent.Create(address));

            logger.LogInformation(
                "{announcement}: Attempt to add a new address for customer {customerId} completed successfully",
                "SUCCEEDED", customerId);

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

    public async Task<IEnumerable<AddressModel>> RetrieveAllAsync(Guid customerId)
    {
        logger.LogInformation(
            "Service => Attempting to retrieve all addresses for customer {customerId}",
            customerId);
        
        try
        {
            var response = await client.GetAsync($"api/customers/{customerId}/addresses");
            response.EnsureSuccessStatusCode();

            var addresses = await response.Content.ReadFromJsonAsync<IEnumerable<AddressModel>>();

            logger.LogInformation(
                "{announcement}: Attempt to retrieve all addresses for customer {customerId} completed successfully with {addressesCount} address(es)",
                "SUCCEEDED", customerId, addresses!.Count());

            return addresses!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex, 
                "{announcement}: The API is unavailable",
                "FAILED");
            
            return new List<AddressModel>();
        }
    }

    public async Task<bool> MarkAddressAsPreferredAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "Service => Attempting to mark address {addressId} as preferred for customer {customerId}",
            addressId, customerId);
        
        try
        {
            await client.PutAsync($"api/customers/{customerId}/addresses/{addressId}?markAsPreferred=true", new StringContent(""));

            logger.LogInformation(
                "{announcement}: Attempt to mark address {addressId} as preferred for customer {customerId} completed successfully",
                "SUCCEEDED", addressId, customerId);

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
