using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.WebApi.Services;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;

namespace SeelansTyres.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TyresController : ControllerBase
{
    private readonly ILogger<TyresController> logger;
    private readonly ISeelansTyresRepository repository;
    private readonly IMapper mapper;

    public TyresController(
        ILogger<TyresController> logger,
        ISeelansTyresRepository repository,
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
}
