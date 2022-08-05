using AutoMapper;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;

namespace SeelansTyres.WebApi.Profiles;

public class CartItemProfile : Profile
{
    public CartItemProfile()
    {
        CreateMap<CartItem, CartItemModel>().ReverseMap();
        CreateMap<CartItem, CreateCartItemModel>().ReverseMap();
    }
}
