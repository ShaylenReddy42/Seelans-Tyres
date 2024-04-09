using Microsoft.AspNetCore.Authorization; // AuthorizationHandler, AuthorizationHandlerContext, AuthorizationFailureReason()
using Microsoft.AspNetCore.Http;          // IHttpContextAccessor
using Microsoft.AspNetCore.Routing;       // GetRouteValue()
using Microsoft.Extensions.Logging;       // ILogger

namespace SeelansTyres.Libraries.Shared.Authorization;

public class CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler : AuthorizationHandler<CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement>
{
	private readonly IHttpContextAccessor httpContextAccessor;
	private readonly ILogger<CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler> logger;

	public CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler(
		IHttpContextAccessor httpContextAccessor,
		ILogger<CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler> logger)
	{
        this.httpContextAccessor = httpContextAccessor;
		this.logger = logger;
	}

	protected override Task HandleRequirementAsync(
		AuthorizationHandlerContext context, 
		CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement requirement)
	{
        logger.LogInformation(
            "{Announcement}: {authorizationRequirement}",
            "AUTHORIZATION REQUIREMENT HIT", "CustomerIdFromClaimsMustMatchCustomerIdFromRoute");

        var customerIdFromClaims = context.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value;
        var customerIdFromRoute = httpContextAccessor.HttpContext!.GetRouteValue(requirement.RouteValueKeyName)!.ToString();

        if (customerIdFromClaims != customerIdFromRoute)
        {
            logger.LogWarning(
                "{Announcement}: CustomerId from claims {customerIdFromClaims} does not match customerId from route {customerIdFromRoute}",
                "FAILED", customerIdFromClaims, customerIdFromRoute);

            logger.LogInformation(
                "{Announcement}: {authorizationRequirement}",
                "AUTHORIZATION REQUIREMENT COMPLETED", "CustomerIdFromClaimsMustMatchCustomerIdFromRoute");

            context.Fail(new AuthorizationFailureReason(this, $"CustomerId from claims does not match customerId from route"));
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "{Announcement}: customerId from claims matches customerId from route, satisfying the rules for this requirement",
            "SUCCEEDED");

        logger.LogInformation(
            "{Announcement}: {authorizationRequirement}",
            "AUTHORIZATION REQUIREMENT COMPLETED", "CustomerIdFromClaimsMustMatchCustomerIdFromRoute");

        context.Succeed(requirement);
        return Task.CompletedTask;
	}
}
