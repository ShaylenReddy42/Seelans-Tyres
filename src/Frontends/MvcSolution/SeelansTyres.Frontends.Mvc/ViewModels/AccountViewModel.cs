namespace SeelansTyres.Frontends.Mvc.ViewModels;

public class AccountViewModel
{
    public UpdateAccountModel UpdateAccountModel { get; set; } = null!;
    public AddressModel AddressModel { get; set; } = null!;
    public CustomerModel Customer { get; set; } = null!;
    public IEnumerable<AddressModel> Addresses { get; set; } = null!;
    public IEnumerable<OrderModel> Orders { get; set; } = null!;
}
