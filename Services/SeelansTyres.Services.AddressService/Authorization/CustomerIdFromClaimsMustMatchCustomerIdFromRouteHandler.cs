using Microsoft.AspNetCore.Authorization;

namespace SeelansTyres.Services.AddressService.Authorization;

public class CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler : AuthorizationHandler<CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement>
{
	private readonly HttpContext httpContext;

	public CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler(IHttpContextAccessor httpContextAccessor)
	{
		httpContext = httpContextAccessor.HttpContext!;
	}

	protected override Task HandleRequirementAsync(
		AuthorizationHandlerContext context, 
		CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement requirement)
	{
		var customerIdFromClaims = context.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value;
		var customerIdFromRoute = httpContext.GetRouteValue("customerId")!.ToString();

		if (customerIdFromClaims != customerIdFromRoute)
		{
			context.Fail();
			return Task.CompletedTask;
		}

		context.Succeed(requirement);
		return Task.CompletedTask;
	}
}
