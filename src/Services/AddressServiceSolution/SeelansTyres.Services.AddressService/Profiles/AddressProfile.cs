using AutoMapper;
using SeelansTyres.Data.AddressData.Entities;
using SeelansTyres.Models.AddressModels.V1;

namespace SeelansTyres.Services.AddressService.Profiles;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<Address, AddressModel>().ReverseMap();
    }
}
