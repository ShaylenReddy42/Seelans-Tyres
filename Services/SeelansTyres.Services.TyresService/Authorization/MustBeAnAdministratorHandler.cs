using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;

namespace SeelansTyres.Services.TyresService.Authorization;

public class MustBeAnAdministratorHandler : AuthorizationHandler<MustBeAnAdministratorRequirement>
{
    private readonly HttpContext httpContext;

    public MustBeAnAdministratorHandler(IHttpContextAccessor httpContextAccessor)
    {
        httpContext = httpContextAccessor.HttpContext!;
    }
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        MustBeAnAdministratorRequirement requirement)
    {
        httpContext.Request.Headers.TryGetValue("X-User-Role", out StringValues userRoles);

        if (userRoles.Count is 0)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (userRoles[0] is not "Administrator")
        {
            context.Fail();
            return Task.CompletedTask;
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
