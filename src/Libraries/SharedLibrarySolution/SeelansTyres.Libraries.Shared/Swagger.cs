using Microsoft.AspNetCore.Authentication.JwtBearer; // JwtBearerDefaults
using Microsoft.AspNetCore.Builder;                  // WebApplication
using Microsoft.Extensions.DependencyInjection;      // IServiceCollection
using Microsoft.OpenApi.Models;                      // OpenApiSecurityScheme, SecuritySchemeType, OpenApiSecurityRequirement, OpenApiReference, ReferenceType
using Swashbuckle.AspNetCore.SwaggerGen;             // SwaggerGenOptions
using System.Diagnostics.CodeAnalysis;               // SuppressMessage

namespace SeelansTyres.Libraries.Shared;

public static class Swagger
{
    /// <summary>
    /// Adds the JWT security definition and requirement to Swagger
    /// </summary>
    /// <remarks>
    ///     There is a way to configure a security definition and requirement using OpenID Connect<br/>
    ///     and IdentityServer4, however, it's not added to avoid needing to create additional clients,<br/>
    ///     and having an additional strong dependency health check from the apis to the identity service<br/>
    ///     in the application's developer environment
    /// </remarks>
    /// <param name="swaggerGenOptions">The swagger gen options used to configure Swagger</param>
    /// <returns>The original swagger gen options with the added security definitions</returns>
    public static SwaggerGenOptions AddCommonSwaggerSecurityDefinitions(this SwaggerGenOptions swaggerGenOptions)
    {
        var jwtSecurityDefinitionName = "JwtBearerAuth";

        swaggerGenOptions.AddSecurityDefinition(jwtSecurityDefinitionName, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Description = "Input a valid token to access this API"
        });

        swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
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

        return swaggerGenOptions;
    }

    /// <summary>
    /// Adds a description of an API to Swagger
    /// </summary>
    /// <param name="swaggerGenOptions">The swagger gen options used to configure Swagger</param>
    /// <param name="apiTitle">The title used for the API</param>
    /// <param name="apiDescription">A short description of the API</param>
    /// <returns>The original swagger gen options with the added Swagger docs</returns>
    [SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded", Justification = "The URLs used are not meant to be testable")]
    public static SwaggerGenOptions AddCommonSwaggerDoc(
        this SwaggerGenOptions swaggerGenOptions,
        string apiTitle,
        string apiDescription)
    {
        swaggerGenOptions.SwaggerDoc("OpenAPISpecification", new()
        {
            Title = apiTitle,
            Description = apiDescription,
            Contact = new OpenApiContact
            {
                Name = "Shaylen Reddy",
                Url = new Uri("https://za.linkedin.com/in/shaylen-reddy")
            },
            License = new OpenApiLicense
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        return swaggerGenOptions;
    }

    /// <summary>
    /// Adds the Swagger UI to the web application
    /// </summary>
    /// <remarks>
    ///     For all apis other than the one on the identity service,<br/>
    ///     a user browsing to view the api is automatically redirected to the Swagger UI
    /// </remarks>
    /// <param name="app">The built web application</param>
    /// <param name="name">Name of the Swagger definition</param>
    /// <returns>The orignal web application with SwaggerUI enabled</returns>
    public static WebApplication UseCommonSwagger(this WebApplication app, string name)
    {
        app.UseSwagger();
        app.UseSwaggerUI(setup =>
        {
            setup.SwaggerEndpoint("/swagger/OpenAPISpecification/swagger.json", name);
        });

        if (!app.Environment.ApplicationName.EndsWith("IdentityService"))
        {
            app.Map("/", httpContext => Task.Run(() => httpContext.Response.Redirect("/swagger")));
        }

        return app;
    }
}
