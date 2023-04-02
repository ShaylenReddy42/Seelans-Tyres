using AutoMapper;                                       // IMapper
using Microsoft.AspNetCore.Mvc;                         // ApiController, ControllerBase, Http Methods, ActionResult
using SeelansTyres.Services.TyresService.Services;      // ITyresRepository
using Microsoft.AspNetCore.Authorization;               // Authorize, AllowAnonymous
using Microsoft.AspNetCore.Authentication.JwtBearer;    // JwtBearerDefaults
using SeelansTyres.Services.TyresService.Data.Entities; // Tyre
using SeelansTyres.Libraries.Shared.Channels;           // PublishUpdateChannel
using SeelansTyres.Libraries.Shared.Messages;           // BaseMessage
using System.Diagnostics;                               // Stopwatch, Activity
using System.Text.Json;                                 // JsonSerializer

namespace SeelansTyres.Services.TyresService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
public class TyresController : ControllerBase
{
    private readonly ILogger<TyresController> logger;
    private readonly ITyresRepository tyresRepository;
    private readonly IMapper mapper;
    private readonly IConfiguration configuration;
    private readonly IWebHostEnvironment environment;
    private readonly PublishUpdateChannel publishUpdateChannel;
    private readonly Stopwatch stopwatch = new();

    public TyresController(
        ILogger<TyresController> logger,
        ITyresRepository tyresRepository,
        IMapper mapper,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        PublishUpdateChannel publishUpdateChannel)
    {
        this.logger = logger;
        this.tyresRepository = tyresRepository;
        this.mapper = mapper;
        this.configuration = configuration;
        this.environment = environment;
        this.publishUpdateChannel = publishUpdateChannel;
    }

    [HttpPost]
    public async Task<ActionResult<TyreModel>> CreateAsync(TyreModel tyreModel)
    {
        logger.LogInformation("API => Attempting to add a new tyre");
        
        var tyre = mapper.Map<TyreModel, Tyre>(tyreModel);

        await tyresRepository.CreateTyreAsync(tyre);

        await tyresRepository.SaveChangesAsync();

        var createdTyre = mapper.Map<Tyre, TyreModel>(tyre);

        return CreatedAtRoute(
            "RetrieveSingleAsync",
            new { id = createdTyre.Id },
            createdTyre);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<TyreModel>>> RetrieveAllAsync(bool availableOnly = true)
    {
        logger.LogInformation(
            "API => Attempting to retrieve all tyres{includingUnavailable}",
            availableOnly is false ? " including unavailable" : "");
        
        var tyres = await tyresRepository.RetrieveAllTyresAsync(availableOnly);

        return Ok(mapper.Map<IEnumerable<Tyre>, IEnumerable<TyreModel>>(tyres));
    }

    [HttpGet("{id}", Name = "RetrieveSingleAsync")]
    public async Task<ActionResult<TyreModel>> RetrieveSingleAsync(Guid id)
    {
        logger.LogInformation(
            "API => Attempting to retrieve tyre {tyreId}",
            id);
        
        var tyre = await tyresRepository.RetrieveSingleTyreAsync(id);

        if (tyre is null)
        {
            logger.LogWarning(
                "{announcement}: Tyre {tyreId} does not exist!",
                "NULL", id);
            
            return NotFound();
        }

        return Ok(mapper.Map<Tyre, TyreModel>(tyre));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAsync(TyreModel tyreModel, Guid id)
    {
        logger.LogInformation(
            "API => Attempting to update tyre {tyreId}",
            id);
        
        var tyre = await tyresRepository.RetrieveSingleTyreAsync(id);

        if (tyre is null)
        {
            logger.LogWarning(
                "{announcement}: Tyre {tyreId} does not exist!",
                "NULL", id);

            return NotFound();
        }

        mapper.Map(tyreModel, tyre);

        tyre.Brand = 
            (await tyresRepository.RetrieveAllBrandsAsync())
                .Single(brand => brand.Id == tyre.BrandId);

        await tyresRepository.SaveChangesAsync();

        logger.LogInformation("Preparing to publish the update for other microservices");

        var baseMessage = new BaseMessage
        {
            TraceId = Activity.Current!.TraceId.ToString(),
            SpanId = Activity.Current!.SpanId.ToString(),
            AccessToken = HttpContext.Request.Headers.Authorization[0]!.Replace("Bearer ", ""),
            SerializedModel = JsonSerializer.SerializeToUtf8Bytes(tyreModel),
            IdOfEntityToUpdate = id
        };

        var configurationKeyForDestination = environment.IsDevelopment() is true
                                           ? "RabbitMQ:Exchanges:UpdateTyre"
                                           : "AzureServiceBus:Topics:UpdateTyre";

        stopwatch.Start();

        await publishUpdateChannel.WriteToChannelAsync(baseMessage, configuration[configurationKeyForDestination]!);

        stopwatch.Stop();

        logger.LogInformation(
            "It took {stopwatchElapsedTime}ms to write the update to the channel",
            stopwatch.ElapsedMilliseconds);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<TyreModel>> DeleteTyreAsync(Guid id)
    {
        logger.LogInformation(
            "API => Attempting to delete tyre {tyreId}",
            id);

        await tyresRepository.DeleteTyreAsync(id);

        return NoContent();
    }
}
