using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Services.TyresService.Services;
using SeelansTyres.Services.TyresService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SeelansTyres.Services.TyresService.Data.Entities;
using SeelansTyres.Libraries.Shared.Services;
using SeelansTyres.Libraries.Shared.Models;
using Microsoft.AspNetCore.Authentication;
using SeelansTyres.Libraries.Shared.Messages;
using System.Diagnostics;
using System.Text.Json;

namespace SeelansTyres.Services.TyresService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "MustBeAnAdministrator")]
public class TyresController : ControllerBase
{
    private readonly ILogger<TyresController> logger;
    private readonly ITyresRepository tyresRepository;
    private readonly IMapper mapper;
    private readonly IConfiguration configuration;
    private readonly IMessagingServicePublisher messagingServicePublisher;
    private readonly RabbitMQSettingsModel rabbitMQSettingsModel;

    public TyresController(
        ILogger<TyresController> logger,
        ITyresRepository tyresRepository,
        IMapper mapper,
        IConfiguration configuration,
        IMessagingServicePublisher messagingServicePublisher)
    {
        this.logger = logger;
        this.tyresRepository = tyresRepository;
        this.mapper = mapper;
        this.configuration = configuration;
        this.messagingServicePublisher = messagingServicePublisher;

        rabbitMQSettingsModel = new()
        {
            UserName = configuration["RabbitMQ:Credentials:UserName"],
            Password = configuration["RabbitMQ:Credentials:Password"],

            HostName = configuration["RabbitMQ:ConnectionProperties:HostName"],
            Port = configuration.GetValue<int>("RabbitMQ:ConnectionProperties:Port")
        };
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
            ActivityTraceId = Activity.Current!.TraceId.ToString(),
            AccessToken = HttpContext.Request.Headers.Authorization[0].Replace("Bearer ", ""),
            SerializedModel = JsonSerializer.SerializeToUtf8Bytes(tyreModel),
            IdOfEntityToUpdate = id
        };

        rabbitMQSettingsModel.Exchange = configuration["RabbitMQ:Exchanges:UpdateTyre"];

        await messagingServicePublisher.PublishMessageAsync(baseMessage, rabbitMQSettingsModel);

        return NoContent();
    }
}
