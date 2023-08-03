using AutoMapper;                                    // IMapper
using SeelansTyres.Services.AddressService.Services; // IAddressRepository
using Microsoft.AspNetCore.Authorization;            // Authorize
using Microsoft.AspNetCore.Authentication.JwtBearer; // JwtBearerDefaults
using SeelansTyres.Data.AddressData.Entities;        // Address
using static System.Net.Mime.MediaTypeNames;         // Application

namespace SeelansTyres.Services.AddressService.Controllers;

[Route("api/customers/{customerId}/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "MustBeARegularCustomer")]
[Consumes(Application.Json)]
[Produces(Application.Json)]
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

    [HttpPut("{addressId}")]
    public async Task<ActionResult> MarkAddressAsPreferredAsync(Guid customerId, Guid addressId, bool markAsPreferred)
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

        await addressRepository.MarkAsPreferredAsync(customerId, address);
        await addressRepository.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{addressId}")]
    public async Task<ActionResult> DeleteAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "API => Attempting to delete address {addressId} for customer {customerId}",
            addressId, customerId);

        await addressRepository.DeleteAsync(customerId, addressId);

        return NoContent();
    }
}
