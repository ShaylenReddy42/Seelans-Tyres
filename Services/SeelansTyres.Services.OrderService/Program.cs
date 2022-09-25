using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SeelansTyres.Services.OrderService.Authorization;
using SeelansTyres.Services.OrderService.Data;
using SeelansTyres.Services.OrderService.Services;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

var assembly = typeof(Program).Assembly;

builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(hostBuilderContext.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithExceptionDetails()
        .Enrich.WithProperty("Application Name", hostBuilderContext.HostingEnvironment.ApplicationName)
        .Enrich.WithProperty("Descriptive Application Name", assembly.GetCustomAttribute<AssemblyProductAttribute>()!.Product)
        .Enrich.WithProperty("Codebase Version", $"v{assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion}")
        .WriteTo.Console();

    var metadata = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToList();

    metadata.ForEach(attribute => loggerConfiguration.Enrich.WithProperty(attribute.Key, attribute.Value));

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
    setup.AddSecurityDefinition("OrdersServiceAuth", new OpenApiSecurityScheme
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
                Id = "OrdersServiceAuth"
            }
        },
        new List<string>()
    }});
});

builder.Services.AddDbContext<OrdersContext>(options =>
    options.UseSqlServer(
        builder.Configuration["SeelansTyresOrderContext"]));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(configure =>
    {
        configure.Authority = builder.Configuration["TokenIssuer"];
        configure.Audience = "OrderService";
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

var app = builder.Build();

app.UseProblemDetails();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
