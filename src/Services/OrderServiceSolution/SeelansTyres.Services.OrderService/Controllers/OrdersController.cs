using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.OrderData.Entities;
using SeelansTyres.Services.OrderService.Models;
using SeelansTyres.Services.OrderService.Services;

namespace SeelansTyres.Services.OrderService.Controllers;

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
    [Authorize(Policy = "MustSatisfyOrderRetrievalRules")]
    public async Task<ActionResult<IEnumerable<OrderModel>>> RetrieveAll(Guid? customerId = null, bool notDeliveredOnly = false)
    {
        logger.LogInformation(
            "API => Attempting to retrieve all orders{for}{customerId}{exceptDelivered}",
            customerId is not null ? " for customer " : "", customerId is not null ? customerId : "", notDeliveredOnly is true ? " except delivered ones" : "");

        var orders = await orderRepository.RetrieveAllAsync(customerId, notDeliveredOnly);

        return Ok(mapper.Map<IEnumerable<Order>, IEnumerable<OrderModel>>(orders));
    }

    [HttpGet("{id}", Name = "GetOrderById")]
    public async Task<ActionResult<OrderModel>> RetrieveSingle(int id)
    {
        logger.LogInformation(
            "API => Attempting to retrieve order {orderId}",
            id);

        var order = await orderRepository.RetrieveSingleAsync(id);

        return order is not null ? Ok(mapper.Map<Order, OrderModel>(order)) : NotFound();
    }

    [HttpPost]
    public async Task<ActionResult<OrderModel>> Create(OrderModel newOrder)
    {
        logger.LogInformation("API => Attempting to place a new order");

        var order = mapper.Map<OrderModel, Order>(newOrder);

        await orderRepository.CreateAsync(order);

        await orderRepository.SaveChangesAsync();

        var createdOrder = mapper.Map<Order, OrderModel>(order);

        return CreatedAtRoute(
            "GetOrderById",
            new { id = createdOrder.Id },
            createdOrder);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "MustBeAnAdministrator")]
    public async Task<ActionResult> MarkOrderAsDelivered(int id, bool delivered = true)
    {
        logger.LogInformation(
            "API => Attempting to mark order {orderId} as delivered",
            id);

        var order = await orderRepository.RetrieveSingleAsync(id);

        if (order is null)
        {
            logger.LogWarning(
                "{announcement}: Order {orderId} does not exist!",
                "NULL", id);
            
            return NotFound();
        }

        order.Delivered = delivered;

        await orderRepository.SaveChangesAsync();

        return NoContent();
    }
}
