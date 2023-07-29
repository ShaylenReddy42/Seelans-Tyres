using Microsoft.AspNetCore.Authorization; // IAuthorizationRequirement

namespace SeelansTyres.Libraries.Shared.Authorization;

public class CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement : IAuthorizationRequirement
{
    public string RouteValueKeyName { get; }

    public CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement(string routeValueKeyName)
    {
        RouteValueKeyName = routeValueKeyName;
    }
}
