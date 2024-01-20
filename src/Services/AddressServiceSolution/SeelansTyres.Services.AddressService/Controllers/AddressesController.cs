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
public class AddressesController(
    ILogger<AddressesController> logger,
    IAddressRepository addressRepository,
    IMapper mapper) : ControllerBase
{

    /// <summary>
    /// Adds a new address to the customer's address book
    /// </summary>
    /// <param name="customerId">The id of the customer that's used to link the address</param>
    /// <param name="newAddress">The model containing the new address to be added</param>
    /// <response code="201">Indicates a successful creation of an address</response>
    /// <returns>The newly created address for the customer in the form of a Task of type ActionResult of type AddressModel</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(AddressModel))]
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
            new { customerId, addressId = createdAddress.Id },
            createdAddress);
    }

    /// <summary>
    /// Retrieves all the addresses for a particular customer
    /// </summary>
    /// <param name="customerId">The id of the customer used to filter for their addresses</param>
    /// <response code="200">A list of the customer's addresses</response>
    /// <returns>A list of the customer's addresses in the form of a Task of type ActionResult of type IEnumberable of type AddressModel</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AddressModel>))]
    public async Task<ActionResult<IEnumerable<AddressModel>>> RetrieveAllAsync(Guid customerId)
    {
        logger.LogInformation(
            "API => Retrieving addresses for customer {customerId}", 
            customerId);

        var addresses = await addressRepository.RetrieveAllAsync(customerId);

        return Ok(mapper.Map<IEnumerable<Address>, IEnumerable<AddressModel>>(addresses));
    }

    /// <summary>
    /// Retrieves a particular address for a particular customer
    /// </summary>
    /// <param name="customerId">The id of the customer used to filter for their addresses</param>
    /// <param name="addressId">The id of the address used to filter for the address</param>
    /// <response code="200">The requested address for the customer</response>
    /// <response code="404">The requested address doesn't exist in the database</response>
    /// <returns>A Task of type ActionResult of type AddressModel</returns>
    [HttpGet("{addressId}", Name = "GetAddressForCustomer")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AddressModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Updates the preferred address for a particular customer
    /// </summary>
    /// <param name="customerId">The id of the customer used to filter for their addresses</param>
    /// <param name="addressId">The id of the address to be marked as the preferred</param>
    /// <param name="markAsPreferred">Used to set whether an address should be marked as preferred or not [UNUSED]</param>
    /// <response code="204">Indicates that the address was marked as preferred successfully</response>
    /// <response code="404">The address to be marked as preferred doesn't exist in the database</response>
    /// <returns>A Task of type ActionResult</returns>
    [HttpPut("{addressId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    /// <summary>
    /// Deletes an address for a particular customer
    /// </summary>
    /// <param name="customerId">The id of the customer used to filter for their addresses</param>
    /// <param name="addressId">The id of the address to be deleted</param>
    /// <response code="204">Indicates that the address to be deleted was done so successfully</response>
    /// <returns>A Task of type ActionResult</returns>
    [HttpDelete("{addressId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteAsync(Guid customerId, Guid addressId)
    {
        logger.LogInformation(
            "API => Attempting to delete address {addressId} for customer {customerId}",
            addressId, customerId);

        await addressRepository.DeleteAsync(customerId, addressId);

        return NoContent();
    }
}
