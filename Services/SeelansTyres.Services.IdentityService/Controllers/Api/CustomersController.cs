using AutoMapper;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Services.IdentityService.Data.Entities;
using SeelansTyres.Services.IdentityService.Extensions;
using SeelansTyres.Services.IdentityService.Models;
using SeelansTyres.Services.IdentityService.Services;

namespace SeelansTyres.Services.IdentityService.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService customerService;
    private readonly ISigningCredentialStore signingCredentialStore;
    private readonly IMapper mapper;

    public CustomersController(
        ICustomerService customerService,
        ISigningCredentialStore signingCredentialStore,
        IMapper mapper)
    {
        this.customerService = customerService;
        this.signingCredentialStore = signingCredentialStore;
        this.mapper = mapper;
    }
    
    [HttpPost]
    [Authorize(Policy = "CreateAccountPolicy")]
    public async Task<ActionResult> CreateAsync(EncryptedDataModel encryptedDataModel)
    {
        var registerModel = await encryptedDataModel.DecryptAsync<RegisterModel>(signingCredentialStore);

        if (registerModel is null)
        {
            return BadRequest("Data got corrupted in transit, decryption failed!");
        }
        
        var customer = await customerService.RetrieveSingleAsync(registerModel.Email);

        if (customer is not null)
        {
            return BadRequest();
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
        if (string.IsNullOrEmpty(email) is true)
        {
            return BadRequest();
        }

        var customer = await customerService.RetrieveSingleAsync(email);

        return customer is not null ? Ok(mapper.Map<Customer, CustomerModel>(customer)) : NotFound();
    }

    [HttpGet("{id}", Name = "RetrieveSingleAsync")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    public async Task<ActionResult<CustomerModel>> RetrieveSingleAsync(Guid id)
    {
        var customer = await customerService.RetrieveSingleAsync(id);

        return Ok(mapper.Map<Customer, CustomerModel>(customer));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    public async Task<ActionResult> UpdateAsync(Guid id, EncryptedDataModel encryptedDataModel)
    {
        var updateAccountModel = await encryptedDataModel.DecryptAsync<UpdateAccountModel>(signingCredentialStore);

        if (updateAccountModel is null)
        {
            return BadRequest("Data got corrupted in transit, decryption failed!");
        }
        
        await customerService.UpdateAsync(id, updateAccountModel);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        await customerService.DeleteAsync(await customerService.RetrieveSingleAsync(id));

        return NoContent();
    }

    [HttpPost("{id}/verifypassword")]
    [Authorize(Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
    public async Task<ActionResult> VerifyPasswordAsync(Guid id, EncryptedDataModel encryptedDataModel)
    {
        var passwordModel = await encryptedDataModel.DecryptAsync<PasswordModel>(signingCredentialStore);

        if (passwordModel is null)
        {
            return BadRequest("Data got corrupted in transit, decryption failed!");
        }
        
        var result = await customerService.VerifyPasswordAsync(id, passwordModel.Password);

        return result is true ? Ok() : BadRequest();
    }

    [HttpPut("{id}/resetpassword")]
    [Authorize(Policy = "ResetPasswordPolicy")]
    public async Task<ActionResult> ResetPasswordAsync(Guid id, EncryptedDataModel encryptedDataModel)
    {
        var passwordModel = await encryptedDataModel.DecryptAsync<PasswordModel>(signingCredentialStore);

        if (passwordModel is null)
        {
            return BadRequest("Data got corrupted in transit, decryption failed!");
        }

        await customerService.ResetPasswordAsync(id, passwordModel.Password);

        return Ok();
    }
}
