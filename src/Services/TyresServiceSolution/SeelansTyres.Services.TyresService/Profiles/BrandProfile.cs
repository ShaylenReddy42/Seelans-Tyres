using AutoMapper;
using SeelansTyres.Models.TyresModels.V1;
using SeelansTyres.Services.TyresService.Data.Entities;

namespace SeelansTyres.Services.TyresService.Profiles;

public class BrandProfile : Profile
{
    public BrandProfile()
    {
        CreateMap<Brand, BrandModel>().ReverseMap();
    }
}
