using ConfigurationSubstitution;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SeelansTyres.Libraries.Shared.Models;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Services.TyresService.Authorization;
using SeelansTyres.Services.TyresService.Data;
using SeelansTyres.Services.TyresService.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration.GetValue<bool>("UseDocker") is false)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5013);
    });
}

builder.Configuration.EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", " ?? ");

builder.Logging.ClearProviders();

var assembly = typeof(Program).Assembly;

var serilogModel = new SerilogModel
{
    Assembly = assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Tyres Microservice"
};

builder.Host.UseCommonSerilog(serilogModel);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    setup.AddSecurityDefinition("TyresServiceAuth", new OpenApiSecurityScheme
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
                Id = "TyresServiceAuth"
            }
        },
        new List<string>()
    }});
});

builder.Services.AddDbContext<TyresContext>(options =>
    options.UseSqlServer(
        builder.Configuration["SeelansTyresTyresContext"]));

builder.Services.AddScoped<ITyresRepository, TyresRepository>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(configure =>
    {
        configure.Authority = builder.Configuration["TokenIssuer"];
        configure.Audience = "TyresService";
        configure.RequireHttpsMetadata = false;
    });

builder.Services.AddTransient<IAuthorizationHandler, MustBeAnAdministratorHandler>();

builder.Services.AddAuthorization(configure =>
{
    configure.AddPolicy("MustBeAnAdministrator", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new MustBeAnAdministratorRequirement());
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddProblemDetails(configure =>
{
    configure.IncludeExceptionDetails = (httpContext, exception) => false;
});

var app = builder.Build();

app.UseProblemDetails();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

if (app.Configuration.GetValue<bool>("UseDocker") is true)
{
    await MigrateDatabase();
}

app.Run();

Task MigrateDatabase()
{
    using var scope = app.Services.CreateScope();
    var tyresContext = scope.ServiceProvider.GetService<TyresContext>();
    tyresContext!.Database.Migrate();

    return Task.CompletedTask;
}
