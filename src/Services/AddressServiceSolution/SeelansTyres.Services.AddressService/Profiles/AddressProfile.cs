using AutoMapper;                             // Profile, CreateMap(), ReverseMap()
using SeelansTyres.Data.AddressData.Entities; // Address

namespace SeelansTyres.Services.AddressService.Profiles;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<Address, AddressModel>().ReverseMap();
    }
}
