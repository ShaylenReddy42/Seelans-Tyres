using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Data.Models
{
    public class CreateOrderModel
    {
        [Required]
        [Column(TypeName = "decimal")]
        public decimal TotalPrice { get; set; }
        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public int AddressId { get; set; }
        public ICollection<CreateOrderItemModel> OrderItems { get; set; } = new List<CreateOrderItemModel>();
    }
}
