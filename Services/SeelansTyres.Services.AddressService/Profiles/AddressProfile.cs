using AutoMapper;
using SeelansTyres.Services.AddressService.Models;
using SeelansTyres.Services.AddressService.Data.Entities;

namespace SeelansTyres.Services.AddressService.Profiles;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<Address, AddressModel>().ReverseMap();
    }
}
