using Microsoft.Extensions.Caching.Memory;
using SeelansTyres.Mvc.Models;
using SeelansTyres.Mvc.Models.External;

namespace SeelansTyres.Mvc.Services;

public class CustomerService : ICustomerService
{
    private readonly HttpContext httpContext;
    private readonly HttpClient client;
    private readonly IMemoryCache cache;

    public CustomerService(
        HttpClient client,
        IHttpContextAccessor httpContextAccessor,
        IMemoryCache cache) => 
            (this.client, httpContext, this.cache) = (client, httpContextAccessor.HttpContext!, cache);

    public async Task<(CustomerModel?, bool, List<string>)> CreateAsync(RegisterModel registerModel)
    {
        CustomerModel? customer = null;
        bool succeeded = default;
        List<string> errors = new();

        try
        {
            var response = await client.PostAsync("api/customers", JsonContent.Create(registerModel));

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

        var response = await client.PutAsync($"api/customers/{customerId}", JsonContent.Create(updateAccountModel));

        // Upon success, remove the model from the cache because it's outdated
        if (response.IsSuccessStatusCode is true)
        {
            cache.Remove(customerId);
        }
    }

    public async Task<bool> DeleteAsync(string password)
    {
        var customerId = Guid.Parse(httpContext.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        var deleteAccountModel = new PasswordModel { Password = password };

        var response = await client.PostAsync($"api/customers/{customerId}/verifypassword", JsonContent.Create(deleteAccountModel));

        if (response.IsSuccessStatusCode)
        {
            await client.DeleteAsync($"api/customers/{customerId}");
            return true;
        }

        return false;
    }

    public async Task ResetPasswordAsync(Guid customerId, string password)
    {
        var deleteAccountModel = new PasswordModel { Password = password };

        await client.PutAsync($"api/customers/{customerId}/resetpassword", JsonContent.Create(deleteAccountModel));
    }
}
