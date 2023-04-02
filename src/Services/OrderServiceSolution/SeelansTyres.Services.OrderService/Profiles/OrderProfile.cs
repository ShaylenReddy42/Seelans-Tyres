using AutoMapper;                           // Profile, CreateMap(), ReverseMap()
using SeelansTyres.Data.OrderData.Entities; // Order

namespace SeelansTyres.Services.OrderService.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderModel>().ReverseMap();
        }
    }
}
