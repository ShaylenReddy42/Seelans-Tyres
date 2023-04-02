using AutoMapper;                                          // Profile, CreateMap()
using SeelansTyres.Services.IdentityService.Data.Entities; // Customer

namespace SeelansTyres.Services.IdentityService.Profiles;

public class CustomerProfile : Profile
{
	public CustomerProfile()
	{
		CreateMap<Customer, CustomerModel>();
		CreateMap<RegisterModel, Customer>();
	}
}
