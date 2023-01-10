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

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["Database:ConnectionString"]!));

builder.Services.AddScoped<IOrderUpdateService, OrderUpdateService>();

if (builder.Environment.IsDevelopment() is true)
{
    builder.Services.AddHostedService<DeleteAccountWorkerWithRabbitMQ>();
    builder.Services.AddHostedService<UpdateAccountWorkerWithRabbitMQ>();
    builder.Services.AddHostedService<UpdateTyreWorkerWithRabbitMQ>();
}
else
{
    builder.Services.AddHostedService<DeleteAccountWorkerWithAzureServiceBus>();
    builder.Services.AddHostedService<UpdateAccountWorkerWithAzureServiceBus>();
    builder.Services.AddHostedService<UpdateTyreWorkerWithAzureServiceBus>();
}

builder.Services.AddHttpClient<ITokenValidationService, TokenValidationService>(client =>
{
    client.BaseAddress = new(builder.Configuration["IdentityServer"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
});

builder.Services.AddHealthChecks()
    .AddCommonDbContextCheck<OrderDbContext>()
    .AddCommonIdentityServerCheck(builder.Configuration["IdentityServer"]!)
    .AddRabbitMQ(
        name: "rabbitmq",
        rabbitConnectionString: builder.Configuration["RabbitMQ:ConnectionProperties:ConnectionString"]!,
        failureStatus: HealthStatus.Degraded);

var app = builder.Build();

app.MapCommonHealthChecks();

app.Run();
