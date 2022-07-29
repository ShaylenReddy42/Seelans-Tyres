using SeelansTyres.Data.Entities;

namespace SeelansTyres.Data.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalPrice { get; set; }
        public bool Delivered { get; set; }
        public CustomerModel? Customer { get; set; }
        public AddressModel? Address { get; set; }
        public ICollection<OrderItemModel> OrderItems { get; set; } = null!;
    }
}
