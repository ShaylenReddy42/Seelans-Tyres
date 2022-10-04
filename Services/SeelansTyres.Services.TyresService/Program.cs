using ConfigurationSubstitution;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SeelansTyres.Services.TyresService.Authorization;
using SeelansTyres.Services.TyresService.Data;
using SeelansTyres.Services.TyresService.Services;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
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

builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(hostBuilderContext.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .Enrich.With<ActivityEnricher>()
        .Enrich.WithProperty("Custom: Application Name", hostBuilderContext.HostingEnvironment.ApplicationName)
        .Enrich.WithProperty("Custom: Descriptive Application Name", assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product)
        .Enrich.WithProperty("Custom: Codebase Version", $"v{assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}")
        .WriteTo.Console();

    var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToList();

    metadata.ForEach(attribute => loggerConfiguration.Enrich.WithProperty($"Custom: {attribute.Key}", attribute.Value));

    if (hostBuilderContext.Configuration.GetValue<bool>("LoggingSinks:Elasticsearch:Enabled") is true)
    {
        loggerConfiguration
            .WriteTo.Elasticsearch(
                new ElasticsearchSinkOptions(new Uri(hostBuilderContext.Configuration["LoggingSinks:Elasticsearch:Url"]))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                    IndexFormat = "seelanstyres-logs-{0:yyyy.MM.dd}",
                    MinimumLogEventLevel = LogEventLevel.Debug
                });
    }
});

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
