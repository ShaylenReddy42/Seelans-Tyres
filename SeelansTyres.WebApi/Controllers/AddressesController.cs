using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.WebApi.Services;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SeelansTyres.WebApi.Controllers;

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
    public async Task<ActionResult<IEnumerable<AddressModel>>> RetrieveAll(Guid customerId)
    {
        if (await addressRepository.CheckIfCustomerExistsAsync(customerId) is false)
        {
            return NotFound();
        }
        
        var addresses = await addressRepository.RetrieveAllAsync(customerId);

        return Ok(mapper.Map<IEnumerable<Address>, IEnumerable<AddressModel>>(addresses));
    }

    [HttpGet("{addressId}", Name = "GetAddressForCustomer")]
    public async Task<ActionResult<AddressModel>> RetrieveSingle(Guid customerId, int addressId)
    {
        if (await addressRepository.CheckIfCustomerExistsAsync(customerId) is false)
        {
            return NotFound();
        }

        var address = await addressRepository.RetrieveSingleAsync(customerId, addressId);

        return Ok(mapper.Map<Address, AddressModel>(address!));
    }

    [HttpPost]
    public async Task<ActionResult<AddressModel>> Create(Guid customerId, CreateAddressModel newAddress)
    {
        if (await addressRepository.CheckIfCustomerExistsAsync(customerId) is false)
        {
            return NotFound();
        }

        var addressEntity = mapper.Map<CreateAddressModel, Address>(newAddress);

        await addressRepository.CreateAsync(customerId, addressEntity);

        await addressRepository.SaveChangesAsync();

        var createdAddress = mapper.Map<Address, AddressModel>(addressEntity);

        return CreatedAtRoute(
            "GetAddressForCustomer",
            new { customerId = customerId, addressId = createdAddress.Id },
            createdAddress);
    }

    [HttpPut("{addressId}")]
    public async Task<ActionResult> MarkAddressAsPreffered(Guid customerId, int addressId, bool markAsPreffered)
    {
        if (await addressRepository.CheckIfCustomerExistsAsync(customerId) is false)
        {
            return NotFound();
        }

        var address = await addressRepository.RetrieveSingleAsync(customerId, addressId);

        if (address is null)
        {
            return NotFound();
        }

        await addressRepository.MarkAsPrefferedAsync(customerId, address);
        await addressRepository.SaveChangesAsync();

        return NoContent();
    }
}
