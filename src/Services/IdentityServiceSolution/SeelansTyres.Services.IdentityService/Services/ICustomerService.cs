using SeelansTyres.Services.IdentityService.Data.Entities; // Customer

namespace SeelansTyres.Services.IdentityService.Services;

/// <summary>
/// <para>Used to work with customers in the database</para>
/// 
/// <para>
///     NOTE: Since this doesn't work directly with<br/>
///           the Customer entity, and works through<br/>
///           the UserManager, this is named CustomerService<br/>
///           rather than CustomerRepository
/// </para>
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// Adds a customer to the database with the provided password
    /// </summary>
    /// <param name="customer">Customer entity</param>
    /// <param name="password">The customer's new password</param>
    /// <returns>A fully-populated Customer entity</returns>
    Task<Customer> CreateAsync(Customer customer, string password);

    /// <summary>
    /// Retrieves a customer from the database
    /// </summary>
    /// <param name="customerId">Id of the customer in the database</param>
    /// <returns>A Customer entity</returns>
    Task<Customer> RetrieveSingleAsync(Guid customerId);

    /// <summary>
    /// Attempts to retrieve a customer from the database with the matching email address
    /// </summary>
    /// <param name="email">The email address of the customer</param>
    /// <returns>A Customer entity if it exists or null</returns>
    Task<Customer?> RetrieveSingleAsync(string email);

    /// <summary>
    /// Updates a customers details
    /// </summary>
    /// <param name="customerId">Id of the customer in the database</param>
    /// <param name="updateAccountModel">The model containing the new customer details</param>
    Task UpdateAsync(Guid customerId, UpdateAccountModel updateAccountModel);

    /// <summary>
    /// Deletes a customer from the database
    /// </summary>
    /// <param name="customer">Customer entity</param>
    Task DeleteAsync(Customer customer);

    /// <summary>
    /// Performs verification on the customer's password
    /// </summary>
    /// <param name="customerId">Id of the customer in the database</param>
    /// <param name="password">The customer's current password</param>
    /// <returns>A boolean indicating if the customer has provided the correct password</returns>
    Task<bool> VerifyPasswordAsync(Guid customerId, string password);

    /// <summary>
    /// Resets a customer's password by removing the old one and adding the new, not overwriting
    /// </summary>
    /// <param name="customerId">Id of the customer in the database</param>
    /// <param name="password">The customer's new password</param>
    Task ResetPasswordAsync(Guid customerId, string password);
}
