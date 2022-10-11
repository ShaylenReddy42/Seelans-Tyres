using ConfigurationSubstitution;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Libraries.Shared.Models;
using SeelansTyres.Services.AddressService.Authorization;
using SeelansTyres.Services.AddressService.Data;
using SeelansTyres.Services.AddressService.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

if (builder.Configuration.GetValue<bool>("UseDocker") is false)
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenLocalhost(5011);
    });
}

builder.Configuration.EnableSubstitutionsWithDelimitedFallbackDefaults("$(", ")", " ?? ");

builder.Logging.ClearProviders();

var assembly = typeof(Program).Assembly;

var serilogModel = new SerilogModel
{
    Assembly = assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Address Microservice"
};

builder.Host.UseCommonSerilog(serilogModel);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    setup.AddSecurityDefinition("AddressServiceAuth", new OpenApiSecurityScheme
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
                Id = "AddressServiceAuth"
            }
        },
        new List<string>()
    }});
});

builder.Services.AddDbContext<AddressContext>(options =>
    options.UseSqlServer(
        builder.Configuration["SeelansTyresAddressContext"]));

builder.Services.AddScoped<IAddressRepository, AddressRepository>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(configure =>
    {
        configure.Authority = builder.Configuration["TokenIssuer"];
        configure.Audience = "AddressService";
        configure.RequireHttpsMetadata = false;
    });

builder.Services.AddTransient<IAuthorizationHandler, MustBeARegularCustomerHandler>();
builder.Services.AddTransient<IAuthorizationHandler, CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler>();

builder.Services.AddAuthorization(configure =>
{
    configure.AddPolicy("MustBeARegularCustomer", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(
            new MustBeARegularCustomerRequirement(),
            new CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement());
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
    var addressContext = scope.ServiceProvider.GetService<AddressContext>();
    addressContext!.Database.Migrate();

    return Task.CompletedTask;
}
