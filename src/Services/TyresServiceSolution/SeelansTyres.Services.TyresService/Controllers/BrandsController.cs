using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Services.TyresService.Services;
using SeelansTyres.Services.TyresService.Data.Entities;
using SeelansTyres.Models.TyresModels.V1;

namespace SeelansTyres.Services.TyresService.Controllers;

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
    public async Task<ActionResult<IEnumerable<BrandModel>>> RetrieveAllAsync()
    {
        logger.LogInformation("API => Attempting to retrieve all brands");
        
        var brands = await tyresRepository.RetrieveAllBrandsAsync();
        
        return Ok(mapper.Map<IEnumerable<Brand>, IEnumerable<BrandModel>>(brands));
    }
}
