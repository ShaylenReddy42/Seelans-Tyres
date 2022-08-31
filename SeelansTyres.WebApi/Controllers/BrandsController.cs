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
    private readonly ITyresRepository tyresRepository;
    private readonly IMapper mapper;

    public BrandsController(
        ILogger<BrandsController> logger,
        ITyresRepository tyresRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.tyresRepository = tyresRepository;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BrandModel>>> RetrieveAll()
    {
        var brands = await tyresRepository.RetrieveAllBrandsAsync();
        
        return Ok(mapper.Map<IEnumerable<Brand>, IEnumerable<BrandModel>>(brands));
    }
}
