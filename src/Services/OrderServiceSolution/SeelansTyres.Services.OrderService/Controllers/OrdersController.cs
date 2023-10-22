using AutoMapper;                                    // IMapper
using Microsoft.AspNetCore.Authentication.JwtBearer; // JwtBearerDefaults
using Microsoft.AspNetCore.Authorization;            // Authorize
using Microsoft.AspNetCore.Mvc;                      // ApiController, ControllerBase, Http Methods, ActionResult
using SeelansTyres.Data.OrderData.Entities;          // Order
using SeelansTyres.Services.OrderService.Services;   // IOrderRepository
using static System.Net.Mime.MediaTypeNames;         // Application

namespace SeelansTyres.Services.OrderService.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Consumes(Application.Json)]
[Produces(Application.Json)]
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

    /// <summary>
    /// Creates an order for a particular customer
    /// </summary>
    /// <param name="newOrder">The order to be created for a customer</param>
    /// <response code="201">Indicates that the order was created successfully and is returned</response>
    /// <returns>The newly created order in the form of a Task of type ActionResult of type OrderModel</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(OrderModel))]
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

    /// <summary>
    /// Retrieves all orders or orders for a particular customer
    /// </summary>
    /// <remarks>
    /// This route must satisfy the following authorization rules:  
    ///     1. An administrator can retrieve all orders, but not for a particular customer  
    ///     2. A customer is only allowed to retrieve their own orders, but not another customer's or all orders
    /// </remarks>
    /// <param name="customerId">The id of the customer used to filter for their orders [OPTIONAL]</param>
    /// <param name="notDeliveredOnly">Indicates whether only the orders that have not been delivered should be returned or not</param>
    /// <response code="200">A list of orders</response>
    /// <returns>A list of orders in the form of a Task of type ActionResult of type IEnumberable of type OrderModel</returns>
    [HttpGet]
    [Authorize(Policy = "MustSatisfyOrderRetrievalRules")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<OrderModel>))]
    public async Task<ActionResult<IEnumerable<OrderModel>>> RetrieveAll(Guid? customerId = null, bool notDeliveredOnly = false)
    {
        logger.LogInformation(
            "API => Attempting to retrieve all orders{for}{customerId}{exceptDelivered}",
            customerId is not null ? " for customer " : "", customerId is not null ? customerId : "", notDeliveredOnly ? " except delivered ones" : "");

        var orders = await orderRepository.RetrieveAllAsync(customerId, notDeliveredOnly);

        return Ok(mapper.Map<IEnumerable<Order>, IEnumerable<OrderModel>>(orders));
    }

    /// <summary>
    /// Retrieves a particular order
    /// </summary>
    /// <param name="id">The id of the requested order</param>
    /// <response code="200">The requested order</response>
    /// <response code="404">Indicates that the requested order doesn't exist in the database</response>
    /// <returns>An order in the form of a Task of type ActionResult of type OrderModel</returns>
    [HttpGet("{id}", Name = "GetOrderById")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrderModel))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderModel>> RetrieveSingle(int id)
    {
        logger.LogInformation(
            "API => Attempting to retrieve order {orderId}",
            id);

        var order = await orderRepository.RetrieveSingleAsync(id);

        return order is not null ? Ok(mapper.Map<Order, OrderModel>(order)) : NotFound();
    }

    /// <summary>
    /// Updates an order to mark it as delivered or not
    /// </summary>
    /// <remarks>
    /// This route must satisfy the following authorization rules:  
    ///     1. The user must be an administrator to perform this action
    /// </remarks>
    /// <param name="id">The id of the order to be marked as delivered</param>
    /// <param name="delivered">Indicates whether to mark the order as delivered or not [OPTIONAL: true]</param>
    /// <response code="204">Indicates the order was marked accordingly</response>
    /// <response code="404">Indicates that the order to be marked as delivered doesn't exist in the database</response>
    /// <returns>A task of type ActionResult</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
