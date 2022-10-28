using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.OrderData.Entities;

namespace SeelansTyres.Data.OrderData;

public class OrdersContext : DbContext
{
	public OrdersContext(DbContextOptions<OrdersContext> options) : base(options) { }

	public DbSet<Order> Orders => Set<Order>();
	public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}
