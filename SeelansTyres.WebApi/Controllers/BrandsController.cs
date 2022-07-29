using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.WebApi.Services;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;

namespace SeelansTyres.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BrandsController : ControllerBase
{
    private readonly ILogger<BrandsController> logger;
    private readonly ISeelansTyresRepository repository;
    private readonly IMapper mapper;

    public BrandsController(
        ILogger<BrandsController> logger,
        ISeelansTyresRepository repository,
        IMapper mapper)
    {
        this.logger = logger;
        this.repository = repository;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BrandModel>>> GetAllBrands()
    {
        var brands = await repository.GetAllBrandsAsync();
        
        return Ok(mapper.Map<IEnumerable<Brand>, IEnumerable<BrandModel>>(brands));
    }
}
