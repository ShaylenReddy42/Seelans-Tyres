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
    public async Task<ActionResult<IEnumerable<AddressModel>>> RetrieveAll(Guid customerId)
    {
        var addresses = await addressRepository.RetrieveAllAsync(customerId);

        return Ok(mapper.Map<IEnumerable<Address>, IEnumerable<AddressModel>>(addresses));
    }

    [HttpGet("{addressId}", Name = "GetAddressForCustomer")]
    public async Task<ActionResult<AddressModel>> RetrieveSingle(Guid customerId, Guid addressId)
    {
        var address = await addressRepository.RetrieveSingleAsync(customerId, addressId);

        return Ok(mapper.Map<Address, AddressModel>(address!));
    }

    [HttpPost]
    public async Task<ActionResult<AddressModel>> Create(Guid customerId, AddressModel newAddress)
    {
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
    public async Task<ActionResult> MarkAddressAsPreffered(Guid customerId, Guid addressId, bool markAsPreffered)
    {
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
