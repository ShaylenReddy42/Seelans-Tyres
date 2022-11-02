using AutoMapper;
using SeelansTyres.Services.TyresService.Data.Entities;
using SeelansTyres.Models.TyresModels.V1;

namespace SeelansTyres.Services.TyresService.Profiles;

public class TyreProfile : Profile
{
    public TyreProfile()
    {
        CreateMap<Tyre, TyreModel>().ReverseMap();
    }
}
