using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.OrderData.Entities;

namespace SeelansTyres.Data.OrderData;

public class OrderDbContext : DbContext
{
	public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

	public DbSet<Order> Orders => Set<Order>();
	public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}
