using Microsoft.AspNetCore.Identity;
using SeelansTyres.Services.IdentityService.Data.Entities;
using System.Diagnostics;

namespace SeelansTyres.Services.IdentityService.Services;

public class CustomerService : ICustomerService
{
	private readonly UserManager<Customer> userManager;
	private readonly ILogger<CustomerService> logger;
	private readonly Stopwatch stopwatch = new();

	public CustomerService(
		UserManager<Customer> userManager,
		ILogger<CustomerService> logger)
	{
		this.userManager = userManager;
		this.logger = logger;
	}

	public async Task<Customer> CreateAsync(Customer customer, string password)
	{
		logger.LogInformation("Service => Attempting to create a new customer account");

		stopwatch.Start();
		try
		{
			await userManager.CreateAsync(customer, password);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			logger.LogError(
				ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to create a new customer account was unsuccessful",
				"FAILED", stopwatch.ElapsedMilliseconds);

			throw ex.GetBaseException();
		}
		stopwatch.Stop();

		logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to create a new customer account completed successfully",
			"SUCCEEDED", stopwatch.ElapsedMilliseconds);

		return customer;
	}

	public async Task<Customer> RetrieveSingleAsync(Guid customerId)
	{
		logger.LogInformation(
			"Service => Attempting to retrieve customer by Id {customerId}",
			customerId);
		
		Customer customer;

		stopwatch.Start();
		try
		{
			customer = await userManager.FindByIdAsync(customerId.ToString());
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			logger.LogError(
				ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve customer by Id {customerId} was unsuccessful",
				"FAILED", stopwatch.ElapsedMilliseconds, customerId);

			throw ex.GetBaseException();
		}
		stopwatch.Stop();

		logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve customer by Id {customerId} completed successfully",
			"SUCCEEDED", stopwatch.ElapsedMilliseconds, customerId);

		return customer;
	}

	public async Task<Customer?> RetrieveSingleAsync(string email)
	{
		logger.LogInformation(
			"Service => Attempting to retrieve customer by email {customerEmail}",
			"***REDACTED***");

		Customer? customer;

		stopwatch.Start();
		try
		{
			customer = await userManager.FindByEmailAsync(email);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			logger.LogError(
				"{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve customer by email {customerEmail} was unsuccessful",
				"FAILED", stopwatch.ElapsedMilliseconds, "***REDACTED***");

			throw ex.GetBaseException();
		}
		stopwatch.Stop();

		logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to retrieve customer by email {customerEmail} completed successfully",
			"SUCCEEDED", stopwatch.ElapsedMilliseconds, "***REDACTED***");

		return customer;
	}

	public async Task UpdateAsync(Guid customerId, UpdateAccountModel updateAccountModel)
	{
		logger.LogInformation(
			"Service => Attempting to update account for customer {customerId}",
			customerId);

		stopwatch.Start();
		try
		{
			var customer = await RetrieveSingleAsync(customerId);

			customer.FirstName = updateAccountModel.FirstName;
			customer.LastName = updateAccountModel.LastName;
			customer.PhoneNumber = updateAccountModel.PhoneNumber;

			await userManager.UpdateAsync(customer);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			logger.LogError(
				ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to update account for customer {customerId} was unsuccessful",
				"FAILED", stopwatch.ElapsedMilliseconds, customerId);

			throw ex.GetBaseException();
		}
		stopwatch.Stop();

		logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to update account for customer {customerId} completed successfully",
			"SUCCEEDED", stopwatch.ElapsedMilliseconds, customerId);
    }

	public async Task DeleteAsync(Customer customer)
	{
		logger.LogInformation(
			"Service => Attempting to delete account for customer {customerId}",
			customer.Id);

		stopwatch.Start();
		try
		{
			await userManager.DeleteAsync(customer);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			logger.LogError(
				ex,
                "{announcement} ({stopwatchElapsedTime}ms): Attempt to delete account for customer {customerId} was unsuccessful",
				"FAILED", stopwatch.ElapsedMilliseconds, customer.Id);

			throw ex.GetBaseException();
		}
		stopwatch.Stop();

		logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Attempt to delete account for customer {customerId} completed successfully",
			"SUCCEEDED", stopwatch.ElapsedMilliseconds, customer.Id);
	}

	public async Task<bool> VerifyPasswordAsync(Guid customerId, string password)
	{
		logger.LogInformation(
			"Service => Attempting a password verification process for customer {customerId}",
			customerId);

		bool passwordMatches = default;

		stopwatch.Start();
		try
		{
			passwordMatches = await userManager.CheckPasswordAsync(await RetrieveSingleAsync(customerId), password);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			logger.LogError(
				ex,
                "{announcement} ({stopwatchElapsedTime}ms): Password verification process for customer {customerId} was unsuccessful",
				"FAILED", stopwatch.ElapsedMilliseconds, customerId);

			throw ex.GetBaseException();
		}
		stopwatch.Stop();

		logger.LogInformation(
			"{announcement} ({stopwatchElapsedTime}ms): Password verification process for customer {customerId} completed successfully",
			"SUCCEEDED", stopwatch.ElapsedMilliseconds, customerId);

		return passwordMatches;
	}

	public async Task ResetPasswordAsync(Guid customerId, string password)
	{
		logger.LogInformation(
			"Service => Attempting a password reset operation for customer {customerId}",
			customerId);
		
		stopwatch.Start();
		try
		{
			var customer = await RetrieveSingleAsync(customerId);

			await userManager.RemovePasswordAsync(customer);
			await userManager.AddPasswordAsync(customer, password);
		}
		catch (Exception ex)
		{
			stopwatch.Stop();

			logger.LogError(
				ex,
                "{announcement} ({stopwatchElapsedTime}ms): Password reset operation for customer {customerId} was unsuccessful",
				"FAILED", stopwatch.ElapsedMilliseconds, customerId);

			throw ex.GetBaseException();
		}
		stopwatch.Stop();

		logger.LogInformation(
            "{announcement} ({stopwatchElapsedTime}ms): Password reset operation for customer {customerId} completed successfully",
			"SUCCEEDED", stopwatch.ElapsedMilliseconds, customerId);
	}
}
