namespace SeelansTyres.Frontends.Mvc.ViewModels;

public class AdminPortalViewModel
{
    public IEnumerable<BrandModel> Brands { get; set; } = [];
    public IEnumerable<OrderModel> Orders { get; set; } = [];
    public IEnumerable<OrderModel> UndeliveredOrders => Orders.Where(order => !order.Delivered);
    public IEnumerable<TyreModel> Tyres { get; set; } = [];
}
