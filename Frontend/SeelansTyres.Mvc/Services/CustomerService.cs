using IdentityModel.Client;
using Microsoft.Extensions.Caching.Memory;
using SeelansTyres.Mvc.Extensions;
using SeelansTyres.Mvc.Models;
using SeelansTyres.Mvc.Models.External;

namespace SeelansTyres.Mvc.Services;

public class CustomerService : ICustomerService
{
    private readonly HttpContext httpContext;
    private readonly HttpClient client;
    private readonly IConfiguration configuration;
    private readonly IMemoryCache cache;

    public CustomerService(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IMemoryCache cache) => 
            (this.client, httpContext, this.configuration, this.cache) = 
            (client, httpContextAccessor.HttpContext!, configuration, cache);

    public async Task<(CustomerModel?, bool, List<string>)> CreateAsync(RegisterModel registerModel)
    {
        CustomerModel? customer = null;
        bool succeeded = default;
        List<string> errors = new();

        try
        {
            client.SetBearerToken(await GetClientAccessTokenAsync("CustomerService.createaccount"));
            var response = await client.PostAsync("api/customers", JsonContent.Create(await registerModel.EncryptAsync(client)));

            if (response.IsSuccessStatusCode)
            {
                succeeded = true;

                customer = await response.Content.ReadFromJsonAsync<CustomerModel>();
            }
            else
            {
                errors.Add($"Customer with email {registerModel.Email} already exists");
            }
        }
        catch (HttpRequestException)
        {
            errors.Add("The identity service is unavailable!");
        }

        return (customer, succeeded, errors);
    }

    public async Task<CustomerModel> RetrieveSingleAsync(Guid customerId)
    {
        if (cache.TryGetValue(customerId, out CustomerModel? customer) is false)
        {
            var response = await client.GetAsync($"api/customers/{customerId}");

            customer = await response.Content.ReadFromJsonAsync<CustomerModel>();

            var cacheEntryOptions =
                new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            cache.Set(customerId, customer, cacheEntryOptions);
        }

        return customer!;
    }

    public async Task<CustomerModel?> RetrieveSingleAsync(string email)
    {
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

        var response = await client.PutAsync($"api/customers/{customerId}", JsonContent.Create(await updateAccountModel.EncryptAsync(client)));

        // Upon success, remove the model from the cache because it's outdated
        if (response.IsSuccessStatusCode is true)
        {
            cache.Remove(customerId);
        }
    }

    public async Task<bool> DeleteAsync(string password)
    {
        var customerId = Guid.Parse(httpContext.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        var passwordModel = new PasswordModel { Password = password };

        var response = await client.PostAsync($"api/customers/{customerId}/verifypassword", JsonContent.Create(await passwordModel.EncryptAsync(client)));

        if (response.IsSuccessStatusCode)
        {
            await client.DeleteAsync($"api/customers/{customerId}");
            return true;
        }

        return false;
    }

    public async Task ResetPasswordAsync(Guid customerId, string password)
    {
        client.SetBearerToken(await GetClientAccessTokenAsync("CustomerService.resetpassword"));

        var passwordModel = new PasswordModel { Password = password };

        await client.PutAsync($"api/customers/{customerId}/resetpassword", JsonContent.Create(await passwordModel.EncryptAsync(client)));
    }

    private async Task<string> GetClientAccessTokenAsync(string scope)
    {
        var discoveryDocument = await client.GetDiscoveryDocumentAsync();

        var tokenResponse =
            await client.RequestClientCredentialsTokenAsync(
                new ClientCredentialsTokenRequest
                {
                    ClientId = configuration["ClientCredentials:ClientId"],
                    ClientSecret = configuration["ClientCredentials:ClientSecret"],
                    Address = discoveryDocument.TokenEndpoint,
                    Scope = scope
                });

        return tokenResponse.AccessToken;
    }
}
