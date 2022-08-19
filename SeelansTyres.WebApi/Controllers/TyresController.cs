using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.WebApi.Services;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SeelansTyres.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TyresController : ControllerBase
{
    private readonly ILogger<TyresController> logger;
    private readonly ITyresRepository repository;
    private readonly IMapper mapper;

    public TyresController(
        ILogger<TyresController> logger,
        ITyresRepository repository,
        IMapper mapper)
    {
        this.logger = logger;
        this.repository = repository;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TyreModel>>> GetAllTyres()
    {
        var tyres = await repository.GetAllTyresAsync();

        return Ok(mapper.Map<IEnumerable<Tyre>, IEnumerable<TyreModel>>(tyres));
    }

    [HttpGet("{id}", Name = "GetTyreById")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<TyreModel>> GetTyreById(int id)
    {
        var tyre = await repository.GetTyreByIdAsync(id);

        if (tyre is not null)
        {
            return Ok(mapper.Map<Tyre, TyreModel>(tyre));
        }

        return NotFound();
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult<TyreModel>> AddNewTyre(CreateTyreModel model)
    {
        var tyreEntity = mapper.Map<CreateTyreModel, Tyre>(model);

        await repository.AddNewTyreAsync(tyreEntity);

        await repository.SaveChangesAsync();

        var createdTyre = mapper.Map<Tyre, TyreModel>(tyreEntity);

        return CreatedAtRoute(
            "GetTyreById",
            new { id = createdTyre.Id },
            createdTyre);
    }

    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
    public async Task<ActionResult> UpdateTyre(CreateTyreModel model, int id)
    {
        var tyre = await repository.GetTyreByIdAsync(id);

        if (tyre is null)
        {
            return NotFound();
        }

        mapper.Map(model, tyre);

        await repository.SaveChangesAsync();

        return NoContent();
    }
}
