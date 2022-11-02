using SeelansTyres.Models.AddressModels.V1;
using SeelansTyres.Models.IdentityModels.V1;
using SeelansTyres.Models.OrderModels.V1;

namespace SeelansTyres.Frontends.Mvc.ViewModels;

public class AccountViewModel
{
    public UpdateAccountModel UpdateAccountModel { get; set; } = null!;
    public AddressModel AddressModel { get; set; } = null!;
    public CustomerModel Customer { get; set; } = null!;
    public IEnumerable<AddressModel> Addresses { get; set; } = null!;
    public IEnumerable<OrderModel> Orders { get; set; } = null!;
}
