using AutoMapper;                           // Profile, CreateMap(), ReverseMap()
using SeelansTyres.Data.OrderData.Entities; // OrderItem

namespace SeelansTyres.Services.OrderService.Profiles;

public class OrderItemProfile : Profile
{
    public OrderItemProfile()
    {
        CreateMap<OrderItem, OrderItemModel>().ReverseMap();
    }
}
