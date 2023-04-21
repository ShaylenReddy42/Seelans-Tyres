using AutoMapper;                                          // IMapper
using IdentityServer4.Stores;                              // ISigningCredentialStore
using Microsoft.AspNetCore.Authentication.JwtBearer;       // JwtBearerDefaults
using Microsoft.AspNetCore.Authorization;                  // Authorize
using SeelansTyres.Libraries.Shared.Channels;              // PublishUpdateChannel
using SeelansTyres.Libraries.Shared.Messages;              // BaseMessage
using SeelansTyres.Libraries.Shared.Models;                // EncryptedDataModel
using SeelansTyres.Services.IdentityService.Data.Entities; // Customer
using SeelansTyres.Services.IdentityService.Extensions;    // DecryptAsync()
using SeelansTyres.Services.IdentityService.Services;      // ICustomerService
using System.Diagnostics;                                  // Stopwatch, Activity
using System.Text.Json;                                    // JsonSerializer

namespace SeelansTyres.Services.IdentityService.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
    
    [HttpPost]
    [Authorize(Policy = "CreateAccountPolicy")]
    public async Task<ActionResult> CreateAsync(EncryptedDataModel encryptedDataModel)
    {
        logger.LogInformation("API => Attempting to create a new customer account, decryption required");
        
        var registerModel = await encryptedDataModel.DecryptAsync<RegisterModel>(signingCredentialStore, logger);

        if (registerModel is null)
        {
            logger.LogWarning(
                "{announcement}: Decryption process failed",
                "NULL");
            
            return BadRequest("Data got corrupted in transit, decryption failed!");
        }
        
        var customer = await customerService.RetrieveSingleAsync(registerModel.Email);

        if (customer is not null)
        {
            logger.LogWarning(
                "{announcement}: Customer with email {customerEmail} already exists",
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

    [HttpGet]
    [Authorize(Policy = "RetrieveSingleByEmailPolicy")]
    public async Task<ActionResult<CustomerModel>> RetrieveSingleAsync(string email)
    {
        logger.LogInformation(
            "API => Attempting to retrieve customer by email {customerEmail}",
            "***REDACTED***");
        
        if (string.IsNullOrEmpty(email.Trim()) is true)
        {
            logger.LogWarning(
                "{announcement}: Authenticated user using client '{clientId}' attempted to retrieve all customers by not specifying an email",
                "FAILED", User.Claims.Single(claim => claim.Type is "client_id").Value);

            return BadRequest();
        }

        var customer = await customerService.RetrieveSingleAsync(email);

        return customer is not null ? Ok(mapper.Map<Customer, CustomerModel>(customer)) : NotFound();
    }

    [HttpGet("{id}", Name = "RetrieveSingleAsync")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    public async Task<ActionResult<CustomerModel>> RetrieveSingleAsync(Guid id)
    {
        logger.LogInformation(
            "API => Attempting to retrieve customer by Id {customerId}",
            id);
        
        var customer = await customerService.RetrieveSingleAsync(id);

        return Ok(mapper.Map<Customer, CustomerModel>(customer));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    public async Task<ActionResult> UpdateAsync(Guid id, EncryptedDataModel encryptedDataModel)
    {
        logger.LogInformation(
            "API => Attempting to update account for customer {customerId}, decryption required",
            id);

        var updateAccountModel = await encryptedDataModel.DecryptAsync<UpdateAccountModel>(signingCredentialStore, logger);

        if (updateAccountModel is null)
        {
            logger.LogWarning(
                "{announcement}: Decryption process failed",
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
            IdOfEntityToUpdate = id
        };

        var configurationKeyForDestination = environment.IsDevelopment() is true
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

    [HttpDelete("{id}")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
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
            IdOfEntityToUpdate = id
        };

        var configurationKeyForDestination = environment.IsDevelopment() is true
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

    [HttpPost("{id}/verifypassword")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    public async Task<ActionResult> VerifyPasswordAsync(Guid id, EncryptedDataModel encryptedDataModel)
    {
        logger.LogInformation(
            "API => Attempting a password verification process for customer {customerId}, decryption required",
            id);

        var passwordModel = await encryptedDataModel.DecryptAsync<PasswordModel>(signingCredentialStore, logger);

        if (passwordModel is null)
        {
            logger.LogWarning(
                "{announcement}: Decryption process failed",
                "NULL");

            return BadRequest("Data got corrupted in transit, decryption failed!");
        }
        
        var result = await customerService.VerifyPasswordAsync(id, passwordModel.Password);

        return result is true ? Ok() : BadRequest();
    }

    [HttpPut("{id}/resetpassword")]
    [Authorize(Policy = "ResetPasswordPolicy")]
    public async Task<ActionResult> ResetPasswordAsync(Guid id, EncryptedDataModel encryptedDataModel)
    {
        logger.LogInformation(
            "API => Attempting a password reset operation for customer {customerId}, decryption required",
            id);

        var passwordModel = await encryptedDataModel.DecryptAsync<PasswordModel>(signingCredentialStore, logger);

        if (passwordModel is null)
        {
            logger.LogWarning(
                "{announcement}: Decryption process failed",
                "NULL");

            return BadRequest("Data got corrupted in transit, decryption failed!");
        }

        await customerService.ResetPasswordAsync(id, passwordModel.Password);

        return Ok();
    }
}
