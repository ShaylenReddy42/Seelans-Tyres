using AutoMapper;
using SeelansTyres.Data.OrderData.Entities;

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
