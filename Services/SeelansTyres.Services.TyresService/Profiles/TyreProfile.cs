using AutoMapper;
using SeelansTyres.Services.TyresService.Models;
using SeelansTyres.Services.TyresService.Data.Entities;

namespace SeelansTyres.WebApi.Profiles;

public class TyreProfile : Profile
{
    public TyreProfile()
    {
        CreateMap<Tyre, TyreModel>().ReverseMap();
    }
}
