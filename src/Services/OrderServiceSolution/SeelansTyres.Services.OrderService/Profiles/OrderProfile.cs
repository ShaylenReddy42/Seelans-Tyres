using AutoMapper;
using SeelansTyres.Services.OrderService.Data.Entities;
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
