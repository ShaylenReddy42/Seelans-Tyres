using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Services.AddressService.Services;
using SeelansTyres.Services.AddressService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SeelansTyres.Services.AddressService.Data.Entities;

namespace SeelansTyres.Services.AddressService.Controllers;

[Route("api/customers/{customerId}/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "MustBeARegularCustomer")]
public class AddressesController : ControllerBase
{
    private readonly ILogger<AddressesController> logger;
    private readonly IAddressRepository addressRepository;
    private readonly IMapper mapper;

    public AddressesController(
        ILogger<AddressesController> logger,
        IAddressRepository addressRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.addressRepository = addressRepository;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressModel>>> RetrieveAllAsync(Guid customerId)
    {
        logger.LogInformation(
            "API => Retrieving addresses for customer {customerId}", 
            customerId);

        var addresses = await addressRepository.RetrieveAllAsync(customerId);

        return Ok(mapper.Map<IEnumerable<Address>, IEnumerable<AddressModel>>(addresses));
    }

    [HttpGet("{addressId}", Name = "GetAddressForCustomer")]
    public async Task<ActionResult<AddressModel>> RetrieveSingleAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "API => Retrieving address {addressId} for customer {customerId}", 
            addressId, customerId);

        var address = await addressRepository.RetrieveSingleAsync(customerId, addressId);

        if (address is null)
        {
            logger.LogWarning(
                "{announcement}: Address {addressId} for customer {customerId} does not exist!",
                "NULL", addressId, customerId);

            return NotFound();
        }

        return Ok(mapper.Map<Address, AddressModel>(address));
    }

    [HttpPost]
    public async Task<ActionResult<AddressModel>> CreateAsync(Guid customerId, AddressModel newAddress)
    {
        logger.LogInformation(
            "API => Adding a new address for customer {customerId}", 
            customerId);

        var addressEntity = mapper.Map<AddressModel, Address>(newAddress);

        await addressRepository.CreateAsync(customerId, addressEntity);

        await addressRepository.SaveChangesAsync();

        var createdAddress = mapper.Map<Address, AddressModel>(addressEntity);

        return CreatedAtRoute(
            "GetAddressForCustomer",
            new { customerId = customerId, addressId = createdAddress.Id },
            createdAddress);
    }

    [HttpPut("{addressId}")]
    public async Task<ActionResult> MarkAddressAsPrefferedAsync(Guid customerId, Guid addressId, bool markAsPreffered)
    {
        logger.LogInformation(
            "API => Marking address {addressId} as preferred for customer {customerId}",
            addressId, customerId);

        var address = await addressRepository.RetrieveSingleAsync(customerId, addressId);

        if (address is null)
        {
            logger.LogWarning(
                "{announcement}: Address {addressId} for customer {customerId} does not exist!",
                "NULL", addressId, customerId);

            return NotFound();
        }

        await addressRepository.MarkAsPrefferedAsync(customerId, address);
        await addressRepository.SaveChangesAsync();

        return NoContent();
    }
}
