using AutoMapper;                                       // Profile, CreateMap(), ReverseMap()
using SeelansTyres.Services.TyresService.Data.Entities; // Tyre

namespace SeelansTyres.Services.TyresService.Profiles;

public class TyreProfile : Profile
{
    public TyreProfile()
    {
        CreateMap<Tyre, TyreModel>().ReverseMap();
    }
}
