using Microsoft.EntityFrameworkCore;
using SeelansTyres.Services.OrderService.Data.Entities;

namespace SeelansTyres.Services.OrderService.Data;

public class OrdersContext : DbContext
{
	public OrdersContext(DbContextOptions<OrdersContext> options) : base(options) { }

	public DbSet<Order> Orders => Set<Order>();
	public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}
