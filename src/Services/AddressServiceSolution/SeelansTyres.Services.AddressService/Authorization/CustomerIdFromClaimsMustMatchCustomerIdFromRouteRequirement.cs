using Microsoft.AspNetCore.Authorization; // IAuthorizationRequirement

namespace SeelansTyres.Services.AddressService.Authorization;

public class CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement : IAuthorizationRequirement
{
	public CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement() { }
}
