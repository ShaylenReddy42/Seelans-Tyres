using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Services.IdentityService.Data.Entities;
using SeelansTyres.Services.IdentityService.Models;
using SeelansTyres.Services.IdentityService.Services;

namespace SeelansTyres.Services.IdentityService.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "CustomerIdFromClaimsMustMatchCustomerIdFromRoute")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService customerService;
    private readonly IMapper mapper;

    public CustomersController(
        ICustomerService customerService,
        IMapper mapper)
    {
        this.customerService = customerService;
        this.mapper = mapper;
    }
    
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult> CreateAsync(RegisterModel registerModel)
    {
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
    [AllowAnonymous]
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
    public async Task<ActionResult<CustomerModel>> RetrieveSingleAsync(Guid id)
    {
        var customer = await customerService.RetrieveSingleAsync(id);

        return Ok(mapper.Map<Customer, CustomerModel>(customer));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(Guid id, UpdateAccountModel updateAccountModel)
    {
        await customerService.UpdateAsync(id, updateAccountModel);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(Guid id)
    {
        await customerService.DeleteAsync(await customerService.RetrieveSingleAsync(id));

        return NoContent();
    }

    [HttpPost("{id}/verifypassword")]
    public async Task<ActionResult> VerifyPasswordAsync(Guid id, PasswordModel passwordModel)
    {
        var result = await customerService.VerifyPasswordAsync(id, passwordModel.Password);

        return result is true ? Ok() : BadRequest();
    }

    [HttpPut("{id}/resetpassword")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPasswordAsync(Guid id, PasswordModel passwordModel)
    {
        await customerService.ResetPasswordAsync(id, passwordModel.Password);

        return Ok();
    }
}
