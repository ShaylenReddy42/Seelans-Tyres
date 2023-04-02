using Microsoft.AspNetCore.Authorization; // AuthorizationHandler, AuthorizationHandlerContext, AuthorizationFailureReason()

namespace SeelansTyres.Services.IdentityService.Authorization;

public class CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler : AuthorizationHandler<CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement>
{
    private readonly HttpContext httpContext;
    private readonly ILogger<CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler> logger;

    public CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler> logger)
    {
        httpContext = httpContextAccessor.HttpContext!;
        this.logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement requirement)
    {
        logger.LogInformation(
            "{announcement}: {authorizationRequirement}",
            "AUTHORIZATION REQUIREMENT HIT", "CustomerIdFromClaimsMustMatchCustomerIdFromRoute");

        var customerIdFromClaims = context.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value;
        var customerIdFromRoute = httpContext.GetRouteValue("id")!.ToString();

        if (customerIdFromClaims != customerIdFromRoute)
        {
            logger.LogWarning(
                "{announcement}: CustomerId from claims {customerIdFromClaims} does not match customerId from route {customerIdFromRoute}",
                "FAILED", customerIdFromClaims, customerIdFromRoute);

            logger.LogInformation(
                "{announcement}: {authorizationRequirement}",
                "AUTHORIZATION REQUIREMENT COMPLETED", "CustomerIdFromClaimsMustMatchCustomerIdFromRoute");

            context.Fail(new AuthorizationFailureReason(this, $"CustomerId from claims does not match customerId from route"));
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "{announcement}: customerId from claims matches customerId from route, satisfying the rules for this requirement",
            "SUCCEEDED");

        logger.LogInformation(
            "{announcement}: {authorizationRequirement}",
            "AUTHORIZATION REQUIREMENT COMPLETED", "CustomerIdFromClaimsMustMatchCustomerIdFromRoute");

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
