namespace SeelansTyres.Frontends.Mvc.HttpClients;

public class AddressServiceClient(
    HttpClient client,
    ILogger<AddressServiceClient> logger) : IAddressServiceClient
{
    public async Task<bool> CreateAsync(AddressModel address, Guid customerId)
    {
        logger.LogInformation(
            "Service => Attempting to add a new address for customer {customerId}",
            customerId);

        try
        {
            await client.PostAsync($"api/customers/{customerId}/addresses", JsonContent.Create(address));

            logger.LogInformation(
                "{Announcement}: Attempt to add a new address for customer {customerId} completed successfully",
                "SUCCEEDED", customerId);

            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: The API is unavailable",
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
                "{Announcement}: Attempt to retrieve all addresses for customer {customerId} completed successfully with {addressesCount} address(es)",
                "SUCCEEDED", customerId, addresses!.Count());

            return addresses!;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: The API is unavailable",
                "FAILED");

            return [];
        }
    }

    public async Task<bool> MarkAddressAsPreferredAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "Service => Attempting to mark address {addressId} as preferred for customer {customerId}",
            addressId, customerId);

        try
        {
            var response = await client.PutAsync($"api/customers/{customerId}/addresses/{addressId}?markAsPreferred=true", null);
            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "{Announcement}: Attempt to mark address {addressId} as preferred for customer {customerId} completed successfully",
                "SUCCEEDED", addressId, customerId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to mark address {addressId} as preferred for customer {customerId} was unsuccessful",
                "FAILED", addressId, customerId);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "Service => Attempting to delete address {addressId} for customer {customerId}",
            addressId, customerId);

        try
        {
            var response = await client.DeleteAsync($"api/customers/{customerId}/addresses/{addressId}");
            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "{Announcement}: Attempt to delete address {addressId} for customer {customerId} completed successfully",
                "SUCCEEDED", addressId, customerId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to delete address {addressId} for customer {customerId} was unsuccessful",
                "FAILED", addressId, customerId);

            return false;
        }
    }
}
