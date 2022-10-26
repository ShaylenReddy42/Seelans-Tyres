using Microsoft.AspNetCore.Authorization;

namespace SeelansTyres.Services.TyresService.Authorization;

public class MustBeAnAdministratorRequirement : IAuthorizationRequirement
{
	public MustBeAnAdministratorRequirement() { }
}
