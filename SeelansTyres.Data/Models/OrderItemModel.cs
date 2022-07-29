using SeelansTyres.Data.Entities;

namespace SeelansTyres.Data.Models
{
    public class OrderItemModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public Tyre? Tyre { get; set; }
    }
}