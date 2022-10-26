using SeelansTyres.Frontends.Mvc.Models.External;

namespace SeelansTyres.Frontends.Mvc.Services;

public interface ICustomerService
{
    Task<(CustomerModel?, bool, List<string>)> CreateAsync(RegisterModel registerModel);
    Task<CustomerModel> RetrieveSingleAsync(Guid customerId);
    Task<CustomerModel?> RetrieveSingleAsync(string email);
    Task UpdateAsync(UpdateAccountModel updateAccountModel);
    Task<bool> DeleteAsync(string password);
    Task ResetPasswordAsync(Guid customerId, string password);
}
