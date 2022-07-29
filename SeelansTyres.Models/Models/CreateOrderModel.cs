using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeelansTyres.Data.Models
{
    public class CreateOrderModel
    {
        [Required]
        public DateTime Time { get; set; }
        [Required]
        public int TotalItems { get; set; }
        [Required]
        [Column(TypeName = "decimal")]
        public decimal TotalPrice { get; set; }
        [Required]
        public bool Delivered { get; set; }
        public CustomerModel? Customer { get; set; }
        public AddressModel? Address { get; set; }
        public ICollection<CreateOrderItemModel> OrderItems { get; set; } = null!;
    }
}
