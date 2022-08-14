using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;
using SeelansTyres.WebApi.Services;

namespace SeelansTyres.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> logger;
    private readonly ISeelansTyresRepository repository;
    private readonly IMapper mapper;

    public OrdersController(
        ILogger<OrdersController> logger,
        ISeelansTyresRepository repository,
        IMapper mapper)
    {
        this.logger = logger;
        this.repository = repository;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderModel>>> GetAllOrders(Guid? customerId = null, bool notDeliveredOnly = false)
    {
        if (customerId is null 
            && User.IsInRole("Administrator") is false) // All orders
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
        else if (customerId is not null 
            && User.IsInRole("Administrator") is true) // Administrator getting orders for a specific customer
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
        else if (customerId is not null 
            && User.IsInRole("Administrator") is false 
            && User.Claims.First(claim => claim.Type.EndsWith("nameidentifier")).Value != customerId.ToString()) // Customer trying to get other customer's orders
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        var orders = await repository.GetAllOrdersAsync(customerId, notDeliveredOnly);

        return Ok(mapper.Map<IEnumerable<Order>, IEnumerable<OrderModel>>(orders));
    }

    [HttpGet("{id}", Name = "GetOrderById")]
    public async Task<ActionResult<OrderModel>> GetOrderById(int id)
    {
        var order = await repository.GetOrderByIdAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<Order, OrderModel>(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderModel>> AddOrder(CreateOrderModel newOrder)
    {
        var order = mapper.Map<CreateOrderModel, Order>(newOrder);

        await repository.AddNewOrderAsync(order);

        await repository.SaveChangesAsync();

        var createdOrder = mapper.Map<Order, OrderModel>(order);

        return CreatedAtRoute(
            "GetOrderById",
            new { id = createdOrder.Id },
            createdOrder);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> MarkOrderAsDelivered(int id, bool delivered = true)
    {
        if (User.IsInRole("Administrator") is false)
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }
        
        var order = await repository.GetOrderByIdAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        order.Delivered = delivered;

        await repository.SaveChangesAsync();

        return NoContent();
    }
}
