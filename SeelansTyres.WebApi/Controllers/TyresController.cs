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

    [HttpGet("{id}")]
    public async Task<ActionResult<TyreModel>> GetTyreById(int id)
    {
        var tyre = await repository.GetTyreByIdAsync(id);

        if (tyre is not null)
        {
            return Ok(mapper.Map<Tyre, TyreModel>(tyre));
        }

        return NotFound();
    }
}
