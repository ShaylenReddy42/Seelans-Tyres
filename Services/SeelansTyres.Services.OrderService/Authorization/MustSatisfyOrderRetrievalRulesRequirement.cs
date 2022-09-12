using Microsoft.AspNetCore.Authorization;

namespace SeelansTyres.Services.OrderService.Authorization;

public class MustSatisfyOrderRetrievalRulesRequirement : IAuthorizationRequirement
{
	public MustSatisfyOrderRetrievalRulesRequirement() { }
}
