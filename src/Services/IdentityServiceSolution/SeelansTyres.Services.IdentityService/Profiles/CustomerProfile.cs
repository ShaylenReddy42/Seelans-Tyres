using AutoMapper;
using SeelansTyres.Services.IdentityService.Data.Entities;

namespace SeelansTyres.Services.IdentityService.Profiles;

public class CustomerProfile : Profile
{
	public CustomerProfile()
	{
		CreateMap<Customer, CustomerModel>();
		CreateMap<RegisterModel, Customer>();
	}
}
