using IdentityModel.Client;                    // SetBearerToken()
using SeelansTyres.Frontends.Mvc.Extensions;   // EncryptAsync()
using SeelansTyres.Frontends.Mvc.Services;     // ICacheService
using SeelansTyres.Libraries.Shared.Constants; // LoggerConstants

namespace SeelansTyres.Frontends.Mvc.HttpClients;

public class CustomerServiceClient(
    HttpClient client,
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    ICacheService cacheService,
    ILogger<CustomerServiceClient> logger,
    IClientCredentialsAuthenticationClient clientCredentialsAuthenticationClient) : ICustomerServiceClient
{
    public async Task<(CustomerModel?, bool, List<string>)> CreateAsync(RegisterModel registerModel)
    {
        logger.LogInformation("Service => Attempting to create a new customer account");

        CustomerModel? customer = null;
        bool succeeded = default;
        List<string> errors = [];

        try
        {
            var accessToken = (await clientCredentialsAuthenticationClient.RetrieveAccessTokenAsync("CustomerService.createaccount"))
                           ?? throw new InvalidOperationException("IdentityServer is unavailable for retrieving an access token using client credentials flow");
            
            client.SetBearerToken(accessToken);

            var response = await client.PostAsync("api/customers", JsonContent.Create(await registerModel.EncryptAsync(client, configuration, logger)));

            if (response.IsSuccessStatusCode)
            {
                succeeded = true;

                customer = await response.Content.ReadFromJsonAsync<CustomerModel>();

                logger.LogInformation(
                    "{Announcement}: Attempt to create a new customer account completed successfully. Customer {CustomerId} created",
                    LoggerConstants.SucceededAnnouncement, customer!.Id);
            }
            else
            {
                logger.LogInformation(
                    "Customer with email {CustomerEmail} already exists",
                    LoggerConstants.Redacted);

                errors.Add($"Customer with email {registerModel.Email} already exists");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "{Announcement}: Attempt to create a new customer account was unsuccessful",
                LoggerConstants.FailedAnnouncement);

            errors.Add("The identity service is unavailable!");
        }

        return (customer, succeeded, errors);
    }

    public async Task<CustomerModel> RetrieveSingleAsync(Guid customerId)
    {
        logger.LogInformation(
            "Service => Attempting to retrieve customer {CustomerId}",
            customerId);

        CustomerModel? customer = null;

        try
        {
            customer = await cacheService.RetrieveAsync<CustomerModel>(customerId.ToString());
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Cache is unavailable");
        }

        if (customer is null)
        {
            logger.LogInformation(
                "Customer {CustomerId} is not in the cache. Retrieving from downstream and adding it",
                customerId);

            var response = await client.GetAsync($"api/customers/{customerId}");

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError(
                    "{Announcement}: Attempt to retrieve customer {CustomerId} from downstream was unsuccessful with status code {StatusCode}",
                    LoggerConstants.FailedAnnouncement, customerId, response.StatusCode);

                customer = new CustomerModel
                {
                    Id = Guid.Empty
                };
            }
            else
            {
                customer = await response.Content.ReadFromJsonAsync<CustomerModel>();
            }

            await cacheService.SetAsync(customerId.ToString(), customer, 5, null);
        }

        if (customer?.Id == Guid.Empty)
        {
            await cacheService.DeleteAsync(customerId.ToString());
        }

        return customer!;
    }

    public async Task<CustomerModel?> RetrieveSingleAsync(string email)
    {
        logger.LogInformation(
            "Service => Attempting to retrieve customer by email {CustomerEmail}",
            LoggerConstants.Redacted);

        var accessToken = await clientCredentialsAuthenticationClient.RetrieveAccessTokenAsync("CustomerService.retrievesinglebyemail");

        if (accessToken is null)
        {
            logger.LogWarning("Couldn't retrieve an access token, aborting");
            
            return null;
        }

        client.SetBearerToken(accessToken);

        var response = await client.GetAsync($"api/customers?email={email}");

        var customer = response.IsSuccessStatusCode switch
        {
            true => await response.Content.ReadFromJsonAsync<CustomerModel>(),
            false => null
        };

        return customer;
    }

    public async Task UpdateAsync(UpdateAccountModel updateAccountModel)
    {
        var customerId = Guid.Parse(httpContextAccessor.HttpContext!.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        logger.LogInformation(
            "Service => Attempting to update account for customer {CustomerId}",
            customerId);

        var response = await client.PutAsync($"api/customers/{customerId}", JsonContent.Create(await updateAccountModel.EncryptAsync(client, configuration, logger)));

        // Upon success, remove the model from the cache because it's outdated
        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation(
                "{Announcement}: Attempt to update account for customer {CustomerId} completed successfully. Removing the old customer info from the cache",
                LoggerConstants.SucceededAnnouncement, customerId);

            await cacheService.DeleteAsync(customerId.ToString());
        }
        else
        {
            logger.LogError(
                "{Announcement}: Attempt to update account for customer {CustomerId} was unsuccessful with status code {StatusCode}",
                LoggerConstants.FailedAnnouncement, customerId, response.StatusCode);
        }
    }

    public async Task<bool> DeleteAsync(string password)
    {
        var customerId = Guid.Parse(httpContextAccessor.HttpContext!.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        logger.LogInformation(
            "Service => Attempting to delete account for customer {CustomerId}",
            customerId);

        var passwordModel = new PasswordModel { Password = password };

        var response = await client.PostAsync($"api/customers/{customerId}/verifypassword", JsonContent.Create(await passwordModel.EncryptAsync(client, configuration, logger)));

        if (response.IsSuccessStatusCode)
        {
            await client.DeleteAsync($"api/customers/{customerId}");
            return true;
        }

        logger.LogError(
            "{Announcement}: Attempt to delete account for customer {CustomerId} was unsuccessful with status code {StatusCode}",
            LoggerConstants.FailedAnnouncement, customerId, response.StatusCode);

        return false;
    }

    public async Task ResetPasswordAsync(Guid customerId, string password)
    {
        logger.LogInformation(
            "Service => Attempting a reset password operation for customer {CustomerId}",
            customerId);

        var accessToken = await clientCredentialsAuthenticationClient.RetrieveAccessTokenAsync("CustomerService.resetpassword");

        if (accessToken is null)
        {
            logger.LogWarning("Couldn't retrieve an access token, aborting");

            return;
        }

        client.SetBearerToken(accessToken);

        var passwordModel = new PasswordModel { Password = password };

        var response = await client.PutAsync($"api/customers/{customerId}/resetpassword", JsonContent.Create(await passwordModel.EncryptAsync(client, configuration, logger)));

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError(
                "{Announcement}: Reset password operation for customer {CustomerId} was unsuccessful",
                LoggerConstants.FailedAnnouncement, customerId);
        }
        else
        {
            logger.LogInformation(
                "{Announcement}: Reset password operation for customer {CustomerId} completed successfully",
                LoggerConstants.SucceededAnnouncement, customerId);
        }
    }
}
