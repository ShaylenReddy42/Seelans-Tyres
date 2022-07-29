using AutoMapper;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;

namespace SeelansTyres.WebApi.Profiles;

public class OrderItemProfile : Profile
{
    public OrderItemProfile()
    {
        CreateMap<OrderItem, OrderItemModel>().ReverseMap();
        CreateMap<OrderItem, CreateOrderItemModel>().ReverseMap();
    }
}
