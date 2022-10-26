using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Primitives;

namespace SeelansTyres.Services.TyresService.Authorization;

public class MustBeAnAdministratorHandler : AuthorizationHandler<MustBeAnAdministratorRequirement>
{
    private readonly HttpContext httpContext;
    private readonly ILogger<MustBeAnAdministratorHandler> logger;

    public MustBeAnAdministratorHandler(
        IHttpContextAccessor httpContextAccessor,
        ILogger<MustBeAnAdministratorHandler> logger)
    {
        httpContext = httpContextAccessor.HttpContext!;
        this.logger = logger;
    }
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        MustBeAnAdministratorRequirement requirement)
    {
        logger.LogInformation(
            "{announcement}: {authorizationRequirement}",
            "AUTHORIZATION REQUIREMENT HIT", "MustBeAnAdministrator");
        
        httpContext.Request.Headers.TryGetValue("X-User-Role", out StringValues userRoles);

        if (userRoles.Count is not 0 && userRoles[0] is "Administrator")
        {
            logger.LogInformation(
                "{announcement}: Authenticated user is an administrator, satisfying the requirement",
                "SUCCEEDED");

            logger.LogInformation(
                "{announcement}: {authorizationRequirement}",
                "AUTHORIZATION REQUIREMENT COMPLETED", "MustBeAnAdministrator");
            
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        logger.LogWarning(
            "{announcement}: Customer {customerId} tried to access resources designated to an administrator",
            "FAILED", context.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")));

        logger.LogInformation(
            "{announcement}: {authorizationRequirement}",
            "AUTHORIZATION REQUIREMENT COMPLETED", "MustBeAnAdministrator");

        context.Fail(new AuthorizationFailureReason(this, "Unauthorized"));
        return Task.CompletedTask;
    }
}
