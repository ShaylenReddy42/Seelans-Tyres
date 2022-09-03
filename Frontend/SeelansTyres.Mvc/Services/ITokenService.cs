using SeelansTyres.Mvc.Data.Entities;

namespace SeelansTyres.Mvc.Services;

public interface ITokenService
{
    void GenerateApiAuthToken(Customer customer, bool isAdmin);
}
