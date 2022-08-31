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
    private readonly IOrderRepository orderRepository;
    private readonly IMapper mapper;

    public OrdersController(
        ILogger<OrdersController> logger,
        IOrderRepository orderRepository,
        IMapper mapper)
    {
        this.logger = logger;
        this.orderRepository = orderRepository;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderModel>>> RetrieveAll(Guid? customerId = null, bool notDeliveredOnly = false)
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

        var orders = await orderRepository.RetrieveAllAsync(customerId, notDeliveredOnly);

        return Ok(mapper.Map<IEnumerable<Order>, IEnumerable<OrderModel>>(orders));
    }

    [HttpGet("{id}", Name = "GetOrderById")]
    public async Task<ActionResult<OrderModel>> RetrieveSingle(int id)
    {
        var order = await orderRepository.RetrieveSingleAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<Order, OrderModel>(order));
    }

    [HttpPost]
    public async Task<ActionResult<OrderModel>> Create(CreateOrderModel newOrder)
    {
        var order = mapper.Map<CreateOrderModel, Order>(newOrder);

        await orderRepository.CreateAsync(order);

        await orderRepository.SaveChangesAsync();

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
        
        var order = await orderRepository.RetrieveSingleAsync(id);

        if (order is null)
        {
            return NotFound();
        }

        order.Delivered = delivered;

        await orderRepository.SaveChangesAsync();

        return NoContent();
    }
}
