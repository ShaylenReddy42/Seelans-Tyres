using AutoMapper;                                       // IMapper
using Microsoft.AspNetCore.Mvc;                         // ApiController, ControllerBase, Http Methods, ActionResult
using SeelansTyres.Services.TyresService.Services;      // ITyresRepository
using SeelansTyres.Services.TyresService.Data.Entities; // Brand
using static System.Net.Mime.MediaTypeNames;            // Application

namespace SeelansTyres.Services.TyresService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Consumes(Application.Json)]
[Produces(Application.Json)]
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
