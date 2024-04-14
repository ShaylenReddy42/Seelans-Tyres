namespace SeelansTyres.Frontends.Mvc.HttpClients;

public class AddressServiceClient(
    HttpClient client,
    ILogger<AddressServiceClient> logger) : IAddressServiceClient
{
    public async Task<bool> CreateAsync(AddressModel address, Guid customerId)
    {
        logger.LogInformation(
            "Service => Attempting to add a new address for customer {CustomerId}",
            customerId);

        try
        {
            var response = await client.PostAsync($"api/customers/{customerId}/addresses", JsonContent.Create(address));
            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "{Announcement}: Attempt to add a new address for customer {CustomerId} completed successfully",
                "SUCCEEDED", customerId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to add a new address for customer {CustomerId} was unsuccessful",
                "FAILED", customerId);

            return false;
        }
    }

    public async Task<IEnumerable<AddressModel>> RetrieveAllAsync(Guid customerId)
    {
        logger.LogInformation(
            "Service => Attempting to retrieve all addresses for customer {CustomerId}",
            customerId);

        try
        {
            var response = await client.GetAsync($"api/customers/{customerId}/addresses");
            response.EnsureSuccessStatusCode();

            var addresses = await response.Content.ReadFromJsonAsync<IEnumerable<AddressModel>>();

            logger.LogInformation(
                "{Announcement}: Attempt to retrieve all addresses for customer {CustomerId} completed successfully with {AddressesCount} address(es)",
                "SUCCEEDED", customerId, addresses?.Count() ?? 0);

            return addresses ?? [];
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to retrieve all addresses for customer {CustomerId} was unsuccessful",
                "FAILED", customerId);

            return [];
        }
    }

    public async Task<bool> MarkAddressAsPreferredAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "Service => Attempting to mark address {AddressId} as preferred for customer {CustomerId}",
            addressId, customerId);

        try
        {
            var response = await client.PutAsync($"api/customers/{customerId}/addresses/{addressId}?markAsPreferred=true", null);
            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "{Announcement}: Attempt to mark address {AddressId} as preferred for customer {CustomerId} completed successfully",
                "SUCCEEDED", addressId, customerId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to mark address {AddressId} as preferred for customer {CustomerId} was unsuccessful",
                "FAILED", addressId, customerId);

            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "Service => Attempting to delete address {AddressId} for customer {CustomerId}",
            addressId, customerId);

        try
        {
            var response = await client.DeleteAsync($"api/customers/{customerId}/addresses/{addressId}");
            response.EnsureSuccessStatusCode();

            logger.LogInformation(
                "{Announcement}: Attempt to delete address {AddressId} for customer {CustomerId} completed successfully",
                "SUCCEEDED", addressId, customerId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to delete address {AddressId} for customer {CustomerId} was unsuccessful",
                "FAILED", addressId, customerId);

            return false;
        }
    }
}
