using Microsoft.AspNetCore.Authorization;

namespace SeelansTyres.Services.AddressService.Authorization;

public class MustBeARegularCustomerRequirement : IAuthorizationRequirement
{
	public MustBeARegularCustomerRequirement() { }
}
