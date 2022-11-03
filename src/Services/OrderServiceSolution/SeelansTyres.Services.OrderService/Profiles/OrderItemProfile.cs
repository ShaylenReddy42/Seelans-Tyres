using AutoMapper;
using SeelansTyres.Data.OrderData.Entities;

namespace SeelansTyres.Services.OrderService.Profiles;

public class OrderItemProfile : Profile
{
    public OrderItemProfile()
    {
        CreateMap<OrderItem, OrderItemModel>().ReverseMap();
    }
}
