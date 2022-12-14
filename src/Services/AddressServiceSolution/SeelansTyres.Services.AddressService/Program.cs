using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Services.AddressService.Authorization;
using SeelansTyres.Data.AddressData;
using SeelansTyres.Services.AddressService.Services;
using System.Reflection;
using SeelansTyres.Libraries.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    KestrelLocalhostPortNumber = 5011,
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Address Microservice"
});

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCommonSwaggerGen();

builder.Services.AddDbContext<AddressDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["Database:ConnectionString"]!,
        options => options.MigrationsAssembly(typeof(AddressDbContext).Assembly.GetName().Name)));

builder.Services.AddScoped<IAddressRepository, AddressRepository>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(configure =>
    {
        configure.Authority = builder.Configuration["IdentityServer"];
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

builder.Services.AddHealthChecks()
    .AddCommonDbContextCheck<AddressDbContext>();

var app = builder.Build();

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
