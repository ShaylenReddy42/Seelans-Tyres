namespace SeelansTyres.Services.OrderService.Models;

public class OrderModel
{
    public int Id { get; set; }
    public DateTime OrderPlaced { get; set; }
    public decimal TotalPrice { get; set; }
    public bool Delivered { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int AddressId { get; set; }
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public ICollection<OrderItemModel> OrderItems { get; set; } = default!;
}
