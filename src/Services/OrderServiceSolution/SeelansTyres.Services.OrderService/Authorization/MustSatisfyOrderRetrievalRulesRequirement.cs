using Microsoft.AspNetCore.Authorization; // IAuthorizationRequirement

namespace SeelansTyres.Services.OrderService.Authorization;

public class MustSatisfyOrderRetrievalRulesRequirement : IAuthorizationRequirement
{
	public MustSatisfyOrderRetrievalRulesRequirement() { }
}
