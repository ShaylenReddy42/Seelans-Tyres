using SeelansTyres.Services.IdentityService.Data.Entities;
using SeelansTyres.Services.IdentityService.Models;

namespace SeelansTyres.Services.IdentityService.Services;

public interface ICustomerService
{
    Task<Customer> CreateAsync(Customer customer, string password);
    Task<Customer> RetrieveSingleAsync(Guid customerId);
    Task<Customer?> RetrieveSingleAsync(string email);
    Task UpdateAsync(Guid customerId, UpdateAccountModel updateAccountModel);
    Task DeleteAsync(Customer customer);

    Task<bool> VerifyPasswordAsync(Guid customerId, string password);
    Task ResetPasswordAsync(Guid customerId, string password);
}
