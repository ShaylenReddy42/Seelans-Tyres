using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;

namespace SeelansTyres.Services.AddressService.Authorization;

public class MustBeARegularCustomerHandler : AuthorizationHandler<MustBeARegularCustomerRequirement>
{
    private readonly HttpContext httpContext;

    public MustBeARegularCustomerHandler(IHttpContextAccessor httpContextAccessor)
    {
        httpContext = httpContextAccessor.HttpContext!;
    }
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        MustBeARegularCustomerRequirement requirement)
    {
        httpContext.Request.Headers.TryGetValue("X-User-Role", out StringValues userRoles);

        if (userRoles.Count is 0)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (userRoles[0] is "Administrator")
        {
            context.Fail();
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
