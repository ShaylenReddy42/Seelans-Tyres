using Microsoft.AspNetCore.Authorization;      // AuthorizationHandler, AuthorizationHandlerContext, AuthorizationFailureReason()
using Microsoft.Extensions.Primitives;         // StringValues
using SeelansTyres.Libraries.Shared.Constants; // LoggerConstants

namespace SeelansTyres.Services.OrderService.Authorization;

public class MustSatisfyOrderRetrievalRulesHandler(
    IHttpContextAccessor httpContextAccessor,
    ILogger<MustSatisfyOrderRetrievalRulesHandler> logger) : AuthorizationHandler<MustSatisfyOrderRetrievalRulesRequirement>
{

    private const string AuthorizationRequirementName = "MustSatisfyOrderRetrievalRules";
    private const string AnnouncementLoggerTemplate = "{Announcement}: {AuthorizationRequirement}";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        MustSatisfyOrderRetrievalRulesRequirement requirement)
    {
        logger.LogInformation(
            AnnouncementLoggerTemplate, 
            "AUTHORIZATION REQUIREMENT HIT", AuthorizationRequirementName);

        httpContextAccessor.HttpContext!.Request.Query.TryGetValue("customerId", out StringValues customerIds);

        string? customerIdFromQuery = customerIds.Count is not 0 ? customerIds[0] : null;
        var customerIdFromClaims = context.User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value ?? "anonymous";

        var isAdmin = context.User.IsInRole("Administrator");

        if (isAdmin && customerIdFromQuery is null)
        {
            logger.LogInformation(
                "{Announcement}: An administrator is allowed to retrieve all orders but not for a specific customer, rules satisfied",
                LoggerConstants.SucceededAnnouncement);

            logger.LogInformation(
                AnnouncementLoggerTemplate, 
                "AUTHORIZATION REQUIREMENT COMPLETED", AuthorizationRequirementName);

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        else if (context.User.Identity!.IsAuthenticated 
            && customerIdFromClaims == customerIdFromQuery)
        {
            logger.LogInformation(
                "{Announcement}: Customer {CustomerId} is allowed to retrieve their own orders, requirement rules satisfied",
                LoggerConstants.SucceededAnnouncement, customerIdFromClaims);

            logger.LogInformation(
                AnnouncementLoggerTemplate, 
                "AUTHORIZATION REQUIREMENT COMPLETED", AuthorizationRequirementName);

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
            "{Announcement}: {FailureMessage}",
            LoggerConstants.FailedAnnouncement, failureMessage);

        logger.LogInformation(
            AnnouncementLoggerTemplate, 
            "AUTHORIZATION REQUIREMENT COMPLETED", AuthorizationRequirementName);

        context.Fail(new AuthorizationFailureReason(this, failureMessage));
        return Task.CompletedTask;
    }
}
