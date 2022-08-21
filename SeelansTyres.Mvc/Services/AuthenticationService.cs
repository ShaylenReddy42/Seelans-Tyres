using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public class AuthenticationService : IAuthenticationService
{
	private readonly HttpClient client;
	private readonly IHttpContextAccessor httpContextAccessor;
	private readonly ILogger<AuthenticationService> logger;

	public AuthenticationService(
		HttpClient client,
		IHttpContextAccessor httpContextAccessor,
		ILogger<AuthenticationService> logger)
	{
		this.client = client;
		this.httpContextAccessor = httpContextAccessor;
		this.logger = logger;
	}

	public async Task<bool> LoginAsync(LoginModel login)
	{
		try
		{
			var response = await client.PostAsync("api/authentication/login", JsonContent.Create(login));
			var token = await response.Content.ReadFromJsonAsync<string>();

			httpContextAccessor.HttpContext!.Session.SetString("ApiAuthToken", token!);

			return true;
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex, "The API is unavailable");
			return false;
		}
	}
}
