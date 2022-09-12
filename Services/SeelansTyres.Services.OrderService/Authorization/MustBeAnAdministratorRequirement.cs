using Microsoft.AspNetCore.Authorization;

namespace SeelansTyres.Services.OrderService.Authorization;

public class MustBeAnAdministratorRequirement : IAuthorizationRequirement
{
	public MustBeAnAdministratorRequirement() { }
}
