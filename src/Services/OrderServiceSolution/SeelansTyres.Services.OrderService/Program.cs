using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Libraries.Shared.Models;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Services.OrderService.Authorization;
using SeelansTyres.Data.OrderData;
using SeelansTyres.Services.OrderService.Services;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    KestrelLocalhostPortNumber = 5012,
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Order Microservice"
});

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCommonSwaggerGen();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["SeelansTyresOrderContext"],
        options => options.MigrationsAssembly(typeof(OrderDbContext).Assembly.GetName().Name)));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(configure =>
    {
        configure.Authority = builder.Configuration["TokenIssuer"];
        configure.Audience = "OrderService";
        configure.RequireHttpsMetadata = false;
    });

builder.Services.AddTransient<IAuthorizationHandler, MustBeAnAdministratorHandler>();
builder.Services.AddTransient<IAuthorizationHandler, MustSatisfyOrderRetrievalRulesHandler>();

builder.Services.AddAuthorization(configure =>
{
    configure.AddPolicy("MustBeAnAdministrator", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new MustBeAnAdministratorRequirement());
    });

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

var healthChecksModel = new HealthChecksModel
{
    EnableElasticsearchHealthCheck = builder.Configuration.GetValue<bool>("LoggingSinks:Elasticsearch:Enabled"),
    ElasticsearchUrl = builder.Configuration["LoggingSinks:Elasticsearch:Url"]
};

builder.Services.AddHealthChecks()
    .AddCommonChecks(healthChecksModel)
    .AddDbContextCheck<OrderDbContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy);

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

if (app.Configuration.GetValue<bool>("UseDocker") is true)
{
    await app.MigrateDatabaseAsync<OrderDbContext>();
}

app.Run();
