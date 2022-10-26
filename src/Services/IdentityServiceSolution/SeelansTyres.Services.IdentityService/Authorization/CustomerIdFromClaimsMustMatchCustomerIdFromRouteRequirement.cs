using Microsoft.AspNetCore.Authorization;

namespace SeelansTyres.Services.IdentityService.Authorization;

public class CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement : IAuthorizationRequirement
{
	public CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement() { }
}
