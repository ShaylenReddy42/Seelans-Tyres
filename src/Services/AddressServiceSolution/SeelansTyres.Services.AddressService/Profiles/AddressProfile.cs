using AutoMapper;
using SeelansTyres.Data.AddressData.Entities;

namespace SeelansTyres.Services.AddressService.Profiles;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<Address, AddressModel>().ReverseMap();
    }
}
