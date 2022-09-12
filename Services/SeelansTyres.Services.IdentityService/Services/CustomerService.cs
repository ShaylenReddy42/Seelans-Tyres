using Microsoft.AspNetCore.Identity;
using SeelansTyres.Services.IdentityService.Data.Entities;
using SeelansTyres.Services.IdentityService.Models;

namespace SeelansTyres.Services.IdentityService.Services;

public class CustomerService : ICustomerService
{
	private readonly UserManager<Customer> userManager;

	public CustomerService(UserManager<Customer> userManager)
	{
		this.userManager = userManager;
	}

	public async Task<Customer> CreateAsync(Customer customer, string password)
	{
		await userManager.CreateAsync(customer, password);

		return customer;
	}

    public async Task<Customer> RetrieveSingleAsync(Guid customerId) =>
		await userManager.FindByIdAsync(customerId.ToString());

	public async Task<Customer?> RetrieveSingleAsync(string email) =>
		await userManager.FindByEmailAsync(email);

	public async Task UpdateAsync(Guid customerId, UpdateAccountModel updateAccountModel)
	{
		var customer = await RetrieveSingleAsync(customerId);

		customer.FirstName = updateAccountModel.FirstName;
		customer.LastName = updateAccountModel.LastName;
		customer.PhoneNumber = updateAccountModel.PhoneNumber;
		
		await userManager.UpdateAsync(customer);
    }

    public async Task DeleteAsync(Customer customer) =>
		await userManager.DeleteAsync(customer);

    public async Task<bool> VerifyPasswordAsync(Guid customerId, string password) =>
		await userManager.CheckPasswordAsync(await RetrieveSingleAsync(customerId), password);

	public async Task ResetPasswordAsync(Guid customerId, string password)
	{
		var customer = await RetrieveSingleAsync(customerId);

		await userManager.RemovePasswordAsync(customer);
		await userManager.AddPasswordAsync(customer, password);
	}
}
