using Hellang.Middleware.ProblemDetails;                // UseProblemDetails()
using Microsoft.AspNetCore.Authentication.JwtBearer;    // JwtBearerDefaults
using Microsoft.AspNetCore.Authorization;               // IAuthorizationHandler
using Microsoft.EntityFrameworkCore;                    // UseSqlServer()
using SeelansTyres.Libraries.Shared;                    // All common methods
using SeelansTyres.Services.OrderService.Authorization; // MustSatisfyOrderRetrievalRulesHandler, MustSatisfyOrderRetrievalRulesRequirement()
using SeelansTyres.Data.OrderData;                      // OrderDbContext
using SeelansTyres.Services.OrderService.Services;      // IOrderRepository, OrderRepository
using System.Reflection;                                // Assembly
using SeelansTyres.Libraries.Shared.Extensions;         // AddCommonStartupDelay()

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Order Microservice"
});

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCommonSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["Database:ConnectionString"]!,
        options => 
        { 
            options.MigrationsAssembly(typeof(OrderDbContext).Assembly.GetName().Name);
            options.EnableRetryOnFailure(maxRetryCount: 5);
        }));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(configure =>
    {
        configure.Authority = builder.Configuration["IdentityServer"];
        configure.Audience = "OrderService";
        configure.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        configure.RequireHttpsMetadata = false;
    });

builder.Services.AddTransient<IAuthorizationHandler, MustSatisfyOrderRetrievalRulesHandler>();

builder.Services.AddAuthorization(configure =>
{
    configure.AddPolicy("MustSatisfyOrderRetrievalRules", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new MustSatisfyOrderRetrievalRulesRequirement());
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddProblemDetails(configure =>
{
    configure.IncludeExceptionDetails = (httpContext, exception) => false;
});

builder.Services.AddHealthChecks()
    .AddCommonDbContextCheck<OrderDbContext>();

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
    await app.MigrateDatabaseAsync<OrderDbContext>();
}

app.Run();
