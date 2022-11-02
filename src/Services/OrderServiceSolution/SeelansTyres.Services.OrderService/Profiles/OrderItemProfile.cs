using AutoMapper;
using SeelansTyres.Data.OrderData.Entities;
using SeelansTyres.Models.OrderModels.V1;

namespace SeelansTyres.Services.OrderService.Profiles;

public class OrderItemProfile : Profile
{
    public OrderItemProfile()
    {
        CreateMap<OrderItem, OrderItemModel>().ReverseMap();
    }
}
