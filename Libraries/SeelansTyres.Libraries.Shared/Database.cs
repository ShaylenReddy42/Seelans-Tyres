using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SeelansTyres.Libraries.Shared;

public static class Database
{
    public static Task<WebApplication> MigrateDatabaseAsync<T>(this WebApplication app) where T : DbContext
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetService<T>();
        dbContext!.Database.Migrate();

        return Task.FromResult(app);
    }
}
