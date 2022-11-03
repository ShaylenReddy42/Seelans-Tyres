using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SeelansTyres.Libraries.Shared.Models;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Services.TyresService.Authorization;
using SeelansTyres.Services.TyresService.Data;
using SeelansTyres.Services.TyresService.Services;
using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SeelansTyres.Libraries.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    KestrelLocalhostPortNumber = 5013,
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Tyres Microservice"
});

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddCommonSwaggerGen();

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

builder.Services.AddSingleton<IMessagingServicePublisher, RabbitMQMessagingServicePublisher>();

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

var healthChecksModel = new HealthChecksModel
{
    EnableElasticsearchHealthCheck = builder.Configuration.GetValue<bool>("LoggingSinks:Elasticsearch:Enabled"),
    ElasticsearchUrl = builder.Configuration["LoggingSinks:Elasticsearch:Url"]
};

var rabbitMQConnectionString = 
    $"amqp://{builder.Configuration["RabbitMQ:ConnectionProperties:HostName"]}:{builder.Configuration["RabbitMQ:ConnectionProperties:Port"]}";

builder.Services.AddHealthChecks()
    .AddCommonChecks(healthChecksModel)
    .AddDbContextCheck<TyresContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy)
    .AddRabbitMQ(
        name: "rabbitmq",
        rabbitConnectionString: rabbitMQConnectionString,
        failureStatus: HealthStatus.Degraded);

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
    await app.MigrateDatabaseAsync<TyresContext>();
}

app.Run();