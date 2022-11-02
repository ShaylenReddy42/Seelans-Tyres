using AutoMapper;
using SeelansTyres.Data.OrderData.Entities;
using SeelansTyres.Models.OrderModels.V1;

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
