using SeelansTyres.Data.Models;

namespace SeelansTyres.Mvc.Services;

public interface IAuthenticationService
{
    Task<bool> LoginAsync(LoginModel login);
}
