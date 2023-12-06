using Hellang.Middleware.ProblemDetails;                        // UseProblemDetails()
using Microsoft.AspNetCore.Authentication.JwtBearer;            // JwtBearerDefaults
using Microsoft.EntityFrameworkCore;                            // UseSqlServer()
using SeelansTyres.Services.TyresService.Data;                  // TyresDbContext
using SeelansTyres.Services.TyresService.Services;              // ITyresRepository, TyresRepository
using System.Reflection;                                        // Assembly
using SeelansTyres.Libraries.Shared.Services;                   // RabbitMQPublisher, AzureServiceBusPublisher
using SeelansTyres.Libraries.Shared.Extensions;                 // AddCommonStartupDelay()
using Microsoft.AspNetCore.Mvc;                                 // ProducesResponseTypeAttribute
using SeelansTyres.Libraries.Shared.Abstractions;               // MigrateDatabaseAsync()
using ShaylenReddy42.UnpublishedUpdatesManagement;              // AddUnpublishedUpdatesManagement()
using ShaylenReddy42.UnpublishedUpdatesManagement.Abstractions; // MigrateUnpublishedUpdatesManagementDatabaseAsync()

var descriptiveApplicationName = "Seelan's Tyres: Tyres Microservice";

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = descriptiveApplicationName
});

// Add services to the container.

builder.Services.AddControllers(configure =>
{
    configure.Filters.Add(new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    setup.AddCommonSwaggerSecurityDefinitions();
    setup.AddCommonSwaggerDoc(descriptiveApplicationName, "This API allows you to manage the tyre catalog as well as retrieve brands");

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    setup.IncludeXmlComments(xmlFilePath);
});

builder.Services.AddDbContext<TyresDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["Database:ConnectionString"]!,
        options =>
        {
            options.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
            options.EnableRetryOnFailure(maxRetryCount: 5);
        }));

builder.Services.AddScoped<ITyresRepository, TyresRepository>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(configure =>
    {
        configure.Authority = builder.Configuration["IdentityServer"];
        configure.Audience = "TyresService";
        configure.TokenValidationParameters.ValidTypes = new[] { "at+jwt" };
        configure.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddProblemDetails(configure =>
{
    configure.IncludeExceptionDetails = (httpContext, exception) => false;
});

builder.Services.AddHealthChecks()
    .AddCommonDbContextCheck<TyresDbContext>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHealthChecks()
        .AddCommonRabbitMQCheck(builder.Configuration["RabbitMQ:ConnectionProperties:ConnectionString"]!);

    builder.Services.AddUnpublishedUpdatesManagement<RabbitMQPublisher>(
        databaseConnectionString: builder.Configuration["Database:ConnectionString"]!);
}
else
{
    builder.Services.AddHealthChecks()
        .AddCommonAzureServiceBusTopicCheck(
            connectionString: builder.Configuration["AzureServiceBus:ConnectionString"]!,
            topicName: builder.Configuration["AzureServiceBus:Topics:UpdateTyre"]!);
    
    builder.Services.AddUnpublishedUpdatesManagement<AzureServiceBusPublisher>(
        databaseConnectionString: builder.Configuration["Database:ConnectionString"]!);
}

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
    await app.MigrateDatabaseAsync<TyresDbContext>();
}

await app.MigrateUnpublishedUpdatesManagementDatabaseAsync();

app.Run();
