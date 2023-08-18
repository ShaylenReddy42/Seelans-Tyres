using Hellang.Middleware.ProblemDetails;             // UseProblemDetails()
using Microsoft.AspNetCore.Authentication.JwtBearer; // JwtBearerDefaults
using Microsoft.AspNetCore.Authorization;            // IAuthorizationHandler
using Microsoft.EntityFrameworkCore;                 // UseSqlServer()
using SeelansTyres.Libraries.Shared;                 // All common methods
using SeelansTyres.Data.AddressData;                 // AddressDbContext
using SeelansTyres.Services.AddressService.Services; // IAddressRepository, AddressRepository
using System.Reflection;                             // Assembly
using SeelansTyres.Libraries.Shared.Extensions;      // AddCommonStartupDelay()
using SeelansTyres.Libraries.Shared.Authorization;   // CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler, CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement()

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Address Microservice"
});

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCommonSwaggerGen();

builder.Services.AddDbContext<AddressDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["Database:ConnectionString"]!,
        options =>
        {
            options.MigrationsAssembly(typeof(AddressDbContext).Assembly.GetName().Name);
            options.EnableRetryOnFailure(maxRetryCount: 5);
        }));

builder.Services.AddScoped<IAddressRepository, AddressRepository>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(configure =>
    {
        configure.Authority = builder.Configuration["IdentityServer"];
        configure.Audience = "AddressService";
        configure.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        configure.RequireHttpsMetadata = false;
    });

builder.Services.AddTransient<IAuthorizationHandler, CustomerIdFromClaimsMustMatchCustomerIdFromRouteHandler>();

builder.Services.AddAuthorization(configure =>
{
    configure.AddPolicy("MustBeARegularCustomer", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context => context.User.IsInRole("Administrator") is false);
        policy.AddRequirements(
            new CustomerIdFromClaimsMustMatchCustomerIdFromRouteRequirement("customerId"));
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddProblemDetails(configure =>
{
    configure.IncludeExceptionDetails = (httpContext, exception) => false;
});

builder.Services.AddHealthChecks()
    .AddCommonDbContextCheck<AddressDbContext>();

var app = builder.Build();

app.HonorForwardedHeaders();

app.UseProblemDetails();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCommonSwagger();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapCommonHealthChecks();

app.AddCommonStartupDelay();

if (app.Configuration.GetValue<bool>("InContainer") is true)
{
    await app.MigrateDatabaseAsync<AddressDbContext>();
}

app.Run();
