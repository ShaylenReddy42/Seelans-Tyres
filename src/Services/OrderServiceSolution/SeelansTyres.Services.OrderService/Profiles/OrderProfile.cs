using AutoMapper;
using SeelansTyres.Data.OrderData.Entities;
using SeelansTyres.Services.OrderService.Models;

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
