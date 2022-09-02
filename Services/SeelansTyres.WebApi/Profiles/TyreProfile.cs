using AutoMapper;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;

namespace SeelansTyres.WebApi.Profiles;

public class TyreProfile : Profile
{
    public TyreProfile()
    {
        CreateMap<Tyre, TyreModel>().ReverseMap();
        CreateMap<Tyre, CreateTyreModel>().ReverseMap();
    }
}
