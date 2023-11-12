using Microsoft.EntityFrameworkCore;                       // UseSqlServer()
using SeelansTyres.Data.OrderData;                         // OrderDbContext
using SeelansTyres.Libraries.Shared.Abstractions;          // All common methods
using SeelansTyres.Libraries.Shared.Extensions;            // AddCommonStartupDelay()
using SeelansTyres.Libraries.Shared.Services;              // ITokenValidationService, TokenValidationService
using SeelansTyres.Workers.OrderWorker.BackgroundServices; // RabbitMQ Workers, AzureServiceBus Workers
using SeelansTyres.Workers.OrderWorker.Services;           // IOrderUpdateService, OrderUpdateService
using static System.Net.Mime.MediaTypeNames;               // Application

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Order Worker Service"
});

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["Database:ConnectionString"]!,
        options => options.EnableRetryOnFailure(maxRetryCount: 5)));

builder.Services.AddScoped<IOrderUpdateService, OrderUpdateService>();

if (builder.Environment.IsDevelopment())
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
})
    .AddCommonResiliencyPolicies<TokenValidationService>(builder.Services);

builder.Services.AddHealthChecks()
    .AddCommonDbContextCheck<OrderDbContext>()
    .AddCommonIdentityServerCheck(builder.Configuration["IdentityServer"]!);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHealthChecks()
        .AddCommonRabbitMQCheck(builder.Configuration["RabbitMQ:ConnectionProperties:ConnectionString"]!);
}
else
{
    builder.Services.AddHealthChecks()
        .AddCommonAzureServiceBusSubscriptionCheck(
            connectionString: builder.Configuration["AzureServiceBus:ConnectionString"]!,
            topicName: builder.Configuration["AzureServiceBus:Topics:DeleteAccount"]!,
            subscriptionName: builder.Configuration["AzureServiceBus:Subscriptions:DeleteAccount"]!)
        .AddCommonAzureServiceBusSubscriptionCheck(
            connectionString: builder.Configuration["AzureServiceBus:ConnectionString"]!,
            topicName: builder.Configuration["AzureServiceBus:Topics:UpdateAccount"]!,
            subscriptionName: builder.Configuration["AzureServiceBus:Subscriptions:UpdateAccount"]!)
        .AddCommonAzureServiceBusSubscriptionCheck(
            connectionString: builder.Configuration["AzureServiceBus:ConnectionString"]!,
            topicName: builder.Configuration["AzureServiceBus:Topics:UpdateTyre"]!,
            subscriptionName: builder.Configuration["AzureServiceBus:Subscriptions:UpdateTyre"]!);
}

var app = builder.Build();

app.HonorForwardedHeaders();

app.MapCommonHealthChecks();

app.AddCommonStartupDelay();

app.Run();
