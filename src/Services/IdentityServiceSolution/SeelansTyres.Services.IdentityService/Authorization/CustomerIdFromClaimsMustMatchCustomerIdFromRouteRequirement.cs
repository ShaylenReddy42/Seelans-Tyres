using Microsoft.AspNetCore.Authorization; // IAuthorizationRequirement

namespace SeelansTyres.Services.IdentityService.Authorization;

public class CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement : IAuthorizationRequirement
{
	public CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement() { }
}
