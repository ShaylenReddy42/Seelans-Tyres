using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SeelansTyres.Data.OrderData;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Libraries.Shared.Models;
using SeelansTyres.Libraries.Shared.Services;
using SeelansTyres.Workers.OrderWorker.BackgroundServices;
using SeelansTyres.Workers.OrderWorker.Services;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    KestrelLocalhostPortNumber = 5022,
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Order Worker Service"
});

builder.Services.AddDbContext<OrdersContext>(options =>
    options.UseSqlServer(
        builder.Configuration["SeelansTyresOrderContext"]));

builder.Services.AddScoped<IOrderUpdateService, OrderUpdateService>();
builder.Services.AddHostedService<DeleteAccountWorker>();
builder.Services.AddHostedService<UpdateAccountWorker>();
builder.Services.AddHostedService<UpdateTyreWorker>();
builder.Services.AddHttpClient<ITokenValidationService, TokenValidationService>(client =>
{
    client.BaseAddress = new(builder.Configuration["TokenIssuer"]);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
});

var healthChecksModel = new HealthChecksModel
{
    EnableElasticsearchHealthCheck = builder.Configuration.GetValue<bool>("LoggingSinks:Elasticsearch:Enabled"),
    ElasticsearchUrl = builder.Configuration["LoggingSinks:Elasticsearch:Url"]
};

builder.Services.AddHealthChecks()
    .AddCommonChecks(healthChecksModel)
    .AddDbContextCheck<OrdersContext>(
        name: "database",
        failureStatus: HealthStatus.Unhealthy)
    .AddRabbitMQ(
        name: "rabbitmq",
        rabbitConnectionString: builder.Configuration["RabbitMQ:ConnectionProperties:ConnectionString"],
        failureStatus: HealthStatus.Degraded);

var app = builder.Build();

app.MapCommonHealthChecks();

app.Run();
