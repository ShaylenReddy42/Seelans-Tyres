using Microsoft.AspNetCore.Authentication.JwtBearer; // JwtBearerDefaults
using Microsoft.AspNetCore.Builder;                  // WebApplication
using Microsoft.Extensions.DependencyInjection;      // IServiceCollection
using Microsoft.OpenApi.Models;                      // OpenApiSecurityScheme, SecuritySchemeType, OpenApiSecurityRequirement, OpenApiReference, ReferenceType

namespace SeelansTyres.Libraries.Shared;

public static class Swagger
{
    /// <summary>
    /// Adds a configured Swagger generator with the jwt security definition to the service collection
    /// </summary>
    /// <remarks>
    ///     There is a way to configure a security definition and requirement using OpenID Connect<br/>
    ///     and IdentityServer4, however, it's not added to avoid needing to create additional clients,<br/>
    ///     and having an additional strong dependency health check from the apis to the identity service<br/>
    ///     in the application's developer environment
    /// </remarks>
    /// <param name="services">The service collection of the web application builder</param>
    /// <returns>The service collection with the added Swagger generator</returns>
    public static IServiceCollection AddCommonSwaggerGen(this IServiceCollection services)
    {
        var jwtSecurityDefinitionName = "JwtBearerAuth";

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(setup =>
        {
            setup.AddSecurityDefinition(jwtSecurityDefinitionName, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "Input a valid token to access this API"
            });

            setup.AddSecurityRequirement(new OpenApiSecurityRequirement
            {{
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = jwtSecurityDefinitionName
                    }
                },
                new List<string>()
            }});
        });

        return services;
    }

    /// <summary>
    /// Adds the Swagger UI to the web application
    /// </summary>
    /// <remarks>
    ///     For all apis other than the one on the identity service,<br/>
    ///     a user browsing to view the api is automatically redirected to the Swagger UI
    /// </remarks>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseCommonSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        if (app.Environment.ApplicationName.EndsWith("IdentityService") is false)
        {
            app.Map("/", httpContext => Task.Run(() => httpContext.Response.Redirect("/swagger")));
        }

        return app;
    }
}
