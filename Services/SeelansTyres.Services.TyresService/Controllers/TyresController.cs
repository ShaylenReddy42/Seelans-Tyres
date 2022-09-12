using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Services.TyresService.Services;
using SeelansTyres.Services.TyresService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SeelansTyres.Services.TyresService.Data.Entities;

namespace SeelansTyres.Services.TyresService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "MustBeAnAdministrator")]
public class TyresController : ControllerBase
{
    private readonly ILogger<TyresController> logger;
    private readonly ITyresRepository tyresRepository;
    private readonly IMapper mapper;

    public TyresController(
        ILogger<TyresController> logger,
        ITyresRepository tyresRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.tyresRepository = tyresRepository;
        this.mapper = mapper;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TyreModel>>> RetrieveAll(bool availableOnly = true)
    {
        var tyres = await tyresRepository.RetrieveAllTyresAsync(availableOnly);

        return Ok(mapper.Map<IEnumerable<Tyre>, IEnumerable<TyreModel>>(tyres));
    }

    [HttpGet("{id}", Name = "GetTyreById")]
    public async Task<ActionResult<TyreModel>> RetrieveSingle(Guid id)
    {
        var tyre = await tyresRepository.RetrieveSingleTyreAsync(id);

        if (tyre is not null)
        {
            return Ok(mapper.Map<Tyre, TyreModel>(tyre));
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<TyreModel>> Create(TyreModel model)
    {
        var tyreEntity = mapper.Map<TyreModel, Tyre>(model);

        await tyresRepository.CreateTyreAsync(tyreEntity);

        await tyresRepository.SaveChangesAsync();

        var createdTyre = mapper.Map<Tyre, TyreModel>(tyreEntity);

        return CreatedAtRoute(
            "GetTyreById",
            new { id = createdTyre.Id },
            createdTyre);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(TyreModel model, Guid id)
    {
        var tyre = await tyresRepository.RetrieveSingleTyreAsync(id);

        if (tyre is null)
        {
            return NotFound();
        }

        mapper.Map(model, tyre);

        tyre.Brand = 
            (await tyresRepository.RetrieveAllBrandsAsync())
                .Single(brand => brand.Id == tyre.BrandId);

        await tyresRepository.SaveChangesAsync();

        return NoContent();
    }
}
