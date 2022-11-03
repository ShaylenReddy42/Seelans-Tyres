namespace SeelansTyres.Frontends.Mvc.ViewModels;

public class AdminPortalViewModel
{
    public IEnumerable<BrandModel> Brands { get; set; } = new List<BrandModel>();
    public IEnumerable<OrderModel> Orders { get; set; } = new List<OrderModel>();
    public IEnumerable<OrderModel> UndeliveredOrders => Orders.Where(order => !order.Delivered);
    public IEnumerable<TyreModel> Tyres { get; set; } = new List<TyreModel>();
}
