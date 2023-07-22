namespace SeelansTyres.Frontends.Mvc.HttpClients;

/// <summary>
/// A strongly-typed http client to communicate with the Identity / Customer Microservice
/// </summary>
public interface ICustomerServiceClient
{
    /// <summary>
    /// Makes a post request to the customer microservice to create a new customer account
    /// </summary>
    /// <param name="registerModel">The model containing the newly created customer</param>
    /// <returns>A tuple containing three pieces of data</returns>

    // Adding a description of the return breaks the xml so I'm adding it as comment instead
    // 1. CustomerModel?: The model of the newly created customer or null if it failed
    // 2. bool: A boolean indicating if a success status code is returned
    // 3. List<string>: A list of possible errors that may have occured on account creation
    Task<(CustomerModel?, bool, List<string>)> CreateAsync(RegisterModel registerModel);

    /// <summary>
    /// Makes a get request at the customer microservice to find and retrieve a customer by id
    /// </summary>
    /// <param name="customerId">The id of the customer</param>
    /// <returns>A customer</returns>
    Task<CustomerModel> RetrieveSingleAsync(Guid customerId);

    /// <summary>
    /// Makes a get request at the customer microservice to find and retrieve a customer by email
    /// </summary>
    /// <param name="email">The email address of a potential customer</param>
    /// <returns>A customer if they exist or null</returns>
    Task<CustomerModel?> RetrieveSingleAsync(string email);

    /// <summary>
    /// Makes a put request to update a customer's details at the customer microservice
    /// </summary>
    /// <param name="updateAccountModel">The model containing the newly updated customer information</param>
    Task UpdateAsync(UpdateAccountModel updateAccountModel);

    /// <summary>
    /// Makes a delete request at the customer microservice to delete a customer's account
    /// </summary>
    /// <param name="password">The password of the customer used for verification before deletion</param>
    /// <returns>A boolean indicating if a success status code is returned</returns>
    Task<bool> DeleteAsync(string password);

    /// <summary>
    /// Makes a put request to update a customer's password at the customer microservice
    /// </summary>
    /// <param name="customerId">The id of the customer</param>
    /// <param name="password">The new password for the customer</param>
    Task ResetPasswordAsync(Guid customerId, string password);
}
