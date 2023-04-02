using Microsoft.AspNetCore.Authorization; // AuthorizationHandler, AuthorizationHandlerContext, AuthorizationFailureReason()
using Microsoft.Extensions.Primitives;    // StringValues

namespace SeelansTyres.Services.OrderService.Authorization;

public class MustSatisfyOrderRetrievalRulesHandler : AuthorizationHandler<MustSatisfyOrderRetrievalRulesRequirement>
{
    private readonly HttpContext httpContext;
    private readonly ILogger<MustSatisfyOrderRetrievalRulesHandler> logger;

    public MustSatisfyOrderRetrievalRulesHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<MustSatisfyOrderRetrievalRulesHandler> logger)
    {
        httpContext = httpContextAccessor.HttpContext!;
        this.logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        MustSatisfyOrderRetrievalRulesRequirement requirement)
    {
        logger.LogInformation(
            "{announcement}: {authorizationRequirement}", 
            "AUTHORIZATION REQUIREMENT HIT", "MustSatisfyOrderRetrievalRules");
        
        httpContext.Request.Query.TryGetValue("customerId", out StringValues customerIds);

        string? customerIdFromQuery = customerIds.Count is not 0 ? customerIds[0] : null;
        var customerIdFromClaims = context.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value ?? "anonymous";

        var isAdmin = context.User.IsInRole("Administrator");

        if (isAdmin is true && customerIdFromQuery is null)
        {
            logger.LogInformation(
                "{announcement}: An administrator is allowed to retrieve all orders but not for a specific customer, rules satisfied",
                "SUCCEEDED");

            logger.LogInformation(
                "{announcement}: {authorizationRequirement}", 
                "AUTHORIZATION REQUIREMENT COMPLETED", "MustSatisfyOrderRetrievalRules");

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        else if (context.User.Identity!.IsAuthenticated is true 
            && customerIdFromClaims == customerIdFromQuery)
        {
            logger.LogInformation(
                "{announcement}: Customer {customerId} is allowed to retrieve their own orders, requirement rules satisfied",
                "SUCCEEDED", customerIdFromClaims);

            logger.LogInformation(
                "{announcement}: {authorizationRequirement}", 
                "AUTHORIZATION REQUIREMENT COMPLETED", "MustSatisfyOrderRetrievalRules");

            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        string failureMessage =
            @"
                Authorization failed based on one of the following reasons:
                1. An administrator may have tried to retrieve orders for a specific customer
                2. A customer may have tried to retrieve orders for another customer
                3. A customer may have tried to retrieve all orders, not just their own
            ".Trim();

        logger.LogWarning(
            "{announcement}: {failureMessage}",
            "FAILED", failureMessage);

        logger.LogInformation(
            "{announcement}: {authorizationRequirement}", 
            "AUTHORIZATION REQUIREMENT COMPLETED", "MustSatisfyOrderRetrievalRules");

        context.Fail(new AuthorizationFailureReason(this, failureMessage));
        return Task.CompletedTask;
    }
}
