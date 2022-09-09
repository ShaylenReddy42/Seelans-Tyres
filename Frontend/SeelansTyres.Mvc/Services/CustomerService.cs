using Microsoft.AspNetCore.Identity;
using SeelansTyres.Mvc.Data.Entities;
using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.Services;

public class CustomerService : ICustomerService
{
    private readonly HttpContext httpContext;
    private readonly UserManager<Customer> userManager;

    public CustomerService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<Customer> userManager) =>
            (httpContext, this.userManager) = (httpContextAccessor.HttpContext!, userManager);

    public async Task<(Customer?, bool, List<string>)> CreateAsync(RegisterModel registerModel)
    {
        Customer? customer = null;
        bool succeeded = default;
        List<string> errors = new();

        try
        {
            customer = await userManager.FindByEmailAsync(registerModel.Email);

            if (customer is null)
            {
                customer = new Customer
                {
                    FirstName = registerModel.FirstName,
                    LastName = registerModel.LastName,
                    Email = registerModel.Email,
                    UserName = registerModel.Email,
                    PhoneNumber = registerModel.PhoneNumber
                };

                var result = await userManager.CreateAsync(customer, registerModel.Password);

                succeeded = result.Succeeded;

                result.Errors.ToList().ForEach(error => errors.Add(error.Description));
            }
            else
            {
                errors.Add($"Customer with email {registerModel.Email} already exists");
            }
        }
        catch (InvalidOperationException)
        {
            errors.Add("The database is unavailable!");
        }

        return (customer, succeeded, errors);
    }

    public async Task UpdateAsync(UpdateAccountModel updateAccountModel)
    {
        var customer = await userManager.GetUserAsync(httpContext.User);

        customer.FirstName = updateAccountModel.FirstName;
        customer.LastName = updateAccountModel.LastName;
        customer.PhoneNumber = updateAccountModel.PhoneNumber;

        await userManager.UpdateAsync(customer);
    }

    public async Task<bool> DeleteAsync(string password)
    {
        var customer = await userManager.GetUserAsync(httpContext.User);

        var passwordIsCorrect = await userManager.CheckPasswordAsync(customer, password);

        if (passwordIsCorrect is true)
        {
            await userManager.DeleteAsync(customer);
            return true;
        }

        return false;
    }
}
