using Microsoft.EntityFrameworkCore;
using SeelansTyres.Data.AddressData;
using SeelansTyres.Libraries.Shared;
using SeelansTyres.Libraries.Shared.Extensions;
using SeelansTyres.Libraries.Shared.Services;
using SeelansTyres.Workers.AddressWorker.BackgroundServices;
using SeelansTyres.Workers.AddressWorker.Services;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);

builder.AddCommonBuilderConfiguration(new()
{
    OriginAssembly = typeof(Program).Assembly,
    DefaultDescriptiveApplicationName = "Seelan's Tyres: Address Worker Service"
});

builder.Services.AddDbContext<AddressDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration["Database:ConnectionString"]!,
        options => options.EnableRetryOnFailure(maxRetryCount: 5)));

builder.Services.AddScoped<IAddressUpdateService, AddressUpdateService>();

if (builder.Environment.IsDevelopment() is true)
{
    builder.Services.AddHostedService<DeleteAccountWorkerWithRabbitMQ>();
}
else
{
    builder.Services.AddHostedService<DeleteAccountWorkerWithAzureServiceBus>();
}

builder.Services.AddHttpClient<ITokenValidationService, TokenValidationService>(client =>
{
    client.BaseAddress = new(builder.Configuration["IdentityServer"]!);
    client.DefaultRequestHeaders.Accept.Add(new(Application.Json));
})
    .AddCommonResiliencyPolicies<TokenValidationService>(builder.Services);

builder.Services.AddHealthChecks()
    .AddCommonDbContextCheck<AddressDbContext>()
    .AddCommonIdentityServerCheck(builder.Configuration["IdentityServer"]!);

if (builder.Environment.IsDevelopment() is true)
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
            subscriptionName: builder.Configuration["AzureServiceBus:Subscriptions:DeleteAccount"]!);
}

var app = builder.Build();

app.HonorForwardedHeaders();

app.MapCommonHealthChecks();

app.AddCommonStartupDelay();

app.Run();
