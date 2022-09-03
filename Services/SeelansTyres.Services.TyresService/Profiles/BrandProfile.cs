using AutoMapper;
using SeelansTyres.Services.TyresService.Models;
using SeelansTyres.Services.TyresService.Data.Entities;

namespace SeelansTyres.WebApi.Profiles;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        CreateMap<Brand, BrandModel>().ReverseMap();
    }
}
