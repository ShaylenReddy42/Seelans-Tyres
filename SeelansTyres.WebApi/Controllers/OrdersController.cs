using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;
using SeelansTyres.WebApi.Services;

namespace SeelansTyres.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
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
    public async Task<ActionResult<IEnumerable<OrderModel>>> GetAllOrders(Guid? customerId = null)
    {
        var orders = await repository.GetAllOrdersAsync(customerId);

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

        logger.LogWarning("AFTER MAPPING");

        return CreatedAtRoute(
            "GetOrderById",
            new { id = createdOrder.Id },
            createdOrder);
    }
}
