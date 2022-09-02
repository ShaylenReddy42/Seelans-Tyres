using AutoMapper;
using SeelansTyres.Services.OrderService.Data.Entities;
using SeelansTyres.Services.OrderService.Models;

namespace SeelansTyres.WebApi.Profiles;

public class OrderItemProfile : Profile
{
    public OrderItemProfile()
    {
        CreateMap<OrderItem, OrderItemModel>().ReverseMap();
        CreateMap<OrderItem, CreateOrderItemModel>().ReverseMap();
    }
}
