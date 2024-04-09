using AutoMapper;                                           // IMapper
using IdentityServer4.Stores;                               // ISigningCredentialStore
using Microsoft.AspNetCore.Authentication.JwtBearer;        // JwtBearerDefaults
using Microsoft.AspNetCore.Authorization;                   // Authorize
using SeelansTyres.Libraries.Shared.Models;                 // EncryptedDataModel
using SeelansTyres.Services.IdentityService.Data.Entities;  // Customer
using SeelansTyres.Services.IdentityService.Extensions;     // DecryptAsync()
using SeelansTyres.Services.IdentityService.Services;       // ICustomerService
using System.Diagnostics;                                   // Stopwatch, Activity
using System.Text.Json;                                     // JsonSerializer
using static System.Net.Mime.MediaTypeNames;                // Application
using ShaylenReddy42.UnpublishedUpdatesManagement.Channels; // PublishUpdateChannel
using ShaylenReddy42.UnpublishedUpdatesManagement.Messages; // BaseMessage

namespace SeelansTyres.Services.IdentityService.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Consumes(Application.Json)]
[Produces(Application.Json)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService customerService;
    private readonly ISigningCredentialStore signingCredentialStore;
    private readonly IMapper mapper;
    private readonly ILogger<CustomersController> logger;
    private readonly IConfiguration configuration;
    private readonly IWebHostEnvironment environment;
    private readonly PublishUpdateChannel publishUpdateChannel;
    private readonly Stopwatch stopwatch = new();

    public CustomersController(
        ICustomerService customerService,
        ISigningCredentialStore signingCredentialStore,
        IMapper mapper,
        ILogger<CustomersController> logger,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        PublishUpdateChannel publishUpdateChannel)
    {
        this.customerService = customerService;
        this.signingCredentialStore = signingCredentialStore;
        this.mapper = mapper;
        this.logger = logger;
        this.configuration = configuration;
        this.environment = environment;
        this.publishUpdateChannel = publishUpdateChannel;
    }

    /// <summary>
    /// Creates a new customer account
    /// </summary>
    /// <remarks>
    /// The register model is encrypted and stored in the EncryptedDataModel using hybrid encryption  
    ///   
    /// RegisterModel:  
    /// 
    ///     {  
    ///         "firstName": "string",  
    ///         "lastName": "string",  
    ///         "email": "string",  
    ///         "phoneNumber": "string",  
    ///         "password": "string",  
    ///         "confirmPassword": "string"  
    ///     }  
    /// 
    /// </remarks>
    /// <param name="encryptedDataModel">The model containing the encrypted register model used to create an account</param>
    /// <response code="201">Indicates that the customer account was created successfully</response>
    /// <response code="400">
    /// Indicates that either decryption of the model failed due to the data being tampered with  
    /// or that the account already exists
    /// </response>
    /// <returns>A Task of type ActionResult</returns>
    [HttpPost]
    [Authorize(Policy = "CreateAccountPolicy")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateAsync(EncryptedDataModel encryptedDataModel)
    {
        logger.LogInformation("API => Attempting to create a new customer account, decryption required");
        
        var registerModel = await encryptedDataModel.DecryptAsync<RegisterModel>(signingCredentialStore, logger);

        if (registerModel is null)
        {
            logger.LogWarning(
                "{Announcement}: Decryption process failed",
                "NULL");
            
            return BadRequest("Data got corrupted in transit, decryption failed!");
        }
        
        var customer = await customerService.RetrieveSingleAsync(registerModel.Email);

        if (customer is not null)
        {
            logger.LogWarning(
                "{Announcement}: Customer with email {customerEmail} already exists",
                "ABORTED", "***REDACTED***");

            return BadRequest("Customer already exists");
        }

        customer = mapper.Map<RegisterModel, Customer>(registerModel);

        customer.UserName = registerModel.Email;

        await customerService.CreateAsync(customer, registerModel.Password);

        var createdCustomer = mapper.Map<Customer, CustomerModel>(customer);

        return CreatedAtRoute(
            routeName: "RetrieveSingleAsync",
            routeValues: new { id = createdCustomer.Id },
            value: createdCustomer);
    }

    /// <summary>
    /// Retrieves a customer via an email address
    /// </summary>
    /// <param name="email">The email address of the requested customer account</param>
    /// <response code="200">The requested customer account</response>
    /// <response code="400">
    /// Indicates that the requester attempted to retrieve ALL customer accounts,  
    /// which isn't allowed via the API for security reasons
    /// </response>
    /// <response code="404">Indicates that the requested customer account doesn't exist in the database</response>
    /// <returns>A customer account in the form of a Task of type ActionResult of type CustomerModel</returns>
    [HttpGet]
    [Authorize(Policy = "RetrieveSingleByEmailPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerModel))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerModel>> RetrieveSingleAsync(string email)
    {
        logger.LogInformation(
            "API => Attempting to retrieve customer by email {customerEmail}",
            "***REDACTED***");
        
        if (string.IsNullOrEmpty(email.Trim()))
        {
            logger.LogWarning(
                "{Announcement}: Authenticated user using client '{clientId}' attempted to retrieve all customers by not specifying an email",
                "FAILED", User.Claims.Single(claim => claim.Type is "client_id").Value);

            return BadRequest();
        }

        var customer = await customerService.RetrieveSingleAsync(email);

        return customer is not null ? Ok(mapper.Map<Customer, CustomerModel>(customer)) : NotFound();
    }

    /// <summary>
    /// Retrieves a customer via an id
    /// </summary>
    /// <param name="id">The id of the requested customer</param>
    /// <response code="200">The requested customer account</response>
    /// <returns>A customer account in the form of a Task of type ActionResult of type CustomerModel</returns>
    [HttpGet("{id}", Name = "RetrieveSingleAsync")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CustomerModel))]
    public async Task<ActionResult<CustomerModel>> RetrieveSingleAsync(Guid id)
    {
        logger.LogInformation(
            "API => Attempting to retrieve customer by Id {customerId}",
            id);
        
        var customer = await customerService.RetrieveSingleAsync(id);

        return Ok(mapper.Map<Customer, CustomerModel>(customer));
    }

    /// <summary>
    /// Updates a customer's account
    /// </summary>
    /// <remarks>
    /// The update account model is encrypted and stored in the EncryptedDataModel using hybrid encryption  
    ///   
    /// UpdateAccountModel:  
    /// 
    ///     {  
    ///         "firstName": "string",  
    ///         "lastName": "string",  
    ///         "phoneNumber": "string"  
    ///     }  
    ///   
    /// This action also publishes the update to a message broker to be consumed by other microservices  
    /// </remarks>
    /// <param name="id">The id of the customer account to be updated</param>
    /// <param name="encryptedDataModel">The model containing the encrypted update account model used to update a customer's account</param>
    /// <response code="204">Indicates that the customer's account was updated successfully</response>
    /// <response code="400">Indicates that the decryption of the model failed due to the data being tampered with</response>
    /// <returns>A Task of type ActionResult</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateAsync(Guid id, EncryptedDataModel encryptedDataModel)
    {
        logger.LogInformation(
            "API => Attempting to update account for customer {customerId}, decryption required",
            id);

        var updateAccountModel = await encryptedDataModel.DecryptAsync<UpdateAccountModel>(signingCredentialStore, logger);

        if (updateAccountModel is null)
        {
            logger.LogWarning(
                "{Announcement}: Decryption process failed",
                "NULL");

            return BadRequest("Data got corrupted in transit, decryption failed!");
        }
        
        await customerService.UpdateAsync(id, updateAccountModel);

        logger.LogInformation("Preparing to publish the update for other microservices");

        var baseMessage = new BaseMessage
        {
            TraceId = Activity.Current!.TraceId.ToString(),
            SpanId = Activity.Current!.SpanId.ToString(),
            AccessToken = HttpContext.Request.Headers.Authorization[0]!.Replace("Bearer ", ""),
            SerializedModel = JsonSerializer.SerializeToUtf8Bytes(updateAccountModel),
            IdOfEntityToUpdate = id.ToString()
        };

        var configurationKeyForDestination = environment.IsDevelopment()
                                           ? "RabbitMQ:Exchanges:UpdateAccount"
                                           : "AzureServiceBus:Topics:UpdateAccount";

        stopwatch.Start();

        await publishUpdateChannel.WriteToChannelAsync(baseMessage, configuration[configurationKeyForDestination]!);

        stopwatch.Stop();

        logger.LogInformation(
            "It took {stopwatchElapsedTime}ms to write the update to the channel",
            stopwatch.ElapsedMilliseconds);

        return NoContent();
    }

    /// <summary>
    /// Deletes a customer's account
    /// </summary>
    /// <remarks>
    /// This action also publishes the account deletion to a message broker to be consumed by other microservices  
    /// </remarks>
    /// <param name="id">The id of the customer's account to be deleted</param>
    /// <response code="204">Indicates that the customer's account was deleted successfully</response>
    /// <returns>A Task of type ActionResult</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        logger.LogInformation(
            "API => Attempting to delete account for customer {customerId}",
            id);

        await customerService.DeleteAsync(await customerService.RetrieveSingleAsync(id));

        logger.LogInformation("Preparing to publish the update for other microservices");

        var baseMessage = new BaseMessage
        {
            TraceId = Activity.Current!.TraceId.ToString(),
            SpanId = Activity.Current!.SpanId.ToString(),
            AccessToken = HttpContext.Request.Headers.Authorization[0]!.Replace("Bearer ", ""),
            IdOfEntityToUpdate = id.ToString()
        };

        var configurationKeyForDestination = environment.IsDevelopment()
                                           ? "RabbitMQ:Exchanges:DeleteAccount"
                                           : "AzureServiceBus:Topics:DeleteAccount";

        stopwatch.Start();

        await publishUpdateChannel.WriteToChannelAsync(baseMessage, configuration[configurationKeyForDestination]!);

        stopwatch.Stop();

        logger.LogInformation(
            "It took {stopwatchElapsedTime}ms to write the update to the channel",
            stopwatch.ElapsedMilliseconds);

        return NoContent();
    }

    /// <summary>
    /// Verifies if a customer's password matches their password in the database
    /// </summary>
    /// <remarks>
    /// The password model is encrypted and stored in the EncryptedDataModel using hybrid encryption  
    ///   
    /// PasswordModel:  
    /// 
    ///     {  
    ///         "password": "string"  
    ///     }  
    /// 
    /// </remarks>
    /// <param name="id">The id of the customer's account that requires password verification</param>
    /// <param name="encryptedDataModel">The model containing the encrypted password model used to verify a customer's password</param>
    /// <response code="200">Indicates that their password was a match</response>
    /// <response code="400">
    /// Indicates that either decryption of the model failed due to the data being tampered with  
    /// or that their password wasn't a match
    /// </response>
    /// <returns>A Task of type ActionResult</returns>
    [HttpPost("{id}/verifypassword")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> VerifyPasswordAsync(Guid id, EncryptedDataModel encryptedDataModel)
    {
        logger.LogInformation(
            "API => Attempting a password verification process for customer {customerId}, decryption required",
            id);

        var passwordModel = await encryptedDataModel.DecryptAsync<PasswordModel>(signingCredentialStore, logger);

        if (passwordModel is null)
        {
            logger.LogWarning(
                "{Announcement}: Decryption process failed",
                "NULL");

            return BadRequest("Data got corrupted in transit, decryption failed!");
        }
        
        var result = await customerService.VerifyPasswordAsync(id, passwordModel.Password);

        return result ? Ok() : BadRequest();
    }

    /// <summary>
    /// Allows a customer to reset their password
    /// </summary>
    /// <remarks>
    /// The password model is encrypted and stored in the EncryptedDataModel using hybrid encryption  
    ///   
    /// PasswordModel:  
    /// 
    ///     {  
    ///         "password": "string"  
    ///     }  
    /// 
    /// </remarks>
    /// <param name="id">The id of the customer who wants to reset their password</param>
    /// <param name="encryptedDataModel">The model containing the encrypted password model used to reset a customer's password</param>
    /// <response code="200">Indicates that the customer's password was reset successfully</response>
    /// <response code="400">Indicates that the decryption of the model failed due to the data being tampered with</response>
    /// <returns>A Task of type ActionResult</returns>
    [HttpPut("{id}/resetpassword")]
    [Authorize(Policy = "ResetPasswordPolicy")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetPasswordAsync(Guid id, EncryptedDataModel encryptedDataModel)
    {
        logger.LogInformation(
            "API => Attempting a password reset operation for customer {customerId}, decryption required",
            id);

        var passwordModel = await encryptedDataModel.DecryptAsync<PasswordModel>(signingCredentialStore, logger);

        if (passwordModel is null)
        {
            logger.LogWarning(
                "{Announcement}: Decryption process failed",
                "NULL");

            return BadRequest("Data got corrupted in transit, decryption failed!");
        }

        await customerService.ResetPasswordAsync(id, passwordModel.Password);

        return Ok();
    }
}
