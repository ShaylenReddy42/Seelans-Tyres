using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Models;

namespace SeelansTyres.Mvc.ViewModels;

public class AccountViewModel
{
    public UpdateAccountModel UpdateAccountModel { get; set; } = null!;
    public CreateAddressModel CreateAddressModel { get; set; } = null!;
    public CustomerModel Customer { get; set; } = null!;
    public IEnumerable<AddressModel> Addresses { get; set; } = null!;
    public IEnumerable<OrderModel> Orders { get; set; } = null!;
}
