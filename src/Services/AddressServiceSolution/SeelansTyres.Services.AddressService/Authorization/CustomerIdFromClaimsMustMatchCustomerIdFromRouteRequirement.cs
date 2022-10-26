using Microsoft.AspNetCore.Authorization;

namespace SeelansTyres.Services.AddressService.Authorization;

public class CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement : IAuthorizationRequirement
{
	public CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement() { }
}
