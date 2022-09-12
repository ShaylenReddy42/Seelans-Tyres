using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;

namespace SeelansTyres.Services.OrderService.Authorization;

public class MustSatisfyOrderRetrievalRulesHandler : AuthorizationHandler<MustSatisfyOrderRetrievalRulesRequirement>
{
    private readonly HttpContext httpContext;

    public MustSatisfyOrderRetrievalRulesHandler(IHttpContextAccessor httpContextAccessor) =>
        httpContext = httpContextAccessor.HttpContext!;
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        MustSatisfyOrderRetrievalRulesRequirement requirement)
    {
        httpContext.Request.Query.TryGetValue("customerId", out StringValues customerIds);

        string? customerIdFromQuery = customerIds.Count is not 0 ? customerIds[0] : null;
        bool isAdmin = default;
        var customerIdFromClaims = context.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value;

        httpContext.Request.Headers.TryGetValue("X-User-Role", out StringValues userRoles);

        if (userRoles.Count is not 0)
        {
            isAdmin = userRoles[0] is "Administrator";
        }

        if (customerIdFromQuery is null && isAdmin is false) // All orders
        {
            context.Fail();
            return Task.CompletedTask;
        }
        else if (customerIdFromQuery is not null && isAdmin is true) // Administrator getting orders for a specific customer
        {
            context.Fail();
            return Task.CompletedTask;
        }
        else if (customerIdFromQuery is not null 
            && isAdmin is false
            && customerIdFromClaims != customerIdFromQuery) // Customer trying to get other customer's orders
        {
            context.Fail();
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
