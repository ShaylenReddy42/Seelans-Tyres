using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace SeelansTyres.Libraries.Shared;

public static class Swagger
{
    public static IServiceCollection AddCommonSwaggerGen(this IServiceCollection services)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(setup =>
        {
            setup.AddSecurityDefinition("JwtBearerAuth", new OpenApiSecurityScheme
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
                        Id = "JwtBearerAuth"
                    }
                },
                new List<string>()
            }});
        });

        return services;
    }

    public static IApplicationBuilder UseCommonSwagger(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }
}
