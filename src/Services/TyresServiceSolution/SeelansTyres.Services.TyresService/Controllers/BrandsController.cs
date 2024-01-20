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
public class BrandsController(
    ILogger<BrandsController> logger,
    ITyresRepository tyresRepository,
    IMapper mapper) : ControllerBase
{

    /// <summary>
    /// Retrieves all the brands
    /// </summary>
    /// <response code="200">A list of brands</response>
    /// <returns>A list of brands in the form of a Task of type ActionResult of type IEnumberable of type BrandModel</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<BrandModel>))]
    public async Task<ActionResult<IEnumerable<BrandModel>>> RetrieveAllAsync()
    {
        logger.LogInformation("API => Attempting to retrieve all brands");
        
        var brands = await tyresRepository.RetrieveAllBrandsAsync();
        
        return Ok(mapper.Map<IEnumerable<Brand>, IEnumerable<BrandModel>>(brands));
    }
}
