using AutoMapper;                                           // IMapper
using Microsoft.AspNetCore.Mvc;                             // ApiController, ControllerBase, Http Methods, ActionResult
using SeelansTyres.Services.TyresService.Services;          // ITyresRepository
using Microsoft.AspNetCore.Authorization;                   // Authorize, AllowAnonymous
using Microsoft.AspNetCore.Authentication.JwtBearer;        // JwtBearerDefaults
using SeelansTyres.Services.TyresService.Data.Entities;     // Tyre
using System.Diagnostics;                                   // Stopwatch, Activity
using System.Text.Json;                                     // JsonSerializer
using static System.Net.Mime.MediaTypeNames;                // Application
using ShaylenReddy42.UnpublishedUpdatesManagement.Channels; // PublishUpdateChannel
using ShaylenReddy42.UnpublishedUpdatesManagement.Messages; // BaseMessage

namespace SeelansTyres.Services.TyresService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator")]
[Consumes(Application.Json)]
[Produces(Application.Json)]
public class TyresController(
    ILogger<TyresController> logger,
    ITyresRepository tyresRepository,
    IMapper mapper,
    IConfiguration configuration,
    IWebHostEnvironment environment,
    PublishUpdateChannel publishUpdateChannel) : ControllerBase
{
    private readonly Stopwatch stopwatch = new();

    /// <summary>
    /// Create a new tyre in the catalog
    /// </summary>
    /// <param name="tyreModel">The new tyre to be created</param>
    /// <response code="201">The newly created tyre</response>
    /// <returns>The newly created tyre in the form of a Task of type ActionResult of type TyreModel</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TyreModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Retrieves all tyres in the catalog
    /// </summary>
    /// <param name="availableOnly">Indicates whether to return tyres that are marked as available for purchase only or not</param>
    /// <response code="200">A list of tyres</response>
    /// <returns>A list of tyres in the form of a Task of type ActionResult of type IEnumberable of type TyreModel</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TyreModel>))]
    public async Task<ActionResult<IEnumerable<TyreModel>>> RetrieveAllAsync(bool availableOnly = true)
    {
        logger.LogInformation(
            "API => Attempting to retrieve all tyres{includingUnavailable}",
            !availableOnly ? " including unavailable" : "");
        
        var tyres = await tyresRepository.RetrieveAllTyresAsync(availableOnly);

        return Ok(mapper.Map<IEnumerable<Tyre>, IEnumerable<TyreModel>>(tyres));
    }

    /// <summary>
    /// Retrieves a tyre from the catalog
    /// </summary>
    /// <param name="id">The id of the tyre to be retrieved</param>
    /// <response code="200">The requested tyre from the catalog</response>
    /// <response code="404">Indicates that the requested tyre from the catalog doesn't exist in the database</response>
    /// <returns>The requested tyre in the form of a Task of type ActionResult of type TyreModel</returns>
    [HttpGet("{id}", Name = "RetrieveSingleAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TyreModel))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TyreModel>> RetrieveSingleAsync(Guid id)
    {
        logger.LogInformation(
            "API => Attempting to retrieve tyre {tyreId}",
            id);
        
        var tyre = await tyresRepository.RetrieveSingleTyreAsync(id);

        if (tyre is null)
        {
            logger.LogWarning(
                "{Announcement}: Tyre {tyreId} does not exist!",
                "NULL", id);
            
            return NotFound();
        }

        return Ok(mapper.Map<Tyre, TyreModel>(tyre));
    }

    /// <summary>
    /// Updates a tyre in the catalog
    /// </summary>
    /// <remarks>
    /// This action also publishes the update to a message broker to be consumed by other microservices
    /// </remarks>
    /// <param name="tyreModel">The model containing the updated tyre properties</param>
    /// <param name="id">The id of the tyre to be updated</param>
    /// <response code="204">Indicates that the tyre was updated successfully</response>
    /// <response code="404">Indicates that the tyre to be updated doesn't exist in the database</response>
    /// <returns>A Task of type ActionResult</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateAsync(TyreModel tyreModel, Guid id)
    {
        logger.LogInformation(
            "API => Attempting to update tyre {tyreId}",
            id);
        
        var tyre = await tyresRepository.RetrieveSingleTyreAsync(id);

        if (tyre is null)
        {
            logger.LogWarning(
                "{Announcement}: Tyre {tyreId} does not exist!",
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
            IdOfEntityToUpdate = id.ToString()
        };

        var configurationKeyForDestination = environment.IsDevelopment()
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

    /// <summary>
    /// Deletes a tyre from the catalog
    /// </summary>
    /// <param name="id">The id of the tyre to be deleted</param>
    /// <response code="204">Indicates that the tyre was deleted from the catalog successfully</response>
    /// <returns>A Task of type ActionResult</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<TyreModel>> DeleteTyreAsync(Guid id)
    {
        logger.LogInformation(
            "API => Attempting to delete tyre {tyreId}",
            id);

        await tyresRepository.DeleteTyreAsync(id);

        return NoContent();
    }
}
