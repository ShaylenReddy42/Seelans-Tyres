using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.WebApi.Services;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;

namespace SeelansTyres.WebApi.Controllers;

[Route("api/customer/{customerId}/[controller]")]
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
        var addresses = await repository.GetAddressesForCustomerAsync(customerId);

        return addresses is not null ? Ok(mapper.Map<IEnumerable<Address>, IEnumerable<AddressModel>>(addresses)) : NotFound();
    }
}
