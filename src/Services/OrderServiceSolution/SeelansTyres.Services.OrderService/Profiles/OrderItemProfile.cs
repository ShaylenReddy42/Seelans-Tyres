using AutoMapper;
using SeelansTyres.Data.OrderData.Entities;
using SeelansTyres.Services.OrderService.Models;

namespace SeelansTyres.Services.OrderService.Profiles;

public class OrderItemProfile : Profile
{
    public OrderItemProfile()
    {
        CreateMap<OrderItem, OrderItemModel>().ReverseMap();
    }
}
