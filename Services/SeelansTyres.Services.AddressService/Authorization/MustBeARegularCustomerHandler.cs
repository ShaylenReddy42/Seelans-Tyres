using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;

namespace SeelansTyres.Services.AddressService.Authorization;

public class MustBeARegularCustomerHandler : AuthorizationHandler<MustBeARegularCustomerRequirement>
{
    private readonly HttpContext httpContext;
    private readonly ILogger<MustBeARegularCustomerHandler> logger;

    public MustBeARegularCustomerHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<MustBeARegularCustomerHandler> logger)
    {
        httpContext = httpContextAccessor.HttpContext!;
        this.logger = logger;
    }
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        MustBeARegularCustomerRequirement requirement)
    {
        logger.LogInformation(
            "{announcement}: {authorizationRequirement}",
            "AUTHORIZATION REQUIREMENT HIT", "MustBeARegularCustomer");

        var customerId = context.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value;

        httpContext.Request.Headers.TryGetValue("X-User-Role", out StringValues userRoles);

        if (userRoles.Count is 0)
        {
            logger.LogInformation(
                "{announcement}: Customer {customerId} has no roles assigned, satisfying the rules for this requirement",
                "SUCCEEDED", customerId);

            logger.LogInformation(
                "{announcment}: {authorizationRequirement}",
                "AUTHORIZATION REQUIREMENT COMPLETED", "MustBeARegularCustomer");

            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (userRoles[0] is "Administrator")
        {
            logger.LogWarning(
                "{announcement}: Customer {customerId} has the administrator role assigned",
                "FAILED", customerId);

            logger.LogInformation(
                "{announcment}: {authorizationRequirement}",
                "AUTHORIZATION REQUIREMENT COMPLETED", "MustBeARegularCustomer");

            context.Fail(new AuthorizationFailureReason(this, $"Customer {customerId} has the administrator role assigned"));
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "{announcement}: Customer {customerId} has satisfied the rules regarding this requirement",
            "SUCCEEDED", customerId);

        logger.LogInformation(
            "{announcment}: {authorizationRequirement}",
            "AUTHORIZATION REQUIREMENT COMPLETED", "MustBeARegularCustomer");

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
