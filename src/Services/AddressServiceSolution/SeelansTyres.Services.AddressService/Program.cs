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

var descriptiveApplicationName = "Seelan's Tyres: Address Microservice";

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = descriptiveApplicationName
});

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    setup.AddCommonSwaggerSecurityDefinitions();
    setup.AddCommonSwaggerDoc(descriptiveApplicationName, "This API allows you to manage addresses for customers");

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    setup.IncludeXmlComments(xmlFilePath);
});

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
        policy.RequireAssertion(context => !context.User.IsInRole("Administrator"));
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
    app.UseCommonSwagger(descriptiveApplicationName);
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapCommonHealthChecks();

app.AddCommonStartupDelay();

if (app.Configuration.GetValue<bool>("InContainer"))
{
    await app.MigrateDatabaseAsync<AddressDbContext>();
}

app.Run();
