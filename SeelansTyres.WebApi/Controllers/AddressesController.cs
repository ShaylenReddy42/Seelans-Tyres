using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.WebApi.Services;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;

namespace SeelansTyres.WebApi.Controllers;

[Route("api/customers/{customerId}/[controller]")]
[ApiController]
public class AddressesController : ControllerBase
{
    private readonly ILogger<AddressesController> logger;
    private readonly ISeelansTyresRepository repository;
    private readonly IMapper mapper;

    public AddressesController(
        ILogger<AddressesController> logger,
        ISeelansTyresRepository repository,
        IMapper mapper)
    {
        this.logger = logger;
        this.repository = repository;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AddressModel>>> GetAddressesForCustomer(Guid customerId)
    {
        if (await repository.CheckIfCustomerExistsAsync(customerId) is false)
        {
            return NotFound();
        }
        
        var addresses = await repository.GetAddressesForCustomerAsync(customerId);

        return Ok(mapper.Map<IEnumerable<Address>, IEnumerable<AddressModel>>(addresses));
    }

    [HttpGet("{addressId}", Name = "GetAddressForCustomer")]
    public async Task<ActionResult<AddressModel>> GetAddressForCustomer(Guid customerId, int addressId)
    {
        if (await repository.CheckIfCustomerExistsAsync(customerId) is false)
        {
            return NotFound();
        }

        var address = await repository.GetAddressForCustomerAsync(customerId, addressId);

        return Ok(mapper.Map<Address, AddressModel>(address!));
    }

    [HttpPost]
    public async Task<ActionResult<AddressModel>> AddNewAddressForUser(Guid customerId, CreateAddressModel newAddress)
    {
        if (await repository.CheckIfCustomerExistsAsync(customerId) is false)
        {
            return NotFound();
        }

        var addressEntity = mapper.Map<CreateAddressModel, Address>(newAddress);

        await repository.AddNewAddressForCustomerAsync(customerId, addressEntity);

        await repository.SaveChangesAsync();

        var createdAddress = mapper.Map<Address, AddressModel>(addressEntity);

        return CreatedAtRoute(
            "GetAddressForCustomer",
            new { customerId = customerId, addressId = createdAddress.Id },
            createdAddress);
    }

    [HttpPut("{addressId}")]
    public async Task<ActionResult> MarkAddressAsPreffered(Guid customerId, int addressId, bool markAsPreffered)
    {
        if (await repository.CheckIfCustomerExistsAsync(customerId) is false)
        {
            return NotFound();
        }

        var address = await repository.GetAddressForCustomerAsync(customerId, addressId);

        if (address is null)
        {
            return NotFound();
        }

        await repository.MarkAsPrefferedAsync(customerId, address);
        await repository.SaveChangesAsync();

        return NoContent();
    }
}
