using AutoMapper;                                       // Profile, CreateMap(), ReverseMap()
using SeelansTyres.Services.TyresService.Data.Entities; // Brand

namespace SeelansTyres.Services.TyresService.Profiles;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        CreateMap<Brand, BrandModel>().ReverseMap();
    }
}
