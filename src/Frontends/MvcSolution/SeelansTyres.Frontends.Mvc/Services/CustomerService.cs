using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using SeelansTyres.Frontends.Mvc.Extensions;
using System.Diagnostics;

namespace SeelansTyres.Frontends.Mvc.Services;

public class CustomerService : ICustomerService
{
    private readonly HttpContext httpContext;
    private readonly HttpClient client;
    private readonly IConfiguration configuration;
    private readonly IMemoryCache cache;
    private readonly ILogger<CustomerService> logger;
    private readonly Stopwatch stopwatch = new();

    public CustomerService(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<CustomerService> logger)
    {
        httpContext = httpContextAccessor.HttpContext!;
        this.client = client;
        this.configuration = configuration;
        this.cache = cache;
        this.logger = logger;
    }

    public async Task<(CustomerModel?, bool, List<string>)> CreateAsync(RegisterModel registerModel)
    {
        logger.LogInformation("Service => Attempting to create a new customer account");
        
        CustomerModel? customer = null;
        bool succeeded = default;
        List<string> errors = new();

        try
        {
            client.SetBearerToken(await GetClientAccessTokenAsync("CustomerService.createaccount"));
            var response = await client.PostAsync("api/customers", JsonContent.Create(await registerModel.EncryptAsync(client, configuration, logger)));

            if (response.IsSuccessStatusCode)
            {
                succeeded = true;

                customer = await response.Content.ReadFromJsonAsync<CustomerModel>();

                logger.LogInformation(
                    "{announcement}: Attempt to create a new customer account completed successfully. Customer {customerId} created",
                    "SUCCEEDED", customer!.Id);
            }
            else
            {
                logger.LogInformation(
                    "Customer with email {customerEmail} already exists",
                    "***REDACTED***");
                
                errors.Add($"Customer with email {registerModel.Email} already exists");
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(
                ex,
                "{announcement}: Attempt to create a new customer account was unsuccessful",
                "FAILED");
            
            errors.Add("The identity service is unavailable!");
        }

        return (customer, succeeded, errors);
    }

    public async Task<CustomerModel> RetrieveSingleAsync(Guid customerId)
    {
        logger.LogInformation(
            "Service => Attempting to retrieve customer {customerId}",
            customerId);
        
        if (cache.TryGetValue(customerId, out CustomerModel? customer) is false)
        {
            logger.LogInformation(
                "Customer {customerId} is not in the cache. Retrieving from downstream and adding it",
                customerId);
            
            var response = await client.GetAsync($"api/customers/{customerId}");

            if (response.IsSuccessStatusCode is false)
            {
                logger.LogError(
                    "{announcement}: Attempt to retrieve customer {customerId} from downstream was unsuccessful with status code {statusCode}",
                    "FAILED", customerId, response.StatusCode);

                customer = new CustomerModel
                {
                    Id = Guid.Empty
                };
            }
            else
            {
                customer = await response.Content.ReadFromJsonAsync<CustomerModel>();
            }

            var cacheEntryOptions =
                new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            cache.Set(customerId, customer, cacheEntryOptions);
        }

        if (customer!.Id == Guid.Empty)
        {
            cache.Remove(customerId);
        }

        return customer!;
    }

    public async Task<CustomerModel?> RetrieveSingleAsync(string email)
    {
        logger.LogInformation(
            "Service => Attempting to retrieve customer by email {customerEmail}",
            "***REDACTED***");
        
        client.SetBearerToken(await GetClientAccessTokenAsync("CustomerService.retrievesinglebyemail"));
        var response = await client.GetAsync($"api/customers?email={email}");

        var customer = response.IsSuccessStatusCode switch
        {
            true  => await response.Content.ReadFromJsonAsync<CustomerModel>(),
            false => null
        };

        return customer;
    }

    public async Task UpdateAsync(UpdateAccountModel updateAccountModel)
    {
        var customerId = Guid.Parse(httpContext.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        logger.LogInformation(
            "Service => Attempting to update account for customer {customerId}",
            customerId);

        var response = await client.PutAsync($"api/customers/{customerId}", JsonContent.Create(await updateAccountModel.EncryptAsync(client, configuration, logger)));

        // Upon success, remove the model from the cache because it's outdated
        if (response.IsSuccessStatusCode is true)
        {
            logger.LogInformation(
                "{announcement}: Attempt to update account for customer {customerId} completed successfully. Removing the old customer info from the cache",
                "SUCCEEDED", customerId);
            
            cache.Remove(customerId);
        }
        else
        {
            logger.LogError(
                "{announcement}: Attempt to update account for customer {customerId} was unsuccessful with status code {statusCode}",
                "FAILED", customerId, response.StatusCode);
        }
    }

    public async Task<bool> DeleteAsync(string password)
    {
        var customerId = Guid.Parse(httpContext.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        logger.LogInformation(
            "Service => Attempting to delete account for customer {customerId}",
            customerId);

        var passwordModel = new PasswordModel { Password = password };

        var response = await client.PostAsync($"api/customers/{customerId}/verifypassword", JsonContent.Create(await passwordModel.EncryptAsync(client, configuration, logger)));

        if (response.IsSuccessStatusCode is true)
        {
            await client.DeleteAsync($"api/customers/{customerId}");
            return true;
        }

        logger.LogError(
            "{announcement}: Attempt to delete account for customer {customerId} was unsuccessful with status code {statusCode}",
            "FAILED", customerId, response.StatusCode);

        return false;
    }

    public async Task ResetPasswordAsync(Guid customerId, string password)
    {
        logger.LogInformation(
            "Service => Attempting a reset password operation for customer {customerId}",
            customerId);
        
        client.SetBearerToken(await GetClientAccessTokenAsync("CustomerService.resetpassword"));

        var passwordModel = new PasswordModel { Password = password };

        var response = await client.PutAsync($"api/customers/{customerId}/resetpassword", JsonContent.Create(await passwordModel.EncryptAsync(client, configuration, logger)));

        if (response.IsSuccessStatusCode is false)
        {
            logger.LogError(
                "{announcement}: Reset password operation for customer {customerId} was unsuccessful",
                "FAILED", customerId);
        }

        logger.LogInformation(
                "{announcement}: Reset password operation for customer {customerId} completed successfully",
                "SUCCEEDED", customerId);
    }

    private async Task<string> GetClientAccessTokenAsync(string additionalScopes)
    {
        logger.LogInformation(
            "Attempting to retrieve an access token from IdentityServer4 using the client credentials flow with {additionalScopes} as additional scope(s)",
            additionalScopes);

        stopwatch.Start();
        
        var discoveryDocument = await client.GetDiscoveryDocumentAsync(configuration["IdentityServer"]);

        if (discoveryDocument.IsError is true)
        {
            stopwatch.Stop();
            
            logger.LogError(
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve the discovery document from IdentityServer4 was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);
        }

        var tokenResponse =
            await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    ClientId = configuration["ClientCredentials:ClientId"],
                    ClientSecret = configuration["ClientCredentials:ClientSecret"],
                    Address = discoveryDocument.TokenEndpoint,
                    Scope = $"SeelansTyresMvcBff.fullaccess {additionalScopes}"
                });

        stopwatch.Start();

        if (tokenResponse.IsError is true)
        {
            logger.LogError(
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve an access token was unsuccessful",
                "FAILED", stopwatch.ElapsedMilliseconds);
        }
        else
        {
            logger.LogInformation(
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve an access token completed successfully",
                "SUCCEEDED", stopwatch.ElapsedMilliseconds);
        }

        return tokenResponse.AccessToken;
    }
}
