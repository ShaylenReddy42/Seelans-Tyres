using SeelansTyres.Mvc.Data.Entities;
using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.Services;

public interface ICustomerService
{
    Task<(Customer?, bool, List<string>)> CreateAsync(RegisterModel registerModel);
    Task UpdateAsync(UpdateAccountModel updateAccountModel);
    Task<bool> DeleteAsync(string password);
}
